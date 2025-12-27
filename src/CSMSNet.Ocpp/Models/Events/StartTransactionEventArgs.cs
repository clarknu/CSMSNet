using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// StartTransaction事件参数
/// </summary>
public class StartTransactionEventArgs : OcppEventArgs
{
    /// <summary>
    /// StartTransaction请求
    /// </summary>
    public StartTransactionRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<StartTransactionResponse> ResponseTask { get; set; } = new();
}
