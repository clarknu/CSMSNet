using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.Ocpp.Transport;
using CSMSNet.Ocpp.Services; // Added namespace
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSMSNet.Ocpp;

/// <summary>
/// OCPP适配器扩展方法
/// </summary>
public static class OcppAdapterExtensions
{
    /// <summary>
    /// 添加OCPP适配器服务 (从IConfiguration加载配置)
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用程序配置</param>
    /// <param name="sectionName">配置节点名称</param>
    /// <returns></returns>
    public static IServiceCollection AddOcppAdapter(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "OcppAdapter")
    {
        var ocppConfig = new OcppAdapterConfiguration();
        configuration.GetSection(sectionName).Bind(ocppConfig);
        
        return services.AddOcppAdapter(config =>
        {
            // 手动复制属性
            foreach (var prop in typeof(OcppAdapterConfiguration).GetProperties())
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    prop.SetValue(config, prop.GetValue(ocppConfig));
                }
            }
        });
    }

    /// <summary>
    /// 添加OCPP适配器服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns></returns>
    public static IServiceCollection AddOcppAdapter(
        this IServiceCollection services, 
        Action<OcppAdapterConfiguration>? configure = null)
    {
        var configuration = new OcppAdapterConfiguration();
        configure?.Invoke(configuration);
        
        services.AddSingleton(configuration);
        // 注册接口和实现
        services.AddSingleton<OcppAdapter>();
        services.AddSingleton<IOcppAdapter>(sp => sp.GetRequiredService<OcppAdapter>());
        services.AddHostedService(sp => sp.GetRequiredService<OcppAdapter>());
        
        // 注册自动询问服务
        services.AddHostedService<ChargePointInterrogator>();
        
        return services;
    }
    
    /// <summary>
    /// 使用OCPP适配器中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns></returns>
    public static IApplicationBuilder UseOcppAdapter(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OcppWebSocketMiddleware>();
    }
}
