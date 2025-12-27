using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Models.Events;
using CSMSNet.OcppAdapter.Models.V16.Requests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp.Services;

/// <summary>
/// 充电桩自动询问服务
/// 在充电桩连接并BootNotification成功后，自动查询配置和状态
/// </summary>
public class ChargePointInterrogator : IHostedService
{
    private readonly IOcppAdapter _ocppAdapter;
    private readonly ILogger<ChargePointInterrogator> _logger;

    public ChargePointInterrogator(IOcppAdapter ocppAdapter, ILogger<ChargePointInterrogator> logger)
    {
        _ocppAdapter = ocppAdapter;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ocppAdapter.OnBootNotification += OnBootNotification;
        _logger.LogInformation("ChargePoint Interrogator started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ocppAdapter.OnBootNotification -= OnBootNotification;
        return Task.CompletedTask;
    }

    private void OnBootNotification(object? sender, BootNotificationEventArgs e)
    {
        // 异步执行询问逻辑，避免阻塞事件处理
        _ = Task.Run(async () =>
        {
            try
            {
                // 等待响应发送完成 (2秒)
                await Task.Delay(2000);

                // 检查桩是否在线（可能boot后立即断开）
                if (!_ocppAdapter.IsChargePointOnline(e.ChargePointId))
                {
                    _logger.LogWarning("Charge point {ChargePointId} offline, skipping interrogation.", e.ChargePointId);
                    return;
                }

                _logger.LogInformation("Starting interrogation for {ChargePointId}", e.ChargePointId);

                // 1. 获取所有配置
                try
                {
                    var configReq = new GetConfigurationRequest(); // 空请求获取所有
                    await _ocppAdapter.GetConfigurationAsync(e.ChargePointId, configReq);
                    _logger.LogInformation("Configuration retrieved for {ChargePointId}", e.ChargePointId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("GetConfiguration failed for {ChargePointId}: {Message}", e.ChargePointId, ex.Message);
                }

                // 2. 获取本地鉴权列表版本
                try
                {
                    var listReq = new GetLocalListVersionRequest();
                    await _ocppAdapter.GetLocalListVersionAsync(e.ChargePointId, listReq);
                    _logger.LogInformation("LocalListVersion retrieved for {ChargePointId}", e.ChargePointId);
                }
                catch (Exception ex)
                {
                    // 很多桩不支持此命令，仅记录Debug
                    _logger.LogDebug("GetLocalListVersion failed for {ChargePointId}: {Message}", e.ChargePointId, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during interrogation for {ChargePointId}", e.ChargePointId);
            }
        });
    }
}
