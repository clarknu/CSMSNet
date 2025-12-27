using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// GetConfiguration请求
/// </summary>
public class GetConfigurationRequest : OcppRequest
{
    public override string Action => "GetConfiguration";

    /// <summary>
    /// 键列表
    /// </summary>
    [JsonPropertyName("key")]
    public List<string>? Key { get; set; }
}
