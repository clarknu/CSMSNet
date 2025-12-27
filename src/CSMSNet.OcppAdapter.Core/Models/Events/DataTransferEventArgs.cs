using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// DataTransfer事件参数
/// </summary>
public class DataTransferEventArgs : OcppEventArgs
{
    /// <summary>
    /// DataTransfer请求
    /// </summary>
    public DataTransferRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<DataTransferResponse> ResponseTask { get; set; } = new();
}
