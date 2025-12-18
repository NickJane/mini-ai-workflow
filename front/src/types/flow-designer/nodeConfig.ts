/**
 * 流程节点配置系统
 * 定义共享节点池，以及不同流程类型可用的节点
 */

import type { Component } from 'vue';

// 节点配置接口
export interface NodeConfig {
  typeName: string;       // 节点类型，同时用于LogicFlow注册和后端序列化
  name: string;           // 节点显示名称
  icon: string;           // Element Plus 图标组件名
  category: string;       // 节点分类（用于过滤）
  description: string;    // 节点描述
  nodeClass?: any;        // 节点类（延迟加载）
}

// 节点分类
export enum NodeCategory {
  BASIC = 'basic',        // 基础节点（所有流程通用）
  LOGIC = 'logic',        // 逻辑节点（逻辑流程专用）
  AI = 'ai',             // AI节点（AI流程专用）
  APPROVAL = 'approval',  // 审批节点（审批流程专用）
}

// 所有可用节点配置（共享节点池）
export const ALL_NODES: NodeConfig[] = [
  // ==================== 基础节点 ====================
  {
    typeName: 'StartNode',
    name: 'Start',
    icon: 'VideoPlay',
    category: NodeCategory.BASIC,
    description: 'Flow entry point',
  },
  {
    typeName: 'ConditionNode',
    name: 'Condition',
    icon: 'Share',
    category: NodeCategory.BASIC,
    description: 'Branch execution by JS expression',
  },
  {
    typeName: 'AssignVariableNode',
    name: 'Assign',
    icon: 'EditPen',
    category: NodeCategory.BASIC,
    description: 'Assign value to session variable',
  },
  {
    typeName: 'LLMNode',
    name: 'LLM',
    icon: 'MagicStick',
    category: NodeCategory.BASIC,
    description: 'Call large language model',
  },
  {
    typeName: 'JSCodeNode',
    name: 'JS Code',
    icon: 'Document',
    category: NodeCategory.BASIC,
    description: 'Execute JavaScript code',
  },
  {
    typeName: 'HttpNode',
    name: 'HTTP',
    icon: 'Link',
    category: NodeCategory.BASIC,
    description: 'Send HTTP request',
  },


  {
    typeName: 'ReplyNode',
    name: 'Reply',
    icon: 'ChatDotRound',
    category: NodeCategory.AI,
    description: 'Send reply message',
  },
  // {
  //   typeName: 'KnowledgeNode',
  //   name: 'Knowledge',
  //   icon: 'Collection',
  //   category: NodeCategory.AI,
  //   description: 'Retrieve knowledge base',
  // },
  
  // ==================== 审批节点（待开发） ====================
  // {
  //   typeName: 'ApprovalNode',
  //   name: 'Approval',
  //   icon: 'CircleCheck',
  //   category: NodeCategory.APPROVAL,
  //   description: 'Initiate approval',
  // },
  // {
  //   typeName: 'NotifyNode',
  //   name: 'Notify',
  //   icon: 'Bell',
  //   category: NodeCategory.APPROVAL,
  //   description: 'Send notification',
  // },
];

// 逻辑流程可用节点配置（基础 + 逻辑）
export const LOGIC_FLOW_NODES = ALL_NODES.filter(node => 
  [NodeCategory.BASIC, NodeCategory.LOGIC].includes(node.category as NodeCategory)
);

// AI流程可用节点配置（基础 + 逻辑 + AI）
export const AI_FLOW_NODES = ALL_NODES.filter(node => 
  [NodeCategory.BASIC, NodeCategory.LOGIC, NodeCategory.AI].includes(node.category as NodeCategory)
);

// 审批流程可用节点配置（基础 + 审批）
export const APPROVAL_FLOW_NODES = ALL_NODES.filter(node => 
  [NodeCategory.BASIC, NodeCategory.APPROVAL].includes(node.category as NodeCategory)
);

// 流程类型
export type FlowType = 'logic' | 'ai' | 'approval';

/**
 * 根据流程类型获取可用节点配置
 */
export function getAvailableNodes(flowType: FlowType): NodeConfig[] {
  const nodeMap: Record<FlowType, NodeConfig[]> = {
    'logic': LOGIC_FLOW_NODES,
    'ai': AI_FLOW_NODES,
    'approval': APPROVAL_FLOW_NODES,
  };
  
  return nodeMap[flowType] || [];
}

/**
 * 动态导入节点类
 * @param nodeType 节点类型
 */
export async function loadNodeClass(nodeType: string) {
  try {
    // 直接使用nodeType加载对应文件
    // nodeType已经与typeName保持一致，都采用PascalCase命名
    const module = await import(`./nodes/${nodeType}.ts`);
    return module.default;
  } catch (error) {
    console.error(`Failed to load node class: ${nodeType}`, error);
    return null;
  }
}

/**
 * 获取Element Plus图标组件
 * 注意：图标需要在使用的地方单独导入
 */
export const ICON_COMPONENTS = {
  'VideoPlay': 'VideoPlay',
  'ChatDotRound': 'ChatDotRound',
  'Search': 'Search',
  'Share': 'Share',
  'Edit': 'Edit',
  'EditPen': 'EditPen',
  'Refresh': 'Refresh',
  'RefreshRight': 'RefreshRight',
  'MagicStick': 'MagicStick',
  'Document': 'Document',
  'Collection': 'Collection',
  'CircleCheck': 'CircleCheck',
  'Bell': 'Bell',
};
