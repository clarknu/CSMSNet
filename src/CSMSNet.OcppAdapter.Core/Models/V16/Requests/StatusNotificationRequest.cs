using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// StatusNotification请求
/// </summary>
public class StatusNotificationRequest : OcppRequest
{
    public override string Action => "StatusNotification";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    [JsonPropertyName("errorCode")]
    public ChargePointErrorCode ErrorCode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public ChargePointStatus Status { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    [JsonPropertyName("info")]
    public string? Info { get; set; }

    /// <summary>
    /// 厂商ID
    /// </summary>
    [JsonPropertyName("vendorId")]
    public string? VendorId { get; set; }

    /// <summary>
    /// 厂商错误代码
    /// </summary>
    [JsonPropertyName("vendorErrorCode")]
    public string? VendorErrorCode { get; set; }
}
