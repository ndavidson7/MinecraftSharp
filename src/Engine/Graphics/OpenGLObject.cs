using System;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Engine.Graphics;

/// <summary>
/// Represents an abstract base class for OpenGL objects, providing common functionality for managing OpenGL resources.
/// </summary>
/// <remarks>
/// This class implements the IDisposable interface to ensure proper resource management. Derived classes
/// must implement the disposal of both managed and unmanaged resources appropriately.
/// </remarks>
public abstract class OpenGLObject : IDisposable
{
    protected readonly GL _gl;
    protected readonly uint _handle;
    protected bool _disposed;

    /// <summary>
    /// Initializes a new instance of the OpenGLObject class using the specified OpenGL context and object handle.
    /// </summary>
    /// <remarks>Ensure that the provided OpenGL context is current before creating or using this object, as
    /// rendering operations depend on the active context.</remarks>
    /// <param name="gl">The OpenGL context to associate with this object.</param>
    /// <param name="handle">The unique handle identifying the OpenGL object.</param>
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