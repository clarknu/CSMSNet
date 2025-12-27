using System.Net.WebSockets;
using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Models.V16.Enums;
using CSMSNet.OcppAdapter.Server.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Transport;

/// <summary>
/// WebSocket服务器
/// 注意: 需要ASP.NET Core环境才能使用此类
/// </summary>
public class WebSocketServer
{
    private readonly OcppAdapterConfiguration _configuration;
    private readonly IConnectionManager _connectionManager;
    private readonly IMessageRouter _messageRouter;
    private readonly ILogger<WebSocketServer>? _logger;
    private readonly IOcppAdapter? _ocppAdapter; // 需要访问StateCache, 但最好通过接口或服务

    public WebSocketServer(
        OcppAdapterConfiguration configuration,
        IConnectionManager connectionManager,
        IMessageRouter messageRouter,
        ILogger<WebSocketServer>? logger = null,
        IOcppAdapter? ocppAdapter = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
        _logger = logger;
        _ocppAdapter = ocppAdapter;
    }

    /// <summary>
    /// 处理WebSocket连接请求
    /// 使用方式: 在ASP.NET Core中间件中调用此方法
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns></returns>
    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        // 提取充电桩ID
        var chargePointId = ExtractChargePointId(context);
        if (string.IsNullOrEmpty(chargePointId))
        {
            _logger?.LogWarning("Failed to extract charge point ID from request path: {Path}", context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid charge point ID");
            return;
        }

        // 子协议协商
        var subProtocol = NegotiateSubProtocol(context);
        if (string.IsNullOrEmpty(subProtocol))
        {
            _logger?.LogWarning("No valid OCPP sub-protocol found for charge point {ChargePointId}", chargePointId);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid sub-protocol");
            return;
        }

        try
        {
            // 接受WebSocket连接
            var webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol);
            
            _logger?.LogInformation(
                "WebSocket connection established for charge point {ChargePointId}, sub-protocol: {SubProtocol}, remote: {RemoteIp}",
                chargePointId,
                subProtocol,
                context.Connection.RemoteIpAddress);

            // 检查是否已有 Accepted 状态
            bool isVerified = false;
            if (_ocppAdapter != null)
            {
                var info = _ocppAdapter.GetChargePointInfo(chargePointId);
                if (info?.Status == RegistrationStatus.Accepted)
                {
                    isVerified = true;
                    _logger?.LogInformation("Charge point {ChargePointId} is already verified (resuming session)", chargePointId);
                }
            }

            // 创建会话
            var session = new WebSocketSession(
                webSocket,
                chargePointId,
                subProtocol,
                _messageRouter,
                null, // Logger
                isVerified);

            // 添加到连接管理器
            if (!_connectionManager.AddSession(session))
            {
                _logger?.LogWarning(
                    "Failed to add session for charge point {ChargePointId}, closing connection",
                    chargePointId);
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Connection rejected",
                    CancellationToken.None);
                return;
            }

            // 订阅断开事件
            session.Disconnected += (sender, reason) =>
            {
                _connectionManager.RemoveSession(chargePointId);
                _logger?.LogInformation(
                    "Session removed for charge point {ChargePointId}, reason: {Reason}",
                    chargePointId,
                    reason);
            };

            // 启动接收循环
            await session.StartReceiving();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error handling WebSocket connection for charge point {ChargePointId}",
                chargePointId);
            throw;
        }
    }

    /// <summary>
    /// 从请求中提取充电桩ID
    /// 支持两种方式:
    /// 1. URL路径: /ocpp/{chargePointId}
    /// 2. 查询参数: ?chargePointId=xxx
    /// </summary>
    private string ExtractChargePointId(HttpContext context)
    {
        // 尝试从路径提取
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path))
        {
            // 假设格式为 /ocpp/{chargePointId} 或 /{chargePointId}
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 1)
            {
                // 如果第一段是 "ocpp", 取第二段; 否则取第一段
                var chargePointId = segments.Length >= 2 && segments[0].Equals("ocpp", StringComparison.OrdinalIgnoreCase)
                    ? segments[1]
                    : segments[0];
                
                if (!string.IsNullOrWhiteSpace(chargePointId))
                    return chargePointId;
            }
        }

        // 尝试从查询参数提取
        if (context.Request.Query.TryGetValue("chargePointId", out var queryValue))
        {
            var chargePointId = queryValue.ToString();
            if (!string.IsNullOrWhiteSpace(chargePointId))
                return chargePointId;
        }

        return string.Empty;
    }

    /// <summary>
    /// 协商WebSocket子协议
    /// 支持的协议: ocpp1.6, ocpp1.5
    /// </summary>
    private string NegotiateSubProtocol(HttpContext context)
    {
        var requestedProtocols = context.WebSockets.WebSocketRequestedProtocols;
        
        // 支持的OCPP协议列表(优先级从高到低)
        var supportedProtocols = new[] { "ocpp1.6", "ocpp1.5" };

        // 查找第一个匹配的协议
        foreach (var supported in supportedProtocols)
        {
            if (requestedProtocols.Contains(supported, StringComparer.OrdinalIgnoreCase))
            {
                return supported;
            }
        }

        // 如果没有请求子协议,默认使用ocpp1.6
        if (requestedProtocols.Count == 0)
        {
            return "ocpp1.6";
        }

        return string.Empty;
    }
}
