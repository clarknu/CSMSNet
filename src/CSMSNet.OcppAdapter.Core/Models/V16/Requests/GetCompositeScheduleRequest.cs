using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// GetCompositeSchedule请求
/// </summary>
public class GetCompositeScheduleRequest : OcppRequest
{
    public override string Action => "GetCompositeSchedule";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 持续时间(秒)
    /// </summary>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// 充电速率单位
    /// </summary>
    [JsonPropertyName("chargingRateUnit")]
    public ChargingRateUnit? ChargingRateUnit { get; set; }
}
