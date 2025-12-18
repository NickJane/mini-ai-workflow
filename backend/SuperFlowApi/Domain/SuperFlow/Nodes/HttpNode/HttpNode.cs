using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// HTTP 请求节点
    /// 根据配置发送 HTTP 请求，并将响应状态码和响应体作为节点输出
    /// </summary>
    public class HttpNode : NodeBase
    {

        /// <summary>
        /// HTTP 方法，如 GET、POST 等
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// 请求地址（支持表达式占位符）
        /// </summary>
        public FullTextMiniExpressionUnit Url { get; set; } = new();

        /// <summary>
        /// 请求头定义（每行一个 header，格式: Key: Value，支持表达式占位符）
        /// </summary>
        public FullTextExpressionUnit? Headers { get; set; }

        /// <summary>
        /// QueryString（例如: ?id=1&name=xx，支持表达式占位符）
        /// </summary>
        public FullTextExpressionUnit? Query { get; set; }

        /// <summary>
        /// 请求体（通常为 JSON 字符串，支持表达式占位符）
        /// </summary>
        public FullTextExpressionUnit? Body { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        public override async Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            try
            {
                // 1. 计算 URL
                var urlText = (await Url.ComputeValue(context, runtime))?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(urlText))
                {
                    return NodeExecuteResult.Error(Id, "http url is required");
                }

                // 2. 计算 Query 并拼接
                var queryText = Query == null
                    ? string.Empty
                    : (await Query.ComputeValue(context, runtime))?.ToString() ?? string.Empty;

                string finalUrl = urlText;
                if (!string.IsNullOrWhiteSpace(queryText))
                {
                    if (queryText.StartsWith("?") || queryText.StartsWith("&"))
                    {
                        finalUrl += queryText;
                    }
                    else
                    {
                        finalUrl += (urlText.Contains("?") ? "&" : "?") + queryText;
                    }
                }

                // 3. 创建 HttpClient
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(TimeoutSeconds <= 0 ? 30 : TimeoutSeconds)
                };

                // 4. 构建 HttpRequestMessage
                var httpMethod = new HttpMethod(string.IsNullOrWhiteSpace(Method) ? "GET" : Method.ToUpperInvariant());
                using var request = new HttpRequestMessage(httpMethod, finalUrl);

                // 5. 计算 Body
                var bodyText = Body == null
                    ? string.Empty
                    : (await Body.ComputeValue(context, runtime))?.ToString() ?? string.Empty;

                // 只有在允许有请求体的方法中才设置 Content
                if (!string.IsNullOrWhiteSpace(bodyText) &&
                    (httpMethod == HttpMethod.Post ||
                     httpMethod == HttpMethod.Put ||
                     httpMethod == HttpMethod.Patch ||
                     httpMethod.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase)))
                {
                    request.Content = new StringContent(bodyText, Encoding.UTF8, "application/json");
                }

                // 6. 计算 Headers
                var headersText = Headers == null
                    ? string.Empty
                    : (await Headers.ComputeValue(context, runtime))?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(headersText))
                {
                    var lines = headersText
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x));

                    foreach (var line in lines)
                    {
                        var idx = line.IndexOf(':');
                        if (idx <= 0)
                        {
                            continue;
                        }

                        var name = line[..idx].Trim();
                        var value = line[(idx + 1)..].Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        // Content-Type 特殊处理到 Content.Headers
                        if (request.Content != null &&
                            name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(value);
                            continue;
                        }

                        // 其他头放到请求头中
                        if (!request.Headers.TryAddWithoutValidation(name, value))
                        {
                            // 如果无法添加到请求头，尝试添加到内容头
                            if (request.Content != null)
                            {
                                request.Content.Headers.TryAddWithoutValidation(name, value);
                            }
                        }
                    }
                }

                // 7. 发送请求
                using var response = await httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (long)response.StatusCode;

                // 8. 返回执行结果
                var resultObject = new
                {
                    responseBody,
                    statusCode
                };

                return NodeExecuteResult.Success(Id, resultObject);
            }
            catch (WebApiException ex)
            {
                return NodeExecuteResult.Error(Id, ex);
            }
            catch (TaskCanceledException ex)
            {
                // 处理超时异常
                return NodeExecuteResult.Error(Id, $"http node timeout after {TimeoutSeconds} seconds: {ex.Message}");
            }
            catch (Exception ex)
            {
                return NodeExecuteResult.Error(Id, $"http node execute failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// HTTP 节点输出定义（用于前端配置）
    /// </summary>
    public class HttpNodeOutput
    {
        /// <summary>
        /// 输出名称（例如：responseBody、statusCode）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 变量类型名称（例如：StringVariable、LongVariable），前端/配置使用
        /// </summary>
        public string VariableType { get; set; } = string.Empty;
    }
}


