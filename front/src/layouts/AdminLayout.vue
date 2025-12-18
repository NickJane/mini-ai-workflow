<!--
  管理后台布局组件
  提供包含顶部导航栏和侧边栏的标准后台管理界面布局
  支持左侧菜单切换右侧内容区域
 -->
<template>
  <el-container class="admin-layout">
    <!-- 顶部导航栏 -->
    <el-header>
      <div class="header-content">
        <h1 class="logo">{{ t('layout.siteTitle') }}</h1>
        <div class="user-info">
          <LanguageSwitcher />
          <span class="username">{{ userInfo?.name || t('layout.user') }}</span>
          <el-button type="danger" size="small" @click="handleLogout">
            {{ t('layout.logout') }}
          </el-button>
        </div>
      </div>
    </el-header>
    
    <el-container>
      <!-- 侧边导航栏 -->
      <el-aside width="220px">
        <el-menu
          :default-active="activeMenu"
          class="side-menu"
          @select="handleMenuSelect"
        >
          <el-menu-item index="home">
            <el-icon><House /></el-icon>
            <span>{{ t('nav.home') }}</span>
          </el-menu-item>
          
          <el-divider style="margin: 8px 0;" />
          
          <el-menu-item index="logic-flow">
            <el-icon><Share /></el-icon>
            <span>{{ t('nav.logicFlow') }}</span>
          </el-menu-item>
          
          <el-menu-item index="ai-flow">
            <el-icon><MagicStick /></el-icon>
            <span>{{ t('nav.aiFlow') }}</span>
          </el-menu-item>
          
          <el-menu-item index="approval-flow">
            <el-icon><CircleCheck /></el-icon>
            <span>{{ t('nav.approvalFlow') }}</span>
          </el-menu-item>
          
          <el-divider style="margin: 8px 0;" />
          
          <el-menu-item index="llm-provider">
            <el-icon><Setting /></el-icon>
            <span>{{ t('layout.modelManagement') }}</span>
          </el-menu-item>
          
          <el-divider style="margin: 8px 0;" />
          
          <el-menu-item index="about">
            <el-icon><InfoFilled /></el-icon>
            <span>{{ t('layout.about') }}</span>
          </el-menu-item>
        </el-menu>
      </el-aside>
      
      <!-- 主内容区域 -->
      <el-main>
        <!-- 根据activeMenu显示不同内容 -->
        <HomeView v-if="activeMenu === 'home'" key="home" />
        <AboutView v-else-if="activeMenu === 'about'" key="about" />
        <FlowList v-else-if="activeMenu === 'logic-flow'" flow-type="logic" key="logic-flow" />
        <FlowList v-else-if="activeMenu === 'ai-flow'" flow-type="ai" key="ai-flow" />
        <FlowList v-else-if="activeMenu === 'approval-flow'" flow-type="approval" key="approval-flow" />
        <LLMProviderList v-else-if="activeMenu === 'llm-provider'" key="llm-provider" />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth';
import { House, InfoFilled, Share, MagicStick, CircleCheck, Setting } from '@element-plus/icons-vue';

const { t } = useI18n();

// 导入页面组件
import HomeView from '@/views/HomeView.vue';
import AboutView from '@/views/AboutView.vue';
import FlowList from '@/views/flow/FlowList.vue';
import LLMProviderList from '@/views/llm/LLMProviderList.vue';
import LanguageSwitcher from '@/components/LanguageSwitcher.vue';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

// 当前激活的菜单项
const activeMenu = ref('home');

// 用户信息
const userInfo = computed(() => authStore.getLoginUserInfo);


// 监听路由变化，更新activeMenu
watch(() => route.path, (newPath) => {
  // 根据路由路径设置activeMenu
  if (newPath === '/') {
    activeMenu.value = 'home';
  } else if (newPath === '/about') {
    activeMenu.value = 'about';
  }
  // 其他路径保持当前activeMenu不变（例如从设计器返回时）
}, { immediate: true });

// 菜单选择处理
const handleMenuSelect = (index: string) => {
  activeMenu.value = index;
  
  // 如果是首页或关于页，同时更新路由（但不刷新页面）
  if (index === 'home') {
    router.push('/');
  } else if (index === 'about') {
    router.push('/about');
  }
  // 流程列表不更新路由，只切换内容
};

// 登出处理
const handleLogout = () => {
  authStore.signOut();
};
</script>

<style scoped>
.admin-layout {
  height: 100vh;
}

.el-header {
  background-color: #409eff;
  color: white;
  display: flex;
  align-items: center;
  padding: 0 20px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.logo {
  margin: 0;
  font-size: 20px;
  font-weight: bold;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.username {
  margin-right: 10px;
}

.el-aside {
  background-color: #fff;
  border-right: 1px solid #dcdfe6;
}

.side-menu {
  height: 100%;
  border-right: none;
}

.el-main {
  background-color: #f5f7fa;
  padding: 20px;
}
</style>
