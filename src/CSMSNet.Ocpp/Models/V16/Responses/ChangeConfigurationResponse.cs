using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// ChangeConfiguration响应
/// </summary>
public class ChangeConfigurationResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public ConfigurationStatus Status { get; set; }
}
