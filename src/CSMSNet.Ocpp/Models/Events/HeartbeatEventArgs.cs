using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// Heartbeat事件参数
/// </summary>
public class HeartbeatEventArgs : OcppEventArgs
{
    /// <summary>
    /// Heartbeat请求
    /// </summary>
    public HeartbeatRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<HeartbeatResponse> ResponseTask { get; set; } = new();
}
