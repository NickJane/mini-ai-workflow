<!--
  流程设计器右键菜单组件
  包含右键菜单和节点选择器
-->
<template>
  <!-- 右键菜单 -->
  <div 
    v-show="visible" 
    class="context-menu"
    :style="{ left: position.x + 'px', top: position.y + 'px' }"
    @click.stop
  >
    <div class="context-menu-item" @click="handleAddNode">
      <span class="menu-icon">➕</span>
      <span>{{ t('flowComponents.addNode') }}</span>
    </div>
    <div class="context-menu-item disabled" @click="handleAddComment">
      <span class="menu-icon">💬</span>
      <span>{{ t('flowComponents.addComment') }}</span>
      <span class="coming-soon">({{ t('flowComponents.comingSoon') }})</span>
    </div>
  </div>

  <!-- 节点选择器面板 -->
  <div 
    v-show="selectorVisible" 
    class="node-selector"
    :style="{ left: selectorPosition.x + 'px', top: selectorPosition.y + 'px' }"
    @click.stop
  >
    <div class="node-selector-header">
      <span>{{ t('flowComponents.selectNode') }}</span>
      <el-icon @click="closeSelector" style="cursor: pointer;"><Close /></el-icon>
    </div>
    <div class="node-selector-body">
      <div 
        v-for="node in nodes" 
        :key="node.typeName"
        class="node-selector-item"
        @click="handleSelectNode(node.typeName)"
      >
        <div class="node-item-icon" :class="getNodeIconClass(node.typeName)">
          <component :is="getNodeIcon(node.icon)" />
        </div>
        <div class="node-item-info">
          <div class="node-item-name">{{ node.name }}</div>
          <div class="node-item-desc">{{ node.description }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { Close } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import type { NodeConfig } from '@/types/flow-designer/nodeConfig';
import * as ElementPlusIcons from '@element-plus/icons-vue';

const { t } = useI18n();

// 定义Props
interface Props {
  visible: boolean;
  position: { x: number; y: number };
  nodes: NodeConfig[];
}

const props = defineProps<Props>();

// 定义Emits
const emit = defineEmits<{
  close: [];
  selectNode: [nodeType: string];
}>();

// 节点选择器状态
const selectorVisible = ref(false);
const selectorPosition = ref({ x: 0, y: 0 });

// 获取节点图标组件
const getNodeIcon = (iconName: string) => {
  return (ElementPlusIcons as any)[iconName] || ElementPlusIcons.Document;
};

// 获取节点图标样式类
const getNodeIconClass = (typeName: string) => {
  const classMap: Record<string, string> = {
    'StartNode': 'start-icon',
    'ReplyNode': 'reply-icon',
    'ConditionNode': 'condition-icon',
    'ForLoopNode': 'forloop-icon',
    'AssignVariableNode': 'assign-icon',
    'LLMNode': 'llm-icon',
    'JSCodeNode': 'jscode-icon',
    'HttpNode': 'http-icon',
  };
  return classMap[typeName] || 'default-icon';
};

// 显示节点选择器
const handleAddNode = () => {
  // 在右键菜单右侧显示节点选择器
  selectorPosition.value = {
    x: props.position.x + 180, // 右键菜单宽度 + 间距
    y: props.position.y
  };
  selectorVisible.value = true;

  // 不发送close事件，让菜单和选择器同时显示
};

// 关闭节点选择器
const closeSelector = () => {
  selectorVisible.value = false;
};

// 选择节点
const handleSelectNode = (nodeType: string) => {
  emit('selectNode', nodeType);
  selectorVisible.value = false;
  emit('close'); // 选择节点后也关闭右键菜单
};

// 添加注释（待实现）
const handleAddComment = () => {
  ElMessage.info(t('flowComponents.addCommentNotImplemented'));
  selectorVisible.value = false; // 关闭节点选择器
  emit('close');
};

// 暴露方法给父组件（用于关闭节点选择器）
defineExpose({
  closeSelector
});
</script>

<style scoped>
/* 右键菜单样式 - Dify风格 */
.context-menu {
  position: fixed;
  z-index: 1000;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
  padding: 4px 0;
  min-width: 160px;
  user-select: none;
}

.context-menu-item {
  display: flex;
  align-items: center;
  padding: 10px 16px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 14px;
  color: #374151;
}

.context-menu-item:hover:not(.disabled) {
  background-color: #f3f4f6;
}

.context-menu-item.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.menu-icon {
  margin-right: 10px;
  font-size: 16px;
}

.coming-soon {
  margin-left: auto;
  font-size: 12px;
  color: #9ca3af;
}

/* 节点选择器面板样式 - Dify风格 */
.node-selector {
  position: fixed;
  z-index: 1001;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  width: 420px;
  max-height: 500px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.node-selector-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #e5e7eb;
  font-size: 14px;
  font-weight: 600;
  color: #1f2937;
  background-color: #f9fafb;
}

.node-selector-body {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px 12px;
  padding: 12px;
  overflow-y: auto;
  max-height: 450px;
}

.node-selector-item {
  display: flex;
  align-items: flex-start;
  padding: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  border-left: 3px solid transparent;
  border-radius: 10px;
}

.node-selector-item:hover {
  background-color: #f3f4f6;
  border-left-color: #3b82f6;
}

.node-item-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  font-size: 18px;
  margin-right: 10px;
  flex-shrink: 0;
  color: #ffffff;
}

/* 各类型节点图标样式 */
.node-item-icon.start-icon {
  background: linear-gradient(135deg, #06b6d4 0%, #3b82f6 100%);
}

.node-item-icon.reply-icon {
  background: linear-gradient(135deg, #8b5cf6 0%, #ec4899 100%);
}

.node-item-icon.condition-icon {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.node-item-icon.forloop-icon {
  background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%);
}

.node-item-icon.assign-icon {
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
}

.node-item-icon.llm-icon {
  background: linear-gradient(135deg, #a855f7 0%, #7c3aed 100%);
}

.node-item-icon.jscode-icon {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.node-item-icon.http-icon {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
}

.node-item-icon.default-icon {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.node-item-info {
  flex: 1;
  min-width: 0;
}

.node-item-name {
  font-size: 14px;
  font-weight: 500;
  color: #1f2937;
  margin-bottom: 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.node-item-desc {
  font-size: 12px;
  color: #6b7280;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 滚动条样式 */
.node-selector-body::-webkit-scrollbar {
  width: 6px;
}

.node-selector-body::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.node-selector-body::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.node-selector-body::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
