using System.Collections.Generic;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Engine.Graphics;

public class BufferObject<T> : OpenGLObject where T : unmanaged
{
    public BufferTargetARB Type { get; init; }

    public BufferUsageARB Usage { get; init; }

    public uint Length { get; private set; }

    public BufferObject(GL gl, BufferTargetARB type, BufferUsageARB usage) : base(gl, gl.GenBuffer())
    {
        Type = type;
        Usage = usage;
    }

    public BufferObject(GL gl, BufferTargetARB type, BufferUsageARB usage, IEnumerable<T> data) : this(gl, type, usage)
    {
        SetData([.. data]);
    }

    public void Bind() => _gl.BindBuffer(Type, _handle);

    public unsafe void SetData(T[] data)
    {
        Bind();
        _gl.BufferData(Type, (nuint)(data.Length * sizeof(T)), data, Usage);
        Length = (uint)data.Length;
    }

    protected override void DisposeManaged() => _gl.DeleteBuffer(_handle);
}