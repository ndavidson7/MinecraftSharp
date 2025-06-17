using Silk.NET.OpenGL;

namespace MinecraftSharp.Graphics;

internal class VertexArrayObject<TVertex, TIndex> : OpenGLObject
    where TVertex : unmanaged
    where TIndex : unmanaged
{
    private uint _numIndices;

    public VertexArrayObject(GL gl) : base(gl, gl.GenVertexArray())
    { }

    public VertexArrayObject(GL gl, BufferObject<TVertex> vbo, BufferObject<TIndex> ebo) : this(gl)
    {
        Bind();

        vbo.Bind();
        ebo.Bind();

        _numIndices = ebo.Length;

        ReflectVertexAttributes();

        Unbind();
    }

    public void Bind() => _gl.BindVertexArray(_handle);

    public void Unbind() => _gl.BindVertexArray(0);

    public unsafe void AddVertexAttribute(uint index, int size, VertexAttribPointerType type, bool normalized, uint stride, int offset)
    {
        Bind();
        _gl.VertexAttribPointer(index, size, type, normalized, stride, offset);
        _gl.EnableVertexAttribArray(index);
        Unbind();
    }

    public unsafe void Draw()
    {
        Bind();
        _gl.DrawElements(PrimitiveType.Triangles, _numIndices, DrawElementsType.UnsignedInt, (void*) 0);
    }

    private void ReflectVertexAttributes()
    {
        var fields = typeof(TVertex).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.GetCustomAttributes(typeof(VertexAttribAttribute), false).FirstOrDefault() is VertexAttribAttribute attrib)
            {
                AddVertexAttribute(attrib.Index, attrib.Size, attrib.Type, attrib.Normalized, attrib.Stride, attrib.Offset);
            }
        }
    }

    protected override void DisposeManaged() => _gl.DeleteVertexArray(_handle);
}
