namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// 充电桩连接事件参数
/// </summary>
public class ChargePointConnectedEventArgs : OcppEventArgs
{
    /// <summary>
    /// 协议版本
    /// </summary>
    public string ProtocolVersion { get; set; } = string.Empty;

    /// <summary>
    /// 远程IP地址
    /// </summary>
    public string RemoteEndpoint { get; set; } = string.Empty;
}
