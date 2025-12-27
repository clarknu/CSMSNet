using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models.V16.Enums;

namespace CSMSNet.OcppAdapter.Models.V16.Common;

/// <summary>
/// ID标签信息
/// </summary>
public class IdTagInfo
{
    /// <summary>
    /// 授权状态
    /// </summary>
    [JsonPropertyName("status")]
    public AuthorizationStatus Status { get; set; }

    /// <summary>
    /// 过期日期
    /// </summary>
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// 父ID标签
    /// </summary>
    [JsonPropertyName("parentIdTag")]
    public string? ParentIdTag { get; set; }
}

/// <summary>
/// 充电配置
/// </summary>
public class ChargingProfile
{
    /// <summary>
    /// 充电配置ID
    /// </summary>
    [JsonPropertyName("chargingProfileId")]
    public int ChargingProfileId { get; set; }

    /// <summary>
    /// 事务ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public int? TransactionId { get; set; }

    /// <summary>
    /// 堆栈级别
    /// </summary>
    [JsonPropertyName("stackLevel")]
    public int StackLevel { get; set; }

    /// <summary>
    /// 充电配置目的
    /// </summary>
    [JsonPropertyName("chargingProfilePurpose")]
    public ChargingProfilePurpose ChargingProfilePurpose { get; set; }

    /// <summary>
    /// 充电配置类型
    /// </summary>
    [JsonPropertyName("chargingProfileKind")]
    public ChargingProfileKind ChargingProfileKind { get; set; }

    /// <summary>
    /// 循环类型
    /// </summary>
    [JsonPropertyName("recurrencyKind")]
    public RecurrencyKind? RecurrencyKind { get; set; }

    /// <summary>
    /// 有效起始时间
    /// </summary>
    [JsonPropertyName("validFrom")]
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// 有效结束时间
    /// </summary>
    [JsonPropertyName("validTo")]
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// 充电计划
    /// </summary>
    [JsonPropertyName("chargingSchedule")]
    public ChargingSchedule ChargingSchedule { get; set; } = new();
}

/// <summary>
/// 充电计划
/// </summary>
public class ChargingSchedule
{
    /// <summary>
    /// 持续时间(秒)
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// 计划开始时间
    /// </summary>
    [JsonPropertyName("startSchedule")]
    public DateTime? StartSchedule { get; set; }

    /// <summary>
    /// 充电速率单位
    /// </summary>
    [JsonPropertyName("chargingRateUnit")]
    public ChargingRateUnit ChargingRateUnit { get; set; }

    /// <summary>
    /// 充电计划时段列表
    /// </summary>
    [JsonPropertyName("chargingSchedulePeriod")]
    public List<ChargingSchedulePeriod> ChargingSchedulePeriod { get; set; } = new();

    /// <summary>
    /// 最小充电速率
    /// </summary>
    [JsonPropertyName("minChargingRate")]
    public decimal? MinChargingRate { get; set; }
}

/// <summary>
/// 充电计划时段
/// </summary>
public class ChargingSchedulePeriod
{
    /// <summary>
    /// 开始时段(秒，相对于计划开始时间)
    /// </summary>
    [JsonPropertyName("startPeriod")]
    public int StartPeriod { get; set; }

    /// <summary>
    /// 功率或电流限制
    /// </summary>
    [JsonPropertyName("limit")]
    public decimal Limit { get; set; }

    /// <summary>
    /// 相数
    /// </summary>
    [JsonPropertyName("numberPhases")]
    public int? NumberPhases { get; set; }
}

/// <summary>
/// 电表值
/// </summary>
public class MeterValue
{
    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 采样值列表
    /// </summary>
    [JsonPropertyName("sampledValue")]
    public List<SampledValue> SampledValue { get; set; } = new();
}

/// <summary>
/// 采样值
/// </summary>
public class SampledValue
{
    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 上下文
    /// </summary>
    [JsonPropertyName("context")]
    public ReadingContext? Context { get; set; }

    /// <summary>
    /// 格式
    /// </summary>
    [JsonPropertyName("format")]
    public ValueFormat? Format { get; set; }

    /// <summary>
    /// 测量物
    /// </summary>
    [JsonPropertyName("measurand")]
    public Measurand? Measurand { get; set; }

    /// <summary>
    /// 相位
    /// </summary>
    [JsonPropertyName("phase")]
    public Phase? Phase { get; set; }

    /// <summary>
    /// 位置
    /// </summary>
    [JsonPropertyName("location")]
    public Location? Location { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [JsonPropertyName("unit")]
    public UnitOfMeasure? Unit { get; set; }
}

/// <summary>
/// 配置键值对
/// </summary>
public class KeyValue
{
    /// <summary>
    /// 键
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 是否只读
    /// </summary>
    [JsonPropertyName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
/// 授权数据
/// </summary>
public class AuthorizationData
{
    /// <summary>
    /// ID标签
    /// </summary>
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;

    /// <summary>
    /// ID标签信息
    /// </summary>
    [JsonPropertyName("idTagInfo")]
    public IdTagInfo? IdTagInfo { get; set; }
}
