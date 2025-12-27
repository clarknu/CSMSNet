using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// BootNotification响应
/// </summary>
public class BootNotificationResponse : OcppResponse
{
    /// <summary>
    /// 注册状态
    /// </summary>
    [JsonPropertyName("status")]
    public RegistrationStatus Status { get; set; }

    /// <summary>
    /// 当前时间
    /// </summary>
    [JsonPropertyName("currentTime")]
    public DateTime CurrentTime { get; set; }

    /// <summary>
    /// 心跳间隔(秒)
    /// </summary>
    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}
