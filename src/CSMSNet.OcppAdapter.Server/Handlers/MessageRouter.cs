using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Exceptions;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Server.Transport;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 消息路由器实现
/// </summary>
public class MessageRouter : IMessageRouter
{
    private readonly IOcppProtocolHandler _protocolHandler;
    private readonly IRequestHandler _requestHandler;
    private readonly ICallMatcher _callMatcher;
    private readonly IConnectionManager _connectionManager;
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<MessageRouter>? _logger;

    public MessageRouter(
        IOcppProtocolHandler protocolHandler,
        IRequestHandler requestHandler,
        ICallMatcher callMatcher,
        IConnectionManager connectionManager,
        OcppAdapterConfiguration configuration,
        ILogger<MessageRouter>? logger = null)
    {
        _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
        _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
        _callMatcher = callMatcher ?? throw new ArgumentNullException(nameof(callMatcher));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    public async Task RouteIncoming(string chargePointId, string json)
    {
        try
        {
            // 验证消息格式
            if (_configuration.EnableJsonSchemaValidation)
            {
                var validation = _protocolHandler.ValidateMessage(json);
                if (!validation.IsValid)
                {
                    _logger?.LogWarning(
                        "Invalid message format from {ChargePointId}: {Errors}",
                        chargePointId,
                        string.Join(", ", validation.Errors));
                    await SendErrorAsync(chargePointId, "", "FormatViolation", "Invalid message format");
                    return;
                }
            }

            // 反序列化消息
            var message = _protocolHandler.DeserializeMessage(json);

            _logger?.LogDebug(
                "Routing message from {ChargePointId}, type: {Type}",
                chargePointId,
                message.GetType().Name);

            // 根据消息类型路由
            switch (message)
            {
                case OcppRequest request:
                    // Call消息 - 充电桩发起的请求
                    var handlerResponse = await _requestHandler.HandleRequestAsync(chargePointId, request);
                    if (handlerResponse != null)
                    {
                        await RouteResponse(chargePointId, handlerResponse);
                    }
                    break;

                case OcppResponse response:
                case OcppError error:
                    // CallResult/CallError - 响应消息
                    _callMatcher.MatchResponse(message.MessageId, message);
                    break;

                default:
                    _logger?.LogWarning(
                        "Unknown message type from {ChargePointId}: {Type}",
                        chargePointId,
                        message.GetType().Name);
                    break;
            }
        }
        catch (OcppProtocolException ex)
        {
            _logger?.LogError(ex,
                "Protocol error routing message from {ChargePointId}",
                chargePointId);
            await SendErrorAsync(chargePointId, "", "ProtocolError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error routing message from {ChargePointId}",
                chargePointId);
            await SendErrorAsync(chargePointId, "", "InternalError", "Internal error processing message");
        }
    }

    public async Task RouteResponse(string chargePointId, IOcppMessage message)
    {
        var session = _connectionManager.GetSession(chargePointId);
        if (session == null)
        {
            _logger?.LogWarning(
                "Cannot route response, charge point {ChargePointId} not connected",
                chargePointId);
            return;
        }

        try
        {
            var json = _protocolHandler.SerializeMessage(message);
            await session.SendMessageAsync(json);

            _logger?.LogDebug(
                "Routed response to {ChargePointId}, MessageId: {MessageId}",
                chargePointId,
                message.MessageId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error routing response to {ChargePointId}",
                chargePointId);
            throw;
        }
    }

    private async Task SendErrorAsync(
        string chargePointId,
        string messageId,
        string errorCode,
        string errorDescription)
    {
        try
        {
            var error = new OcppError
            {
                MessageId = messageId,
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };

            await RouteResponse(chargePointId, error);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error sending error response to {ChargePointId}",
                chargePointId);
        }
    }
}
