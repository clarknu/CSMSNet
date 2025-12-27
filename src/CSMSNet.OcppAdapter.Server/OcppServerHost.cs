using CSMSNet.OcppAdapter.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSMSNet.OcppAdapter.Server;

/// <summary>
/// OCPP服务器主机辅助类
/// 用于独立运行OCPP服务器
/// </summary>
public static class OcppServerHost
{
    /// <summary>
    /// 创建并运行OCPP服务器
    /// </summary>
    /// <param name="url">监听地址</param>
    /// <returns></returns>
    public static async Task RunAsync(string url = "http://*:5000")
    {
        var builder = WebApplication.CreateBuilder();

        // 配置日志
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // 添加OCPP适配器服务
        builder.Services.AddOcppAdapter(config =>
        {
            config.ListenUrl = url;
            config.HeartbeatInterval = TimeSpan.FromMinutes(5);
            config.MaxConcurrentConnections = 1000;
        });

        var app = builder.Build();

        // 启用WebSocket
        app.UseWebSockets();

        // 使用OCPP适配器中间件
        app.UseOcppAdapter();

        // 启动应用
        Console.WriteLine($"Starting OCPP Server on {url}...");
        await app.RunAsync(url);
    }
}
