using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// StopTransaction响应
/// </summary>
public class StopTransactionResponse : OcppResponse
{
    /// <summary>
    /// ID标签信息
    /// </summary>
    [JsonPropertyName("idTagInfo")]
    public IdTagInfo? IdTagInfo { get; set; }
}
