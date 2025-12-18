using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;
using System.Text.RegularExpressions;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    public class ReplyNode : NodeBase
    {
        public ExpressionUnitBase Message { get; set; }
        private string text;

        public override async Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            try
            {
                text = (await Message.ComputeValue(context, runtime))?.ToString() ?? "";


                Func<IAsyncEnumerable<string>> streamingExecutor = () => ParseAndStreamTextAsync(context, runtime);

                return NodeExecuteResult.Success(
                    Id,
                    result: new { mode = "streaming" },
                    streamingExecutor: streamingExecutor
                );
            }
            catch (Exception ex)
            {
                return NodeExecuteResult.Error(Id, $"reply node execute failed: {ex.Message}");
            }
        }

        private async IAsyncEnumerable<string> ParseAndStreamTextAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            // 使用正则表达式匹配所有 {{...}} 占位符
            var regex = new Regex(@"\{\{([^}]+)\}\}");
            int lastIndex = 0;

            foreach (Match match in regex.Matches(text))
            {
                // 1. 输出引用前的普通文字
                if (match.Index > lastIndex)
                {
                    var plainText = text.Substring(lastIndex, match.Index - lastIndex);
                    yield return plainText;
                }

                // 2. 解析引用
                var reference = match.Groups[1].Value.Trim();

                // 判断是变量引用还是节点输出引用
                if (reference.Contains('.'))
                {
                    // 节点输出引用格式: nodeId.jsonpath
                    await foreach (var chunk in ResolveNodeOutputReferenceAsync(reference, context, runtime))
                    {
                        yield return chunk;
                    }
                }
                else
                {
                    // 变量引用格式: variableName
                    var value = ResolveVariableReference(reference, context);
                    if (!string.IsNullOrEmpty(value))
                    {
                        yield return value;
                    }
                }

                lastIndex = match.Index + match.Length;
            }

            // 3. 输出最后剩余的普通文字
            if (lastIndex < text.Length)
            {
                var remainingText = text.Substring(lastIndex);
                yield return remainingText;
            }
        }

        private string ResolveVariableReference(string variableName, FlowRuntimeContext context)
        {
            var variable = context.FlowConfigInfoForRun?.Variables?.FirstOrDefault(x => x.Name == variableName);
            return variable?.GetTypedValue()?.ToString() ?? string.Empty;
        }


        private async IAsyncEnumerable<string> ResolveNodeOutputReferenceAsync(
            string reference,
            FlowRuntimeContext context,
            FlowRuntimeService runtime)
        {
            // 分离 nodeId 和 jsonPath
            var parts = reference.Split(new[] { '.' }, 2);
            if (parts.Length < 2)
            {
                yield return $"[reference format error: {reference}]";
                yield break;
            }

            var nodeId = parts[0].Trim();
            var jsonPath = parts[1].Trim();

            // 获取源节点的执行结果
            var sourceNodeResult = await runtime.GetNodeExecuteResult(context, nodeId);
            if (sourceNodeResult == null)
            {
                yield return $"[node not found: {nodeId}]";
                yield break;
            }

            if (sourceNodeResult.StreamingExecutor != null)
            {
                // 流式输出 LLM 响应
                await foreach (var chunk in sourceNodeResult.StreamingExecutor())
                {
                    yield return chunk;
                }
            }
            else
            {
                var value = ExtractValueByJsonPath(sourceNodeResult.Result, jsonPath);
                if (!string.IsNullOrEmpty(value))
                {
                    yield return value;
                }
            }
        }

        private string ExtractValueByJsonPath(object? source, string jsonPath)
        {
            if (source == null)
            {
                return string.Empty;
            }

            try
            {
                // 将对象序列化为 JToken 以支持 JSONPath 查询
                var json = JsonConvert.SerializeObject(source);
                var jToken = JToken.Parse(json);

                // 使用 JSONPath 选择数据
                // 简化版：直接按属性路径访问
                var pathParts = jsonPath.Split('.');
                JToken current = jToken;

                foreach (var part in pathParts)
                {
                    if (current is JObject jobj && jobj.ContainsKey(part))
                    {
                        current = jobj[part]!;
                    }
                    else if (current is JArray jarray && int.TryParse(part, out int index))
                    {
                        if (index >= 0 && index < jarray.Count)
                        {
                            current = jarray[index];
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }

                // 返回最终值
                return current.Type == JTokenType.String
                    ? current.Value<string>() ?? string.Empty
                    : current.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
