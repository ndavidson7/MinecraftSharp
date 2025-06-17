using Silk.NET.OpenGL;

namespace MinecraftSharp.Graphics;

internal abstract class OpenGLObject : IDisposable
{
    protected readonly GL _gl;
    protected readonly uint _handle;
    protected bool _disposed;

    protected OpenGLObject(GL gl, uint handle)
    {
        _gl = gl;
        _handle = handle;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                DisposeManaged();

            DisposeUnmanaged();

            _disposed = true;
        }
    }

    protected virtual void DisposeManaged() { }
    protected virtual void DisposeUnmanaged() { }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
