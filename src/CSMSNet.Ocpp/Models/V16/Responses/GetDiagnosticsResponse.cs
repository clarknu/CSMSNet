using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// GetDiagnostics响应
/// </summary>
public class GetDiagnosticsResponse : OcppResponse
{
    /// <summary>
    /// 诊断文件名
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}
