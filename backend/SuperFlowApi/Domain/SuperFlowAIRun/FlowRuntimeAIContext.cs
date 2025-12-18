



using SuperFlowApi.Domain.SuperFlow;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlowAIRun;

public class FlowRuntimeAIContext : FlowRuntimeContext
{
    public AIChatRequest Request { get; set; }

    /// <summary>
    /// dify的逻辑, 会话级别的变量;
    /// </summary>
    /// <value></value>
    public override List<Variable> Variables { get; set; }

    public int DialogueCount { get; set; }
}