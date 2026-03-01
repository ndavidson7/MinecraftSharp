using System.Collections.Generic;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Client;

internal class ChunkManager
{
    private readonly GL _gl;
    private readonly Dictionary<(int, int, int), Chunk> _chunks = [];
    private readonly byte _chunkSize;

    public ChunkManager(GL gl, byte chunkSize = 16)
    {
        _gl = gl;
        _chunkSize = chunkSize;
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        var key = (x / _chunkSize, y / _chunkSize, z / _chunkSize);
        if (!_chunks.TryGetValue(key, out Chunk? chunk))
        {
            chunk = new Chunk(_gl, key.Item1, key.Item2, key.Item3, _chunkSize);
            _chunks[key] = chunk;
        }
        return chunk;
    }

    public void Update(double deltaTime)
    {
        foreach (Chunk chunk in _chunks.Values)
        {
            chunk.Update(deltaTime);
        }
    }

    public void Draw()
    {
        foreach (Chunk chunk in _chunks.Values)
        {
            chunk.Draw();
        }
    }
}