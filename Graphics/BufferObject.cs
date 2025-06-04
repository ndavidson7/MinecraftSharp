using Silk.NET.OpenGL;

namespace MinecraftSharp.Graphics;

internal class BufferObject<T> : IDisposable where T : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _disposed;

    public BufferTargetARB Type { get; init; }
    
    public BufferUsageARB Usage { get; init; }

    public uint Length { get; private set; }

    public BufferObject(GL gl, BufferTargetARB type, BufferUsageARB usage)
    {
        _gl = gl;
        _handle = _gl.GenBuffer();
        Type = type;
        Usage = usage;
    }

    public BufferObject(GL gl, BufferTargetARB type, BufferUsageARB usage, T[] data) : this(gl, type, usage)
    {
        SetData(data);
    }

    public void Bind() => _gl.BindBuffer(Type, _handle);

    public unsafe void SetData(T[] data)
    {
        Bind();
        _gl.BufferData<T>(Type, (nuint) (data.Length * sizeof(T)), data, Usage);
        Length = (uint) data.Length;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _gl.DeleteBuffer(_handle);
            }

            _disposed = true;
        }
    }

    ~BufferObject()
    {
        if (!_disposed)
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
