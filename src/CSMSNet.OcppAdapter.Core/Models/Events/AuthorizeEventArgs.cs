using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Models.Events;

/// <summary>
/// Authorize事件参数
/// </summary>
public class AuthorizeEventArgs : OcppEventArgs
{
    /// <summary>
    /// Authorize请求
    /// </summary>
    public AuthorizeRequest Request { get; set; } = default!;

    /// <summary>
    /// 响应控制
    /// </summary>
    public TaskCompletionSource<AuthorizeResponse> ResponseTask { get; set; } = new();
}
