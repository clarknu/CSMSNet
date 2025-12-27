using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// MeterValues事件参数
/// </summary>
public class MeterValuesEventArgs : OcppEventArgs
{
    /// <summary>
    /// MeterValues请求
    /// </summary>
    public MeterValuesRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<MeterValuesResponse> ResponseTask { get; set; } = new();
}
