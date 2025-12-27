using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Enums;

/// <summary>
/// 注册状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegistrationStatus
{
    Accepted,
    Pending,
    Rejected
}

/// <summary>
/// 授权状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizationStatus
{
    Accepted,
    Blocked,
    Expired,
    Invalid,
    ConcurrentTx
}

/// <summary>
/// 充电桩状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargePointStatus
{
    Available,
    Preparing,
    Charging,
    SuspendedEVSE,
    SuspendedEV,
    Finishing,
    Reserved,
    Unavailable,
    Faulted
}

/// <summary>
/// 充电桩错误代码
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargePointErrorCode
{
    ConnectorLockFailure,
    EVCommunicationError,
    GroundFailure,
    HighTemperature,
    InternalError,
    LocalListConflict,
    NoError,
    OtherError,
    OverCurrentFailure,
    PowerMeterFailure,
    PowerSwitchFailure,
    ReaderFailure,
    ResetFailure,
    UnderVoltage,
    OverVoltage,
    WeakSignal
}

/// <summary>
/// 远程启动/停止状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RemoteStartStopStatus
{
    Accepted,
    Rejected
}

/// <summary>
/// 配置状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfigurationStatus
{
    Accepted,
    Rejected,
    RebootRequired,
    NotSupported
}

/// <summary>
/// 重置类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResetType
{
    Hard,
    Soft
}

/// <summary>
/// 重置状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResetStatus
{
    Accepted,
    Rejected
}

/// <summary>
/// 解锁状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnlockStatus
{
    Unlocked,
    UnlockFailed,
    NotSupported
}

/// <summary>
/// 停止原因
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Reason
{
    EmergencyStop,
    EVDisconnected,
    HardReset,
    Local,
    Other,
    PowerLoss,
    Reboot,
    Remote,
    SoftReset,
    UnlockCommand,
    DeAuthorized
}

/// <summary>
/// 可用性类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AvailabilityType
{
    Inoperative,
    Operative
}

/// <summary>
/// 可用性状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AvailabilityStatus
{
    Accepted,
    Rejected,
    Scheduled
}

/// <summary>
/// 清除缓存状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClearCacheStatus
{
    Accepted,
    Rejected
}

/// <summary>
/// 充电配置目的
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargingProfilePurpose
{
    ChargePointMaxProfile,
    TxDefaultProfile,
    TxProfile
}

/// <summary>
/// 充电配置类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargingProfileKind
{
    Absolute,
    Recurring,
    Relative
}

/// <summary>
/// 充电速率单位
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargingRateUnit
{
    W, // 瓦特
    A  // 安培
}

/// <summary>
/// 循环类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecurrencyKind
{
    Daily,
    Weekly
}

/// <summary>
/// 充电配置状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargingProfileStatus
{
    Accepted,
    Rejected,
    NotSupported
}

/// <summary>
/// 清除充电配置状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClearChargingProfileStatus
{
    Accepted,
    Unknown
}

/// <summary>
/// 预约状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationStatus
{
    Accepted,
    Faulted,
    Occupied,
    Rejected,
    Unavailable
}

/// <summary>
/// 取消预约状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CancelReservationStatus
{
    Accepted,
    Rejected
}

/// <summary>
/// 数据传输状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataTransferStatus
{
    Accepted,
    Rejected,
    UnknownMessageId,
    UnknownVendorId
}

/// <summary>
/// 测量值上下文
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReadingContext
{
    InterruptionBegin,
    InterruptionEnd,
    SampleClock,
    SamplePeriodic,
    TransactionBegin,
    TransactionEnd,
    Trigger,
    Other
}

/// <summary>
/// 值格式
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueFormat
{
    Raw,
    SignedData
}

/// <summary>
/// 测量物
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Measurand
{
    [JsonPropertyName("Energy.Active.Export.Register")]
    EnergyActiveExportRegister,
    [JsonPropertyName("Energy.Active.Import.Register")]
    EnergyActiveImportRegister,
    [JsonPropertyName("Energy.Reactive.Export.Register")]
    EnergyReactiveExportRegister,
    [JsonPropertyName("Energy.Reactive.Import.Register")]
    EnergyReactiveImportRegister,
    [JsonPropertyName("Energy.Active.Export.Interval")]
    EnergyActiveExportInterval,
    [JsonPropertyName("Energy.Active.Import.Interval")]
    EnergyActiveImportInterval,
    [JsonPropertyName("Energy.Reactive.Export.Interval")]
    EnergyReactiveExportInterval,
    [JsonPropertyName("Energy.Reactive.Import.Interval")]
    EnergyReactiveImportInterval,
    [JsonPropertyName("Power.Active.Export")]
    PowerActiveExport,
    [JsonPropertyName("Power.Active.Import")]
    PowerActiveImport,
    [JsonPropertyName("Power.Offered")]
    PowerOffered,
    [JsonPropertyName("Power.Reactive.Export")]
    PowerReactiveExport,
    [JsonPropertyName("Power.Reactive.Import")]
    PowerReactiveImport,
    [JsonPropertyName("Power.Factor")]
    PowerFactor,
    [JsonPropertyName("Current.Import")]
    CurrentImport,
    [JsonPropertyName("Current.Export")]
    CurrentExport,
    [JsonPropertyName("Current.Offered")]
    CurrentOffered,
    Voltage,
    Frequency,
    Temperature,
    SoC,
    RPM
}

/// <summary>
/// 相位
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Phase
{
    L1,
    L2,
    L3,
    N,
    [JsonPropertyName("L1-N")]
    L1N,
    [JsonPropertyName("L2-N")]
    L2N,
    [JsonPropertyName("L3-N")]
    L3N,
    [JsonPropertyName("L1-L2")]
    L1L2,
    [JsonPropertyName("L2-L3")]
    L2L3,
    [JsonPropertyName("L3-L1")]
    L3L1
}

/// <summary>
/// 位置
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Location
{
    Cable,
    EV,
    Inlet,
    Outlet,
    Body
}

/// <summary>
/// 测量单位
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitOfMeasure
{
    Wh,
    kWh,
    varh,
    kvarh,
    W,
    kW,
    VA,
    kVA,
    var,
    kvar,
    A,
    V,
    K,
    Celsius,
    Fahrenheit,
    Percent
}

/// <summary>
/// 获取组合计划状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GetCompositeScheduleStatus
{
    Accepted,
    Rejected
}

/// <summary>
/// 固件状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FirmwareStatus
{
    Downloaded,
    DownloadFailed,
    Downloading,
    Idle,
    InstallationFailed,
    Installing,
    Installed
}

/// <summary>
/// 诊断状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiagnosticsStatus
{
    Idle,
    Uploaded,
    UploadFailed,
    Uploading
}

/// <summary>
/// 消息触发器
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTrigger
{
    BootNotification,
    DiagnosticsStatusNotification,
    FirmwareStatusNotification,
    Heartbeat,
    MeterValues,
    StatusNotification
}

/// <summary>
/// 触发消息状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerMessageStatus
{
    Accepted,
    Rejected,
    NotImplemented
}

/// <summary>
/// 更新类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UpdateType
{
    Differential,
    Full
}

/// <summary>
/// 更新状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UpdateStatus
{
    Accepted,
    Failed,
    NotSupported,
    VersionMismatch
}
