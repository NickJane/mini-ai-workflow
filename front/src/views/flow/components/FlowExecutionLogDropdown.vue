<template>
  <el-dropdown
    trigger="click"
    class="log-dropdown"
    @command="handleCommand"
    @visible-change="handleVisibleChange"
  >
    <el-button class="log-dropdown-btn" :loading="isLoading" text>
      运行日志
      <el-icon class="el-icon--right">
        <ArrowDown />
      </el-icon>
    </el-button>
    <template #dropdown>
      <el-dropdown-menu class="log-dropdown-menu">
        <template v-if="executionLogs.length">
          <el-dropdown-item
            v-for="log in executionLogs"
            :key="log.flowInstanceId"
            :command="log.flowInstanceId"
            class="log-dropdown-item"
          >
            <div class="log-item">
              <div class="log-item__header">
                <span class="log-item__name">{{ log.displayName || '未命名运行' }}</span>
                <el-tag size="small" :type="log.isSuccess ? 'success' : 'danger'">
                  {{ log.isSuccess ? '成功' : '失败' }}
                </el-tag>
              </div>
              <div class="log-item__meta">
                <span>{{ formatDatetime(log.createdTime) }}</span>
                <span>耗时：{{ formatDuration(log.runDurationMs) }}</span>
                <span>用户：{{ log.runUser || '系统' }}</span>
              </div>
            </div>
          </el-dropdown-item>
        </template>
        <el-dropdown-item
          v-else
          disabled
          class="log-dropdown-empty"
        >
          暂无运行日志
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup lang="ts">
import { ArrowDown } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n'; // 添加 useI18n 导入
import { FlowService } from '@/services/flow.service';
import type { IFlowRunLogDto } from '@/types/flow.types';

const props = withDefaults(defineProps<{
  flowId?: number | null;
}>(), {
  flowId: null
});

const executionLogs = ref<IFlowRunLogDto[]>([]);
const isLoading = ref(false);
const router = useRouter();
const { t } = useI18n(); // 添加 useI18n 初始化

const hasFlowId = computed(() => typeof props.flowId === 'number' && !Number.isNaN(props.flowId));

const formatDatetime = (value?: string | null) => {
  if (!value) return '--';
  return new Date(value).toLocaleString();
};

const formatDuration = (value?: number | null) => {
  if (!value || value <= 0) return '--';
  if (value < 1000) return `${value} ms`;
  return `${(value / 1000).toFixed(1)} s`;
};

const fetchExecutionLogs = async () => {
  if (!hasFlowId.value) {
    executionLogs.value = [];
    return;
  }

  try {
    isLoading.value = true;
    const response = await FlowService.getExecutionLogsByFlowId(props.flowId as number);
    if (response.errCode === 0) {
      executionLogs.value = response.data ?? [];
    } else {
      executionLogs.value = [];
      ElMessage.error(response.errMsg || '获取运行日志失败');
    }
  } catch (error) {
    console.error('获取运行日志失败:', error);
    executionLogs.value = [];
    ElMessage.error('获取运行日志失败');
  } finally {
    isLoading.value = false;
  }
};

const openRunLogWindow = (logId: number) => {
  if (!hasFlowId.value) {
    ElMessage.warning('请先保存流程');
    return;
  }

  const routeInfo = router.resolve({
    name: 'flowRunLogViewer',
    params: {
      flowId: props.flowId,
      logId
    }
  });
  window.open(routeInfo.href, '_blank', 'noopener');
};

const handleCommand = (command: number | string | Record<string, number>) => {
  const id = typeof command === 'object'
    ? Number((command as Record<string, number>).id)
    : Number(command);

  if (!Number.isNaN(id)) {
    openRunLogWindow(id);
  }
};

const handleVisibleChange = (visible: boolean) => {
  if (visible) {
    fetchExecutionLogs();
  }
};

watch(() => props.flowId, () => {
  executionLogs.value = [];
});
</script>

<style scoped>
.log-dropdown-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  font-weight: 500;
  color: #1f2937;
}

.log-dropdown-menu {
  width: 360px;
  max-height: 360px;
  overflow-y: auto;
  padding: 8px 0;
}

.log-dropdown-item {
  white-space: normal;
}

.log-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.log-item__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
  color: #111827;
}

.log-item__name {
  font-weight: 600;
}

.log-item__meta {
  display: flex;
  flex-direction: column;
  font-size: 12px;
  color: #6b7280;
  gap: 2px;
}

.log-dropdown-empty {
  color: #9ca3af;
  cursor: default;
}
</style>

