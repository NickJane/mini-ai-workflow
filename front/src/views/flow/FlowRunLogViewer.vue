<template>
  <div class="raw-log-viewer">
    <header class="raw-log-viewer__header">
      <div class="header-left">
        <el-button text :icon="Close" @click="handleCloseWindow">{{ t('flowLog.close') }}</el-button>
        <h2>{{ t('flowLog.title') }}</h2>
      </div>
      <div class="header-right">
        <!-- 日志下拉列表 -->
        <el-select
          v-model="currentLogId"
          :placeholder="t('flowLog.selectLog')"
          @change="handleLogChange"
          style="width: 300px"
          :loading="isLoading"
        >
          <el-option
            v-for="log in logList"
            :key="log.flowInstanceId"
            :label="formatLogLabel(log)"
            :value="log.flowInstanceId"
          />
        </el-select>
        <el-button text :icon="Refresh" @click="loadLogList">{{ t('flowLog.refresh') }}</el-button>
      </div>
    </header>

    <section class="raw-log-viewer__body">
      <el-skeleton v-if="isLoading" :rows="6" animated />

      <el-empty
        v-else-if="!rawLogJson"
        :description="t('flowLog.noLog')"
      />

      <div v-else class="raw-log-panel">
        <div class="panel-actions">
          <el-button type="primary" @click="copyJson">{{ t('flowLog.copyJson') }}</el-button>
        </div>
        <div class="json-viewer-container">
          <JsonViewer
            :value="logData"
            :expand-depth="3"
            copyable
            boxed
            sort
          />
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { Close, Refresh } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { JsonViewer } from 'vue3-json-viewer';
import 'vue3-json-viewer/dist/vue3-json-viewer.css';
import { FlowService } from '@/services/flow.service';
import type { IFlowRunLogDto } from '@/types/flow.types';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const flowId = computed(() => Number(route.params.flowId));
const logId = computed(() => Number(route.params.logId));

const isLoading = ref(false);
const rawLogJson = ref<string>('');
const logData = ref<any>(null); // JSON 对象数据
const logList = ref<IFlowRunLogDto[]>([]); // 日志列表
const currentLogId = ref<number>(0); // 当前选中的日志ID

// 加载日志列表
const loadLogList = async () => {
  if (!flowId.value) {
    ElMessage.warning(t('flowLog.missingFlowId'));
    return;
  }

  try {
    isLoading.value = true;
    const response = await FlowService.getExecutionLogsByFlowId(flowId.value);
    if (response.errCode === 0 && response.data) {
      logList.value = response.data;
      // 如果 URL 中有 logId，则同步到下拉列表
      if (logId.value) {
        currentLogId.value = logId.value;
        // 显示当前日志
        const matchedLog = logList.value.find(item => item.flowInstanceId === logId.value);
        if (matchedLog) {
          rawLogJson.value = JSON.stringify(matchedLog, null, 2);
          logData.value = matchedLog; // 设置对象数据供 JsonViewer 使用
        } else {
          rawLogJson.value = '';
          logData.value = null;
          ElMessage.warning(t('flowLog.noLogFound'));
        }
      } else if (logList.value.length > 0) {
        // 如果没有指定日志ID，默认选择第一个
        currentLogId.value = logList.value[0].flowInstanceId;
        rawLogJson.value = JSON.stringify(logList.value[0], null, 2);
        logData.value = logList.value[0];
      } else {
        ElMessage.warning(t('flowLog.noLog'));
      }
    } else {
      rawLogJson.value = '';
      logData.value = null;
      ElMessage.error(response.errMsg || t('flowLog.loadFailed'));
    }
  } catch (error) {
    console.error('加载运行日志失败:', error);
    rawLogJson.value = '';
    logData.value = null;
    ElMessage.error(t('flowLog.loadFailed'));
  } finally {
    isLoading.value = false;
  }
};

// 格式化日志标签显示
const formatLogLabel = (log: IFlowRunLogDto): string => {
  const date = new Date(log.createdTime);
  const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')} ${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}:${String(date.getSeconds()).padStart(2, '0')}`;
  const status = log.isSuccess ? t('flowLog.success') : t('flowLog.failed');
  return `${dateStr} - ${status} (ID: ${log.flowInstanceId})`;
};

// 切换日志
const handleLogChange = (logId: number) => {
  // 更新 URL 参数
  router.push({
    name: route.name as string,
    params: {
      flowId: flowId.value,
      logId: logId
    }
  });
};

const copyJson = async () => {
  if (!rawLogJson.value) return;
  try {
    await navigator.clipboard.writeText(rawLogJson.value);
    ElMessage.success(t('flowLog.copySuccess'));
  } catch (error) {
    console.error('Copy failed:', error);
    ElMessage.error(t('flowLog.copyFailed'));
  }
};

const handleCloseWindow = () => {
  window.close();
};

onMounted(() => {
  loadLogList();
});

watch([flowId, logId], () => {
  // 当路由参数变化时，重新加载日志列表
  loadLogList();
});
</script>

<style scoped>
.raw-log-viewer {
  width: 100vw;
  height: 100vh;
  background-color: #f5f7fa;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.raw-log-viewer__header {
  height: 64px;
  padding: 0 24px;
  border-bottom: 1px solid #e5e7eb;
  background-color: #fff;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-left h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #111827;
}

.raw-log-viewer__body {
  flex: 1;
  min-height: 0;
  padding: 24px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.raw-log-panel {
  flex: 1;
  min-height: 0;
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  border: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.panel-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  flex-shrink: 0;
  margin-bottom: 16px;
}

.json-viewer-container {
  flex: 1;
  min-height: 0;
  overflow-y: scroll;
  overflow-x: auto;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #fafafa;
  padding: 12px;
}

/* 自定义 JsonViewer 样式 */
.json-viewer-container :deep(.jv-container) {
  background: transparent;
  font-family: 'JetBrains Mono', 'Fira Code', Consolas, Monaco, 'Courier New', monospace;
  font-size: 13px;
  line-height: 1.6;
}

.json-viewer-container :deep(.jv-key) {
  color: #0451a5;
  font-weight: 500;
}

.json-viewer-container :deep(.jv-string) {
  color: #a31515;
}

.json-viewer-container :deep(.jv-number) {
  color: #098658;
}

.json-viewer-container :deep(.jv-boolean) {
  color: #0000ff;
  font-weight: 500;
}

.json-viewer-container :deep(.jv-null) {
  color: #808080;
  font-weight: 500;
}

.json-viewer-container :deep(.jv-toggle) {
  cursor: pointer;
  user-select: none;
}

.json-viewer-container :deep(.jv-item) {
  padding: 2px 0;
}
</style>

