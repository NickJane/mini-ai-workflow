using Microsoft.AspNetCore.Mvc;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Dtos;
using SuperFlowApi.Infrastructure;
using Nop.WebApiFramework.Exceptions;
using System.Net;

namespace SuperFlowApi.Controllers.SuperFlow
{
    /// <summary>
    /// 大模型提供者管理控制器
    /// 管理不同平台（如阿里云、OpenAI等）的大模型配置信息
    /// </summary>
    [ApiGroup(nameof(EnumApiGroupNames.SuperFlow))]
    public partial class FlowLLMProviderController : BaseControllerController
    {
        private readonly IFreeSql _freeSql;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlowLLMProviderController> _logger;

        /// <summary>
        /// 构造函数，注入FreeSqlProvider和Logger
        /// </summary>
        public FlowLLMProviderController(FreeSqlProvider freeSqlProvider, ILogger<FlowLLMProviderController> logger, IServiceProvider serviceProvider)
        {
            _freeSql = freeSqlProvider.GetFreeSql();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 创建大模型提供者
        /// </summary>
        /// <param name="dto">大模型提供者信息</param>
        /// <returns>创建成功的大模型提供者信息</returns>
        /// <remarks>
        /// 请求示例:
        /// POST /api/FlowLLMProvider/Create
        /// {
        ///    "platformName": "阿里云",
        ///    "llmNames": ["qwen-max", "qwen-plus", "qwen-turbo"],
        ///    "llmAPIUrl": "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation",
        ///    "llmAPIKey": "sk-xxxxx"
        /// }
        /// </remarks>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(JsonResponse<FlowLLMProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] FlowLLMProviderDto dto)
        {
            try
            {
                var fsql = _freeSql;

                // 验证必填字段
                if (string.IsNullOrWhiteSpace(dto.PlatformName))
                {
                    return Error("platform name is required");
                }
                if (dto.LLMNames == null || dto.LLMNames.Count == 0)
                {
                    return Error("model name list is required");
                }

                // 检查平台名称是否已存在
                var exists = await fsql.Select<FlowLLMProviderEntity>()
                    .Where(x => x.PlatformName == dto.PlatformName)
                    .AnyAsync();

                if (exists)
                {
                    return Error($"platform '{dto.PlatformName}' already exists");
                }

                // 创建实体
                var entity = new FlowLLMProviderEntity
                {
                    Id = SnowflakeId.NextId(),
                    PlatformName = dto.PlatformName,
                    LLMNames = dto.LLMNames,
                    LLMAPIUrl = dto.LLMAPIUrl ?? string.Empty,
                    LLMAPIKey = dto.LLMAPIKey ?? string.Empty
                };

                // 插入数据库
                await fsql.Insert(entity).ExecuteAffrowsAsync();

                _logger.LogInformation("create flow llm provider success，ID: {Id}, platform: {Platform}, model count: {ModelCount}", 
                    entity.Id, entity.PlatformName, entity.LLMNames.Count);

                return Ok(FlowLLMProviderDto.FromEntity(entity));
            }
            catch (WebApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "create flow llm provider failed");
                throw new WebApiException($"create flow llm provider failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取大模型提供者信息
        /// </summary>
        /// <param name="id">大模型提供者ID</param>
        /// <returns>大模型提供者详细信息</returns>
        [HttpGet("GetById")]
        [ProducesResponseType(typeof(JsonResponse<FlowLLMProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] long id)
        {
            try
            {
                var fsql = _freeSql;

                var entity = await fsql.Select<FlowLLMProviderEntity>().Where(x => x.Id == id).FirstAsync();
                if (entity == null)
                {
                    return Error($"get flow llm provider failed，ID: {id}");
                }

                return Ok(FlowLLMProviderDto.FromEntity(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get flow llm provider failed，ID: {Id}", id);
                throw new WebApiException($"get flow llm provider failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取大模型提供者列表
        /// 支持按平台名称筛选，返回所有符合条件的提供者
        /// </summary>
        /// <param name="platformName">平台名称筛选（可选），支持模糊查询</param>
        /// <returns>大模型提供者列表</returns>
        [HttpGet("GetList")]
        [ProducesResponseType(typeof(JsonResponse<List<FlowLLMProviderDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetListAsync([FromQuery] string? platformName = null)
        {
            try
            {
                var fsql = _freeSql;

                var query = fsql.Select<FlowLLMProviderEntity>();

                // 根据platformName参数筛选
                if (!string.IsNullOrWhiteSpace(platformName))
                {
                    query = query.Where(x => x.PlatformName.Contains(platformName));
                }

                var result = await query
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                return Ok(result.Select(x => FlowLLMProviderDto.FromEntity(x)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get flow llm provider list failed");
                throw new WebApiException($"get flow llm provider list failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新大模型提供者信息
        /// </summary>
        /// <param name="dto">更新的大模型提供者信息</param>
        /// <returns>更新后的大模型提供者信息</returns>
        /// <remarks>
        /// 请求示例:
        /// POST /api/FlowLLMProvider/Update
        /// {
        ///    "id": 1,
        ///    "platformName": "阿里云",
        ///    "llmNames": ["qwen-max", "qwen-plus", "qwen-turbo", "qwen-long"],
        ///    "llmAPIUrl": "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation",
        ///    "llmAPIKey": "sk-xxxxx"
        /// }
        /// </remarks>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(JsonResponse<FlowLLMProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateAsync([FromBody] FlowLLMProviderDto dto)
        {
            try
            {
                if (!dto.Id.HasValue || dto.Id.Value <= 0)
                {
                    return Error("ID is required");
                }

                var fsql = _freeSql;

                // 先查询实体是否存在
                var entity = await fsql.Select<FlowLLMProviderEntity>().Where(x => x.Id == dto.Id.Value).FirstAsync();

                if (entity == null)
                {
                    return Error($"get flow llm provider failed，ID: {dto.Id}");
                }

                // 更新字段
                if (!string.IsNullOrWhiteSpace(dto.PlatformName))
                {
                    entity.PlatformName = dto.PlatformName;
                }
                if (dto.LLMNames != null && dto.LLMNames.Count > 0)
                {
                    entity.LLMNames = dto.LLMNames;
                }
                if (dto.LLMAPIUrl != null)
                {
                    entity.LLMAPIUrl = dto.LLMAPIUrl;
                }
                if (dto.LLMAPIKey != null)
                {
                    entity.LLMAPIKey = dto.LLMAPIKey;
                }

                // 更新数据库
                var affectedRows = await fsql.Update<FlowLLMProviderEntity>()
                    .SetSource(entity)
                    .ExecuteAffrowsAsync();

                if (affectedRows == 0)
                {
                    throw new WebApiException("update flow llm provider failed，no rows affected");
                }

                _logger.LogInformation("update flow llm provider success，ID: {Id}", entity.Id);

                return Ok(FlowLLMProviderDto.FromEntity(entity));
            }
            catch (WebApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "update flow llm provider failed，ID: {Id}", dto.Id);
                throw new WebApiException($"update flow llm provider failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除大模型提供者
        /// 物理删除，不可恢复
        /// </summary>
        /// <param name="id">大模型提供者ID</param>
        /// <returns>删除结果</returns>
        [HttpPost("Delete")]
        [ProducesResponseType(typeof(JsonResponse<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] long id)
        {
            try
            {
                var fsql = _freeSql;

                var rowsAffected = await fsql.Delete<FlowLLMProviderEntity>()
                    .Where(x => x.Id == id)
                    .ExecuteAffrowsAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("delete flow llm provider success，ID: {Id}", id);
                    return Ok(true);
                }

                return Error($"delete flow llm provider failed，ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "delete flow llm provider failed，ID: {Id}", id);
                throw new WebApiException($"delete flow llm provider failed: {ex.Message}");
            }
        }
    }
}
