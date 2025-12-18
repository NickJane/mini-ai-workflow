import type { VariableItemType } from './Parameters/Variable';

/**
 * 节点输出项
 * 定义节点的单个输出属性（名称+类型）
 */
export interface NodeOutputItem {
  /** 输出属性名称 */
  name: string;
  /** 输出属性类型枚举 */
  variableType: VariableItemType;
}

/**
 * 节点数据接口
 * 定义所有节点的公共数据结构
 */
export interface NodeBase {
  /** 节点ID */
  id?: string;
  /** 节点描述 */
  description?: string;
  /** 节点显示名称 */
  displayName?: string;

  
  /**
   * 类型名称，后端c#用newtonsoft来实现多态序列化
   * 每个节点都应该设置此属性，用于标识节点类型
   */
  typeName: string;
  
  /**
   * 是否有输出结果
   * 标记该节点是否产生输出内容，有输出的节点可以在表达式/提示词中被引用
   * 例如：LLM节点、JS代码节点、HTTP节点等有输出，开始节点、条件节点等无输出
   */
  hasOutput?: boolean;
  
  /**
   * 节点输出定义
   * 定义节点的输出结构（属性名+类型）
   * - LLM节点：固定输出 [{ name: 'text', variable: StringVariable }]
   * - JSCode节点：用户自定义输出
   */
  outputs?: NodeOutputItem[];
  
  /** 其他自定义属性 */
  [key: string]: any;
}

/**
 * 节点基类模型
 * 所有节点模型都应该继承此类
 */
import { HtmlNodeModel } from '@logicflow/core';

export class NodeBaseModel extends HtmlNodeModel {
  /**
   * 默认不允许缩放（避免所有节点都出现四角手柄）
   * 若需要缩放的节点，请在子类中覆写返回 true
   */
  isResizable() {
    return false;
  }

  // ==================== LogicFlow 生命周期方法 ====================
  
  /**
   * 【LogicFlow 生命周期方法】设置节点的默认属性
   * 在节点初始化时自动调用，用于设置节点的默认尺寸、样式等
   * 子类可以重写此方法来定义自己的默认样式
   */
  setAttributes(attributes?: { width?: number; height?: number }) {
    // 默认节点尺寸
    this.width = attributes?.width ?? 240;
    this.height = attributes?.height ?? 90;
    // 默认禁止缩放，避免无需求节点出现四角手柄
    this.resizable = false;
    // 节点文本默认不可编辑
    this.text.editable = false;
  }

  /**
   * 【LogicFlow 方法】设置节点属性
   * 重写此方法以确保属性变化时触发视图更新
   */
  setProperties(properties: Record<string, any>) {
    // 调用父类方法，保持LogicFlow标准行为
    super.setProperties(properties);
  }

  // ==================== 自定义辅助方法 ====================

  /**
   * 【自定义辅助方法】获取节点数据
   * 返回节点的业务属性数据
   */
  getNodeData(): NodeBase {
    return this.properties as NodeBase;
  }

  /**
   * 【自定义辅助方法】设置节点数据
   * 合并更新节点的业务属性数据
   */
  setNodeData(data: Partial<NodeBase>) {
    this.properties = {
      ...this.properties,
      ...data
    };
  }

  // ==================== 自定义回调方法（子类可重写） ====================

  /**
   * 【自定义回调】连线添加回调
   * 当从该节点连接线到其他节点时，在 FlowDesigner.vue 的 edge:add 事件中被调用
   * 子类可重写此方法来处理连线添加逻辑（如条件节点需要更新规则的 lineId）
   * 
   * @param edgeId 连接线ID
   * @param sourceAnchorId 起始锚点ID
   * @param targetNodeId 目标节点ID
   * @param targetAnchorId 目标锚点ID
   */
  onEdgeAdd(edgeId: string, sourceAnchorId: string, targetNodeId: string, targetAnchorId: string): void {
    // 默认实现为空，子类可重写此方法来处理连线添加逻辑
  }

  /**
   * 【自定义回调】连线删除回调
   * 当删除从该节点出发的连线时，在 FlowDesigner.vue 的 edge:delete 事件中被调用
   * 子类可重写此方法来处理连线删除逻辑（如条件节点需要清除规则的 lineId）
   * 
   * @param edgeId 连接线ID
   * @param sourceAnchorId 起始锚点ID
   */
  onEdgeDelete(edgeId: string, sourceAnchorId: string): void {
    // 默认实现为空，子类可重写此方法来处理连线删除逻辑
  }

  /**
   * 【自定义回调】属性更新后的回调
   * 当节点属性通过属性面板保存后，在 FlowDesigner.vue 的 handleSaveNodeProperties 中被调用
   * 子类可重写此方法来处理属性更新后的逻辑（如条件节点需要刷新连线位置）
   * 
   * 使用场景：
   * - 条件节点添加/删除条件后，节点高度变化，需要刷新所有相关连线的位置
   * - 其他可能影响节点尺寸或锚点位置的属性变化
   * 
   * @param lf LogicFlow实例，用于操作画布（如删除/添加边）
   * @param flowStore Pinia状态管理实例，可获取所有节点和边的数据
   */
  onPropertiesUpdated(lf: any, flowStore: any): void {
    // 默认实现为空，子类可重写此方法来处理属性更新后的逻辑
  }
}