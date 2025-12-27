using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// MeterValues请求
/// </summary>
public class MeterValuesRequest : OcppRequest
{
    public override string Action => "MeterValues";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// 事务ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public int? TransactionId { get; set; }

    /// <summary>
    /// 电表值列表
    /// </summary>
    [JsonPropertyName("meterValue")]
    public List<MeterValue> MeterValue { get; set; } = new();
}
