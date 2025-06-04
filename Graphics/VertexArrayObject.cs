using Silk.NET.OpenGL;

namespace MinecraftSharp.Graphics;

internal class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private uint _numIndices;
    private bool _disposed;

    public VertexArrayObject(GL gl)
    {
        _gl = gl;
        _handle = _gl.GenVertexArray();
    }

    public unsafe VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo) : this(gl)
    {
        Bind();

        vbo.Bind();
        ebo.Bind();

        _numIndices = ebo.Length;

        Unbind();
    }

    public void Bind() => _gl.BindVertexArray(_handle);

    public void Unbind() => _gl.BindVertexArray(0);

    public unsafe void AddVertexAttribute(uint index, int size, VertexAttribPointerType type, bool normalized, uint stride, int offset)
    {
        Bind();
        _gl.VertexAttribPointer(index, size, type, normalized, stride * (uint) sizeof(TVertexType), offset * sizeof(TVertexType));
        _gl.EnableVertexAttribArray(index);
        Unbind();
    }

    public unsafe void Draw()
    {
        Bind();
        _gl.DrawElements(PrimitiveType.Triangles, _numIndices, DrawElementsType.UnsignedInt, (void*) 0);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _gl.DeleteVertexArray(_handle);
            }

            _disposed = true;
        }
    }

    ~VertexArrayObject()
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
