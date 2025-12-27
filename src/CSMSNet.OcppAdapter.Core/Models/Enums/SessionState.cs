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
    /// 已关闭
    /// </summary>
    Closed
}
