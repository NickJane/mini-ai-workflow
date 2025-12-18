/**
 * 连接线
 */
export interface NodeLine {
  /**
   * 线id
   */
  id: string;

  /**
   * 开始节点id
   */
  fromNodeId: string;

  /**
   * 结束节点id
   */
  toNodeId: string;

  /**
   * 起始锚点ID（可选，用于多锚点节点识别具体分支）
   */
  sourceAnchorId?: string;

  /**
   * 目标锚点ID（可选）
   */
  targetAnchorId?: string;
}