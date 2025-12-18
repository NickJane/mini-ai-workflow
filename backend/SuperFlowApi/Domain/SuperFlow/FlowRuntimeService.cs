namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 流程运行时服务
    /// </summary>
    public abstract class FlowRuntimeService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IFreeSql _freeSql;
        
        /// <summary>
        /// 当前正在执行的节点
        /// </summary>
        public NodeBase? CurrentNode { get; protected set; }

        public FlowRuntimeService(IServiceProvider serviceProvider, IFreeSql freeSql)
        {
            _serviceProvider = serviceProvider;
            _freeSql = freeSql;
        }

        public abstract Task RunAsync(FlowRuntimeContext context);

        public virtual Task<INodeExecuteResult?> GetNodeExecuteResult(FlowRuntimeContext context, string nodeId)
        {
            // 默认实现：不支持
            return Task.FromResult<INodeExecuteResult?>(null);
        }
    }
}