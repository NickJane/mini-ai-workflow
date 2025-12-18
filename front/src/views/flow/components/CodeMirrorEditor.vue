<!--
  CodeMirror编辑器组件
  
  功能：
  1. JavaScript代码高亮
  2. 将{{变量}}渲染为可视化标签
  3. 支持节点输出变量：{{nodeId.属性名}} 显示为 "节点名.属性名"
  4. 支持点击标签查看/删除
  5. 支持光标位置插入变量
-->
<template>
  <div class="codemirror-editor-wrapper">
    <div ref="editorRef" class="codemirror-editor"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onBeforeUnmount } from 'vue';
import { EditorView, Decoration, ViewPlugin, WidgetType, keymap } from '@codemirror/view';
import type { DecorationSet, ViewUpdate } from '@codemirror/view';
import { EditorState, StateEffect } from '@codemirror/state';
import { javascript } from '@codemirror/lang-javascript';
import { defaultKeymap, history, historyKeymap } from '@codemirror/commands';
import { useFlowDesignerStore } from '@/stores/flowDesigner';
import { useI18n } from 'vue-i18n'; // 添加 useI18n 导入

// Props
interface Props {
  modelValue: string;
  placeholder?: string;
  readonly?: boolean;
  disabled?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Enter your code', // 修改默认 placeholder 为英文
  readonly: false,
  disabled: false
});

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: string];
  'change': [value: string];
}>();

// Refs
const editorRef = ref<HTMLElement>();
let editorView: EditorView | null = null;

// 状态管理
const flowStore = useFlowDesignerStore();

// 辅助函数：将 nodeId.属性名 转换为 节点名.属性名
const getDisplayName = (variableName: string): string => {
  // 检查是否是节点输出格式（nodeId.属性名）
  const dotIndex = variableName.indexOf('.');
  if (dotIndex > 0) {
    const nodeId = variableName.substring(0, dotIndex);
    const outputName = variableName.substring(dotIndex + 1);
    
    // 从 flowStore 查找节点
    const nodes = flowStore.currentNodes || [];
    const node = nodes.find(n => n.id === nodeId);
    
    if (node) {
      // 使用节点的显示名称
      const nodeName = node.properties?.displayName || nodeId;
      return `${nodeName}.${outputName}`;
    }
  }
  
  // 不是节点输出格式，直接返回原名称
  return variableName;
};

// 变量标签Widget
class VariableWidget extends WidgetType {
  constructor(readonly name: string) {
    super();
  }

  eq(other: VariableWidget) {
    return other.name === this.name;
  }

  toDOM() {
    const span = document.createElement('span');
    span.className = 'cm-variable-tag';
    // 显示转换后的名称（节点名.属性名）
    span.textContent = getDisplayName(this.name);
    // 保存原始变量名（nodeId.属性名）
    span.setAttribute('data-variable', this.name);
    return span;
  }

  ignoreEvent() {
    return false;
  }
}

// 创建变量装饰器
function createVariableDecorations(state: EditorState): DecorationSet {
  const decorations: any[] = [];
  const text = state.doc.toString();
  
  // 匹配 {{variableName}} 格式
  const regex = /\{\{([^}]+)\}\}/g;
  let match;
  
  while ((match = regex.exec(text)) !== null) {
    const from = match.index;
    const to = from + match[0].length;
    const variableName = match[1];
    
    // 创建widget装饰器，替换 {{variableName}} 为可视化标签
    decorations.push(
      Decoration.replace({
        widget: new VariableWidget(variableName),
        inclusive: true
      }).range(from, to)
    );
  }
  
  return Decoration.set(decorations);
}

// 变量装饰器插件
const variablePlugin = ViewPlugin.fromClass(
  class {
    decorations: DecorationSet;

    constructor(view: EditorView) {
      this.decorations = createVariableDecorations(view.state);
    }

    update(update: ViewUpdate) {
      if (update.docChanged || update.viewportChanged) {
        this.decorations = createVariableDecorations(update.state);
      }
    }
  },
  {
    decorations: (v) => v.decorations
  }
);

// 初始化编辑器
onMounted(() => {
  if (!editorRef.value) return;

  const startState = EditorState.create({
    doc: props.modelValue || '',
    extensions: [
      javascript(), // JavaScript语法高亮
      history(), // 历史记录（撤销/重做）
      keymap.of([...defaultKeymap, ...historyKeymap]), // 键盘映射
      variablePlugin, // 变量装饰器插件
      EditorView.editable.of(!props.readonly && !props.disabled), // 可编辑性
      EditorView.updateListener.of((update) => {
        if (update.docChanged) {
          const newValue = update.state.doc.toString();
          emit('update:modelValue', newValue);
          emit('change', newValue);
        }
      }),
      EditorView.theme({
        '&': {
          fontSize: '13px',
          fontFamily: 'Consolas, Monaco, "Courier New", monospace',
          height: '100%'
        },
        '.cm-content': {
          padding: '8px',
          minHeight: '100px'
        },
        '.cm-line': {
          padding: '0 2px',
          lineHeight: '1.6'
        },
        '&.cm-focused': {
          outline: 'none'
        },
        '.cm-variable-tag': {
          display: 'inline-block',
          padding: '1px 6px',
          margin: '0 2px',
          backgroundColor: '#e3f2fd',
          color: '#1976d2',
          borderRadius: '3px',
          fontSize: '12px',
          fontWeight: '500',
          cursor: 'pointer',
          border: '1px solid #90caf9',
          userSelect: 'none'
        },
        '.cm-variable-tag:hover': {
          backgroundColor: '#bbdefb',
          borderColor: '#64b5f6'
        }
      })
    ]
  });

  editorView = new EditorView({
    state: startState,
    parent: editorRef.value
  });
});

// 监听外部值变化
watch(() => props.modelValue, (newValue) => {
  if (editorView && newValue !== editorView.state.doc.toString()) {
    editorView.dispatch({
      changes: {
        from: 0,
        to: editorView.state.doc.length,
        insert: newValue || ''
      }
    });
  }
});

// 监听readonly/disabled变化
watch(() => [props.readonly, props.disabled], ([readonly, disabled]) => {
  if (editorView) {
    editorView.dispatch({
      effects: StateEffect.reconfigure.of([
        EditorView.editable.of(!readonly && !disabled)
      ])
    });
  }
});

// 暴露方法：在光标位置插入文本
const insertAtCursor = (text: string) => {
  if (!editorView) return;
  
  const selection = editorView.state.selection.main;
  editorView.dispatch({
    changes: {
      from: selection.from,
      to: selection.to,
      insert: text
    },
    selection: {
      anchor: selection.from + text.length
    }
  });
  editorView.focus();
};

// 暴露给父组件
defineExpose({
  insertAtCursor
});

// 清理
onBeforeUnmount(() => {
  if (editorView) {
    editorView.destroy();
    editorView = null;
  }
});
</script>

<style scoped>
.codemirror-editor-wrapper {
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  overflow: hidden;
  background: #ffffff;
}

.codemirror-editor-wrapper:hover {
  border-color: #c0c4cc;
}

.codemirror-editor-wrapper:focus-within {
  border-color: #409eff;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.1);
}

.codemirror-editor :deep(.cm-editor) {
  height: 100%;
}

.codemirror-editor :deep(.cm-scroller) {
  overflow: auto;
}
</style>
