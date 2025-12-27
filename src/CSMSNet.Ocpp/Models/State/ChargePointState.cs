using CSMSNet.OcppAdapter.Models.V16.Enums;
using System.Collections.Concurrent;

namespace CSMSNet.OcppAdapter.Models.State;

/// <summary>
/// 充电桩基础信息
/// </summary>
public class ChargePointInfo
{
    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 注册状态
    /// </summary>
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    /// <summary>
    /// 厂商
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// 型号
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// 充电桩盒序列号
    /// </summary>
    public string? ChargeBoxSerialNumber { get; set; }

    /// <summary>
    /// 固件版本
    /// </summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// ICCID
    /// </summary>
    public string? Iccid { get; set; }

    /// <summary>
    /// IMSI
    /// </summary>
    public string? Imsi { get; set; }

    /// <summary>
    /// 电表类型
    /// </summary>
    public string? MeterType { get; set; }

    /// <summary>
    /// 电表序列号
    /// </summary>
    public string? MeterSerialNumber { get; set; }

    /// <summary>
    /// OCPP协议版本
    /// </summary>
    public string ProtocolVersion { get; set; } = "1.6";
}

/// <summary>
/// 充电桩状态
/// </summary>
public class ChargePointStatus
{
    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// 最后通信时间
    /// </summary>
    public DateTime? LastCommunicationAt { get; set; }

    /// <summary>
    /// 连接器状态列表
    /// </summary>
    public Dictionary<int, ConnectorStatus> ConnectorStatuses { get; set; } = new();

    /// <summary>
    /// 固件更新状态
    /// </summary>
    public CSMSNet.OcppAdapter.Models.V16.Enums.FirmwareStatus? FirmwareStatus { get; set; }

    /// <summary>
    /// 诊断日志上传状态
    /// </summary>
    public CSMSNet.OcppAdapter.Models.V16.Enums.DiagnosticsStatus? DiagnosticsStatus { get; set; }

    /// <summary>
    /// 本地鉴权列表版本
    /// </summary>
    public int? LocalAuthListVersion { get; set; }

    /// <summary>
    /// 额定功率
    /// </summary>
    public decimal? PowerRating { get; set; }

    /// <summary>
    /// 连接器数量
    /// </summary>
    public int ConnectorCount { get; set; }

    /// <summary>
    /// 配置项缓存
    /// </summary>
    public ConcurrentDictionary<string, string> ConfigurationItems { get; set; } = new();
}

/// <summary>
/// 电池状态信息
/// </summary>
public class BatteryStatus
{
    /// <summary>
    /// 荷电状态 (%)
    /// </summary>
    public decimal? SoC { get; set; }

    /// <summary>
    /// 电池电压 (V)
    /// </summary>
    public decimal? Voltage { get; set; }

    /// <summary>
    /// 电池电流 (A)
    /// </summary>
    public decimal? Current { get; set; }

    /// <summary>
    /// 电池功率 (W)
    /// </summary>
    public decimal? Power { get; set; }

    /// <summary>
    /// 电池温度 (Celsius)
    /// </summary>
    public decimal? Temperature { get; set; }
}

/// <summary>
/// 连接器状态
/// </summary>
public class ConnectorStatus
{
    /// <summary>
    /// 连接器ID（0表示整个充电桩）
    /// </summary>
    public int ConnectorId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 错误代码
    /// </summary>
    public string ErrorCode { get; set; } = "NoError";

    /// <summary>
    /// 信息
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 厂商ID
    /// </summary>
    public string? VendorId { get; set; }

    /// <summary>
    /// 厂商错误代码
    /// </summary>
    public string? VendorErrorCode { get; set; }

    /// <summary>
    /// 当前关联的交易ID
    /// </summary>
    public int? CurrentTransactionId { get; set; }

    /// <summary>
    /// 实时电压 (V)
    /// </summary>
    public decimal? InstantVoltage { get; set; }

    /// <summary>
    /// 实时电流 (A)
    /// </summary>
    public decimal? InstantCurrent { get; set; }

    /// <summary>
    /// 实时功率 (W)
    /// </summary>
    public decimal? InstantPower { get; set; }
    
    /// <summary>
    /// 最近一次电表读数 (Wh)
    /// </summary>
    public int? LastMeterValue { get; set; }

    /// <summary>
    /// 关联的电池信息
    /// </summary>
    public BatteryStatus? Battery { get; set; }
}

/// <summary>
/// 充电事务
/// </summary>
public class Transaction
{
    /// <summary>
    /// 事务ID
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的连接器ID列表
    /// </summary>
    public List<int> ConnectorIds { get; set; } = new();

    /// <summary>
    /// 卡号
    /// </summary>
    public string IdTag { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 各连接器的起始电量（ConnectorId -> MeterValue Wh）
    /// </summary>
    public Dictionary<int, int> MeterStartValues { get; set; } = new();

    /// <summary>
    /// 各连接器是否已设置正式起始电表值 (ConnectorId -> bool)
    /// </summary>
    public Dictionary<int, bool> HasSetOfficialMeterStart { get; set; } = new();

    /// <summary>
    /// 已消耗的总电量 (Wh)
    /// </summary>
    public int? TotalConsumedEnergy { get; set; }

    /// <summary>
    /// 预约ID
    /// </summary>
    public int? ReservationId { get; set; }

    /// <summary>
    /// 最近一次电表值更新时间
    /// </summary>
    public DateTime? LastMeterUpdateAt { get; set; }

    /// <summary>
    /// 当前总功率 (W)
    /// </summary>
    public decimal? CurrentPower { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 预约信息
/// </summary>
public class Reservation
{
    /// <summary>
    /// 预约ID
    /// </summary>
    public int ReservationId { get; set; }

    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; set; } = string.Empty;

    /// <summary>
    /// 连接器ID
    /// </summary>
    public int ConnectorId { get; set; }

    /// <summary>
    /// 卡号
    /// </summary>
    public string IdTag { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// 父卡号
    /// </summary>
    public string? ParentIdTag { get; set; }
}

/// <summary>
/// 配置项
/// </summary>
public class ConfigurationItem
{
    /// <summary>
    /// 配置键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 配置值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 是否只读
    /// </summary>
    public bool Readonly { get; set; }
}
