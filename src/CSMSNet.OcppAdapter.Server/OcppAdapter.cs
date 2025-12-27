using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.State;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.OcppAdapter.Protocol.V16;
using CSMSNet.OcppAdapter.Server.Handlers;
using CSMSNet.OcppAdapter.Server.State;
using CSMSNet.OcppAdapter.Server.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server;

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
    private readonly CommandHandler _commandHandler;
    private readonly WebSocketServer _webSocketServer;
    private readonly ILogger<WebSocketServer>? _webSocketLogger;

    public OcppAdapter(
        OcppAdapterConfiguration configuration,
        ILogger<OcppAdapter>? logger = null,
        ILogger<WebSocketServer>? webSocketLogger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
        _webSocketLogger = webSocketLogger;
        
        // 初始化核心组件
        _stateCache = new StateCache();
        _protocolHandler = new Ocpp16ProtocolHandler();
        _connectionManager = new ConnectionManager(_configuration);
        _callMatcher = new CallMatcher(_configuration);
        
        // 创建RequestHandler (先不带Router)
        var requestHandlerImpl = new RequestHandler(_stateCache, null!, _configuration);
        _requestHandler = requestHandlerImpl;
        
        // 创建Router (带RequestHandler)
        _messageRouter = new MessageRouter(_protocolHandler, _requestHandler, _callMatcher, _connectionManager, _configuration);
        
        // 重新创建RequestHandler (带Router) - 为了解决循环依赖，这里使用反射或者直接创建新的实例比较好
        // 但由于RequestHandler内部只依赖IMessageRouter接口，我们可以通过反射设置私有字段，或者修改构造逻辑。
        // 为了简单起见，我们重新创建一个新的RequestHandler，并将旧的丢弃。
        // 更好的做法是使用Setter注入或者延迟加载，但这里我们直接重新创建。
        _requestHandler = new RequestHandler(_stateCache, _messageRouter, _configuration, null); // Logger暂空或者传入
        
        // 实际上，MessageRouter依赖RequestHandler, RequestHandler依赖MessageRouter。
        // 这是一个典型的循环依赖。
        // 我们可以让MessageRouter只依赖IRequestHandler接口，而RequestHandler依赖IMessageRouter接口。
        // 解决办法：
        // 1. 使用属性注入 (Setter Injection)
        // 2. 使用Lazy<T>
        // 3. 在MessageRouter中延迟获取RequestHandler (如果支持)
        
        // 当前代码结构中，RequestHandler需要MessageRouter来发送响应。MessageRouter需要RequestHandler来分发请求。
        // 我们修改一下MessageRouter的构造，允许后续设置RequestHandler? 不，MessageRouter是核心。
        // 让我们看看MessageRouter的代码，它需要IRequestHandler。
        
        // 我们可以先创建MessageRouter，传入一个空的RequestHandler代理，然后再设置真正的Handler?
        // 或者，我们可以修改RequestHandler，使其MessageRouter属性可设置。
        
        // 这里采用最简单的方法：重新创建Router，这次传入真正的RequestHandler。
        // 这里的循环依赖必须解开。
        // 方案：MessageRouter 不直接依赖 RequestHandler，而是依赖一个 IRequestDispatcher 接口?
        // 或者，我们可以创建一个 IMessageSender 接口供 RequestHandler 使用，MessageRouter 实现它。
        
        // 为了不改动太多现有代码，我们这里使用一种稍微“脏”一点的方法：
        // 既然我们控制了实例化，我们可以先创建Router，传入null的Handler（如果允许），然后再设置。
        // 但MessageRouter构造函数要求非空。
        
        // 让我们检查一下RequestHandler的构造函数。它需要IMessageRouter。
        // 让我们检查一下MessageRouter的构造函数。它需要IRequestHandler。
        
        // 这是一个死锁。必须打破。
        // 我们修改MessageRouter，使其允许RequestHandler为null，并提供SetRequestHandler方法?
        // 或者，我们使用一个中间层。
        
        // 鉴于我不能轻易修改MessageRouter的签名（它在之前步骤中已被确立），我将使用一个Wrapper。
        // 但实际上，在C#中，引用类型是引用传递的。
        // 我们可以创建一个实现了IRequestHandler的Wrapper类。
        
        var requestHandlerWrapper = new RequestHandlerWrapper();
        _messageRouter = new MessageRouter(_protocolHandler, requestHandlerWrapper, _callMatcher, _connectionManager, _configuration);
        
        // 现在创建真正的RequestHandler，传入Router
        var realRequestHandler = new RequestHandler(_stateCache, _messageRouter, _configuration, null, _connectionManager);
        
        // 将真正的Handler注入Wrapper
        requestHandlerWrapper.SetInner(realRequestHandler);
        _requestHandler = realRequestHandler;
        
        // 初始化CommandHandler
        _commandHandler = new CommandHandler(_protocolHandler, _callMatcher, _connectionManager, _configuration);
        
        // 初始化WebSocketServer (注入this以访问状态)
        _webSocketServer = new WebSocketServer(_configuration, _connectionManager, _messageRouter, _webSocketLogger, this);
        
        // 绑定事件
        BindEvents();
    }
    
    // 用于解决循环依赖的Wrapper
    private class RequestHandlerWrapper : IRequestHandler
    {
        private IRequestHandler? _inner;
        
        public void SetInner(IRequestHandler inner) => _inner = inner;

        public event EventHandler<BootNotificationEventArgs>? OnBootNotification { add { } remove { } }
        public event EventHandler<HeartbeatEventArgs>? OnHeartbeat { add { } remove { } }
        public event EventHandler<StatusNotificationEventArgs>? OnStatusNotification { add { } remove { } }
        public event EventHandler<AuthorizeEventArgs>? OnAuthorize { add { } remove { } }
        public event EventHandler<StartTransactionEventArgs>? OnStartTransaction { add { } remove { } }
        public event EventHandler<StopTransactionEventArgs>? OnStopTransaction { add { } remove { } }
        public event EventHandler<MeterValuesEventArgs>? OnMeterValues { add { } remove { } }
        public event EventHandler<DataTransferEventArgs>? OnDataTransfer { add { } remove { } }
        public event EventHandler<DiagnosticsStatusNotificationEventArgs>? OnDiagnosticsStatusNotification { add { } remove { } }
        public event EventHandler<FirmwareStatusNotificationEventArgs>? OnFirmwareStatusNotification { add { } remove { } }

        public Task HandleRequestAsync(string chargePointId, OcppRequest request)
        {
            if (_inner == null) throw new InvalidOperationException("RequestHandler not initialized");
            return _inner.HandleRequestAsync(chargePointId, request);
        }
        
        // Wrapper需要转发事件订阅吗？
        // 实际上MessageRouter只调用HandleRequestAsync，不订阅事件。
        // 事件是由OcppAdapter订阅的，它订阅的是_requestHandler (即realRequestHandler)。
        // 所以Wrapper不需要处理事件。
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
        return _webSocketServer.HandleWebSocketAsync(context);
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

    public ChargePointStatus? GetChargePointStatus(string chargePointId)
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
        return _commandHandler.RemoteStartTransactionAsync(chargePointId, request, cancellationToken);
    }

    public Task<RemoteStopTransactionResponse> RemoteStopTransactionAsync(
        string chargePointId,
        RemoteStopTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.RemoteStopTransactionAsync(chargePointId, request, cancellationToken);
    }

    public Task<ResetResponse> ResetAsync(
        string chargePointId,
        ResetRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ResetAsync(chargePointId, request, cancellationToken);
    }

    public Task<UnlockConnectorResponse> UnlockConnectorAsync(
        string chargePointId,
        UnlockConnectorRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.UnlockConnectorAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetConfigurationResponse> GetConfigurationAsync(
        string chargePointId,
        GetConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.GetConfigurationAsync(chargePointId, request, cancellationToken);
    }

    public Task<ChangeConfigurationResponse> ChangeConfigurationAsync(
        string chargePointId,
        ChangeConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ChangeConfigurationAsync(chargePointId, request, cancellationToken);
    }

    public Task<ClearCacheResponse> ClearCacheAsync(
        string chargePointId,
        ClearCacheRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ClearCacheAsync(chargePointId, request, cancellationToken);
    }

    public Task<DataTransferResponse> DataTransferAsync(
        string chargePointId,
        DataTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.DataTransferAsync(chargePointId, request, cancellationToken);
    }

    public Task<ChangeAvailabilityResponse> ChangeAvailabilityAsync(
        string chargePointId,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ChangeAvailabilityAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetDiagnosticsResponse> GetDiagnosticsAsync(
        string chargePointId,
        GetDiagnosticsRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.GetDiagnosticsAsync(chargePointId, request, cancellationToken);
    }

    public Task<UpdateFirmwareResponse> UpdateFirmwareAsync(
        string chargePointId,
        UpdateFirmwareRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.UpdateFirmwareAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetLocalListVersionResponse> GetLocalListVersionAsync(
        string chargePointId,
        GetLocalListVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.GetLocalListVersionAsync(chargePointId, request, cancellationToken);
    }

    public Task<SendLocalListResponse> SendLocalListAsync(
        string chargePointId,
        SendLocalListRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.SendLocalListAsync(chargePointId, request, cancellationToken);
    }

    public Task<CancelReservationResponse> CancelReservationAsync(
        string chargePointId,
        CancelReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.CancelReservationAsync(chargePointId, request, cancellationToken);
    }

    public Task<ReserveNowResponse> ReserveNowAsync(
        string chargePointId,
        ReserveNowRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ReserveNowAsync(chargePointId, request, cancellationToken);
    }

    public Task<ClearChargingProfileResponse> ClearChargingProfileAsync(
        string chargePointId,
        ClearChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.ClearChargingProfileAsync(chargePointId, request, cancellationToken);
    }

    public Task<GetCompositeScheduleResponse> GetCompositeScheduleAsync(
        string chargePointId,
        GetCompositeScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.GetCompositeScheduleAsync(chargePointId, request, cancellationToken);
    }

    public Task<SetChargingProfileResponse> SetChargingProfileAsync(
        string chargePointId,
        SetChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.SetChargingProfileAsync(chargePointId, request, cancellationToken);
    }

    public Task<TriggerMessageResponse> TriggerMessageAsync(
        string chargePointId,
        TriggerMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _commandHandler.TriggerMessageAsync(chargePointId, request, cancellationToken);
    }


    #endregion
}
