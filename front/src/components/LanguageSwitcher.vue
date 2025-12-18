<template>
  <el-dropdown @command="handleCommand" trigger="click">
    <el-button text class="language-btn">
      <el-icon><Operation /></el-icon>
      <span class="lang-label">{{ currentLanguageLabel }}</span>
    </el-button>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item command="zh-CN" :disabled="locale === 'zh-CN'">
          <span class="lang-item">
            <span class="flag">🇨🇳</span>
            <span>简体中文</span>
            <el-icon v-if="locale === 'zh-CN'" class="check-icon"><Check /></el-icon>
          </span>
        </el-dropdown-item>
        <el-dropdown-item command="en-US" :disabled="locale === 'en-US'">
          <span class="lang-item">
            <span class="flag">🇺🇸</span>
            <span>English</span>
            <el-icon v-if="locale === 'en-US'" class="check-icon"><Check /></el-icon>
          </span>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { Operation, Check } from '@element-plus/icons-vue'
import { changeLanguage, type Language } from '@/locales'
import { ElMessage } from 'element-plus'

const { locale } = useI18n()

// 当前语言显示标签
const currentLanguageLabel = computed(() => {
  return locale.value === 'zh-CN' ? '中文' : 'EN'
})

// 处理语言切换
const handleCommand = (lang: Language) => {
  if (lang === locale.value) return
  
  changeLanguage(lang)
  ElMessage.success({
    message: lang === 'zh-CN' ? '语言已切换为中文' : 'Language switched to English',
    duration: 2000
  })
  
  // 刷新页面以确保所有组件都使用新语言
  setTimeout(() => {
    location.reload()
  }, 500)
}
</script>

<style scoped>
.language-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  font-size: 14px;
  color: #606266;
  transition: color 0.3s;
}

.language-btn:hover {
  color: #409eff;
}

.lang-label {
  font-weight: 500;
}

.lang-item {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 120px;
}

.flag {
  font-size: 18px;
}

.check-icon {
  margin-left: auto;
  color: #409eff;
}
</style>
