using System.Text.Json;
using System.Text.Json.Nodes;
using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Models;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using CSMSNet.OcppAdapter.Models.V16.Responses;

namespace CSMSNet.Ocpp.V16;

/// <summary>
/// OCPP 1.6协议处理器
/// </summary>
public class Ocpp16ProtocolHandler : IOcppProtocolHandler
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<string, Type> _requestTypes;
    private readonly Dictionary<string, Type> _responseTypes;

    public Ocpp16ProtocolHandler()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // 初始化请求类型映射
        _requestTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorize"] = typeof(AuthorizeRequest),
            ["BootNotification"] = typeof(BootNotificationRequest),
            ["Heartbeat"] = typeof(HeartbeatRequest),
            ["StatusNotification"] = typeof(StatusNotificationRequest),
            ["MeterValues"] = typeof(MeterValuesRequest),
            ["StartTransaction"] = typeof(StartTransactionRequest),
            ["StopTransaction"] = typeof(StopTransactionRequest),
            ["DataTransfer"] = typeof(DataTransferRequest),
            ["DiagnosticsStatusNotification"] = typeof(DiagnosticsStatusNotificationRequest),
            ["FirmwareStatusNotification"] = typeof(FirmwareStatusNotificationRequest),
            // 系统下发指令
            ["RemoteStartTransaction"] = typeof(RemoteStartTransactionRequest),
            ["RemoteStopTransaction"] = typeof(RemoteStopTransactionRequest),
            ["Reset"] = typeof(ResetRequest),
            ["UnlockConnector"] = typeof(UnlockConnectorRequest),
            ["GetConfiguration"] = typeof(GetConfigurationRequest),
            ["ChangeConfiguration"] = typeof(ChangeConfigurationRequest),
            ["ClearCache"] = typeof(ClearCacheRequest),
            ["ChangeAvailability"] = typeof(ChangeAvailabilityRequest),
            ["GetDiagnostics"] = typeof(GetDiagnosticsRequest),
            ["UpdateFirmware"] = typeof(UpdateFirmwareRequest),
            ["GetLocalListVersion"] = typeof(GetLocalListVersionRequest),
            ["SendLocalList"] = typeof(SendLocalListRequest),
            ["CancelReservation"] = typeof(CancelReservationRequest),
            ["ReserveNow"] = typeof(ReserveNowRequest),
            ["ClearChargingProfile"] = typeof(ClearChargingProfileRequest),
            ["GetCompositeSchedule"] = typeof(GetCompositeScheduleRequest),
            ["SetChargingProfile"] = typeof(SetChargingProfileRequest),
            ["TriggerMessage"] = typeof(TriggerMessageRequest),
        };

        // 初始化响应类型映射
        _responseTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorize"] = typeof(AuthorizeResponse),
            ["BootNotification"] = typeof(BootNotificationResponse),
            ["Heartbeat"] = typeof(HeartbeatResponse),
            ["StatusNotification"] = typeof(StatusNotificationResponse),
            ["MeterValues"] = typeof(MeterValuesResponse),
            ["StartTransaction"] = typeof(StartTransactionResponse),
            ["StopTransaction"] = typeof(StopTransactionResponse),
            ["DataTransfer"] = typeof(DataTransferResponse),
            ["DiagnosticsStatusNotification"] = typeof(DiagnosticsStatusNotificationResponse),
            ["FirmwareStatusNotification"] = typeof(FirmwareStatusNotificationResponse),
            // 系统下发指令响应
            ["RemoteStartTransaction"] = typeof(RemoteStartTransactionResponse),
            ["RemoteStopTransaction"] = typeof(RemoteStopTransactionResponse),
            ["Reset"] = typeof(ResetResponse),
            ["UnlockConnector"] = typeof(UnlockConnectorResponse),
            ["GetConfiguration"] = typeof(GetConfigurationResponse),
            ["ChangeConfiguration"] = typeof(ChangeConfigurationResponse),
            ["ClearCache"] = typeof(ClearCacheResponse),
            ["ChangeAvailability"] = typeof(ChangeAvailabilityResponse),
            ["GetDiagnostics"] = typeof(GetDiagnosticsResponse),
            ["UpdateFirmware"] = typeof(UpdateFirmwareResponse),
            ["GetLocalListVersion"] = typeof(GetLocalListVersionResponse),
            ["SendLocalList"] = typeof(SendLocalListResponse),
            ["CancelReservation"] = typeof(CancelReservationResponse),
            ["ReserveNow"] = typeof(ReserveNowResponse),
            ["ClearChargingProfile"] = typeof(ClearChargingProfileResponse),
            ["GetCompositeSchedule"] = typeof(GetCompositeScheduleResponse),
            ["SetChargingProfile"] = typeof(SetChargingProfileResponse),
            ["TriggerMessage"] = typeof(TriggerMessageResponse),
        };
    }

    public string GetSupportedVersion()
    {
        return Ocpp16Constants.Version;
    }

    public IOcppMessage DeserializeMessage(string json)
    {
        var array = JsonSerializer.Deserialize<JsonArray>(json, _jsonOptions);
        if (array == null || array.Count < 3)
        {
            throw new InvalidOperationException("Invalid OCPP message format");
        }

        var messageTypeId = array[Ocpp16Constants.CallIndex.MessageTypeId]?.GetValue<int>() 
            ?? throw new InvalidOperationException("Missing message type");

        return messageTypeId switch
        {
            (int)OcppMessageType.Call => DeserializeCall(array),
            (int)OcppMessageType.CallResult => DeserializeCallResult(array),
            (int)OcppMessageType.CallError => DeserializeCallError(array),
            _ => throw new InvalidOperationException($"Unknown message type: {messageTypeId}")
        };
    }

    private IOcppMessage DeserializeCall(JsonArray array)
    {
        if (array.Count != Ocpp16Constants.CallIndex.Length)
        {
            throw new InvalidOperationException("Invalid Call message format");
        }

        var messageId = array[Ocpp16Constants.CallIndex.MessageId]?.GetValue<string>() 
            ?? throw new InvalidOperationException("Missing message ID");
        var action = array[Ocpp16Constants.CallIndex.Action]?.GetValue<string>() 
            ?? throw new InvalidOperationException("Missing action");
        var payloadNode = array[Ocpp16Constants.CallIndex.Payload];

        if (!_requestTypes.TryGetValue(action, out var requestType))
        {
            throw new InvalidOperationException($"Unknown action: {action}");
        }

        var payload = payloadNode?.Deserialize(requestType, _jsonOptions) as OcppRequest
            ?? throw new InvalidOperationException("Failed to deserialize payload");

        payload.MessageId = messageId;
        return payload;
    }

    private IOcppMessage DeserializeCallResult(JsonArray array)
    {
        if (array.Count != Ocpp16Constants.CallResultIndex.Length)
        {
            throw new InvalidOperationException("Invalid CallResult message format");
        }

        var messageId = array[Ocpp16Constants.CallResultIndex.MessageId]?.GetValue<string>() 
            ?? throw new InvalidOperationException("Missing message ID");
        var payloadNode = array[Ocpp16Constants.CallResultIndex.Payload];

        // 这里需要通过上下文知道对应的请求Action来确定响应类型
        // 实际使用时会由CallMatcher提供这个信息
        // 暂时返回一个通用响应对象
        var response = new GenericOcppResponse();
        response.MessageId = messageId;
        response.RawPayload = payloadNode?.ToJsonString() ?? "{}";
        return response;
    }

    private IOcppMessage DeserializeCallError(JsonArray array)
    {
        if (array.Count != Ocpp16Constants.CallErrorIndex.Length)
        {
            throw new InvalidOperationException("Invalid CallError message format");
        }

        var messageId = array[Ocpp16Constants.CallErrorIndex.MessageId]?.GetValue<string>() 
            ?? throw new InvalidOperationException("Missing message ID");
        var errorCode = array[Ocpp16Constants.CallErrorIndex.ErrorCode]?.GetValue<string>() 
            ?? throw new InvalidOperationException("Missing error code");
        var errorDescription = array[Ocpp16Constants.CallErrorIndex.ErrorDescription]?.GetValue<string>() ?? "";
        var errorDetails = array[Ocpp16Constants.CallErrorIndex.ErrorDetails];

        return new OcppError
        {
            MessageId = messageId,
            ErrorCode = errorCode,
            ErrorDescription = errorDescription,
            ErrorDetails = errorDetails?.Deserialize<object>(_jsonOptions)
        };
    }

    public string SerializeMessage(IOcppMessage message)
    {
        return message switch
        {
            OcppRequest request => SerializeCall(request),
            OcppResponse response => SerializeCallResult(response),
            OcppError error => SerializeCallError(error),
            _ => throw new InvalidOperationException($"Unknown message type: {message.GetType()}")
        };
    }

    private string SerializeCall(OcppRequest request)
    {
        var array = new object[]
        {
            (int)OcppMessageType.Call,
            request.MessageId,
            request.Action,
            request
        };

        return JsonSerializer.Serialize(array, _jsonOptions);
    }

    private string SerializeCallResult(OcppResponse response)
    {
        var array = new object[]
        {
            (int)OcppMessageType.CallResult,
            response.MessageId,
            response
        };

        return JsonSerializer.Serialize(array, _jsonOptions);
    }

    private string SerializeCallError(OcppError error)
    {
        var array = new object[]
        {
            (int)OcppMessageType.CallError,
            error.MessageId,
            error.ErrorCode,
            error.ErrorDescription,
            error.ErrorDetails ?? new { }
        };

        return JsonSerializer.Serialize(array, _jsonOptions);
    }

    public ValidationResult ValidateMessage(string json)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            var array = JsonSerializer.Deserialize<JsonArray>(json, _jsonOptions);
            if (array == null || array.Count < 3)
            {
                result.IsValid = false;
                result.Errors.Add("Invalid message format: array is null or too short");
                return result;
            }

            var messageTypeId = array[0]?.GetValue<int>();
            if (messageTypeId == null || (messageTypeId < 2 || messageTypeId > 4))
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid message type ID: {messageTypeId}");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"JSON parsing error: {ex.Message}");
        }

        return result;
    }

    public Type? GetRequestType(string action)
    {
        _requestTypes.TryGetValue(action, out var type);
        return type;
    }

    public Type? GetResponseType(string action)
    {
        _responseTypes.TryGetValue(action, out var type);
        return type;
    }
}

/// <summary>
/// 通用OCPP响应(用于反序列化时不知道具体类型的场景)
/// </summary>
internal class GenericOcppResponse : OcppResponse
{
    public string RawPayload { get; set; } = string.Empty;
}
