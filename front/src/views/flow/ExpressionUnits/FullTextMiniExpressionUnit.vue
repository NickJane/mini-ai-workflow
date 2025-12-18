<template>
  <div class="fulltext-mini-expression-unit">
    <div class="inline-editor">
      <CodeMirrorEditor
        ref="editorRef"
        v-model="localValue"
        :placeholder="resolvedPlaceholder"
        :disabled="disabled"
        :readonly="readonly"
        :rows="1"
        @change="handleInput"
      />
      <el-tooltip :content="t('flowComponents.insertVariable')" placement="top">
        <el-button
          size="small"
          text
          class="variable-btn"
          @click="showVariableSelectorDialog"
          :disabled="disabled || readonly"
        >
          {x}
        </el-button>
      </el-tooltip>
    </div>

    <VariableSelector
      v-model:visible="variableSelectorVisible"
      @select="handleVariableSelect"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import CodeMirrorEditor from '../components/CodeMirrorEditor.vue';
import VariableSelector from '../components/VariableSelector.vue';
import type { VariableItem } from '../components/VariableSelector.vue';
import type { FullTextMiniExpressionUnit } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';

interface Props {
  modelValue: FullTextMiniExpressionUnit;
  placeholder?: string;
  disabled?: boolean;
  readonly?: boolean;
}

interface Emits {
  (e: 'update:modelValue', value: FullTextMiniExpressionUnit): void;
  (e: 'change', value: FullTextMiniExpressionUnit): void;
  (e: 'blur'): void;
  (e: 'focus'): void;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '',
  disabled: false,
  readonly: false
});

const emit = defineEmits<Emits>();
const { t } = useI18n();

const editorRef = ref();
const variableSelectorVisible = ref(false);

const resolvedPlaceholder = computed(() => props.placeholder || t('flowComponents.enterText'));

const localValue = computed({
  get: () => props.modelValue.Text || '',
  set: (value: string) => {
    // handled by handleInput to keep change event consistent
  }
});

const handleInput = (value: string) => {
  const updatedValue: FullTextMiniExpressionUnit = {
    ...props.modelValue,
    Text: value
  };
  emit('update:modelValue', updatedValue);
  emit('change', updatedValue);
};

const showVariableSelectorDialog = () => {
  variableSelectorVisible.value = true;
};

const handleVariableSelect = (variable: VariableItem) => {
  if (!editorRef.value) return;
  editorRef.value.insertAtCursor(`{{${variable.key}}}`);
};
</script>

<style scoped>
.fulltext-mini-expression-unit {
  width: 100%;
}

.inline-editor {
  display: flex;
  align-items: center;
  gap: 6px;
  width: 100%;
}

.inline-editor :deep(.codemirror-editor-wrapper) {
  flex: 1;
  min-height: 32px;
}

.inline-editor :deep(.codemirror-editor),
.inline-editor :deep(.cm-editor),
.inline-editor :deep(.cm-scroller) {
  min-height: 32px;
  max-height: 36px;
}

.variable-btn {
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 13px;
  font-weight: 600;
  padding: 4px 8px;
}
</style>
