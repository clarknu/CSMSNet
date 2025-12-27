using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.OcppAdapter.Server.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CSMSNet.OcppAdapter.Server;

/// <summary>
/// OCPP适配器扩展方法
/// </summary>
public static class OcppAdapterExtensions
{
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
