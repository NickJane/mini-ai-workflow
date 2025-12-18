<!--
  首页视图 - 超级流程平台
 -->
<template>
  <div class="home-view">
    <!-- 欢迎横幅 -->
    <div class="welcome-banner">
      <h1>{{ t('home.welcome') }}</h1>
      <p>{{ t('home.greeting', { name: userInfo?.name || 'User' }) }}</p>
    </div>

    <!-- 快速统计 -->
    <div class="quick-stats">
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card shadow="hover">
            <el-statistic :title="t('nav.logicFlow')" :value="logicFlowCount" :loading="loading">
              <template #prefix>
                <el-icon style="color: #409eff;"><Share /></el-icon>
              </template>
              <template #suffix>{{ t('home.items') }}</template>
            </el-statistic>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card shadow="hover">
            <el-statistic :title="t('nav.aiFlow')" :value="aiFlowCount" :loading="loading">
              <template #prefix>
                <el-icon style="color: #7c3aed;"><MagicStick /></el-icon>
              </template>
              <template #suffix>{{ t('home.items') }}</template>
            </el-statistic>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card shadow="hover">
            <el-statistic :title="t('nav.approvalFlow')" :value="approvalFlowCount" :loading="loading">
              <template #prefix>
                <el-icon style="color: #10b981;"><CircleCheck /></el-icon>
              </template>
              <template #suffix>{{ t('home.items') }}</template>
            </el-statistic>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 功能介绍 -->
    <el-card class="feature-intro" shadow="never">
      <template #header>
        <h3>{{ t('home.platformFeatures') }}</h3>
      </template>
      <el-row :gutter="20">
        <el-col :span="8">
          <div class="feature-item">
            <el-icon :size="32" color="#409eff"><Share /></el-icon>
            <h4>{{ t('nav.logicFlow') }}</h4>
            <p>{{ t('home.logicFlowDesc') }}</p>
          </div>
        </el-col>
        <el-col :span="8">
          <div class="feature-item">
            <el-icon :size="32" color="#7c3aed"><MagicStick /></el-icon>
            <h4>{{ t('nav.aiFlow') }}</h4>
            <p>{{ t('home.aiFlowDesc') }}</p>
          </div>
        </el-col>
        <el-col :span="8">
          <div class="feature-item">
            <el-icon :size="32" color="#10b981"><CircleCheck /></el-icon>
            <h4>{{ t('nav.approvalFlow') }}</h4>
            <p>{{ t('home.approvalFlowDesc') }}</p>
          </div>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth';
import { Share, MagicStick, CircleCheck } from '@element-plus/icons-vue';
import { FlowService } from '@/services/flow.service';
import { FlowType } from '@/types/flow.types';

const { t } = useI18n();

const authStore = useAuthStore();

// 用户信息
const userInfo = computed(() => authStore.getLoginUserInfo);

// 流程统计数据
const logicFlowCount = ref(0);
const aiFlowCount = ref(0);
const approvalFlowCount = ref(0);
const loading = ref(true);

// 加载统计数据
const loadStatistics = async () => {
  try {
    loading.value = true;
    
    // 并行请求三种流程的数据
    const [logicResponse, aiResponse, approvalResponse] = await Promise.all([
      FlowService.getFlowList({ flowType: FlowType.LogicFlow, pageIndex: 1, pageSize: 1000 }),
      FlowService.getFlowList({ flowType: FlowType.AIFlow, pageIndex: 1, pageSize: 1000 }),
      FlowService.getFlowList({ flowType: FlowType.ApprovalFlow, pageIndex: 1, pageSize: 1000 })
    ]);
    
    // 统计每种类型的数量
    if (logicResponse.errCode === 0 && logicResponse.data) {
      logicFlowCount.value = logicResponse.data.length;
    }
    if (aiResponse.errCode === 0 && aiResponse.data) {
      aiFlowCount.value = aiResponse.data.length;
    }
    if (approvalResponse.errCode === 0 && approvalResponse.data) {
      approvalFlowCount.value = approvalResponse.data.length;
    }
  } catch (error) {
    console.error('Failed to load statistics:', error);
  } finally {
    loading.value = false;
  }
};

// 组件挂载时加载数据
onMounted(() => {
  loadStatistics();
});
</script>

<style scoped>
.home-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.welcome-banner {
  text-align: center;
  padding: 40px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  color: white;
  margin-bottom: 30px;
}

.welcome-banner h1 {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.welcome-banner p {
  font-size: 16px;
  margin: 0;
  opacity: 0.95;
}

.quick-stats {
  margin-bottom: 30px;
}

:deep(.el-statistic__head) {
  font-size: 14px;
  color: #909399;
}

:deep(.el-statistic__content) {
  font-size: 28px;
  font-weight: 600;
  color: #303133;
}

.feature-intro {
  margin-top: 20px;
}

.feature-intro h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #303133;
}

.feature-item {
  text-align: center;
  padding: 20px;
}

.feature-item h4 {
  margin: 12px 0 8px 0;
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.feature-item p {
  font-size: 14px;
  color: #606266;
  line-height: 1.6;
  margin: 0;
}
</style>
