using ImGuiNET;
using MinecraftSharp.Graphics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using MinecraftSharp.Graphics.Textures;

namespace MinecraftSharp;

internal class Game : IDisposable
{
    private readonly IWindow _window;
    private bool _isWindowFocused = true;
    private GL? _gl;
    private Camera? _camera;
    private InputManager? _inputManager;
    private ImGuiController? _imGuiController;

    private string? _openGlVersion;

    private ShaderProgram? _blockShader;
    private Mesh<Vertex, uint>? _blockMesh;
    private float _blockShininess = 32;

    // generate array of block coordinates for a 10x10x1 flat plane of blocks centered at the origin
    private Vector3[] _blockCoordinates =
        [.. Enumerable.Range(-4, 9)
            .SelectMany(x => Enumerable.Range(-4, 9)
            .Select(z => new Vector3(x, 0, z)))];

    private ShaderProgram? _sunShader;
    private VertexArrayObject<Vertex, uint>? _sunVao;
    private Vector3 _sunOrbit = new(10, 10, 0);
    private float _sunOrbitSpeed = 0.2f;
    private float _sunAngle = 0f;
    private Vector4 _sunAmbient = new(51 / 255f, 51 / 255f, 51 / 255f, 1);
    private Vector4 _sunDiffuse = new(128 / 255f, 128 / 255f, 128 / 255f, 1);
    private Vector4 _sunSpecular = new(255 / 255f, 255 / 255f, 255 / 255f, 1);

    private Mesh<float, uint>? _skyboxMesh;
    private ShaderProgram? _skyboxShader;
    
    private bool _wireframeMode;

    private bool _isFpsCapped = true;
    private float _targetFps = 60f;

    private bool _disposed;

    public Game(WindowOptions options)
    {
        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Closing += OnClose;
        _window.FocusChanged += OnFocusChanged;
    }

    public void Run()
    {
        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred during game execution: {ex}");
            throw;
        }
        finally
        {
            _window.Dispose();
        }
    }

    private unsafe void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _camera = new(new(0, 0, 5), _window.Size.X / (float)_window.Size.Y);
        IInputContext input = _window.CreateInput();
        _inputManager = new(input, _window, _camera!);
        _imGuiController = new(_gl, _window, input);

        _openGlVersion = Marshal.PtrToStringAnsi((nint)_gl.GetString(StringName.Version)) ?? "Unknown";

        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);

        Vertex[] vertices =
        [
            // Front face (+Z)
            new(new Vector3(-0.5f, -0.5f,  0.5f),   new Vector3(0, 0, 1),   new Vector2(0.0f, 0.0f)), // 0
            new(new Vector3( 0.5f, -0.5f,  0.5f),   new Vector3(0, 0, 1),   new Vector2(1.0f, 0.0f)), // 1
            new(new Vector3( 0.5f,  0.5f,  0.5f),   new Vector3(0, 0, 1),   new Vector2(1.0f, 1.0f)), // 2
            new(new Vector3(-0.5f,  0.5f,  0.5f),   new Vector3(0, 0, 1),   new Vector2(0.0f, 1.0f)), // 3

            // Back face (-Z)
            new(new Vector3( 0.5f, -0.5f, -0.5f),   new Vector3(0, 0, -1),  new Vector2(0.0f, 0.0f)), // 4
            new(new Vector3(-0.5f, -0.5f, -0.5f),   new Vector3(0, 0, -1),  new Vector2(1.0f, 0.0f)), // 5
            new(new Vector3(-0.5f,  0.5f, -0.5f),   new Vector3(0, 0, -1),  new Vector2(1.0f, 1.0f)), // 6
            new(new Vector3( 0.5f,  0.5f, -0.5f),   new Vector3(0, 0, -1),  new Vector2(0.0f, 1.0f)), // 7

            // Left face (-X)
            new(new Vector3(-0.5f, -0.5f, -0.5f),   new Vector3(-1, 0, 0),  new Vector2(0.0f, 0.0f)), // 8
            new(new Vector3(-0.5f, -0.5f,  0.5f),   new Vector3(-1, 0, 0),  new Vector2(1.0f, 0.0f)), // 9
            new(new Vector3(-0.5f,  0.5f,  0.5f),   new Vector3(-1, 0, 0),  new Vector2(1.0f, 1.0f)), // 10
            new(new Vector3(-0.5f,  0.5f, -0.5f),   new Vector3(-1, 0, 0),  new Vector2(0.0f, 1.0f)), // 11

            // Right face (+X)
            new(new Vector3( 0.5f, -0.5f,  0.5f),   new Vector3(1, 0, 0),   new Vector2(0.0f, 0.0f)), // 12
            new(new Vector3( 0.5f, -0.5f, -0.5f),   new Vector3(1, 0, 0),   new Vector2(1.0f, 0.0f)), // 13
            new(new Vector3( 0.5f,  0.5f, -0.5f),   new Vector3(1, 0, 0),   new Vector2(1.0f, 1.0f)), // 14
            new(new Vector3( 0.5f,  0.5f,  0.5f),   new Vector3(1, 0, 0),   new Vector2(0.0f, 1.0f)), // 15

            // Top face (+Y)
            new(new Vector3(-0.5f,  0.5f,  0.5f),   new Vector3(0, 1, 0),   new Vector2(0.0f, 0.0f)), // 16
            new(new Vector3( 0.5f,  0.5f,  0.5f),   new Vector3(0, 1, 0),   new Vector2(1.0f, 0.0f)), // 17
            new(new Vector3( 0.5f,  0.5f, -0.5f),   new Vector3(0, 1, 0),   new Vector2(1.0f, 1.0f)), // 18
            new(new Vector3(-0.5f,  0.5f, -0.5f),   new Vector3(0, 1, 0),   new Vector2(0.0f, 1.0f)), // 19

            // Bottom face (-Y)
            new(new Vector3(-0.5f, -0.5f, -0.5f),   new Vector3(0, -1, 0),  new Vector2(0.0f, 0.0f)), // 20
            new(new Vector3( 0.5f, -0.5f, -0.5f),   new Vector3(0, -1, 0),  new Vector2(1.0f, 0.0f)), // 21
            new(new Vector3( 0.5f, -0.5f,  0.5f),   new Vector3(0, -1, 0),  new Vector2(1.0f, 1.0f)), // 22
            new(new Vector3(-0.5f, -0.5f,  0.5f),   new Vector3(0, -1, 0),  new Vector2(0.0f, 1.0f)), // 23
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

        BufferObject<Vertex> vbo = new(_gl, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw, vertices);
        BufferObject<uint> ebo = new(_gl, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, indices);

        List<Texture2D> textures =
        [
            new(_gl, Path.Combine("Content", "Textures", "grass_block_side.png")), // diffuse map
            //new(_gl, Path.Combine("Content", "Textures", "container2_specular.png"), TextureUnit.Texture1) // specular map
        ];

        _blockMesh = new(_gl, vbo, ebo, textures);
        _blockShader = new(_gl, File.ReadAllText(Path.Combine("Content", "Shaders", "simple_block_shader_vertex.glsl")), File.ReadAllText(Path.Combine("Content", "Shaders", "simple_block_shader_fragment.glsl")));
        _blockShader.SetUniform("material.diffuse", 0); // texture slot 0
        _blockShader.SetUniform("material.specular", 1); // texture slot 1
        
        _sunVao = new(_gl, vbo, ebo);
        _sunShader = new(_gl, File.ReadAllText(Path.Combine("Content", "Shaders", "simple_sun_shader_vertex.glsl")), File.ReadAllText(Path.Combine("Content", "Shaders", "simple_sun_shader_fragment.glsl")));

        float[] skyboxVertices =
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

        uint[] skyboxIndices =
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

        TextureCubemap skyboxTexture = new(_gl, 
        [
                Path.Combine("Content", "Textures", "skybox2", "right.jpg"),
                Path.Combine("Content", "Textures", "skybox2", "left.jpg"),
                Path.Combine("Content", "Textures", "skybox2", "top.jpg"),
                Path.Combine("Content", "Textures", "skybox2", "bottom.jpg"),
                Path.Combine("Content", "Textures", "skybox2", "front.jpg"),
                Path.Combine("Content", "Textures", "skybox2", "back.jpg")
            ]);

        _skyboxMesh = new(_gl, skyboxVertices, skyboxIndices, [skyboxTexture], vao =>
        {
            vao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        });
            
        _skyboxShader = new(_gl, File.ReadAllText(Path.Combine("Content", "Shaders", "skybox_vertex.glsl")),
            File.ReadAllText(Path.Combine("Content", "Shaders", "skybox_fragment.glsl")));
        _skyboxShader.SetUniform("skybox", 0);
        
        _window.Center();
        _window.IsVisible = true;
    }

    private void OnRender(double deltaTime)
    {
        _imGuiController?.Update((float)deltaTime);

        _gl?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4x4 view = _camera!.ViewMatrix;
        Matrix4x4 projection = _camera!.ProjectionMatrix;

        // get uniforms for sun
        _sunAngle += (float)deltaTime * _sunOrbitSpeed;
        Vector3 sunPosition = new(
            _sunOrbit.X * MathF.Sin(_sunAngle), // X: east-west
            _sunOrbit.Y * MathF.Cos(_sunAngle), // Y: height in sky
            0f                                 // Z: no tilt
        );
        Vector3 sunDirection = Vector3.Normalize(-sunPosition);
        Matrix4x4 sunModel = Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(sunPosition);
        Matrix4x4 sunMvp = sunModel * view * projection;

        // enable wireframe mode if set
        _gl!.PolygonMode(TriangleFace.FrontAndBack, _wireframeMode ? PolygonMode.Line : PolygonMode.Fill);

        // draw sun
        _sunShader!.Use();
        _sunShader.SetUniform("mvpMatrix", sunMvp);
        _sunShader.SetUniform("color", _sunSpecular);
        _sunVao!.Draw();

        // draw block
        _blockShader!.Use();
        _blockShader.SetUniform("material.shininess", _blockShininess);
        _blockShader.SetUniform("sun.direction", sunDirection);
        _blockShader.SetUniform("sun.ambient", _sunAmbient);
        _blockShader.SetUniform("sun.diffuse", _sunDiffuse);
        _blockShader.SetUniform("sun.specular", _sunSpecular);
        _blockShader.SetUniform("viewPosition", _camera.Position);

        foreach (Vector3 coordinates in _blockCoordinates)
        {
            // get block-specific uniforms
            Matrix4x4 blockModel = Matrix4x4.CreateTranslation(coordinates);
            Matrix4x4 blockMvp = blockModel * view * projection;
            Matrix4x4.Invert(blockModel, out Matrix4x4 transInvModel);
            transInvModel = Matrix4x4.Transpose(transInvModel);

            _blockShader.SetUniform("modelMatrix", blockModel);
            _blockShader.SetUniform("transInvModelMatrix", transInvModel);
            _blockShader.SetUniform("mvpMatrix", blockMvp);
            
            _blockMesh?.Draw(_blockShader);
        }
        
        // draw skybox
        _gl.DepthFunc(DepthFunction.Lequal);
        _skyboxShader!.Use();
        Matrix4x4 skyboxView = view with { M41 = 0, M42 = 0, M43 = 0 };
        _skyboxShader.SetUniform("view", skyboxView);
        _skyboxShader.SetUniform("projection", projection);
        _skyboxMesh?.Draw(_skyboxShader);
        // reset depth function
        _gl.DepthFunc(DepthFunction.Less);

        ImGui.ShowDemoWindow();
        ShowDebugMenu(deltaTime);

        _imGuiController?.Render();
    }

    private void OnUpdate(double deltaTime)
    {
        if (!_isWindowFocused)
            return;

        _inputManager?.Update((float)deltaTime);

        _window.FramesPerSecond = _isFpsCapped switch
        {
            false when _window.FramesPerSecond != 0 => 0,
            true when _targetFps != _window.FramesPerSecond => _targetFps,
            _ => _window.FramesPerSecond
        };
    }

    private void OnFramebufferResize(Vector2D<int> newSize)
    {
        _gl?.Viewport(newSize);

        if (_camera is not null)
            _camera.AspectRatio = newSize.X / (float)newSize.Y;
    }

    private void OnFocusChanged(bool focused)
    {
        _isWindowFocused = focused;
    }

    private void OnClose() => Dispose();

    private void ShowDebugMenu(double deltaTime)
    {
        if (!ImGui.Begin("Debug Menu"))
        {
            ImGui.End();
            return;
        }

        const float MinShininess = 0, MaxShininess = 100;
        if (ImGui.CollapsingHeader("Block"))
        {
            ImGui.SliderFloat("Shininess", ref _blockShininess, MinShininess, MaxShininess);
        }

        if (ImGui.CollapsingHeader("Sun"))
        {
            ImGui.SliderFloat3("Orbit", ref _sunOrbit, -20f, 20f);
            ImGui.SliderFloat("Orbit Speed", ref _sunOrbitSpeed, 0f, 1f);
            ImGui.ColorEdit4("Ambient Color", ref _sunAmbient);
            ImGui.ColorEdit4("Diffuse Color", ref _sunDiffuse);
            ImGui.ColorEdit4("Specular Color", ref _sunSpecular);
        }

        if (ImGui.CollapsingHeader("Camera"))
        {
            ImGui.Text($"Position: {_camera?.Position}");
        }

        if (ImGui.CollapsingHeader("Rendering"))
        {
            ImGui.Text($"OpenGL Version: {_openGlVersion}");
            ImGui.Text($"Viewport Size: {_window.Size}");
            ImGui.Text($"FPS: {Math.Round(1 / deltaTime)}");
            ImGui.Checkbox("Wireframe Mode", ref _wireframeMode);
        }

        const float MinFps = 30, MaxFps = 240;
        if (ImGui.CollapsingHeader("Settings"))
        {
            ImGui.Checkbox("Cap FPS", ref _isFpsCapped);
            if (!_isFpsCapped)
                ImGui.BeginDisabled();
            ImGui.SliderFloat("FPS Limit", ref _targetFps, MinFps, MaxFps);
            if (!_isFpsCapped)
                ImGui.EndDisabled();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _window.Load -= OnLoad;
                _window.Update -= OnUpdate;
                _window.Render -= OnRender;
                _window.FramebufferResize -= OnFramebufferResize;
                _window.Closing -= OnClose;
                _window.FocusChanged -= OnFocusChanged;

                _imGuiController?.Dispose();
                _inputManager?.Dispose();
                _blockMesh?.Dispose();
                _sunVao?.Dispose();
                _blockShader?.Dispose();
                _sunShader?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
