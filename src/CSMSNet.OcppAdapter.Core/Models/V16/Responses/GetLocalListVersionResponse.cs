using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// GetLocalListVersion响应
/// </summary>
public class GetLocalListVersionResponse : OcppResponse
{
    /// <summary>
    /// 列表版本
    /// </summary>
    [JsonPropertyName("listVersion")]
    public int ListVersion { get; set; }
}
