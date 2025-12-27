using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Common;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// RemoteStartTransaction请求
/// </summary>
public class RemoteStartTransactionRequest : OcppRequest
{
    public override string Action => "RemoteStartTransaction";

    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int? ConnectorId { get; set; }

    /// <summary>
    /// 充电配置
    /// </summary>
    [JsonPropertyName("chargingProfile")]
    public ChargingProfile? ChargingProfile { get; set; }
}
