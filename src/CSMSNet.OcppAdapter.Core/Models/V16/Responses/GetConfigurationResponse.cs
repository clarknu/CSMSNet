using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// GetConfiguration响应
/// </summary>
public class GetConfigurationResponse : OcppResponse
{
    /// <summary>
    /// 配置键列表
    /// </summary>
    [JsonPropertyName("configurationKey")]
    public List<KeyValue>? ConfigurationKey { get; set; }

    /// <summary>
    /// 未知键列表
    /// </summary>
    [JsonPropertyName("unknownKey")]
    public List<string>? UnknownKey { get; set; }
}
