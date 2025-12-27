using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Exceptions;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;
using CSMSNet.Ocpp.Transport;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp.Handlers;

/// <summary>
/// OCPP指令发送器
/// </summary>
public class OcppCommandSender
{
    private readonly IOcppProtocolHandler _protocolHandler;
    private readonly ICallMatcher _callMatcher;
    private readonly IConnectionManager _connectionManager;
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<OcppCommandSender>? _logger;

    public OcppCommandSender(
        IOcppProtocolHandler protocolHandler,
        ICallMatcher callMatcher,
        IConnectionManager connectionManager,
        OcppAdapterConfiguration configuration,
        ILogger<OcppCommandSender>? logger = null)
    {
        _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
        _callMatcher = callMatcher ?? throw new ArgumentNullException(nameof(callMatcher));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    /// <summary>
    /// 发送指令并等待响应
    /// </summary>
    public async Task<TResponse> SendCommandAsync<TRequest, TResponse>(
        string chargePointId,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : OcppRequest
        where TResponse : OcppResponse
    {
        // 检查充电桩是否在线
        var session = _connectionManager.GetSession(chargePointId);
        if (session == null)
        {
            throw new OcppConnectionException(
                chargePointId,
                $"Charge point {chargePointId} is not connected");
        }

        // 生成MessageId
        request.MessageId = Guid.NewGuid().ToString();

        _logger?.LogInformation(
            "Sending command {Action} to charge point {ChargePointId}, MessageId: {MessageId}",
            request.Action,
            chargePointId,
            request.MessageId);

        try
        {
            // 序列化为Call消息
            var json = _protocolHandler.SerializeMessage(request);

            // 注册到CallMatcher
            var pendingCall = new PendingCall
            {
                MessageId = request.MessageId,
                ChargePointId = chargePointId,
                Action = request.Action,
                Request = request,
                Timeout = _configuration.CommandResponseTimeout
            };

            // 发送消息
            await session.SendMessageAsync(json);

            // 等待响应(带超时控制)
            var response = await _callMatcher.RegisterCall(pendingCall);

            // 类型转换
            if (response is TResponse typedResponse)
            {
                _logger?.LogInformation(
                    "Received response for command {Action} from {ChargePointId}",
                    request.Action,
                    chargePointId);
                return typedResponse;
            }

            throw new OcppProtocolException($"Unexpected response type: {response.GetType().Name}");
        }
        catch (OcppTimeoutException ex)
        {
            _logger?.LogWarning(ex,
                "Command {Action} timeout for charge point {ChargePointId}",
                request.Action,
                chargePointId);
            throw;
        }
        catch (OcppConnectionException ex)
        {
            _logger?.LogError(ex,
                "Connection error sending command {Action} to {ChargePointId}",
                request.Action,
                chargePointId);
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error sending command {Action} to charge point {ChargePointId}",
                request.Action,
                chargePointId);
            throw new OcppException($"Error sending command {request.Action}", ex);
        }
    }

    public Task<RemoteStartTransactionResponse> RemoteStartTransactionAsync(
        string chargePointId,
        RemoteStartTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<RemoteStartTransactionRequest, RemoteStartTransactionResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<RemoteStopTransactionResponse> RemoteStopTransactionAsync(
        string chargePointId,
        RemoteStopTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<RemoteStopTransactionRequest, RemoteStopTransactionResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ResetResponse> ResetAsync(
        string chargePointId,
        ResetRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ResetRequest, ResetResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<UnlockConnectorResponse> UnlockConnectorAsync(
        string chargePointId,
        UnlockConnectorRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<UnlockConnectorRequest, UnlockConnectorResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<GetConfigurationResponse> GetConfigurationAsync(
        string chargePointId,
        GetConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<GetConfigurationRequest, GetConfigurationResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ChangeConfigurationResponse> ChangeConfigurationAsync(
        string chargePointId,
        ChangeConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ChangeConfigurationRequest, ChangeConfigurationResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ClearCacheResponse> ClearCacheAsync(
        string chargePointId,
        ClearCacheRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ClearCacheRequest, ClearCacheResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<DataTransferResponse> DataTransferAsync(
        string chargePointId,
        DataTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<DataTransferRequest, DataTransferResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ChangeAvailabilityResponse> ChangeAvailabilityAsync(
        string chargePointId,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ChangeAvailabilityRequest, ChangeAvailabilityResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<GetDiagnosticsResponse> GetDiagnosticsAsync(
        string chargePointId,
        GetDiagnosticsRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<GetDiagnosticsRequest, GetDiagnosticsResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<UpdateFirmwareResponse> UpdateFirmwareAsync(
        string chargePointId,
        UpdateFirmwareRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<UpdateFirmwareRequest, UpdateFirmwareResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<GetLocalListVersionResponse> GetLocalListVersionAsync(
        string chargePointId,
        GetLocalListVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<GetLocalListVersionRequest, GetLocalListVersionResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<SendLocalListResponse> SendLocalListAsync(
        string chargePointId,
        SendLocalListRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<SendLocalListRequest, SendLocalListResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<CancelReservationResponse> CancelReservationAsync(
        string chargePointId,
        CancelReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<CancelReservationRequest, CancelReservationResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ReserveNowResponse> ReserveNowAsync(
        string chargePointId,
        ReserveNowRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ReserveNowRequest, ReserveNowResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<ClearChargingProfileResponse> ClearChargingProfileAsync(
        string chargePointId,
        ClearChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<ClearChargingProfileRequest, ClearChargingProfileResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<GetCompositeScheduleResponse> GetCompositeScheduleAsync(
        string chargePointId,
        GetCompositeScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<GetCompositeScheduleRequest, GetCompositeScheduleResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<SetChargingProfileResponse> SetChargingProfileAsync(
        string chargePointId,
        SetChargingProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<SetChargingProfileRequest, SetChargingProfileResponse>(
            chargePointId, request, cancellationToken);
    }

    public Task<TriggerMessageResponse> TriggerMessageAsync(
        string chargePointId,
        TriggerMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendCommandAsync<TriggerMessageRequest, TriggerMessageResponse>(
            chargePointId, request, cancellationToken);
    }
}
