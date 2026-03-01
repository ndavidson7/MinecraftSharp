using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MinecraftSharp.Server.Configuration;

using Serilog;

namespace MinecraftSharp.Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddOptions<ServerOptions>()
            .BindConfiguration(ServerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TODO: not sure if this is or the client's entrypoint is more idiomatic
        builder.Services.AddHostedService<Server>();

        builder.Services.AddSerilog((services, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

        IHost host = builder.Build();
        await host.RunAsync();
    }
}