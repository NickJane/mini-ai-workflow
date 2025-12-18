<template>
  <!-- 使用与property-group相同的结构 -->
  <div v-if="hasOutput" class="property-group">
    <div class="group-header">
      <h4 class="group-title">{{ t('flowComponents.nodeOutput') }}</h4>
      <el-button
        v-if="isEditable"
        type="primary"
        size="small"
        :icon="Plus"
        @click="handleAddOutput"
        style="height: 24px; padding: 0 10px; font-size: 12px;"
      >
        {{ t('flowComponents.addOutput') }}
      </el-button>
    </div>

    <!-- 输出列表 -->
    <div class="output-list">
      <div
        v-for="(output, index) in outputs"
        :key="index"
        class="output-item"
      >
        <!-- 属性名 -->
        <div class="output-name">
          <el-input
            v-if="isEditable"
            v-model="output.name"
            :placeholder="t('flowComponents.outputPropertyName')"
            size="small"
            maxlength="15"
            show-word-limit
            @input="handleOutputNameChange($event, index)"
            @blur="validateOutputName(index)"
            :class="{ 'is-error': outputErrors[index] }"
          />
          <span v-else>{{ output.name }}</span>
        </div>
        
        <!-- 类型显示 -->
        <div class="output-type">
          <el-select
            v-if="isEditable"
            v-model="output.variableType"
            :placeholder="t('flowComponents.outputType')"
            size="small"
            @change="handleOutputChange"
          >
            <el-option label="String" :value="VariableItemType.StringVariable" />
            <el-option label="Number" :value="VariableItemType.LongVariable" />
            <el-option label="Decimal" :value="VariableItemType.DecimalVariable" />
            <el-option label="Boolean" :value="VariableItemType.BooleanVariable" />
            <el-option label="DateTime" :value="VariableItemType.DateTimeVariable" />
            <el-option label="Object" :value="VariableItemType.ObjectVariable" />
            <el-option label="Array" :value="VariableItemType.ArrayVariable" />
          </el-select>
          <span v-else class="output-type-badge">{{ getTypeLabel(output.variableType) }}</span>
        </div>

        <!-- 删除按钮 -->
        <el-button
          v-if="isEditable"
          type="danger"
          size="small"
          :icon="Delete"
          circle
          @click="handleDeleteOutput(index)"
          style="width: 24px; height: 24px;"
        />
      </div>
      
      <!-- 错误提示 -->
      <div v-if="isEditable && outputErrors.some((e: string) => e)" class="error-tips">
        <div v-for="(error, index) in outputErrors" :key="index">
          <span v-if="error" class="error-text">{{ outputs[index]?.name || t('flowComponents.outputProperty') + (index + 1) }}: {{ error }}</span>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="outputs.length === 0" class="empty-state">
        <span class="empty-text">{{ t('flowComponents.noOutputDefined') }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { Plus, Delete } from '@element-plus/icons-vue';
import type { NodeOutputItem } from '@/types/flow-designer/NodeBase';
import { VariableItemType } from '@/types/flow-designer/Parameters/Variable';
import type { VariableItemType as VariableItemTypeEnum } from '@/types/flow-designer/Parameters/Variable';

const { t } = useI18n();

// 组件属性
interface Props {
  /** 是否有输出 */
  hasOutput: boolean;
  /** 输出列表 */
  outputs: NodeOutputItem[];
  /** 是否可编辑（LLM节点不可编辑，JSCode节点可编辑） */
  isEditable?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  isEditable: false,
  outputs: () => []
});

// 事件定义
const emit = defineEmits<{
  change: [outputs: NodeOutputItem[]];
}>();

// 输出错误信息（每个输出对应一个错误信息）
const outputErrors = ref<string[]>([]);

// 监听outputs变化，初始化errors数组
watch(() => props.outputs, (newOutputs) => {
  outputErrors.value = new Array(newOutputs.length).fill('');
}, { immediate: true });

// 获取类型标签
const getTypeLabel = (typeName: VariableItemTypeEnum): string => {
  const typeMap: Record<string, string> = {
    'StringVariable': 'String',
    'LongVariable': 'Number',
    'DecimalVariable': 'Decimal',
    'BooleanVariable': 'Boolean',
    'DateTimeVariable': 'DateTime',
    'ObjectVariable': 'Object',
    'ArrayVariable': 'Array'
  };
  return typeMap[typeName] || typeName;
};

// 添加输出
const handleAddOutput = () => {
  const newOutput: NodeOutputItem = {
    name: '',
    variableType: VariableItemType.StringVariable
  };
  // 同步添加错误信息槽位
  outputErrors.value.push('');
  emit('change', [...props.outputs, newOutput]);
};

// 删除输出
const handleDeleteOutput = (index: number) => {
  const newOutputs = [...props.outputs];
  newOutputs.splice(index, 1);
  // 同步删除错误信息
  outputErrors.value.splice(index, 1);
  emit('change', newOutputs);
};

// 输出变化
const handleOutputChange = () => {
  emit('change', [...props.outputs]);
};

// 校验变量名是否符合规范
const isValidVariableName = (name: string): boolean => {
  if (!name) return false;
  // 变量名规则：只能包含字母、数字、下划线，且不能以数字开头
  const regex = /^[a-zA-Z_][a-zA-Z0-9_]*$/;
  return regex.test(name);
};

// 检查是否有重名
const hasDuplicateName = (name: string, currentIndex: number): boolean => {
  return props.outputs.some((output, index) => 
    index !== currentIndex && output.name === name
  );
};

// 处理属性名输入
const handleOutputNameChange = (value: string, index: number) => {
  // 清除之前的错误
  outputErrors.value[index] = '';
  
  // 实时校验
  if (value && !isValidVariableName(value)) {
    outputErrors.value[index] = t('flowComponents.variableNameInvalid');
  } else if (value && hasDuplicateName(value, index)) {
    outputErrors.value[index] = t('flowComponents.outputPropertyNameDuplicate');
  }
  
  handleOutputChange();
};

// 失焦时校验
const validateOutputName = (index: number) => {
  const name = props.outputs[index]?.name;
  
  if (!name) {
    outputErrors.value[index] = t('flowComponents.outputPropertyNameRequired');
    return false;
  }
  
  if (!isValidVariableName(name)) {
    outputErrors.value[index] = t('flowComponents.variableNameInvalid');
    return false;
  }
  
  if (hasDuplicateName(name, index)) {
    outputErrors.value[index] = t('flowComponents.outputPropertyNameDuplicate');
    return false;
  }
  
  outputErrors.value[index] = '';
  return true;
};
</script>

<style scoped>
/* 使用与NodePropertiesPanel相同的样式 */
.property-group {
  margin-bottom: 16px;
}

.group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.group-title {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #374151;
  line-height: 20px;
  padding-bottom: 6px;
  border-bottom: 1px solid #f3f4f6;
  flex: 1;
}

.output-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.output-item {
  display: flex;
  align-items: center;
  gap: 12px;
}

/* 属性名区域 - 更宽 */
.output-name {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
}

.output-name span {
  width: 100%;
  font-size: 13px;
  color: #606266;
}

.output-name :deep(.el-input) {
  width: 100%;
}

.output-name :deep(.el-input__wrapper) {
  font-size: 13px;
  height: 28px;
}

.output-name :deep(.el-input__inner) {
  height: 28px;
  line-height: 28px;
}

/* 类型选择区域 - 更窄 */
.output-type {
  width: 140px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
}

.output-type-badge {
  display: inline-block;
  font-size: 12px;
  color: #059669;
  padding: 2px 8px;
  background: #d1fae5;
  border: 1px solid #6ee7b7;
  border-radius: 4px;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
}

.output-type :deep(.el-select) {
  width: 100%;
}

.output-type :deep(.el-select .el-input__wrapper) {
  height: 28px;
}

.output-type :deep(.el-input__inner) {
  font-size: 13px;
  height: 28px;
  line-height: 28px;
}

.empty-state {
  padding: 12px;
  text-align: center;
  background: #f9fafb;
  border-radius: 4px;
}

.empty-text {
  font-size: 12px;
  color: #9ca3af;
}

/* 错误提示 */
.error-tips {
  margin-top: 8px;
  padding: 8px 12px;
  background: #fef0f0;
  border: 1px solid #fde2e2;
  border-radius: 4px;
}

.error-text {
  display: block;
  font-size: 12px;
  color: #f56c6c;
  line-height: 1.6;
  margin-bottom: 4px;
}

.error-text:last-child {
  margin-bottom: 0;
}

/* 输入框错误状态 */
.output-name :deep(.el-input.is-error .el-input__wrapper) {
  border-color: #f56c6c !important;
  box-shadow: 0 0 0 1px #f56c6c inset !important;
}

.output-name :deep(.el-input.is-error .el-input__wrapper:hover) {
  border-color: #f56c6c !important;
}

.output-name :deep(.el-input.is-error .el-input__wrapper.is-focus) {
  border-color: #f56c6c !important;
  box-shadow: 0 0 0 1px #f56c6c inset !important;
}
</style>
