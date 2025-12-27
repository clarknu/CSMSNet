using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// DataTransfer响应
/// </summary>
public class DataTransferResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public DataTransferStatus Status { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
