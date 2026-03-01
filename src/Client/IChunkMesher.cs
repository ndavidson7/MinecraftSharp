using MinecraftSharp.Engine.Graphics;

namespace MinecraftSharp.Client;

internal interface IChunkMesher
{
    /// <summary>
    /// Creates a mesh for the given chunk.
    /// </summary>
    /// <param name="chunk">The chunk to create a mesh for.</param>
    /// <returns>A mesh representing the chunk.</returns>
    Mesh<Vertex, uint> CreateMesh(Chunk chunk);
}