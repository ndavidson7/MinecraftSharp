using Silk.NET.Maths;
using Silk.NET.Windowing;

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
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 1)),
#else
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 1)),
#endif
            IsVisible = false,
            FramesPerSecond = 60,
        };

        using Game game = new(options);
        game.Run();
    }
}
