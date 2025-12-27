using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.Ocpp.Handlers;

/// <summary>
/// 待响应的Call消息
/// </summary>
public class PendingCall
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 消息Action
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 原始请求对象
    /// </summary>
    public OcppRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应等待句柄
    /// </summary>
    public TaskCompletionSource<IOcppMessage> ResponseTask { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 超时时间
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationTokenSource CancellationToken { get; set; } = new();
}
