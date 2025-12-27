using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// DiagnosticsStatusNotification事件参数
/// </summary>
public class DiagnosticsStatusNotificationEventArgs : OcppEventArgs
{
    /// <summary>
    /// DiagnosticsStatusNotification请求
    /// </summary>
    public DiagnosticsStatusNotificationRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<DiagnosticsStatusNotificationResponse> ResponseTask { get; set; } = new();
}
