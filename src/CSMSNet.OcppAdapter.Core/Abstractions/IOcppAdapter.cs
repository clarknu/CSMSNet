using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.State;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.OcppAdapter.Abstractions;

/// <summary>
/// OCPP适配器主接口
/// </summary>
public interface IOcppAdapter
{
    /// <summary>
    /// 启动适配器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止适配器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken = default);

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
    /// 获取所有活跃事务
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

    #region 事件定义

    /// <summary>
    /// 充电桩连接事件
    /// </summary>
    event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected;

    /// <summary>
    /// 充电桩断开连接事件
    /// </summary>
    event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected;

    /// <summary>
    /// BootNotification事件
    /// </summary>
    event EventHandler<BootNotificationEventArgs>? OnBootNotification;

    /// <summary>
    /// StatusNotification事件
    /// </summary>
    event EventHandler<StatusNotificationEventArgs>? OnStatusNotification;

    /// <summary>
    /// StartTransaction事件
    /// </summary>
    event EventHandler<StartTransactionEventArgs>? OnStartTransaction;

    /// <summary>
    /// StopTransaction事件
    /// </summary>
    event EventHandler<StopTransactionEventArgs>? OnStopTransaction;

    /// <summary>
    /// Authorize事件
    /// </summary>
    event EventHandler<AuthorizeEventArgs>? OnAuthorize;

    /// <summary>
    /// MeterValues事件
    /// </summary>
    event EventHandler<MeterValuesEventArgs>? OnMeterValues;

    /// <summary>
    /// Heartbeat事件
    /// </summary>
    event EventHandler<HeartbeatEventArgs>? OnHeartbeat;

    /// <summary>
    /// DataTransfer事件
    /// </summary>
    event EventHandler<DataTransferEventArgs>? OnDataTransfer;

    /// <summary>
    /// DiagnosticsStatusNotification事件
    /// </summary>
    event EventHandler<DiagnosticsStatusNotificationEventArgs>? OnDiagnosticsStatusNotification;

    /// <summary>
    /// FirmwareStatusNotification事件
    /// </summary>
    event EventHandler<FirmwareStatusNotificationEventArgs>? OnFirmwareStatusNotification;

    #endregion

    #region 指令调用接口

    /// <summary>
    /// 远程启动充电
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<RemoteStartTransactionResponse> RemoteStartTransactionAsync(
        string chargePointId,
        RemoteStartTransactionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 远程停止充电
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<RemoteStopTransactionResponse> RemoteStopTransactionAsync(
        string chargePointId,
        RemoteStopTransactionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置充电桩
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ResetResponse> ResetAsync(
        string chargePointId,
        ResetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 解锁连接器
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<UnlockConnectorResponse> UnlockConnectorAsync(
        string chargePointId,
        UnlockConnectorRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<GetConfigurationResponse> GetConfigurationAsync(
        string chargePointId,
        GetConfigurationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 修改配置
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ChangeConfigurationResponse> ChangeConfigurationAsync(
        string chargePointId,
        ChangeConfigurationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ClearCacheResponse> ClearCacheAsync(
        string chargePointId,
        ClearCacheRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 数据传输
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<DataTransferResponse> DataTransferAsync(
        string chargePointId,
        DataTransferRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更改可用性
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ChangeAvailabilityResponse> ChangeAvailabilityAsync(
        string chargePointId,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取诊断信息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<GetDiagnosticsResponse> GetDiagnosticsAsync(
        string chargePointId,
        GetDiagnosticsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新固件
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<UpdateFirmwareResponse> UpdateFirmwareAsync(
        string chargePointId,
        UpdateFirmwareRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取本地列表版本
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<GetLocalListVersionResponse> GetLocalListVersionAsync(
        string chargePointId,
        GetLocalListVersionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送本地列表
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<SendLocalListResponse> SendLocalListAsync(
        string chargePointId,
        SendLocalListRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消预约
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<CancelReservationResponse> CancelReservationAsync(
        string chargePointId,
        CancelReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 立即预约
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ReserveNowResponse> ReserveNowAsync(
        string chargePointId,
        ReserveNowRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除充电配置
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<ClearChargingProfileResponse> ClearChargingProfileAsync(
        string chargePointId,
        ClearChargingProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取组合计划
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<GetCompositeScheduleResponse> GetCompositeScheduleAsync(
        string chargePointId,
        GetCompositeScheduleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置充电配置
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<SetChargingProfileResponse> SetChargingProfileAsync(
        string chargePointId,
        SetChargingProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 触发消息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应对象</returns>
    Task<TriggerMessageResponse> TriggerMessageAsync(
        string chargePointId,
        TriggerMessageRequest request,
        CancellationToken cancellationToken = default);

    #endregion
}
