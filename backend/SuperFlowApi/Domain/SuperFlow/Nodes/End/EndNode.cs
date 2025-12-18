
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 结束节点
    /// </summary>
    public class EndNode : NodeBase
    {
        public override Task<INodeExecuteResult> ExecuteInnerAsync(FlowRuntimeContext context, FlowRuntimeService runtime)
        {
            throw new NotImplementedException();
        }
    }
}
