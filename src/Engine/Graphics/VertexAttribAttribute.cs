using System;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Engine.Graphics;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class VertexAttribAttribute : Attribute
{
    /// <summary>
    /// The index of the vertex attribute
    /// </summary>
    public uint Index { get; }

    /// <summary>
    /// The number of components in the vertex attribute
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// The data type of all components in the vertex attribute
    /// </summary>
    public VertexAttribPointerType Type { get; }

    /// <summary>
    /// Whether the components should be converted to floats via integer normalization, e.g., an unsigned byte value of 255 becomes 1.0f
    /// </summary>
    public bool Normalized { get; }

    /// <summary>
    /// The byte offset between consecutive vertex attribute data
    /// </summary>
    public uint Stride { get; }

    /// <summary>
    /// The byte offset of the first component of the vertex attribute in the vertex data
    /// </summary>
    public int Offset { get; }

    public VertexAttribAttribute(uint index, int size, VertexAttribPointerType type, bool normalized, uint stride, int offset)
    {
        Index = index;
        Size = size;
        Type = type;
        Normalized = normalized;
        Stride = stride;
        Offset = offset;
    }
}