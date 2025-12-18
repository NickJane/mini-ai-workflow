



using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using FreeSql.DataAnnotations;

namespace SuperFlowApi.Domain.SuperFlow
{
    /// <summary>
    /// 流程实体，统一存储所有类型的流程（LogicFlow/AIFlow/ApprovalFlow）
    /// 通过FlowType字段进行区分
    /// </summary>
    [Table(Name = "FlowEntity")]
    [Index("idx_FlowType", "FlowType", false)]
    public class FlowEntity
    {
        public FlowEntity()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// 流程显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 流程类型（LogicFlow/AIFlow/ApprovalFlow）
        /// </summary>
        public FlowType FlowType { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 数据流运行信息
        /// </summary>
        [Column(DbType = "longtext")]
        [JsonMap]
        public FlowConfigInfo? ConfigInfoForRun { get; set; }

        /// <summary>
        /// Web配置信息
        /// </summary>
        [Column(DbType = "longtext")]
        public string? ConfigInfoForWeb { get; set; }

        /// <summary>
        /// 最新修改时间
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// 最新修改人名称
        /// </summary>
        public string? LastModifyBy { get; set; }

    }
    public enum FlowType
    {
        LogicFlow,
        AIFlow,
        ApprovalFlow,

    }

}