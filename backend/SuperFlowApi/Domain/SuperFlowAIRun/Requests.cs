using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlowAIRun
{
    #region Chat API Models

    /// <summary>
    /// Dify 聊天请求模型
    /// </summary>
    public class AIChatRequest
    {
        /// <summary>
        /// 对话内容
        /// </summary>
        [Required]
        public string Query { get; set; } = default!;

        /// <summary>
        /// 用户标识, 前端可不传, 后端自动获取
        /// </summary>
        [Required]
        public string User { get; set; } = default!;

        /// <summary>
        /// 会话ID，用于继续特定会话; 参考dify的逻辑, 传空的话, 则创建新的会话
        /// </summary>
        public string? ConversationId { get; set; }

        /// <summary>
        /// 是否启用流式响应, blocking: 阻止模式, streaming: 流式模式
        /// </summary>
        public string ResponseMode { get; set; } = "streaming";

        /// <summary>
        /// 附加输入参数, 该参数对应入参
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 文件列表，支持上传图片、文档等文件
        /// </summary>
        public List<AIFileRequest> Files { get; set; } = new List<AIFileRequest>();



        /// <summary>
        /// 构建入参信息
        /// </summary>
        /// <param name="config"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static List<Variable> BuildInputParameters(FlowConfigInfo config, Dictionary<string, object?> inputs)
        {
            var result = new List<Variable>();

            if (!inputs.Any() && !config.InputParameters.Any())
                return result;

            // 遍历入参定义
            foreach (var parmDefine in config.InputParameters)
            {
                try
                {
                    string findName = parmDefine.Name;

                    // 请求参数中存在该入参
                    if (inputs.ContainsKey(findName))
                    {
                        var value = inputs[findName];

                        // 如果入参值为null
                        if (value == null)
                        {
                            if(parmDefine.Required)
                            {
                                throw new ArgumentException($"input parmeter [{parmDefine.Name}] can not be null");
                            }

                            parmDefine.SetValue(null, out string? errorMsg1); // 入参值为null
                            result.Add(parmDefine);
                            continue;
                        }

                            var flag = parmDefine.SetValue(JToken.FromObject(value), out string? errorMsg);
                            if (flag == false)
                                throw new ArgumentException($"input parmeter [{parmDefine.Name}] value: [{value}], not fit {parmDefine.GetType().Name} format");
                        

                        result.Add(parmDefine);
                    }
                    else
                    {
                        // 请求体中不存在入参 检查是否必填t
                        if (parmDefine.Required)
                            throw new ArgumentException($"input parmeter [{parmDefine.Name}] can not be null");
                        else
                        {
                            // 请求体中不存在入参, 并且该入参非必填, 则直接加入即可, 用的时候使用默认值
                            result.Add(parmDefine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new WebApiException($"{ex.Message}");
                }
            }

            return result;
        }


    }

    /// <summary>
    /// LLM Token使用情况
    /// </summary>
    public class LLMUsage
    {
        /// <summary>
        /// 提示词Token数
        /// </summary>
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// 提示词单价
        /// </summary>
        [JsonProperty("prompt_unit_price")]
        public string PromptUnitPrice { get; set; } = string.Empty;

        /// <summary>
        /// 提示词价格单位
        /// </summary>
        [JsonProperty("prompt_price_unit")]
        public string PromptPriceUnit { get; set; } = string.Empty;

        /// <summary>
        /// 提示词价格
        /// </summary>
        [JsonProperty("prompt_price")]
        public string PromptPrice { get; set; } = string.Empty;

        /// <summary>
        /// 补全Token数
        /// </summary>
        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// 补全单价
        /// </summary>
        [JsonProperty("completion_unit_price")]
        public string CompletionUnitPrice { get; set; } = string.Empty;

        /// <summary>
        /// 补全价格单位
        /// </summary>
        [JsonProperty("completion_price_unit")]
        public string CompletionPriceUnit { get; set; } = string.Empty;

        /// <summary>
        /// 补全价格
        /// </summary>
        [JsonProperty("completion_price")]
        public string CompletionPrice { get; set; } = string.Empty;

        /// <summary>
        /// 总Token数
        /// </summary>
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }

        /// <summary>
        /// 总价格
        /// </summary>
        [JsonProperty("total_price")]
        public string TotalPrice { get; set; } = string.Empty;

        /// <summary>
        /// 货币
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// 延迟时间（毫秒）
        /// </summary>
        [JsonProperty("latency")]
        public double Latency { get; set; }
    }

    /// <summary>
    /// AI请求文件模型, 仅支持在线文件
    /// </summary>
    public class AIFileRequest
    {
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节） （当transfer_method为local_file时使用）
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// 文件MIME类型 （当transfer_method为local_file时使用）
        /// </summary>
        public string? MimeType { get; set; } = string.Empty;
    }

    #endregion

}