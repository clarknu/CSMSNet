namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// OCPP事件参数基类
/// </summary>
public class OcppEventArgs : EventArgs
{
    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 事件时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// WebSocket会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
}
