using CSMSNet.OcppAdapter.Models.Enums;

namespace CSMSNet.OcppAdapter.Models;

/// <summary>
/// 会话信息
/// </summary>
public class SessionInfo
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 协议版本
    /// </summary>
    public string ProtocolVersion { get; set; } = string.Empty;

    /// <summary>
    /// 连接建立时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// 已发送消息数
    /// </summary>
    public int MessagesSent { get; set; }

    /// <summary>
    /// 已接收消息数
    /// </summary>
    public int MessagesReceived { get; set; }

    /// <summary>
    /// 会话状态
    /// </summary>
    public SessionState State { get; set; }
}
