<!--
  关于页面
 -->
<template>
  <div class="about-view">
    <el-card>
      <template #header>
        <h2>关于本项目</h2>
      </template>
      <div class="content">
        <h3>技术栈</h3>
        <ul>
          <li><strong>Vue 3:</strong> 渐进式 JavaScript 框架</li>
          <li><strong>TypeScript:</strong> JavaScript 的超集，提供类型安全</li>
          <li><strong>Vite:</strong> 新一代前端构建工具</li>
          <li><strong>Vue Router:</strong> Vue.js 官方路由管理器</li>
          <li><strong>Pinia:</strong> Vue.js 官方状态管理库</li>
          <li><strong>Element Plus:</strong> 基于 Vue 3 的组件库</li>
          <li><strong>Axios:</strong> 基于 Promise 的 HTTP 客户端</li>
        </ul>
        
        <el-divider />
        
        <h3>项目结构</h3>
        <ul>
          <li><code>/src/types</code> - TypeScript 类型定义</li>
          <li><code>/src/utils</code> - 工具函数</li>
          <li><code>/src/services</code> - API 服务层</li>
          <li><code>/src/stores</code> - Pinia 状态管理</li>
          <li><code>/src/router</code> - 路由配置</li>
          <li><code>/src/layouts</code> - 布局组件</li>
          <li><code>/src/views</code> - 页面组件</li>
        </ul>
        
        <el-divider />
        
        <h3>后端配置</h3>
        <p>开发环境后端地址：<code>http://localhost:30050</code></p>
        <p>可在 <code>.env.development</code> 文件中修改</p>
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.about-view {
  max-width: 800px;
  margin: 0 auto;
}

.content {
  line-height: 1.8;
}

h3 {
  margin: 20px 0 10px 0;
  color: #409eff;
}

ul {
  list-style: disc;
  padding-left: 30px;
}

li {
  margin: 8px 0;
}

code {
  background-color: #f5f7fa;
  padding: 2px 6px;
  border-radius: 3px;
  color: #e74c3c;
  font-family: 'Courier New', monospace;
}

p {
  margin: 10px 0;
}
</style>
