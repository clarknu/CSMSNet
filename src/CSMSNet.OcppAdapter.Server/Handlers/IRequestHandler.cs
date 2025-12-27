using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.Events;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 请求处理器接口
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// 处理充电桩请求
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <returns>响应消息</returns>
    Task<IOcppMessage> HandleRequestAsync(string chargePointId, OcppRequest request);

    event EventHandler<BootNotificationEventArgs>? OnBootNotification;
    event EventHandler<HeartbeatEventArgs>? OnHeartbeat;
    event EventHandler<StatusNotificationEventArgs>? OnStatusNotification;
    event EventHandler<AuthorizeEventArgs>? OnAuthorize;
    event EventHandler<StartTransactionEventArgs>? OnStartTransaction;
    event EventHandler<StopTransactionEventArgs>? OnStopTransaction;
    event EventHandler<MeterValuesEventArgs>? OnMeterValues;
    event EventHandler<DataTransferEventArgs>? OnDataTransfer;
    event EventHandler<DiagnosticsStatusNotificationEventArgs>? OnDiagnosticsStatusNotification;
    event EventHandler<FirmwareStatusNotificationEventArgs>? OnFirmwareStatusNotification;
}
