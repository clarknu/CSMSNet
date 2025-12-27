using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// StartTransaction响应
/// </summary>
public class StartTransactionResponse : OcppResponse
{
    /// <summary>
    /// ID标签信息
    /// </summary>
    [JsonPropertyName("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();

    /// <summary>
    /// 事务ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }
}
