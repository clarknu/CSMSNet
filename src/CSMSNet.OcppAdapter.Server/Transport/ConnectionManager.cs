using System.Collections.Concurrent;
using System.Net.WebSockets;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models.Enums;
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

        // 处理重复连接
        if (_sessions.TryGetValue(session.ChargePointId, out var existingSession))
        {
            _logger?.LogWarning(
                "Duplicate connection for charge point {ChargePointId}, strategy: {Strategy}",
                session.ChargePointId,
                _configuration.DuplicateConnectionStrategy);

            switch (_configuration.DuplicateConnectionStrategy)
            {
                case DuplicateConnectionStrategy.Replace:
                    // 关闭旧连接
                    _ = existingSession.CloseAsync(WebSocketCloseStatus.NormalClosure, "Replaced by new connection");
                    _sessions.TryRemove(session.ChargePointId, out _);
                    break;

                case DuplicateConnectionStrategy.Reject:
                    // 拒绝新连接
                    _failedConnections++;
                    return false;

                case DuplicateConnectionStrategy.Duplicate:
                    // 允许重复连接(不推荐)
                    _logger?.LogWarning(
                        "Allowing duplicate connection for {ChargePointId} (not recommended)",
                        session.ChargePointId);
                    break;
            }
        }

        // 添加会话
        if (_sessions.TryAdd(session.ChargePointId, session))
        {
            Interlocked.Increment(ref _totalConnectionsEver);
            _logger?.LogInformation(
                "Session added for charge point {ChargePointId}, session ID: {SessionId}",
                session.ChargePointId,
                session.SessionId);
            return true;
        }

        _failedConnections++;
        return false;
    }

    public bool RemoveSession(string chargePointId)
    {
        if (_sessions.TryRemove(chargePointId, out var session))
        {
            _logger?.LogInformation(
                "Session removed for charge point {ChargePointId}, session ID: {SessionId}",
                chargePointId,
                session.SessionId);
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

    /// <summary>
    /// 获取连接监控指标
    /// </summary>
    /// <returns>连接指标</returns>
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

    /// <summary>
    /// 清理超时会话
    /// </summary>
    /// <returns></returns>
    public async Task CleanupInactiveSessions()
    {
        var timeout = _configuration.SessionInactivityTimeout;
        var now = DateTime.UtcNow;
        var inactiveSessions = _sessions.Values
            .Where(s => now - s.LastActivityAt > timeout)
            .ToList();

        if (inactiveSessions.Count == 0)
            return;

        _logger?.LogInformation(
            "Cleaning up {Count} inactive sessions (timeout: {Timeout})",
            inactiveSessions.Count,
            timeout);

        foreach (var session in inactiveSessions)
        {
            try
            {
                await session.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session timeout");
                RemoveSession(session.ChargePointId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error cleaning up session for charge point {ChargePointId}",
                    session.ChargePointId);
            }
        }
    }
}
