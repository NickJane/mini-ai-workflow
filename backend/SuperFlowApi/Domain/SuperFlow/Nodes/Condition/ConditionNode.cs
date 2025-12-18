using Newtonsoft.Json.Schema;
using SuperFlowApi.Domain.SuperFlow.ComputeUnits;
using SuperFlowApi.Domain.SuperFlow.Nodes;
using Nop.WebApiFramework.Exceptions;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 条件节点规则
    /// </summary>
    public class ConditionNodeRule
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// JS表达式单元，用于评估条件是否满足
        /// </summary>
        public ExpressionUnitBase ExpressionUnit { get; set; }
        
        /// <summary>
        /// 条件描述（可选）
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 满足条件时的输出连接ID
        /// </summary>
        public string? LineId { get; set; }
    }
    
    /// <summary>
    /// 条件节点 - 根据 JS 表达式结果决定执行路径
    /// 按顺序评估每个条件，返回第一个满足条件的连接线
    /// 如果所有条件都不满足，返回 ElseRule 的连接线
    /// </summary>
    public class ConditionNode : NodeBase
    {
        /// <summary>
        /// 条件规则列表，按顺序评估
        /// </summary>
        public List<ConditionNodeRule> Conditions { get; set; } = new List<ConditionNodeRule>();
        
        /// <summary>
        /// 默认规则，当所有条件都不满足时执行
        /// </summary>
        public ConditionNodeRule ElseRule { get; set; }

        /// <summary>
        /// 执行节点内部逻辑
        /// 条件节点本身不执行具体操作，只负责选择路径
        /// </summary>
        public override Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            // 条件节点不需要执行额外逻辑，只通过 GetExecuteLine 选择路径
            return Task.FromResult<INodeExecuteResult>(SuperFlow.NodeExecuteResult.Success(Id, "condition node execute success"));
        }

        /// <summary>
        /// 根据条件评估结果获取执行线路
        /// </summary>
        public override async Task<NodeLine?> GetExecuteLine(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            // 按顺序评估每个条件
            if (Conditions != null && Conditions.Count > 0)
            {
                foreach (var condition in Conditions)
                {
                    if (condition.ExpressionUnit == null)
                    {
                        throw new WebApiException($"condition node {condition} rule {condition.Description} expression unit is null");
                    }

                    if (string.IsNullOrWhiteSpace(condition.LineId))
                    {
                        throw new WebApiException($"condition node {DisplayName} rule {condition.Description} line id is null");
                    }

                    try
                    {
                        // 执行表达式计算
                        var result = await condition.ExpressionUnit.ComputeValue(context, runtime);
                        
                        // 判断结果是否为 true
                        bool isConditionMet = false;
                        if (result is bool boolResult)
                        {
                            isConditionMet = boolResult;
                        }
                        else if (result != null)
                        {
                            // 尝试将结果转换为布尔值
                            // 非空、非零、非空字符串都视为 true
                            isConditionMet = Convert.ToBoolean(result);
                        }

                        // 如果条件满足，返回对应的连接线
                        if (isConditionMet)
                        {
                            var line = context.FlowConfigInfoForRun.Lines.FirstOrDefault(x => x.Id == condition.LineId);
                            if (line == null)
                            {
                                throw new WebApiException($"condition node {DisplayName} rule {condition.Description} line {condition.LineId} not found");
                            }
                            return line;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new WebApiException($"condition node {DisplayName} rule {condition.Description} execute failed: {ex.Message}");
                    }
                }
            }

            // 所有条件都不满足，使用 ElseRule
            if (ElseRule != null && !string.IsNullOrWhiteSpace(ElseRule.LineId))
            {
                var elseLine = context.FlowConfigInfoForRun.Lines.FirstOrDefault(x => x.Id == ElseRule.LineId);
                if (elseLine == null)
                {
                    throw new WebApiException($"condition node {DisplayName} default rule line {ElseRule.LineId} not found");
                }
                return elseLine;
            }

            throw new WebApiException($"condition node {DisplayName} no available execute path");
        }
    }
}