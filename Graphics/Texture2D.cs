using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinecraftSharp.Graphics;

internal class Texture2D : IDisposable
{
    private const TextureTarget Target = TextureTarget.Texture2D;

    private readonly GL _gl;
    private readonly uint _handle;

    public Texture2D(GL gl, string path, TextureUnit slot = TextureUnit.Texture0, TextureType type = TextureType.None)
    {
        _gl = gl;
        _handle = _gl.GenTexture();
        Slot = slot;
        Type = type;

        Bind();

        LoadImage(path);

        SetParameters();
    }

    public TextureUnit Slot { get; }

    public TextureType Type { get; }

    public void Bind() => _gl.BindTexture(Target, _handle);

    public void Unbind() => _gl.BindTexture(Target, 0);

    public void Use()
    {
        _gl.ActiveTexture(Slot);
        Bind();
    }

    public void Dispose() => _gl.DeleteTexture(_handle);

    private void SetParameters()
    {
        _gl.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        _gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(Target, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(Target, TextureParameterName.TextureMaxLevel, 8);
        _gl.GenerateMipmap(Target);
    }

    private unsafe void LoadImage(string path)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(path);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                ReadOnlySpan<Rgba32> pixelRow = accessor.GetRowSpan(y);
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixelRow);
            }
        });
    }
}
