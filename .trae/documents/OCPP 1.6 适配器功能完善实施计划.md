# OCPP 1.6 适配器功能完善计划

根据对现有代码和文档的全面分析，目前项目已完成基础框架、核心消息定义和基础连接管理，但在**业务集成能力**（事件触发）、**Web服务集成**（ASP.NET Core Middleware）以及**完整协议覆盖**（剩余消息类型）方面存在缺失。

本计划将针对这些缺失部分进行补全，使适配器具备完整的生产级使用能力。

## 1. 完善事件驱动机制 (优先级: 高)
目前 `RequestHandler` 仅返回默认响应，未触发业务事件，导致上层应用无法处理业务逻辑。
- **目标**: 打通消息处理与事件通知的链路。
- **实施步骤**:
    1.  修改 `IRequestHandler` 接口，定义与 `IOcppAdapter` 对应的事件委托或事件成员。
    2.  重构 `RequestHandler` 类，在处理各类请求（如 `BootNotification`, `StartTransaction` 等）时，触发相应事件。
    3.  实现事件等待机制：支持等待用户代码处理事件并设置响应结果（利用 `TaskCompletionSource`）。
    4.  在 `OcppAdapter` 构造函数中订阅 `RequestHandler` 的事件，并将其转发给公共事件订阅者。

## 2. 实现 ASP.NET Core 集成组件 (优先级: 高)
目前 `WebSocketServer` 类虽然存在，但缺少接入 ASP.NET Core 管道的胶水代码。
- **目标**: 提供标准的 Middleware 和 Extension Methods，支持一行代码集成。
- **实施步骤**:
    1.  创建 `OcppWebSocketMiddleware` 类，封装 `WebSocketServer.HandleWebSocketAsync` 调用。
    2.  创建 `OcppAdapterExtensions` 类，提供 `AddOcppAdapter()` (DI注册) 和 `UseOcppAdapter()` (中间件配置) 方法。
    3.  确保 DI 容器正确注入 `IOcppAdapter`, `IConnectionManager` 等核心服务。

## 3. 补全剩余 OCPP 1.6 消息模型 (优先级: 中)
根据 JSON Schema 补全尚未实现的 Priority 2 & 3 消息模型。
- **目标**: 实现 100% OCPP 1.6 Core, Firmware, Smart Charging, Remote Trigger 消息覆盖。
- **待实现消息对**:
    - **Smart Charging**: `SetChargingProfile`, `ClearChargingProfile`, `GetCompositeSchedule`
    - **Reservation**: `ReserveNow`, `CancelReservation`
    - **Firmware Management**: `UpdateFirmware`, `FirmwareStatusNotification`, `GetDiagnostics`, `DiagnosticsStatusNotification`
    - **Local List**: `SendLocalList`, `GetLocalListVersion`
    - **Other**: `ChangeAvailability`, `TriggerMessage`

## 4. 激活后台监控服务 (优先级: 中)
目前连接管理逻辑已存在，但缺乏自动触发清理的定时任务。
- **目标**: 自动检测并断开僵尸连接。
- **实施步骤**:
    1.  在 `OcppAdapter` 中引入 `Microsoft.Extensions.Hosting.BackgroundService` 或内部 `Timer`。
    2.  定期调用 `ConnectionManager.CleanupInactiveSessions`。
    3.  实现心跳超时检测逻辑（根据 `LastActivity` 时间）。

## 5. 验证与测试
- **目标**: 确保新功能正常工作。
- **实施步骤**:
    1.  编写单元测试验证新添加的消息模型的序列化/反序列化。
    2.  编写集成测试模拟 `StartTransaction` 流程，验证事件是否正确触发并返回用户设定的响应。

执行顺序建议：先完成 **事件机制** 和 **Middleware**，这是系统可用的基础；随后补全 **消息模型**；最后添加 **后台监控**。