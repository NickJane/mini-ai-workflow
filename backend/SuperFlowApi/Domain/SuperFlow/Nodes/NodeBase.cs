
using System.Runtime.Serialization;
using Nop.WebApiFramework.Exceptions;


namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 节点能力接口
    /// </summary>
    public interface ISuperFlowNode : IDisposable
    {
        Task<INodeExecuteResult> ExecuteAsync(FlowRuntimeContext context, FlowRuntimeService runtime);
        Task<NodeLine?> GetExecuteLine(FlowRuntimeContext context, FlowRuntimeService runtime);
    }


    [JsonConverter(typeof(JsonInheritanceConverter), "typeName")]
    [KnownType(typeof(StartNode))]
    [KnownType(typeof(EndNode))]
    [KnownType(typeof(ReplyNode))]
    [KnownType(typeof(ConditionNode))]
    [KnownType(typeof(AssignVariableNode))]
    [KnownType(typeof(LLMNode))]
    [KnownType(typeof(JSCodeNode))]
    [KnownType(typeof(HttpNode))]
    public abstract class NodeBase : ISuperFlowNode
    {
        public NodeBase()
        {
        }

        /// <summary>
        /// id
        /// </summary>
        [JsonRequired]
        public string Id { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual Task<NodeLine?> GetExecuteLine(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            var line = context.FlowConfigInfoForRun.Lines.FirstOrDefault(x => x.FromNodeId == Id);
            return Task.FromResult(line);
            
        }


        public async Task<INodeExecuteResult> ExecuteAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            return await this.ExecuteInnerAsync(context, runtime);
        }

        public abstract Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime);


        public virtual Task CallbackAfterExecuteAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            return Task.CompletedTask;
        }
    }
}