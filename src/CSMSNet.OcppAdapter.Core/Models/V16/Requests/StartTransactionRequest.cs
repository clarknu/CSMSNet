using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// StartTransaction请求
/// </summary>
public class StartTransactionRequest : OcppRequest
{
    public override string Action => "StartTransaction";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;

    /// <summary>
    /// 起始电量(Wh)
    /// </summary>
    [JsonPropertyName("meterStart")]
    public int MeterStart { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 预约ID
    /// </summary>
    [JsonPropertyName("reservationId")]
    public int? ReservationId { get; set; }
}
