<!--
  JS表达式单元编辑器组件
  
  功能特性：
  1. 模式切换 - 支持"表达式模式"和"函数模式"两种编辑方式
  2. 表达式模式 - 直接编写JS表达式，如：{{age}} > 18
  3. 函数模式 - 编写完整的JS函数，必须包含 main() 方法
  4. 变量插入 - 点击 {x} 按钮打开对话框，选择输入参数或会话变量
  5. 全屏编辑 - 点击放大图标进入全屏模式（80vw x 80vh），点击缩小图标或遮罩退出
  6. 默认模板 - 根据模式自动提供初始模板
  7. 变量格式 - 变量以 {{variableName}} 格式插入，方便后端替换
  
  Props 参数：
  - modelValue: JSExpressionUnit - 绑定值（v-model），包含 functionCode、expressionCode 和 isFunctionMode
  - placeholder: string - 占位符文本
  - disabled: boolean - 是否禁用，默认 false
  - readonly: boolean - 是否只读，默认 false
  - rows: number - 普通模式的行数，默认 8
  - lockMode: boolean - 是否锁定模式（禁用模式切换），默认 false
  
  Events 事件：
  - update:modelValue - v-model 更新事件
  - change - 值变化事件
  
  使用示例：
  ```vue
  <script setup lang="ts">
  import { ref } from 'vue';
  import type { JSExpressionUnit } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
  import { ExpressionUnitFactory } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
  
  const expression = ref<JSExpressionUnit>(ExpressionUnitFactory.createJSExpression());
  </script>
  
  <template>
    <JSExpressionUnitEditor
      v-model="expression"
      @change="handleChange"
    />
  </template>
  ```
-->
<template>
  <!-- 全屏遮罩 -->
  <div v-if="isFullscreen" class="fullscreen-backdrop" @click="toggleFullscreen"></div>
  
  <div :class="['js-expression-unit-editor', { 'is-fullscreen': isFullscreen }]">
    <!-- 标题栏 -->
    <div class="editor-header">
      <div class="header-left">
        <span class="editor-title">{{ t('flowComponents.jsExpression') }}</span>
        <el-popover
          placement="bottom-start"
          :width="360"
          trigger="hover"
        >
          <template #reference>
            <el-icon class="help-icon"><QuestionFilled /></el-icon>
          </template>
          <div class="help-content">
            <div class="help-section">
              <div class="help-title">{{ t('flowComponents.expressionMode') }}</div>
              <div class="help-text">{{ t('flowComponents.expressionModeHelp') }}</div>
            </div>
            <div class="help-section">
              <div class="help-title">{{ t('flowComponents.functionMode') }}</div>
              <div class="help-text">{{ t('flowComponents.functionModeHelp') }}</div>
            </div>
            <div class="help-section">
              <div class="help-title">{{ t('flowComponents.variableUsage') }}</div>
              <div class="help-text">{{ t('flowComponents.variableUsageHelp') }}</div>
            </div>
          </div>
        </el-popover>
      </div>
      <div class="header-center">
        <!-- 模式切换 -->
        <el-radio-group 
          v-if="!lockMode"
          v-model="currentMode" 
          size="small"
          @change="handleModeChange"
          class="mode-switch"
        >
          <el-radio-button label="expression">{{ t('flowComponents.expressionMode') }}</el-radio-button>
          <el-radio-button label="function">{{ t('flowComponents.functionMode') }}</el-radio-button>
        </el-radio-group>
        <!-- 模式锁定时显示当前模式 -->
        <span v-else class="mode-locked">
          {{ currentMode === 'function' ? t('flowComponents.functionMode') : t('flowComponents.expressionMode') }}
        </span>
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
    
    <!-- 代码编辑区域 -->
    <div class="editor-container" :class="{ 'is-fullscreen': isFullscreen }">
      <CodeMirrorEditor
        ref="editorRef"
        v-model="localCode"
        :placeholder="currentPlaceholder"
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
import { ref, watch, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { FullScreen, Close, QuestionFilled } from '@element-plus/icons-vue';
import type { JSExpressionUnit } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';
import CodeMirrorEditor from '../components/CodeMirrorEditor.vue';
import VariableSelector from '../components/VariableSelector.vue';
import type { VariableItem } from '../components/VariableSelector.vue';

const { t } = useI18n();

// 组件属性
interface Props {
  modelValue: JSExpressionUnit;
  placeholder?: string;
  disabled?: boolean;
  readonly?: boolean;
  rows?: number;
  lockMode?: boolean; // 是否锁定模式，禁用模式切换
}

// 组件事件
interface Emits {
  (e: 'update:modelValue', value: JSExpressionUnit): void;
  (e: 'change', value: JSExpressionUnit): void;
  (e: 'blur'): void;
  (e: 'focus'): void;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '',
  disabled: false,
  readonly: false,
  rows: 8,
  lockMode: false
});

const emit = defineEmits<Emits>();


// 组件状态
const editorRef = ref();
const variableSelectorVisible = ref(false);
const isFullscreen = ref(false);

// 当前模式
const currentMode = ref<'expression' | 'function'>(
  props.modelValue.isFunctionMode ? 'function' : 'expression'
);

// 当前代码（基于模式动态获取对应字段）
const localCode = computed({
  get: () => {
    return currentMode.value === 'function' 
      ? props.modelValue.functionCode || ''
      : props.modelValue.expressionCode || '';
  },
  set: (value: string) => {
    // 通过 emitUpdate 函数来处理更新
  }
});

// 默认模板
const expressionTemplate = '';
const functionTemplate = `function main() {
  // 可以使用变量，例如：{{age}} > 18
  return true;
}`;

// 当前占位符
const currentPlaceholder = computed(() => {
  return props.placeholder || (
    currentMode.value === 'expression' 
      ? t('flowComponents.enterJSExpression')
      : t('flowComponents.enterJSFunction')
  );
});


// 初始化：如果代码为空，使用默认模板
onMounted(() => {
  const currentCode = currentMode.value === 'function' 
    ? props.modelValue.functionCode 
    : props.modelValue.expressionCode;
  
  if (!currentCode) {
    const defaultCode = currentMode.value === 'expression' ? expressionTemplate : functionTemplate;
    emitUpdate(defaultCode);
  }
});

// 监听外部值变化
watch(() => props.modelValue, (newValue) => {
  currentMode.value = newValue.isFunctionMode ? 'function' : 'expression';
}, { deep: true });

// 处理模式切换
const handleModeChange = (mode: string | number) => {
  const isFunctionMode = mode === 'function';
  
  // 获取切换后模式的代码
  const targetCode = isFunctionMode 
    ? props.modelValue.functionCode 
    : props.modelValue.expressionCode;
  
  // 如果目标模式的代码为空，填充默认模板
  const codeToUse = targetCode || (isFunctionMode ? functionTemplate : expressionTemplate);
  
  emitUpdate(codeToUse, isFunctionMode);
};

// 发送更新事件
const emitUpdate = (code: string, isFunctionMode?: boolean) => {
  const isFunction = isFunctionMode !== undefined ? isFunctionMode : currentMode.value === 'function';
  
  const updatedValue: JSExpressionUnit = {
    ...props.modelValue,
    functionCode: isFunction ? code : props.modelValue.functionCode,
    expressionCode: isFunction ? props.modelValue.expressionCode : code,
    isFunctionMode: isFunction
  };
  emit('update:modelValue', updatedValue);
  emit('change', updatedValue);
};

// 事件处理
const handleInput = (value: string) => {
  emitUpdate(value);
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
  
  // 使用CodeMirrorEditor的insertAtCursor方法
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

.js-expression-unit-editor {
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
      
      .expression-textarea {
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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
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
    
    .help-icon {
      font-size: 16px;
      color: rgba(255, 255, 255, 0.85);
      cursor: pointer;
      transition: all 0.2s ease;
      
      &:hover {
        color: #ffffff;
        transform: scale(1.1);
      }
    }
    
    .header-center {
      flex: 1;
      display: flex;
      justify-content: center;
    }
    
    // 模式锁定显示
    .mode-locked {
      font-size: 13px;
      font-weight: 500;
      color: #606266;
      padding: 4px 12px;
      background: #f5f7fa;
      border-radius: 4px;
    }
    
    // 模式切换按钮组
    .mode-switch {
      background: rgba(255, 255, 255, 0.15);
      border-radius: 6px;
      padding: 3px;
      
      :deep(.el-radio-button) {
        &:first-child .el-radio-button__inner {
          border-left: none;
        }
      }
      
      :deep(.el-radio-button__inner) {
        background-color: transparent;
        border: none;
        color: rgba(255, 255, 255, 0.85);
        font-size: 12px;
        padding: 6px 16px;
        box-shadow: none;
        transition: all 0.3s ease;
        font-weight: 500;
        
        &:hover {
          background-color: rgba(255, 255, 255, 0.1);
          color: #ffffff;
        }
      }
      
      :deep(.el-radio-button__original-radio:checked + .el-radio-button__inner) {
        background-color: rgba(255, 255, 255, 0.95);
        color: #667eea;
        font-weight: 600;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      }
      
      :deep(.el-radio-button:first-child .el-radio-button__inner) {
        border-radius: 4px;
      }
      
      :deep(.el-radio-button:last-child .el-radio-button__inner) {
        border-radius: 4px;
      }
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
    
    .expression-textarea {
      font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
      
      :deep(.el-textarea__inner) {
        font-size: 13px;
        line-height: 1.6;
        border: none;
        padding: 0;
        resize: vertical;
      }
    }
  }
}

// 帮助提示内容样式
.help-content {
  padding: 8px 0;
  
  .help-section {
    padding: 8px 0;
    
    &:not(:last-child) {
      border-bottom: 1px solid #f0f0f0;
    }
    
    .help-title {
      font-size: 13px;
      font-weight: 600;
      color: #303133;
      margin-bottom: 6px;
    }
    
    .help-text {
      font-size: 12px;
      color: #606266;
      line-height: 1.6;
    }
  }
}

</style>
