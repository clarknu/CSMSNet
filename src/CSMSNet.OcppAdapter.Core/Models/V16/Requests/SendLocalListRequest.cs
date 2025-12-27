using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Common;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// SendLocalList请求
/// </summary>
public class SendLocalListRequest : OcppRequest
{
    public override string Action => "SendLocalList";

    /// <summary>
    /// 列表版本
    /// </summary>
    [JsonPropertyName("listVersion")]
    public int ListVersion { get; set; }

    /// <summary>
    /// 本地授权列表
    /// </summary>
    [JsonPropertyName("localAuthorizationList")]
    public List<AuthorizationData> LocalAuthorizationList { get; set; } = new();

    /// <summary>
    /// 更新类型
    /// </summary>
    [JsonPropertyName("updateType")]
    public UpdateType UpdateType { get; set; }
}
