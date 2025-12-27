using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// Reset请求
/// </summary>
public class ResetRequest : OcppRequest
{
    public override string Action => "Reset";

    /// <summary>
    /// 重置类型
    /// </summary>
    [JsonPropertyName("type")]
    public ResetType Type { get; set; }
}
