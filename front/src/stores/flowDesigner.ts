/**
 * 流程设计器状态管理 Store
 * 使用 Pinia 管理流程数据，支持多个流程实例
 */
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { NodeBase } from '@/types/flow-designer/NodeBase';
import type { AnyVariable } from '@/types/flow-designer/Parameters/Variable';

// 节点数据接口
export interface FlowNode {
  id: string;
  type: string;
  x: number;
  y: number;
  properties: NodeBase & Record<string, any>;
}

// 连接线数据接口
export interface FlowEdge {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  /** 起始锚点ID（用于识别多锚点节点的具体分支，如条件节点的不同条件分支） */
  sourceAnchorId?: string;
  /** 目标锚点ID */
  targetAnchorId?: string;
  startPoint?: { x: number; y: number };
  endPoint?: { x: number; y: number };
}

// 流程数据接口
export interface FlowData {
  id: string;
  name: string;
  nodes: FlowNode[];
  edges: FlowEdge[];
  /** 输入参数列表 */
  inputParameters: AnyVariable[];
  /** 会话变量列表 */
  sessionVariables: AnyVariable[];
}

// 全局状态接口
export interface FlowDesignerState {
  flows: Record<string, FlowData>; // 流程ID -> 流程数据
  currentFlowId: string | null;    // 当前活跃的流程ID
  selectedNodeId: string | null;   // 当前选中的节点ID
}

export const useFlowDesignerStore = defineStore('flowDesigner', () => {
  // 状态
  const flows = ref<Record<string, FlowData>>({});
  const currentFlowId = ref<string | null>(null);
  const selectedNodeId = ref<string | null>(null);

  // Getters
  const currentFlow = computed(() => {
    return currentFlowId.value ? flows.value[currentFlowId.value] : null;
  });

  const currentNodes = computed(() => {
    return currentFlow.value?.nodes || [];
  });

  const currentEdges = computed(() => {
    return currentFlow.value?.edges || [];
  });

  const selectedNode = computed(() => {
    if (!selectedNodeId.value || !currentFlow.value) return null;
    return currentFlow.value.nodes.find(node => node.id === selectedNodeId.value) || null;
  });

  const currentInputParameters = computed(() => {
    return currentFlow.value?.inputParameters || [];
  });

  const currentSessionVariables = computed(() => {
    return currentFlow.value?.sessionVariables || [];
  });

  // Actions
  
  /**
   * 初始化或切换到指定流程
   */
  const initFlow = (flowId: string, flowName: string = '未命名流程') => {
    if (!flows.value[flowId]) {
      flows.value[flowId] = {
        id: flowId,
        name: flowName,
        nodes: [],
        edges: [],
        inputParameters: [],
        sessionVariables: []
      };
    }
    currentFlowId.value = flowId;
    selectedNodeId.value = null;
  };

  /**
   * 设置流程数据（从后端加载时使用）
   */
  const setFlowData = (flowId: string, data: { 
    nodes: FlowNode[]; 
    edges: FlowEdge[]; 
    name?: string;
    inputParameters?: AnyVariable[];
    sessionVariables?: AnyVariable[];
  }) => {
    if (!flows.value[flowId]) {
      flows.value[flowId] = {
        id: flowId,
        name: data.name || '未命名流程',
        nodes: [],
        edges: [],
        inputParameters: [],
        sessionVariables: []
      };
    }
    
    flows.value[flowId].nodes = data.nodes;
    flows.value[flowId].edges = data.edges;
    flows.value[flowId].inputParameters = data.inputParameters || [];
    flows.value[flowId].sessionVariables = data.sessionVariables || [];
    if (data.name) {
      flows.value[flowId].name = data.name;
    }
  };

  /**
   * 添加节点
   */
  const addNode = (node: FlowNode) => {
    if (!currentFlow.value) return;
    
    // 检查节点ID是否已存在
    const existingIndex = currentFlow.value.nodes.findIndex(n => n.id === node.id);
    if (existingIndex >= 0) {
      // 如果存在，则更新
      currentFlow.value.nodes[existingIndex] = node;
    } else {
      // 如果不存在，则添加
      currentFlow.value.nodes.push(node);
    }
  };

  /**
   * 删除节点
   */
  const removeNode = (nodeId: string) => {
    if (!currentFlow.value) return;
    
    // 删除节点
    const nodeIndex = currentFlow.value.nodes.findIndex(n => n.id === nodeId);
    if (nodeIndex >= 0) {
      currentFlow.value.nodes.splice(nodeIndex, 1);
    }
    
    // 删除相关连接线
    currentFlow.value.edges = currentFlow.value.edges.filter(
      edge => edge.sourceNodeId !== nodeId && edge.targetNodeId !== nodeId
    );
    
    // 如果删除的是当前选中节点，清除选中状态
    if (selectedNodeId.value === nodeId) {
      selectedNodeId.value = null;
    }
  };

  /**
   * 更新节点属性
   */
  const updateNodeProperties = (nodeId: string, properties: Record<string, any>) => {
    if (!currentFlow.value) return;
    
    const node = currentFlow.value.nodes.find(n => n.id === nodeId);
    if (node) {
      node.properties = { ...node.properties, ...properties };
    }
  };

  /**
   * 更新节点位置
   */
  const updateNodePosition = (nodeId: string, x: number, y: number) => {
    if (!currentFlow.value) return;
    
    const node = currentFlow.value.nodes.find(n => n.id === nodeId);
    if (node) {
      node.x = x;
      node.y = y;
    }
  };

  /**
   * 添加连接线
   */
  const addEdge = (edge: FlowEdge) => {
    if (!currentFlow.value) return;
    
    // 检查连接线ID是否已存在
    const existingIndex = currentFlow.value.edges.findIndex(e => e.id === edge.id);
    if (existingIndex >= 0) {
      currentFlow.value.edges[existingIndex] = edge;
    } else {
      currentFlow.value.edges.push(edge);
    }
  };

  /**
   * 删除连接线
   */
  const removeEdge = (edgeId: string) => {
    if (!currentFlow.value) return;
    
    const edgeIndex = currentFlow.value.edges.findIndex(e => e.id === edgeId);
    if (edgeIndex >= 0) {
      currentFlow.value.edges.splice(edgeIndex, 1);
    }
  };

  /**
   * 选中节点
   */
  const selectNode = (nodeId: string | null) => {
    selectedNodeId.value = nodeId;
  };

  /**
   * 获取LogicFlow格式的图形数据
   */
  const getLogicFlowData = () => {
    if (!currentFlow.value) return { nodes: [], edges: [] };
    
    return {
      nodes: currentFlow.value.nodes.map(node => ({
        id: node.id,
        type: node.type,
        x: node.x,
        y: node.y,
        properties: node.properties
      })),
      edges: currentFlow.value.edges.map(edge => {
        const edgeData: any = {
          id: edge.id,
          sourceNodeId: edge.sourceNodeId,
          targetNodeId: edge.targetNodeId
        };
        
        // 只有当锚点信息存在时才添加（避免undefined导致LogicFlow报错）
        if (edge.sourceAnchorId) edgeData.sourceAnchorId = edge.sourceAnchorId;
        if (edge.targetAnchorId) edgeData.targetAnchorId = edge.targetAnchorId;
        if (edge.startPoint) edgeData.startPoint = edge.startPoint;
        if (edge.endPoint) edgeData.endPoint = edge.endPoint;
        
        return edgeData;
      })
    };
  };

  /**
   * 从LogicFlow数据同步到状态管理
   */
  const syncFromLogicFlow = (data: { nodes: any[]; edges: any[] }) => {
    if (!currentFlow.value) return;
    
    // 同步节点数据
    currentFlow.value.nodes = data.nodes.map(node => ({
      id: node.id,
      type: node.type,
      x: node.x,
      y: node.y,
      properties: node.properties || {}
    }));
    
    // 同步连接线数据
    currentFlow.value.edges = data.edges.map(edge => ({
      id: edge.id,
      sourceNodeId: edge.sourceNodeId,
      targetNodeId: edge.targetNodeId,
      startPoint: edge.startPoint,
      endPoint: edge.endPoint
    }));
  };

  /**
   * 清空当前流程
   */
  const clearCurrentFlow = () => {
    if (!currentFlow.value) return;
    
    currentFlow.value.nodes = [];
    currentFlow.value.edges = [];
    currentFlow.value.inputParameters = [];
    currentFlow.value.sessionVariables = [];
    selectedNodeId.value = null;
  };

  // === 变量管理方法 ===

  /**
   * 添加输入参数
   */
  const addInputParameter = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    // 检查名称是否重复
    const existingIndex = currentFlow.value.inputParameters.findIndex(v => v.name === variable.name);
    if (existingIndex >= 0) {
      // 如果存在，则更新
      currentFlow.value.inputParameters[existingIndex] = variable;
    } else {
      // 如果不存在，则添加
      currentFlow.value.inputParameters.push(variable);
    }
  };

  /**
   * 更新输入参数
   */
  const updateInputParameter = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    const index = currentFlow.value.inputParameters.findIndex(v => v.name === variable.name);
    if (index >= 0) {
      currentFlow.value.inputParameters[index] = variable;
    }
  };

  /**
   * 删除输入参数
   */
  const removeInputParameter = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    const index = currentFlow.value.inputParameters.findIndex(v => v.name === variable.name);
    if (index >= 0) {
      currentFlow.value.inputParameters.splice(index, 1);
    }
  };

  /**
   * 设置输入参数列表
   */
  const setInputParameters = (variables: AnyVariable[]) => {
    if (!currentFlow.value) return;
    currentFlow.value.inputParameters = variables;
  };

  /**
   * 添加会话变量
   */
  const addSessionVariable = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    // 检查名称是否重复
    const existingIndex = currentFlow.value.sessionVariables.findIndex(v => v.name === variable.name);
    if (existingIndex >= 0) {
      // 如果存在，则更新
      currentFlow.value.sessionVariables[existingIndex] = variable;
    } else {
      // 如果不存在，则添加
      currentFlow.value.sessionVariables.push(variable);
    }
  };

  /**
   * 更新会话变量
   */
  const updateSessionVariable = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    const index = currentFlow.value.sessionVariables.findIndex(v => v.name === variable.name);
    if (index >= 0) {
      currentFlow.value.sessionVariables[index] = variable;
    }
  };

  /**
   * 删除会话变量
   */
  const removeSessionVariable = (variable: AnyVariable) => {
    if (!currentFlow.value) return;
    
    const index = currentFlow.value.sessionVariables.findIndex(v => v.name === variable.name);
    if (index >= 0) {
      currentFlow.value.sessionVariables.splice(index, 1);
    }
  };

  /**
   * 设置会话变量列表
   */
  const setSessionVariables = (variables: AnyVariable[]) => {
    if (!currentFlow.value) return;
    currentFlow.value.sessionVariables = variables;
  };

  return {
    // State
    flows,
    currentFlowId,
    selectedNodeId,
    
    // Getters
    currentFlow,
    currentNodes,
    currentEdges,
    selectedNode,
    currentInputParameters,
    currentSessionVariables,
    
    // Actions
    initFlow,
    setFlowData,
    addNode,
    removeNode,
    updateNodeProperties,
    updateNodePosition,
    addEdge,
    removeEdge,
    selectNode,
    getLogicFlowData,
    syncFromLogicFlow,
    clearCurrentFlow,
    
    // Variable Management Actions
    addInputParameter,
    updateInputParameter,
    removeInputParameter,
    setInputParameters,
    addSessionVariable,
    updateSessionVariable,
    removeSessionVariable,
    setSessionVariables
  };
});
