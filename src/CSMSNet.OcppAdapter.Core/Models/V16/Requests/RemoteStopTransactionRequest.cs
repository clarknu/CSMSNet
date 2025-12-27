using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// RemoteStopTransaction请求
/// </summary>
public class RemoteStopTransactionRequest : OcppRequest
{
    public override string Action => "RemoteStopTransaction";

    /// <summary>
    /// 事务ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }
}
