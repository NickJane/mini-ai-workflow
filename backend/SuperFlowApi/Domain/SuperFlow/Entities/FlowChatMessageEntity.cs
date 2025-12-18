using System;
using FreeSql.DataAnnotations;
using SuperFlowApi.Domain.SuperFlowAIRun;

namespace SuperFlowApi.Domain.SuperFlow.Entities
{
    /// <summary>
    /// 对话消息实体
    /// </summary>
    [Table(Name = "FlowChatMessageEntity")]
    [Index("idx_UserId_ConversationId_CreatedAt", "UserId asc,ConversationId asc,CreatedAt asc", IsUnique = false)]
    [Index("idx_AppName_CreatedAt", "AppName asc,CreatedAt asc", IsUnique = false)]
    public class FlowChatMessageEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Column(IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column(StringLength = 100)]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// 流程ID
        /// </summary>
        public long FlowId { get; set; }

        /// <summary>
        /// 对话ID
        /// </summary>
        [Column(StringLength = 100)]
        public string ConversationId { get; set; } = string.Empty;

        /// <summary>
        /// 流程实例ID, 表示一次对话的唯一标识
        /// </summary>
        public long FlowInstanceId { get; set; } 

        /// <summary>
        /// 用户问题
        /// </summary>
        [Column(DbType = "text")]
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// AI回答
        /// </summary>
        [Column(DbType = "text")]
        public string Answer { get; set; } = string.Empty;



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
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        
        public List<AIFileRequest> Files { get; set; } = new List<AIFileRequest>();
    }
}
