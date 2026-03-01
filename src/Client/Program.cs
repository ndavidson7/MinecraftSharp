using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MinecraftSharp.Client.Configuration;

using Serilog;

namespace MinecraftSharp.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddOptions<Settings>()
            .BindConfiguration(Settings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<Game>();

        builder.Services.AddSerilog((services, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

        IHost host = builder.Build();
        await host.StartAsync();

        // Start game
        using var game = host.Services.GetRequiredService<Game>();
        game.Run();

        await host.StopAsync();
    }
}