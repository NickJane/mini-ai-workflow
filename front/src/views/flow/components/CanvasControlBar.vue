<!--
  画布底部控制栏组件
  包含添加节点按钮和缩放控制按钮
-->
<template>
  <div class="canvas-controls">
    <!-- 添加节点按钮 -->
    <el-button 
      class="add-node-btn" 
      :icon="Plus" 
      @click="handleAddNode"
      :title="t('flowComponents.addNode')"
    />
    
    <!-- 缩放控制按钮组 -->
    <div class="zoom-controls">
      <el-button-group class="zoom-button-group">
        <el-button @click="handleZoomOut" :icon="ZoomOut" :title="t('flowComponents.zoomOut')" />
        <el-button class="zoom-display" disabled>
          <span class="zoom-text">{{ zoomPercent }}%</span>
        </el-button>
        <el-button @click="handleResetZoom" :icon="FullScreen" :title="t('flowComponents.fitCanvas')" />
        <el-button @click="handleZoomIn" :icon="ZoomIn" :title="t('flowComponents.zoomIn')" />
      </el-button-group>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ZoomIn, ZoomOut, FullScreen, Plus } from '@element-plus/icons-vue';
import { useI18n } from 'vue-i18n';

// 定义 Props
interface Props {
  /** 当前缩放百分比 */
  zoomPercent?: number;
}

const props = withDefaults(defineProps<Props>(), {
  zoomPercent: 100
});

// 定义 Emits
const emit = defineEmits<{
  addNode: [];
  zoomIn: [];
  zoomOut: [];
  resetZoom: [];
}>();

const { t } = useI18n();

// 添加节点
const handleAddNode = () => {
  emit('addNode');
};

// 放大
const handleZoomIn = () => {
  emit('zoomIn');
};

// 缩小
const handleZoomOut = () => {
  emit('zoomOut');
};

// 重置缩放
const handleResetZoom = () => {
  emit('resetZoom');
};
</script>

<style scoped>
/* 画布控制按钮 - 底部居中样式 */
.canvas-controls {
  position: fixed;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1000;
  pointer-events: none;
  display: flex;
  align-items: center;
  gap: 12px;
}

/* 添加节点按钮 */
.add-node-btn {
  pointer-events: all;
  background: #ffffff;
  border: none;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.12);
  color: #409eff;
  padding: 10px 14px;
  min-width: 42px;
  transition: all 0.2s ease;
}

.add-node-btn:hover {
  background: #ecf5ff;
  color: #409eff;
  border-color: transparent;
}

.add-node-btn:active {
  background: #d9ecff;
}

/* 缩放按钮组 */
.zoom-button-group {
  pointer-events: all;
  background: #ffffff;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.12);
  overflow: hidden;
  display: flex;
}

.zoom-button-group :deep(.el-button) {
  border: none;
  background: #ffffff;
  color: #606266;
  padding: 10px 14px;
  min-width: 42px;
  transition: all 0.2s ease;
  border-radius: 0;
}

.zoom-button-group :deep(.el-button:first-child) {
  border-radius: 8px 0 0 8px;
}

.zoom-button-group :deep(.el-button:last-child) {
  border-radius: 0 8px 8px 0;
}

.zoom-button-group :deep(.el-button:hover) {
  background: #f5f7fa;
  color: #409eff;
}

.zoom-button-group :deep(.el-button:active) {
  background: #e8ecf1;
}

.zoom-button-group :deep(.el-button + .el-button) {
  border-left: 1px solid #e4e7ed;
}

/* 缩放百分比显示 */
.zoom-button-group :deep(.zoom-display) {
  cursor: default;
  min-width: 60px;
  padding: 10px 8px;
}

.zoom-button-group :deep(.zoom-display.is-disabled) {
  background: #ffffff;
  color: #606266;
  opacity: 1;
}

.zoom-button-group :deep(.zoom-display.is-disabled:hover) {
  background: #ffffff;
  color: #606266;
}

.zoom-text {
  font-size: 13px;
  font-weight: 500;
  font-family: 'Monaco', 'Courier New', monospace;
}
</style>
