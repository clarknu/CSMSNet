using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 消息路由器接口
/// </summary>
public interface IMessageRouter
{
    /// <summary>
    /// 路由入站消息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="json">JSON消息</param>
    /// <returns></returns>
    Task RouteIncoming(string chargePointId, string json);

    /// <summary>
    /// 路由响应消息
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="message">OCPP消息对象</param>
    /// <returns></returns>
    Task RouteResponse(string chargePointId, IOcppMessage message);
}
