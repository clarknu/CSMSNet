using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// Authorize请求
/// </summary>
public class AuthorizeRequest : OcppRequest
{
    public override string Action => "Authorize";

    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;
}
