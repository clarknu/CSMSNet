using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// DataTransfer请求
/// </summary>
public class DataTransferRequest : OcppRequest
{
    public override string Action => "DataTransfer";

    /// <summary>
    /// 厂商ID
    /// </summary>
    [JsonPropertyName("vendorId")]
    public string VendorId { get; set; } = string.Empty;

    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
