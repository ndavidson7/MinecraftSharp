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
            API = GraphicsAPI.Default,
            IsVisible = false,
            ShouldSwapAutomatically = true,
            FramesPerSecond = 60,
        };

        using Game game = new(options);
        game.Run();
    }
}
