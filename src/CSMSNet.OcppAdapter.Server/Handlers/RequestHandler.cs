using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.V16.Enums;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.OcppAdapter.Server.Transport; // Added for IConnectionManager
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 请求处理器实现
/// </summary>
public class RequestHandler : IRequestHandler
{
    private readonly IStateCache _stateCache;
    private readonly IMessageRouter _messageRouter;
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<RequestHandler>? _logger;
    private readonly IConnectionManager? _connectionManager; // Added dependency

    public event EventHandler<BootNotificationEventArgs>? OnBootNotification;
    public event EventHandler<HeartbeatEventArgs>? OnHeartbeat;
    public event EventHandler<StatusNotificationEventArgs>? OnStatusNotification;
    public event EventHandler<AuthorizeEventArgs>? OnAuthorize;
    public event EventHandler<StartTransactionEventArgs>? OnStartTransaction;
    public event EventHandler<StopTransactionEventArgs>? OnStopTransaction;
    public event EventHandler<MeterValuesEventArgs>? OnMeterValues;
    public event EventHandler<DataTransferEventArgs>? OnDataTransfer;
    public event EventHandler<DiagnosticsStatusNotificationEventArgs>? OnDiagnosticsStatusNotification;
    public event EventHandler<FirmwareStatusNotificationEventArgs>? OnFirmwareStatusNotification;

    public RequestHandler(
        IStateCache stateCache,
        IMessageRouter messageRouter,
        OcppAdapterConfiguration configuration,
        ILogger<RequestHandler>? logger = null,
        IConnectionManager? connectionManager = null) // Optional to avoid breaking tests/existing code immediately, but should be required
    {
        _stateCache = stateCache ?? throw new ArgumentNullException(nameof(stateCache));
        _messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task HandleRequestAsync(string chargePointId, OcppRequest request)
    {
        _logger?.LogDebug(
            "Handling request {Action} from charge point {ChargePointId}, MessageId: {MessageId}",
            request.Action,
            chargePointId,
            request.MessageId);

        try
        {
            IOcppMessage response = request switch
            {
                BootNotificationRequest bootReq => await HandleBootNotificationAsync(chargePointId, bootReq),
                HeartbeatRequest heartbeatReq => await HandleHeartbeatAsync(chargePointId, heartbeatReq),
                StatusNotificationRequest statusReq => await HandleStatusNotificationAsync(chargePointId, statusReq),
                AuthorizeRequest authReq => await HandleAuthorizeAsync(chargePointId, authReq),
                StartTransactionRequest startReq => await HandleStartTransactionAsync(chargePointId, startReq),
                StopTransactionRequest stopReq => await HandleStopTransactionAsync(chargePointId, stopReq),
                MeterValuesRequest meterReq => await HandleMeterValuesAsync(chargePointId, meterReq),
                DataTransferRequest dataReq => await HandleDataTransferAsync(chargePointId, dataReq),
                DiagnosticsStatusNotificationRequest diagReq => await HandleDiagnosticsStatusNotificationAsync(chargePointId, diagReq),
                FirmwareStatusNotificationRequest firmReq => await HandleFirmwareStatusNotificationAsync(chargePointId, firmReq),
                _ => throw new NotSupportedException($"Unsupported action: {request.Action}")
            };

            response.MessageId = request.MessageId;
            await _messageRouter.RouteResponse(chargePointId, response);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error handling request {Action} from charge point {ChargePointId}",
                request.Action,
                chargePointId);

            var error = new OcppError
            {
                MessageId = request.MessageId,
                ErrorCode = "InternalError",
                ErrorDescription = "Error processing request"
            };
            await _messageRouter.RouteResponse(chargePointId, error);
        }
    }

    private async Task<BootNotificationResponse> HandleBootNotificationAsync(
        string chargePointId,
        BootNotificationRequest request)
    {
        // 更新状态缓存
        var info = new CSMSNet.OcppAdapter.Models.State.ChargePointInfo
        {
            ChargePointId = chargePointId,
            Vendor = request.ChargePointVendor,
            Model = request.ChargePointModel,
            SerialNumber = request.ChargePointSerialNumber,
            FirmwareVersion = request.FirmwareVersion,
            Status = RegistrationStatus.Accepted // 默认为Accepted, 除非业务层修改
        };
        _stateCache.UpdateChargePointInfo(info);
        _stateCache.MarkOnline(chargePointId);

        _logger?.LogInformation(
            "BootNotification from {ChargePointId}, Model: {Model}, Vendor: {Vendor}",
            chargePointId,
            request.ChargePointModel,
            request.ChargePointVendor);

        var args = new BootNotificationEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnBootNotification != null)
        {
            OnBootNotification.Invoke(this, args);
            var response = await WaitForResponseAsync(args.ResponseTask, GetDefaultBootNotificationResponse());
            
            // 更新注册状态
            if (response.Status != RegistrationStatus.Accepted)
            {
                info.Status = response.Status;
                _stateCache.UpdateChargePointInfo(info);
            }
            else
            {
                // 如果接受，标记会话为已验证
                if (_connectionManager != null)
                {
                    var session = _connectionManager.GetSession(chargePointId);
                    session?.MarkVerified();
                }
            }
            
            return response;
        }

        // 默认接受时也标记验证
        if (_connectionManager != null)
        {
            var session = _connectionManager.GetSession(chargePointId);
            session?.MarkVerified();
        }

        return GetDefaultBootNotificationResponse();
    }

    private BootNotificationResponse GetDefaultBootNotificationResponse()
    {
        return new BootNotificationResponse
        {
            Status = RegistrationStatus.Accepted,
            CurrentTime = DateTime.UtcNow,
            Interval = (int)_configuration.HeartbeatInterval.TotalSeconds
        };
    }

    private async Task<HeartbeatResponse> HandleHeartbeatAsync(
        string chargePointId,
        HeartbeatRequest request)
    {
        _stateCache.UpdateLastCommunication(chargePointId);

        var args = new HeartbeatEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnHeartbeat != null)
        {
            OnHeartbeat.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new HeartbeatResponse { CurrentTime = DateTime.UtcNow });
        }

        return new HeartbeatResponse
        {
            CurrentTime = DateTime.UtcNow
        };
    }

    private async Task<StatusNotificationResponse> HandleStatusNotificationAsync(
        string chargePointId,
        StatusNotificationRequest request)
    {
        // 更新连接器状态
        var status = new CSMSNet.OcppAdapter.Models.State.ConnectorStatus
        {
            ConnectorId = request.ConnectorId,
            Status = request.Status.ToString(),
            ErrorCode = request.ErrorCode.ToString(),
            Timestamp = request.Timestamp ?? DateTime.UtcNow
        };
        _stateCache.UpdateConnectorStatus(chargePointId, status);

        _logger?.LogDebug(
            "StatusNotification from {ChargePointId}, Connector: {ConnectorId}, Status: {Status}",
            chargePointId,
            request.ConnectorId,
            request.Status);

        var args = new StatusNotificationEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnStatusNotification != null)
        {
            OnStatusNotification.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new StatusNotificationResponse());
        }

        return new StatusNotificationResponse();
    }

    private async Task<AuthorizeResponse> HandleAuthorizeAsync(
        string chargePointId,
        AuthorizeRequest request)
    {
        var args = new AuthorizeEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnAuthorize != null)
        {
            OnAuthorize.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new AuthorizeResponse
            {
                IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo { Status = AuthorizationStatus.Accepted }
            });
        }

        return new AuthorizeResponse
        {
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };
    }

    private async Task<StartTransactionResponse> HandleStartTransactionAsync(
        string chargePointId,
        StartTransactionRequest request)
    {
        // 生成临时事务ID作为默认值
        var transactionId = new Random().Next(1, 1000000);

        var args = new StartTransactionEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnStartTransaction != null)
        {
            OnStartTransaction.Invoke(this, args);
            var response = await WaitForResponseAsync(args.ResponseTask, new StartTransactionResponse
            {
                TransactionId = transactionId,
                IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo { Status = AuthorizationStatus.Accepted }
            });

            // 如果业务层接受了事务，更新本地缓存
            if (response.IdTagInfo.Status == AuthorizationStatus.Accepted)
            {
                var transaction = new CSMSNet.OcppAdapter.Models.State.Transaction
                {
                    ChargePointId = chargePointId,
                    TransactionId = response.TransactionId,
                    ConnectorId = request.ConnectorId,
                    IdTag = request.IdTag,
                    MeterStart = request.MeterStart,
                    StartTime = request.Timestamp
                };
                _stateCache.CreateTransaction(transaction);
                
                _logger?.LogInformation(
                    "Transaction started on {ChargePointId}, Connector: {ConnectorId}, TransactionId: {TransactionId}",
                    chargePointId,
                    request.ConnectorId,
                    response.TransactionId);
            }
            
            return response;
        }

        // 默认行为：接受并创建事务
        var defaultTransaction = new CSMSNet.OcppAdapter.Models.State.Transaction
        {
            ChargePointId = chargePointId,
            TransactionId = transactionId,
            ConnectorId = request.ConnectorId,
            IdTag = request.IdTag,
            MeterStart = request.MeterStart,
            StartTime = request.Timestamp
        };
        _stateCache.CreateTransaction(defaultTransaction);

        _logger?.LogInformation(
            "Transaction started on {ChargePointId}, Connector: {ConnectorId}, TransactionId: {TransactionId} (Default)",
            chargePointId,
            request.ConnectorId,
            transactionId);

        return new StartTransactionResponse
        {
            TransactionId = transactionId,
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };
    }

    private async Task<StopTransactionResponse> HandleStopTransactionAsync(
        string chargePointId,
        StopTransactionRequest request)
    {
        _stateCache.EndTransaction(chargePointId, request.TransactionId);

        _logger?.LogInformation(
            "Transaction stopped on {ChargePointId}, TransactionId: {TransactionId}",
            chargePointId,
            request.TransactionId);

        var args = new StopTransactionEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnStopTransaction != null)
        {
            OnStopTransaction.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new StopTransactionResponse
            {
                IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo { Status = AuthorizationStatus.Accepted }
            });
        }

        return new StopTransactionResponse
        {
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };
    }

    private async Task<MeterValuesResponse> HandleMeterValuesAsync(
        string chargePointId,
        MeterValuesRequest request)
    {
        _stateCache.UpdateLastCommunication(chargePointId);

        var args = new MeterValuesEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnMeterValues != null)
        {
            OnMeterValues.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new MeterValuesResponse());
        }

        return new MeterValuesResponse();
    }

    private async Task<DataTransferResponse> HandleDataTransferAsync(
        string chargePointId,
        DataTransferRequest request)
    {
        var args = new DataTransferEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnDataTransfer != null)
        {
            OnDataTransfer.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new DataTransferResponse { Status = DataTransferStatus.Rejected });
        }

        return new DataTransferResponse
        {
            Status = DataTransferStatus.Rejected
        };
    }

    private async Task<DiagnosticsStatusNotificationResponse> HandleDiagnosticsStatusNotificationAsync(
        string chargePointId,
        DiagnosticsStatusNotificationRequest request)
    {
        var args = new DiagnosticsStatusNotificationEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnDiagnosticsStatusNotification != null)
        {
            OnDiagnosticsStatusNotification.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new DiagnosticsStatusNotificationResponse());
        }

        return new DiagnosticsStatusNotificationResponse();
    }

    private async Task<FirmwareStatusNotificationResponse> HandleFirmwareStatusNotificationAsync(
        string chargePointId,
        FirmwareStatusNotificationRequest request)
    {
        var args = new FirmwareStatusNotificationEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        if (OnFirmwareStatusNotification != null)
        {
            OnFirmwareStatusNotification.Invoke(this, args);
            return await WaitForResponseAsync(args.ResponseTask, new FirmwareStatusNotificationResponse());
        }

        return new FirmwareStatusNotificationResponse();
    }

    private async Task<T> WaitForResponseAsync<T>(TaskCompletionSource<T> tcs, T defaultResponse)
    {
        // 默认等待30秒，或者使用配置
        var timeout = TimeSpan.FromSeconds(30); 
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }

        _logger?.LogWarning("Timeout waiting for event response");
        return defaultResponse;
    }
}
