using System.Numerics;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Engine.Graphics;

public readonly record struct Vertex
{
    private const int SizeInBytes = 32;

    [VertexAttrib(0, 3, VertexAttribPointerType.Float, false, SizeInBytes, 0)]
    public readonly Vector3 Position;

    [VertexAttrib(1, 3, VertexAttribPointerType.Float, false, SizeInBytes, 12)]
    public readonly Vector3 Normal;

    [VertexAttrib(2, 2, VertexAttribPointerType.Float, false, SizeInBytes, 24)]
    public readonly Vector2 TextureCoordinates;

    public Vertex(Vector3 position, Vector3 normal, Vector2 textureCoordinates)
    {
        Position = position;
        Normal = normal;
        TextureCoordinates = textureCoordinates;
    }
}