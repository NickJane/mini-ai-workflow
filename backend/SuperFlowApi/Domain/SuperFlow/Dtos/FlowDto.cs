using Newtonsoft.Json;

namespace SuperFlowApi.Domain.SuperFlow.Dtos
{
    /// <summary>
    /// 更新Flow的DTO
    /// </summary>
    public class FlowDto
    {
        /// <summary>
        /// 流ID
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 流显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        public FlowType FlowType { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 数据流运行信息
        /// </summary>
        public FlowConfigInfo? ConfigInfoForRun { get; set; }

        /// <summary>
        /// Web配置信息
        /// </summary>
        public string? ConfigInfoForWeb { get; set; }

        public DateTime? LastModified { get; set; }

        public string? LastModifyBy { get; set; }


        public static FlowDto FromEntity(FlowEntity entity)
        {
            return new FlowDto
            {
                Id = entity.Id,
                DisplayName = entity.DisplayName,
                FlowType = entity.FlowType,
                Description = entity.Description,
                ConfigInfoForRun = entity.ConfigInfoForRun,
                ConfigInfoForWeb = entity.ConfigInfoForWeb,
                LastModified = entity.LastModified,
                LastModifyBy = entity.LastModifyBy
            };
        }
    }
}
