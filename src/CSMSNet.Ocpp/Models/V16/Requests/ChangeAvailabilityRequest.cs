using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// ChangeAvailability请求
/// </summary>
public class ChangeAvailabilityRequest : OcppRequest
{
    public override string Action => "ChangeAvailability";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 可用性类型
    /// </summary>
    [JsonPropertyName("type")]
    public AvailabilityType Type { get; set; }
}
