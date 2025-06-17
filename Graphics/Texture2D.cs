using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MinecraftSharp.Graphics;

internal class Texture2D : OpenGLObject
{
    private const TextureTarget Target = TextureTarget.Texture2D;

    private static readonly DecoderOptions DecoderOptions;

    static Texture2D()
    {
        Configuration configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        DecoderOptions = new DecoderOptions
        {
            Configuration = configuration,
        };
    }

    public Texture2D(GL gl, string path, TextureUnit slot = TextureUnit.Texture0, TextureType type = TextureType.None) : base(gl, gl.GenTexture())
    {
        Slot = slot;
        Type = type;

        Bind();

        LoadImage(gl, path);

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

    protected override void DisposeManaged() => _gl.DeleteTexture(_handle);

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

    private static unsafe void LoadImage(GL gl, string path)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(DecoderOptions, path);
        image.Mutate(x => x.Flip(FlipMode.Vertical)); // OpenGL expects the origin at the bottom-left

        if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            throw new InvalidOperationException("Could not get single image buffer pointer. Image is too large or PreferContiguousImageBuffers is set to false.");

        gl.TexImage2D<Rgba32>(Target, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, memory.Span);
    }
}
