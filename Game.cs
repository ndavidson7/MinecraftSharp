using MinecraftSharp.Graphics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace MinecraftSharp;

internal class Game : IDisposable
{
    private readonly IWindow _window;
    private bool _isWindowFocused = true;
    private GL? _gl;
    private Camera? _camera;
    private InputManager? _inputManager;

    private ShaderProgram? _blockShader;
    private VertexArrayObject<float, uint>? _blockVao;
    private Texture2D? _blockDiffuseMap;
    private Texture2D? _blockSpecularMap;
    private float _blockShininess = 32;
    
    private ShaderProgram? _sunShader;
    private VertexArrayObject<float, uint>? _sunVao;
    private Vector3 _sunOrbit = new(10, 10, 0);
    private float _sunOrbitSpeed = 0.2f;
    private Color _sunAmbient = Color.FromArgb(255, 51, 51, 51);
    private Color _sunDiffuse = Color.FromArgb(255, 128, 128, 128);
    private Color _sunSpecular = Color.FromArgb(255, 255, 255, 255);
    
    private Color _backgroundColor = Color.Black;

    public Game(WindowOptions options)
    {
        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Closing += OnClose;
        _window.FocusChanged += isFocused => _isWindowFocused = isFocused;
    }

    public void Run()
    {
        _window.Run();

        _window.Dispose();
    }

    private void OnLoad()
    {
        _gl = _window.CreateOpenGL();

        _gl.ClearColor(_backgroundColor);
        _gl.Enable(EnableCap.DepthTest);

        float[] vertices =
        [
            // positions            // normals  // texture coords
            // Front face (+Z)
            -0.5f, -0.5f,  0.5f,    0, 0, 1,    0.0f, 0.0f, // 0
             0.5f, -0.5f,  0.5f,    0, 0, 1,    1.0f, 0.0f, // 1
             0.5f,  0.5f,  0.5f,    0, 0, 1,    1.0f, 1.0f, // 2
            -0.5f,  0.5f,  0.5f,    0, 0, 1,    0.0f, 1.0f, // 3

            // Back face (-Z)
             0.5f, -0.5f, -0.5f,    0, 0, -1,   0.0f, 0.0f, // 4
            -0.5f, -0.5f, -0.5f,    0, 0, -1,   1.0f, 0.0f, // 5
            -0.5f,  0.5f, -0.5f,    0, 0, -1,   1.0f, 1.0f, // 6
             0.5f,  0.5f, -0.5f,    0, 0, -1,   0.0f, 1.0f, // 7

            // Left face (-X)
            -0.5f, -0.5f, -0.5f,    -1, 0, 0,   0.0f, 0.0f, // 8
            -0.5f, -0.5f,  0.5f,    -1, 0, 0,   1.0f, 0.0f, // 9
            -0.5f,  0.5f,  0.5f,    -1, 0, 0,   1.0f, 1.0f, // 10
            -0.5f,  0.5f, -0.5f,    -1, 0, 0,   0.0f, 1.0f, // 11

            // Right face (+X)
             0.5f, -0.5f,  0.5f,    1, 0, 0,    0.0f, 0.0f, // 12
             0.5f, -0.5f, -0.5f,    1, 0, 0,    1.0f, 0.0f, // 13
             0.5f,  0.5f, -0.5f,    1, 0, 0,    1.0f, 1.0f, // 14
             0.5f,  0.5f,  0.5f,    1, 0, 0,    0.0f, 1.0f, // 15

            // Top face (+Y)
            -0.5f,  0.5f,  0.5f,    0, 1, 0,    0.0f, 0.0f, // 16
             0.5f,  0.5f,  0.5f,    0, 1, 0,    1.0f, 0.0f, // 17
             0.5f,  0.5f, -0.5f,    0, 1, 0,    1.0f, 1.0f, // 18
            -0.5f,  0.5f, -0.5f,    0, 1, 0,    0.0f, 1.0f, // 19

            // Bottom face (-Y)
            -0.5f, -0.5f, -0.5f,    0, -1, 0,   0.0f, 0.0f, // 20
             0.5f, -0.5f, -0.5f,    0, -1, 0,   1.0f, 0.0f, // 21
             0.5f, -0.5f,  0.5f,    0, -1, 0,   1.0f, 1.0f, // 22
            -0.5f, -0.5f,  0.5f,    0, -1, 0,   0.0f, 1.0f, // 23
        ];

        uint[] indices =
        [
            // Front face
            0, 1, 2, 2, 3, 0,
            // Back face
            4, 5, 6, 6, 7, 4,
            // Left face
            8, 9, 10, 10, 11, 8,
            // Right face
            12, 13, 14, 14, 15, 12,
            // Top face
            16, 17, 18, 18, 19, 16,
            // Bottom face
            20, 21, 22, 22, 23, 20
        ];
        
        BufferObject<float> vbo = new(_gl, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw, vertices);
        BufferObject<uint> ebo = new(_gl, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, indices);

        _blockVao = new(_gl, vbo, ebo);
        _blockVao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 8, 0); // positions
        _blockVao.AddVertexAttribute(1, 3, VertexAttribPointerType.Float, false, 8, 3); // normals
        _blockVao.AddVertexAttribute(2, 2, VertexAttribPointerType.Float, false, 8, 6); // texture coords
        _blockDiffuseMap = new(_gl, Path.Combine("Content", "Textures", "container2.png"));
        _blockSpecularMap = new(_gl, Path.Combine("Content", "Textures", "container2_specular.png"), TextureUnit.Texture1);

        _sunVao = new(_gl, vbo, ebo);
        _sunVao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 8, 0); // positions

        _camera = new(new(0, 0, 5), _window.Size.X / (float)_window.Size.Y);
        _inputManager = new(_window.CreateInput(), _window, _camera!);

        _blockShader = new(_gl, File.ReadAllText(Path.Combine("Content", "simple_block_shader_vertex.glsl")), File.ReadAllText(Path.Combine("Content", "simple_block_shader_fragment.glsl")));
        _sunShader = new(_gl, File.ReadAllText(Path.Combine("Content", "simple_sun_shader_vertex.glsl")), File.ReadAllText(Path.Combine("Content", "simple_sun_shader_fragment.glsl")));
        
        _window.Center();
        _window.IsVisible = true;
    }

    private void OnRender(double deltaTime)
    {
        _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        float totalElapsedSeconds = (float)_window.Time;

        Matrix4x4 view = _camera!.ViewMatrix;
        Matrix4x4 projection = _camera!.ProjectionMatrix;

        // get uniforms for sun
        float sunAngle = totalElapsedSeconds * _sunOrbitSpeed;
        Vector3 sunPosition = new(
            _sunOrbit.X * MathF.Sin(sunAngle), // X: east-west
            _sunOrbit.Y * MathF.Cos(sunAngle), // Y: height in sky
            0f                                 // Z: no tilt
        );
        Vector3 sunDirection = Vector3.Normalize(-sunPosition);
        Matrix4x4 sunModel = Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(sunPosition);
        Matrix4x4 sunMvp = sunModel * view * projection;

        // draw sun
        _sunShader!.Use();
        _sunShader.SetUniform("mvpMatrix", sunMvp);
        _sunShader.SetUniform("color", _sunSpecular);
        _sunVao!.Draw();

        // get uniforms for block
        Matrix4x4 blockModel = Matrix4x4.Identity;
        Matrix4x4 blockMvp = blockModel * view * projection;
        Matrix4x4.Invert(Matrix4x4.Transpose(blockModel), out Matrix4x4 transInvModel);
        //Matrix4x4.Invert(blockModel, out Matrix4x4 transInvModel2);
        //transInvModel2 = Matrix4x4.Transpose(transInvModel2);

        // draw block
        _blockDiffuseMap!.Use();
        _blockSpecularMap!.Use();
        _blockShader!.Use();
        _blockShader.SetUniform("modelMatrix", blockModel);
        _blockShader.SetUniform("transInvModelMatrix", transInvModel);
        _blockShader.SetUniform("mvpMatrix", blockMvp);
        _blockShader.SetUniform("material.diffuse", 0); // texture slot 0
        _blockShader.SetUniform("material.specular", 1); // texture slot 1
        _blockShader.SetUniform("material.shininess", _blockShininess);
        _blockShader.SetUniform("sun.direction", sunDirection);
        _blockShader.SetUniform("sun.ambient", _sunAmbient);
        _blockShader.SetUniform("sun.diffuse", _sunDiffuse);
        _blockShader.SetUniform("sun.specular", _sunSpecular);
        _blockShader.SetUniform("viewPosition", _camera.Position);
        _blockVao!.Draw();
    }

    private void OnUpdate(double deltaTime)
    {
        if (!_isWindowFocused)
            return;

        _inputManager!.OnUpdate((float)deltaTime);
    }

    private void OnFramebufferResize(Vector2D<int> newSize)
    {
        _gl!.Viewport(newSize);
        _camera!.AspectRatio = newSize.X / (float)newSize.Y;
    }

    private void OnClose() => Dispose();

    public void Dispose()
    {
        _blockVao?.Dispose();
        _sunVao?.Dispose();
        _blockShader?.Dispose();
        _sunShader?.Dispose();
    }
}
