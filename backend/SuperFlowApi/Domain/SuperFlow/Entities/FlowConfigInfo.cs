

using SuperFlowApi.Domain.SuperFlow.Nodes;
using SuperFlowApi.Domain.SuperFlow.Parmeters;

namespace SuperFlowApi.Domain.SuperFlow;

public class FlowConfigInfo
{
    // /// <summary>
    // /// 自定义变量
    // /// </summary>
    public IEnumerable<Variable> Variables { get; set; } = new List<Variable>();

    /// <summary>
    /// 输入参数
    /// </summary>
    public IEnumerable<Variable> InputParameters { get; set; } = new List<Variable>();

    // /// <summary>
    // /// 输出参数
    // /// </summary>
    // public IEnumerable<OutputParameter> OutputParameters { get; set; } = Enumerable.Empty<OutputParameter>();

    /// <summary>
    /// 节点
    /// </summary>
    public List<NodeBase> Nodes { get; set; } = new List<NodeBase>();

    /// <summary>
    /// 线
    /// </summary>
    public List<NodeLine> Lines { get; set; } = new List<NodeLine>();

}
