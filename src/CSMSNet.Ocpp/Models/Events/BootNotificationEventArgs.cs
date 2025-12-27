using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// BootNotification事件参数
/// </summary>
public class BootNotificationEventArgs : OcppEventArgs
{
    /// <summary>
    /// BootNotification请求
    /// </summary>
    public BootNotificationRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<BootNotificationResponse> ResponseTask { get; set; } = new();
}
