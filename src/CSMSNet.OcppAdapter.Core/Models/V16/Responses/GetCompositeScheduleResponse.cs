using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Common;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Responses;

/// <summary>
/// GetCompositeSchedule响应
/// </summary>
public class GetCompositeScheduleResponse : OcppResponse
{
    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public GetCompositeScheduleStatus Status { get; set; }

    /// <summary>
    /// 连接器ID
    /// </summary>
    [JsonPropertyName("connectorId")]
    public int? ConnectorId { get; set; }

    /// <summary>
    /// 计划开始时间
    /// </summary>
    [JsonPropertyName("scheduleStart")]
    public DateTime? ScheduleStart { get; set; }

    /// <summary>
    /// 充电计划
    /// </summary>
    [JsonPropertyName("chargingSchedule")]
    public ChargingSchedule? ChargingSchedule { get; set; }
}
