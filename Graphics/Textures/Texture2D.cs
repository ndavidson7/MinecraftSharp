using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinecraftSharp.Graphics.Textures;

internal sealed class Texture2D : Texture
{
    public Texture2D(GL gl, string path) : base(gl)
    {
        Bind();
        
        using Image<Rgba32> image = LoadImage(path, out Memory<Rgba32> memory);
        gl.TexImage2D<Rgba32>(Target, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, memory.Span);
        
        SetParameters();
    }
    
    protected override TextureTarget Target => TextureTarget.Texture2D;
    
    private void SetParameters()
    {
        _gl.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.NearestMipmapNearest);
        _gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(Target, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(Target, TextureParameterName.TextureMaxLevel, 8);
        _gl.GenerateMipmap(Target);
    }
}
