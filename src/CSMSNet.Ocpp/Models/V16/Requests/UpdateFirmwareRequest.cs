using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// UpdateFirmware请求
/// </summary>
public class UpdateFirmwareRequest : OcppRequest
{
    public override string Action => "UpdateFirmware";

    /// <summary>
    /// 固件下载地址
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = default!;

    /// <summary>
    /// 重试次数
    /// </summary>
    [JsonPropertyName("retries")]
    public int? Retries { get; set; }

    /// <summary>
    /// 检索时间
    /// </summary>
    [JsonPropertyName("retrieveDate")]
    public DateTime RetrieveDate { get; set; }

    /// <summary>
    /// 重试间隔(秒)
    /// </summary>
    [JsonPropertyName("retryInterval")]
    public int? RetryInterval { get; set; }
}
