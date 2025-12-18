using System;
using FreeSql.DataAnnotations;

namespace SuperFlowApi.Domain.SuperFlow.Entities
{
    /// <summary>
    /// 流程对话会话实体
    /// </summary>
    [Table(Name = "FlowChatConversationEntity")]
    [Index("idx_ConversationId_User", "ConversationId asc,User asc", IsUnique = true)]
    public class FlowChatConversationEntity
    {
        /// <summary>
        /// 对话ID
        /// </summary>
        [Column(StringLength = 100, IsPrimary = true, IsIdentity = true)]
        public string ConversationId { get; set; } = string.Empty;
        /// <summary>
        /// 用户标识
        /// </summary>
        [Column(StringLength = 100)]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// 流程ID
        /// </summary> 
        public long FlowId { get; set; }


        /// <summary>
        /// 会话标题（取第一次对话用户说的话前10个字符+...）
        /// </summary>
        [Column(StringLength = 200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 消息数量
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        /// <summary>
        /// 提示词消耗的 token 数
        /// </summary>
        public int? PromptTokens { get; set; }

        /// <summary>
        /// 生成内容消耗的 token 数
        /// </summary>
        public int? CompletionTokens { get; set; }

        /// <summary>
        /// 总 token 数
        /// </summary>
        public int? TotalTokens { get; set; }

        /// <summary>
        /// 会话变量
        /// </summary>
        [Column(DbType = "longtext")]
        [JsonMap]
        public Dictionary<string, JToken?> Variables { get; set; } = new();
    }
}
