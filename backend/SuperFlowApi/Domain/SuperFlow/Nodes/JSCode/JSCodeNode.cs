using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// JS代码节点输出定义
    /// </summary>
    public class JSCodeNodeOutput
    {
        /// <summary>
        /// 输出名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public VariableItemType VariableType { get; set; }
    }

    /// <summary>
    /// JS代码节点 - 执行JavaScript代码并返回多个输出
    /// </summary>
    public class JSCodeNode : NodeBase
    {
        /// <summary>
        /// 是否有输出
        /// </summary>
        [JsonProperty("hasOutput")]
        public bool HasOutput { get; set; } = true;

        /// <summary>
        /// 输出列表
        /// JS 函数的 return 对象需要与此列表匹配
        /// 例如: return { x: 1, y: "ok" } 对应 outputs: [{ name: "x" }, { name: "y" }]
        /// </summary>
        [JsonProperty("outputs")]
        public List<JSCodeNodeOutput> Outputs { get; set; } = new();

        /// <summary>
        /// JS表达式单元（包含代码和执行逻辑）
        /// </summary>
        [JsonProperty("codeUnit")]
        public JSExpressionUnit CodeUnit { get; set; } = new();

        /// <summary>
        /// 执行逻辑
        /// </summary>
        public override async Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            try
            {
                var executeResult = await CodeUnit.ComputeValue(context, runtime);

                var jtoken = executeResult.TOJToken();

                JObject resultObject = new JObject();
                foreach (var output in Outputs)
                {
                    if (jtoken[output.Name] != null)
                    {
                        resultObject[output.Name] = jtoken[output.Name];
                    }
                    else
                        resultObject[output.Name] = null;
                }

                return NodeExecuteResult.Success(Id, resultObject);
            }
            catch (WebApiException ex)
            {
                return NodeExecuteResult.Error(Id, ex.Message);
            }
            catch (Exception ex)
            {
                return NodeExecuteResult.Error(Id, $"js code node execute failed: {ex.Message}");
            }
        }
    }
}
