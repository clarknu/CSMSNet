using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// StatusNotification事件参数
/// </summary>
public class StatusNotificationEventArgs : OcppEventArgs
{
    /// <summary>
    /// StatusNotification请求
    /// </summary>
    public StatusNotificationRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<StatusNotificationResponse> ResponseTask { get; set; } = new();
}
