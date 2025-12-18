<!--
  文本输入单元编辑器组件
  使用 CodeMirror 6 编辑器，支持变量标签显示和全屏编辑
  
  功能特性：
  1. CodeMirror 编辑器 - 提供语法高亮和更好的编辑体验
  2. 变量标签显示 - 变量 {{variableName}} 显示为可视化的蓝色标签
  3. 变量插入 - 点击 {x} 按钮打开选择器，选择变量、参数、系统变量、节点输出等
  4. 全屏编辑 - 点击放大图标进入全屏模式（80vw x 80vh）
  5. 智能搜索 - 变量选择器支持实时搜索和分组显示
  6. 表达式单元 - 使用 FullTextExpressionUnit 存储富文本内容，支持占位符解析
  
  Props 参数：
  - modelValue: FullTextExpressionUnit - 绑定值（v-model）
  - placeholder: string - 占位符文本
  - disabled: boolean - 是否禁用，默认 false
  - readonly: boolean - 是否只读，默认 false
  - rows: number - （已废弃）仅为向后兼容保留
  - title: string - 编辑器标题，默认 "文本输入"
  
  Events 事件：
  - update:modelValue - v-model 更新事件
  - change - 值变化事件
  - blur - 失焦事件
  - focus - 获焦事件
  
  使用示例：
  ```vue
  <script setup lang="ts">
  import { ref } from 'vue';
  import type { FullTextExpressionUnit } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
  import { ExpressionUnitFactory } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
  
  const systemPrompt = ref<FullTextExpressionUnit>(ExpressionUnitFactory.createFullTextExpression());
  </script>
  
  <template>
    <TextInputUnit
      v-model="systemPrompt"
      title="System Prompt"
      placeholder="请输入系统提示词，使用{{变量名}}引用变量"
      @change="handleChange"
    />
  </template>
  ```
-->
<template>
  <!-- 全屏遮罩 -->
  <div v-if="isFullscreen" class="fullscreen-backdrop" @click="toggleFullscreen"></div>
  
  <div :class="['text-input-unit-editor', { 'is-fullscreen': isFullscreen }]">
    <!-- 标题栏 -->
    <div class="editor-header">
      <div class="header-left">
        <span class="editor-title">{{ displayTitle }}</span>
      </div>
      <div class="editor-actions">
        <el-tooltip :content="t('flowComponents.insertVariable')" placement="top">
          <el-button 
            size="small" 
            text 
            @click="showVariableSelectorDialog"
            class="action-btn variable-btn"
          >
            {x}
          </el-button>
        </el-tooltip>
        <el-tooltip :content="isFullscreen ? t('flowComponents.exitFullscreen') : t('flowComponents.fullscreenEdit')" placement="top">
          <el-button 
            size="small" 
            text 
            @click="toggleFullscreen"
            class="action-btn fullscreen-btn"
          >
            <el-icon v-if="!isFullscreen"><FullScreen /></el-icon>
            <el-icon v-else><Close /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>
    
    <!-- 编辑区域 -->
    <div class="editor-container" :class="{ 'is-fullscreen': isFullscreen }">
      <CodeMirrorEditor
        ref="editorRef"
        v-model="localValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        @change="handleInput"
      />
    </div>

    <!-- 变量选择器 -->
    <VariableSelector
      v-model:visible="variableSelectorVisible"
      @select="handleVariableSelect"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { FullScreen, Close } from '@element-plus/icons-vue';
import type { FullTextExpressionUnit } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
import CodeMirrorEditor from '../components/CodeMirrorEditor.vue';
import VariableSelector from '../components/VariableSelector.vue';
import type { VariableItem } from '../components/VariableSelector.vue';

const { t } = useI18n();

// 组件属性
interface Props {
  modelValue: FullTextExpressionUnit;
  placeholder?: string;
  disabled?: boolean;
  readonly?: boolean;
  rows?: number;
  title?: string;
}

// 组件事件
interface Emits {
  (e: 'update:modelValue', value: FullTextExpressionUnit): void;
  (e: 'change', value: FullTextExpressionUnit): void;
  (e: 'blur'): void;
  (e: 'focus'): void;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '',
  disabled: false,
  readonly: false,
  rows: 4,
  title: ''
});

const emit = defineEmits<Emits>();

// 实际显示的标题（带默认值）
const displayTitle = computed(() => props.title || t('flowComponents.textInput'));

// 组件状态
const editorRef = ref();
const variableSelectorVisible = ref(false);
const isFullscreen = ref(false);

// 本地值（操作 Text 属性）
const localValue = computed({
  get: () => props.modelValue.Text || '',
  set: (value: string) => {
    // 通过 emitUpdate 函数来处理更新
  }
});

// 事件处理：更新 Text 属性
const handleInput = (value: string) => {
  const updatedUnit: FullTextExpressionUnit = {
    ...props.modelValue,
    Text: value
  };
  emit('update:modelValue', updatedUnit);
  emit('change', updatedUnit);
};

const handleBlur = () => {
  emit('blur');
};

const handleFocus = () => {
  emit('focus');
};

// 切换全屏模式
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value;
};

// 显示变量选择器
const showVariableSelectorDialog = () => {
  variableSelectorVisible.value = true;
};

// 处理变量选择
const handleVariableSelect = (variable: VariableItem) => {
  if (!editorRef.value) return;
  
  // 使用 CodeMirrorEditor 的 insertAtCursor 方法
  editorRef.value.insertAtCursor(`{{${variable.key}}}`);
};
</script>

<style scoped lang="scss">
// 全屏遮罩
.fullscreen-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 8888;
}

.text-input-unit-editor {
  border: 1px solid #dcdfe6;
  border-radius: 8px;
  overflow: hidden;
  transition: all 0.3s ease;
  
  // 全屏状态
  &.is-fullscreen {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 80vw;
    max-width: 1200px;
    height: 80vh;
    z-index: 9999;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
    display: flex;
    flex-direction: column;
    
    .editor-container {
      flex: 1;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      
      .text-textarea {
        flex: 1;
        
        :deep(.el-textarea) {
          height: 100%;
        }
        
        :deep(.el-textarea__inner) {
          height: 100% !important;
          resize: none !important;
        }
      }
    }
  }
  
  // 标题栏
  .editor-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 10px 16px;
    background: linear-gradient(135deg, #10b981 0%, #059669 100%);
    border-bottom: none;
    flex-shrink: 0;
    gap: 12px;
    
    .header-left {
      display: flex;
      align-items: center;
      flex-shrink: 0;
      gap: 8px;
    }
    
    .editor-title {
      font-size: 14px;
      font-weight: 600;
      color: #ffffff;
      letter-spacing: 0.5px;
    }
    
    .editor-actions {
      display: flex;
      gap: 4px;
      flex-shrink: 0;
      
      .action-btn {
        color: #ffffff;
        padding: 6px;
        min-width: 32px;
        height: 32px;
        border-radius: 6px;
        transition: all 0.2s ease;
        
        &:hover {
          background: rgba(255, 255, 255, 0.2);
          transform: translateY(-1px);
        }
        
        &:active {
          transform: translateY(0);
        }
        
        .el-icon {
          font-size: 16px;
        }
      }
      
      .variable-btn {
        font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        font-size: 14px;
        font-weight: bold;
        padding: 4px 10px;
      }
    }
  }
  
  // 编辑器容器
  .editor-container {
    position: relative;
    background: #ffffff;
    padding: 12px;
    
    &.is-fullscreen {
      flex: 1;
      overflow: hidden;
      
      :deep(.codemirror-editor-wrapper) {
        height: 100%;
      }
      
      :deep(.codemirror-editor) {
        height: 100%;
      }
      
      :deep(.cm-editor) {
        height: 100%;
      }
    }
  }
}
</style>
