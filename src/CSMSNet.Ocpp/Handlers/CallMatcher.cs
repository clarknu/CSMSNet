using System.Collections.Concurrent;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Exceptions;
using CSMSNet.OcppAdapter.Models;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp.Handlers;

/// <summary>
/// 请求-响应匹配器实现
/// </summary>
public class CallMatcher : ICallMatcher
{
    private readonly ConcurrentDictionary<string, PendingCall> _pendingCalls = new();
    private readonly OcppAdapterConfiguration _configuration;
    private readonly ILogger<CallMatcher>? _logger;
    private readonly Timer _cleanupTimer;

    public CallMatcher(
        OcppAdapterConfiguration configuration,
        ILogger<CallMatcher>? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;

        // 启动定时清理任务
        _cleanupTimer = new Timer(
            _ => CleanupExpired(),
            null,
            _configuration.CallMatcherCleanupInterval,
            _configuration.CallMatcherCleanupInterval);
    }

    public async Task<IOcppMessage> RegisterCall(PendingCall pendingCall)
    {
        if (pendingCall == null)
            throw new ArgumentNullException(nameof(pendingCall));

        if (!_pendingCalls.TryAdd(pendingCall.MessageId, pendingCall))
        {
            _logger?.LogWarning(
                "Failed to register call, MessageId already exists: {MessageId}",
                pendingCall.MessageId);
            throw new InvalidOperationException($"MessageId {pendingCall.MessageId} already registered");
        }

        _logger?.LogDebug(
            "Registered call {Action} for charge point {ChargePointId}, MessageId: {MessageId}, Timeout: {Timeout}",
            pendingCall.Action,
            pendingCall.ChargePointId,
            pendingCall.MessageId,
            pendingCall.Timeout);

        try
        {
            // 等待响应或超时
            var timeoutTask = Task.Delay(pendingCall.Timeout, pendingCall.CancellationToken.Token);
            var completedTask = await Task.WhenAny(pendingCall.ResponseTask.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 超时
                _logger?.LogWarning(
                    "Call timeout for charge point {ChargePointId}, Action: {Action}, MessageId: {MessageId}",
                    pendingCall.ChargePointId,
                    pendingCall.Action,
                    pendingCall.MessageId);

                // 清理并抛出超时异常
                _pendingCalls.TryRemove(pendingCall.MessageId, out _);
                throw new OcppTimeoutException(
                    $"Command timeout after {pendingCall.Timeout.TotalSeconds}s for {pendingCall.Action}");
            }

            // 正常返回响应
            return await pendingCall.ResponseTask.Task;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation(
                "Call cancelled for charge point {ChargePointId}, MessageId: {MessageId}",
                pendingCall.ChargePointId,
                pendingCall.MessageId);
            throw;
        }
        finally
        {
            // 确保清理
            _pendingCalls.TryRemove(pendingCall.MessageId, out _);
            pendingCall.CancellationToken.Dispose();
        }
    }

    public bool MatchResponse(string messageId, IOcppMessage response)
    {
        if (string.IsNullOrEmpty(messageId))
            throw new ArgumentNullException(nameof(messageId));

        if (response == null)
            throw new ArgumentNullException(nameof(response));

        if (!_pendingCalls.TryGetValue(messageId, out var pendingCall))
        {
            _logger?.LogWarning(
                "Received response for unknown MessageId: {MessageId}",
                messageId);
            return false;
        }

        _logger?.LogDebug(
            "Matched response for charge point {ChargePointId}, MessageId: {MessageId}",
            pendingCall.ChargePointId,
            messageId);

        // 处理错误响应
        if (response is OcppError error)
        {
            var exception = new OcppProtocolException(
                $"Received CallError: {error.ErrorCode} - {error.ErrorDescription}")
            {
                ErrorCode = error.ErrorCode,
                ErrorDescription = error.ErrorDescription,
                ErrorDetails = error.ErrorDetails
            };

            pendingCall.ResponseTask.TrySetException(exception);
        }
        else
        {
            // 正常响应
            pendingCall.ResponseTask.TrySetResult(response);
        }

        return true;
    }

    public void CancelCall(string messageId)
    {
        if (_pendingCalls.TryRemove(messageId, out var pendingCall))
        {
            _logger?.LogInformation(
                "Cancelled call for charge point {ChargePointId}, MessageId: {MessageId}",
                pendingCall.ChargePointId,
                messageId);

            pendingCall.CancellationToken.Cancel();
            pendingCall.ResponseTask.TrySetCanceled();
            pendingCall.CancellationToken.Dispose();
        }
    }

    public void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        var expiredCalls = _pendingCalls.Values
            .Where(call => now - call.CreatedAt > call.Timeout)
            .ToList();

        if (expiredCalls.Count == 0)
            return;

        _logger?.LogInformation(
            "Cleaning up {Count} expired calls",
            expiredCalls.Count);

        foreach (var call in expiredCalls)
        {
            if (_pendingCalls.TryRemove(call.MessageId, out _))
            {
                _logger?.LogWarning(
                    "Expired call for charge point {ChargePointId}, Action: {Action}, MessageId: {MessageId}",
                    call.ChargePointId,
                    call.Action,
                    call.MessageId);

                var exception = new OcppTimeoutException(
                    $"Call expired after {call.Timeout.TotalSeconds}s for {call.Action}");
                call.ResponseTask.TrySetException(exception);
                call.CancellationToken.Dispose();
            }
        }
    }

    public void CleanupByChargePoint(string chargePointId)
    {
        var calls = _pendingCalls.Values
            .Where(call => call.ChargePointId == chargePointId)
            .ToList();

        if (calls.Count == 0)
            return;

        _logger?.LogInformation(
            "Cleaning up {Count} pending calls for charge point {ChargePointId}",
            calls.Count,
            chargePointId);

        foreach (var call in calls)
        {
            CancelCall(call.MessageId);
        }
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();

        // 取消所有待处理的Call
        foreach (var call in _pendingCalls.Values)
        {
            call.CancellationToken.Cancel();
            call.ResponseTask.TrySetCanceled();
            call.CancellationToken.Dispose();
        }

        _pendingCalls.Clear();
    }
}
