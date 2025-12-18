<!--
  简单布局组件
  用于展示不需要后台管理界面框架的页面，比如：
  - 登录页面
  - 错误页面等
 -->
<template>
  <div class="simple-layout">
    <!-- 使用插槽来渲染子组件 -->
    <slot></slot>
  </div>
</template>

<style scoped>
.simple-layout {
  min-height: 100vh;
  background-color: #f5f7fa;
}
</style>
