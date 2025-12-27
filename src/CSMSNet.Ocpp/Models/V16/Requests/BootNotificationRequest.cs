using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// BootNotification请求
/// </summary>
public class BootNotificationRequest : OcppRequest
{
    public override string Action => "BootNotification";

    /// <summary>
    /// 充电桩厂商
    /// </summary>
    [JsonPropertyName("chargePointVendor")]
    public string ChargePointVendor { get; set; } = string.Empty;

    /// <summary>
    /// 充电桩型号
    /// </summary>
    [JsonPropertyName("chargePointModel")]
    public string ChargePointModel { get; set; } = string.Empty;

    /// <summary>
    /// 充电桩序列号
    /// </summary>
    [JsonPropertyName("chargePointSerialNumber")]
    public string? ChargePointSerialNumber { get; set; }

    /// <summary>
    /// 充电桩盒序列号
    /// </summary>
    [JsonPropertyName("chargeBoxSerialNumber")]
    public string? ChargeBoxSerialNumber { get; set; }

    /// <summary>
    /// 固件版本
    /// </summary>
    [JsonPropertyName("firmwareVersion")]
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// ICCID
    /// </summary>
    [JsonPropertyName("iccid")]
    public string? Iccid { get; set; }

    /// <summary>
    /// IMSI
    /// </summary>
    [JsonPropertyName("imsi")]
    public string? Imsi { get; set; }

    /// <summary>
    /// 电表类型
    /// </summary>
    [JsonPropertyName("meterType")]
    public string? MeterType { get; set; }

    /// <summary>
    /// 电表序列号
    /// </summary>
    [JsonPropertyName("meterSerialNumber")]
    public string? MeterSerialNumber { get; set; }
}
