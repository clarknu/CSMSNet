using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// FirmwareStatusNotification请求
/// </summary>
public class FirmwareStatusNotificationRequest : OcppRequest
{
    public override string Action => "FirmwareStatusNotification";

    /// <summary>
    /// 固件状态
    /// </summary>
    [JsonPropertyName("status")]
    public FirmwareStatus Status { get; set; }
}
