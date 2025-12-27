using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// StopTransaction请求
/// </summary>
public class StopTransactionRequest : OcppRequest
{
    public override string Action => "StopTransaction";

    /// <summary>
    /// 事务ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }

    /// <summary>
    /// 结束电量(Wh)
    /// </summary>
    [JsonPropertyName("meterStop")]
    public int MeterStop { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string? IdTag { get; set; }

    /// <summary>
    /// 停止原因
    /// </summary>
    [JsonPropertyName("reason")]
    public Reason? Reason { get; set; }

    /// <summary>
    /// 交易数据
    /// </summary>
    [JsonPropertyName("transactionData")]
    public List<MeterValue>? TransactionData { get; set; }
}
