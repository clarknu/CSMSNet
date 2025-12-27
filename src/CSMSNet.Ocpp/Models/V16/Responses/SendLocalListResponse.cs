using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// SendLocalList响应
/// </summary>
public class SendLocalListResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public UpdateStatus Status { get; set; }
}
