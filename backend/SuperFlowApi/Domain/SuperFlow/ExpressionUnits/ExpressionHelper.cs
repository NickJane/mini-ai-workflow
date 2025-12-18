using SuperFlowApi.Domain.SuperFlow.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperFlowApi.Domain.SuperFlowAIRun;

namespace SuperFlowApi.Domain.SuperFlow.ComputeUnits;


public static class ExpressionHelper
{
    /// <summary>
    /// 解析文本中的所有占位符并替换为实际值
    /// </summary>
    /// <param name="context">流程运行时上下文</param>
    /// <param name="service">流程运行时服务</param>
    /// <param name="text">包含占位符的文本</param>
    /// <returns>替换后的文本</returns>
    public static async Task<string> ReplacePlaceholdersAsync(FlowRuntimeContext context, FlowRuntimeService service, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        try
        {
            // 使用正则表达式匹配所有 {{...}} 占位符
            var regex = new Regex(@"\{\{([^}]+)\}\}");
            var result = text;

            // 遍历所有匹配项并替换
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var placeholder = match.Value; // 完整的 {{...}}
                var content = match.Groups[1].Value.Trim(); // {{}} 内的内容

                // 解析占位符并获取值
                var value = await ResolvePlaceholderAsync(content, context, service);

                // 替换占位符为实际值
                result = result.Replace(placeholder, value);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"占位符解析失败: {ex.Message}", ex);
        }
    }

    private static async Task<string> ResolvePlaceholderAsync(string content, FlowRuntimeContext context, FlowRuntimeService service)
    {
        if (content.StartsWith("sys.", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveSystemVariable(content, context);
        }

        if (content.Contains('.'))
        {
            return await ResolveNodeOutputAsync(content, context, service);
        }

        return ResolveVariable(content, context);
    }

    private static string ResolveVariable(string variableName, FlowRuntimeContext context)
    {
        var value = ResolveVariableValue(variableName, context);
        return value?.ToString() ?? $"[变量未找到: {variableName}]";
    }

    public static object? ResolveVariableValue(string variableName, FlowRuntimeContext context)
    {
        if (context.Variables != null)
        {
            var variable = context.Variables.FirstOrDefault(v =>
                v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));
            if (variable != null)
            {
                return variable.GetTypedValue();
            }
        }

        if (context.InputVariables != null)
        {
            var variable = context.InputVariables.FirstOrDefault(v =>
                v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));
            if (variable != null)
            {
                return variable.GetTypedValue();
            }
        }

        return null;
    }

    /// <summary>
    /// 解析系统变量（返回字符串形式）
    /// </summary>
    private static string ResolveSystemVariable(string sysVar, FlowRuntimeContext context)
    {
        var value = ResolveSystemVariableValue(sysVar, context);
        return value?.ToString() ?? $"[系统变量未找到: {sysVar}]";
    }

    /// <summary>
    /// 解析系统变量（返回原始值）- 公共方法
    /// 格式: sys.PropertyName
    /// 例如: sys.StartTime, sys.FlowId
    /// </summary>
    public static object? ResolveSystemVariableValue(string sysVar, FlowRuntimeContext context)
    {
        // 移除 "sys." 前缀
        var propertyName = sysVar.Substring(4);
        /*
        const systemVariables = ref<VariableItem[]>([
{ key: 'sys.user', label: 'sys.user', type: 'String', category: 'system' },
{ key: 'sys.flowId', label: 'sys.flowId', type: 'String', category: 'system' },
{ key: 'sys.flowInstanceId', label: 'sys.flowInstanceId', type: 'String', category: 'system' },
{ key: 'sys.dialogueCount', label: 'sys.dialogueCount', type: 'Number', category: 'system' },
{ key: 'sys.conversationId', label: 'sys.conversationId', type: 'String', category: 'system' }
]);

        */
        switch (propertyName)
        {
            case "query":
                return ((FlowRuntimeAIContext)context).Request.Query;
            case "user":
                return context.User;
            case "flowId":
                return context.FlowId;
            case "flowInstanceId":
                return context.FlowInstanceId;
            case "dialogueCount":
                return ((FlowRuntimeAIContext)context).DialogueCount;
            case "conversationId":
                return ((FlowRuntimeAIContext)context).Request.ConversationId;
            case "files":
                return ((FlowRuntimeAIContext)context).Request.Files;
            default:
                throw new WebApiException($"can not resolve system variable: {sysVar}");
        }
    }

    /// <summary>
    /// 解析节点输出引用
    /// 格式: nodeId.jsonPath
    /// 根据当前运行节点类型，决定是否需要完整解析：
    /// 1. 当前是 Reply节点 且引用的是LLM节点 -> 保留占位符 {{nodeId.property}}（用于流式输出）
    /// 2. 其他情况 -> 完整解析并返回节点输出结果
    /// </summary>
    private static async Task<string> ResolveNodeOutputAsync(string reference, FlowRuntimeContext context, FlowRuntimeService service)
    {
        // 分离 nodeId 和 jsonPath
        var parts = reference.Split(new[] { '.' }, 2);
        if (parts.Length < 2)
        {
            // 翻译为英文
            return $"[node reference format error: {reference}]";
        }

        var nodeId = parts[0].Trim();
        var jsonPath = parts[1].Trim();

        // 获取目标节点的定义（用于判断节点类型）
        var targetNode = context.FlowConfigInfoForRun?.Nodes?.FirstOrDefault(n => n.Id == nodeId);
        if (targetNode == null)
        {
            return $"[node not found: {nodeId}]";
        }

        // 判断目标节点是否为 LLM 节点
        bool isLLMNode = targetNode is LLMNode;

        // 判断当前运行节点是否为 Reply 节点
        bool isCurrentReplyNode = service.CurrentNode is ReplyNode;

        // 特殊情况：当前是 Reply节点 且引用的是 LLM节点 -> 保留占位符（用于流式输出）
        if (isCurrentReplyNode && isLLMNode)
        {
            return "{{" + reference + "}}";
        }

        // 常规情况：获取节点执行结果并解析
        var nodeResult = await service.GetNodeExecuteResult(context, nodeId);
        if (nodeResult == null)
        {
            return $"[node not executed: {nodeId}]";
        }

        // 如果该节点有流式输出，且不是在 Reply 节点中引用，则需要等待流式输出完成
        if (nodeResult.StreamingExecutor != null)
        {
            // 对于非 Reply 节点引用的流式节点，需要完整获取输出
            var fullResponse = new System.Text.StringBuilder();
            await foreach (var chunk in nodeResult.StreamingExecutor())
            {
                fullResponse.Append(chunk);
            }
            return fullResponse.ToString();
        }

        // 从节点结果中提取指定路径的值
        return ExtractValueByJsonPath(nodeResult.Result, jsonPath);
    }

    /// <summary>
    /// 解析节点输出引用（返回原始值）- 公共方法
    /// 用于 JSExpressionUnit 等需要获取原始对象值的场景
    /// </summary>
    public static async Task<object?> ResolveNodeOutputValueAsync(
        string reference,
        FlowRuntimeContext context,
        FlowRuntimeService service)
    {
        // 分离 nodeId 和 jsonPath
        var parts = reference.Split(new[] { '.' }, 2);
        if (parts.Length < 2)
        {
            return null;
        }

        var nodeId = parts[0].Trim();
        var jsonPath = parts[1].Trim();

        // 获取节点执行结果
        var nodeResult = await service.GetNodeExecuteResult(context, nodeId);
        if (nodeResult == null)
        {
            return null;
        }

        // 如果该节点有流式输出，需要完整获取输出
        if (nodeResult.StreamingExecutor != null)
        {
            var fullResponse = new System.Text.StringBuilder();
            await foreach (var chunk in nodeResult.StreamingExecutor())
            {
                fullResponse.Append(chunk);
            }
            return fullResponse.ToString();
        }

        // 从节点结果中提取指定路径的值（返回原始对象）
        return ExtractValueByJsonPathAsObject(nodeResult.Result, jsonPath);
    }

    /// <summary>
    /// 使用 JSONPath 从对象中提取值（返回字符串）
    /// 例如: response 或 metadata.model
    /// </summary>
    private static string ExtractValueByJsonPath(object? source, string jsonPath)
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

            // 简化版 JSONPath：直接按属性路径访问
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
                        return $"[array index out of bounds: {part}]";
                    }
                }
                else
                {
                    return $"[property not found: {part}]";
                }
            }

            // 返回最终值
            return current.Type == JTokenType.String
                ? current.Value<string>() ?? string.Empty
                : current.ToString();
        }
        catch (Exception ex)
        {
            return $"[json path parse error: {ex.Message}]";
        }
    }

    /// <summary>
    /// 使用 JSONPath 从对象中提取值（返回原始对象）- 公共方法
    /// 用于 JSExpressionUnit 等需要保留原始类型的场景
    /// </summary>
    public static object? ExtractValueByJsonPathAsObject(object? source, string jsonPath)
    {
        if (source == null)
        {
            return null;
        }

        try
        {
            var json = JsonConvert.SerializeObject(source);
            var jToken = JToken.Parse(json);

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
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            // 返回原生类型或对象
            return current.Type switch
            {
                JTokenType.String => current.Value<string>(),
                JTokenType.Integer => current.Value<long>(),
                JTokenType.Float => current.Value<double>(),
                JTokenType.Boolean => current.Value<bool>(),
                JTokenType.Null => null,
                _ => current.ToObject<object>()
            };
        }
        catch
        {
            return null;
        }
    }
}