using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// UnlockConnector请求
/// </summary>
public class UnlockConnectorRequest : OcppRequest
{
    public override string Action => "UnlockConnector";

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }
}
