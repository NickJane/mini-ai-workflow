using Microsoft.AspNetCore.Mvc;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Dtos;
using SuperFlowApi.Domain.SuperFlow.Entities;
using SuperFlowApi.Infrastructure;
using Nop.WebApiFramework.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace SuperFlowApi.Controllers.SuperFlow
{
    /// <summary>
    /// 回话管理
    /// </summary>
    [ApiGroup(nameof(EnumApiGroupNames.SuperFlow))]
    public partial class FlowConversationController : BaseControllerController
    {
        private readonly IFreeSql _freeSql;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlowConversationController> _logger;

        /// <summary>
        /// 构造函数，注入FreeSqlProvider和Logger
        /// </summary>
        public FlowConversationController(FreeSqlProvider freeSqlProvider, ILogger<FlowConversationController> logger, IServiceProvider serviceProvider)
        {
            _freeSql = freeSqlProvider.GetFreeSql();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        private string GetUserId()
        {
            return HttpContext.Request.Headers["phoneNumber"];
        }

        /// <summary>
        /// 本地会话列表（从数据库读取）
        /// </summary>
        /// <param name="flowId">flowId</param>
        /// <param name="first_id">第一条记录ID（ConversationId）</param>
        /// <param name="limit">返回数量限制</param>
        /// <returns>本地会话列表</returns>
        [HttpGet("{flowId}/local-conversations")]
        public async Task<ActionResult<LocalConversationsResponse>> GetLocalConversations(
            string flowId,
            [FromQuery] string? first_id = null,
            [FromQuery] int limit = 20)
        {
            var user = GetUserId();

            // 将 flowId 转换为 long 类型
            if (!long.TryParse(flowId, out var flowIdLong))
            {
                return BadRequest(new { success = false, message = "无效的流程ID" });
            }

            var query = _freeSql.Select<FlowChatConversationEntity>()
                .Where(x => x.User == user && x.FlowId == flowIdLong);

            // 分页处理：使用 ConversationId 作为分页标识
            if (!string.IsNullOrEmpty(first_id))
            {
                // 根据 UpdatedAt 进行分页
                var firstConv = await _freeSql.Select<FlowChatConversationEntity>()
                    .Where(x => x.ConversationId == first_id)
                    .FirstAsync();

                if (firstConv != null)
                {
                    query = query.Where(x => x.UpdatedAt < firstConv.UpdatedAt);
                }
            }

            var conversations = await query
                .OrderByDescending(x => x.IsTop)
                .OrderByDescending(x => x.UpdatedAt)
                .Limit(limit)
                .ToListAsync();

            var result = new LocalConversationsResponse
            {
                Data = conversations.Select(x => new LocalConversation
                {
                    Id = x.ConversationId,
                    Title = x.Title,
                    IsTop = x.IsTop,
                    LastMessageTime = x.UpdatedAt,
                    MessageCount = x.MessageCount
                }).ToList(),
                HasMore = conversations.Count == limit,
                Limit = limit
            };

            return Ok(result);
        }

        /// <summary>
        /// 本地会话消息历史（从数据库读取）
        /// </summary>
        /// <param name="flowId">flowId</param>
        /// <param name="conversation_id">会话ID</param>
        /// <param name="first_id">第一条记录ID</param>
        /// <param name="limit">返回数量限制</param>
        /// <returns>本地消息历史</returns>
        [HttpGet("{flowId}/local-messages")]
        public async Task<ActionResult<LocalMessagesResponse>> GetLocalMessages(
            string flowId,
            [FromQuery] string conversation_id,
            [FromQuery] string? first_id = null,
            [FromQuery] int limit = 20)
        {
            var user = GetUserId();

            // 将 flowId 转换为 long 类型
            if (!long.TryParse(flowId, out var flowIdLong))
            {
                return BadRequest(new { success = false, message = "无效的流程ID" });
            }

            var query = _freeSql.Select<FlowChatMessageEntity>()
                .Where(x => x.ConversationId == conversation_id &&
                           x.User == user &&
                           x.FlowId == flowIdLong);

            // 分页处理
            if (!string.IsNullOrEmpty(first_id) && long.TryParse(first_id, out var firstIdLong))
            {
                query = query.Where(x => x.Id < firstIdLong);
            }

            var messages = await query
                .OrderBy(x => x.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            var result = new LocalMessagesResponse
            {
                Data = messages.Select(x => new LocalMessage
                {
                    Id = x.Id.ToString(),  // 将 long 类型 ID 转换为字符串
                    ConversationId = x.ConversationId,
                    Question = x.Question,
                    Answer = x.Answer,
                    Files = x.Files,
                    CreatedAt = x.CreatedAt,
                    TotalTokens = x.TotalTokens,
                }).ToList(),
                HasMore = messages.Count == limit,
                Limit = limit
            };

            return Ok(result);
        }

        /// <summary>
        /// 本地会话切换会话置顶状态（自动切换：未置顶->置顶，已置顶->取消置顶）
        /// </summary>
        /// <param name="flowId">flowId</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{flowId}/local-conversations-toggle-top")]
        public async Task<ActionResult> ToggleLocalConversationTop(
            string flowId,
            [FromQuery] string conversationId)
        {
            var user = GetUserId();

            // 将 flowId 转换为 long 类型
            if (!long.TryParse(flowId, out var flowIdLong))
            {
                return BadRequest(new { success = false, message = "无效的流程ID" });
            }

            // 先查询当前置顶状态
            var conversation = await _freeSql.Select<FlowChatConversationEntity>()
                .Where(x => x.ConversationId == conversationId &&
                           x.User == user &&
                           x.FlowId == flowIdLong)
                .FirstAsync();

            if (conversation == null)
            {
                return NotFound(new { success = false, message = "会话不存在" });
            }

            // 切换置顶状态
            var newTopStatus = !conversation.IsTop;

            var affectedRows = await _freeSql.Update<FlowChatConversationEntity>()
                .Set(x => x.IsTop, newTopStatus)
                .Set(x => x.UpdatedAt, DateTime.Now)  // 更新修改时间
                .Where(x => x.ConversationId == conversationId &&
                           x.User == user &&
                           x.FlowId == flowIdLong)
                .ExecuteAffrowsAsync();

            if (affectedRows > 0)
            {
                return Ok(new { success = true, message = newTopStatus ? "置顶成功" : "取消置顶成功", isTop = newTopStatus });
            }
            else
            {
                return Ok(new { success = false, message = "操作失败" });
            }
        }

        /// <summary>
        /// 本地会话修改标题
        /// </summary>
        /// <param name="flowId">flowId</param>
        /// <param name="conversationId">会话ID</param>
        /// <param name="request">修改标题请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("{flowId}/local-conversations-update-title")]
        public async Task<ActionResult> UpdateLocalConversationTitle(
            string flowId,
            [FromQuery] string conversationId,
            [FromBody] UpdateConversationTitleRequest request)
        {
            var user = GetUserId();

            // 将 flowId 转换为 long 类型
            if (!long.TryParse(flowId, out var flowIdLong))
            {
                return BadRequest(new { success = false, message = "无效的流程ID" });
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { success = false, message = "标题不能为空" });
            }

            var affectedRows = await _freeSql.Update<FlowChatConversationEntity>()
                .Set(x => x.Title, request.Title.Trim())
                .Set(x => x.UpdatedAt, DateTime.Now)  // 更新修改时间
                .Where(x => x.ConversationId == conversationId &&
                           x.User == user &&
                           x.FlowId == flowIdLong)
                .ExecuteAffrowsAsync();

            if (affectedRows > 0)
            {
                return Ok(new { success = true, message = "修改标题成功" });
            }
            else
            {
                return NotFound(new { success = false, message = "会话不存在" });
            }
        }

        /// <summary>
        /// 本地会话删除（物理删除）
        /// </summary>
        /// <param name="flowId">flowId</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{flowId}/local-conversations-delete")]
        public async Task<ActionResult> DeleteLocalConversation(
            string flowId,
            [FromQuery] string conversationId)
        {
            var user = GetUserId();

            // 将 flowId 转换为 long 类型
            if (!long.TryParse(flowId, out var flowIdLong))
            {
                return BadRequest(new { success = false, message = "无效的流程ID" });
            }

            // 使用事务删除会话及相关消息
            using var uow = _freeSql.CreateUnitOfWork();
            try
            {
                // 先删除该会话的所有消息
                var deletedMessages = await _freeSql.Delete<FlowChatMessageEntity>()
                    .Where(x => x.ConversationId == conversationId &&
                               x.User == user &&
                               x.FlowId == flowIdLong)
                    .ExecuteAffrowsAsync();

                // 再删除会话
                var deletedConversation = await _freeSql.Delete<FlowChatConversationEntity>()
                    .Where(x => x.ConversationId == conversationId &&
                               x.User == user &&
                               x.FlowId == flowIdLong)
                    .ExecuteAffrowsAsync();

                if (deletedConversation > 0)
                {
                    uow.Commit();
                    return Ok(new { success = true, message = "删除会话成功", deletedMessages = deletedMessages });
                }
                else
                {
                    uow.Rollback();
                    return NotFound(new { success = false, message = "会话不存在" });
                }
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "删除会话失败: ConversationId={ConversationId}", conversationId);
                return StatusCode(500, new { success = false, message = "删除会话失败" });
            }
        }
    }
}
