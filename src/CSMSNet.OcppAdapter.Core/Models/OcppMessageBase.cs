namespace CSMSNet.OcppAdapter.Models;

/// <summary>
/// OCPP消息类型
/// </summary>
public enum OcppMessageType
{
    /// <summary>
    /// Call - 请求消息
    /// </summary>
    Call = 2,

    /// <summary>
    /// CallResult - 成功响应消息
    /// </summary>
    CallResult = 3,

    /// <summary>
    /// CallError - 错误响应消息
    /// </summary>
    CallError = 4
}

/// <summary>
/// OCPP消息基接口
/// </summary>
public interface IOcppMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    OcppMessageType MessageType { get; }

    /// <summary>
    /// 消息唯一标识符
    /// </summary>
    string MessageId { get; set; }
}

/// <summary>
/// OCPP请求消息基类
/// </summary>
public abstract class OcppRequest : IOcppMessage
{
    public OcppMessageType MessageType => OcppMessageType.Call;
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 消息动作名称
    /// </summary>
    public abstract string Action { get; }
}

/// <summary>
/// OCPP响应消息基类
/// </summary>
public abstract class OcppResponse : IOcppMessage
{
    public OcppMessageType MessageType => OcppMessageType.CallResult;
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 是否超时
    /// </summary>
    public bool IsTimeout { get; set; }
}

/// <summary>
/// OCPP错误响应
/// </summary>
public class OcppError : IOcppMessage
{
    public OcppMessageType MessageType => OcppMessageType.CallError;
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 错误代码
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// 错误描述
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// 错误详情
    /// </summary>
    public object? ErrorDetails { get; set; }
}

/// <summary>
/// OCPP错误代码常量
/// </summary>
public static class OcppErrorCode
{
    /// <summary>
    /// 请求的操作不受支持或未实现
    /// </summary>
    public const string NotImplemented = "NotImplemented";

    /// <summary>
    /// 请求的操作不受支持或无法执行
    /// </summary>
    public const string NotSupported = "NotSupported";

    /// <summary>
    /// 内部错误
    /// </summary>
    public const string InternalError = "InternalError";

    /// <summary>
    /// 协议错误
    /// </summary>
    public const string ProtocolError = "ProtocolError";

    /// <summary>
    /// 安全错误
    /// </summary>
    public const string SecurityError = "SecurityError";

    /// <summary>
    /// 格式违规
    /// </summary>
    public const string FormationViolation = "FormationViolation";

    /// <summary>
    /// 属性约束违规
    /// </summary>
    public const string PropertyConstraintViolation = "PropertyConstraintViolation";

    /// <summary>
    /// 出现次数约束违规
    /// </summary>
    public const string OccurrenceConstraintViolation = "OccurrenceConstraintViolation";

    /// <summary>
    /// 类型约束违规
    /// </summary>
    public const string TypeConstraintViolation = "TypeConstraintViolation";

    /// <summary>
    /// 通用错误
    /// </summary>
    public const string GenericError = "GenericError";
}
