namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// LLM 适配器工厂
    /// 根据平台名称创建对应的适配器实例
    /// </summary>
    public static class LLMAdapterFactory
    {
        // 所有支持的适配器列表
        private static readonly List<ILLMAdapter> _adapters = new()
        {
            new AliyunLLMAdapter(),
            // 未来可以在这里添加其他平台的适配器
            // new OpenAILLMAdapter(),
            // new BaiduLLMAdapter(),
        };

        /// <summary>
        /// 根据平台名称获取对应的适配器
        /// </summary>
        /// <param name="platformName">平台名称（如：阿里云、aliyun、OpenAI 等）</param>
        /// <returns>匹配的适配器，如果没有匹配则返回默认的阿里云适配器</returns>
        public static ILLMAdapter GetAdapter(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
            {
                // 默认使用阿里云适配器
                return _adapters[0];
            }

            // 查找支持该平台的适配器（不区分大小写）
            var adapter = _adapters.FirstOrDefault(a =>
                a.SupportedPlatforms.Any(p =>
                    p.Equals(platformName, StringComparison.OrdinalIgnoreCase)));

            // 如果没有找到匹配的适配器，返回默认的阿里云适配器
            return adapter ?? _adapters[0];
        }

        /// <summary>
        /// 获取所有已注册的适配器
        /// </summary>
        public static IReadOnlyList<ILLMAdapter> GetAllAdapters() => _adapters.AsReadOnly();
    }
}
