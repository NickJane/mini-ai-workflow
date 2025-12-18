/**
 * 应用入口文件
 * 初始化 Vue 应用，注册插件和全局组件
 */
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { computed } from 'vue'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import 'element-plus/dist/index.css'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'
import i18n, { elementLocales } from '@/locales'

// 创建 Vue 应用实例
const app = createApp(App)
const pinia = createPinia()

// 注册所有 Element Plus 图标组件
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// 注册插件
app.use(pinia)        // 状态管理
app.use(router)       // 路由
app.use(i18n)         // 国际化
app.use(ElementPlus)  // UI 组件库

// 初始化 auth store
// 如果本地存储中有 token，启动自动刷新定时器
const authStore = useAuthStore()
authStore.init()

// 挂载应用到 DOM
app.mount('#app')
