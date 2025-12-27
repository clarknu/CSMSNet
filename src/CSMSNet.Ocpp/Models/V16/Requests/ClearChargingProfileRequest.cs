using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// ClearChargingProfile请求
/// </summary>
public class ClearChargingProfileRequest : OcppRequest
{
    public override string Action => "ClearChargingProfile";

    /// <summary>
    /// 充电配置ID
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int? ConnectorId { get; set; }

    /// <summary>
    /// 充电配置目的
    /// </summary>
    [JsonPropertyName("chargingProfilePurpose")]
    public ChargingProfilePurpose? ChargingProfilePurpose { get; set; }

    /// <summary>
    /// 堆栈级别
    /// </summary>
    [JsonPropertyName("stackLevel")]
    public int? StackLevel { get; set; }
}
