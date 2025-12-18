

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFlowApi.Domain.SuperFlow.Nodes
{
    /// <summary>
    /// 连接线
    /// </summary>
    public class NodeLine
    {
        /// <summary>
        /// 线id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 开始节点id
        /// </summary>

        public string FromNodeId { get; set; } = string.Empty;

        /// <summary>
        /// 结束节点id
        /// </summary>

        public string ToNodeId { get; set; } = string.Empty;

        /**
   * 起始锚点ID（可选，用于多锚点节点识别具体分支）
   */
        public string? SourceAnchorId { get; set; } 

        /**
   * 目标锚点ID（可选）
   */
        public string? TargetAnchorId { get; set; }
    }
}