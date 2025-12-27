# OCPP 1.6适配器开发实施指南

## 当前完成进度

### 已完成

#### 阶段一：基础框架搭建 ✅
1. **解决方案结构**
   - CSMSNet.OcppAdapter.Core - 核心类库
   - CSMSNet.OcppAdapter.Protocol - 协议处理
   - CSMSNet.OcppAdapter.Server - 服务实现
   - CSMSNet.OcppAdapter.Tests - 单元测试

2. **核心基础设施**
   - [x] OcppAdapterConfiguration - 配置类
   - [x] OcppException系列 - 异常处理
   - [x] IOcppMessage, OcppRequest, OcppResponse - 消息基类
   - [x] IOcppProtocolHandler - 协议处理器接口
   - [x] IStateCache - 状态缓存接口
   - [x] IOcppAdapter - 适配器主接口(部分)
   - [x] ChargePointState模型 - 状态数据模型
   - [x] OCPP 1.6 所有枚举类型
   - [x] OCPP 1.6 公共复合类型

3. **示例消息对象**
   - [x] BootNotificationRequest/Response

### 待完成

#### 阶段二：OCPP 1.6消息对象完整实现

根据`protocols/OCPP_1.6_documentation/schemas/json/`目录下的JSON Schema,需要实现以下消息对象:

##### 充电桩发起的消息(Call from Charge Point)
- [ ] AuthorizeRequest/Response
- [ ] HeartbeatRequest/Response  
- [ ] StatusNotificationRequest/Response
- [ ] MeterValuesRequest/Response
- [ ] StartTransactionRequest/Response
- [ ] StopTransactionRequest/Response
- [ ] DataTransferRequest/Response
- [ ] DiagnosticsStatusNotificationRequest/Response (安全扩展)
- [ ] FirmwareStatusNotificationRequest/Response

##### 系统发起的消息(Call from Central System)
- [ ] RemoteStartTransactionRequest/Response
- [ ] RemoteStopTransactionRequest/Response
- [ ] GetConfigurationRequest/Response
- [ ] ChangeConfigurationRequest/Response
- [ ] ResetRequest/Response
- [ ] UnlockConnectorRequest/Response
- [ ] ClearCacheRequest/Response
- [ ] ChangeAvailabilityRequest/Response
- [ ] SetChargingProfileRequest/Response
- [ ] ClearChargingProfileRequest/Response
- [ ] GetCompositeScheduleRequest/Response
- [ ] ReserveNowRequest/Response
- [ ] CancelReservationRequest/Response
- [ ] SendLocalListRequest/Response
- [ ] GetLocalListVersionRequest/Response
- [ ] UpdateFirmwareRequest/Response
- [ ] GetDiagnosticsRequest/Response
- [ ] TriggerMessageRequest/Response

## 消息对象创建模板

### 请求对象模板

```csharp
using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// {MessageName}请求
/// </summary>
public class {MessageName}Request : OcppRequest
{
    public override string Action => "{MessageName}";

    // 根据JSON Schema定义属性
    // 必填字段不使用可空类型
    // 可选字段使用可空类型
    // 使用JsonPropertyName特性指定JSON属性名(camelCase)
    
    /// <summary>
    /// {属性说明}
    /// </summary>
    [JsonPropertyName("{camelCasePropertyName}")]
    public {Type} PropertyName { get; set; } = default!;
}
```

### 响应对象模板

```csharp
using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// {MessageName}响应
/// </summary>
public class {MessageName}Response : OcppResponse
{
    // 根据JSON Schema定义属性
    
    /// <summary>
    /// {属性说明}
    /// </summary>
    [JsonPropertyName("{camelCasePropertyName}")]
    public {Type} PropertyName { get; set; } = default!;
}
```

## 创建步骤

### 1. 读取JSON Schema
从`protocols/OCPP_1.6_documentation/schemas/json/`读取对应的schema文件

### 2. 映射类型
- `"type": "string"` → `string`
- `"type": "integer"` → `int`
- `"type": "number"` → `decimal`
- `"type": "boolean"` → `bool`
- `"type": "string", "format": "date-time"` → `DateTime`
- `"type": "array"` → `List<T>`
- `"type": "object"` → 自定义类或已有复合类型
- `"enum"` → 使用Enums命名空间中的枚举

### 3. 处理必填和可选
- `"required"`数组中的字段 → 不可空类型
- 不在`"required"`中的字段 → 可空类型(`?`)

### 4. 文件组织
- 请求类: `Models/V16/Requests/{MessageName}Request.cs`
- 响应类: `Models/V16/Responses/{MessageName}Response.cs`

## 下一步实施计划

### 优先级1：核心业务消息(必须实现)
1. Authorize
2. Heartbeat
3. StatusNotification
4. MeterValues
5. StartTransaction
6. StopTransaction

### 优先级2：核心控制指令(必须实现)
1. RemoteStartTransaction
2. RemoteStopTransaction
3. Reset
4. UnlockConnector
5. GetConfiguration
6. ChangeConfiguration
7. ClearCache

### 优先级3：扩展功能(建议实现)
1. ChangeAvailability
2. SetChargingProfile
3. ClearChargingProfile
4. GetCompositeSchedule
5. ReserveNow
6. CancelReservation
7. SendLocalList
8. GetLocalListVersion
9. UpdateFirmware
10. TriggerMessage

### 优先级4：诊断功能(可选实现)
1. GetDiagnostics
2. DiagnosticsStatusNotification
3. FirmwareStatusNotification

## 阶段三：协议处理器实现

在`CSMSNet.OcppAdapter.Protocol`项目中实现:

### 文件结构
```
Protocol/
├── V16/
│   ├── Ocpp16ProtocolHandler.cs      // 实现IOcppProtocolHandler
│   ├── Ocpp16MessageSerializer.cs    // JSON序列化/反序列化
│   ├── Ocpp16MessageValidator.cs     // Schema验证
│   ├── Ocpp16Constants.cs            // 常量定义
│   └── Schemas/                      // 复制JSON Schema文件(嵌入资源)
```

### 核心实现点
1. **消息序列化**
   - 使用System.Text.Json
   - 配置JsonSerializerOptions(camelCase命名,忽略null值)
   - 支持OCPP消息格式`[MessageType, MessageId, Action, Payload]`

2. **消息验证**
   - 使用JsonSchema.Net库
   - 从嵌入资源加载Schema文件
   - 验证消息格式完整性

3. **类型映射**
   - 维护Action到Request/Response类型的映射字典
   - 支持动态反序列化到正确类型

## 阶段四：WebSocket服务器实现

在`CSMSNet.OcppAdapter.Server`项目中实现:

### 文件结构
```
Server/
├── OcppAdapter.cs                    // IOcppAdapter实现
├── Transport/
│   ├── WebSocketServer.cs           // ASP.NET Core WebSocket服务器
│   ├── WebSocketSession.cs          // 单个连接会话
│   └── MessageSerializer.cs         // 消息序列化辅助
├── Handlers/
│   ├── MessageRouter.cs             // 消息路由分发
│   ├── RequestHandler.cs            // 处理充电桩请求
│   ├── ResponseHandler.cs           // 处理充电桩响应
│   └── CallMatcher.cs               // 请求-响应匹配
├── State/
│   ├── StateCache.cs                // IStateCache实现
│   ├── ConnectionManager.cs         // 连接管理
│   └── SessionManager.cs            // 会话管理
└── Utils/
    ├── TimeoutHelper.cs             // 超时控制辅助
    └── IdGenerator.cs               // MessageId生成
```

### 关键技术点
1. **WebSocket服务器**
   - 使用ASP.NET Core Middleware
   - 支持OCPP子协议协商(ocpp1.6)
   - 路由提取充电桩ID(URL路径或查询参数)

2. **消息处理流程**
   - 接收WebSocket文本消息
   - 解析OCPP消息格式`[MessageType, MessageId, ...]`
   - 路由到对应处理器
   - 异步处理并返回响应

3. **超时控制**
   - 使用Task.WhenAny实现超时
   - CancellationTokenSource控制取消
   - 超时自动生成默认响应

## 阶段五：事件机制实现

### 事件参数定义
在`Core/Models/Events/`创建事件参数类:

```csharp
public class StartTransactionEventArgs : EventArgs
{
    public string ChargePointId { get; set; }
    public StartTransactionRequest Request { get; set; }
    public TaskCompletionSource<StartTransactionResponse> ResponseTask { get; set; }
}
```

### 事件定义
在`IOcppAdapter`接口中添加:

```csharp
event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected;
event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected;
event EventHandler<BootNotificationEventArgs>? OnBootNotification;
event EventHandler<StatusNotificationEventArgs>? OnStatusNotification;
event EventHandler<MeterValuesEventArgs>? OnMeterValues;
event EventHandler<StartTransactionEventArgs>? OnStartTransaction;
event EventHandler<StopTransactionEventArgs>? OnStopTransaction;
event EventHandler<AuthorizeEventArgs>? OnAuthorize;
event EventHandler<DataTransferEventArgs>? OnDataTransfer;
```

## 阶段六：指令接口实现

在`IOcppAdapter`接口中添加指令方法:

```csharp
Task<RemoteStartTransactionResponse> RemoteStartTransactionAsync(
    string chargePointId, 
    RemoteStartTransactionRequest request, 
    CancellationToken cancellationToken = default);

Task<RemoteStopTransactionResponse> RemoteStopTransactionAsync(
    string chargePointId, 
    RemoteStopTransactionRequest request, 
    CancellationToken cancellationToken = default);

// ... 其他指令接口
```

## 测试策略

### 单元测试
1. **消息序列化测试** - 验证JSON往返转换
2. **协议验证测试** - 验证Schema校验
3. **状态缓存测试** - 验证状态更新逻辑
4. **超时控制测试** - 验证超时机制

### 集成测试
1. **模拟充电桩客户端** - 测试消息收发
2. **并发连接测试** - 测试多桩场景
3. **异常场景测试** - 测试错误处理

## 参考资料位置

- OCPP 1.6规范: `protocols/OCPP_1.6_documentation/ocpp-1.6 edition 2.pdf`
- OCPP-J规范: `protocols/OCPP_1.6_documentation/ocpp-j-1.6-specification.pdf`
- JSON Schemas: `protocols/OCPP_1.6_documentation/schemas/json/`
- 设计文档: `.qoder/quests/ocpp-adapter-development.md`

## 后续扩展

### OCPP 2.x支持
1. 在`Protocol/V20/`创建OCPP 2.0处理器
2. 在`Core/Models/V20/`创建OCPP 2.0消息对象
3. 适配器根据协议版本选择对应处理器

### 外部集成
1. 添加持久化接口,支持外部状态存储
2. 添加认证接口,支持自定义身份验证
3. 添加监控指标接口,支持Prometheus等监控系统
