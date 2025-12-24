/**
 * 路由配置文件
 * Vue Router 用于实现前端路由，使单页应用能够在不同页面间切换
 * 
 * 注意：流程列表在AdminLayout中通过组件切换展示，不使用路由
 */
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// 创建路由实例
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { 
        layout: 'simple',
        requiresAuth: false 
      }
    },
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue'),
      meta: { 
        layout: 'admin',
        requiresAuth: true 
      }
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('@/views/AboutView.vue'),
      meta: { 
        layout: 'admin',
        requiresAuth: true 
      }
    },
    // ==================== 流程设计器（全屏显示） ====================
    // 统一设计器，通过flowType参数区分：logic/ai/approval
    {
      path: '/designer/:flowType/:id?',
      name: 'flowDesigner',
      component: () => import('@/views/flow/FlowDesigner.vue'),
      meta: { 
        layout: 'simple',
        requiresAuth: true,
        title: '流程设计器'
      }
    },

    {
      path: '/flow/chat-test/:flowId',
      name: 'flowChatTest',
      component: () => import('@/views/flow/FlowChatTest.vue'),
      meta: {
        layout: 'simple',
        requiresAuth: true,
        title: 'AI工作流测试'
      }
    }
  ]
})

/**
 * 路由守卫
 * 在每次路由跳转前执行，用于检查认证状态
 */
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();

  // 白名单路由直接放行
  if (to.path === '/login') {
    next()
    return
  }

  // 需要认证但未登录时跳转到登录页
  if (to.meta.requiresAuth && !authStore.getIsAuthenticated) {
    next('/login')
    return
  }

  // 其他情况放行
  next()
})

export default router
