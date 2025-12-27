using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Server.Handlers;

/// <summary>
/// 请求处理器接口
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// 处理充电桩请求
    /// </summary>
    /// <param name="chargePointId">充电桩ID</param>
    /// <param name="request">请求对象</param>
    /// <returns></returns>
    Task HandleRequestAsync(string chargePointId, OcppRequest request);
}
