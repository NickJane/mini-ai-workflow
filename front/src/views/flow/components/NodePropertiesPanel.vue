<!--
  节点属性面板组件
  右侧抽屉式面板，显示选中节点的属性配置
-->
<template>
  <el-drawer
    :model-value="visible"
    :title="t('flowComponents.nodeProperties')"
    direction="rtl"
    size="680px"
    :before-close="handleClose"
    class="node-properties-drawer"
    @update:model-value="$emit('update:visible', $event)"
  >
    <div v-if="flowStore.selectedNode" class="properties-content">
      <!-- 节点基本信息 -->
      <div class="node-info-section">
        <div class="node-header">
          <div class="node-icon" :class="getNodeIconClass(flowStore.selectedNode.type)">
            <component :is="getNodeIcon(flowStore.selectedNode.type)" />
          </div>
          <div class="node-title">
            <h3>{{ getNodeDisplayName(flowStore.selectedNode.type) }}</h3>
            <p class="node-type">{{ flowStore.selectedNode.type }}</p>
          </div>
        </div>
      </div>

      <!-- 属性表单 -->
      <div class="properties-form">
        <el-form :model="editableProperties" label-position="top" class="inline-form">
          <!-- 基础属性组 -->
          <div class="property-group">
            <h4 class="group-title">{{ t('flowComponents.basicProperties') }}</h4>
            <div class="property-fields">
              <NodePropertiesPanelSingleField
                :label="t('flowComponents.displayName')"
                :value="editableProperties.displayName"
                :placeholder="t('flowComponents.enterDisplayName')"
                :description="t('flowComponents.nodeDisplayNameDesc')"
                @update:value="updateProperty('displayName', $event)"
              />
            </div>
          </div>
          <!-- 节点特有属性 -->
          <div v-if="nodeSpecificFields.length > 0" class="property-group">
            <h4 class="group-title">{{ t('flowComponents.nodeConfiguration') }}</h4>
            
            <NodePropertiesPanelSingleField
              v-for="field in nodeSpecificFields"
              :key="field.key"
              :label="field.label"
              :type="field.type"
              :value="editableProperties[field.key]"
              :placeholder="field.placeholder"
              :description="field.description"
              :options="field.options"
              :variables="field.variables"
            @update:value="updateProperty(field.key, $event)"
            @importCurl="handleFieldImportCurl(field, $event)"
            />
          </div>
        </el-form>
        
        <!-- 节点输出面板 -->
        <NodePropertiesOutPutPanel
          :has-output="editableProperties.hasOutput || false"
          :outputs="editableProperties.outputs || []"
          :is-editable="isOutputEditable"
          @change="updateProperty('outputs', $event)"
        />
      </div>
    </div>

    <!-- 未选中节点时的提示 -->
    <div v-else class="empty-state">
      <el-empty :description="t('flowComponents.selectNode')" />
    </div>

    <!-- 底部操作按钮 -->
    <template #footer>
      <div class="drawer-footer">
        <el-button @click="handleClose">{{ t('flowComponents.cancel') }}</el-button>
        <el-button type="primary" @click="handleSave">{{ t('flowComponents.save') }}</el-button>
      </div>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { ElMessage } from 'element-plus';
import { VideoPlay, ChatDotRound, Share, EditPen, MagicStick, Document, Refresh, Link } from '@element-plus/icons-vue';
import NodePropertiesPanelSingleField from './NodePropertiesPanelSingleField.vue';
import NodePropertiesOutPutPanel from './NodePropertiesOutPutPanel.vue';
import { useFlowDesignerStore } from '@/stores/flowDesigner';
import { ExpressionUnitFactory } from '@/types/flow-designer/ExpressionUnits/ExpressionUnitBase';

const { t } = useI18n();

// 属性字段配置接口
interface PropertyFieldConfig {
  key: string;
  label: string;
  type: string;
  placeholder?: string;
  description?: string;
  options?: Array<{ label: string; value: any }>;
  variables?: any[]; // 可用变量列表（条件编辑器使用）
}

// 组件属性
interface Props {
  visible: boolean;
}

// 组件事件
interface Emits {
  (e: 'update:visible', visible: boolean): void;
  (e: 'save', nodeId: string, properties: any): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

// 状态管理
const flowStore = useFlowDesignerStore();

// 可编辑的属性数据（本地副本）
const editableProperties = ref<any>({});

// 监听选中节点变化，同步属性数据
watch(() => flowStore.selectedNode, (newNode) => {
  if (newNode) {
    // 创建属性的深度拷贝，避免直接修改原始数据
    editableProperties.value = JSON.parse(JSON.stringify(newNode.properties || {}));
    // 确保基础属性存在
    if (!editableProperties.value.id) {
      editableProperties.value.id = newNode.id;
    }
    if (!editableProperties.value.displayName) {
      editableProperties.value.displayName = getNodeDisplayName(newNode.type);
    }
    
    // 特殊处理：LLM节点的记忆配置
    // 将memoryEnabled和memoryRounds合并为memoryConfig对象供UI使用
    if (newNode.type === 'LLMNode') {
      editableProperties.value.memoryConfig = {
        enabled: editableProperties.value.memoryEnabled || false,
        rounds: editableProperties.value.memoryRounds || 5
      };
    }
  } else {
    editableProperties.value = {};
  }
}, { immediate: true, deep: true });

// 获取节点显示名称
const getNodeDisplayName = (nodeType: string): string => {
  const nameMap: Record<string, string> = {
    'StartNode': '开始',
    'ReplyNode': '回复',
    'ConditionNode': '条件判断',
    'ForLoopNode': 'For 循环',
    'AssignVariableNode': '变量赋值',
    'LLMNode': 'LLM节点',
    'JSCodeNode': 'JS代码',
    'HttpNode': 'HTTP'
  };
  return nameMap[nodeType] || nodeType;
};

// 获取节点图标
const getNodeIcon = (nodeType: string) => {
  const iconMap: Record<string, any> = {
    'StartNode': VideoPlay,
    'ReplyNode': ChatDotRound,
    'ConditionNode': Share,
    'ForLoopNode': Refresh,
    'AssignVariableNode': EditPen,
    'LLMNode': MagicStick,
    'JSCodeNode': Document,
    'HttpNode': Link
  };
  return iconMap[nodeType] || VideoPlay;
};

// 获取节点图标样式类
const getNodeIconClass = (nodeType: string): string => {
  const classMap: Record<string, string> = {
    'StartNode': 'start-icon',
    'ReplyNode': 'reply-icon',
    'ConditionNode': 'condition-icon',
    'ForLoopNode': 'forloop-icon',
    'AssignVariableNode': 'assign-icon',
    'LLMNode': 'llm-icon',
    'JSCodeNode': 'jscode-icon',
    'HttpNode': 'http-icon'
  };
  return classMap[nodeType] || 'default-icon';
};

// 获取节点特有属性字段配置
const nodeSpecificFields = computed((): PropertyFieldConfig[] => {
  if (!flowStore.selectedNode) return [];
  
  const nodeType = flowStore.selectedNode.type;
  
  // 根据节点类型返回不同的属性字段配置
  switch (nodeType) {
    case 'StartNode':
      return [
        // StartNode 目前没有特有属性，可以根据需要添加
      ];
      
    case 'ReplyNode':
      return [
        {
          key: 'message',
          label: '回复内容',
          type: 'textInput',
          placeholder: '请输入回复消息内容，使用 {{变量名}} 引用变量',
          description: '回复给用户的消息内容，可以使用 {{变量名}} 引用变量'
        }
      ];
      
    case 'ConditionNode':
      return [
        {
          key: 'conditions',
          label: t('flowComponents.conditionList'),
          type: 'conditions',
          description: t('flowComponents.conditionListDesc'),
          variables: flowStore.currentInputParameters.concat(flowStore.currentSessionVariables)
        },
        {
          key: 'defaultDescription',
          label: t('flowComponents.defaultBranch'),
          type: 'text',
          placeholder: t('flowComponents.enterDefaultBranchDesc'),
          description: t('flowComponents.defaultBranchDesc')
        }
      ];
      
    case 'ForLoopNode':
      return [
        {
          key: 'loopCount',
          label: t('flowComponents.loopCount'),
          type: 'jsExpression',
          placeholder: t('flowComponents.enterLoopCount'),
          description: t('flowComponents.loopCountDesc'),
          variables: flowStore.currentInputParameters.concat(flowStore.currentSessionVariables)
        }
      ];
      
    case 'AssignVariableNode':
      return [
        {
          key: 'assignments',
          label: t('flowComponents.assignmentList'),
          type: 'assignments',
          description: t('flowComponents.assignmentListDesc'),
          variables: flowStore.currentSessionVariables
        }
      ];
      
    case 'LLMNode':
      return [
        {
          key: 'modelSelection',
          label: t('flowComponents.modelSelection'),
          type: 'modelSelector',
          placeholder: t('flowComponents.selectLLMModel'),
          description: t('flowComponents.modelSelectionDesc')
        },
        {
          key: 'temperature',
          label: t('flowComponents.temperature'),
          type: 'slider',
          description: t('flowComponents.temperatureDesc')
        },
        {
          key: 'systemPrompt',
          label: t('flowComponents.systemPrompt'),
          type: 'textInput',
          placeholder: t('flowComponents.enterSystemPrompt'),
          description: t('flowComponents.systemPromptDesc')
        },
        {
          key: 'userPrompt',
          label: t('flowComponents.userPrompt'),
          type: 'textInput',
          placeholder: t('flowComponents.enterUserPrompt'),
          description: t('flowComponents.userPromptDesc')
        },
        {
          key: 'memoryConfig',
          label: t('flowComponents.memoryConfig'),
          type: 'memoryConfig',
          description: t('flowComponents.memoryConfigDesc')
        },
        {
          key: 'enableThinking',
          label: t('flowComponents.enableThinking'),
          type: 'boolean',
          description: t('flowComponents.enableThinkingDesc')
        },
        {
          key: 'pictures',
          label: t('flowComponents.pictures'),
          type: 'jsExpression',
          placeholder: t('flowComponents.enterPictures'),
          description: t('flowComponents.picturesDesc') + '\nFormat example: [ {url: ""}, {url: ""} ]'
        }
      ];
      
    case 'JSCodeNode':
      return [
        {
          key: 'codeUnit',
          label: t('flowComponents.jsCode'),
          type: 'jsExpression',
          placeholder: t('flowComponents.enterJSCode'),
          description: t('flowComponents.jsCodeDesc')
        }
      ];

    case 'HttpNode':
      return [
        {
          key: 'method',
          label: 'HTTP Method',
          type: 'httpMethod',
          options: [
            { label: 'GET', value: 'GET' },
            { label: 'POST', value: 'POST' }
          ],
          description: '仅支持 GET / POST'
        },
        {
          key: 'url',
          label: 'URL',
          type: 'fullTextMiniExpression',
          placeholder: 'https://api.example.com',
          description: '使用表达式模式，推荐直接填写字符串或变量'
        },
        {
          key: 'headers',
          label: 'Headers',
          type: 'textInput',
          placeholder: 'Content-Type: application/json\nX-Request-Id: {{sys.RequestId}}',
          description: '使用 FullText 模式，每行一个键值对，例如：Content-Type: application/json'
        },
        {
          key: 'query',
          label: 'Query',
          type: 'textInput',
          placeholder: '?name=abc&age=18',
          description: '使用 FullText 模式，示例：?name=abc 或 name=abc&age=18'
        },
        {
          key: 'body',
          label: 'Body',
          type: 'textInput',
          placeholder: '请求体，支持变量 {{var}}',
          description: '使用富文本表达式，便于多行/模板输入'
        },
        {
          key: 'timeoutSeconds',
          label: 'Timeout (秒)',
          type: 'number',
          placeholder: '默认 5',
          description: '请求超时时间，单位秒'
        }
      ];
      
    default:
      return [];
  }
});

// 判断节点输出是否可编辑
// LLM节点的输出是固定的（不可编辑），JSCode节点的输出是用户自定义的（可编辑）
const isOutputEditable = computed(() => {
  if (!flowStore.selectedNode) return false;
  const nodeType = flowStore.selectedNode.type;
  // JSCode节点可编辑（将来实现），LLM节点不可编辑
  return nodeType === 'JSCodeNode';
});

// 更新属性值
const updateProperty = (key: string, value: any) => {
  editableProperties.value[key] = value;
};

// 处理 HTTP cURL 导入
interface ParsedCurlResult {
  method?: string;
  url?: string;
  headers?: string;
  query?: string;
  body?: string;
}

const handleHttpCurlImport = (parsed: ParsedCurlResult) => {
  if (!parsed) return;

  if (parsed.method) {
    editableProperties.value.method = parsed.method.toUpperCase();
  }
  if (parsed.url !== undefined) {
    editableProperties.value.url = ExpressionUnitFactory.createFullTextMiniExpression(parsed.url);
  }
  if (parsed.headers !== undefined) {
    editableProperties.value.headers = ExpressionUnitFactory.createFullTextExpression(parsed.headers);
  }
  if (parsed.query !== undefined) {
    editableProperties.value.query = ExpressionUnitFactory.createFullTextExpression(parsed.query);
  }
  if (parsed.body !== undefined) {
    editableProperties.value.body = ExpressionUnitFactory.createFullTextExpression(parsed.body);
  }
};

const handleFieldImportCurl = (field: PropertyFieldConfig, parsed: ParsedCurlResult) => {
  if (field.key === 'method') {
    handleHttpCurlImport(parsed);
  }
};

// 处理关闭
const handleClose = () => {
  emit('update:visible', false);
};

// 处理保存
const handleSave = () => {
  if (!flowStore.selectedNode) return;
  
  // 准备保存的属性数据
  const propertiesToSave = { ...editableProperties.value };
  
  // 特殊处理：LLM节点的记忆配置
  // 将memoryConfig对象拆分为memoryEnabled和memoryRounds
  if (flowStore.selectedNode.type === 'LLMNode' && propertiesToSave.memoryConfig) {
    propertiesToSave.memoryEnabled = propertiesToSave.memoryConfig.enabled;
    propertiesToSave.memoryRounds = propertiesToSave.memoryConfig.rounds;
    delete propertiesToSave.memoryConfig; // 删除临时的memoryConfig对象
  }
  
  // 发送保存事件，传递编辑后的属性
  emit('save', flowStore.selectedNode.id, propertiesToSave);
  
  // 成功提示由父组件FlowDesigner显示
  handleClose();
};
</script>

<style scoped>
.node-properties-drawer {
  --el-drawer-padding-primary: 0;
}

/* 抽屉头部样式 */
.node-properties-drawer :deep(.el-drawer__header) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 12px 16px;
  margin-bottom: 0;
  min-height: unset;
}

.node-properties-drawer :deep(.el-drawer__title) {
  color: white;
  font-weight: 600;
  font-size: 15px;
}

.node-properties-drawer :deep(.el-drawer__close-btn) {
  color: white;
  font-size: 18px;
  opacity: 0.8;
  transition: opacity 0.2s;
}

.node-properties-drawer :deep(.el-drawer__close-btn):hover {
  opacity: 1;
}

.properties-content {
  padding: 16px;
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* 节点信息区域 */
.node-info-section {
  margin-bottom: 12px;
  padding: 8px 16px;
  background: #f8f9fc;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
}

.node-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.node-icon {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 16px;
}

.node-icon.start-icon {
  background: linear-gradient(135deg, #06b6d4 0%, #3b82f6 100%);
}

.node-icon.reply-icon {
  background: linear-gradient(135deg, #8b5cf6 0%, #ec4899 100%);
}

.node-icon.condition-icon {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.node-icon.forloop-icon {
  background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%);
}

.node-icon.assign-icon {
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
}

.node-icon.llm-icon {
  background: linear-gradient(135deg, #a855f7 0%, #7c3aed 100%);
}

.node-icon.jscode-icon {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.node-icon.http-icon {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
}

.node-icon.default-icon {
  background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
}

.node-title h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
  line-height: 20px;
}

.node-type {
  margin: 2px 0 0 0;
  font-size: 11px;
  color: #6b7280;
  line-height: 14px;
}

/* 属性表单 */
.properties-form {
  flex: 1;
  overflow-y: auto;
}

.property-group {
  margin-bottom: 16px;
}

.property-group:last-child {
  margin-bottom: 8px;
}

.group-title {
  margin: 0 0 12px 0;
  font-size: 14px;
  font-weight: 600;
  color: #374151;
  line-height: 20px;
  padding-bottom: 6px;
  border-bottom: 1px solid #f3f4f6;
}

/* 内联表单样式 */
.inline-form {
  :deep(.property-field) {
    display: flex;
    align-items: flex-start;
    margin-bottom: 12px;
    gap: 12px;
  }
  
  :deep(.property-field:last-child) {
    margin-bottom: 6px;
  }
  
  :deep(.property-field .field-label) {
    margin-bottom: 0;
    min-width: 90px;
    width: 90px;
    flex-shrink: 0;
    line-height: 28px;
    text-align: right;
    padding-right: 6px;
    color: #606266;
    font-weight: normal;
    font-size: 13px;
  }
  
  :deep(.property-field .field-content) {
    flex: 1;
    min-width: 0;
  }
  
  :deep(.property-field .el-input),
  :deep(.property-field .el-input-number),
  :deep(.property-field .el-select) {
    width: 100% !important;
  }
  
  :deep(.property-field .el-input__inner),
  :deep(.property-field .el-input-number__inner) {
    font-size: 13px;
    height: 28px;
    line-height: 28px;
  }
  
  :deep(.property-field .el-select .el-input__wrapper) {
    height: 28px;
  }
  
  :deep(.property-field .el-select .el-input__inner) {
    font-size: 13px;
    height: 28px;
    line-height: 28px;
  }
  
  :deep(.property-field .el-textarea) {
    width: 100% !important;
  }
  
  :deep(.property-field .el-textarea__inner) {
    min-height: 52px;
    font-size: 13px;
  }
  
  :deep(.property-field .field-description) {
    margin-top: 3px;
    font-size: 11px;
    color: #909399;
    line-height: 1.3;
    max-width: 100%;
    word-wrap: break-word;
  }
  
  :deep(.property-field .el-switch) {
    margin-top: 2px;
    transform: scale(0.85);
    transform-origin: left center;
  }
}

/* 空状态 */
.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 40px 20px;
}

/* 底部操作区域 */
.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 12px 16px;
  border-top: 1px solid #e4e7ed;
  background: linear-gradient(135deg, #f8f9fa 0%, #f1f3f5 100%);
}

.drawer-footer .el-button {
  border-radius: 6px;
  padding: 6px 16px;
  font-size: 13px;
}

.drawer-footer .el-button--primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
}

.drawer-footer .el-button--primary:hover {
  background: linear-gradient(135deg, #5a6fd8 0%, #6b4190 100%);
}

/* 滚动条样式 */
.properties-form::-webkit-scrollbar {
  width: 6px;
}

.properties-form::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.properties-form::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.properties-form::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
