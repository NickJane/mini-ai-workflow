<!--
  应用根组件
  根据路由配置动态选择布局组件
 -->
<template>
  <!-- 根据路由元信息选择布局 -->
  <AdminLayout v-if="useAdminLayout">
    <router-view />
  </AdminLayout>
  <SimpleLayout v-else>
    <router-view />
  </SimpleLayout>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import AdminLayout from "@/layouts/AdminLayout.vue";
import SimpleLayout from "@/layouts/SimpleLayout.vue";

const route = useRoute();

/**
 * 计算属性 - 根据路由元数据动态选择布局
 * 判断当前路由是否使用 admin 布局
 */
const useAdminLayout = computed(() => {
  const layout = route.meta.layout;
  return layout === "admin";
});
</script>

<style>
/* 全局重置样式 */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html,
body {
  height: 100%;
  width: 100%;
  overflow: hidden; /* 防止出现双滚动条 */
}

/* 应用根容器 */
#app {
  height: 100%;
  width: 100%;
}

/* Element Plus 组件全局样式调整 */
.el-container {
  height: 100%;
  width: 100%;
}

.el-header {
  height: 60px !important;
  line-height: 60px;
  background-color: #fff;
  border-bottom: 1px solid #dcdfe6;
  padding: 0 20px;
}

.el-aside {
  background-color: #fff;
  border-right: 1px solid #dcdfe6;
}

.el-main {
  background-color: #f5f7fa;
  padding: 20px;
  height: calc(100% - 60px);
  overflow-y: auto; /* 内容区域可滚动 */
}

/* 滚动条样式 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
