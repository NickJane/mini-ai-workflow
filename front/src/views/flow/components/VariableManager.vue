<template>
  <div class="variable-manager">
    <!-- 顶部按钮区域 -->
    <div class="variable-header">
      <div class="header-info">
        <div class="header-title">
          <el-icon :size="18" class="header-icon" :style="{ color: iconColor }">
            <component :is="icon" />
          </el-icon>
          <span class="title-text">{{ title }}</span>
          <el-tag :type="tagType" size="small" class="ml-2">{{ t('flowComponents.variableCount', { count: variables.length }) }}</el-tag>
        </div>
        <p class="header-description">{{ description }}</p>
      </div>
      <el-button 
        type="primary" 
        :icon="Plus" 
        @click="showAddDialog"
        size="small"
        :class="buttonClass"
      >
        {{ t('flowComponents.add' + (type === 'input' ? 'InputParameter' : 'SessionVariable')) }}
      </el-button>
    </div>

    <!-- 变量列表 -->
    <div class="variable-list">
      <el-empty v-if="variables.length === 0" :description="t('flowComponents.noVariables')" :image-size="80" />
      
      <div v-else class="variable-list-container">
        <div 
          v-for="variable in variables" 
          :key="variable.id || variable.name"
          class="variable-row"
          :class="cardClass"
        >
          <div class="row-left">
            <el-icon :size="16" class="variable-type-icon">
              <component :is="getVariableIcon(variable.typeName)" />
            </el-icon>
            <span class="variable-name">{{ variable.name }}</span>
            <el-tag 
              :type="getVariableTagType(variable.typeName)" 
              size="small"
              class="type-tag"
            >
              {{ getVariableTypeLabel(variable.typeName) }}
            </el-tag>
            <el-tag 
              v-if="type === 'input'"
              :type="variable.required ? 'danger' : 'info'" 
              size="mini"
              class="required-tag"
            >
              {{ variable.required ? t('flowComponents.required') : t('flowComponents.optional') }}
            </el-tag>
          </div>
          
          <div class="row-center">
            <div 
              v-if="variable.defaultValue !== undefined && variable.defaultValue !== null" 
              class="default-value"
              :title="formatDefaultValue(variable.defaultValue)"
            >
              {{ formatDefaultValue(variable.defaultValue) }}
            </div>
            <div v-else-if="hasChildren(variable)" class="children-info">
              {{ t('flowComponents.childCount', { count: getChildrenCount(variable) }) }}
            </div>
            <div v-else class="no-default">{{ t('flowComponents.noDefault') }}</div>
          </div>
          
          <div class="row-right">
            <el-tooltip :content="t('common.edit')">
              <el-button 
                type="primary" 
                :icon="Edit" 
                size="small" 
                text 
                @click="editVariable(variable)"
              />
            </el-tooltip>
            <el-tooltip :content="t('common.delete')">
              <el-button 
                type="danger" 
                :icon="Delete" 
                size="small" 
                text 
                @click="deleteVariable(variable)"
              />
            </el-tooltip>
          </div>
        </div>
      </div>
    </div>

    <!-- 添加/编辑对话框 -->
    <el-dialog 
      v-model="dialogVisible" 
      :title="isEditing ? t('flowComponents.edit' + (type === 'input' ? 'InputParameter' : 'SessionVariable')) : t('flowComponents.add' + (type === 'input' ? 'InputParameter' : 'SessionVariable'))"
      width="600px"
      :class="dialogClass"
      append-to-body
      :z-index="2000"
    >
      <el-form 
        ref="formRef" 
        :model="formData" 
        :rules="formRules" 
        label-width="80px"
        class="variable-form"
      >
        <el-form-item :label="t('flowComponents.variableName')" prop="name">
          <el-input 
            v-model="formData.name" 
            :placeholder="t('flowComponents.enterVariableName')"
            :prefix-icon="Edit"
          />
        </el-form-item>
        
        <el-form-item :label="t('flowComponents.variableType')" prop="typeName">
          <el-select 
            v-model="formData.typeName" 
            :placeholder="t('flowComponents.selectVariableType')"
            @change="onTypeChange"
            style="width: 100%"
          >
            <el-option 
              v-for="type in variableTypes" 
              :key="type.value" 
              :label="type.label" 
              :value="type.value"
            >
              <div class="option-item">
                <el-icon :size="14">
                  <component :is="type.icon" />
                </el-icon>
                <span class="ml-2">{{ type.label }}</span>
              </div>
            </el-option>
          </el-select>
        </el-form-item>
        
        <el-form-item :label="t('flowComponents.isRequired')" v-if="type === 'input'">
          <el-switch v-model="formData.required" />
        </el-form-item>
        
        <el-form-item :label="t('flowComponents.defaultValue')" v-if="showDefaultValue">
          <component 
            :is="getDefaultValueComponent()" 
            v-model="formData.defaultValue"
            :placeholder="getDefaultValuePlaceholder()"
            style="width: 100%"
          />
        </el-form-item>
        
        <!-- 对象变量的子属性配置 -->
        <el-form-item :label="t('flowComponents.childProperties')" v-if="formData.typeName === 'ObjectVariable'">
          <div class="children-manager">
            <el-button 
              type="primary" 
              :icon="Plus" 
              size="small" 
              @click="addChildProperty"
              text
            >
              {{ t('flowComponents.addProperty') }}
            </el-button>
            <div v-for="(child, index) in formData.children" :key="index" class="child-item">
              <el-input 
                v-model="child.name" 
                :placeholder="t('flowComponents.propertyName')"
                size="small"
                style="width: 120px; margin-right: 8px"
              />
              <el-select 
                v-model="child.typeName" 
                :placeholder="t('flowComponents.variableType')"
                size="small"
                style="width: 120px; margin-right: 8px"
              >
                <el-option 
                  v-for="type in simpleVariableTypes" 
                  :key="type.value" 
                  :label="type.label" 
                  :value="type.value"
                />
              </el-select>
              <el-switch v-if="props.type === 'input'" v-model="child.required" size="small" style="margin-right: 8px" />
              <el-button 
                type="danger" 
                :icon="Delete" 
                size="small" 
                text 
                @click="removeChildProperty(index)"
              />
            </div>
          </div>
        </el-form-item>
        
        <!-- 数组变量的元素类型配置 -->
        <el-form-item :label="t('flowComponents.elementType')" v-if="formData.typeName === 'ArrayVariable'">
          <el-select 
            v-model="formData.itemType" 
            :placeholder="t('flowComponents.selectElementType')"
            style="width: 100%"
          >
            <el-option 
              v-for="type in variableTypes" 
              :key="type.value" 
              :label="type.label" 
              :value="type.value"
            />
          </el-select>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="submitForm" :class="buttonClass">
          {{ isEditing ? t('flowComponents.update') : t('flowComponents.add') }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus';
import { 
  Plus, Edit, Delete, 
  Document, Clock, Switch, Coin, 
  Calendar, Collection, Grid
} from '@element-plus/icons-vue';
import type { 
  AnyVariable, 
  VariableItemType, 
  StringVariable, 
  LongVariable, 
  DecimalVariable, 
  BooleanVariable, 
  DateTimeVariable, 
  ObjectVariable, 
  ArrayVariable 
} from '@/types/flow-designer/Parameters/Variable';
import { VariableFactory } from '@/types/flow-designer/Parameters/Variable';

const { t } = useI18n();

// Props 定义
interface Props {
  /** 变量类型：'input' | 'session' */
  type: 'input' | 'session';
  /** 变量列表 */
  variables: AnyVariable[];
  /** 所有变量列表（用于唯一性检查） */
  allVariables?: AnyVariable[];
}

const props = withDefaults(defineProps<Props>(), {
  type: 'input',
  variables: () => []
});

// Emits 定义
const emit = defineEmits<{
  'update:variables': [variables: AnyVariable[]];
  'add': [variable: AnyVariable];
  'update': [variable: AnyVariable];
  'delete': [variable: AnyVariable];
}>();

// 根据类型计算样式和配置
const config = computed(() => {
  const isInput = props.type === 'input';
  return {
    title: isInput ? t('flowComponents.inputParameterManagement') : t('flowComponents.sessionVariableManagement'),
    description: isInput ? t('flowComponents.inputParameterDesc') : t('flowComponents.sessionVariableDesc'),
    icon: isInput ? Document : Collection,
    iconColor: isInput ? '#409EFF' : '#67C23A',
    tagType: isInput ? 'primary' : 'success',
    buttonClass: isInput ? 'input-button' : 'session-button',
    cardClass: isInput ? 'input-card' : 'session-card',
    dialogClass: isInput ? 'input-dialog' : 'session-dialog'
  };
});

// 解构计算属性
const { title, description, icon, iconColor, tagType, buttonClass, cardClass, dialogClass } = config.value;

// 表单相关
const dialogVisible = ref(false);
const isEditing = ref(false);
const editingIndex = ref(-1);
const originalVariableName = ref<string>('');
const formRef = ref<FormInstance>();

// 表单数据
const formData = reactive<{
  name: string;
  typeName: VariableItemType;
  required: boolean;
  defaultValue: any;
  children: Array<{ name: string; typeName: VariableItemType; required: boolean }>;
  itemType?: VariableItemType;
}>({
  name: '',
  typeName: 'StringVariable' as VariableItemType,
  required: false,
  defaultValue: undefined,
  children: [],
  itemType: undefined
});

// 表单验证规则
const formRules: FormRules = {
  name: [
    { required: true, message: t('flowComponents.enterVariableName'), trigger: 'blur' },
    { 
      validator: (rule, value, callback) => {
        if (!value) {
          callback();
          return;
        }
        
        // 检查变量名格式
        if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(value)) {
          callback(new Error(t('flowComponents.variableNameInvalid')));
          return;
        }
        
        // 检查唯一性
        const allVars = props.allVariables || [];
        const existingVar = allVars.find(v => v.name === value);
        
        // 编辑模式下，如果是当前正在编辑的变量，则允许
        if (isEditing.value && existingVar && originalVariableName.value === value) {
          callback();
          return;
        }
        
        // 检查是否重名
        if (existingVar) {
          callback(new Error(t('flowComponents.variableNameExists')));
          return;
        }
        
        callback();
      }, 
      trigger: 'blur' 
    }
  ],
  typeName: [
    { required: true, message: t('flowComponents.selectVariableType'), trigger: 'change' }
  ]
};

// 变量类型选项
const variableTypes = [
  { label: t('flowComponents.string'), value: 'StringVariable', icon: Document },
  { label: t('flowComponents.number'), value: 'LongVariable', icon: Coin },
  { label: t('flowComponents.decimal'), value: 'DecimalVariable', icon: Coin },
  { label: t('flowComponents.boolean'), value: 'BooleanVariable', icon: Switch },
  { label: t('flowComponents.dateTime'), value: 'DateTimeVariable', icon: Calendar },
  { label: t('flowComponents.object'), value: 'ObjectVariable', icon: Collection },
  { label: t('flowComponents.array'), value: 'ArrayVariable', icon: Grid }
];

// 简单类型（用于对象属性）
const simpleVariableTypes = variableTypes.filter(t => 
  !['ObjectVariable', 'ArrayVariable'].includes(t.value)
);

// 是否显示默认值输入
const showDefaultValue = computed(() => {
  return !['ObjectVariable', 'ArrayVariable'].includes(formData.typeName);
});

// 获取变量图标
const getVariableIcon = (typeName: VariableItemType) => {
  const typeMap = {
    StringVariable: Document,
    LongVariable: Coin,
    DecimalVariable: Coin,
    BooleanVariable: Switch,
    DateTimeVariable: Calendar,
    ObjectVariable: Collection,
    ArrayVariable: Grid
  };
  return typeMap[typeName] || Document;
};

// 获取变量标签类型
const getVariableTagType = (typeName: VariableItemType) => {
  const typeMap = {
    StringVariable: 'primary',
    LongVariable: 'success',
    DecimalVariable: 'success',
    BooleanVariable: 'warning',
    DateTimeVariable: 'info',
    ObjectVariable: 'danger',
    ArrayVariable: 'danger'
  };
  return typeMap[typeName] || 'primary';
};

// 获取变量类型标签
const getVariableTypeLabel = (typeName: VariableItemType) => {
  const typeMap: Record<VariableItemType, string> = {
    StringVariable: t('flowComponents.string'),
    LongVariable: t('flowComponents.number'),
    DecimalVariable: t('flowComponents.decimal'),
    BooleanVariable: t('flowComponents.boolean'),
    DateTimeVariable: t('flowComponents.dateTime'),
    ObjectVariable: t('flowComponents.object'),
    ArrayVariable: t('flowComponents.array')
  };
  return typeMap[typeName] || t('flowComponents.unknown');
};

// 格式化默认值显示
const formatDefaultValue = (value: any) => {
  if (value === null || value === undefined) return t('flowComponents.noDefault');
  
  if (typeof value === 'object') {
    return JSON.stringify(value);
  }
  
  return String(value);
};

// 检查是否有子项
const hasChildren = (variable: AnyVariable) => {
  return variable.typeName === 'ObjectVariable' || variable.typeName === 'ArrayVariable';
};

// 获取子项数量
const getChildrenCount = (variable: AnyVariable) => {
  if (variable.typeName === 'ObjectVariable' || variable.typeName === 'ArrayVariable') {
    return (variable as ObjectVariable | ArrayVariable).children?.length || 0;
  }
  return 0;
};

// 获取默认值输入组件
const getDefaultValueComponent = () => {
  switch (formData.typeName) {
    case 'StringVariable':
      return 'el-input';
    case 'LongVariable':
    case 'DecimalVariable':
      return 'el-input-number';
    case 'BooleanVariable':
      return 'el-switch';
    case 'DateTimeVariable':
      return 'el-date-picker';
    default:
      return 'el-input';
  }
};

// 获取默认值占位符
const getDefaultValuePlaceholder = () => {
  switch (formData.typeName) {
    case 'StringVariable':
      return t('flowComponents.enterDefaultStringValue');
    case 'LongVariable':
      return t('flowComponents.enterDefaultNumberValue');
    case 'DecimalVariable':
      return t('flowComponents.enterDefaultDecimalValue');
    case 'DateTimeVariable':
      return t('flowComponents.selectDefaultDateTime');
    default:
      return t('flowComponents.enterDefaultValue');
  }
};

// 显示添加对话框
const showAddDialog = () => {
  isEditing.value = false;
  editingIndex.value = -1;
  resetForm();
  dialogVisible.value = true;
};

// 编辑变量
const editVariable = (variable: AnyVariable) => {
  isEditing.value = true;
  editingIndex.value = props.variables.findIndex(v => v.name === variable.name);
  
  // 存储原始变量名，用于验证时判断是否是同一个变量
  originalVariableName.value = variable.name;
  
  // 填充表单数据
  formData.name = variable.name;
  formData.typeName = variable.typeName;
  formData.required = variable.required || false;
  formData.defaultValue = variable.defaultValue;
  
  if (variable.typeName === 'ObjectVariable') {
    formData.children = (variable as ObjectVariable).children?.map(child => ({
      name: child.name,
      typeName: child.typeName,
      required: child.required || false
    })) || [];
  } else if (variable.typeName === 'ArrayVariable') {
    formData.itemType = (variable as ArrayVariable).itemType;
    formData.children = (variable as ArrayVariable).children?.map(child => ({
      name: child.name,
      typeName: child.typeName,
      required: child.required || false
    })) || [];
  } else {
    formData.children = [];
  }
  
  dialogVisible.value = true;
};

// 删除变量
const deleteVariable = async (variable: AnyVariable) => {
  try {
    await ElMessageBox.confirm(
      t('flowComponents.deleteConfirm', { name: variable.name }),
      t('flowComponents.deleteConfirmTitle'),
      {
        confirmButtonText: t('common.confirm'),
        cancelButtonText: t('common.cancel'),
        type: 'warning'
      }
    );
    
    emit('delete', variable);
    ElMessage.success(t('flowComponents.deleteSuccess'));
  } catch {
    // 用户取消删除
  }
};

// 类型变化处理
const onTypeChange = () => {
  formData.defaultValue = undefined;
  formData.children = [];
  formData.itemType = undefined;
};

// 添加子属性
const addChildProperty = () => {
  formData.children.push({
    name: '',
    typeName: 'StringVariable' as VariableItemType,
    required: false
  });
};

// 删除子属性
const removeChildProperty = (index: number) => {
  formData.children.splice(index, 1);
};

// 重置表单
const resetForm = () => {
  formData.name = '';
  formData.typeName = 'StringVariable' as VariableItemType;
  formData.required = false;
  formData.defaultValue = undefined;
  formData.children = [];
  formData.itemType = undefined;
  originalVariableName.value = '';
  formRef.value?.resetFields();
};

// 提交表单
const submitForm = async () => {
  if (!formRef.value) return;
  
  await formRef.value.validate();
  
  // 创建变量对象
  let variable: AnyVariable;
  
  switch (formData.typeName) {
    case 'StringVariable':
      variable = VariableFactory.createStringVariable(
        formData.name, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'LongVariable':
      variable = VariableFactory.createLongVariable(
        formData.name, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'DecimalVariable':
      variable = VariableFactory.createDecimalVariable(
        formData.name, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'BooleanVariable':
      variable = VariableFactory.createBooleanVariable(
        formData.name, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'DateTimeVariable':
      variable = VariableFactory.createDateTimeVariable(
        formData.name, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'ObjectVariable':
      // 将子属性转换为变量对象
      const children = formData.children.map(child => {
        switch (child.typeName) {
          case 'StringVariable':
            return VariableFactory.createStringVariable(child.name, child.required);
          case 'LongVariable':
            return VariableFactory.createLongVariable(child.name, child.required);
          case 'DecimalVariable':
            return VariableFactory.createDecimalVariable(child.name, child.required);
          case 'BooleanVariable':
            return VariableFactory.createBooleanVariable(child.name, child.required);
          case 'DateTimeVariable':
            return VariableFactory.createDateTimeVariable(child.name, child.required);
          default:
            return VariableFactory.createStringVariable(child.name, child.required);
        }
      });
      
      variable = VariableFactory.createObjectVariable(
        formData.name, 
        children, 
        formData.required, 
        formData.defaultValue
      );
      break;
      
    case 'ArrayVariable':
      variable = VariableFactory.createArrayVariable(
        formData.name, 
        formData.itemType || 'StringVariable' as VariableItemType, 
        formData.required, 
        formData.children.map(child => {
          switch (child.typeName) {
            case 'StringVariable':
              return VariableFactory.createStringVariable(child.name, child.required);
            case 'LongVariable':
              return VariableFactory.createLongVariable(child.name, child.required);
            case 'DecimalVariable':
              return VariableFactory.createDecimalVariable(child.name, child.required);
            case 'BooleanVariable':
              return VariableFactory.createBooleanVariable(child.name, child.required);
            case 'DateTimeVariable':
              return VariableFactory.createDateTimeVariable(child.name, child.required);
            default:
              return VariableFactory.createStringVariable(child.name, child.required);
          }
        }), 
        formData.defaultValue
      );
      break;
      
    default:
      variable = VariableFactory.createStringVariable(formData.name, formData.required);
  }
  
  if (isEditing.value) {
    emit('update', variable);
    ElMessage.success(t('flowComponents.updateSuccess'));
  } else {
    emit('add', variable);
    ElMessage.success(t('flowComponents.addSuccess'));
  }
  
  dialogVisible.value = false;
  resetForm();
};
</script>

<style scoped lang="scss">
.variable-manager {
  .variable-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    padding: 16px;
    background: #fafafa;
    border-radius: 8px;
    margin-bottom: 16px;
    
    .header-info {
      flex: 1;
      
      .header-title {
        display: flex;
        align-items: center;
        font-size: 16px;
        font-weight: 600;
        color: #303133;
        margin-bottom: 8px;
        
        .header-icon {
          margin-right: 8px;
          transition: transform 0.2s;
        }
        
        .title-text {
          margin-right: 8px;
        }
      }
      
      .header-description {
        font-size: 13px;
        color: #909399;
        margin: 0;
        line-height: 1.4;
      }
    }
  }
  
  .variable-list {
    .variable-list-container {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    
    .variable-row {
      display: flex;
      align-items: center;
      padding: 12px 16px;
      border: 1px solid #e4e7ed;
      border-radius: 6px;
      background: #fff;
      transition: all 0.2s;
      min-height: 50px;
      
      &:hover {
        border-color: #c0c4cc;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
      }
      
      &.input-card {
        border-left: 3px solid #409EFF;
        
        &:hover {
          border-left-color: #337ecc;
        }
      }
      
      &.session-card {
        border-left: 3px solid #67C23A;
        
        &:hover {
          border-left-color: #529b2e;
        }
      }
      
      .row-left {
        display: flex;
        align-items: center;
        flex: 1;
        min-width: 0;
        
        .variable-type-icon {
          margin-right: 8px;
          color: #909399;
          flex-shrink: 0;
        }
        
        .variable-name {
          color: #303133;
          margin-right: 8px;
          font-size: 14px;
          flex-shrink: 0;
        }
        
        .type-tag {
          margin-right: 6px;
          flex-shrink: 0;
        }
        
        .required-tag {
          flex-shrink: 0;
        }
      }
      
      .row-center {
        flex: 1;
        padding: 0 16px;
        min-width: 0;
        
        .default-value {
          color: #606266;
          font-size: 13px;
          white-space: nowrap;
          overflow: hidden;
          text-overflow: ellipsis;
          max-width: 150px;
        }
        
        .children-info {
          color: #909399;
          font-size: 13px;
        }
        
        .no-default {
          color: #c0c4cc;
          font-size: 13px;
        }
      }
      
      .row-right {
        display: flex;
        align-items: center;
        gap: 4px;
        flex-shrink: 0;
      }
    }
  }
}

// 按钮样式区分
.input-button {
  background: linear-gradient(135deg, #409EFF, #337ecc);
  border-color: #409EFF;
  
  &:hover {
    background: linear-gradient(135deg, #337ecc, #2a6aa0);
  }
}

.session-button {
  background: linear-gradient(135deg, #67C23A, #529b2e);
  border-color: #67C23A;
  
  &:hover {
    background: linear-gradient(135deg, #529b2e, #467725);
  }
}

// 对话框样式（全局样式，因为dialog挂载到body）

.variable-form {
  .option-item {
    display: flex;
    align-items: center;
  }
  
  .children-manager {
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    padding: 12px;
    background: #fafafa;
    
    .child-item {
      display: flex;
      align-items: center;
      margin-top: 8px;
      
      &:first-child {
        margin-top: 8px;
      }
    }
  }
}

.ml-2 {
  margin-left: 8px;
}
</style>
