namespace SuperFlowApi.Domain.SuperFlow.Dtos
{
    /// <summary>
    /// 大模型提供者DTO
    /// </summary>
    public class FlowLLMProviderDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 平台名称（如：阿里云、OpenAI等）
        /// </summary>
        public string? PlatformName { get; set; }

        /// <summary>
        /// 模型名称列表（如：["gpt-4", "gpt-4-turbo", "gpt-3.5-turbo"]）, 注意模型名称在平台内必须唯一
        /// </summary>
        public List<string>? LLMNames { get; set; }

        /// <summary>
        /// 大模型API地址
        /// </summary>
        public string? LLMAPIUrl { get; set; }

        /// <summary>
        /// 大模型API密钥
        /// </summary>
        public string? LLMAPIKey { get; set; }

        /// <summary>
        /// 从实体转换为DTO
        /// </summary>
        public static FlowLLMProviderDto FromEntity(FlowLLMProviderEntity entity)
        {
            return new FlowLLMProviderDto
            {
                Id = entity.Id,
                PlatformName = entity.PlatformName,
                LLMNames = entity.LLMNames,
                LLMAPIUrl = entity.LLMAPIUrl,
                LLMAPIKey = entity.LLMAPIKey
            };
        }

        /// <summary>
        /// 转换为实体
        /// </summary>
        public FlowLLMProviderEntity ToEntity()
        {
            return new FlowLLMProviderEntity
            {
                Id = Id ?? 0,
                PlatformName = PlatformName ?? string.Empty,
                LLMNames = LLMNames ?? new List<string>(),
                LLMAPIUrl = LLMAPIUrl ?? string.Empty,
                LLMAPIKey = LLMAPIKey ?? string.Empty
            };
        }
    }
}
