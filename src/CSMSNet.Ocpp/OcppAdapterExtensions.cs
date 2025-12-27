using CSMSNet.OcppAdapter.Abstractions;
using CSMSNet.OcppAdapter.Configuration;
using CSMSNet.Ocpp.Transport;
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
            // 手动复制属性，或者我们可以修改AddOcppAdapter以接受现有的实例
            // 但为了保持简单，我们这里使用反射来复制属性，或者因为AddOcppAdapter只是为了配置一个新实例
            // 我们可以直接注册实例，跳过AddOcppAdapter的内部逻辑吗？
            // 不，AddOcppAdapter做了很多注册工作。
            
            // 更好的方式：重构 AddOcppAdapter 内部逻辑
            // 但既然我不能轻易修改所有引用，我将在这里复制属性
            // 实际上，OcppAdapterConfiguration 只有属性，可以使用 MemberwiseClone 或者逐个赋值
            // 但为了稳健，我们使用 System.Text.Json 或反射
            
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
