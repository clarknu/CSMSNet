using System.Collections.Concurrent;
using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Models.State;

namespace CSMSNet.OcppAdapter.Server.State;

/// <summary>
/// 状态缓存管理器实现
/// </summary>
public class StateCache : IStateCache
{
    private readonly ConcurrentDictionary<string, ChargePointInfo> _chargePointInfos = new();
    private readonly ConcurrentDictionary<string, ChargePointStatus> _chargePointStatuses = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ConnectorStatus>> _connectorStatuses = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Transaction>> _activeTransactions = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Reservation>> _reservations = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConfigurationItem>> _configurations = new();

    public ChargePointInfo? GetChargePointInfo(string chargePointId)
    {
        _chargePointInfos.TryGetValue(chargePointId, out var info);
        return info;
    }

    public ChargePointStatus? GetChargePointStatus(string chargePointId)
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
        connectors.AddOrUpdate(status.ConnectorId, status, (_, _) => status);
    }

    public void MarkOnline(string chargePointId)
    {
        var status = _chargePointStatuses.GetOrAdd(chargePointId, _ => new ChargePointStatus
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
        transactions.AddOrUpdate(transaction.ConnectorId, transaction, (_, _) => transaction);
    }

    public void UpdateTransactionMeter(string chargePointId, int connectorId, int meterValue)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            if (transactions.TryGetValue(connectorId, out var transaction))
            {
                transaction.MeterCurrent = meterValue;
                transaction.LastMeterUpdateAt = DateTime.UtcNow;
            }
        }
    }

    public void EndTransaction(string chargePointId, int transactionId)
    {
        if (_activeTransactions.TryGetValue(chargePointId, out var transactions))
        {
            var transaction = transactions.Values.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction != null)
            {
                transactions.TryRemove(transaction.ConnectorId, out _);
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
    }
}
