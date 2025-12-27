using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// Heartbeat响应
/// </summary>
public class HeartbeatResponse : OcppResponse
{
    /// <summary>
    /// 当前时间
    /// </summary>
    [JsonPropertyName("currentTime")]
    public DateTime CurrentTime { get; set; }
}
