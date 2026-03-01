using System;

namespace MinecraftSharp.Engine.Graphics.Textures;

/// <summary>
/// The direction of a cubemap face
/// </summary>
public enum CubemapFaceDirection
{
    /// <summary>
    /// Right
    /// </summary>
    PositiveX,

    /// <summary>
    /// Left
    /// </summary>
    NegativeX,

    /// <summary>
    /// Top
    /// </summary>
    PositiveY,

    /// <summary>
    /// Bottom
    /// </summary>
    NegativeY,

    /// <summary>
    /// Front
    /// </summary>
    PositiveZ,

    /// <summary>
    /// Back
    /// </summary>
    NegativeZ
}

/// <summary>
/// Represents a face of a cubemap, associating an embedded resource with a specific direction within the cubemap.
/// </summary>
/// <remarks>Implements the IComparable interface to allow ordering of cubemap faces by their direction. This can
/// be useful when processing or rendering cubemaps in a consistent order.</remarks>
/// <param name="Resource">The embedded resource that provides the image or data for this cubemap face.</param>
/// <param name="Direction">The direction that specifies the orientation of this face within the cubemap.</param>
public record CubemapFace(EmbeddedResource Resource, CubemapFaceDirection Direction) : IComparable<CubemapFace>
{
    public int CompareTo(CubemapFace? other)
    {
        return Direction.CompareTo(other?.Direction);
    }
}