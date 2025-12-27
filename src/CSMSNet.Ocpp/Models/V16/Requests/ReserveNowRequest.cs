using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// ReserveNow请求
/// </summary>
public class ReserveNowRequest : OcppRequest
{
    public override string Action => "ReserveNow";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [JsonPropertyName("expiryDate")]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = default!;

    /// <summary>
    /// 预约ID
    /// </summary>
    [JsonPropertyName("reservationId")]
    public int ReservationId { get; set; }

    /// <summary>
    /// 父ID标签
    /// </summary>
    [JsonPropertyName("parentIdTag")]
    public string? ParentIdTag { get; set; }
}
