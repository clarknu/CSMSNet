using CSMSNet.OcppAdapter.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp.Transport;

/// <summary>
/// OCPP WebSocket中间件
/// </summary>
public class OcppWebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOcppAdapter _adapter;
    private readonly ILogger<OcppWebSocketMiddleware>? _logger;

    public OcppWebSocketMiddleware(
        RequestDelegate next,
        IOcppAdapter adapter,
        ILogger<OcppWebSocketMiddleware>? logger = null)
    {
        _next = next;
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 检查是否是WebSocket请求，并且路径匹配(这里假设路径由外部路由控制，或者我们在HandleWebSocketAsync中检查)
        // 但通常中间件会被映射到特定路径，所以这里只检查是否是WebSocket请求
        if (context.WebSockets.IsWebSocketRequest)
        {
            // 由于IOcppAdapter接口没有暴露HandleWebSocketAsync，我们需要转换类型
            // 或者修改IOcppAdapter接口。
            // 建议修改IOcppAdapter接口以支持HandleWebSocketAsync，或者直接依赖OcppAdapter类
            // 但依赖具体类不好。
            // 我们在OcppAdapter.cs中添加了HandleWebSocketAsync，但它是OcppAdapter的方法。
            
            if (_adapter is OcppAdapter concreteAdapter)
            {
                await concreteAdapter.HandleWebSocketAsync(context);
                return;
            }
            else
            {
                _logger?.LogWarning("IOcppAdapter implementation is not OcppAdapter, cannot handle WebSocket request");
            }
        }

        await _next(context);
    }
}
