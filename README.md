# CSMSNet - 电动汽车充电云平台

一个基于 .NET 10 构建的完整电动汽车充电云平台 (CSMS) 解决方案，集成了高性能 OCPP 协议适配器和现代化的 Blazor Web 管理界面。

## 项目特性

### 核心功能
- **OCPP 1.6J 协议支持**: 完整的 OCPP 1.6 JSON 协议实现（已验证）。
- **高性能适配器**: 基于 WebSocket 的高并发连接处理。
- **现代化 Web 管理**: 使用 ASP.NET Core Blazor (Interactive Auto) 构建的实时管理界面。
- **状态管理**: 实时监控充电桩连接状态、事务状态。
- **OCPP 2.1 准备**: 包含 OCPP 2.1 协议文档与 Schema，为未来升级预留架构支持。

### 技术栈
- **核心框架**: .NET 10
- **Web 框架**: ASP.NET Core Blazor (Server + Client / Interactive Auto)
- **协议处理**: System.Text.Json
- **架构**: 模块化设计，核心逻辑与 Web 展示分离

## 项目结构

```
CSMSNet.sln                           # 解决方案文件
├── src/
│   ├── CSMSNet.Ocpp/                 # OCPP 协议核心库 (适配器)
│   │   ├── Abstractions/             # 接口定义
│   │   ├── Models/                   # 数据模型 (V16, Events, State)
│   │   ├── Protocol/                 # 协议处理逻辑
│   │   └── Server/                   # WebSocket 服务与连接管理
│   │
│   └── CSMSNet.Web/                  # Web 应用解决方案目录
│       ├── CSMSNet.Web/              # Blazor Web 应用 (服务端/Host)
│       └── CSMSNet.Web.Client/       # Blazor Web 应用 (客户端/WASM)
│
└── tests/
    └── CSMSNet.Ocpp.Tests/           # 单元测试
```

## 快速开始

### 1. 环境要求
- .NET 10 SDK (预览版或正式版)

### 2. 运行项目

```bash
# 还原依赖
dotnet restore

# 运行 Web 应用
cd src/CSMSNet.Web/CSMSNet.Web
dotnet run
```

应用启动后：
- **OCPP 服务器**: 监听 `/ocpp/{chargePointId}` (WebSocket)
- **Web 管理界面**: `http://localhost:5169` (或控制台显示的端口)

### 3. 集成指南 (针对开发)

如果您需要在现有 ASP.NET Core 应用中集成 OCPP 适配器：

```csharp
using CSMSNet.Ocpp;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册服务
// 将自动读取配置中的 "OcppAdapter" 节点
builder.Services.AddOcppAdapter(builder.Configuration);

var app = builder.Build();

// 2. 启用 WebSocket
app.UseWebSockets();

// 3. 映射 OCPP 端点
app.Map("/ocpp", b => b.UseOcppAdapter());

app.Run();
```

## 配置说明

在 `appsettings.json` 中配置 OCPP 适配器：

```json
{
  "OcppAdapter": {
    "HeartbeatInterval": "00:05:00",
    "BusinessEventTimeout": "00:00:30",
    "SessionInactivityTimeout": "00:30:00"
  }
}
```

## 协议支持详情

本项目目前实现基于以下规范:
- **OCPP 1.6 Edition 2 (JSON)**: 完全支持
- **OCPP 2.1**: 已包含协议文档和 Schema，代码实现规划中

## 许可证

[待定]
