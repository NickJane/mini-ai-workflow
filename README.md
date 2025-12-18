# mini-ai-workflow

A ready-to-use mini AI workflow designer developed using .NET 10 and Vue 3 (currently focused on flow/process design), which can, to some extent, replace Dify.

## 项目简介

`mini-ai-workflow` 是一个用于搭建 AI 工作流的可视化流程编排工具，前端基于 Vue 3 与 LogicFlow 实现流程设计与调试，后端基于 .NET 10（代码后续开源）。当前仓库已暂时提交前端工程。

## 技术栈

### 前端

- **框架**：Vue 3 + TypeScript
- **构建工具**：Vite
- **UI 组件库**：Element Plus
- **路由管理**：Vue Router
- **状态管理**：Pinia
- **HTTP 请求**：Axios
- **流程编排**：@logicflow/core, @logicflow/extension
- **代码编辑器**：CodeMirror（含 JavaScript 语言支持）
- **国际化**：vue-i18n（中英文多语言）
- **JSON 展示**：vue3-json-viewer
- **Markdown 渲染**：marked

> 前端代码位于 `front` 目录，整体依赖均为开源库。

### 后端

- **技术栈**：.NET 10（ASP.NET Core，计划开源，当前仓库暂无完整后端代码）

## 本地运行（前端）

### 环境要求

- **Node.js**：建议 ≥ 18
- **包管理器**：npm（或兼容的 pnpm / yarn，自行替换命令）

### 启动步骤

在项目根目录下执行：

```bash
cd front

# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 构建生产版本
npm run build
```

前端默认使用 Vite 开发服务器（通常为 `http://localhost:5173`）。

### 后端地址配置

- 开发环境默认后端地址：`http://localhost:30050`
- 可在前端项目的 `.env.development` 文件中修改后端 API 基础地址。

## 登录说明

- **用户名**：使用中国大陆手机号码格式（11 位手机号，例如 `13800001234`）。
- **密码**：任意字符串即可（仅作示例用途）。
- **登录即注册**：如果手机号未注册，则第一次登录会自动完成注册，无需单独注册流程。

上述登录规则仅适用于本项目的演示/开发环境，请勿使用真实重要密码。
