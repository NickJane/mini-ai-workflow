<!--
  VariableSelector - 变量选择器组件
  
  作用：
  通用的变量选择对话框组件，用于在表达式编辑器中选择变量、参数、系统变量和节点输出。
  样式参考 Dify，支持搜索、分组显示和图标标识。
  
  支持的变量类型：
  - 节点输出：其他节点的输出变量
  - 输入参数：流程的输入参数
  - 会话变量：流程中定义的变量
  - 系统变量：系统预定义的变量
  
  使用方式：
  ```vue
  <VariableSelector
    v-model:visible="selectorVisible"
    @select="handleVariableSelect"
  />
  ```
-->
<template>
  <el-dialog
    v-model="visible"
    :title="t('flowComponents.selectVariable')"
    width="520px"
    :close-on-click-modal="false"
    class="variable-selector-dialog"
  >
    <!-- 搜索框 -->
    <div class="search-container">
      <el-input
        v-model="searchKeyword"
        :placeholder="t('flowComponents.searchVariable')"
        :prefix-icon="Search"
        clearable
        class="search-input"
      />
    </div>

    <!-- 变量列表 -->
    <div class="variable-list">
      <!-- 节点输出（两级结构） -->
      <div v-if="filteredNodeOutputGroups.length > 0" class="variable-group">
        <div class="group-title">{{ t('flowComponents.nodeOutput') }}</div>
        <!-- 节点分组 -->
        <div v-for="nodeGroup in filteredNodeOutputGroups" :key="nodeGroup.nodeId" class="node-output-group">
          <!-- 节点名称（不可选） -->
          <div class="node-group-title">
            <span class="node-icon-small">■</span>
            <span class="node-name-text">{{ nodeGroup.nodeName }}</span>
          </div>
          <!-- 节点输出属性（可选） -->
          <div
            v-for="output in nodeGroup.outputs"
            :key="output.key"
            class="variable-item variable-item-nested"
            @click="handleSelect(output)"
          >
            <div class="variable-info">
              <span class="variable-icon node-icon">{x}</span>
              <span class="variable-name">{{ output.outputName }}</span>
            </div>
            <span class="variable-type">{{ output.type }}</span>
          </div>
        </div>
      </div>

      <!-- 输入参数 -->
      <div v-if="filteredInputParameters.length > 0" class="variable-group">
        <div class="group-title">{{ t('flowComponents.inputParameters') }}</div>
        <div
          v-for="variable in filteredInputParameters"
          :key="variable.id || variable.name"
          class="variable-item"
          @click="handleSelect({ key: variable.name, label: variable.name, type: getVariableTypeLabel(variable.typeName), category: 'input' })"
        >
          <div class="variable-info">
            <span class="variable-icon input-icon">{x}</span>
            <span class="variable-name">{{ variable.name }}</span>
          </div>
          <span class="variable-type">{{ getVariableTypeLabel(variable.typeName) }}</span>
        </div>
      </div>

      <!-- 会话变量 -->
      <div v-if="filteredSessionVariables.length > 0" class="variable-group">
        <div class="group-title">{{ t('flowComponents.sessionVariables') }}</div>
        <div
          v-for="variable in filteredSessionVariables"
          :key="variable.id || variable.name"
          class="variable-item"
          @click="handleSelect({ key: variable.name, label: variable.name, type: getVariableTypeLabel(variable.typeName), category: 'session' })"
        >
          <div class="variable-info">
            <span class="variable-icon session-icon">↗</span>
            <span class="variable-name">{{ variable.name }}</span>
          </div>
          <span class="variable-type">{{ getVariableTypeLabel(variable.typeName) }}</span>
        </div>
      </div>

      <!-- 系统变量 -->
      <div v-if="filteredSystemVariables.length > 0" class="variable-group">
        <div class="group-title">{{ t('flowComponents.systemVariables') }}</div>
        <div
          v-for="variable in filteredSystemVariables"
          :key="variable.key"
          class="variable-item"
          @click="handleSelect(variable)"
        >
          <div class="variable-info">
            <span class="variable-icon system-icon">⊙</span>
            <span class="variable-name">{{ variable.label }}</span>
          </div>
          <span class="variable-type">{{ variable.type }}</span>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="isEmpty" class="empty-state">
        <span class="empty-icon">🔍</span>
        <p class="empty-text">{{ t('flowComponents.noVariableFound') }}</p>
      </div>
    </div>

  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { Search } from '@element-plus/icons-vue';
import { useFlowDesignerStore } from '@/stores/flowDesigner';
import type { AnyVariable, VariableItemType } from '@/types/flow-designer/Parameters/Variable';

const { t } = useI18n();

// 变量项接口
export interface VariableItem {
  key: string;        // 变量唯一标识
  label: string;      // 显示名称
  type: string;       // 类型标签（String、Number等）
  category: 'node' | 'input' | 'session' | 'system'; // 分类
}

// Props
interface Props {
  visible: boolean;
}

const props = defineProps<Props>();

// Emits
const emit = defineEmits<{
  'update:visible': [value: boolean];
  'select': [variable: VariableItem];
}>();

// 状态管理
const flowStore = useFlowDesignerStore();

// 搜索关键词
const searchKeyword = ref('');

// 双向绑定 visible
const visible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value)
});

// 获取变量数据
const inputParameters = computed(() => flowStore.currentInputParameters || []);
const sessionVariables = computed(() => flowStore.currentSessionVariables || []);

// 系统变量（预定义）
const systemVariables = ref<VariableItem[]>([
  { key: 'sys.query', label: 'sys.query', type: 'String', category: 'system' },
  { key: 'sys.user', label: 'sys.user', type: 'String', category: 'system' },
  { key: 'sys.flowId', label: 'sys.flowId', type: 'String', category: 'system' },
  { key: 'sys.flowInstanceId', label: 'sys.flowInstanceId', type: 'String', category: 'system' },
  { key: 'sys.dialogueCount', label: 'sys.dialogueCount', type: 'Number', category: 'system' },
  { key: 'sys.conversationId', label: 'sys.conversationId', type: 'String', category: 'system' },
  { key: 'sys.files', label: 'sys.files', type: 'Array', category: 'system' }
]);

// 节点输出分组接口
interface NodeOutputGroup {
  nodeId: string;
  nodeName: string;
  outputs: {
    key: string;         // nodeId.属性名
    label: string;       // 显示名称（节点名.属性名）
    outputName: string;  // 属性名
    type: string;        // 类型标签
    category: 'node';
  }[];
}

// 节点输出分组（按节点分组）
const nodeOutputGroups = computed<NodeOutputGroup[]>(() => {
  const groups: NodeOutputGroup[] = [];
  const nodes = flowStore.currentNodes || [];
  
  nodes.forEach(node => {
    // 检查节点是否有输出定义
    if (node.properties?.outputs && Array.isArray(node.properties.outputs) && node.properties.outputs.length > 0) {
      const nodeName = node.properties?.displayName || node.id;
      const outputs = node.properties.outputs.map((output: any) => ({
        key: `${node.id}.${output.name}`,           // 代码格式：nodeId.属性名
        label: `${nodeName}.${output.name}`,        // 显示格式：节点名.属性名
        outputName: output.name,                     // 属性名
        type: getVariableTypeLabel(output.variableType), // 输出类型
        category: 'node' as const
      }));
      
      groups.push({
        nodeId: node.id,
        nodeName,
        outputs
      });
    }
  });
  
  return groups;
});

// 过滤函数
const filterByKeyword = (items: any[], keyword: string) => {
  if (!keyword) return items;
  const lowerKeyword = keyword.toLowerCase();
  return items.filter(item => {
    const name = item.name || item.label || item.key;
    return name.toLowerCase().includes(lowerKeyword);
  });
};

// 过滤后的节点输出分组
const filteredNodeOutputGroups = computed(() => {
  if (!searchKeyword.value) return nodeOutputGroups.value;
  
  const keyword = searchKeyword.value.toLowerCase();
  return nodeOutputGroups.value
    .map(group => {
      // 过滤输出属性
      const filteredOutputs = group.outputs.filter(output => 
        output.outputName.toLowerCase().includes(keyword) ||
        group.nodeName.toLowerCase().includes(keyword)
      );
      
      if (filteredOutputs.length > 0) {
        return {
          ...group,
          outputs: filteredOutputs
        };
      }
      return null;
    })
    .filter(group => group !== null) as NodeOutputGroup[];
});

const filteredInputParameters = computed(() => filterByKeyword(inputParameters.value, searchKeyword.value));
const filteredSessionVariables = computed(() => filterByKeyword(sessionVariables.value, searchKeyword.value));
const filteredSystemVariables = computed(() => filterByKeyword(systemVariables.value, searchKeyword.value));

// 判断是否为空
const isEmpty = computed(() => {
  return filteredNodeOutputGroups.value.length === 0 &&
         filteredInputParameters.value.length === 0 &&
         filteredSessionVariables.value.length === 0 &&
         filteredSystemVariables.value.length === 0;
});

// 获取变量类型标签
const getVariableTypeLabel = (typeName: VariableItemType): string => {
  const typeMap: Record<VariableItemType, string> = {
    'LongVariable': t('flowComponents.number'),
    'DecimalVariable': t('flowComponents.number'),
    'StringVariable': t('flowComponents.string'),
    'ObjectVariable': t('flowComponents.object'),
    'DateTimeVariable': t('flowComponents.dateTime'),
    'BooleanVariable': t('flowComponents.boolean'),
    'ArrayVariable': t('flowComponents.array')
  };
  return typeMap[typeName] || typeName;
};

// 处理选择
const handleSelect = (variable: VariableItem) => {
  emit('select', variable);
  handleClose();
};

// 关闭对话框
const handleClose = () => {
  visible.value = false;
  searchKeyword.value = '';
};

// 监听对话框打开，重置搜索
watch(visible, (newValue) => {
  if (newValue) {
    searchKeyword.value = '';
  }
});
</script>

<style scoped lang="scss">
.variable-selector-dialog {
  :deep(.el-dialog__header) {
    padding: 16px 20px;
    border-bottom: 1px solid #e5e7eb;
    
    .el-dialog__title {
      font-size: 16px;
      font-weight: 600;
      color: #111827;
    }
  }
  
  :deep(.el-dialog__body) {
    padding: 0;
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }
}

.search-container {
  padding: 16px 20px;
  border-bottom: 1px solid #f3f4f6;
  
  .search-input {
    :deep(.el-input__wrapper) {
      background: #f9fafb;
      box-shadow: none;
      border: 1px solid #e5e7eb;
      
      &:hover {
        border-color: #d1d5db;
      }
      
      &.is-focus {
        border-color: #2563eb;
        background: #ffffff;
      }
    }
  }
}

.variable-list {
  max-height: 500px;
  overflow-y: auto;
  padding: 12px 0;
  
  .variable-group {
    margin-bottom: 16px;
    
    &:last-child {
      margin-bottom: 0;
    }
    
    .group-title {
      font-size: 12px;
      font-weight: 600;
      color: #6b7280;
      text-transform: uppercase;
      padding: 8px 20px 8px 20px;
      letter-spacing: 0.5px;
    }
    
    // 节点输出分组容器
    .node-output-group {
      margin-bottom: 12px;
      
      &:last-child {
        margin-bottom: 0;
      }
      
      // 节点名称标题（不可选）
      .node-group-title {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 8px 20px;
        background: #f9fafb;
        border-left: 3px solid #2563eb;
        
        .node-icon-small {
          color: #2563eb;
          font-size: 10px;
        }
        
        .node-name-text {
          font-size: 13px;
          font-weight: 600;
          color: #374151;
        }
      }
    }
    
    .variable-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 10px 20px;
      cursor: pointer;
      transition: all 0.2s ease;
      
      &:hover {
        background: #f9fafb;
      }
      
      &:active {
        background: #f3f4f6;
      }
      
      // 嵌套的变量项（节点输出属性）
      &.variable-item-nested {
        padding-left: 48px; // 增加左侧缩进
        background: #ffffff;
        
        &:hover {
          background: #f3f4f6;
        }
      }
      
      .variable-info {
        display: flex;
        align-items: center;
        gap: 10px;
        flex: 1;
        min-width: 0;
      }
      
      .variable-icon {
        flex-shrink: 0;
        width: 20px;
        height: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 12px;
        font-weight: 600;
        border-radius: 4px;
        
        &.node-icon {
          color: #2563eb;
          background: #dbeafe;
          border: 1px solid #bfdbfe;
        }
        
        &.input-icon {
          color: #7c3aed;
          background: #ede9fe;
          border: 1px solid #ddd6fe;
        }
        
        &.session-icon {
          color: #0891b2;
          background: #cffafe;
          border: 1px solid #a5f3fc;
        }
        
        &.system-icon {
          color: #dc2626;
          background: #fee2e2;
          border: 1px solid #fecaca;
        }
      }
      
      .variable-name {
        flex: 1;
        font-size: 13px;
        font-weight: 500;
        color: #111827;
        font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
      
      .variable-type {
        flex-shrink: 0;
        font-size: 12px;
        color: #9ca3af;
        font-weight: 400;
        margin-left: 12px;
      }
    }
  }
  
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 20px;
    
    .empty-icon {
      font-size: 48px;
      margin-bottom: 12px;
      opacity: 0.5;
    }
    
    .empty-text {
      font-size: 14px;
      color: #9ca3af;
      margin: 0;
    }
  }
}

// 滚动条样式
.variable-list::-webkit-scrollbar {
  width: 6px;
}

.variable-list::-webkit-scrollbar-track {
  background: transparent;
}

.variable-list::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 3px;
  
  &:hover {
    background: #9ca3af;
  }
}
</style>
