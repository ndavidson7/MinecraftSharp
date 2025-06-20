using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MinecraftSharp.Graphics.Textures;

internal abstract class Texture : OpenGLObject
{
    protected static readonly DecoderOptions DecoderOptions;
    
    static Texture()
    {
        Configuration configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        DecoderOptions = new DecoderOptions
        {
            Configuration = configuration,
        };
    }
    
    public Texture(GL gl) : base(gl, gl.GenTexture())
    { }
    
    protected abstract TextureTarget Target { get; }
    
    public void Bind() => _gl.BindTexture(Target, _handle);
    
    public void Unbind() => _gl.BindTexture(Target, 0);

    public void Use(TextureUnit slot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(slot);
        Bind();
    }
    
    protected override void DisposeManaged() => _gl.DeleteTexture(_handle);
    
    protected static unsafe Image<Rgba32> LoadImage(string path, out Memory<Rgba32> memory, bool flip = true)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(DecoderOptions, path);
        if (flip)
            image.Mutate(x => x.Flip(FlipMode.Vertical)); // OpenGL expects the origin at the bottom-left

        if (!image.DangerousTryGetSinglePixelMemory(out memory))
            throw new InvalidOperationException("Could not get single image buffer pointer. Image is too large or PreferContiguousImageBuffers is set to false.");

        return image;
    }
}