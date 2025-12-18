using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Parmeters;
using SuperFlowApi.Domain.SuperFlowAIRun;

namespace SuperFlowApi.Domain.SuperFlow.Dtos
{
    /// <summary>
    /// 本地会话列表响应
    /// </summary>
    public class LocalConversationsResponse
    {
        /// <summary>
        /// 会话列表数据
        /// </summary>
        public List<LocalConversation> Data { get; set; } = new List<LocalConversation>();

        /// <summary>
        /// 是否有更多数据
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// 本次返回数量限制
        /// </summary>
        public int Limit { get; set; }
    }

    /// <summary>
    /// 本地会话信息
    /// </summary>
    public class LocalConversation
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 会话标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 最后消息时间
        /// </summary>
        public DateTime LastMessageTime { get; set; }

        /// <summary>
        /// 消息数量
        /// </summary>
        public int MessageCount { get; set; }
    }

    /// <summary>
    /// 本地消息列表响应
    /// </summary>
    public class LocalMessagesResponse
    {
        /// <summary>
        /// 消息列表数据
        /// </summary>
        public List<LocalMessage> Data { get; set; } = new List<LocalMessage>();

        /// <summary>
        /// 是否有更多数据
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// 本次返回数量限制
        /// </summary>
        public int Limit { get; set; }
    }

    /// <summary>
    /// 本地消息信息
    /// </summary>
    public class LocalMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        public string ConversationId { get; set; } = string.Empty;

        /// <summary>
        /// 用户问题
        /// </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// AI回答
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// 附件文件列表
        /// </summary>
        public List<AIFileRequest> Files { get; set; } = new List<AIFileRequest>();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Token总数
        /// </summary>
        public int? TotalTokens { get; set; }
    }

    /// <summary>
    /// 更新会话标题请求
    /// </summary>
    public class UpdateConversationTitleRequest
    {
        /// <summary>
        /// 新标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
    }
}
