using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// TriggerMessage请求
/// </summary>
public class TriggerMessageRequest : OcppRequest
{
    public override string Action => "TriggerMessage";

    /// <summary>
    /// 请求的消息
    /// </summary>
    [JsonPropertyName("requestedMessage")]
    public MessageTrigger RequestedMessage { get; set; }

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int? ConnectorId { get; set; }
}
