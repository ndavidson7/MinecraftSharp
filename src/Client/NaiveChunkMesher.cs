using System;

using MinecraftSharp.Engine.Graphics;

namespace MinecraftSharp.Client;

internal class NaiveChunkMesher : IChunkMesher
{
    public Mesh<Vertex, uint> CreateMesh(Chunk chunk)
    {
        throw new NotImplementedException();
    }
}