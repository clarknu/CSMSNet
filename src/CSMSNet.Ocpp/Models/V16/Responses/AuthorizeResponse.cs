using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// Authorize响应
/// </summary>
public class AuthorizeResponse : OcppResponse
{
    /// <summary>
    /// ID标签信息
    /// </summary>
    [JsonPropertyName("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();
}
