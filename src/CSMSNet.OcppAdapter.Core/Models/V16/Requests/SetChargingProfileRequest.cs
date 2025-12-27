using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// SetChargingProfile请求
/// </summary>
public class SetChargingProfileRequest : OcppRequest
{
    public override string Action => "SetChargingProfile";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 充电配置
    /// </summary>
    [JsonPropertyName("csChargingProfiles")]
    public ChargingProfile CsChargingProfiles { get; set; } = default!;
}
