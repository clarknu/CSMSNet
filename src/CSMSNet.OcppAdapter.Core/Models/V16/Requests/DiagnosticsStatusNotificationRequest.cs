using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// DiagnosticsStatusNotification请求
/// </summary>
public class DiagnosticsStatusNotificationRequest : OcppRequest
{
    public override string Action => "DiagnosticsStatusNotification";

    /// <summary>
    /// 诊断状态
    /// </summary>
    [JsonPropertyName("status")]
    public DiagnosticsStatus Status { get; set; }
}
