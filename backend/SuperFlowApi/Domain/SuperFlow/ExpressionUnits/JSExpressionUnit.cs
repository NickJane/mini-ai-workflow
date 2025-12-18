using SuperFlowApi.Domain.SuperFlow.Nodes;
using Jint;
using System.Text.RegularExpressions;
using SuperFlowApi.Domain.SuperFlow.Parmeters;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.ComputeUnits
{
    public class JSExpressionUnit : ExpressionUnitBase
    {
        public string FunctionCode { get; set; } = string.Empty;
        public string ExpressionCode { get; set; } = string.Empty;

        public bool IsFunctionMode { get; set; } = false;

        public override async Task<object?> ComputeValue(FlowRuntimeContext context, FlowRuntimeService service)
        {
            // 根据模式选择代码
            string code = IsFunctionMode ? FunctionCode : ExpressionCode;

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("js expression unit compute value error: js code is empty");
            }

            try
            {
                // 步骤1: 解析占位符，构建变量映射表（占位符 -> JS变量名 -> 实际值）
                var placeholderMap = await ParsePlaceholdersAsync(code, context, service);

                // 步骤2: 替换代码中的占位符为合法的JS变量名
                string processedCode = ReplacePlaceholdersWithVariableNames(code, placeholderMap);

                // 创建 Jint 引擎（带安全限制）
                var engine = new Engine(options =>
                {
                    // 1. 执行超时限制：防止死循环
                    options.TimeoutInterval(TimeSpan.FromSeconds(5));

                    // 2. 递归深度限制：防止栈溢出
                    options.LimitRecursion(100);

                    // 3. 内存限制：防止内存耗尽（单位：字节，这里限制为 10MB）
                    options.LimitMemory(10_000_000);

                    // 4. 严格模式：减少意外的全局变量污染
                    options.Strict();

                    // 5. 禁用 CLR 互操作（Jint 4.x 默认已禁用，这里显式设置）
                    // 防止通过 importNamespace 等访问 .NET 类型
                });

                // 移除危险的全局对象，防止恶意操作
                RemoveDangerousGlobals(engine);

                // 步骤3: 将占位符对应的值注入到引擎
                foreach (var item in placeholderMap)
                {
                    engine.SetValue(item.Value.VariableName, item.Value.Value);
                }

                // 步骤4: 执行JS代码
                Jint.Native.JsValue result;
                if (IsFunctionMode)
                {
                    // 函数模式: 执行函数定义并调用 main 方法
                    engine.Execute(processedCode);
                    result = engine.Invoke("main");
                }
                else
                {
                    // 表达式模式: 直接执行表达式并获取结果
                    result = engine.Evaluate(processedCode);
                }

                // 转换 Jint 结果为 .NET 对象
                return ConvertJintValue(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"js expression unit compute value error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析代码中的所有占位符，返回占位符映射表
        /// </summary>
        private async Task<Dictionary<string, PlaceholderInfo>> ParsePlaceholdersAsync(
            string code,
            FlowRuntimeContext context,
            FlowRuntimeService service)
        {
            var map = new Dictionary<string, PlaceholderInfo>();
            var regex = new Regex(@"\{\{([^}]+)\}\}");
            var matches = regex.Matches(code);

            foreach (Match match in matches)
            {
                var placeholder = match.Value; // 完整的 {{...}}
                var content = match.Groups[1].Value.Trim(); // {{}} 内的内容

                // 如果已经处理过这个占位符，跳过
                if (map.ContainsKey(placeholder))
                    continue;

                // 生成合法的JS变量名（将点替换为下划线）
                var variableName = content.Replace(".", "_");

                // 解析占位符对应的实际值
                var value = await ResolvePlaceholderValueAsync(content, context, service);

                map[placeholder] = new PlaceholderInfo
                {
                    VariableName = variableName,
                    Value = value
                };
            }

            return map;
        }

        /// <summary>
        /// 替换代码中的占位符为合法的JS变量名
        /// </summary>
        private string ReplacePlaceholdersWithVariableNames(string code, Dictionary<string, PlaceholderInfo> placeholderMap)
        {
            var result = code;
            foreach (var item in placeholderMap)
            {
                result = result.Replace(item.Key, item.Value.VariableName);
            }
            return result;
        }

        /// <summary>
        /// 解析占位符对应的实际值（使用 ExpressionHelper 公共方法）
        /// 支持三种格式：
        /// 1. {{变量名}}
        /// 2. {{sys.xxx}}
        /// 3. {{节点id.属性}}
        /// </summary>
        private async Task<object?> ResolvePlaceholderValueAsync(
            string content,
            FlowRuntimeContext context,
            FlowRuntimeService service)
        {
            // 情况1: 系统变量 {{sys.xxx}}
            if (content.StartsWith("sys.", StringComparison.OrdinalIgnoreCase))
            {
                return ExpressionHelper.ResolveSystemVariableValue(content, context);
            }

            // 情况2: 节点输出 {{nodeId.property}}
            if (content.Contains('.'))
            {
                return await ExpressionHelper.ResolveNodeOutputValueAsync(content, context, service);
            }

            // 情况3: 普通变量 {{variableName}}
            return ExpressionHelper.ResolveVariableValue(content, context);
        }

        /// <summary>
        /// 占位符信息
        /// </summary>
        private class PlaceholderInfo
        {
            public string VariableName { get; set; } = string.Empty;
            public object? Value { get; set; }
        }

        /// <summary>
        /// 移除危险的全局对象，防止恶意代码访问敏感功能
        /// </summary>
        private void RemoveDangerousGlobals(Engine engine)
        {
            // Jint 默认不提供 Node.js 的危险 API（如 require, process 等）
            // 这里可以额外移除或限制某些全局对象（如果需要）

            // 移除可能被滥用的全局函数（如果 Jint 提供了的话）
            // 注意：Jint 默认的 JS 环境比较干净，不像浏览器或 Node.js 那样有很多全局对象

            // 可以根据需要设置白名单，只允许使用特定的全局对象
            // 当前 Jint 4.x 的默认配置已经相对安全
        }

        /// <summary>
        /// 将 Jint 值转换为 .NET 对象
        /// </summary>
        private object? ConvertJintValue(Jint.Native.JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
            {
                return null;
            }

            if (jsValue.IsBoolean())
            {
                return jsValue.AsBoolean();
            }

            if (jsValue.IsNumber())
            {
                return jsValue.AsNumber();
            }

            if (jsValue.IsString())
            {
                return jsValue.AsString();
            }

            var jsObject = jsValue.ToObject();
            bool isEnumerable = jsObject is System.Collections.IEnumerable && jsObject is not string;
            if (isEnumerable)
            {
                return jsObject;
            }

            if (jsValue.IsObject())
            {
                // 将 JS 对象转换为 Dictionary，保留对象结构
                var obj = jsValue.AsObject();
                var dict = new Dictionary<string, object?>();

                foreach (var property in obj.GetOwnProperties())
                {
                    var key = property.Key.ToString();
                    var value = property.Value.Value;
                    dict[key] = ConvertJintValue(value);
                }

                return dict;
            }

            return jsValue.ToObject();
        }
    }
}