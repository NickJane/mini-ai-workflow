using Newtonsoft.Json;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.Nodes;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 阿里云通义千问大模型适配器
    /// 支持阿里云的 OpenAI 兼容模式 API
    /// 支持深度思考（reasoning）功能，使用 &lt;think&gt; 标签分离思考过程和最终回复
    /// </summary>
    public class AliyunLLMAdapter : ILLMAdapter
    {
        public string[] SupportedPlatforms => new[] { "阿里云", "aliyun", "Aliyun", "ALIYUN" };

        /// <summary>
        /// 流式调用阿里云 LLM API
        /// 使用 OpenAI 兼容模式，支持 stream_options.include_usage 获取 token 统计
        /// </summary>
        public async IAsyncEnumerable<string> CallStreamingAsync(
            string apiUrl,
            string apiKey,
            string modelName,
            List<LLMAIMessageDto<JToken>> messages,
            double temperature,
            TokenUsage tokenUsage,
            bool enableThinking)
        {
            // 深度思考状态跟踪
            bool isThinkingStarted = false;  // 是否已输出 <think> 开始标签
            bool isAnswering = false;        // 是否已开始输出正式回复
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.Timeout = TimeSpan.FromMinutes(5); // 流式请求可能需要更长时间

            // 构建请求体（OpenAI 兼容格式）
            // 注意：enable_thinking 作为顶层参数传递，符合阿里云API规范
            var requestBody = new
            {
                model = modelName,
                messages = messages,
                temperature = temperature,
                stream = true,
                enable_thinking = enableThinking,  // 深度思考开关（顶层参数）
                stream_options = new  // 阿里云支持此参数以在流式响应中返回 token 使用信息
                {
                    include_usage = true
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new WebApiException($"Aliyun LLM API call failed, status code: {response.StatusCode}, response: {errorText}");
            }

            // 读取流式响应
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6); // 去掉 "data: " 前缀

                if (data == "[DONE]")
                {
                    // 如果思考过程已开始但未结束，补充结束标签
                    if (isThinkingStarted && !isAnswering)
                    {
                        yield return "</think>";
                    }
                    break;
                }

                // 解析 SSE 数据并提取内容和 token 信息
                var parseResult = ParseStreamChunk(data, tokenUsage, enableThinking, ref isThinkingStarted, ref isAnswering);
                if (!string.IsNullOrEmpty(parseResult))
                {
                    yield return parseResult;
                }
            }
        }

        /// <summary>
        /// 解析阿里云流式响应的数据块
        /// 支持 OpenAI 兼容模式和阿里云原生格式
        /// 支持深度思考（reasoning）功能，自动添加 &lt;think&gt; 标签
        /// </summary>
        private string? ParseStreamChunk(
            string data, 
            TokenUsage tokenUsage, 
            bool enableThinking,
            ref bool isThinkingStarted,
            ref bool isAnswering)
        {
            try
            {
                dynamic? chunk = JsonConvert.DeserializeObject(data);

                // ========== 处理 Token 使用信息 ==========
                // 阿里云在启用 stream_options.include_usage 后，
                // 最后一个 chunk 不包含 choices，只包含 usage 信息
                if (chunk?.usage != null)
                {
                    // OpenAI 兼容模式格式
                    tokenUsage.PromptTokens = (int?)chunk.usage.prompt_tokens ?? 0;
                    tokenUsage.CompletionTokens = (int?)chunk.usage.completion_tokens ?? 0;
                    tokenUsage.TotalTokens = (int?)chunk.usage.total_tokens ?? 0;

                    // 如果上面为0，尝试阿里云原生格式
                    if (tokenUsage.TotalTokens == 0)
                    {
                        tokenUsage.PromptTokens = (int?)chunk.usage.input_tokens ?? 0;
                        tokenUsage.CompletionTokens = (int?)chunk.usage.output_tokens ?? 0;
                        tokenUsage.TotalTokens = tokenUsage.PromptTokens + tokenUsage.CompletionTokens;
                    }

                    // 最后一个只包含 usage 的 chunk，不返回内容
                    return null;
                }

                // ========== 处理深度思考和消息内容 ==========
                if (chunk?.choices != null && chunk.choices.Count > 0)
                {
                    var delta = chunk.choices[0].delta;
                    
                    // 处理深度思考内容 (reasoning_content)
                    string? reasoningContent = delta?.reasoning_content?.ToString();
                    if (!string.IsNullOrEmpty(reasoningContent) && enableThinking)
                    {
                        // 第一次遇到思考内容时，添加开始标签
                        if (!isThinkingStarted)
                        {
                            isThinkingStarted = true;
                            return "<think>" + reasoningContent;
                        }
                        return reasoningContent;
                    }
                    
                    // 处理正式回复内容 (content)
                    string? content = delta?.content?.ToString();
                    if (!string.IsNullOrEmpty(content))
                    {
                        // 第一次遇到正式回复时，关闭思考标签（如果之前有开启）
                        if (!isAnswering)
                        {
                            isAnswering = true;
                            if (isThinkingStarted && enableThinking)
                            {
                                return "</think>" + content;
                            }
                        }
                        return content;
                    }
                }

                // 阿里云原生格式: output.text（通常不包含 reasoning_content）
                if (chunk?.output?.text != null)
                {
                    return chunk.output.text.ToString();
                }

                return null;
            }
            catch
            {
                // 忽略解析错误
                return null;
            }
        }
    }
}
