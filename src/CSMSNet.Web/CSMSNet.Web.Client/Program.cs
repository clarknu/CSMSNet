using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CSMSNet.Web.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        await builder.Build().RunAsync();
    }
}
