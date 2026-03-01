using Silk.NET.OpenGL;

namespace MinecraftSharp.Client;

internal class World
{
    private readonly string _name;

    public World(GL gl, string name)
    {
        _name = name;
        ChunkManager = new ChunkManager(gl);
    }

    public ChunkManager ChunkManager { get; }
}