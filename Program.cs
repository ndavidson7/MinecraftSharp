using Serilog;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Diagnostics;

namespace MinecraftSharp;

internal class Program
{
    static void Main(string[] args)
    {
        // TODO: Serialize and deserialize these
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = "MinecraftSharp",
#if DEBUG
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug | ContextFlags.ForwardCompatible, new APIVersion(4, 1)),
#else
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 1)),
#endif
            IsVisible = false,
            FramesPerSecond = 60,
            VSync = false,
            Samples = 4, // Enable 4x MSAA
        };

        try
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/minecraftsharp.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize logging: {ex}");
            return;
        }

        try
        {
            Log.Information("Creating window");
            
            using Game game = new(options);

            Log.Information("Starting game");

            game.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "A fatal error occurred");
        }
    }
}
