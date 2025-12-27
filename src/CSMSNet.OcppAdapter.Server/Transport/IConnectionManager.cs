using CSMSNet.OcppAdapter.Models.Events;

namespace CSMSNet.OcppAdapter.Server.Transport;

/// <summary>
/// 连接管理器接口
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// 添加会话
    /// </summary>
    /// <param name="session">WebSocket会话</param>
    /// <returns>是否成功添加</returns>
    bool AddSession(WebSocketSession session);

    /// <summary>
    /// 移除会话
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>是否成功移除</returns>
    bool RemoveSession(string chargePointId);

    /// <summary>
    /// 获取会话
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>WebSocket会话(可空)</returns>
    WebSocketSession? GetSession(string chargePointId);

    /// <summary>
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    List<WebSocketSession> GetAllSessions();

    /// <summary>
    /// 关闭所有会话
    /// </summary>
    /// <returns></returns>
    Task CloseAllSessions();

    /// <summary>
    /// 获取当前连接数
    /// </summary>
    /// <returns>连接数</returns>
    int GetConnectionCount();

    /// <summary>
    /// 获取活跃会话数
    /// </summary>
    /// <returns>活跃会话数</returns>
    int GetActiveSessionCount();
    
    /// <summary>
    /// 清理超时会话
    /// </summary>
    Task CleanupInactiveSessions();
    
    /// <summary>
    /// 获取连接指标
    /// </summary>
    Models.ConnectionMetrics GetConnectionMetrics();

    event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected;
    event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected;
}
