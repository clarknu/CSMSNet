namespace CSMSNet.OcppAdapter.Models.Enums;

/// <summary>
/// WebSocket会话状态
/// </summary>
public enum SessionState
{
    /// <summary>
    /// 连接建立中
    /// </summary>
    Connecting,

    /// <summary>
    /// 已连接
    /// </summary>
    Connected,

    /// <summary>
    /// 关闭中
    /// </summary>
    Closing,

    /// <summary>
    /// 已关闭(彻底销毁)
    /// </summary>
    Closed,

    /// <summary>
    /// 异常断开(等待重连)
    /// </summary>
    Disconnected
}
