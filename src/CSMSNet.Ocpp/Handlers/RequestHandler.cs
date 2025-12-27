using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.V16.Enums;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.Ocpp.Transport;
using Microsoft.Extensions.Logging;
using CSMSNet.OcppAdapter.Models.State; // For BatteryStatus

namespace CSMSNet.Ocpp.Handlers;

/// <summary>
/// 请求处理器实现
/// </summary>
public class RequestHandler : IRequestHandler
{
    private readonly IStateCache _stateCache;
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<RequestHandler>? _logger;
    private readonly IConnectionManager? _connectionManager;

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
        OcppAdapterConfiguration configuration,
        ILogger<RequestHandler>? logger = null,
        IConnectionManager? connectionManager = null)
    {
        _stateCache = stateCache ?? throw new ArgumentNullException(nameof(stateCache));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task<IOcppMessage> HandleRequestAsync(string chargePointId, OcppRequest request)
    {
        _logger?.LogDebug(
            "Handling request {Action} from charge point {ChargePointId}, MessageId: {MessageId}",
            request.Action,
            chargePointId,
            request.MessageId);

        try
        {
            // 验证会话状态
            if (_connectionManager != null)
            {
                var session = _connectionManager.GetSession(chargePointId);
                // 如果会话存在但未验证，且请求不是BootNotification，则拒绝
                if (session != null && !session.IsVerified && request is not BootNotificationRequest)
                {
                    _logger?.LogWarning("Rejected request {Action} from unverified charge point {ChargePointId}", request.Action, chargePointId);
                    return new OcppError
                    {
                        MessageId = request.MessageId,
                        ErrorCode = "SecurityError",
                        ErrorDescription = "BootNotification required"
                    };
                }
            }

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
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error handling request {Action} from charge point {ChargePointId}",
                request.Action,
                chargePointId);

            return new OcppError
            {
                MessageId = request.MessageId,
                ErrorCode = "InternalError",
                ErrorDescription = "Error processing request"
            };
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

                // 如果被拒绝，断开连接
                if (_connectionManager != null)
                {
                    var session = _connectionManager.GetSession(chargePointId);
                    if (session != null)
                    {
                        _logger?.LogWarning("BootNotification rejected for {ChargePointId}. Closing connection.", chargePointId);
                        // 异步断开连接，给予少量时间发送响应
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(500); // 等待响应发送完成
                                await session.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation, "BootNotification Rejected");
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Error closing rejected session for {ChargePointId}", chargePointId);
                            }
                        });
                    }
                }
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

    private Task<HeartbeatResponse> HandleHeartbeatAsync(
        string chargePointId,
        HeartbeatRequest request)
    {
        _stateCache.UpdateLastCommunication(chargePointId);

        var args = new HeartbeatEventArgs
        {
            ChargePointId = chargePointId,
            Request = request
        };

        // 触发事件通知（不等待结果）
        OnHeartbeat?.Invoke(this, args);

        // 立即返回
        return Task.FromResult(new HeartbeatResponse
        {
            CurrentTime = DateTime.UtcNow
        });
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
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            Info = request.Info,
            VendorId = request.VendorId,
            VendorErrorCode = request.VendorErrorCode
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
                    ConnectorIds = new List<int> { request.ConnectorId },
                    IdTag = request.IdTag,
                    MeterStartValues = new Dictionary<int, int> { { request.ConnectorId, request.MeterStart } },
                    StartTime = request.Timestamp,
                    LastUpdated = DateTime.UtcNow
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
            ConnectorIds = new List<int> { request.ConnectorId },
            IdTag = request.IdTag,
            MeterStartValues = new Dictionary<int, int> { { request.ConnectorId, request.MeterStart } },
            StartTime = request.Timestamp,
            LastUpdated = DateTime.UtcNow
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

        // 解析电表值并更新缓存
        try
        {
            BatteryStatus? batteryStatus = null;
            decimal? instantVoltage = null;
            decimal? instantCurrent = null;
            decimal? instantPower = null;
            int? energyActiveImportRegister = null;

            foreach (var meterValue in request.MeterValue)
            {
                foreach (var sampledValue in meterValue.SampledValue)
                {
                    if (decimal.TryParse(sampledValue.Value, out var val))
                    {
                        var measurand = sampledValue.Measurand ?? Measurand.EnergyActiveImportRegister;
                        var location = sampledValue.Location ?? Location.Outlet;
                        var context = sampledValue.Context ?? ReadingContext.SamplePeriodic;

                        switch (measurand)
                        {
                            case Measurand.SoC:
                                if (batteryStatus == null) batteryStatus = new BatteryStatus();
                                batteryStatus.SoC = val;
                                break;
                            case Measurand.Voltage:
                                if (location == Location.EV)
                                {
                                    if (batteryStatus == null) batteryStatus = new BatteryStatus();
                                    batteryStatus.Voltage = val;
                                }
                                else
                                {
                                    instantVoltage = val;
                                }
                                break;
                            case Measurand.CurrentImport:
                            case Measurand.CurrentOffered:
                                if (location == Location.EV)
                                {
                                    if (batteryStatus == null) batteryStatus = new BatteryStatus();
                                    batteryStatus.Current = val;
                                }
                                else
                                {
                                    instantCurrent = val;
                                }
                                break;
                            case Measurand.PowerActiveImport:
                                if (location == Location.EV)
                                {
                                    if (batteryStatus == null) batteryStatus = new BatteryStatus();
                                    batteryStatus.Power = val;
                                }
                                else
                                {
                                    instantPower = val;
                                }
                                break;
                            case Measurand.Temperature:
                                if (batteryStatus == null) batteryStatus = new BatteryStatus();
                                batteryStatus.Temperature = val;
                                break;
                            case Measurand.EnergyActiveImportRegister:
                                energyActiveImportRegister = (int)val;
                                
                                // 如果是电表读数，我们需要传递上下文给状态缓存
                                if (request.TransactionId.HasValue)
                                {
                                    _stateCache.UpdateTransactionMeter(
                                        chargePointId, 
                                        request.ConnectorId, 
                                        energyActiveImportRegister.Value,
                                        context);
                                }
                                break;
                        }
                    }
                }
            }

            // 更新电池状态
            if (batteryStatus != null)
            {
                _stateCache.UpdateBatteryStatus(chargePointId, request.ConnectorId, batteryStatus);
            }

            // 更新连接器快照
            _stateCache.UpdateConnectorSnapshot(chargePointId, request.ConnectorId, instantVoltage, instantCurrent, instantPower);

            // 更新事务快照 (只更新功率)
            if (request.TransactionId.HasValue)
            {
                _stateCache.UpdateTransactionSnapshot(
                    chargePointId, 
                    request.ConnectorId, 
                    instantPower); // Removed SoC, Voltage, Current

                // UpdateTransactionMeter 已经在循环内部调用了，这里不需要再次调用
                // 但为了兼容旧逻辑或者防止循环内没有触发更新（比如没有TransactionId但有meter value? 不可能，有check）
                // 上面的循环是针对每个SampledValue处理的。如果一个 MeterValue 里有多个 SampledValue，
                // 其中包含 EnergyActiveImportRegister，那么上面的逻辑会多次调用 UpdateTransactionMeter。
                // 这是正确的，因为我们需要捕捉每一次读数及其上下文。
                // 所以这里移除原来在这里的 UpdateTransactionMeter 调用。
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing MeterValues from {ChargePointId}", chargePointId);
        }

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
        // 更新诊断状态
        _stateCache.UpdateDiagnosticsStatus(chargePointId, request.Status); // Pass Enum directly

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
        // 更新固件状态
        _stateCache.UpdateFirmwareStatus(chargePointId, request.Status); // Pass Enum directly

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
