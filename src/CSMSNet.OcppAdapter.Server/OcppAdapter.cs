using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.State;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.OcppAdapter.Protocol.V16;
using CSMSNet.OcppAdapter.Server.Handlers;
using CSMSNet.OcppAdapter.Server.State;
using CSMSNet.OcppAdapter.Server.Transport;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server;

/// <summary>
/// OCPP适配器主类
/// </summary>
public class OcppAdapter : IOcppAdapter
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

    public OcppAdapter(
        OcppAdapterConfiguration configuration,
        ILogger<OcppAdapter>? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
        
        // 初始化核心组件
        _stateCache = new StateCache();
        _protocolHandler = new Ocpp16ProtocolHandler();
        _connectionManager = new ConnectionManager(_configuration);
        _callMatcher = new CallMatcher(_configuration);
        _requestHandler = new RequestHandler(_stateCache, null!, _configuration); // MessageRouter后续设置
        _messageRouter = new MessageRouter(_protocolHandler, _requestHandler, _callMatcher, _connectionManager, _configuration);
        
        // 设置RequestHandler的MessageRouter依赖(通过反射或重新创建)
        _requestHandler = new RequestHandler(_stateCache, _messageRouter, _configuration);
        _messageRouter = new MessageRouter(_protocolHandler, _requestHandler, _callMatcher, _connectionManager, _configuration);
        
        _commandHandler = new CommandHandler(_protocolHandler, _callMatcher, _connectionManager, _configuration);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("OCPP Adapter starting on {ListenUrl}", _configuration.ListenUrl);
        
        // TODO: 启动WebSocket服务器(需要ASP.NET Core集成)
        // WebSocketServer将在后续实现
        
        _logger?.LogInformation(
            "OCPP Adapter started successfully. Protocol: {Version}, Max Connections: {MaxConnections}",
            _protocolHandler.GetSupportedVersion(),
            _configuration.MaxConcurrentConnections);
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("OCPP Adapter stopping");
        
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

    #endregion
}
