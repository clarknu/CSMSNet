using CSMSNet.OcppAdapter.Models.Enums;

namespace CSMSNet.OcppAdapter.Configuration;

/// <summary>
/// OCPP适配器配置
/// </summary>
public class OcppAdapterConfiguration
{
    /// <summary>
    /// WebSocket服务监听地址
    /// </summary>
    public string ListenUrl { get; set; } = "http://+:8080/";

    /// <summary>
    /// BootNotification等待超时
    /// </summary>
    public TimeSpan BootNotificationTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 业务事件处理超时
    /// </summary>
    public TimeSpan BusinessEventTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 指令响应超时
    /// </summary>
    public TimeSpan CommandResponseTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 消息发送超时
    /// </summary>
    public TimeSpan MessageSendTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 返回给充电桩的心跳间隔（在BootNotificationResponse中）
    /// </summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    /// 心跳超时倍数（心跳间隔的倍数）
    /// </summary>
    public double HeartbeatTimeoutMultiplier { get; set; } = 2.5;

    /// <summary>
    /// 是否启用充电桩身份验证
    /// </summary>
    public bool EnableAuthentication { get; set; } = false;

    /// <summary>
    /// 最大并发连接数
    /// </summary>
    public int MaxConcurrentConnections { get; set; } = 10000;

    /// <summary>
    /// 每个充电桩消息队列大小
    /// </summary>
    public int MessageQueueSize { get; set; } = 1000;

    /// <summary>
    /// 断线后状态保留时间
    /// </summary>
    public TimeSpan StateRetentionTime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 是否启用JSON Schema验证
    /// </summary>
    public bool EnableJsonSchemaValidation { get; set; } = true;

    /// <summary>
    /// 重复连接处理策略
    /// </summary>
    public DuplicateConnectionStrategy DuplicateConnectionStrategy { get; set; } = DuplicateConnectionStrategy.Replace;

    /// <summary>
    /// 会话无活动超时
    /// </summary>
    public TimeSpan SessionInactivityTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 消息接收超时
    /// </summary>
    public TimeSpan MessageReceiveTimeout { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    /// 连接建立超时
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 启用心跳监控
    /// </summary>
    public bool EnableHeartbeatMonitoring { get; set; } = true;

    /// <summary>
    /// 心跳检测间隔
    /// </summary>
    public TimeSpan HeartbeatCheckInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 会话清理间隔
    /// </summary>
    public TimeSpan SessionCleanupInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// CallMatcher清理过期Call间隔
    /// </summary>
    public TimeSpan CallMatcherCleanupInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 支持的OCPP协议版本
    /// </summary>
    public string[] SupportedProtocols { get; set; } = new[] { "ocpp1.6", "ocpp1.5" };

    /// <summary>
    /// 默认OCPP协议版本
    /// </summary>
    public string DefaultProtocol { get; set; } = "ocpp1.6";
}
