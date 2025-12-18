<!--
  大模型提供者管理页面
  用于管理不同平台（如阿里云、OpenAI等）的大模型配置信息
-->
<template>
  <div class="llm-provider-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <h2>{{ t('llmProviderMgmt.title') }}</h2>
      <el-button type="primary" :icon="Plus" @click="createProvider">{{ t('llmProviderMgmt.add') }}</el-button>
    </div>

    <!-- 提供者列表 - 表格形式 -->
    <el-table 
      :data="providerList" 
      v-loading="loading"
      stripe
      style="width: 100%"
    >
      <el-table-column prop="id" :label="t('llmProviderMgmt.id')" width="150" />
      <el-table-column prop="platformName" :label="t('llmProviderMgmt.platformName')" width="80" />
      <el-table-column :label="t('llmProviderMgmt.modelNames')" min-width="300">
        <template #default="{ row }">
          <el-tag 
            v-for="(name, index) in row.llmNames" 
            :key="index" 
            size="small"
            style="margin-right: 6px; margin-bottom: 4px;"
          >
            {{ name }}
          </el-tag>
          <span v-if="!row.llmNames || row.llmNames.length === 0" style="color: #909399;">{{ t('llmProviderMgmt.notSet') }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="llmapiUrl" :label="t('llmProviderMgmt.apiUrl')" min-width="200" show-overflow-tooltip />
      <el-table-column :label="t('llmProviderMgmt.apiKey')" width="200">
        <template #default="{ row }">
          <div style="display: flex; align-items: center; gap: 8px;">
            <span class="api-key-masked">{{ maskApiKey(row.llmapiKey) }}</span>
            <el-button 
              text 
              type="primary" 
              :icon="CopyDocument" 
              size="small"
              @click="copyApiKey(row.llmapiKey)"
              :title="t('llmProviderMgmt.copyApiKey')"
            />
          </div>
        </template>
      </el-table-column>
      <el-table-column :label="t('llmProviderMgmt.operation')" width="230" fixed="right">
        <template #default="{ row }">
          <el-button text type="primary" :icon="Edit" @click="editProvider(row)">{{ t('llmProviderMgmt.edit') }}</el-button>
          <el-button text type="danger" :icon="Delete" @click="deleteProvider(row)">{{ t('llmProviderMgmt.delete') }}</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 空状态 -->
    <el-empty 
      v-if="providerList.length === 0 && !loading" 
      :description="t('llmProviderMgmt.emptyState')"
      class="empty-state"
    />

    <!-- 创建/编辑提供者对话框 -->
    <el-dialog 
      v-model="dialogVisible" 
      :title="editingProvider ? t('llmProviderMgmt.editTitle') : t('llmProviderMgmt.createTitle')"
      width="600px"
    >
      <el-form :model="formData" label-width="100px" ref="formRef" :rules="formRules">
        <el-form-item :label="t('llmProviderMgmt.platformName')" prop="platformName" required>
          <el-input 
            v-model="formData.platformName" 
            :placeholder="t('llmProviderMgmt.platformPlaceholder')"
            clearable
          />
        </el-form-item>
        
        <el-form-item :label="t('llmProviderMgmt.modelNames')" prop="llmNames" required>
          <div style="margin-bottom: 8px;">
            <el-tag
              v-for="(name, index) in formData.llmNames"
              :key="index"
              closable
              @close="removeModelName(index)"
              style="margin-right: 6px; margin-bottom: 6px;"
            >
              {{ name }}
            </el-tag>
          </div>
          <el-input
            v-model="newModelName"
            :placeholder="t('llmProviderMgmt.modelPlaceholder')"
            @keyup.enter="addModelName"
            clearable
          >
            <template #append>
              <el-button :icon="Plus" @click="addModelName">{{ t('llmProviderMgmt.addModel') }}</el-button>
            </template>
          </el-input>
          <div style="color: #909399; font-size: 12px; margin-top: 4px;">
            {{ t('llmProviderMgmt.duplicateModelHint') }}
          </div>
        </el-form-item>
        
        <el-form-item :label="t('llmProviderMgmt.apiUrl')" prop="llmapiUrl" required>
          <el-input 
            v-model="formData.llmapiUrl" 
            :placeholder="t('llmProviderMgmt.apiUrlPlaceholder')"
            clearable
          />
        </el-form-item>
        
        <el-form-item :label="t('llmProviderMgmt.apiKey')" prop="llmapiKey" required>
          <el-input 
            v-model="formData.llmapiKey" 
            :placeholder="t('llmProviderMgmt.apiKeyPlaceholder')"
            clearable
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('llmProviderMgmt.cancel') }}</el-button>
        <el-button type="primary" @click="saveProvider" :loading="saving">{{ t('llmProviderMgmt.confirm') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { Plus, Edit, Delete, CopyDocument } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus';
import { LLMProviderService } from '@/services/llmProvider.service';
import type { IFlowLLMProviderDto } from '@/types/llmProvider.types';

const { t } = useI18n();

// 提供者列表数据
const providerList = ref<IFlowLLMProviderDto[]>([]);
const loading = ref(false);

// 对话框状态
const dialogVisible = ref(false);
const editingProvider = ref<IFlowLLMProviderDto | null>(null);
const saving = ref(false);

// 表单引用
const formRef = ref<FormInstance>();

// 表单数据
const formData = ref({
  platformName: '',
  llmNames: [] as string[],
  llmapiUrl: '',
  llmapiKey: ''
});

// 新增模型名称输入框
const newModelName = ref('');

// 表单验证规则
const formRules: FormRules = {
  platformName: [
    { required: true, message: t('llmProviderMgmt.platformNameRequired'), trigger: 'blur' }
  ],
  llmNames: [
    {
      validator: (rule, value, callback) => {
        if (!value || value.length === 0) {
          callback(new Error(t('llmProviderMgmt.modelNameRequired')));
        } else {
          callback();
        }
      },
      trigger: 'change'
    }
  ],
  llmapiUrl: [
    { required: true, message: t('llmProviderMgmt.apiUrlRequired'), trigger: 'blur' },
    { type: 'url', message: t('llmProviderMgmt.apiUrlInvalid'), trigger: 'blur' }
  ],
  llmapiKey: [
    { required: true, message: t('llmProviderMgmt.apiKeyRequired'), trigger: 'blur' }
  ]
};

// 加载提供者列表
const loadProviderList = async () => {
  try {
    loading.value = true;
    const response = await LLMProviderService.getProviderList();
    
    if (response.errCode === 0 && response.data) {
      providerList.value = response.data;
    } else {
      ElMessage.error(response.errMsg || t('llmProviderMgmt.loadFailed'));
    }
  } catch (error) {
    console.error('Failed to load LLM provider list:', error);
    ElMessage.error(t('llmProviderMgmt.loadFailed'));
  } finally {
    loading.value = false;
  }
};

// 创建提供者
const createProvider = () => {
  editingProvider.value = null;
  formData.value = {
    platformName: '',
    llmNames: [],
    llmapiUrl: '',
    llmapiKey: ''
  };
  newModelName.value = '';
  dialogVisible.value = true;
};

// 编辑提供者
const editProvider = (provider: IFlowLLMProviderDto) => {
  editingProvider.value = provider;
  formData.value = {
    platformName: provider.platformName || '',
    llmNames: provider.llmNames ? [...provider.llmNames] : [],
    llmapiUrl: provider.llmapiUrl || '',
    llmapiKey: provider.llmapiKey || ''
  };
  newModelName.value = '';
  dialogVisible.value = true;
};

// 保存提供者
const saveProvider = async () => {
  if (!formRef.value) return;
  
  // 表单验证
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;

  try {
    saving.value = true;
    
    if (editingProvider.value && editingProvider.value.id) {
      // 编辑提供者
      const response = await LLMProviderService.updateProvider({
        id: editingProvider.value.id,
        platformName: formData.value.platformName,
        llmNames: formData.value.llmNames,
        llmapiUrl: formData.value.llmapiUrl,
        llmapiKey: formData.value.llmapiKey
      });
      
      if (response.errCode === 0) {
        ElMessage.success(t('llmProviderMgmt.updateSuccess'));
        await loadProviderList();
        dialogVisible.value = false;
      } else {
        ElMessage.error(response.errMsg || t('llmProviderMgmt.updateFailed'));
      }
    } else {
      // 新建提供者
      const response = await LLMProviderService.createProvider({
        platformName: formData.value.platformName,
        llmNames: formData.value.llmNames,
        llmapiUrl: formData.value.llmapiUrl,
        llmapiKey: formData.value.llmapiKey
      });
      
      if (response.errCode === 0) {
        ElMessage.success(t('llmProviderMgmt.createSuccess'));
        await loadProviderList();
        dialogVisible.value = false;
      } else {
        ElMessage.error(response.errMsg || t('llmProviderMgmt.createFailed'));
      }
    }
  } catch (error) {
    console.error('Failed to save LLM provider:', error);
    ElMessage.error(t('llmProviderMgmt.saveFailed'));
  } finally {
    saving.value = false;
  }
};

// 删除提供者
const deleteProvider = async (provider: IFlowLLMProviderDto) => {
  ElMessageBox.confirm(t('llmProviderMgmt.deleteConfirm'), t('llmProviderMgmt.deleteHint'), {
    type: 'warning',
    confirmButtonText: t('llmProviderMgmt.confirm'),
    cancelButtonText: t('llmProviderMgmt.cancel')
  }).then(async () => {
    if (!provider.id) {
      ElMessage.warning(t('llmProviderMgmt.invalidProviderId'));
      return;
    }
    
    try {
      const response = await LLMProviderService.deleteProvider(provider.id);
      if (response.errCode === 0) {
        ElMessage.success(t('llmProviderMgmt.deleteSuccess'));
        await loadProviderList();
      } else {
        ElMessage.error(response.errMsg || t('llmProviderMgmt.deleteFailed'));
      }
    } catch (error) {
      console.error('Failed to delete LLM provider:', error);
      ElMessage.error(t('llmProviderMgmt.deleteFailed'));
    }
  }).catch(() => {});
};

// 添加模型名称
const addModelName = () => {
  const name = newModelName.value.trim();
  if (!name) {
    ElMessage.warning(t('llmProviderMgmt.modelNameEmpty'));
    return;
  }
  
  // 检查是否已存在（同一平台内不能重复）
  if (formData.value.llmNames.includes(name)) {
    ElMessage.warning(t('llmProviderMgmt.modelNameDuplicate'));
    return;
  }
  
  formData.value.llmNames.push(name);
  newModelName.value = '';
  
  // 手动触发表单验证
  formRef.value?.validateField('llmNames');
};

// 移除模型名称
const removeModelName = (index: number) => {
  formData.value.llmNames.splice(index, 1);
  
  // 手动触发表单验证
  formRef.value?.validateField('llmNames');
};

// 复制API密钥
const copyApiKey = async (apiKey: string | undefined) => {
  if (!apiKey) {
    ElMessage.warning(t('llmProviderMgmt.apiKeyEmpty'));
    return;
  }
  
  try {
    await navigator.clipboard.writeText(apiKey);
    ElMessage.success(t('llmProviderMgmt.copySuccess'));
  } catch (error) {
    console.error('Copy failed:', error);
    ElMessage.error(t('llmProviderMgmt.copyFailed'));
  }
};

// 脱敏显示API密钥
const maskApiKey = (apiKey: string | undefined) => {
  if (!apiKey) return t('llmProviderMgmt.notSet');
  if (apiKey.length <= 8) return '****';
  return `${apiKey.substring(0, 4)}****${apiKey.substring(apiKey.length - 4)}`;
};

// 组件挂载时加载数据
onMounted(() => {
  loadProviderList();
});

// 监听语言变化
watch(() => t('llmProviderMgmt.title'), () => {
  // 语言变化时重新渲染
});
</script>

<style scoped>
.llm-provider-container {
  padding: 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-header h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: #303133;
}

.api-key-masked {
  font-family: monospace;
  color: #909399;
  font-size: 13px;
}

.empty-state {
  padding: 60px 0;
}
</style>
