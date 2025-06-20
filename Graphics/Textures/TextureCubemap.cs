using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinecraftSharp.Graphics.Textures;

internal class TextureCubemap : Texture
{
    public TextureCubemap(GL gl, string[] paths) : base(gl)
    {
        if (paths.Length != 6)
            throw new ArgumentException("The array of paths must contain 6 paths");
        
        Bind();

        for (int i = 0; i < paths.Length; i++)
        {
            string path = paths[i];
            
            using Image<Rgba32> image = LoadImage(path, out Memory<Rgba32> memory, false);
            gl.TexImage2D<Rgba32>(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, memory.Span);
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