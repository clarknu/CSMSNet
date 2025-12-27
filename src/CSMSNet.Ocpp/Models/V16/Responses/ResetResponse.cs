using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// Reset响应
/// </summary>
public class ResetResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public ResetStatus Status { get; set; }
}
