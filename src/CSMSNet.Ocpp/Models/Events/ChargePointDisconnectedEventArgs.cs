namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// 充电桩断开连接事件参数
/// </summary>
public class ChargePointDisconnectedEventArgs : OcppEventArgs
{
    /// <summary>
    /// 断开原因
    /// </summary>
    public string DisconnectReason { get; set; } = string.Empty;

    /// <summary>
    /// 连接持续时长
    /// </summary>
    public TimeSpan ConnectionDuration { get; set; }
}
