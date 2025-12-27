using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// FirmwareStatusNotification事件参数
/// </summary>
public class FirmwareStatusNotificationEventArgs : OcppEventArgs
{
    /// <summary>
    /// FirmwareStatusNotification请求
    /// </summary>
    public FirmwareStatusNotificationRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<FirmwareStatusNotificationResponse> ResponseTask { get; set; } = new();
}
