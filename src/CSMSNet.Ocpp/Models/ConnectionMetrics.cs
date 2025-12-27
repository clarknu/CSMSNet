namespace CSMSNet.OcppAdapter.Models;

/// <summary>
/// 连接监控指标
/// </summary>
public class ConnectionMetrics
{
    /// <summary>
    /// 当前连接数
    /// </summary>
    public int CurrentConnections { get; set; }

    /// <summary>
    /// 活跃会话数
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// 历史总连接数
    /// </summary>
    public int TotalConnectionsEver { get; set; }

    /// <summary>
    /// 连接失败次数
    /// </summary>
    public int FailedConnections { get; set; }

    /// <summary>
    /// 平均连接时长
    /// </summary>
    public TimeSpan AverageConnectionDuration { get; set; }
}
