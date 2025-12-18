using SuperFlowApi.Domain.SuperFlow.Nodes;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 大模型适配器接口
    /// 用于适配不同的大模型提供商（阿里云、OpenAI、百度等）
    /// </summary>
    public interface ILLMAdapter
    {
        /// <summary>
        /// 适配器支持的平台名称（用于匹配）
        /// </summary>
        string[] SupportedPlatforms { get; }

        /// <summary>
        /// 流式调用 LLM API
        /// </summary>
        /// <param name="apiUrl">API 地址</param>
        /// <param name="apiKey">API 密钥</param>
        /// <param name="modelName">模型名称</param>
        /// <param name="messages">对话消息列表</param>
        /// <param name="temperature">温度参数</param>
        /// <param name="tokenUsage">Token 使用统计对象（引用传递，用于收集 token 信息）</param>
        /// <returns>流式返回的文本块</returns>
        IAsyncEnumerable<string> CallStreamingAsync(
            string apiUrl,
            string apiKey,
            string modelName,
            List<LLMAIMessageDto<JToken>> messages,
            double temperature,
            TokenUsage tokenUsage, bool enableThinking);
    }
}
