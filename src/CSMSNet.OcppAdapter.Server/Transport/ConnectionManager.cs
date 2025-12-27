using System.Collections.Concurrent;
using System.Net.WebSockets;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models.Enums;
using CSMSNet.OcppAdapter.Models.Events;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Transport;

/// <summary>
/// 连接管理器实现
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocketSession> _sessions = new();
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<ConnectionManager>? _logger;
    private int _totalConnectionsEver;
    private int _failedConnections;

    public event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected;
    public event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected;
    public event EventHandler<ChargePointConnectedEventArgs>? OnSessionCreated;
    public event EventHandler<ChargePointDisconnectedEventArgs>? OnSessionClosed;

    public ConnectionManager(
        OcppAdapterConfiguration configuration,
        ILogger<ConnectionManager>? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    public bool AddSession(WebSocketSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // 检查连接数限制
        if (_sessions.Count >= _configuration.MaxConcurrentConnections)
        {
            _logger?.LogWarning(
                "Connection limit reached ({Limit}), rejecting charge point {ChargePointId}",
                _configuration.MaxConcurrentConnections,
                session.ChargePointId);
            _failedConnections++;
            return false;
        }

        // 处理已存在的连接/会话
        if (_sessions.TryGetValue(session.ChargePointId, out var existingSession))
        {
            // 场景1：存在断开的会话（等待重连）
            if (existingSession.State == SessionState.Disconnected)
            {
                var retentionTime = _configuration.StateRetentionTime;
                var disconnectDuration = DateTime.UtcNow - (existingSession.LastDisconnectedAt ?? DateTime.MinValue);

                if (disconnectDuration < retentionTime)
                {
                    // 在保留时间内，复用会话状态
                    _logger?.LogInformation(
                        "Restoring session for {ChargePointId}, SessionID: {OldSessionId} -> {NewSessionId} (Logic Reused)",
                        session.ChargePointId,
                        existingSession.SessionId,
                        existingSession.SessionId); // ID will be same after restore

                    // 将旧会话的状态（ID, 验证状态等）复制到新会话对象
                    session.RestoreState(existingSession.SessionId, existingSession.IsVerified, existingSession.ConnectedAt);
                    
                    // 替换字典中的对象
                    _sessions[session.ChargePointId] = session;
                    
                    // 触发网络连接事件（不触发SessionCreated，因为是复用）
                    OnChargePointConnected?.Invoke(this, new ChargePointConnectedEventArgs
                    {
                        ChargePointId = session.ChargePointId,
                        SessionId = session.SessionId,
                        Timestamp = DateTime.UtcNow
                    });

                    return true;
                }
                else
                {
                    // 超时，清理旧会话
                    _logger?.LogInformation("Disconnected session for {ChargePointId} expired. Creating new session.", session.ChargePointId);
                    ForceRemoveSession(existingSession, "Session expired before reconnect");
                }
            }
            // 场景2：存在活跃连接（重复连接）
            else
            {
                _logger?.LogWarning(
                    "Duplicate connection for charge point {ChargePointId}, strategy: {Strategy}",
                    session.ChargePointId,
                    _configuration.DuplicateConnectionStrategy);

                switch (_configuration.DuplicateConnectionStrategy)
                {
                    case DuplicateConnectionStrategy.Replace:
                        // 关闭旧连接并移除
                        _ = existingSession.CloseAsync(WebSocketCloseStatus.NormalClosure, "Replaced by new connection");
                        ForceRemoveSession(existingSession, "Replaced by new connection");
                        break;

                    case DuplicateConnectionStrategy.Reject:
                        // 拒绝新连接
                        _failedConnections++;
                        return false;

                    case DuplicateConnectionStrategy.Duplicate:
                        // 允许重复连接(不推荐) - 这里其实字典只能存一个，所以会覆盖
                         _logger?.LogWarning(
                            "Allowing duplicate connection for {ChargePointId} (Overwrite in dictionary)",
                            session.ChargePointId);
                        break;
                }
            }
        }

        // 添加新会话
        if (_sessions.TryAdd(session.ChargePointId, session))
        {
            Interlocked.Increment(ref _totalConnectionsEver);
            _logger?.LogInformation(
                "Session added for charge point {ChargePointId}, session ID: {SessionId}",
                session.ChargePointId,
                session.SessionId);
            
            // 触发会话创建事件
            OnSessionCreated?.Invoke(this, new ChargePointConnectedEventArgs
            {
                ChargePointId = session.ChargePointId,
                SessionId = session.SessionId,
                Timestamp = DateTime.UtcNow
            });

            // 触发网络连接事件
            OnChargePointConnected?.Invoke(this, new ChargePointConnectedEventArgs
            {
                ChargePointId = session.ChargePointId,
                SessionId = session.SessionId,
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }

        _failedConnections++;
        return false;
    }

    public bool RemoveSession(string chargePointId)
    {
        if (_sessions.TryGetValue(chargePointId, out var session))
        {
            // 如果是异常断开（Disconnected），且配置了保留时间，则保留会话
            if (session.State == SessionState.Disconnected)
            {
                 _logger?.LogInformation(
                    "Network disconnected for {ChargePointId}, retaining session (State: {State})",
                    chargePointId, session.State);
                
                // 仅触发网络断开事件
                OnChargePointDisconnected?.Invoke(this, new ChargePointDisconnectedEventArgs
                {
                    ChargePointId = chargePointId,
                    SessionId = session.SessionId,
                    Timestamp = DateTime.UtcNow,
                    DisconnectReason = "Network Disconnected"
                });

                // 不从字典移除
                return true;
            }

            // 如果是正常关闭（Closed）或正在关闭，则彻底移除
            if (session.State == SessionState.Closed || session.State == SessionState.Closing)
            {
                return ForceRemoveSession(session, "Session Closed Normally");
            }
        }

        return false;
    }

    /// <summary>
    /// 强制移除会话并触发销毁事件
    /// </summary>
    private bool ForceRemoveSession(WebSocketSession session, string reason)
    {
        if (_sessions.TryRemove(session.ChargePointId, out var removedSession))
        {
             _logger?.LogInformation(
                "Session permanently removed for charge point {ChargePointId}, session ID: {SessionId}",
                session.ChargePointId,
                session.SessionId);

             // 如果还没触发过网络断开（例如直接从Connected变成Closed），触发它？
             // 通常RemoveSession是由于Disconnect引起的，所以网络断开应该已经触发或者在此触发。
             // 为了保险，我们触发网络断开（如果还没断的话？很难判断）。
             // 简化处理：ForceRemove 意味着会话结束。
             
             OnChargePointDisconnected?.Invoke(this, new ChargePointDisconnectedEventArgs
             {
                 ChargePointId = session.ChargePointId,
                 SessionId = session.SessionId,
                 Timestamp = DateTime.UtcNow,
                 DisconnectReason = reason
             });

             OnSessionClosed?.Invoke(this, new ChargePointDisconnectedEventArgs
             {
                 ChargePointId = session.ChargePointId,
                 SessionId = session.SessionId,
                 Timestamp = DateTime.UtcNow,
                 DisconnectReason = reason
             });

             return true;
        }
        return false;
    }

    public WebSocketSession? GetSession(string chargePointId)
    {
        _sessions.TryGetValue(chargePointId, out var session);
        return session;
    }

    public List<WebSocketSession> GetAllSessions()
    {
        return _sessions.Values.ToList();
    }

    public async Task CloseAllSessions()
    {
        _logger?.LogInformation("Closing all sessions, count: {Count}", _sessions.Count);

        var closeTasks = _sessions.Values
            .Select(session => session.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown"))
            .ToArray();

        await Task.WhenAll(closeTasks);

        _sessions.Clear();
    }

    public int GetConnectionCount()
    {
        return _sessions.Count;
    }

    public int GetActiveSessionCount()
    {
        var activeThreshold = DateTime.UtcNow.AddMinutes(-5); // 5分钟内有活动的会话视为活跃
        return _sessions.Values.Count(s => s.LastActivityAt > activeThreshold);
    }

    public Models.ConnectionMetrics GetConnectionMetrics()
    {
        var sessions = _sessions.Values.ToList();
        var avgDuration = sessions.Any()
            ? TimeSpan.FromTicks((long)sessions.Average(s => (DateTime.UtcNow - s.ConnectedAt).Ticks))
            : TimeSpan.Zero;

        return new Models.ConnectionMetrics
        {
            CurrentConnections = _sessions.Count,
            ActiveSessions = GetActiveSessionCount(),
            TotalConnectionsEver = _totalConnectionsEver,
            FailedConnections = _failedConnections,
            AverageConnectionDuration = avgDuration
        };
    }

    public async Task CleanupInactiveSessions()
    {
        var inactivityTimeout = _configuration.SessionInactivityTimeout;
        var retentionTimeout = _configuration.StateRetentionTime;
        var now = DateTime.UtcNow;

        var sessionsToCheck = _sessions.Values.ToList();

        foreach (var session in sessionsToCheck)
        {
            // 1. 检查断开连接的会话是否超过保留期
            if (session.State == SessionState.Disconnected)
            {
                if (session.LastDisconnectedAt.HasValue && 
                    now - session.LastDisconnectedAt.Value > retentionTimeout)
                {
                    _logger?.LogInformation("Cleaning up expired disconnected session for {ChargePointId}", session.ChargePointId);
                    ForceRemoveSession(session, "Session Retention Timeout");
                }
                continue;
            }

            // 2. 检查连接中但无活动的会话
            if (now - session.LastActivityAt > inactivityTimeout)
            {
                 _logger?.LogInformation("Cleaning up inactive session for {ChargePointId}", session.ChargePointId);
                 try
                 {
                     await session.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session Inactivity Timeout");
                     // CloseAsync sets state to Closed.
                     // RemoveSession will be called by event handler.
                     // RemoveSession will see Closed and call ForceRemoveSession.
                 }
                 catch (Exception ex)
                 {
                     _logger?.LogError(ex, "Error closing inactive session for {ChargePointId}", session.ChargePointId);
                     ForceRemoveSession(session, "Error closing inactive session");
                 }
            }
        }
    }
}
