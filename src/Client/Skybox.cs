using System;
using System.Numerics;

using MinecraftSharp.Engine;
using MinecraftSharp.Engine.Graphics;
using MinecraftSharp.Engine.Graphics.Textures;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Client;

internal class Skybox : IDisposable
{
    private static readonly float[] Vertices =
    [
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
    ];

    private static readonly uint[] Indices =
    [
        // Back face (viewed from inside)
        0, 1, 2, 0, 2, 3,
        // Front face (viewed from inside)
        4, 6, 5, 4, 7, 6,
        // Left face
        4, 5, 1, 1, 0, 4,
        // Right face
        3, 2, 6, 6, 7, 3,
        // Top face
        4, 0, 3, 3, 7, 4,
        // Bottom face
        1, 5, 6, 6, 2, 1
    ];

    private static readonly EmbeddedResource VertexShader = new("MinecraftSharp.Client.Content.Shaders.skybox_vertex.glsl");
    private static readonly EmbeddedResource FragmentShader = new("MinecraftSharp.Client.Content.Shaders.skybox_fragment.glsl");

    private readonly GL _gl;
    private readonly Mesh<float, uint> _mesh;
    private readonly ShaderProgram _shader;
    private bool _disposed;

    /// <summary>
    /// Creates a skybox with the specified <see cref="TextureCubemap"/>.
    /// </summary>
    /// <param name="gl">The OpenGL context</param>
    public Skybox(GL gl, TextureCubemap cubemap)
    {
        _gl = gl;

        _mesh = new Mesh<float, uint>(
            gl,
            Vertices,
            Indices,
            [cubemap],
            vao => vao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0)
        );

        _shader = new(gl, VertexShader.GetStringContents(), FragmentShader.GetStringContents());
        _shader.SetUniform("skybox", 0);
    }

    public void Draw(in Matrix4x4 view, in Matrix4x4 projection)
    {
        int oldDepthFunc = _gl.GetInteger(GetPName.DepthFunc);

        _gl.DepthFunc(DepthFunction.Lequal);

        _shader.Use();
        Matrix4x4 skyboxView = view with { M41 = 0, M42 = 0, M43 = 0 }; // remove translation, keeping only rotation
        _shader.SetUniform("view", skyboxView);
        _shader.SetUniform("projection", projection);
        _mesh.Draw();

        _gl.DepthFunc((DepthFunction)oldDepthFunc); // restore original depth function
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _shader.Dispose();
                _mesh.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}