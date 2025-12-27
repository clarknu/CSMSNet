using System.Collections.Concurrent;
using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Models.State;
using CSMSNet.OcppAdapter.Models.V16.Enums; // Added Enums namespace

namespace CSMSNet.Ocpp.State;

/// <summary>
/// 状态缓存管理器实现
/// </summary>
public class StateCache : IStateCache
{
    private readonly ConcurrentDictionary<string, ChargePointInfo> _chargePointInfos = new();
    private readonly ConcurrentDictionary<string, CSMSNet.OcppAdapter.Models.State.ChargePointStatus> _chargePointStatuses = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ConnectorStatus>> _connectorStatuses = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Transaction>> _activeTransactions = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation>> _reservations = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConfigurationItem>> _configurations = new();

    public ChargePointInfo? GetChargePointInfo(string chargePointId)
    {
        _chargePointInfos.TryGetValue(chargePointId, out var info);
        return info;
    }

    public CSMSNet.OcppAdapter.Models.State.ChargePointStatus? GetChargePointStatus(string chargePointId)
    {
        _chargePointStatuses.TryGetValue(chargePointId, out var status);
        return status;
    }

    public ConnectorStatus? GetConnectorStatus(string chargePointId, int connectorId)
    {
        if (_connectorStatuses.TryGetValue(chargePointId, out var connectors))
        {
            connectors.TryGetValue(connectorId, out var status);
            return status;
        }
        return null;
    }

    public Transaction? GetActiveTransaction(string chargePointId, int connectorId)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            transactions.TryGetValue(connectorId, out var transaction);
            return transaction;
        }
        return null;
    }

    public List<Transaction> GetAllActiveTransactions(string chargePointId)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            return transactions.Values.ToList();
        }
        return new List<Transaction>();
    }

    public Reservation? GetReservation(string chargePointId, int connectorId)
    {
        if (_reservations.TryGetValue(chargePointId, out var reservations))
        {
            reservations.TryGetValue(connectorId, out var reservation);
            return reservation;
        }
        return null;
    }

    public bool IsChargePointOnline(string chargePointId)
    {
        if (_chargePointStatuses.TryGetValue(chargePointId, out var status))
        {
            return status.IsOnline;
        }
        return false;
    }

    public List<string> GetAllConnectedChargePoints()
    {
        return _chargePointStatuses
            .Where(kvp => kvp.Value.IsOnline)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public void UpdateChargePointInfo(ChargePointInfo info)
    {
        _chargePointInfos.AddOrUpdate(info.ChargePointId, info, (_, _) => info);
    }

    public void UpdateConnectorStatus(string chargePointId, ConnectorStatus status)
    {
        var connectors = _connectorStatuses.GetOrAdd(chargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
        connectors.AddOrUpdate(status.ConnectorId, status, (_, existing) => 
        {
            // 保持Battery和其他字段不被简单的StatusNotification覆盖，除非新对象有值
            // 这里我们假设传入的status是一个基础的状态对象，所以我们需要保留现有的丰富数据
            // 但如果传入的是一个完整的对象，那就直接替换
            // 实际上，StatusNotification只包含 Status, ErrorCode, Info, Timestamp 等
            
            // 如果是简单的更新（没有Battery等信息），我们保留旧的Battery
            if (status.Battery == null && existing.Battery != null)
            {
                status.Battery = existing.Battery;
            }
            if (status.InstantVoltage == null && existing.InstantVoltage != null)
            {
                status.InstantVoltage = existing.InstantVoltage;
                status.InstantCurrent = existing.InstantCurrent;
                status.InstantPower = existing.InstantPower;
            }
            // 保留LastMeterValue
            if (status.LastMeterValue == null && existing.LastMeterValue != null)
            {
                status.LastMeterValue = existing.LastMeterValue;
            }
            
            return status;
        });

        // 同时更新ChargePointStatus中的ConnectorStatuses
        var cpStatus = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus { ChargePointId = chargePointId });
        if (cpStatus.ConnectorStatuses.ContainsKey(status.ConnectorId))
        {
            cpStatus.ConnectorStatuses[status.ConnectorId] = status;
        }
        else
        {
            cpStatus.ConnectorStatuses.Add(status.ConnectorId, status);
        }
    }

    public void UpdateBatteryStatus(string chargePointId, int connectorId, BatteryStatus batteryStatus)
    {
        var connectors = _connectorStatuses.GetOrAdd(chargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
        if (connectors.TryGetValue(connectorId, out var status))
        {
            status.Battery = batteryStatus;
        }
        else
        {
            // 如果连接器状态不存在，创建一个新的
            var newStatus = new ConnectorStatus
            {
                ConnectorId = connectorId,
                Battery = batteryStatus,
                Timestamp = DateTime.UtcNow
            };
            connectors.TryAdd(connectorId, newStatus);
        }
    }

    public void UpdateConnectorSnapshot(string chargePointId, int connectorId, decimal? voltage, decimal? current, decimal? power)
    {
        var connectors = _connectorStatuses.GetOrAdd(chargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
        if (connectors.TryGetValue(connectorId, out var status))
        {
            if (voltage.HasValue) status.InstantVoltage = voltage;
            if (current.HasValue) status.InstantCurrent = current;
            if (power.HasValue) status.InstantPower = power;
        }
    }

    public void UpdateFirmwareStatus(string chargePointId, FirmwareStatus status)
    {
        var cpStatus = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus { ChargePointId = chargePointId });
        cpStatus.FirmwareStatus = status;
    }

    public void UpdateDiagnosticsStatus(string chargePointId, DiagnosticsStatus status)
    {
        var cpStatus = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus { ChargePointId = chargePointId });
        cpStatus.DiagnosticsStatus = status;
    }

    public void UpdateLocalAuthListVersion(string chargePointId, int version)
    {
        var cpStatus = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus { ChargePointId = chargePointId });
        cpStatus.LocalAuthListVersion = version;
    }

    public void MarkOnline(string chargePointId)
    {
        var status = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus
        {
            ChargePointId = chargePointId
        });

        status.IsOnline = true;
        status.ConnectedAt = DateTime.UtcNow;
        status.LastCommunicationAt = DateTime.UtcNow;
    }

    public void MarkOffline(string chargePointId)
    {
        if (_chargePointStatuses.TryGetValue(chargePointId, out var status))
        {
            status.IsOnline = false;
        }
    }

    public void UpdateLastCommunication(string chargePointId)
    {
        if (_chargePointStatuses.TryGetValue(chargePointId, out var status))
        {
            status.LastCommunicationAt = DateTime.UtcNow;
        }
    }

    public void CreateTransaction(Transaction transaction)
    {
        var transactions = _activeTransactions.GetOrAdd(transaction.ChargePointId, 
            _ => new ConcurrentDictionary<int, Transaction>());
        
        // 由于现在Transaction支持多连接器，我们需要为每个连接器建立索引
        foreach (var connectorId in transaction.ConnectorIds)
        {
            transactions.AddOrUpdate(connectorId, transaction, (_, _) => transaction);

            // 更新连接器的CurrentTransactionId
            var connectors = _connectorStatuses.GetOrAdd(transaction.ChargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
            if (connectors.TryGetValue(connectorId, out var status))
            {
                status.CurrentTransactionId = transaction.TransactionId;
            }
        }
    }

    public void UpdateTransactionMeter(string chargePointId, int connectorId, int meterValue, ReadingContext context = ReadingContext.SamplePeriodic)
    {
        // 1. 更新连接器的 LastMeterValue
        var connectors = _connectorStatuses.GetOrAdd(chargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
        if (connectors.TryGetValue(connectorId, out var status))
        {
            status.LastMeterValue = meterValue;
        }

        // 2. 更新交易的总电量
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            if (transactions.TryGetValue(connectorId, out var transaction))
            {
                // 逻辑A: 如果是 Transaction.Begin，强制更新起始值
                if (context == ReadingContext.TransactionBegin)
                {
                    transaction.MeterStartValues[connectorId] = meterValue;
                    transaction.HasSetOfficialMeterStart[connectorId] = true;
                }
                // 逻辑B: 如果尚未设置正式起始值，则取第一个报送的值作为起始值
                else if (!transaction.HasSetOfficialMeterStart.TryGetValue(connectorId, out var hasSet) || !hasSet)
                {
                    // 只有当MeterStartValues中没有值或者值为0时（通常创建时会赋值StartTransaction中的MeterStart），
                    // 或者我们认为StartTransaction中的值可能不准，需要以第一个MeterValue为准。
                    // 根据需求描述：“否则则取交易创建后第一个报送的电表值作为充电起始值”
                    // 这意味着如果 StartTransaction 给的值不可信（比如0），或者我们仅仅依靠标志位。
                    // 这里我们假设 Transaction创建时已经填入了 StartTransactionRequest 中的值。
                    // 但如果没有收到过 Transaction.Begin，且这是第一个读数，我们是否要覆盖？
                    // 需求说：“取交易创建后第一个报送的电表值作为充电起始值”。
                    // 我们可以理解为：只要没收到过 Transaction.Begin，且这是该连接器第一次收到 MeterValue，就更新起始值？
                    // 或者更严谨一点：如果 StartTransaction 中的值是 0，且这是第一个读数，就更新。
                    // 为了简单且符合描述，我们检查是否已设置正式值。如果没有，就用当前值更新起始值，并标记为已设置（虽然不是Transaction.Begin，但也算作“第一个报送值”）
                    // 但要注意，后续的 SamplePeriodic 不应该再更新起始值了。
                    // 所以我们需要一个状态来记录“是否已经确定了起始值”。
                    
                    // 这里我们在 Transaction 中增加了一个 HasSetOfficialMeterStart 字典。
                    // 如果对应连接器没有被标记为 true，说明这是“第一个”报送的值（或者 Transaction.Begin 还没来）。
                    // 我们将其作为起始值。
                    transaction.MeterStartValues[connectorId] = meterValue;
                    transaction.HasSetOfficialMeterStart[connectorId] = true;
                }

                // 计算该交易所有连接器的总消耗电量
                long totalConsumed = 0;
                foreach (var cid in transaction.ConnectorIds)
                {
                    // 获取连接器当前读数（如果当前连接器就是 cid，使用 meterValue，否则查缓存）
                    int currentReading = 0;
                    if (cid == connectorId)
                    {
                        currentReading = meterValue;
                    }
                    else if (connectors.TryGetValue(cid, out var connStatus) && connStatus.LastMeterValue.HasValue)
                    {
                        currentReading = connStatus.LastMeterValue.Value;
                    }

                    // 获取起始读数
                    if (transaction.MeterStartValues.TryGetValue(cid, out int startReading))
                    {
                        // 累加增量 (当前 - 起始)，防止负数
                        if (currentReading >= startReading)
                        {
                            totalConsumed += (currentReading - startReading);
                        }
                    }
                }

                transaction.TotalConsumedEnergy = (int)totalConsumed;
                transaction.LastMeterUpdateAt = DateTime.UtcNow;
                transaction.LastUpdated = DateTime.UtcNow;
            }
        }
    }

    public void UpdateTransactionSnapshot(string chargePointId, int connectorId, decimal? power)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            if (transactions.TryGetValue(connectorId, out var transaction))
            {
                // 只更新功率
                if (power.HasValue) transaction.CurrentPower = power;
                transaction.LastUpdated = DateTime.UtcNow;
            }
        }
    }

    public void EndTransaction(string chargePointId, int transactionId)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            // 找到对应的交易
            var transaction = transactions.Values.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction != null)
            {
                // 从所有关联的连接器索引中移除
                foreach (var connectorId in transaction.ConnectorIds)
                {
                    transactions.TryRemove(connectorId, out _);

                    // 清除连接器的CurrentTransactionId
                    var connectors = _connectorStatuses.GetOrAdd(chargePointId, _ => new ConcurrentDictionary<int, ConnectorStatus>());
                    if (connectors.TryGetValue(connectorId, out var status))
                    {
                        status.CurrentTransactionId = null;
                    }
                }
            }
        }
    }

    public void CreateReservation(Reservation reservation)
    {
        var reservations = _reservations.GetOrAdd(reservation.ChargePointId,
            _ => new ConcurrentDictionary<int, Reservation>());
        reservations.AddOrUpdate(reservation.ConnectorId, reservation, (_, _) => reservation);
    }

    public void CancelReservation(string chargePointId, int reservationId)
    {
        if (_reservations.TryGetValue(chargePointId, out var reservations))
        {
            var reservation = reservations.Values.FirstOrDefault(r => r.ReservationId == reservationId);
            if (reservation != null)
            {
                reservations.TryRemove(reservation.ConnectorId, out _);
            }
        }
    }

    public ConfigurationItem? GetConfiguration(string chargePointId, string key)
    {
        if (_configurations.TryGetValue(chargePointId, out var configs))
        {
            configs.TryGetValue(key, out var item);
            return item;
        }
        return null;
    }

    public List<ConfigurationItem> GetAllConfigurations(string chargePointId)
    {
        if (_configurations.TryGetValue(chargePointId, out var configs))
        {
            return configs.Values.ToList();
        }
        return new List<ConfigurationItem>();
    }

    public void UpdateConfiguration(string chargePointId, ConfigurationItem item)
    {
        var configs = _configurations.GetOrAdd(chargePointId,
            _ => new ConcurrentDictionary<string, ConfigurationItem>(StringComparer.OrdinalIgnoreCase));
        configs.AddOrUpdate(item.Key, item, (_, _) => item);

        // 同时更新ChargePointStatus中的ConfigurationItems
        var cpStatus = _chargePointStatuses.GetOrAdd(chargePointId, _ => new CSMSNet.OcppAdapter.Models.State.ChargePointStatus { ChargePointId = chargePointId });
        cpStatus.ConfigurationItems.AddOrUpdate(item.Key, item.Value ?? string.Empty, (_, _) => item.Value ?? string.Empty);
    }
}
