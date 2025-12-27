using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// RemoteStartTransaction响应
/// </summary>
public class RemoteStartTransactionResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public RemoteStartStopStatus Status { get; set; }
}
