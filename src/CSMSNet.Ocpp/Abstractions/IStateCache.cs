using CSMSNet.OcppAdapter.Models.State;

namespace CSMSNet.OcppAdapter.Abstractions;

/// <summary>
/// 状态缓存管理器接口
/// </summary>
public interface IStateCache
{
    /// <summary>
    /// 获取充电桩基础信息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>充电桩信息（可空）</returns>
    ChargePointInfo? GetChargePointInfo(string chargePointId);

    /// <summary>
    /// 获取充电桩状态
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>充电桩状态（可空）</returns>
    ChargePointStatus? GetChargePointStatus(string chargePointId);

    /// <summary>
    /// 获取连接器状态
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="connectorId">连接器ID</param>
    /// <returns>连接器状态（可空）</returns>
    ConnectorStatus? GetConnectorStatus(string chargePointId, int connectorId);

    /// <summary>
    /// 获取活跃事务
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="connectorId">连接器ID</param>
    /// <returns>事务（可空）</returns>
    Transaction? GetActiveTransaction(string chargePointId, int connectorId);

    /// <summary>
    /// 获取充电桩所有活跃事务
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>事务列表</returns>
    List<Transaction> GetAllActiveTransactions(string chargePointId);

    /// <summary>
    /// 获取预约信息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="connectorId">连接器ID</param>
    /// <returns>预约信息（可空）</returns>
    Reservation? GetReservation(string chargePointId, int connectorId);

    /// <summary>
    /// 判断充电桩是否在线
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>是否在线</returns>
    bool IsChargePointOnline(string chargePointId);

    /// <summary>
    /// 获取所有在线充电桩ID
    /// </summary>
    /// <returns>充电桩ID列表</returns>
    List<string> GetAllConnectedChargePoints();

    /// <summary>
    /// 更新充电桩基础信息
    /// </summary>
    /// <param name="info">充电桩信息</param>
    void UpdateChargePointInfo(ChargePointInfo info);

    /// <summary>
    /// 更新连接器状态
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="status">连接器状态</param>
    void UpdateConnectorStatus(string chargePointId, ConnectorStatus status);

    /// <summary>
    /// 标记充电桩在线
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    void MarkOnline(string chargePointId);

    /// <summary>
    /// 标记充电桩离线
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    void MarkOffline(string chargePointId);

    /// <summary>
    /// 更新最后通信时间
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    void UpdateLastCommunication(string chargePointId);

    /// <summary>
    /// 创建活跃事务
    /// </summary>
    /// <param name="transaction">事务对象</param>
    void CreateTransaction(Transaction transaction);

    /// <summary>
    /// 更新事务电量
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="connectorId">连接器ID</param>
    /// <param name="meterValue">电量值</param>
    void UpdateTransactionMeter(string chargePointId, int connectorId, int meterValue);

    /// <summary>
    /// 结束事务
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="transactionId">事务ID</param>
    void EndTransaction(string chargePointId, int transactionId);

    /// <summary>
    /// 创建预约
    /// </summary>
    /// <param name="reservation">预约对象</param>
    void CreateReservation(Reservation reservation);

    /// <summary>
    /// 取消预约
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="reservationId">预约ID</param>
    void CancelReservation(string chargePointId, int reservationId);

    /// <summary>
    /// 获取配置项
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="key">配置键</param>
    /// <returns>配置项（可空）</returns>
    ConfigurationItem? GetConfiguration(string chargePointId, string key);

    /// <summary>
    /// 获取所有配置项
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <returns>配置项列表</returns>
    List<ConfigurationItem> GetAllConfigurations(string chargePointId);

    /// <summary>
    /// 更新配置项
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="item">配置项</param>
    void UpdateConfiguration(string chargePointId, ConfigurationItem item);
}
