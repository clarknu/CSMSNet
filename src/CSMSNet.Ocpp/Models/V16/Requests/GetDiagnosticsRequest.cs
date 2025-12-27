using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// GetDiagnostics请求
/// </summary>
public class GetDiagnosticsRequest : OcppRequest
{
    public override string Action => "GetDiagnostics";

    /// <summary>
    /// 上传地址
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = default!;

    /// <summary>
    /// 重试次数
    /// </summary>
    [JsonPropertyName("retries")]
    public int? Retries { get; set; }

    /// <summary>
    /// 重试间隔(秒)
    /// </summary>
    [JsonPropertyName("retryInterval")]
    public int? RetryInterval { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonPropertyName("stopTime")]
    public DateTime? StopTime { get; set; }
}
