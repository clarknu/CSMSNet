using CSMSNet.OcppAdapter.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration; // Added
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSMSNet.Ocpp;

/// <summary>
/// OCPP服务器主机辅助类
/// 用于独立运行OCPP服务器
/// </summary>
public static class OcppServerHost
{
    /// <summary>
    /// 创建并运行OCPP服务器
    /// </summary>
    /// <param name="url">监听地址 (如果为null，则使用配置文件中的值)</param>
    /// <returns></returns>
    public static async Task RunAsync(string? url = null)
    {
        var builder = WebApplication.CreateBuilder();

        // 配置日志
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // 获取配置
        var configSection = builder.Configuration.GetSection("OcppAdapter");
        
        // 确定最终URL
        // 创建临时配置对象以获取默认值或绑定值
        var tempConfig = new OcppAdapterConfiguration();
        configSection.Bind(tempConfig);
        
        var finalUrl = !string.IsNullOrEmpty(url) ? url : tempConfig.ListenUrl;
        
        // 如果配置和参数都没有指定URL，且默认值为空（虽然不可能，因为有默认值），则使用默认
        if (string.IsNullOrEmpty(finalUrl))
        {
            finalUrl = "http://*:5000";
        }

        // 添加OCPP适配器服务
        builder.Services.AddOcppAdapter(config =>
        {
            // 从配置文件绑定
            configSection.Bind(config);
            
            // 如果提供了URL参数，覆盖配置
            if (!string.IsNullOrEmpty(url))
            {
                config.ListenUrl = url;
            }
        });

        var app = builder.Build();

        // 启用WebSocket
        app.UseWebSockets();

        // 使用OCPP适配器中间件
        app.UseOcppAdapter();

        // 启动应用
        Console.WriteLine($"Starting OCPP Server on {finalUrl}...");
        await app.RunAsync(finalUrl);
    }
}
