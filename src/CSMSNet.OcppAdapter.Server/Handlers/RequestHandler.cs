using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Enums;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 请求处理器实现(基础版本)
/// </summary>
public class RequestHandler : IRequestHandler
{
    private readonly IStateCache _stateCache;
    private readonly IMessageRouter _messageRouter;
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<RequestHandler>? _logger;

    public RequestHandler(
        IStateCache stateCache,
        IMessageRouter messageRouter,
        OcppAdapterConfiguration configuration,
        ILogger<RequestHandler>? logger = null)
    {
        _stateCache = stateCache ?? throw new ArgumentNullException(nameof(stateCache));
        _messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
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
            FirmwareVersion = request.FirmwareVersion
        };
        _stateCache.UpdateChargePointInfo(info);
        _stateCache.MarkOnline(chargePointId);

        _logger?.LogInformation(
            "BootNotification from {ChargePointId}, Model: {Model}, Vendor: {Vendor}",
            chargePointId,
            request.ChargePointModel,
            request.ChargePointVendor);

        // TODO: 在阶段三添加事件触发机制
        // 当前返回默认响应
        return await Task.FromResult(new BootNotificationResponse
        {
            Status = RegistrationStatus.Accepted,
            CurrentTime = DateTime.UtcNow,
            Interval = (int)_configuration.HeartbeatInterval.TotalSeconds
        });
    }

    private async Task<HeartbeatResponse> HandleHeartbeatAsync(
        string chargePointId,
        HeartbeatRequest request)
    {
        _stateCache.UpdateLastCommunication(chargePointId);

        return await Task.FromResult(new HeartbeatResponse
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
            Timestamp = request.Timestamp ?? DateTime.UtcNow
        };
        _stateCache.UpdateConnectorStatus(chargePointId, status);

        _logger?.LogDebug(
            "StatusNotification from {ChargePointId}, Connector: {ConnectorId}, Status: {Status}",
            chargePointId,
            request.ConnectorId,
            request.Status);

        return await Task.FromResult(new StatusNotificationResponse());
    }

    private async Task<AuthorizeResponse> HandleAuthorizeAsync(
        string chargePointId,
        AuthorizeRequest request)
    {
        // TODO: 在阶段三添加授权事件处理
        // 当前返回默认接受
        return await Task.FromResult(new AuthorizeResponse
        {
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        });
    }

    private async Task<StartTransactionResponse> HandleStartTransactionAsync(
        string chargePointId,
        StartTransactionRequest request)
    {
        // 生成事务ID
        var transactionId = new Random().Next(1, 1000000);

        // 创建事务
        var transaction = new CSMSNet.OcppAdapter.Models.State.Transaction
        {
            ChargePointId = chargePointId,
            TransactionId = transactionId,
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
            transactionId);

        return await Task.FromResult(new StartTransactionResponse
        {
            TransactionId = transactionId,
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        });
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

        return await Task.FromResult(new StopTransactionResponse
        {
            IdTagInfo = new CSMSNet.OcppAdapter.Models.V16.Common.IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        });
    }

    private async Task<MeterValuesResponse> HandleMeterValuesAsync(
        string chargePointId,
        MeterValuesRequest request)
    {
        // TODO: 更新电表数据
        _stateCache.UpdateLastCommunication(chargePointId);

        return await Task.FromResult(new MeterValuesResponse());
    }

    private async Task<DataTransferResponse> HandleDataTransferAsync(
        string chargePointId,
        DataTransferRequest request)
    {
        // TODO: 在阶段三添加DataTransfer事件处理
        return await Task.FromResult(new DataTransferResponse
        {
            Status = DataTransferStatus.Rejected
        });
    }
}
