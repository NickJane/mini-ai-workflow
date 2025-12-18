using Newtonsoft.Json;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;
using System.Text.RegularExpressions;
using SuperFlowApi.Domain.SuperFlow.Entities;
using SuperFlowApi.Domain.SuperFlowAIRun;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// LLM节点 - 调用大语言模型
    /// 支持多种大模型提供商，可配置温度参数、系统提示词和用户提示词
    /// </summary>
    public class LLMNode : NodeBase
    {
        /// <summary>
        /// 模型选择（对应FlowLLMProviderEntity中的某个模型）
        /// 格式：平台名称|模型名称，例如 "阿里云|qwen-max"
        /// </summary>
        public string ModelSelection { get; set; } = string.Empty;

        /// <summary>
        /// 温度参数 - 控制生成的随机性，范围0-1，值越高越随机，值越低越确定
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// 系统提示词 - 定义AI的角色和行为
        /// </summary>
        public FullTextExpressionUnit SystemPrompt { get; set; } = new();

        /// <summary>
        /// 用户提示词 - 用户的具体问题或指令，支持变量引用 ${variableName}
        /// </summary>
        public FullTextExpressionUnit UserPrompt { get; set; } = new();

        /// <summary>
        /// 是否启用记忆功能（上下文对话）
        /// </summary>
        public bool MemoryEnabled { get; set; } = false;

        /// <summary>
        /// 记忆轮数 - 保留最近N轮对话作为上下文，仅在MemoryEnabled为true时生效
        /// </summary>
        public int MemoryRounds { get; set; } = 5;

        /// <summary>
        /// 最后一次执行的 Token 使用情况（用于流式输出完成后获取）
        /// </summary>
        [JsonIgnore]
        public TokenUsage? LastTokenUsage { get; private set; }

        public bool? EnableThinking { get; set; } = false;

        public JSExpressionUnit? Pictures { get; set; } = null;

        /// <summary>
        /// 执行节点逻辑 - 调用大语言模型并返回结果
        /// </summary>
        public override async Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            var aiContext = context as FlowRuntimeAIContext;
            try
            {
                #region 参数验证
                // 用户提示词与系统提示词不能都为空
                if (string.IsNullOrWhiteSpace(UserPrompt.Text) && string.IsNullOrWhiteSpace(SystemPrompt.Text))
                {
                    return NodeExecuteResult.Error(Id, "system prompt and user prompt cannot both be empty");
                }

                // 1. 验证模型选择
                if (string.IsNullOrWhiteSpace(ModelSelection))
                {
                    return NodeExecuteResult.Error(Id, "model selection is required");
                }

                // 2. 解析模型选择（格式：平台名称|模型名称）
                var parts = ModelSelection.Split('|');
                if (parts.Length != 2)
                {
                    return NodeExecuteResult.Error(Id, $"model selection format error, should be 'platform name|model name', current value: {ModelSelection}");
                }

                string platformName = parts[0].Trim();
                string modelName = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(platformName) || string.IsNullOrWhiteSpace(modelName))
                {
                    return NodeExecuteResult.Error(Id, "platform name or model name cannot be empty");
                }

                // 3. 获取FreeSql服务并查询LLM提供者配置

                var llmProvider = await context.FreeSql.Select<FlowLLMProviderEntity>()
                    .Where(x => x.PlatformName == platformName)
                    .FirstAsync();

                if (llmProvider == null)
                {
                    return NodeExecuteResult.Error(Id, $"platform '{platformName}' configuration not found, please configure in the background");
                }

                // 4. 验证模型名称是否在该平台的支持列表中
                if (llmProvider.LLMNames == null || !llmProvider.LLMNames.Contains(modelName))
                {
                    var supportedModels = llmProvider.LLMNames != null ? string.Join(", ", llmProvider.LLMNames) : "no";
                    return NodeExecuteResult.Error(Id, $"platform '{platformName}' does not support model '{modelName}', supported models: {supportedModels}");
                }
                #endregion

                // 5. 替换提示词中的变量引用
                string processedSystemPrompt = (await SystemPrompt.ComputeValue(context, runtime))?.ToString() ?? "";
                string processedUserPrompt = (await UserPrompt.ComputeValue(context, runtime))?.ToString() ?? "";

                // 6. 构建对话消息
                var messages = new List<LLMAIMessageDto<JToken>>();

                // 添加系统消息
                if (!string.IsNullOrWhiteSpace(processedSystemPrompt))
                {
                    messages.Add(new LLMAIMessageDto<JToken>
                    {
                        role = "system",
                        content = processedSystemPrompt.TOJToken()
                    });
                }

                // 如果启用记忆功能，添加历史对话
                if (MemoryEnabled && MemoryRounds > 0)
                {
                    var lst = context.FreeSql.Select<FlowChatMessageEntity>().
                                Where(x => x.ConversationId == aiContext.Request.ConversationId).
                                OrderByDescending(x => x.CreatedAt).
                                Take(MemoryRounds).ToList();
                    lst.Reverse();
                    foreach (var item in lst)
                    {
                        messages.Add(new LLMAIMessageDto<JToken>
                        {
                            role = "user",
                            content = item.Question.TOJToken()
                        });
                        messages.Add(new LLMAIMessageDto<JToken>
                        {
                            role = "assistant",
                            content = item.Answer.TOJToken()
                        });
                    }
                }

                // 添加用户消息
                if (!string.IsNullOrWhiteSpace(processedUserPrompt))
                {
                    if (Pictures == null || string.IsNullOrEmpty(Pictures.ExpressionCode))
                    {
                        messages.Add(new LLMAIMessageDto<JToken>
                        {
                            role = "user",
                            content = processedUserPrompt.TOJToken()
                        });
                    }
                    else
                    {
                        var picture = await Pictures.ComputeValue(context, runtime);
                        if (picture != null)
                        {
                            var finalContent = new JArray();
                            var pictureJToken = picture.TOJToken();
                            if (pictureJToken is JArray ja)
                            {
                                ja.ToList().ForEach(x => finalContent.Add(new JObject { { "type", "image_url" }, { "image_url", new JObject { { "url", x["url"].ToString() } } } }));
                            }
                            else
                            {
                                throw new Exception("pictures must be array");
                            }
                            finalContent.Add(new JObject { { "type", "text" }, { "text", processedUserPrompt } });

                            messages.Add(new LLMAIMessageDto<JToken>
                            {
                                role = "user",
                                content = finalContent
                            });
                        }
                    }
                }
                // 8. 封装流式输出委托（只封装委托，不立即执行）
                Func<IAsyncEnumerable<string>> streamingExecutor = () => ExecuteLLMStreamingAsync(
                    llmProvider,
                    modelName,
                    messages,
                    context
                );

                // 9. 返回成功结果
                // Result 包含元数据，StreamingExecutor 用于流式输出
                return NodeExecuteResult.Success(Id,
                    result: new
                    {
                        Platform = platformName,
                        Model = modelName,
                        Temperature = Temperature,
                        MemoryEnabled = MemoryEnabled
                    },
                    streamingExecutor: streamingExecutor
                );
            }
            catch (WebApiException ex)
            {
                return NodeExecuteResult.Error(Id, ex);
            }
            catch (Exception ex)
            {
                return NodeExecuteResult.Error(Id, $"llm node execute failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行 LLM 流式输出（包含三件事：调用API、保存历史、记录token）
        /// </summary>
        private async IAsyncEnumerable<string> ExecuteLLMStreamingAsync(
            FlowLLMProviderEntity llmProvider, string modelName, List<LLMAIMessageDto<JToken>> messages,
            FlowRuntimeContext context
            )
        {
            TokenUsage tokenUsage = new();
            var fullResponse = new System.Text.StringBuilder();

            // 1. 根据平台名称获取对应的适配器
            var adapter = LLMAdapterFactory.GetAdapter(llmProvider.PlatformName);

            // 2. 流式调用 LLM API（使用适配器）
            await foreach (var chunk in adapter.CallStreamingAsync(
                llmProvider.LLMAPIUrl,
                llmProvider.LLMAPIKey,
                modelName,
                messages,
                Temperature,
                tokenUsage,
                EnableThinking ?? false))
            {
                fullResponse.Append(chunk);
                yield return chunk;
            }

        }

    }

    public class LLMAIMessageDto
    {
        public string role { get; set; }
        public virtual string content { get; set; }
    }
    public class LLMAIMessageDto<T> : LLMAIMessageDto
    {
        public virtual T content { get; set; }
    }

    /// <summary>
    /// LLM 执行结果（包含响应和 token 使用情况）
    /// </summary>
    public class LLMExecutionResult
    {
        /// <summary>
        /// LLM 响应内容
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Token 使用情况
        /// </summary>
        public TokenUsage TokenUsage { get; set; } = new TokenUsage();
    }

    /// <summary>
    /// Token 使用情况
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// 提示词消耗的 token 数
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// 生成内容消耗的 token 数
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// 总 token 数
        /// </summary>
        public int TotalTokens { get; set; }
    }
}
