using System;
using System.Collections.Generic;

using Silk.NET.OpenGL;

using Texture = MinecraftSharp.Engine.Graphics.Textures.Texture;

namespace MinecraftSharp.Engine.Graphics;

public class Mesh<TVertex, TIndex> : IDisposable
    where TVertex : unmanaged
    where TIndex : unmanaged
{
    private readonly VertexArrayObject<TVertex, TIndex> _vao;
    private readonly BufferObject<TVertex> _vbo;
    private readonly BufferObject<TIndex> _ebo;
    private readonly List<Texture> _textures;

    public Mesh(GL gl, BufferObject<TVertex> vbo, BufferObject<TIndex> ebo, IEnumerable<Texture> textures, Action<VertexArrayObject<TVertex, TIndex>>? onVaoCreation = null)
    {
        _vbo = vbo;
        _ebo = ebo;
        _vao = new VertexArrayObject<TVertex, TIndex>(gl, _vbo, _ebo);
        _textures = [.. textures];

        onVaoCreation?.Invoke(_vao);
    }

    public Mesh(GL gl, IEnumerable<TVertex> vertices, IEnumerable<TIndex> indices, IEnumerable<Texture> textures, Action<VertexArrayObject<TVertex, TIndex>>? onVaoCreation = null)
        : this(gl, new BufferObject<TVertex>(gl, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw, vertices), new BufferObject<TIndex>(gl, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, indices), textures, onVaoCreation)
    { }

    public void Draw()
    {
        // TODO: This won't work for more than one texture
        foreach (Texture texture in _textures)
        {
            texture.Use();
        }

        _vao.Draw();
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }
    }
}