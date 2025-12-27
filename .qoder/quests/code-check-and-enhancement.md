# OCPP适配器代码完整性检查与功能补充设计

## 一、问题诊断

### 当前实施状态

#### 已完成部分
- ✅ 项目结构与依赖配置
- ✅ 核心数据模型(枚举、复合类型、消息对象)
- ✅ 协议处理器(Ocpp16ProtocolHandler)
- ✅ 状态缓存(StateCache)
- ✅ 接口定义(IOcppAdapter, IOcppProtocolHandler, IStateCache)

#### 缺失部分(高优先级)
1. **WebSocket通信层** - 完全缺失
2. **消息路由与处理器** - 完全缺失
3. **请求-响应匹配器** - 完全缺失
4. **事件机制** - 仅有接口注释
5. **指令调用接口** - 仅有接口注释
6. **连接管理** - 完全缺失
7. **超时控制机制** - 完全缺失
8. **心跳检测机制** - 完全缺失

### 影响评估

| 缺失模块 | 严重程度 | 影响范围 | 优先级 |
|---------|---------|---------|--------|
| WebSocket通信层 | 极高 | 系统无法运行 | P0 |
| 消息路由与处理器 | 极高 | 消息无法处理 | P0 |
| 请求-响应匹配器 | 高 | 指令调用无法实现 | P0 |
| 事件机制 | 高 | 业务集成受限 | P1 |
| 指令调用接口 | 高 | 核心功能缺失 | P1 |
| 连接管理 | 中 | 影响稳定性 | P1 |
| 超时控制 | 中 | 影响可靠性 | P2 |
| 心跳检测 | 中 | 影响在线监测 | P2 |

---

## 二、功能补充设计

### 模块一:WebSocket通信层

#### 设计目标
建立CSMS与充电桩之间的双向WebSocket通信通道,支持OCPP子协议协商、连接管理、消息收发。

#### 核心组件

##### 1. WebSocketServer
**职责**: WebSocket服务器主入口,基于ASP.NET Core中间件

**功能清单**:
- 监听指定URL,接受WebSocket连接请求
- 子协议协商(Sec-WebSocket-Protocol: ocpp1.6)
- 从URL路径或查询参数提取充电桩ID
- 创建并管理WebSocket会话
- 服务器生命周期管理(启动、停止、优雅关闭)

**关键接口方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| StartAsync | CancellationToken | Task | 启动WebSocket服务器 |
| StopAsync | CancellationToken | Task | 停止服务器并断开所有连接 |
| AcceptConnection | HttpContext | Task | 接受并升级WebSocket连接 |
| ExtractChargePointId | HttpContext | string | 从请求中提取充电桩ID |

**配置参数**:
- ListenUrl: 监听地址(如http://+:8080/)
- AllowedSubProtocols: 允许的子协议列表(ocpp1.6, ocpp1.5)
- MaxConcurrentConnections: 最大连接数限制
- ConnectionTimeout: 连接建立超时时间

##### 2. WebSocketSession
**职责**: 单个充电桩的WebSocket连接会话管理

**状态属性**:
| 属性 | 类型 | 说明 |
|-----|------|------|
| SessionId | string | 会话唯一标识(GUID) |
| ChargePointId | string | 充电桩ID |
| WebSocket | WebSocket | 底层WebSocket对象 |
| State | SessionState | 会话状态(Connecting/Connected/Closing/Closed) |
| ProtocolVersion | string | 协商的协议版本 |
| ConnectedAt | DateTime | 连接建立时间 |
| LastActivityAt | DateTime | 最后活动时间 |

**核心方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| StartReceiving | - | Task | 启动接收循环 |
| SendMessageAsync | string message | Task | 发送文本消息 |
| CloseAsync | WebSocketCloseStatus, string | Task | 关闭连接 |
| UpdateActivity | - | void | 更新最后活动时间 |

**接收循环逻辑**:
1. 循环监听WebSocket接收缓冲区
2. 读取完整文本消息(处理分帧)
3. 更新LastActivityAt时间戳
4. 将消息转发至消息路由器
5. 捕获异常,触发连接断开事件

**异常处理**:
- 连接意外断开: 触发OnDisconnected事件,清理会话
- 消息格式错误: 记录日志,发送CallError响应
- 网络超时: 自动重连或标记离线

##### 3. ConnectionManager
**职责**: 管理所有活跃的WebSocket连接会话

**数据结构**:
```
ConcurrentDictionary<string, WebSocketSession> sessions
  Key: ChargePointId
  Value: WebSocketSession实例
```

**核心方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| AddSession | WebSocketSession | bool | 添加新会话 |
| RemoveSession | string chargePointId | bool | 移除会话 |
| GetSession | string chargePointId | WebSocketSession? | 获取会话 |
| GetAllSessions | - | List\<WebSocketSession\> | 获取所有会话 |
| CloseAllSessions | - | Task | 关闭所有连接 |

**连接限制策略**:
- 检查当前连接数是否超过MaxConcurrentConnections
- 同一充电桩ID重复连接时,关闭旧连接或拒绝新连接
- 提供连接数监控接口

**会话清理**:
- 定期检查无活动会话(基于LastActivityAt)
- 超时会话自动断开并清理
- 资源释放(WebSocket.Dispose)

#### 消息收发流程

##### 接收流程
```
充电桩 → WebSocket → WebSocketSession.ReceiveLoop 
       → MessageRouter.RouteIncoming 
       → 协议解析 
       → 消息处理器
```

##### 发送流程
```
业务层 → OcppAdapter.SendCommandAsync 
       → ConnectionManager.GetSession 
       → WebSocketSession.SendMessageAsync 
       → WebSocket.SendAsync 
       → 充电桩
```

#### ASP.NET Core集成方案

**中间件管道配置**:
1. 在Program.cs或Startup.cs中注册WebSocket中间件
2. 配置路由规则(如/ocpp/{chargePointId})
3. 升级HTTP连接为WebSocket
4. 传递给WebSocketServer处理

**示例中间件结构**:
- 检查请求路径是否匹配OCPP路由
- 验证子协议头(Sec-WebSocket-Protocol)
- 调用WebSocketServer.AcceptConnection
- 创建WebSocketSession并加入ConnectionManager

---

### 模块二:消息路由与处理器

#### 设计目标
实现消息的智能路由分发,根据消息类型(Call/CallResult/CallError)和Action将消息路由到正确的处理器。

#### 核心组件

##### 1. MessageRouter
**职责**: 消息路由分发中心

**路由逻辑**:
| 消息类型 | MessageTypeId | 路由目标 | 说明 |
|---------|--------------|---------|------|
| Call | 2 | RequestHandler | 处理充电桩发起的请求 |
| CallResult | 3 | CallMatcher | 匹配待处理的Call |
| CallError | 4 | CallMatcher | 匹配待处理的Call |

**核心方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| RouteIncoming | string chargePointId, string json | Task | 路由入站消息 |
| RouteResponse | string chargePointId, IOcppMessage | Task | 路由响应消息 |

**处理流程**:
1. 使用协议处理器反序列化JSON
2. 根据MessageType判断消息类型
3. Call消息: 转发至RequestHandler
4. CallResult/CallError: 转发至CallMatcher
5. 捕获路由异常,发送CallError响应

##### 2. RequestHandler
**职责**: 处理充电桩发起的业务请求(Call消息)

**处理流程**:
1. 接收Call消息(如BootNotification)
2. 更新StateCache状态(根据业务类型)
3. 触发业务事件(如OnBootNotification)
4. 等待业务层处理结果(通过TaskCompletionSource)
5. 构造CallResult响应
6. 通过WebSocketSession发送响应

**超时处理**:
- 使用Task.WhenAny实现超时控制
- 超时时间从OcppAdapterConfiguration读取
- 超时后返回默认响应或错误响应

**消息类型处理映射表**:
| Action | 状态更新 | 触发事件 | 响应构造 |
|--------|---------|---------|---------|
| BootNotification | UpdateChargePointInfo, MarkOnline | OnBootNotification | BootNotificationResponse |
| StatusNotification | UpdateConnectorStatus | OnStatusNotification | StatusNotificationResponse |
| StartTransaction | CreateTransaction | OnStartTransaction | StartTransactionResponse |
| StopTransaction | EndTransaction | OnStopTransaction | StopTransactionResponse |
| Heartbeat | UpdateLastCommunication | OnHeartbeat | HeartbeatResponse |
| Authorize | - | OnAuthorize | AuthorizeResponse |
| MeterValues | UpdateTransactionMeter | OnMeterValues | MeterValuesResponse |
| DataTransfer | - | OnDataTransfer | DataTransferResponse |

**默认响应策略**:
- BootNotification: Accepted状态,Interval=300秒,当前UTC时间
- StatusNotification/MeterValues: 空响应
- Authorize: 默认Accepted或根据配置返回
- StartTransaction: 生成TransactionId,返回IdTagInfo
- StopTransaction: 返回IdTagInfo

##### 3. CommandHandler
**职责**: 处理系统下发的指令(由业务层主动发起)

**核心方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| SendCommandAsync\<TReq, TRes\> | string chargePointId, TReq request, CancellationToken | Task\<TRes\> | 发送指令并等待响应 |

**执行流程**:
1. 检查充电桩是否在线(ConnectionManager)
2. 生成MessageId(GUID)
3. 序列化为Call消息
4. 注册到CallMatcher(等待响应)
5. 通过WebSocketSession发送消息
6. 异步等待CallResult(使用TaskCompletionSource)
7. 超时检查(CommandResponseTimeout)
8. 反序列化响应并返回

**支持的指令**:
- RemoteStartTransaction
- RemoteStopTransaction
- Reset
- UnlockConnector
- GetConfiguration
- ChangeConfiguration
- ClearCache

---

### 模块三:请求-响应匹配器

#### 设计目标
实现Call消息与CallResult/CallError的配对,支持异步等待响应。

#### 核心组件

##### CallMatcher
**职责**: 管理待响应的Call消息,匹配返回的CallResult

**数据结构**:
```
ConcurrentDictionary<string, PendingCall> pendingCalls
  Key: MessageId
  Value: PendingCall对象
```

**PendingCall结构**:
| 字段 | 类型 | 说明 |
|-----|------|------|
| MessageId | string | 消息唯一标识 |
| ChargePointId | string | 充电桩ID |
| Action | string | 消息Action |
| Request | OcppRequest | 原始请求对象 |
| ResponseTask | TaskCompletionSource\<IOcppMessage\> | 响应等待句柄 |
| CreatedAt | DateTime | 创建时间 |
| Timeout | TimeSpan | 超时时间 |
| CancellationToken | CancellationTokenSource | 取消令牌 |

**核心方法**:
| 方法 | 参数 | 返回值 | 说明 |
|-----|------|-------|------|
| RegisterCall | PendingCall | Task\<IOcppMessage\> | 注册待响应Call |
| MatchResponse | string messageId, IOcppMessage | bool | 匹配响应消息 |
| CancelCall | string messageId | void | 取消等待 |
| CleanupExpired | - | void | 清理超时Call |

**匹配流程**:
1. 接收CallResult/CallError消息
2. 从pendingCalls中查找对应MessageId
3. 使用协议处理器反序列化为强类型响应
4. 通过TaskCompletionSource.SetResult返回结果
5. 从pendingCalls移除已匹配项

**超时处理**:
- 后台定时任务(Timer)每30秒清理过期Call
- 检查CreatedAt + Timeout是否超过当前时间
- 超时Call触发TaskCompletionSource.SetException
- 记录超时日志

**取消机制**:
- 支持CancellationToken取消等待
- 取消时从pendingCalls移除
- 触发TaskCompletionSource.SetCanceled

---

### 模块四:事件机制

#### 设计目标
为业务层提供充电桩事件通知机制,支持异步处理和响应控制。

#### 事件参数设计

##### 事件参数基类
| 字段 | 类型 | 说明 |
|-----|------|------|
| ChargePointId | string | 充电桩ID |
| Timestamp | DateTime | 事件时间戳 |
| SessionId | string | WebSocket会话ID |

##### 业务事件参数

###### ChargePointConnectedEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| ProtocolVersion | string | 协议版本 |
| RemoteEndpoint | string | 远程IP地址 |

###### ChargePointDisconnectedEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| DisconnectReason | string | 断开原因 |
| ConnectionDuration | TimeSpan | 连接持续时长 |

###### BootNotificationEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | BootNotificationRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<BootNotificationResponse\> | 响应控制 |

###### StatusNotificationEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | StatusNotificationRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<StatusNotificationResponse\> | 响应控制 |

###### StartTransactionEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | StartTransactionRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<StartTransactionResponse\> | 响应控制 |

###### StopTransactionEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | StopTransactionRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<StopTransactionResponse\> | 响应控制 |

###### AuthorizeEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | AuthorizeRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<AuthorizeResponse\> | 响应控制 |

###### MeterValuesEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | MeterValuesRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<MeterValuesResponse\> | 响应控制 |

###### HeartbeatEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | HeartbeatRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<HeartbeatResponse\> | 响应控制 |

###### DataTransferEventArgs
| 字段 | 类型 | 说明 |
|-----|------|------|
| Request | DataTransferRequest | 请求对象 |
| ResponseTask | TaskCompletionSource\<DataTransferResponse\> | 响应控制 |

#### 事件定义

在IOcppAdapter接口中添加:
```
event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected
event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected
event EventHandler<BootNotificationEventArgs>? OnBootNotification
event EventHandler<StatusNotificationEventArgs>? OnStatusNotification
event EventHandler<StartTransactionEventArgs>? OnStartTransaction
event EventHandler<StopTransactionEventArgs>? OnStopTransaction
event EventHandler<AuthorizeEventArgs>? OnAuthorize
event EventHandler<MeterValuesEventArgs>? OnMeterValues
event EventHandler<HeartbeatEventArgs>? OnHeartbeat
event EventHandler<DataTransferEventArgs>? OnDataTransfer
```

#### 事件触发流程

##### 同步事件(连接状态)
1. WebSocketSession建立连接时触发OnChargePointConnected
2. 直接调用事件处理器,不等待返回
3. 记录异常但不影响主流程

##### 异步事件(业务请求)
1. RequestHandler接收到充电桩请求
2. 创建事件参数,包含ResponseTask
3. 触发对应事件(如OnBootNotification)
4. 等待业务层通过ResponseTask.SetResult提供响应
5. 应用超时控制(BusinessEventTimeout)
6. 超时后使用默认响应

#### 默认响应策略

| 事件 | 默认响应 | 说明 |
|-----|---------|------|
| OnBootNotification | Accepted, Interval=300s | 允许充电桩注册 |
| OnAuthorize | Accepted | 默认通过授权 |
| OnStartTransaction | 生成TransactionId, Accepted | 允许启动事务 |
| OnStopTransaction | Accepted | 确认事务结束 |
| OnStatusNotification | 空响应 | 仅确认接收 |
| OnMeterValues | 空响应 | 仅确认接收 |
| OnHeartbeat | 当前UTC时间 | 心跳响应 |
| OnDataTransfer | Rejected | 默认拒绝 |

---

### 模块五:指令调用接口

#### 设计目标
为业务层提供向充电桩下发指令的接口,支持异步调用和超时控制。

#### IOcppAdapter接口扩展

##### 指令方法定义

###### RemoteStartTransaction
```
参数:
  - chargePointId: string (充电桩ID)
  - request: RemoteStartTransactionRequest (包含IdTag, ConnectorId等)
  - cancellationToken: CancellationToken (可选,默认default)
返回值:
  - Task<RemoteStartTransactionResponse>
异常:
  - OcppConnectionException: 充电桩离线
  - OcppTimeoutException: 响应超时
  - OperationCanceledException: 操作取消
```

###### RemoteStopTransaction
```
参数:
  - chargePointId: string
  - request: RemoteStopTransactionRequest (包含TransactionId)
  - cancellationToken: CancellationToken
返回值:
  - Task<RemoteStopTransactionResponse>
```

###### Reset
```
参数:
  - chargePointId: string
  - request: ResetRequest (包含ResetType: Hard/Soft)
  - cancellationToken: CancellationToken
返回值:
  - Task<ResetResponse>
```

###### UnlockConnector
```
参数:
  - chargePointId: string
  - request: UnlockConnectorRequest (包含ConnectorId)
  - cancellationToken: CancellationToken
返回值:
  - Task<UnlockConnectorResponse>
```

###### GetConfiguration
```
参数:
  - chargePointId: string
  - request: GetConfigurationRequest (可选Key列表)
  - cancellationToken: CancellationToken
返回值:
  - Task<GetConfigurationResponse>
```

###### ChangeConfiguration
```
参数:
  - chargePointId: string
  - request: ChangeConfigurationRequest (Key, Value)
  - cancellationToken: CancellationToken
返回值:
  - Task<ChangeConfigurationResponse>
```

###### ClearCache
```
参数:
  - chargePointId: string
  - request: ClearCacheRequest (空请求)
  - cancellationToken: CancellationToken
返回值:
  - Task<ClearCacheResponse>
```

###### DataTransfer(系统发起)
```
参数:
  - chargePointId: string
  - request: DataTransferRequest (VendorId, MessageId, Data)
  - cancellationToken: CancellationToken
返回值:
  - Task<DataTransferResponse>
```

#### 调用流程

1. 业务层调用指令方法(如RemoteStartTransactionAsync)
2. OcppAdapter委托给CommandHandler
3. CommandHandler检查连接状态
4. 生成MessageId并序列化为Call消息
5. 注册到CallMatcher
6. 通过WebSocketSession发送消息
7. 等待CallResult(使用Task.WhenAny超时控制)
8. 超时检查(CommandResponseTimeout,默认60秒)
9. 反序列化响应并返回

#### 错误处理

| 错误场景 | 异常类型 | 错误信息 |
|---------|---------|---------|
| 充电桩离线 | OcppConnectionException | "Charge point {id} is not connected" |
| 响应超时 | OcppTimeoutException | "Command timeout after {timeout}s" |
| 取消操作 | OperationCanceledException | - |
| CallError响应 | OcppProtocolException | 包含ErrorCode和Description |
| 网络异常 | OcppConnectionException | 包含InnerException |

---

### 模块六:连接管理增强

#### 设计目标
增强连接管理能力,支持连接限制、会话清理、健康检查。

#### 功能清单

##### 1. 连接数限制
- 检查当前连接数是否超过MaxConcurrentConnections
- 超过限制时拒绝新连接
- 提供当前连接数查询接口

##### 2. 重复连接处理
**策略选项**:
- Replace: 关闭旧连接,接受新连接(默认)
- Reject: 拒绝新连接,保持旧连接
- Duplicate: 允许同一ID多连接(不推荐)

**实现逻辑**:
- 检查ChargePointId是否已存在连接
- 根据配置策略处理
- 记录连接替换日志

##### 3. 会话清理
**定时清理任务**(每分钟执行):
- 遍历所有会话
- 检查LastActivityAt是否超过阈值(如30分钟无活动)
- 关闭并移除超时会话
- 更新StateCache离线状态

##### 4. 连接监控
**监控指标**:
- 当前连接数
- 活跃会话数(近期有消息交互)
- 历史连接总数
- 连接失败次数
- 平均连接时长

**查询接口**:
- GetConnectionMetrics(): ConnectionMetrics
- GetActiveSessionCount(): int
- GetSessionInfo(chargePointId): SessionInfo

---

### 模块七:超时控制机制

#### 设计目标
实现统一的超时控制机制,避免资源泄漏和无限等待。

#### 超时场景

| 场景 | 配置项 | 默认值 | 说明 |
|-----|--------|-------|------|
| BootNotification等待 | BootNotificationTimeout | 60秒 | 充电桩连接后必须发送 |
| 业务事件处理 | BusinessEventTimeout | 30秒 | 业务层处理事件超时 |
| 指令响应等待 | CommandResponseTimeout | 60秒 | 发送指令后等待响应 |
| WebSocket消息接收 | MessageReceiveTimeout | 300秒 | 单条消息接收超时 |
| 连接建立 | ConnectionTimeout | 30秒 | WebSocket握手超时 |

#### 实现方式

##### Task.WhenAny模式
```
伪代码:
var timeoutTask = Task.Delay(timeout, cancellationToken)
var businessTask = ProcessAsync()
var completedTask = await Task.WhenAny(businessTask, timeoutTask)

if (completedTask == timeoutTask):
    使用默认响应或抛出超时异常
else:
    返回业务处理结果
```

##### CancellationTokenSource
- 为每个超时操作创建CancellationTokenSource
- 设置CancelAfter(timeout)
- 操作完成后Dispose资源

##### 超时后处理

**业务事件超时**:
- 使用默认响应(如BootNotification返回Accepted)
- 记录警告日志
- 不中断连接

**指令超时**:
- 抛出OcppTimeoutException
- 从CallMatcher移除待匹配项
- 业务层捕获异常处理

**连接超时**:
- 关闭WebSocket连接
- 清理会话资源
- 记录错误日志

---

### 模块八:心跳检测机制

#### 设计目标
实现双向心跳检测,及时发现失活连接,维护准确的在线状态。

#### 心跳机制

##### 充电桩 → CSMS (被动心跳)
- 充电桩定期发送Heartbeat请求
- 间隔由BootNotification响应的Interval指定(默认300秒)
- CSMS接收后更新LastCommunicationAt
- 返回HeartbeatResponse(当前UTC时间)

##### CSMS → 充电桩 (主动检测)
**定时任务**(每分钟执行):
1. 遍历所有在线充电桩
2. 检查LastCommunicationAt距离当前时间
3. 超过HeartbeatInterval * 2视为失活
4. 尝试发送TriggerMessage(Heartbeat)或直接标记离线

##### 离线判定逻辑
```
if (当前时间 - LastCommunicationAt > HeartbeatInterval * 2):
    标记充电桩离线(MarkOffline)
    触发OnChargePointDisconnected事件
    保留StateCache数据(不清除)
```

##### 重连处理
- 充电桩重新连接后发送BootNotification
- 更新在线状态(MarkOnline)
- 触发OnChargePointConnected事件
- 恢复通信

---

## 三、数据流图

### 充电桩上线流程
```
充电桩 → WebSocket连接 → WebSocketServer.AcceptConnection
      → 创建WebSocketSession → ConnectionManager.AddSession
      → StateCache.MarkOnline
      → 触发OnChargePointConnected事件

充电桩 → BootNotification → WebSocketSession接收
      → MessageRouter → RequestHandler
      → 触发OnBootNotification事件 → 业务层处理
      → 返回BootNotificationResponse → WebSocketSession发送
      → StateCache.UpdateChargePointInfo
```

### 启动充电事务流程
```
充电桩 → StatusNotification(Preparing) → 更新连接器状态
      → 触发OnStatusNotification

充电桩 → Authorize → 触发OnAuthorize → 业务层验证
      → 返回AuthorizeResponse(Accepted)

充电桩 → StartTransaction → 触发OnStartTransaction
      → 业务层生成TransactionId → StateCache.CreateTransaction
      → 返回StartTransactionResponse

充电桩 → StatusNotification(Charging) → 更新连接器状态

充电桩 → MeterValues(周期性) → 触发OnMeterValues
      → StateCache.UpdateTransactionMeter
```

### 远程启动流程
```
业务层 → OcppAdapter.RemoteStartTransactionAsync
      → CommandHandler.SendCommandAsync
      → 生成MessageId → CallMatcher.RegisterCall
      → 序列化Call消息 → WebSocketSession.SendMessageAsync
      → 等待响应(超时60秒)

充电桩 → CallResult → WebSocketSession接收
      → MessageRouter → CallMatcher.MatchResponse
      → TaskCompletionSource.SetResult
      → 返回RemoteStartTransactionResponse给业务层
```

### 连接断开流程
```
WebSocket异常 → WebSocketSession.ReceiveLoop异常
      → ConnectionManager.RemoveSession
      → StateCache.MarkOffline
      → 触发OnChargePointDisconnected事件
      → 清理CallMatcher中该充电桩的待处理Call
```

---

## 四、接口与数据模型补充

### 新增接口定义

#### IWebSocketServer
```
接口方法:
  - StartAsync(CancellationToken): Task
  - StopAsync(CancellationToken): Task
  - AcceptConnection(HttpContext): Task<WebSocketSession?>
```

#### IConnectionManager
```
接口方法:
  - AddSession(WebSocketSession): bool
  - RemoveSession(string chargePointId): bool
  - GetSession(string chargePointId): WebSocketSession?
  - GetAllSessions(): List<WebSocketSession>
  - CloseAllSessions(): Task
  - GetConnectionCount(): int
```

#### IMessageRouter
```
接口方法:
  - RouteIncoming(string chargePointId, string json): Task
  - RouteResponse(string chargePointId, IOcppMessage message): Task
```

#### ICallMatcher
```
接口方法:
  - RegisterCall(PendingCall): Task<IOcppMessage>
  - MatchResponse(string messageId, IOcppMessage): bool
  - CancelCall(string messageId): void
  - CleanupExpired(): void
```

### 新增数据模型

#### SessionState枚举
```
枚举值:
  - Connecting: 连接建立中
  - Connected: 已连接
  - Closing: 关闭中
  - Closed: 已关闭
```

#### ConnectionMetrics
```
字段:
  - CurrentConnections: int (当前连接数)
  - ActiveSessions: int (活跃会话数)
  - TotalConnectionsEver: int (历史总连接数)
  - FailedConnections: int (连接失败次数)
  - AverageConnectionDuration: TimeSpan (平均连接时长)
```

#### SessionInfo
```
字段:
  - SessionId: string
  - ChargePointId: string
  - ProtocolVersion: string
  - ConnectedAt: DateTime
  - LastActivityAt: DateTime
  - MessagesSent: int
  - MessagesReceived: int
  - State: SessionState
```

---

## 五、实施优先级与依赖关系

### 实施阶段规划

#### 阶段一:通信基础(P0,预计3-5天)
**必须完成**:
1. WebSocketServer实现
2. WebSocketSession实现
3. ConnectionManager实现
4. 基础消息收发测试

**交付物**:
- 可运行的WebSocket服务器
- 充电桩能成功连接
- 能接收和发送JSON消息

#### 阶段二:消息处理(P0,预计3-4天)
**必须完成**:
1. MessageRouter实现
2. RequestHandler实现
3. CallMatcher实现
4. BootNotification处理测试

**交付物**:
- 能正确路由和处理Call消息
- 能返回响应给充电桩
- 请求-响应配对功能正常

#### 阶段三:事件与指令(P1,预计2-3天)
**必须完成**:
1. 事件参数类定义
2. 事件机制实现
3. 指令调用接口实现
4. CommandHandler实现

**交付物**:
- 业务事件能正确触发
- 系统指令能下发并获取响应
- 超时控制生效

#### 阶段四:稳定性增强(P2,预计2-3天)
**建议完成**:
1. 连接管理增强
2. 超时控制完善
3. 心跳检测实现
4. 异常场景处理

**交付物**:
- 连接限制生效
- 超时自动清理
- 离线检测准确
- 异常场景不崩溃

### 模块依赖关系

```
mermaid依赖图:
WebSocketServer → WebSocketSession → ConnectionManager
WebSocketSession → MessageRouter
MessageRouter → RequestHandler
MessageRouter → CallMatcher
RequestHandler → StateCache
RequestHandler → EventSystem
CommandHandler → CallMatcher
CommandHandler → ConnectionManager
OcppAdapter → 所有模块
```

**关键路径**:
WebSocketServer → MessageRouter → RequestHandler (核心流程)

---

## 六、配置参数补充

### OcppAdapterConfiguration新增字段

| 配置项 | 类型 | 默认值 | 说明 |
|-------|------|-------|------|
| DuplicateConnectionStrategy | DuplicateConnectionStrategy | Replace | 重复连接处理策略 |
| SessionInactivityTimeout | TimeSpan | 30分钟 | 会话无活动超时 |
| MessageReceiveTimeout | TimeSpan | 300秒 | 消息接收超时 |
| ConnectionTimeout | TimeSpan | 30秒 | 连接建立超时 |
| EnableHeartbeatMonitoring | bool | true | 启用心跳监控 |
| HeartbeatCheckInterval | TimeSpan | 60秒 | 心跳检测间隔 |
| CallMatcherCleanupInterval | TimeSpan | 30秒 | 清理过期Call间隔 |

### DuplicateConnectionStrategy枚举
```
枚举值:
  - Replace: 关闭旧连接,接受新连接
  - Reject: 拒绝新连接,保持旧连接
  - Duplicate: 允许重复连接(不推荐)
```

---

## 七、日志与监控建议

### 日志记录点

#### 连接层日志
- 充电桩连接成功/失败
- 连接断开(含原因)
- WebSocket异常
- 连接数超限拒绝

#### 消息层日志
- 接收/发送消息(仅记录Action和MessageId,避免大量数据)
- 消息路由异常
- 消息格式错误
- 未知Action

#### 业务层日志
- 事件触发
- 事件处理超时
- 指令发送成功/失败
- 指令响应超时

#### 状态层日志
- 充电桩上线/离线
- 事务启动/停止
- 状态更新异常

### 监控指标

| 指标 | 类型 | 说明 |
|-----|------|------|
| ocpp_connections_current | Gauge | 当前连接数 |
| ocpp_connections_total | Counter | 累计连接数 |
| ocpp_messages_received_total | Counter | 接收消息数(按Action分组) |
| ocpp_messages_sent_total | Counter | 发送消息数(按Action分组) |
| ocpp_request_duration_seconds | Histogram | 请求处理耗时 |
| ocpp_command_timeout_total | Counter | 指令超时次数 |
| ocpp_event_timeout_total | Counter | 事件处理超时次数 |
| ocpp_websocket_errors_total | Counter | WebSocket错误次数 |

---

## 八、测试策略

### 单元测试

#### 协议处理器测试
- 序列化/反序列化准确性
- 消息格式验证
- 类型映射正确性

#### 状态缓存测试
- 并发读写安全
- 数据一致性
- 查询准确性

#### CallMatcher测试
- 请求-响应配对
- 超时清理
- 并发场景

### 集成测试

#### WebSocket通信测试
- 模拟充电桩客户端
- 连接建立与断开
- 消息收发完整性

#### 业务流程测试
- 充电桩上线流程
- 启动充电事务流程
- 远程启动流程
- 心跳检测流程

#### 异常场景测试
- 网络断开
- 消息超时
- 格式错误
- 并发连接

### 性能测试

#### 压力测试
- 1000并发连接
- 高频消息收发(100msg/s)
- 内存占用监控
- CPU占用监控

#### 稳定性测试
- 7*24小时运行
- 内存泄漏检测
- 连接泄漏检测

---

## 九、风险与应对

### 技术风险

| 风险 | 严重度 | 应对措施 |
|-----|-------|---------|
| WebSocket连接不稳定 | 高 | 实现重连机制,增加心跳检测 |
| 内存泄漏 | 中 | 定时清理,使用弱引用,压力测试 |
| 并发竞争 | 中 | 使用并发集合,加锁保护关键区 |
| 消息积压 | 中 | 限流,消息队列,异步处理 |
| 超时处理不当 | 低 | 统一超时控制,完善测试 |

### 开发风险

| 风险 | 严重度 | 应对措施 |
|-----|-------|---------|
| 开发周期延长 | 中 | 分阶段交付,优先核心功能 |
| 需求理解偏差 | 低 | 参考OCPP规范,编写测试用例 |
| 代码质量问题 | 低 | 代码审查,单元测试覆盖 |

---

## 十、总结

### 核心补充模块
1. **WebSocket通信层** - 系统运行基础
2. **消息路由与处理器** - 消息处理核心
3. **请求-响应匹配器** - 异步调用支撑
4. **事件机制** - 业务集成接口
5. **指令调用接口** - 控制能力实现
6. **连接管理增强** - 稳定性保障
7. **超时控制机制** - 可靠性保障
8. **心跳检测机制** - 在线监测准确性

### 实施建议
- 优先完成P0模块(WebSocket通信、消息处理、CallMatcher)
- 逐步完善P1模块(事件、指令)
- 最后优化P2模块(连接管理、心跳检测)
- 每个阶段完成后进行集成测试
- 持续关注性能和稳定性指标

### 预期成果
完成所有补充模块后,系统将具备:
- 完整的充电桩接入能力
- 稳定的消息处理能力
- 灵活的业务集成能力
- 可靠的指令下发能力
- 健壮的异常处理能力
