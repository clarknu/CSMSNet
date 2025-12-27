# OCPP 1.6协议接入适配器

一个基于.NET 9的高性能OCPP协议接入适配器，为充电站管理系统(CSMS)提供充电桩接入能力。

## 项目特性

✅ **已完成功能**

### 核心基础设施
- ✅ 完整的项目结构和依赖配置
- ✅ 配置管理系统(OcppAdapterConfiguration)
- ✅ 异常处理体系(OcppException系列)
- ✅ 强类型消息基类(IOcppMessage, OcppRequest, OcppResponse)

### OCPP 1.6协议支持
- ✅ 20+个枚举类型定义(状态、错误码、配置等)
- ✅ 核心复合类型(IdTagInfo、ChargingProfile、MeterValue等)
- ✅ 核心消息对象(18对请求/响应):
  - Authorize/BootNotification/Heartbeat
  - StatusNotification/MeterValues
  - StartTransaction/StopTransaction
  - RemoteStartTransaction/RemoteStopTransaction
  - Reset/UnlockConnector
  - GetConfiguration/ChangeConfiguration
  - ClearCache/DataTransfer

### 协议处理层
- ✅ OCPP 1.6协议处理器(Ocpp16ProtocolHandler)
- ✅ JSON序列化/反序列化(基于System.Text.Json)
- ✅ OCPP消息格式处理([2, id, action, payload])
- ✅ 消息验证基础框架

### 状态管理
- ✅ 完整的状态缓存管理器(StateCache)
- ✅ 充电桩信息、状态、事务、预约缓存
- ✅ 线程安全的并发访问
- ✅ 充电桩在线/离线状态管理

### 接口设计
- ✅ IOcppAdapter主接口
- ✅ IOcppProtocolHandler协议抽象
- ✅ IStateCache状态缓存接口
- ✅ 查询接口(充电桩信息、状态、事务等)

## 项目结构

```
CSMSNet.OcppAdapter.sln
├── src/
│   ├── CSMSNet.OcppAdapter.Core/          # 核心类库
│   │   ├── Abstractions/                   # 接口定义
│   │   ├── Models/                         # 数据模型
│   │   │   ├── V16/                        # OCPP 1.6模型
│   │   │   │   ├── Enums/                  # 枚举类型
│   │   │   │   ├── Common/                 # 公共类型
│   │   │   │   ├── Requests/               # 请求对象
│   │   │   │   └── Responses/              # 响应对象
│   │   │   └── State/                      # 状态模型
│   │   ├── Configuration/                  # 配置类
│   │   └── Exceptions/                     # 异常定义
│   │
│   ├── CSMSNet.OcppAdapter.Protocol/      # 协议处理
│   │   └── V16/                            # OCPP 1.6实现
│   │       ├── Ocpp16ProtocolHandler.cs
│   │       └── Ocpp16Constants.cs
│   │
│   └── CSMSNet.OcppAdapter.Server/        # 服务实现
│       ├── State/                          # 状态管理
│       │   └── StateCache.cs
│       └── OcppAdapter.cs                  # 适配器主类
│
└── tests/
    └── CSMSNet.OcppAdapter.Tests/         # 单元测试
```

## 快速开始

### 安装

```bash
dotnet add reference path/to/CSMSNet.OcppAdapter.Core
dotnet add reference path/to/CSMSNet.OcppAdapter.Server
```

### 基本使用

```csharp
using CSMSNet.OcppAdapter.Server;
using CSMSNet.OcppAdapter.Configuration;

// 创建配置
var config = new OcppAdapterConfiguration
{
    ListenUrl = "http://+:8080/",
    HeartbeatInterval = TimeSpan.FromMinutes(5),
    BusinessEventTimeout = TimeSpan.FromSeconds(30)
};

// 创建适配器实例
var adapter = new OcppAdapter(config);

// 启动适配器
await adapter.StartAsync();

// 查询充电桩状态
var chargePointId = "CP001";
var isOnline = adapter.IsChargePointOnline(chargePointId);
var status = adapter.GetChargePointStatus(chargePointId);
var info = adapter.GetChargePointInfo(chargePointId);

// 停止适配器
await adapter.StopAsync();
```

## 配置选项

| 配置项 | 类型 | 默认值 | 说明 |
|-------|------|-------|------|
| ListenUrl | string | "http://+:8080/" | WebSocket监听地址 |
| BootNotificationTimeout | TimeSpan | 60秒 | BootNotification等待超时 |
| BusinessEventTimeout | TimeSpan | 30秒 | 业务事件处理超时 |
| CommandResponseTimeout | TimeSpan | 60秒 | 指令响应超时 |
| HeartbeatInterval | TimeSpan | 300秒 | 心跳间隔 |
| MaxConcurrentConnections | int | 10000 | 最大并发连接数 |
| EnableJsonSchemaValidation | bool | true | 启用JSON Schema验证 |

## 后续开发计划

### 待完成功能

#### 高优先级(核心功能)
- [ ] WebSocket服务器实现
- [ ] 消息路由器
- [ ] 请求/响应处理器
- [ ] 调用匹配器(请求-响应配对)
- [ ] 事件机制实现
- [ ] 超时控制机制

#### 中优先级(扩展功能)
- [ ] 完整的OCPP 1.6消息对象(剩余12对)
- [ ] 连接管理器和会话管理器
- [ ] 指令调用接口
- [ ] WebSocket子协议协商
- [ ] 心跳检测机制

#### 低优先级(优化和测试)
- [ ] JSON Schema验证实现
- [ ] 单元测试
- [ ] 集成测试
- [ ] 性能测试和优化
- [ ] OCPP 2.x支持(预留)

详细的实施指南请参考 [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)

## 技术栈

- .NET 9
- System.Text.Json (JSON序列化)
- JsonSchema.Net (Schema验证)
- Microsoft.Extensions.Logging (日志)

## 设计文档

完整的系统设计文档位于: `.qoder/quests/ocpp-adapter-development.md`

包含:
- 系统架构设计
- 消息流转设计
- 状态缓存设计
- 异步编程与超时控制
- 接口设计规范

## 协议参考

本项目实现基于以下OCPP规范:
- OCPP 1.6 Edition 2 (`protocols/OCPP_1.6_documentation/ocpp-1.6 edition 2.pdf`)
- OCPP-J 1.6 (`protocols/OCPP_1.6_documentation/ocpp-j-1.6-specification.pdf`)
- JSON Schemas (`protocols/OCPP_1.6_documentation/schemas/json/`)

## 许可证

[待定]

## 贡献

欢迎贡献代码!请遵循项目的代码规范和提交流程。

## 联系方式

[待定]
