using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// StopTransaction事件参数
/// </summary>
public class StopTransactionEventArgs : OcppEventArgs
{
    /// <summary>
    /// StopTransaction请求
    /// </summary>
    public StopTransactionRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<StopTransactionResponse> ResponseTask { get; set; } = new();
}
