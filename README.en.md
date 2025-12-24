## mini-ai-workflow

[中文文档](README.md) | **English**

A ready-to-use mini AI workflow designer built with .NET 10 and Vue 3 (currently focused on flow / process design), which can, to some extent, be used as a lightweight alternative to Dify.

## Overview

`mini-ai-workflow` is a visual workflow orchestration tool for building AI workflows.  
The frontend is built with Vue 3 and LogicFlow for flow design and debugging, while the backend is an ASP.NET Core Web API running on .NET 10.  
This repository already contains both the frontend and backend code and can be run locally out of the box.

This repository is positioned as a relatively **simple demo project**: the core design tries to be reasonably robust and extensible, but this repo itself **will not receive ongoing updates** and is mainly for demonstration and learning.  
In the future, there might be a small **paid edition** that builds on top of this open-source version and adds a few extra features, for example:  
- **Request logging and auditing** for API calls  
- **Integration with other AI workflow nodes/systems** so you can chain or embed external workflows  

The **most important core logic is already open-sourced** here; the paid source code would only contain a small amount of extra “nice-to-have” functionality.  
If this little project happens to help you and you don’t mind buying the author a coffee, that paid edition is simply a humble way to say thanks and give a bit of motivation for building more small useful tools.

## Tech Stack

### Frontend

- **Framework**: Vue 3 + TypeScript  
- **Build Tool**: Vite  
- **UI Library**: Element Plus  
- **Routing**: Vue Router  
- **State Management**: Pinia  
- **HTTP Client**: Axios  
- **Flow Orchestration**: `@logicflow/core`, `@logicflow/extension`  
- **Code Editor**: CodeMirror (with JavaScript language support)  
- **i18n**: `vue-i18n` (Chinese & English)  
- **JSON Viewer**: `vue3-json-viewer`  
- **Markdown Rendering**: `marked`  

> Frontend code is located in the `front` directory. All dependencies are open-source libraries.

### Backend

- **Runtime & Framework**: .NET 10 / ASP.NET Core Web API  
- **Core Libraries**:
  - `Jint` (JavaScript runtime for executing scripts inside flows)
- **Infrastructure & Abstractions**:
  - Custom Web API framework: `Nop.WebApiFramework`
  - Shared infrastructure library: `Nop.Infrastructure`
  - Logging: Serilog + console (with optional Seq)
  - Optional tracing: OpenTelemetry (e.g., Zipkin)
- **Data Storage**:
  - Database: MySQL (via `FreeSql`, default database name: `aiworkflowfree`)
  - Optional cache: Redis (connection string configurable, currently not actually used in the code)

> Backend code is under `backend/SuperFlowApi`. Shared base libraries are under `backend/DotnetLabs`.

## Run Frontend Locally

### Requirements

- **Node.js**: ≥ 18 is recommended  
- **Package Manager**: npm (or compatible pnpm / yarn, adjust commands accordingly)

### Steps

From the project root:

```bash
cd front

# Install dependencies
npm install

# Start dev server
npm run dev

# Build for production
npm run build
```

### Backend API Base URL

- Default backend URL in development: `http://localhost:30050`  
- You can create or edit `.env.development` in the frontend root and set the backend base URL, for example:

```bash
VITE_API_BASE_URL=http://localhost:30050
```

If there is already a shared HTTP helper (e.g. `http.util.ts`) or config file, make sure the base URL inside matches the actual backend address.

## Run Backend Locally

### Requirements

- **.NET SDK**: .NET 10 SDK (with `dotnet` CLI)  
- **Database**: MySQL (5.7+/8.0+ recommended), default connection info (can be customized):
  - Server: `localhost`
  - Port: `3306`
  - Database: `aiworkflowfree`
  - User: `root`
  - Password: `sa1234`
- (Optional) **Redis**: Local `127.0.0.1:6379`, used for caching related features

> Database and connection strings can be configured in `backend/SuperFlowApi/appsettings.Development.json` under `FreeSqlDatabase` and `ConnectionStrings` sections.

### Steps

From the project root:

```bash
cd backend/SuperFlowApi

# Restore dependencies (run once or after dependency changes)
dotnet restore

# Run in Development (uses http profile defined in launchSettings.json, default http://localhost:30050)
dotnet run
```

When the backend starts successfully you should see ASP.NET Core logs and the default listening address `http://localhost:30050`.

## Frontend–Backend Integration

1. Start the backend (see “Run Backend Locally”) and ensure it listens on `http://localhost:30050` or the same URL you configured in the frontend env file.  
2. Start the frontend (see “Run Frontend Locally”) and open the URL shown by Vite in your browser.  
3. If you encounter CORS or network errors when using login, flow design or execution features, please check:
   - Backend is running and the port is correct  
   - Frontend API base URL is configured correctly  
   - Network requests in the browser (DevTools → Network) are pointing to the expected backend URL  

## Important: LLM Configuration

- **You must configure an LLM before using any LLM-related nodes** on the canvas; otherwise these nodes will not work correctly.  
- The current implementation only supports (and has only been tested with) **Alibaba Cloud (Aliyun) model services**, such as Qwen (Tongyi Qianwen), etc.  
- Please use the credentials you created in the Aliyun console (keys, base URL, model name, etc.) to configure backend model access.  
- If you extend or adapt other vendors’ models in the future, please update the corresponding configuration and code, and also keep this section of the README in sync.

> It is recommended to configure and test model connectivity before doing serious workflow design.

Reference UI screenshot (for illustration only, actual UI may differ):

![LLM configuration notice](docs/images/step1.png)

## Product Preview

Some screenshots of the running system to give you a quick idea of the main UI and interactions:

![Flow design & run preview 1](docs/images/step2.png)

![Flow design & run preview 2](docs/images/step3.png)

## Login

- **Username**: Mainland China mobile phone number (11 digits, e.g. `13800001234`)  
- **Password**: Any string is accepted (demo only, no strong policy)  
- **Login = Registration**: If the phone number has not been registered before, the first login will automatically create the account.

> This login rule is only for demo / development purposes. **Do not** use real or important passwords here.


