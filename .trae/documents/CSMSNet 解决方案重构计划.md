# 重构 CSMSNet 解决方案计划

根据您的要求，我将把现有的 OCPP 适配器解决方案重构为一个完整的电动汽车充电云平台（CSMS）解决方案。

## 1. 解决方案结构重组

我们将把解决方案从单一的适配器模式转变为包含 Web 应用的完整平台。

### 变更概览
- **解决方案重命名**: `CSMSNet.OcppAdapter.sln` -> `CSMSNet.sln`
- **新建核心库**: `CSMSNet.Ocpp` (合并原有的 Core, Protocol, Server 项目)
- **新建 Web 应用**: `CSMSNet.Web` (Blazor Web App, Server + Client 模式)

### 新的目录结构
```text
CSMSNet/
├── src/
│   ├── CSMSNet.Ocpp/           # [新建] 合并后的 OCPP 协议核心库 (包含原 Core/Protocol/Server)
│   ├── CSMSNet.Web/            # [新建] Blazor 服务端项目 (集成 OCPP 适配器)
│   └── CSMSNet.Web.Client/     # [新建] Blazor 客户端项目 (WebAssembly)
├── tests/
│   └── CSMSNet.Ocpp.Tests/     # [重命名] 单元测试项目
└── CSMSNet.sln                 # [重命名] 解决方案文件
```

## 2. 详细执行步骤

### 步骤 1: 代码整合 (CSMSNet.Ocpp)
我们将创建一个新的类库 `CSMSNet.Ocpp`，并将分散的代码整合进去。
- **合并内容**:
    - `Core`: 移动 Models, Abstractions, Configuration 到 `CSMSNet.Ocpp/Models`, `CSMSNet.Ocpp/Abstractions` 等。
    - `Protocol`: 移动协议处理逻辑到 `CSMSNet.Ocpp/Protocol`。
    - `Server`: 移动 WebSocket 处理和连接管理到 `CSMSNet.Ocpp/Server`。
- **命名空间调整**: 统一调整为 `CSMSNet.Ocpp.*`。
- **依赖项**: 添加 `Microsoft.AspNetCore.App` 框架引用以支持 WebSocket 和 HttpContext。

### 步骤 2: 创建 Web 应用 (CSMSNet.Web)
创建一个现代化的 Blazor Web App。
- **技术栈**: .NET 8 Blazor Web App (Interactive Auto 模式，包含 Server 和 Client)。
- **功能**:
    - 作为 OCPP 服务器的主机。
    - 提供用户界面（充电桩管理、监控等）。
- **集成**:
    - 在 `Program.cs` 中注册 OCPP 服务: `builder.Services.AddOcppAdapter()`。
    - 配置中间件路径: 使用 `app.Map("/ocpp", b => b.UseOcppAdapter())` 确保不干扰 Blazor 的 SignalR 连接。

### 步骤 3: 清理与迁移
- 删除旧的 `CSMSNet.OcppAdapter.*` 项目文件和文件夹。
- 更新解决方案文件 (.sln)。
- 更新单元测试项目的引用。

### 步骤 4: 文档更新
- 更新 `README.md` 和其他文档，反映新的架构和使用说明。

## 3. 验证计划
- 编译通过所有项目。
- 运行单元测试确保逻辑未破坏。
- 启动 Web 应用，验证：
    1.  Blazor 页面能否正常访问。
    2.  OCPP 适配器能否在 `/ocpp` 路径下接受 WebSocket 连接。

请确认是否按照此计划执行？
