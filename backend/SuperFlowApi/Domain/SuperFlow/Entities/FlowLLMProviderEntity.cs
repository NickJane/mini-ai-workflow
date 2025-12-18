



using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using FreeSql.DataAnnotations;

namespace SuperFlowApi.Domain.SuperFlow
{
    /// <summary>
    /// 大模型提供者管理
    /// </summary>
    [Table(Name = "FlowLLMProviderEntity")]
    [Index("idx_FlowType", "FlowType", false)]
    public class FlowLLMProviderEntity
    {
        public FlowLLMProviderEntity()
        {
        }

        /// <summary>
        /// 主键ID, 实例Id
        /// </summary>
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// 平台名称, 唯一
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 模型名称, 平台内唯一
        /// </summary>
        [JsonMap]
        [Column(DbType = "text")]
        public List<string> LLMNames { get; set; }

        /// <summary>
        /// 大模型API地址
        /// </summary>
        public string LLMAPIUrl { get; set; }

        /// <summary>
        /// 大模型API密钥
        /// </summary>
        public string LLMAPIKey { get; set; }

    }

}