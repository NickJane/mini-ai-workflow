
using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlow
{
    /// <summary>
    /// 流程运行时上下文
    /// </summary>
    public class FlowRuntimeContext
    {
        public long FlowId { get; set; }
        public string User { get; set; }
        public long FlowInstanceId { get; set; }
        public string DisplayName { get; set; }
        public FlowConfigInfo FlowConfigInfoForRun { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public DateTime StartTime { get; set; }
        public IFreeSql FreeSql { get; set; }

        /// <summary>
        /// 用户请求的输入参数
        /// </summary>
        public List<Variable> InputVariables { get; set; }
        public Dictionary<string, object> InputVariablesOriginal { get; set; }


        /// <summary>
        /// 会话变量
        /// </summary>
        public virtual List<Variable> Variables { get; set; }

        public HttpContext HttpContext { get; set; }


        /// <summary>
        /// 获取数据流上下文服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService? ResolveService<TService>() where TService : class
        {
            return ServiceProvider.GetService<TService>();
        }


    }
}