using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.State;
using CSMSNet.OcppAdapter.Models.V16.Enums; // Ensure enums are available
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.Ocpp.V16;
using CSMSNet.Ocpp.Handlers;
using CSMSNet.Ocpp.State;
using CSMSNet.Ocpp.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp;

/// <summary>
/// OCPP适配器主类
/// 实现IOcppAdapter业务接口和IHostedService生命周期接口
/// </summary>
public class OcppAdapter : IOcppAdapter, IHostedService
{
    private readonly OcppAdapterConfiguration _configuration;
    private readonly IStateCache _stateCache;
    private readonly ILogger<OcppAdapter>? _logger;
    private readonly IOcppProtocolHandler _protocolHandler;
    private readonly IConnectionManager _connectionManager;
    private readonly ICallMatcher _callMatcher;
    private readonly IMessageRouter _messageRouter;
    private readonly IRequestHandler _requestHandler;
    private readonly OcppCommandSender _commandSender;
    private readonly OcppWebSocketHandler _webSocketHandler;
    private readonly ILogger<OcppWebSocketHandler>? _webSocketLogger;

    public OcppAdapter(
        OcppAdapterConfiguration configuration,
        ILogger<OcppAdapter>? logger = null,
        ILogger<OcppWebSocketHandler>? webSocketLogger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
        _webSocketLogger = webSocketLogger;
        
        // 初始化核心组件
        _stateCache = new StateCache();
        _protocolHandler = new Ocpp16ProtocolHandler();
        _connectionManager = new ConnectionManager(_configuration);
        _callMatcher = new CallMatcher(_configuration);
        
        // 创建RequestHandler (无需Router)
        _requestHandler = new RequestHandler(_stateCache, _configuration, null, _connectionManager);
        
        // 创建Router (带RequestHandler)
        _messageRouter = new MessageRouter(_protocolHandler, _requestHandler, _callMatcher, _connectionManager, _configuration);
        
        // 初始化CommandSender
        _commandSender = new OcppCommandSender(_protocolHandler, _callMatcher, _connectionManager, _configuration);
        
        // 初始化WebSocketHandler (注入this以访问状态)
        _webSocketHandler = new OcppWebSocketHandler(_configuration, _connectionManager, _messageRouter, _webSocketLogger, this);
        
        // 绑定事件
        BindEvents();
    }
    
    private void BindEvents()
    {
        // 绑定RequestHandler事件
        _requestHandler.OnBootNotification += (s, e) => OnBootNotification?.Invoke(this, e);
        _requestHandler.OnHeartbeat += (s, e) => OnHeartbeat?.Invoke(this, e);
        _requestHandler.OnStatusNotification += (s, e) => OnStatusNotification?.Invoke(this, e);
        _requestHandler.OnAuthorize += (s, e) => OnAuthorize?.Invoke(this, e);
        _requestHandler.OnStartTransaction += (s, e) => OnStartTransaction?.Invoke(this, e);
        _requestHandler.OnStopTransaction += (s, e) => OnStopTransaction?.Invoke(this, e);
        _requestHandler.OnMeterValues += (s, e) => OnMeterValues?.Invoke(this, e);
        _requestHandler.OnDataTransfer += (s, e) => OnDataTransfer?.Invoke(this, e);
        _requestHandler.OnDiagnosticsStatusNotification += (s, e) => OnDiagnosticsStatusNotification?.Invoke(this, e);
        _requestHandler.OnFirmwareStatusNotification += (s, e) => OnFirmwareStatusNotification?.Invoke(this, e);

        // 绑定ConnectionManager事件
        _connectionManager.OnChargePointConnected += (s, e) => OnChargePointConnected?.Invoke(this, e);
        _connectionManager.OnChargePointDisconnected += (s, e) => OnChargePointDisconnected?.Invoke(this, e);
        _connectionManager.OnSessionCreated += (s, e) => OnSessionCreated?.Invoke(this, e);
        _connectionManager.OnSessionClosed += (s, e) => OnSessionClosed?.Invoke(this, e);
    }

    /// <summary>
    /// 启动后台服务 (IHostedService实现)
    /// 注意: 此方法由Host自动调用，用于启动后台清理任务等。
    /// WebSocket服务由ASP.NET Core Middleware (OcppWebSocketMiddleware) 处理，不在此处启动。
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("OCPP Adapter background service starting...");
        
        // 启动后台监控任务
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_configuration.SessionCleanupInterval, cancellationToken);
                    await _connectionManager.CleanupInactiveSessions();
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in background monitoring task");
                }
            }
        }, cancellationToken);
        
        _logger?.LogInformation(
            "OCPP Adapter started successfully. Protocol: {Version}, Max Connections: {MaxConnections}",
            _protocolHandler.GetSupportedVersion(),
            _configuration.MaxConcurrentConnections);
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// 处理WebSocket请求 (供Middleware调用)
    /// </summary>
    public Task HandleWebSocketAsync(HttpContext context)
    {
        return _webSocketHandler.HandleWebSocketAsync(context);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("OCPP Adapter background service stopping");
        
        // 关闭所有连接
        await _connectionManager.CloseAllSessions();
        
        // 释放资源
        if (_callMatcher is IDisposable disposableCallMatcher)
        {
            disposableCallMatcher.Dispose();
        }
        
        _logger?.LogInformation("OCPP Adapter stopped");
    }

    public ChargePointInfo? GetChargePointInfo(string chargePointId)
    {
        return _stateCache.GetChargePointInfo(chargePointId);
    }

    public CSMSNet.OcppAdapter.Models.State.ChargePointStatus? GetChargePointStatus(string chargePointId)
    {
        return _stateCache.GetChargePointStatus(chargePointId);
    }

    public ConnectorStatus? GetConnectorStatus(string chargePointId, int connectorId)
    {
        return _stateCache.GetConnectorStatus(chargePointId, connectorId);
    }

    public Transaction? GetActiveTransaction(string chargePointId, int connectorId)
    {
        return _stateCache.GetActiveTransaction(chargePointId, connectorId);
    }

    public List<Transaction> GetAllActiveTransactions(string chargePointId)
    {
        return _stateCache.GetAllActiveTransactions(chargePointId);
    }

    public Reservation? GetReservation(string chargePointId, int connectorId)
    {
        return _stateCache.GetReservation(chargePointId, connectorId);
    }

    public bool IsChargePointOnline(string chargePointId)
    {
        return _stateCache.IsChargePointOnline(chargePointId);
    }

    public List<string> GetAllConnectedChargePoints()
    {
        return _stateCache.GetAllConnectedChargePoints();
    }

    #region 事件定义

    public event EventHandler<ChargePointConnectedEventArgs>? OnChargePointConnected;
    public event EventHandler<ChargePointDisconnectedEventArgs>? OnChargePointDisconnected;
    public event EventHandler<ChargePointConnectedEventArgs>? OnSessionCreated;
    public event EventHandler<ChargePointDisconnectedEventArgs>? OnSessionClosed;
    public event EventHandler<BootNotificationEventArgs>? OnBootNotification;
    public event EventHandler<StatusNotificationEventArgs>? OnStatusNotification;
    public event EventHandler<StartTransactionEventArgs>? OnStartTransaction;
    public event EventHandler<StopTransactionEventArgs>? OnStopTransaction;
    public event EventHandler<AuthorizeEventArgs>? OnAuthorize;
    public event EventHandler<MeterValuesEventArgs>? OnMeterValues;
    public event EventHandler<HeartbeatEventArgs>? OnHeartbeat;
    public event EventHandler<DataTransferEventArgs>? OnDataTransfer;
    public event EventHandler<DiagnosticsStatusNotificationEventArgs>? OnDiagnosticsStatusNotification;
    public event EventHandler<FirmwareStatusNotificationEventArgs>? OnFirmwareStatusNotification;

    #endregion

    #region 指令调用接口

    public Task<RemoteStartTransactionResponse> RemoteStartTransactionAsync(
        string chargePointId,
        RemoteStartTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.RemoteStartTransactionAsync(chargePointId, request, cancellationToken);
    }

    public Task<RemoteStopTransactionResponse> RemoteStopTransactionAsync(
        string chargePointId,
        RemoteStopTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.RemoteStopTransactionAsync(chargePointId, request, cancellationToken);
    }

    public Task<ResetResponse> ResetAsync(
        string chargePointId,
        ResetRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.ResetAsync(chargePointId, request, cancellationToken);
    }

    public Task<UnlockConnectorResponse> UnlockConnectorAsync(
        string chargePointId,
        UnlockConnectorRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.UnlockConnectorAsync(chargePointId, request, cancellationToken);
    }

    public async Task<GetConfigurationResponse> GetConfigurationAsync(
        string chargePointId,
        GetConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _commandSender.GetConfigurationAsync(chargePointId, request, cancellationToken);

        // 更新缓存
        if (response.ConfigurationKey != null)
        {
            foreach (var item in response.ConfigurationKey)
            {
                _stateCache.UpdateConfiguration(chargePointId, new ConfigurationItem
                {
                    Key = item.Key,
                    Value = item.Value,
                    Readonly = item.Readonly
                });
            }
        }

        return response;
    }

    public async Task<ChangeConfigurationResponse> ChangeConfigurationAsync(
        string chargePointId,
        ChangeConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _commandSender.ChangeConfigurationAsync(chargePointId, request, cancellationToken);

        // 如果设置成功，更新缓存
        if (response.Status == ConfigurationStatus.Accepted || response.Status == ConfigurationStatus.RebootRequired)
        {
            _stateCache.UpdateConfiguration(chargePointId, new ConfigurationItem
            {
                Key = request.Key,
                Value = request.Value,
                Readonly = false // 假设能修改则不是只读，或者我们不知道，但缓存值是最重要的
            });
        }

        return response;
    }

    public Task<ClearCacheResponse> ClearCacheAsync(
        string chargePointId,
        ClearCacheRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.ClearCacheAsync(chargePointId, request, cancellationToken);
    }

    public Task<DataTransferResponse> DataTransferAsync(
        string chargePointId,
        DataTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.DataTransferAsync(chargePointId, request, cancellationToken);
    }

    public Task<ChangeAvailabilityResponse> ChangeAvailabilityAsync(
        string chargePointId,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.ChangeAvailabilityAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetDiagnosticsResponse> GetDiagnosticsAsync(
        string chargePointId,
        GetDiagnosticsRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.GetDiagnosticsAsync(chargePointId, request, cancellationToken);
    }

    public Task<UpdateFirmwareResponse> UpdateFirmwareAsync(
        string chargePointId,
        UpdateFirmwareRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.UpdateFirmwareAsync(chargePointId, request, cancellationToken);
    }

    public async Task<GetLocalListVersionResponse> GetLocalListVersionAsync(
        string chargePointId,
        GetLocalListVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _commandSender.GetLocalListVersionAsync(chargePointId, request, cancellationToken);

        if (response.ListVersion >= 0)
        {
            _stateCache.UpdateLocalAuthListVersion(chargePointId, response.ListVersion);
        }

        return response;
    }

    public async Task<SendLocalListResponse> SendLocalListAsync(
        string chargePointId,
        SendLocalListRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _commandSender.SendLocalListAsync(chargePointId, request, cancellationToken);

        // 如果设置成功，更新缓存版本
        if (response.Status == UpdateStatus.Accepted)
        {
            _stateCache.UpdateLocalAuthListVersion(chargePointId, request.ListVersion);
        }

        return response;
    }

    public Task<CancelReservationResponse> CancelReservationAsync(
        string chargePointId,
        CancelReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.CancelReservationAsync(chargePointId, request, cancellationToken);
    }

    public Task<ReserveNowResponse> ReserveNowAsync(
        string chargePointId,
        ReserveNowRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.ReserveNowAsync(chargePointId, request, cancellationToken);
    }

    public Task<ClearChargingProfileResponse> ClearChargingProfileAsync(
        string chargePointId,
        ClearChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.ClearChargingProfileAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetCompositeScheduleResponse> GetCompositeScheduleAsync(
        string chargePointId,
        GetCompositeScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.GetCompositeScheduleAsync(chargePointId, request, cancellationToken);
    }

    public Task<SetChargingProfileResponse> SetChargingProfileAsync(
        string chargePointId,
        SetChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.SetChargingProfileAsync(chargePointId, request, cancellationToken);
    }

    public Task<TriggerMessageResponse> TriggerMessageAsync(
        string chargePointId,
        TriggerMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandSender.TriggerMessageAsync(chargePointId, request, cancellationToken);
    }


    #endregion
}
