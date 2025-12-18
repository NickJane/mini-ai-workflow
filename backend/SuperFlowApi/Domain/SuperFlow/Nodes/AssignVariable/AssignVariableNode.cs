using Newtonsoft.Json.Schema;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;
using SuperFlowApi.Domain.SuperFlow.Nodes;
using Nop.WebApiFramework.Exceptions;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{

    public class AssignmentItem
    {
        /// <summary>
        /// 赋值项ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 目标变量名（会话变量的名称）
        /// </summary>
        public string TargetVariableName { get; set; }

        /// <summary>
        /// 表达式单元（支持多种表达式类型）
        /// </summary>
        public ExpressionUnitBase ExpressionUnit { get; set; }
    }

    /// <summary>
    /// 赋值节点
    /// </summary>
    public class AssignVariableNode : NodeBase
    {
        /// <summary>
        /// 变量赋值列表
        /// </summary>
        public List<AssignmentItem> Assignments { get; set; } = new();

        /// <summary>
        /// 执行逻辑
        /// </summary>
        public override async Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            // 执行赋值操作
            foreach (var assignment in Assignments)
            {
                var result = await assignment.ExpressionUnit.ComputeValue(context, runtime);
                Variable variable = context.FlowConfigInfoForRun.Variables.FirstOrDefault(x => x.Name == assignment.TargetVariableName);
                if (variable == null)
                {
                    throw new WebApiException($"variable {assignment.TargetVariableName} not found");
                }
                variable.SetValue(result.TOJToken(), out string? errorMsg);
            }
            return SuperFlow.NodeExecuteResult.Success(Id, "assign variable node execute success");
        }

    }
}