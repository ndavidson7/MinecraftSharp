using Silk.NET.OpenGL;

namespace MinecraftSharp.Graphics;

internal class Mesh<TVertex, TIndex> : IDisposable
    where TVertex : unmanaged
    where TIndex : unmanaged
{
    private readonly VertexArrayObject<TVertex, TIndex> _vao;
    private readonly BufferObject<TVertex> _vbo;
    private readonly BufferObject<TIndex> _ebo;
    private readonly List<Texture2D> _textures = [];

    public Mesh(GL gl, BufferObject<TVertex> vbo, BufferObject<TIndex> ebo, List<Texture2D> textures, Action<VertexArrayObject<TVertex, TIndex>>? onVaoCreation = null)
    {
        _vbo = vbo;
        _ebo = ebo;
        _vao = new VertexArrayObject<TVertex, TIndex>(gl, _vbo, _ebo);
        _textures = textures;

        onVaoCreation?.Invoke(_vao);
    }

    public Mesh(GL gl, TVertex[] vertices, TIndex[] indices, List<Texture2D> textures) : this(gl, new BufferObject<TVertex>(gl, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw, vertices), new BufferObject<TIndex>(gl, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, indices), textures)
    { }

    public void Draw(ShaderProgram shader)
    {
        // TODO: De-hardcode this
        foreach (Texture2D texture in _textures)
        {
            texture.Use();
        }

        shader.SetUniform("material.diffuse", 0); // texture slot 0
        shader.SetUniform("material.specular", 1); // texture slot 1

        _vao.Draw();
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
        foreach (Texture2D texture in _textures)
        {
            texture.Dispose();
        }
    }
}
