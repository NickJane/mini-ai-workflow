<!--
  统一流程设计器
  使用LogicFlow实现Dify风格的流程设计
  通过路由参数flowType区分不同类型的流程：logic/ai/approval
-->
<template>
  <div class="flow-designer">
    <!-- 工具栏 -->
    <div class="toolbar">
      <div class="toolbar-left">
        <el-button @click="closeWindow" :icon="Close" text>{{ t('flowDesigner.close') }}</el-button>
        <el-divider direction="vertical" />
        <span class="flow-title">{{ flowTitle }}</span>
      </div>
      
      <div class="toolbar-center">
        <!-- 显示当前流程名称 -->
        <span class="current-flow-name">{{ currentFlowName || t('flowDesigner.unnamedFlow') }}</span>
      </div>

      <div class="toolbar-right">
        <el-button 
          v-if="flowId"
          @click="openChatTest" 
          type="success" 
          :icon="VideoPlay"
        >
          {{ t('flowDesigner.run') }}
        </el-button>
        <FlowExecutionLogDropdown
          v-if="flowId"
          :flow-id="flowId"
        />
        <el-button @click="saveFlow" type="primary" :icon="DocumentAdd">
          {{ t('flowDesigner.save') }}
        </el-button>
      </div>
    </div>

    <!-- LogicFlow 画布容器 -->
    <div id="lf-container" ref="lfContainer" @contextmenu.prevent="handleCanvasContextMenu"></div>

    <!-- 画布底部控制栏 -->
    <CanvasControlBar 
      :zoom-percent="currentZoomPercent"
      @add-node="handleOpenAddNodeDialog"
      @zoom-in="zoomIn"
      @zoom-out="zoomOut"
      @reset-zoom="resetZoom"
    />

    <!-- 右键菜单组件 -->
    <FlowRightMenuComponent
      ref="rightMenuRef"
      :visible="contextMenuVisible"
      :position="contextMenuPosition"
      :nodes="availableNodes"
      @close="closeContextMenu"
      @select-node="addNodeAtPosition"
    />

    <!-- 节点/连线右键菜单 -->
    <NodeContextMenu
      :visible="nodeContextMenuVisible"
      :position="nodeContextMenuPosition"
      :target-type="contextMenuTargetType"
      :target-id="contextMenuTargetId"
      @close="closeNodeContextMenu"
      @delete="handleDeleteTarget"
    />

    <!-- 节点属性面板 -->
    <NodePropertiesPanel
      :visible="propertiesPanelVisible"
      @update:visible="propertiesPanelVisible = $event"
      @save="handleSaveNodeProperties"
    />

    <!-- 变量管理面板 -->
    <div 
      v-if="showVariablePanel" 
      class="variable-panel"
      :class="{ 'panel-expanded': showVariablePanel }"
      @click="handlePanelOverlayClick"
    >
      <div class="panel-content" @click.stop>
        <div class="panel-header">
          <h3>{{ t('flowDesigner.processConfig') }}</h3>
          <el-button 
            type="text" 
            :icon="Close" 
            @click="toggleVariablePanel"
            class="close-btn"
          />
        </div>
        
        <el-tabs v-model="activeVariableTab" class="variable-tabs">
        <el-tab-pane :label="t('flowDesigner.inputParameters')" name="input">
          <VariableManager
            type="input"
            :variables="flowStore.currentInputParameters"
            :all-variables="allVariables"
            @add="handleAddInputParameter"
            @update="handleUpdateInputParameter"
            @delete="handleDeleteInputParameter"
          />
        </el-tab-pane>
        
        <el-tab-pane :label="t('flowDesigner.sessionVariables')" name="session">
          <VariableManager
            type="session"
            :variables="flowStore.currentSessionVariables"
            :all-variables="allVariables"
            @add="handleAddSessionVariable"
            @update="handleUpdateSessionVariable"
            @delete="handleDeleteSessionVariable"
          />
        </el-tab-pane>
        </el-tabs>
      </div>
    </div>

    <!-- 变量管理按钮 -->
    <el-button 
      v-if="!showVariablePanel"
      class="variable-toggle-btn"
      type="primary"
      :icon="Setting"
      @click="toggleVariablePanel"
      :title="t('flowDesigner.manageVariables')"
    >
      {{ t('flowDesigner.variables') }}
    </el-button>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import LogicFlow, { BezierEdge, BezierEdgeModel } from '@logicflow/core';
import '@logicflow/core/dist/index.css';
import { Group, NodeResize } from '@logicflow/extension';
import '@logicflow/extension/dist/index.css';
import { ZoomIn, ZoomOut, FullScreen, DocumentAdd, Close, Setting, VideoPlay } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { FlowService } from '@/services/flow.service';

// 导入节点配置系统
import { getAvailableNodes, loadNodeClass, type NodeConfig } from '@/types/flow-designer/nodeConfig';
import { generateNodeId } from '@/utils/uuid';
import { useFlowDesignerStore } from '@/stores/flowDesigner';
import type { IFlowConfigInfo, IUpdateFlowRequest } from '@/types/flow.types';
import type { NodeBase } from '@/types/flow-designer/NodeBase';
import type { NodeLine } from '@/types/flow-designer/NodeLine';

// 导入右键菜单组件
import FlowRightMenuComponent from './components/FlowRightMenuComponent.vue';
// 导入节点属性面板组件
import NodePropertiesPanel from './components/NodePropertiesPanel.vue';
// 导入变量管理组件
import VariableManager from './components/VariableManager.vue';
// 导入画布控制栏组件
import CanvasControlBar from './components/CanvasControlBar.vue';
// 导入节点/连线右键菜单组件
import NodeContextMenu from './components/NodeContextMenu.vue';
// 导入运行日志下拉组件
import FlowExecutionLogDropdown from './components/FlowExecutionLogDropdown.vue';
// 导入变量类型
import type { AnyVariable } from '@/types/flow-designer/Parameters/Variable';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const lfContainer = ref<HTMLElement | null>(null);
const rightMenuRef = ref<InstanceType<typeof FlowRightMenuComponent> | null>(null);
let lf: LogicFlow | null = null;

// 自定义贝塞尔曲线边类（类似 Dify 的曲线效果）
class CustomBezierModel extends BezierEdgeModel {
  setAttributes(): void {
    super.setAttributes();
    // 设置曲线的偏移量，控制曲线的弯曲程度
    this.offset = this?.properties?.offset || 100;
  }
  
  getEdgeStyle() {
    const style = super.getEdgeStyle();
    // 优化贝塞尔曲线样式
    return {
      ...style,
      stroke: '#94a3b8',
      strokeWidth: 1.5,
      // 添加动画效果（可选）
    };
  }
}

const customBezier = {
  type: 'customBezier',
  view: BezierEdge,
  model: CustomBezierModel,
};

// 状态管理
const flowStore = useFlowDesignerStore();

// 从路由获取流程类型和ID
const flowType = computed(() => route.params.flowType as string);
const flowId = computed(() => route.params.id ? Number(route.params.id) : null);

// 当前流程名称
const currentFlowName = ref<string>('');

// 右键菜单状态（画布空白处）
const contextMenuVisible = ref(false);
const contextMenuPosition = ref({ x: 0, y: 0 });
const clickPosition = ref({ x: 0, y: 0 }); // 记录画布点击位置

// 节点/连线右键菜单状态
const nodeContextMenuVisible = ref(false);
const nodeContextMenuPosition = ref({ x: 0, y: 0 });
const contextMenuTargetType = ref<'node' | 'edge' | null>(null);
const contextMenuTargetId = ref<string | null>(null);

// 属性面板状态
const propertiesPanelVisible = ref(false);

// 变量管理面板状态
const showVariablePanel = ref(false);
const activeVariableTab = ref<'input' | 'session'>('input');

// 缩放状态
const currentZoomPercent = ref(80); // 初始化为80%

// 根据流程类型获取标题
const flowTitle = computed(() => {
  const titles: Record<string, string> = {
    'logic': t('flowDesigner.logicFlowTitle'),
    'ai': t('flowDesigner.aiFlowTitle'),
    'approval': t('flowDesigner.approvalFlowTitle')
  };
  return titles[flowType.value] || t('flowDesigner.title');
});

// 所有变量（用于唯一性检查）
const allVariables = computed(() => {
  return [
    ...flowStore.currentInputParameters,
    ...flowStore.currentSessionVariables
  ];
});

// 获取当前流程类型的可用节点配置
const availableNodes = ref<NodeConfig[]>([]);

// 根据flowType更新可用节点
watch(flowType, (newType) => {
  if (newType === 'logic') {
    availableNodes.value = getAvailableNodes('logic');
  } else if (newType === 'ai') {
    availableNodes.value = getAvailableNodes('ai');
  } else if (newType === 'approval') {
    availableNodes.value = getAvailableNodes('approval');
  }
}, { immediate: true });

// 打开添加节点对话框
const handleOpenAddNodeDialog = () => {
  // 将对话框显示在屏幕中心位置
  const centerX = window.innerWidth / 2;
  const centerY = window.innerHeight / 2;
  
  contextMenuPosition.value = {
    x: centerX - 140, // 根据菜单宽度调整
    y: centerY - 100
  };
  
  // 设置节点位置：底部控制栏上方 + 随机 100-200px
  if (lf && lfContainer.value) {
    // 获取画布容器的尺寸
    const rect = lfContainer.value.getBoundingClientRect();
    
    // 计算底部控制栏上方的位置（控制栏高度约 64px）
    const bottomControlBarHeight = 64;
    const randomOffset = Math.floor(Math.random() * 101) + 100; // 100-200 之间的随机值
    
    // 计算屏幕坐标：画布底部 - 控制栏高度 - 随机偏移
    const screenY = rect.bottom - bottomControlBarHeight - randomOffset;
    const screenX = rect.left + rect.width / 2; // 水平居中
    
    // 转换为画布坐标
    const point = lf.getPointByClient(screenX, screenY);
    clickPosition.value = {
      x: point.canvasOverlayPosition.x,
      y: point.canvasOverlayPosition.y
    };
  }
  
  contextMenuVisible.value = true;
  // 直接打开节点选择器
  if (rightMenuRef.value) {
    // 延迟一下让菜单先显示，然后再点击
    setTimeout(() => {
      const addNodeItem = document.querySelector('.context-menu-item:not(.disabled)') as HTMLElement;
      if (addNodeItem) {
        addNodeItem.click();
      }
    }, 10);
  }
};

// 关闭窗口
const closeWindow = () => {
  window.close();
};

// 打开聊天测试页面
const openChatTest = () => {
  if (!flowId.value) {
    ElMessage.warning(t('flowDesigner.flowIdNotExist'));
    return;
  }
  
  const routeInfo = router.resolve({
    name: 'flowChatTest',
    params: {
      flowId: String(flowId.value)
    }
  });
  window.open(routeInfo.href, '_blank', 'noopener');
};

// 初始化 LogicFlow
const initLogicFlow = async () => {
  if (!lfContainer.value) return;

  // 确保插件注册（防止仅 plugins 数组未生效的情况）
  LogicFlow.use(Group);
  LogicFlow.use(NodeResize);

  // 创建 LogicFlow 实例
  lf = new LogicFlow({
    container: lfContainer.value,
    grid: {
      size: 10,
      visible: true,
      type: 'dot',
      config: {
        color: '#e1e4e8',
        thickness: 1,
      }
    },
    background: {
      backgroundImage: 'none',
      backgroundColor: '#f8fafc',
    },
    keyboard: {
      enabled: true,
    },
    // 允许节点使用 NodeResize 插件进行缩放
    allowResize: true,
    // 边的样式配置
    edgeStyle: {
      stroke: '#94a3b8',
      strokeWidth: 2,
    },
    // 允许调整边
    adjustEdge: true,
    // 允许调整边的起点和终点
    adjustEdgeStartAndEnd: true,
    // 启用历史记录
    history: true,
    // 设置默认边类型为贝塞尔曲线
    edgeGenerator: () => {
      return 'customBezier';
    },
    // 注册 Group 与 NodeResize 插件，确保分组节点可调整大小
    plugins: [Group, NodeResize],
  });

  // 注册自定义贝塞尔曲线边
  lf.register(customBezier);

  // 动态注册可用节点
  for (const nodeConfig of availableNodes.value) {
    const NodeClass = await loadNodeClass(nodeConfig.typeName);
    if (NodeClass) {
      lf.register(NodeClass);
    }
  }

  // 如果有flowId，从后端加载流程数据
  if (flowId.value) {
    await loadFlowData(flowId.value);
  } else {
    // 如果没有flowId，渲染空画布
    lf.render({ nodes: [], edges: [] });
  }

    // 监听画布事件
    lf.on('node:click', ({ data }: any) => {
      flowStore.addNode({
        id: data.id,
        type: data.type,
        x: data.x,
        y: data.y,
        properties: {
          ...data.properties,
          typeName: data.properties?.typeName || data.type
        } as NodeBase & Record<string, any>
      });
    // 设置选中节点并显示属性面板
    flowStore.selectNode(data.id);
    propertiesPanelVisible.value = true;
  });

  // 监听节点右键点击
  lf.on('node:contextmenu', ({ data, e }) => {
    e.preventDefault();
    nodeContextMenuPosition.value = {
      x: e.clientX,
      y: e.clientY
    };
    contextMenuTargetType.value = 'node';
    contextMenuTargetId.value = data.id;
    nodeContextMenuVisible.value = true;
    // 关闭其他菜单
    contextMenuVisible.value = false;
  });

  lf.on('node:delete', ({ data }) => {
    // 节点删除事件 从状态管理中移除节点
    flowStore.removeNode(data.id);
    
    // 如果删除的是当前选中的节点，关闭属性面板
    if (flowStore.selectedNodeId === data.id) {
      flowStore.selectNode(null);
      propertiesPanelVisible.value = false;
    }
  });

  lf.on('edge:click', ({ data }) => {
    // 连线点击事件
  });

  // 监听连线右键点击
  lf.on('edge:contextmenu', ({ data, e }) => {
    e.preventDefault();
    nodeContextMenuPosition.value = {
      x: e.clientX,
      y: e.clientY
    };
    contextMenuTargetType.value = 'edge';
    contextMenuTargetId.value = data.id;
    nodeContextMenuVisible.value = true;
    // 关闭其他菜单
    contextMenuVisible.value = false;
  });

  lf.on('edge:add', ({ data }) => {
    // 从edgeModel获取锚点ID（必须在添加到Pinia之前获取）
    let sourceAnchorId = '';
    let targetAnchorId = '';
    if (lf) {
      const edgeModel = lf.getEdgeModelById(data.id);
      if (edgeModel) {
        sourceAnchorId = edgeModel.sourceAnchorId || '';
        targetAnchorId = edgeModel.targetAnchorId || '';
      }
    }
    
    // 用户在画布上连接两个节点时触发 添加边到状态管理（保存锚点信息）
    flowStore.addEdge({
      id: data.id,
      sourceNodeId: data.sourceNodeId,
      targetNodeId: data.targetNodeId,
      sourceAnchorId: sourceAnchorId || undefined,  // 空字符串转为undefined
      targetAnchorId: targetAnchorId || undefined   // 空字符串转为undefined
    });
    
    // 调用源节点的onEdgeAdd方法（基类有空实现）
    if (lf) {
      const sourceNodeModel = lf.getNodeModelById(data.sourceNodeId);
      if (sourceNodeModel) {
        sourceNodeModel.onEdgeAdd(
          data.id,
          sourceAnchorId,
          data.targetNodeId,
          targetAnchorId
        );
        
        // 同步更新后的节点数据到Pinia（节点的属性可能已更新，如条件节点的lineId）
        flowStore.updateNodeProperties(data.sourceNodeId, sourceNodeModel.properties);
      }
    }
  });

  lf.on('edge:delete', ({ data }) => {
    // 在删除之前从Pinia获取锚点信息（因为edgeModel已经被删除）
    const edge = flowStore.currentEdges.find(e => e.id === data.id);
    const sourceAnchorId = edge?.sourceAnchorId || '';
    
    // 连线删除事件 从状态管理中移除边
    flowStore.removeEdge(data.id);
    
    // 调用源节点的onEdgeDelete方法（基类有空实现）
    if (lf && sourceAnchorId) {
      const sourceNodeModel = lf.getNodeModelById(data.sourceNodeId);
      if (sourceNodeModel) {
        sourceNodeModel.onEdgeDelete(
          data.id,
          sourceAnchorId
        );
        
        // 同步更新后的节点数据到Pinia（节点的属性可能已更新，如条件节点的lineId）
        flowStore.updateNodeProperties(data.sourceNodeId, sourceNodeModel.properties);
      }
    }
  });

  lf.on('connection:not-allowed', ({ data, msg }) => {
    // 连接不符合规则时触发（如条件节点输出端口已连接）
    ElMessage.warning(msg || t('flowDesigner.connectionNotAllowed'));
  });

  // 监听画布点击事件（点击空白区域时关闭属性面板）
  lf.on('blank:click', () => {
    flowStore.selectNode(null);
    propertiesPanelVisible.value = false;
  });

  // 监听缩放事件，更新缩放百分比
  lf.on('graph:transform', ({ transform }) => {
    if (transform && transform.SCALE_X !== undefined) {
      currentZoomPercent.value = Math.round(transform.SCALE_X * 100);
    }
  });

  // 设置初始缩放为 80%
  setTimeout(() => {
    if (lf) {
      lf.zoom(0.88);
      currentZoomPercent.value = 88;
    }
  }, 100);
};

// 从后端加载流程数据
const loadFlowData = async (id: number) => {
  try {
    const response = await FlowService.getFlowById(id);
    if (response.errCode === 0 && response.data) {
      const flowData = response.data;
      
      // 设置流程名称
      currentFlowName.value = flowData.displayName || t('flowDesigner.unnamedFlow');
      
      // 如果有configInfoForWeb配置，加载到画布
      if (flowData.configInfoForWeb && lf) {
        try {
          const webConfig = JSON.parse(flowData.configInfoForWeb);
          
          // 先同步状态管理数据（包含完整的属性信息）
          if (webConfig.storeData) {
            flowStore.setFlowData(id.toString(), {
              nodes: webConfig.storeData.nodes || [],
              edges: webConfig.storeData.edges || [],
              inputParameters: webConfig.storeData.inputParameters || [],
              sessionVariables: webConfig.storeData.sessionVariables || [],
              name: currentFlowName.value
            });
          }
          
          // 再加载LogicFlow图形数据
          if (webConfig.logicFlowData) {
            // 将所有边的类型转换为贝塞尔曲线
            if (webConfig.logicFlowData.edges) {
              webConfig.logicFlowData.edges = webConfig.logicFlowData.edges.map((edge: any) => ({
                ...edge,
                type: 'customBezier'
              }));
            }
            lf.render(webConfig.logicFlowData as { nodes: any[]; edges: any[] });
          }
          
          // 如果状态管理数据不存在，从LogicFlow重建
          if (!webConfig.storeData) {
            setTimeout(() => {
              if (lf) {
                const currentGraphData = lf.getGraphData() as { nodes: any[]; edges: any[] };
                flowStore.syncFromLogicFlow(currentGraphData);
              }
            }, 100);
          }
          
          ElMessage.success(t('flowDesigner.flowLoadSuccess'));
        } catch (error) {
          console.error('解析流程配置失败:', error);
          ElMessage.warning(t('flowDesigner.configFormatError'));
        }
      } else if (flowData.configInfoForRun) {
        // 如果没有Web配置但有运行配置，尝试从运行配置加载变量数据
        const runConfig = flowData.configInfoForRun;
        flowStore.setFlowData(id.toString(), {
          nodes: [],
          edges: [],
          inputParameters: runConfig.inputParameters || [],
          sessionVariables: runConfig.variables || [],
          name: currentFlowName.value
        });
        
        // 渲染空画布
        if (lf) {
          lf.render({ nodes: [], edges: [] });
        }
        
        ElMessage.info(t('flowDesigner.variableLoadedEmptyCanvas'));
      } else {
        // 如果都没有配置，初始化空的流程数据
        flowStore.setFlowData(id.toString(), {
          nodes: [],
          edges: [],
          inputParameters: [],
          sessionVariables: [],
          name: currentFlowName.value
        });
        
        // 渲染空画布
        if (lf) {
          lf.render({ nodes: [], edges: [] });
        }
        
        ElMessage.info(t('flowDesigner.emptyCanvasInitialized'));
      }
    } else {
      ElMessage.error(response.errMsg || t('flowDesigner.loadFlowFailed'));
    }
  } catch (error) {
    console.error('加载流程数据失败:', error);
    ElMessage.error(t('flowDesigner.loadFlowDataFailed'));
  }
};

onMounted(() => {
  // 初始化流程状态
  const flowIdString = flowId.value ? flowId.value.toString() : 'temp-' + Date.now();
  flowStore.initFlow(flowIdString, currentFlowName.value || t('flowDesigner.unnamedFlow'));
  
  initLogicFlow();
  
  // 点击其他地方关闭菜单
  const closeAllMenus = () => {
    closeContextMenu();
    closeNodeContextMenu();
  };
  document.addEventListener('click', closeAllMenus);
});

// 组件卸载时销毁 LogicFlow 实例
onBeforeUnmount(() => {
  if (lf) {
    lf.destroy();
    lf = null;
  }
  // 清理所有事件监听
  const closeAllMenus = () => {
    closeContextMenu();
    closeNodeContextMenu();
  };
  document.removeEventListener('click', closeAllMenus);
});

// 处理画布右键菜单
const handleCanvasContextMenu = (event: MouseEvent) => {
  // 检查是否点击在节点或边上，如果是则不显示菜单
  const target = event.target as HTMLElement;
  if (target.closest('.lf-node') || target.closest('.lf-edge')) {
    return;
  }
  
  // 记录点击位置（屏幕坐标）
  contextMenuPosition.value = {
    x: event.clientX,
    y: event.clientY
  };
  
  // 记录画布坐标（用于添加节点）
  if (lf) {
    const point = lf.getPointByClient(event.clientX, event.clientY);
    clickPosition.value = {
      x: point.canvasOverlayPosition.x,
      y: point.canvasOverlayPosition.y
    };
  }
  
  contextMenuVisible.value = true;
  event.stopPropagation();
};

// 关闭右键菜单（画布空白处）
const closeContextMenu = () => {
  contextMenuVisible.value = false;
  // 也关闭节点选择器
  if (rightMenuRef.value) {
    rightMenuRef.value.closeSelector();
  }
};

// 关闭节点/连线右键菜单
const closeNodeContextMenu = () => {
  nodeContextMenuVisible.value = false;
  contextMenuTargetType.value = null;
  contextMenuTargetId.value = null;
};

// 处理删除节点/连线
const handleDeleteTarget = (targetType: 'node' | 'edge', targetId: string) => {
  if (!lf) return;
  
  if (targetType === 'node') {
    // 删除节点
    lf.deleteNode(targetId);
    ElMessage.success(t('flowDesigner.nodeDeleted'));
  } else if (targetType === 'edge') {
    // 删除连线
    lf.deleteEdge(targetId);
    ElMessage.success(t('flowDesigner.edgeDeleted'));
  }
};

// 在指定位置添加节点
const addNodeAtPosition = (nodeType: string) => {
  if (!lf) return;
  
  // 查找节点配置
  const nodeConfig = availableNodes.value.find(n => n.typeName === nodeType);
  if (!nodeConfig) return;
  
  // 生成唯一节点ID
  const nodeId = generateNodeId(nodeType);
  
  // 创建节点数据（基本属性）
  const newNode = {
    id: nodeId,
    type: nodeType,
    x: clickPosition.value.x,
    y: clickPosition.value.y,
    properties: {
      id: nodeId,
      typeName: nodeType,
      displayName: nodeConfig.name
    }
  };
  
  // 先在LogicFlow中添加节点，让节点模型初始化默认属性
  lf.addNode(newNode);
  
  // 从LogicFlow获取完整的节点数据（包含节点模型设置的所有默认属性）
  const nodeModel = lf.getNodeModelById(nodeId);
  if (nodeModel) {
    const completeNode = {
      id: nodeId,
      type: nodeType,
      x: nodeModel.x,
      y: nodeModel.y,
      properties: { 
        ...nodeModel.properties,
        id: nodeId,
        typeName: nodeType
      } as NodeBase & Record<string, any>  // 确保包含必需属性
    };
    
    // 同步完整数据到状态管理
    flowStore.addNode(completeNode);
  }
  
  ElMessage.success(t('flowDesigner.nodeAdded', { name: nodeConfig.name }));
};

// 放大画布
const zoomIn = () => {
  if (!lf) return;
  lf.zoom(true);
  // 缩放百分比会通过 graph:transform 事件自动更新
};

// 缩小画布
const zoomOut = () => {
  if (!lf) return;
  lf.zoom(false);
  // 缩放百分比会通过 graph:transform 事件自动更新
};

// 重置缩放
const resetZoom = () => {
  if (!lf) return;
  lf.resetZoom();
  lf.resetTranslate();
  currentZoomPercent.value = 100;
};


// 生成用于运行的配置信息 (轻量级)
const generateConfigInfoForRun = (): IFlowConfigInfo => {
  const nodes: NodeBase[] = flowStore.currentNodes.map(node => {
    const runNode: NodeBase = {
      id: node.id,
      description: node.properties.description,
      displayName: node.properties.displayName,
      typeName: node.properties.typeName
    };
    
    // 添加其他节点特有属性（排除UI相关属性）
    Object.entries(node.properties).forEach(([key, value]) => {
      if (!['x', 'y', 'width', 'height', 'zIndex', 'id', 'description', 'displayName', 'typeName'].includes(key)) {
        (runNode as any)[key] = value;
      }
    });
    
    return runNode;
  });

  const lines: NodeLine[] = flowStore.currentEdges.map(edge => ({
    id: edge.id,
    fromNodeId: edge.sourceNodeId,
    toNodeId: edge.targetNodeId,
    sourceAnchorId: edge.sourceAnchorId,  // 包含起始锚点ID（对于多锚点节点如条件节点很重要）
    targetAnchorId: edge.targetAnchorId   // 包含目标锚点ID
  }));

  return {
    // 包含变量和参数数据
    variables: flowStore.currentSessionVariables,
    inputParameters: flowStore.currentInputParameters,
    nodes,
    lines
  };
};

// 生成用于Web回显的配置信息 (完整数据)
const generateConfigInfoForWeb = (): string => {
  const webConfig = {
    // LogicFlow 完整数据
    logicFlowData: lf?.getGraphData() || { nodes: [], edges: [] },
    // Pinia 状态管理数据
    storeData: {
      nodes: flowStore.currentNodes,
      edges: flowStore.currentEdges,
      inputParameters: flowStore.currentInputParameters,
      sessionVariables: flowStore.currentSessionVariables
    },
    // 其他前端需要的配置信息
    designerConfig: {
      flowType: flowType.value,
      lastModified: new Date().toISOString(),
      version: '1.0.0'
    }
  };
  
  return JSON.stringify(webConfig);
};

// 保存流程
const saveFlow = async () => {
  if (!lf) {
    ElMessage.error(t('flowDesigner.canvasNotInitialized'));
    return;
  }

  if (!flowId.value) {
    ElMessage.error(t('flowDesigner.cannotSaveWithoutFlowId'));
    return;
  }

  try {
    // 获取当前画布数据
    const currentGraphData = lf.getGraphData() as { nodes: any[]; edges: any[] };
    
    // 同步节点位置信息
    currentGraphData.nodes.forEach((lfNode: any) => {
      const existingNode = flowStore.currentNodes.find(n => n.id === lfNode.id);
      if (existingNode) {
        flowStore.updateNodePosition(lfNode.id, lfNode.x, lfNode.y);
      }
    });
    
    // 同步连接线数据（包含锚点信息）
    flowStore.currentFlow?.edges.splice(0);
    currentGraphData.edges.forEach((lfEdge: any) => {
      // 从edgeModel获取完整的锚点信息
      const edgeModel = lf ? lf.getEdgeModelById(lfEdge.id) : null;
      flowStore.addEdge({
        id: lfEdge.id,
        sourceNodeId: lfEdge.sourceNodeId,
        targetNodeId: lfEdge.targetNodeId,
        sourceAnchorId: edgeModel?.sourceAnchorId,  // 添加锚点信息
        targetAnchorId: edgeModel?.targetAnchorId,  // 添加锚点信息
        startPoint: lfEdge.startPoint,
        endPoint: lfEdge.endPoint
      });
    });
    
    // 生成两种配置信息
    const configInfoForRun = generateConfigInfoForRun();
    const configInfoForWeb = generateConfigInfoForWeb();

    // 调用API保存
    const updateRequest: IUpdateFlowRequest = {
      id: flowId.value,
      configInfoForRun,
      configInfoForWeb
    };

    const response = await FlowService.updateFlow(updateRequest);
    
    if (response.errCode === 0) {
      ElMessage.success(t('flowDesigner.flowSaveSuccess'));
    } else {
      ElMessage.error(response.errMsg || t('flowDesigner.saveFailed'));
    }
  } catch (error) {
    console.error('保存流程失败:', error);
    ElMessage.error(t('flowDesigner.saveFlowFailed'));
  }
};

// 处理保存节点属性
const handleSaveNodeProperties = (nodeId: string, properties: any) => {
  if (!lf) return;
  
  try {
    // 更新状态管理中的节点属性
    flowStore.updateNodeProperties(nodeId, properties);
    
    // 直接更新LogicFlow节点模型的属性（不重新渲染整个画布）
    const nodeModel = lf.getNodeModelById(nodeId);
    if (nodeModel) {
      // 使用setProperties方法更新节点属性
      nodeModel.setProperties(properties);
      
      // 调用节点的属性更新后回调（子类如条件节点会重写此方法来刷新连线）
      if (typeof nodeModel.onPropertiesUpdated === 'function') {
        // 直接传递 flowStore，让节点自己获取需要的数据
        nodeModel.onPropertiesUpdated(lf, flowStore);
      }
    }
    
    ElMessage.success(t('flowDesigner.nodePropertiesSaved'));
  } catch (error) {
    console.error('更新节点属性失败:', error);
    ElMessage.error(t('flowDesigner.updateNodePropertiesFailed'));
  }
};

// === 变量管理方法 ===

// 切换变量管理面板
const toggleVariablePanel = () => {
  showVariablePanel.value = !showVariablePanel.value;
};

// 处理面板遮罩点击（点击遮罩关闭面板）
const handlePanelOverlayClick = () => {
  showVariablePanel.value = false;
};

// 输入参数管理方法
const handleAddInputParameter = (variable: AnyVariable) => {
  flowStore.addInputParameter(variable);
  console.log('添加输入参数:', variable);
};

const handleUpdateInputParameter = (variable: AnyVariable) => {
  flowStore.updateInputParameter(variable);
  console.log('更新输入参数:', variable);
};

const handleDeleteInputParameter = (variable: AnyVariable) => {
  flowStore.removeInputParameter(variable);
  console.log('删除输入参数:', variable);
};

// 会话变量管理方法
const handleAddSessionVariable = (variable: AnyVariable) => {
  flowStore.addSessionVariable(variable);
  console.log('添加会话变量:', variable);
};

const handleUpdateSessionVariable = (variable: AnyVariable) => {
  flowStore.updateSessionVariable(variable);
  console.log('更新会话变量:', variable);
};

const handleDeleteSessionVariable = (variable: AnyVariable) => {
  flowStore.removeSessionVariable(variable);
  console.log('删除会话变量:', variable);
};
</script>

<style scoped>
.flow-designer {
  width: 100vw;
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: #ffffff;
  overflow: hidden;
}

.toolbar {
  height: 60px;
  padding: 0 20px;
  background-color: #ffffff;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.toolbar-left,
.toolbar-center,
.toolbar-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.flow-title {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

/* 当前流程名称 */
.current-flow-name {
  font-size: 18px;
  font-weight: 600;
  color: #1f2937;
  padding: 8px 16px;
  background-color: #f3f4f6;
  border-radius: 6px;
}

#lf-container {
  flex: 1;
  position: relative;
  background-color: #f8fafc;
}

/* Dify风格节点样式 - 新版横向布局 */
:deep(.dify-node) {
  display: flex;
  flex-direction: column;
  padding: 12px;
  background: #ffffff;
  border: 1.5px solid #e5e7eb;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  cursor: pointer;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

:deep(.dify-node:hover) {
  border-color: #3b82f6;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
  transform: translateY(-1px);
}

:deep(.lf-node-selected .dify-node) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* 节点头部：图标 + 名称 */
:deep(.node-header) {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
}

/* 节点图标 */
:deep(.node-icon) {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  flex-shrink: 0;
  color: #ffffff;
}

:deep(.start-node .node-icon) {
  background: linear-gradient(135deg, #06b6d4 0%, #3b82f6 100%);
}

:deep(.reply-node .node-icon) {
  background: linear-gradient(135deg, #8b5cf6 0%, #ec4899 100%);
}

:deep(.condition-node .node-icon) {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

:deep(.assign-variable-node .node-icon) {
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
}

:deep(.llm-node .node-icon) {
  background: linear-gradient(135deg, #a855f7 0%, #7c3aed 100%);
}

:deep(.jscode-node .node-icon) {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

/* 节点名称 */
:deep(.node-name) {
  font-size: 14px;
  font-weight: 600;
  color: #1f2937;
  line-height: 20px;
}

/* 节点信息区域 */
:deep(.node-info) {
  display: flex;
  align-items: center;
  gap: 6px;
  min-height: 20px;
}

/* 信息文本 */
:deep(.info-text) {
  font-size: 12px;
  color: #6b7280;
  line-height: 18px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 锚点样式 */
:deep(.lf-anchor) {
  width: 8px;
  height: 8px;
  background: #ffffff;
  border: 2px solid #3b82f6;
  border-radius: 50%;
}

:deep(.lf-anchor:hover) {
  width: 10px;
  height: 10px;
  background: #3b82f6;
  border-color: #2563eb;
}

/* 边的箭头样式 */
:deep(.lf-arrow) {
  fill: #94a3b8;
}

:deep(.lf-edge-selected .lf-arrow) {
  fill: #3b82f6;
}

/* 变量管理面板样式 - 浮动抽屉样式 */
.variable-panel {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 1000;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.variable-panel.panel-expanded {
  opacity: 1;
  pointer-events: all;
}

.variable-panel::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(2px);
}

.panel-content {
  position: absolute;
  top: 60px;
  left: 20px;
  width: 580px;
  height: calc(100vh - 80px);
  background: #ffffff;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  transform: translateX(-600px);
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden;
}

.variable-panel.panel-expanded .panel-content {
  transform: translateX(0);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 8px 8px 0 0;
}

.panel-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: white;
}

.close-btn {
  color: white;
  font-size: 16px;
  padding: 4px;
  transition: opacity 0.2s;
}

.close-btn:hover {
  opacity: 0.8;
  color: white;
}

.variable-tabs {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0;
  overflow: hidden;
  border-top: 1px solid #f0f0f0;
}

.variable-tabs :deep(.el-tabs__content) {
  flex: 1;
  overflow-y: auto;
  padding: 16px 20px 20px;
}

.variable-tabs :deep(.el-tabs__header) {
  margin: 0;
  background: transparent;
  border-bottom: 1px solid #e4e7ed;
}

.variable-tabs :deep(.el-tabs__nav-wrap) {
  padding: 16px 20px 0;
}

.variable-tabs :deep(.el-tabs__nav) {
  background: transparent;
}

/* 变量管理切换按钮 */
.variable-toggle-btn {
  position: fixed;
  top: 50%;
  left: 20px;
  transform: translateY(-50%);
  z-index: 999;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
  border-radius: 50%;
  width: 56px;
  height: 56px;
  padding: 0;
  font-size: 12px;
  transition: all 0.3s ease;
}

.variable-toggle-btn:hover {
  transform: translateY(-50%) scale(1.05);
  box-shadow: 0 4px 20px rgba(64, 158, 255, 0.3);
}

/* 面板展开时隐藏切换按钮（避免重叠） */
.variable-panel.panel-expanded ~ .variable-toggle-btn {
  opacity: 0;
  pointer-events: none;
}

/* 移除画布挤压样式 - 现在变量面板是浮动的 */
</style>

<!-- 全局样式：变量管理对话框 -->
<style>
/* 输入参数对话框样式 */
.input-dialog .el-dialog__header {
  background: linear-gradient(135deg, #409EFF, #337ecc);
  color: white;
}

.input-dialog .el-dialog__title {
  color: white;
}

/* 会话变量对话框样式 */
.session-dialog .el-dialog__header {
  background: linear-gradient(135deg, #67C23A, #529b2e);
  color: white;
}

.session-dialog .el-dialog__title {
  color: white;
}

/* 确保dialog在最高层级 */
.input-dialog.el-dialog,
.session-dialog.el-dialog {
  z-index: 2000 !important;
}

/* 确保遮罩层也在合适的层级 */
.el-overlay.is-message-box {
  z-index: 1999 !important;
}
</style>
