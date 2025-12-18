using Newtonsoft.Json.Schema;
using SuperFlowApi.Domain.SuperFlow.Nodes;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 开始节点
    /// </summary>
    public class StartNode : NodeBase
    {
        public override Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            return Task.FromResult<INodeExecuteResult>(NodeExecuteResult.Success(Id));
        }
    }
}