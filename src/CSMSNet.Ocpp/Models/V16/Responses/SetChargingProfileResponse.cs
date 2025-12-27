using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// SetChargingProfile响应
/// </summary>
public class SetChargingProfileResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public ChargingProfileStatus Status { get; set; }
}
