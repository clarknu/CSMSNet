using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.Ocpp.Handlers;

/// <summary>
/// 请求-响应匹配器接口
/// </summary>
public interface ICallMatcher
{
    /// <summary>
    /// 注册待响应Call
    /// </summary>
    /// <param name="pendingCall">待处理Call</param>
    /// <returns>响应消息任务</returns>
    Task<IOcppMessage> RegisterCall(PendingCall pendingCall);

    /// <summary>
    /// 匹配响应消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="response">响应消息</param>
    /// <returns>是否匹配成功</returns>
    bool MatchResponse(string messageId, IOcppMessage response);

    /// <summary>
    /// 取消等待
    /// </summary>
    /// <param name="messageId">消息ID</param>
    void CancelCall(string messageId);

    /// <summary>
    /// 清理过期Call
    /// </summary>
    void CleanupExpired();

    /// <summary>
    /// 清理充电桩的所有待处理Call
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    void CleanupByChargePoint(string chargePointId);
}
