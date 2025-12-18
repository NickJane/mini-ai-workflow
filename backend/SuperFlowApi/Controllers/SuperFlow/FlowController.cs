using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Dtos;
using SuperFlowApi.Infrastructure;
using Nop.WebApiFramework.Exceptions;
using Nop.WebApiFramework.UserAccount.Jwt.Model;

namespace SuperFlowApi.Controllers.SuperFlow
{
    [ApiGroup(nameof(EnumApiGroupNames.SuperFlow))]
    public partial class FlowController : BaseControllerController
    {
        private readonly IFreeSql _freeSql;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlowController> _logger;

        /// <summary>
        /// 构造函数，注入FreeSqlProvider和Logger
        /// </summary>
        public FlowController(FreeSqlProvider freeSqlProvider, ILogger<FlowController> logger, IServiceProvider serviceProvider)
        {
            _freeSql = freeSqlProvider.GetFreeSql();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 创建新的流程
        /// 支持LogicFlow/AIFlow/ApprovalFlow三种类型
        /// </summary>
        /// <param name="dto">创建流程的DTO</param>
        /// <returns>创建成功的流程信息</returns>
        /// <remarks>
        /// 请求示例:
        /// POST /api/Flow/Create
        /// {
        ///    "name": "测试AI流",
        ///    "displayName": "测试AI流显示名称",
        ///    "flowType": 1,  // 0=LogicFlow, 1=AIFlow, 2=ApprovalFlow
        ///    "description": "这是一个测试AI流",
        ///    "configInfoForRun": null,
        ///    "configInfoForWeb": null
        /// }
        /// </remarks>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(JsonResponse<FlowDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] FlowDto dto)
        {
            try
            {
                var fsql = _freeSql;

                // 创建流程实体
                var entity = new FlowEntity
                {
                    Id = SnowflakeId.NextId(),
                    DisplayName = dto.DisplayName,
                    FlowType = dto.FlowType,
                    Description = dto.Description,
                    ConfigInfoForRun = dto.ConfigInfoForRun,
                    ConfigInfoForWeb = dto.ConfigInfoForWeb,
                    LastModified = DateTime.Now,
                    LastModifyBy = LoginUserDto.Current?.NickName ?? "System"
                };

                // 插入数据库
                await fsql.Insert(entity).ExecuteAffrowsAsync();

                _logger.LogInformation("create flow success，ID: {Id}, type: {FlowType}", entity.Id, entity.FlowType);

                return Ok(FlowDto.FromEntity(entity));
            }
            catch (WebApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "create flow failed");
                throw new WebApiException($"create flow failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取流程信息
        /// </summary>
        /// <param name="id">流程ID</param>
        /// <returns>流程详细信息</returns>
        [HttpGet("GetById")]
        [ProducesResponseType(typeof(JsonResponse<FlowDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] long id)
        {
            try
            {
                var fsql = _freeSql;

                var entity = await fsql.Select<FlowEntity>().Where(x => x.Id == id).FirstAsync();
                if (entity == null)
                {
                    return Error($"get flow by id failed，ID: {id}");
                }

                return Ok(FlowDto.FromEntity(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get flow by id failed，ID: {Id}", id);
                throw new WebApiException($"get flow by id failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取流程列表
        /// 支持按类型筛选，返回所有符合条件的流程
        /// </summary>
        /// <param name="flowType">流程类型筛选，null表示获取所有类型</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>流程列表</returns>
        [HttpGet("GetList")]
        [ProducesResponseType(typeof(JsonResponse<List<FlowDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] FlowType? flowType = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var fsql = _freeSql;

                var query = fsql.Select<FlowEntity>();

                // 根据flowType参数筛选
                if (flowType.HasValue)
                {
                    query = query.Where(x => x.FlowType == flowType.Value);
                }

                var result = await query
                    .OrderByDescending(x => x.LastModified)
                    .Page(pageIndex, pageSize)
                    .ToListAsync();

                _logger.LogInformation("get flow list success，type: {FlowType}, count: {Count}", flowType, result.Count);

                return Ok(result.Select(x => FlowDto.FromEntity(x)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get flow list failed");
                throw new WebApiException($"get flow list failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新流程信息
        /// 只更新提供的字段，null值的字段不更新
        /// </summary>
        /// <param name="dto">更新流程的DTO</param>
        /// <returns>更新后的流程信息</returns>
        /// <remarks>
        /// 请求示例:
        /// POST /api/Flow/Update
        /// {
        ///    "id": 1,
        ///    "name": "更新后的名称",
        ///    "displayName": "更新后的显示名称",
        ///    "description": "更新后的描述"
        /// }
        /// </remarks>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(JsonResponse<FlowDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateAsync([FromBody] FlowDto dto)
        {
            try
            {
                var fsql = _freeSql;

                // 先查询实体
                var entity = await fsql.Select<FlowEntity>().Where(x => x.Id == dto.Id).FirstAsync();

                if (entity == null)
                {
                    return Error($"get flow by id failed，ID: {dto.Id}");
                }

                if (!string.IsNullOrEmpty(dto.DisplayName))
                {
                    entity.DisplayName = dto.DisplayName;
                }
                if (!string.IsNullOrEmpty(dto.Description))
                {
                    entity.Description = dto.Description;
                }
                if (dto.ConfigInfoForRun != null)
                {
                    entity.ConfigInfoForRun = dto.ConfigInfoForRun;
                }
                if (dto.ConfigInfoForWeb != null)
                {
                    entity.ConfigInfoForWeb = dto.ConfigInfoForWeb;
                }

                entity.LastModified = DateTime.Now;
                entity.LastModifyBy = LoginUserDto.Current?.NickName ?? "System";

                // 更新数据库
                var affectedRows = await fsql.Update<FlowEntity>()
                    .SetSource(entity)
                    .ExecuteAffrowsAsync();

                if (affectedRows == 0)
                {
                    throw new WebApiException("update flow failed，no rows affected");
                }

                _logger.LogInformation("update flow success，ID: {Id}", entity.Id);

                return Ok(FlowDto.FromEntity(entity));
            }
            catch (WebApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "update flow failed，ID: {Id}", dto.Id);
                throw new WebApiException($"update flow failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除流程
        /// 物理删除，不可恢复
        /// </summary>
        /// <param name="id">流程ID</param>
        /// <returns>删除结果</returns>
        [HttpPost("Delete")]
        [ProducesResponseType(typeof(JsonResponse<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(JsonResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync([FromQuery] long id)
        {
            try
            {
                var fsql = _freeSql;

                var rowsAffected = await fsql.Delete<FlowEntity>()
                    .Where(x => x.Id == id)
                    .ExecuteAffrowsAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("delete flow success，ID: {Id}", id);
                    return Ok(true);
                }

                return Error($"delete flow failed，ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "delete flow failed，ID: {Id}", id);
                throw new WebApiException($"delete flow failed: {ex.Message}");
            }
        }

    }
}
