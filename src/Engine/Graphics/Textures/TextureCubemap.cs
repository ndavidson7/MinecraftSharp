using System;
using System.IO;

using Silk.NET.OpenGL;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinecraftSharp.Engine.Graphics.Textures;

/// <summary>
/// Represents a cubemap texture composed of six faces, each corresponding to a direction in 3D space. Provides
/// functionality for creating and managing cubemap textures for use in graphics rendering.
/// </summary>
/// <remarks>Requires exactly six faces, each mapped to a different direction.</remarks>
public class TextureCubemap : Texture
{
    public TextureCubemap(GL gl, params CubemapFace[] faces) : base(gl)
    {
        if (faces.Length != 6)
            throw new ArgumentException("Must provide 6 faces", nameof(faces));

        Bind();

        Array.Sort(faces);
        for (int i = 0; i < faces.Length; i++)
        {
            CubemapFace face = faces[i];
            if ((int)face.Direction != i)
            {
                throw new ArgumentException($"Missing {typeof(CubemapFace).Name} with direction {(CubemapFaceDirection)i}", nameof(faces));
            }

            using Stream stream = face.Resource.GetStream();
            using Image<Rgba32> image = LoadImage(stream, out Memory<Rgba32> memory, false);
            gl.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, memory.Span);
        }

        SetParameters();
    }

    protected override TextureTarget Target => TextureTarget.TextureCubeMap;

    private void SetParameters()
    {
        _gl.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureWrapR, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
    }
}