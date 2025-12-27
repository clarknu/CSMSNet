using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// ChangeConfiguration请求
/// </summary>
public class ChangeConfigurationRequest : OcppRequest
{
    public override string Action => "ChangeConfiguration";

    /// <summary>
    /// 键
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
