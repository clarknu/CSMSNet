using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Abstractions;

/// <summary>
/// 消息验证结果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 是否验证成功
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// OCPP协议处理器接口
/// </summary>
public interface IOcppProtocolHandler
{
    /// <summary>
    /// 获取支持的协议版本号
    /// </summary>
    /// <returns>协议版本号（如"1.6"）</returns>
    string GetSupportedVersion();

    /// <summary>
    /// 反序列化JSON为强类型对象
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>OCPP消息对象</returns>
    IOcppMessage DeserializeMessage(string json);

    /// <summary>
    /// 序列化强类型对象为JSON
    /// </summary>
    /// <param name="message">OCPP消息对象</param>
    /// <returns>JSON字符串</returns>
    string SerializeMessage(IOcppMessage message);

    /// <summary>
    /// 验证消息格式
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateMessage(string json);

    /// <summary>
    /// 根据Action获取请求类型
    /// </summary>
    /// <param name="action">消息动作名称</param>
    /// <returns>请求类型</returns>
    Type? GetRequestType(string action);

    /// <summary>
    /// 根据Action获取响应类型
    /// </summary>
    /// <param name="action">消息动作名称</param>
    /// <returns>响应类型</returns>
    Type? GetResponseType(string action);
}
