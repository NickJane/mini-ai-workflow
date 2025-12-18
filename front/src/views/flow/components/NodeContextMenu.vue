<!--
  节点/连线右键菜单组件
  提供删除、复制等操作
-->
<template>
  <div 
    v-show="visible" 
    class="node-context-menu"
    :style="{ left: position.x + 'px', top: position.y + 'px' }"
    @click.stop
  >
    <div class="context-menu-item" @click="handleDelete">
      <el-icon class="menu-icon"><Delete /></el-icon>
      <span>{{ t('flowComponents.delete') }}{{ targetType === 'node' ? t('flowComponents.node') : t('flowComponents.edge') }}</span>
    </div>
    <div class="context-menu-item disabled" @click="handleCopy">
      <el-icon class="menu-icon"><DocumentCopy /></el-icon>
      <span>{{ t('flowComponents.copy') }}</span>
      <span class="coming-soon">({{ t('flowComponents.comingSoon') }})</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Delete, DocumentCopy } from '@element-plus/icons-vue';

const { t } = useI18n();

// 定义Props
interface Props {
  visible: boolean;
  position: { x: number; y: number };
  targetType: 'node' | 'edge' | null;
  targetId: string | null;
}

const props = defineProps<Props>();

// 定义Emits
const emit = defineEmits<{
  close: [];
  delete: [targetType: 'node' | 'edge', targetId: string];
  copy: [targetType: 'node' | 'edge', targetId: string];
}>();

// 删除节点/连线
const handleDelete = () => {
  if (props.targetType && props.targetId) {
    emit('delete', props.targetType, props.targetId);
  }
  emit('close');
};

// 复制节点（待实现）
const handleCopy = () => {
  // 待实现
  emit('close');
};
</script>

<style scoped>
/* 节点右键菜单样式 */
.node-context-menu {
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
</style>
