import { createI18n } from 'vue-i18n'
import zhCN from './zh-CN'
import enUS from './en-US'

// Element Plus 的语言包
import zhCnElementPlus from 'element-plus/es/locale/lang/zh-cn'
import enElementPlus from 'element-plus/es/locale/lang/en'

// 支持的语言列表
export type Language = 'zh-CN' | 'en-US'

// 获取默认语言（优先级：localStorage > 浏览器语言 > 默认中文）
const getDefaultLocale = (): Language => {
  const savedLocale = localStorage.getItem('language') as Language
  if (savedLocale && ['zh-CN', 'en-US'].includes(savedLocale)) {
    return savedLocale
  }
  
  const browserLang = navigator.language
  return browserLang.startsWith('zh') ? 'zh-CN' : 'en-US'
}

// 创建 i18n 实例
const i18n = createI18n({
  legacy: false,  // 使用 Composition API 模式
  locale: getDefaultLocale(),
  fallbackLocale: 'zh-CN',
  messages: {
    'zh-CN': zhCN,
    'en-US': enUS
  },
  // 全局注入 $t
  globalInjection: true
})

// Element Plus 语言映射
export const elementLocales: Record<Language, any> = {
  'zh-CN': zhCnElementPlus,
  'en-US': enElementPlus
}

// 切换语言的辅助函数
export const changeLanguage = (lang: Language) => {
  i18n.global.locale.value = lang
  localStorage.setItem('language', lang)
  // 更新 HTML lang 属性
  document.documentElement.lang = lang
}

// 获取当前语言
export const getCurrentLanguage = (): Language => {
  return i18n.global.locale.value as Language
}

export default i18n
