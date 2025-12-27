using System.Net.WebSockets;
using System.Text;
using CSMSNet.OcppAdapter.Models.Enums;
using CSMSNet.OcppAdapter.Server.Handlers;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server.Transport;

/// <summary>
/// WebSocket会话
/// </summary>
public class WebSocketSession
{
    private readonly WebSocket _webSocket;
    private readonly IMessageRouter _messageRouter;
    private readonly ILogger<WebSocketSession>? _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// 充电桩ID
    /// </summary>
    public string ChargePointId { get; }

    /// <summary>
    /// WebSocket对象
    /// </summary>
    public WebSocket WebSocket => _webSocket;

    /// <summary>
    /// 会话状态
    /// </summary>
    public SessionState State { get; private set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    public string ProtocolVersion { get; }

    /// <summary>
    /// 连接建立时间
    /// </summary>
    public DateTime ConnectedAt { get; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    /// <summary>
    /// 已发送消息数
    /// </summary>
    public int MessagesSent { get; private set; }

    /// <summary>
    /// 已接收消息数
    /// </summary>
    public int MessagesReceived { get; private set; }

    /// <summary>
    /// 断开连接事件
    /// </summary>
    public event EventHandler<string>? Disconnected;

    public WebSocketSession(
        WebSocket webSocket,
        string chargePointId,
        string protocolVersion,
        IMessageRouter messageRouter,
        ILogger<WebSocketSession>? logger = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
        _logger = logger;

        SessionId = Guid.NewGuid().ToString();
        ChargePointId = chargePointId ?? throw new ArgumentNullException(nameof(chargePointId));
        ProtocolVersion = protocolVersion ?? "1.6";
        ConnectedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        State = SessionState.Connected;
    }

    /// <summary>
    /// 启动接收循环
    /// </summary>
    /// <returns></returns>
    public async Task StartReceiving()
    {
        State = SessionState.Connected;
        var buffer = new byte[1024 * 16]; // 16KB缓冲区
        var messageBuilder = new StringBuilder();

        try
        {
            _logger?.LogInformation(
                "Start receiving messages for charge point {ChargePointId}, session {SessionId}",
                ChargePointId,
                SessionId);

            while (_webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                messageBuilder.Clear();
                WebSocketReceiveResult result;

                // 读取完整消息(处理分帧)
                do
                {
                    result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger?.LogInformation(
                            "Received close message from charge point {ChargePointId}",
                            ChargePointId);
                        await HandleCloseAsync(result.CloseStatus, result.CloseStatusDescription);
                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(text);
                    }

                } while (!result.EndOfMessage);

                // 更新活动时间
                UpdateActivity();
                MessagesReceived++;

                // 处理消息
                var message = messageBuilder.ToString();
                if (!string.IsNullOrEmpty(message))
                {
                    _logger?.LogDebug(
                        "Received message from {ChargePointId}: {Message}",
                        ChargePointId,
                        message.Length > 200 ? message.Substring(0, 200) + "..." : message);

                    // 转发至消息路由器
                    try
                    {
                        await _messageRouter.RouteIncoming(ChargePointId, message);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex,
                            "Error routing message from charge point {ChargePointId}",
                            ChargePointId);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation(
                "Receiving cancelled for charge point {ChargePointId}",
                ChargePointId);
        }
        catch (WebSocketException ex)
        {
            _logger?.LogWarning(ex,
                "WebSocket error for charge point {ChargePointId}",
                ChargePointId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Unexpected error in receive loop for charge point {ChargePointId}",
                ChargePointId);
        }
        finally
        {
            await HandleDisconnectAsync("Receive loop ended");
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <returns></returns>
    public async Task SendMessageAsync(string message)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            _logger?.LogWarning(
                "Cannot send message, WebSocket state is {State} for charge point {ChargePointId}",
                _webSocket.State,
                ChargePointId);
            return;
        }

        try
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token);

            UpdateActivity();
            MessagesSent++;

            _logger?.LogDebug(
                "Sent message to {ChargePointId}: {Message}",
                ChargePointId,
                message.Length > 200 ? message.Substring(0, 200) + "..." : message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error sending message to charge point {ChargePointId}",
                ChargePointId);
            throw;
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="closeStatus">关闭状态</param>
    /// <param name="statusDescription">状态描述</param>
    /// <returns></returns>
    public async Task CloseAsync(
        WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
        string? statusDescription = null)
    {
        if (State == SessionState.Closing || State == SessionState.Closed)
            return;

        State = SessionState.Closing;

        try
        {
            if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
            {
                await _webSocket.CloseAsync(
                    closeStatus,
                    statusDescription ?? "Normal closure",
                    CancellationToken.None);
            }

            _logger?.LogInformation(
                "WebSocket closed for charge point {ChargePointId}, reason: {Reason}",
                ChargePointId,
                statusDescription ?? "Normal closure");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Error closing WebSocket for charge point {ChargePointId}",
                ChargePointId);
        }
        finally
        {
            State = SessionState.Closed;
            _cancellationTokenSource.Cancel();
            _webSocket.Dispose();
        }
    }

    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    private async Task HandleCloseAsync(WebSocketCloseStatus? closeStatus, string? description)
    {
        _logger?.LogInformation(
            "Handling close for charge point {ChargePointId}, status: {Status}, description: {Description}",
            ChargePointId,
            closeStatus,
            description);

        await CloseAsync(closeStatus ?? WebSocketCloseStatus.NormalClosure, description);
        await HandleDisconnectAsync(description ?? "Client closed connection");
    }

    private async Task HandleDisconnectAsync(string reason)
    {
        State = SessionState.Closed;

        _logger?.LogInformation(
            "Charge point {ChargePointId} disconnected, reason: {Reason}",
            ChargePointId,
            reason);

        // 触发断开事件
        Disconnected?.Invoke(this, reason);

        await Task.CompletedTask;
    }
}
