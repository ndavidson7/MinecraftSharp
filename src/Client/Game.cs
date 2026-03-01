using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImGuiNET;

using LiteNetLib;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MinecraftSharp.Client.Configuration;
using MinecraftSharp.Engine;
using MinecraftSharp.Engine.Graphics;
using MinecraftSharp.Engine.Graphics.Textures;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace MinecraftSharp.Client;

internal partial class Game : IDisposable
{
    private readonly Settings _settings;
    private readonly ILogger<Game> _logger;
    private readonly IWindow _window;
    private bool _isWindowFocused = true;
    private GL? _gl;
    private Camera? _camera;
    private InputManager? _inputManager;
    private ImGuiController? _imGuiController;
    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _client;

    private string? _openGlVersion;

    private ShaderProgram? _blockShader;
    private Mesh<Vertex, uint>? _blockMesh;

    // generate array of block coordinates for a 10x10x1 flat plane of blocks centered at the origin
    private readonly Vector3[] _blockCoordinates =
        [.. Enumerable.Range(-4, 9)
            .SelectMany(x => Enumerable.Range(-4, 9)
            .Select(z => new Vector3(x, 0, z)))];

    private Skybox? _skybox;

    private bool _wireframeMode;

    private bool _isFpsCapped = true;
    private float _targetFps = 240f;

    private bool _disposed;

    public Game(IOptions<Settings> settings, ILogger<Game> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // TODO: Serialize and deserialize settings
        _window = Window.Create(WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = "MinecraftSharp",
#if DEBUG
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug | ContextFlags.ForwardCompatible, new APIVersion(4, 1)),
#else
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 1)),
#endif
            IsVisible = false,
            FramesPerSecond = _settings.MaxFps,
            VSync = false,
            Samples = 4, // Enable 4x MSAA
        });

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Closing += OnClose;
        _window.FocusChanged += OnFocusChanged;

        _listener.NetworkReceiveEvent += OnNetworkReceive;
        _client = new(_listener);
    }

    public void Run()
    {
        _logger.LogInformation("Starting game");

        try
        {
            _window.Run();
        }
        finally
        {
            _window.Dispose();
        }

        _logger.LogInformation("Closing game");
    }

    private unsafe void OnLoad()
    {
        TraceMethodProgress(LogLevel.Debug, "Started");

        _gl = _window.CreateOpenGL();
        _camera = new(new(0, 0, 5), _window.Size.X / (float)_window.Size.Y);
        IInputContext input = _window.CreateInput();
        _inputManager = new(input, _window, _camera!);
        _imGuiController = new(_gl, _window, input);

        _openGlVersion = Marshal.PtrToStringAnsi((nint)_gl.GetString(StringName.Version)) ?? "Unknown";

        if ((_gl.GetInteger(GetPName.ContextFlags) & (int)GLEnum.ContextFlagDebugBit) != 0)
        {
            _gl.Enable(EnableCap.DebugOutput);
            _gl.Enable(EnableCap.DebugOutputSynchronous);
            _gl.DebugMessageCallback(OnGLDebugMessage, null);
            _gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, [], true);
        }

        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        _gl.Enable(EnableCap.Multisample);
        _gl.Enable(EnableCap.TextureCubeMapSeamless);

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
            new(_gl, new EmbeddedResource("MinecraftSharp.Client.Content.Textures.grass_block_side.png")), // diffuse map
        ];

        _blockMesh = new(_gl, vbo, ebo, textures);
        _blockShader = new(_gl,
            new EmbeddedResource("MinecraftSharp.Client.Content.Shaders.block_texture_vertex.glsl").GetStringContents(),
            new EmbeddedResource("MinecraftSharp.Client.Content.Shaders.block_texture_fragment.glsl").GetStringContents());
        _blockShader.SetUniform("textureSampler", 0);

        _skybox = new(_gl, new TextureCubemap(_gl,
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.x_pos.png"), CubemapFaceDirection.PositiveX),
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.x_neg.png"), CubemapFaceDirection.NegativeX),
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.y_pos.png"), CubemapFaceDirection.PositiveY),
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.y_neg.png"), CubemapFaceDirection.NegativeY),
                //new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.z_pos.png"), CubemapFaceDirection.PositiveZ),
                //new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.z_neg.png"), CubemapFaceDirection.NegativeZ)
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.z_pos.png"), CubemapFaceDirection.NegativeZ),
                new CubemapFace(new EmbeddedResource("MinecraftSharp.Client.Content.Textures.Skyboxes.Debug.z_neg.png"), CubemapFaceDirection.PositiveZ)
            ));

        _window.Center();
        _window.IsVisible = true;

        _client.Start();
        _client.Connect("localhost", 25565, "SomeConnectionKey");

        TraceMethodProgress(LogLevel.Debug, "Completed");
    }

    private void OnRender(double deltaTime)
    {
        TraceMethodProgress(LogLevel.Trace, "Started");

        _imGuiController?.Update((float)deltaTime);

        _gl?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // enable wireframe mode if set
        _gl!.PolygonMode(TriangleFace.FrontAndBack, _wireframeMode ? PolygonMode.Line : PolygonMode.Fill);

        Matrix4x4 view = _camera!.ViewMatrix;
        Matrix4x4 projection = _camera!.ProjectionMatrix;

        // draw block
        _blockShader!.Use();

        foreach (Vector3 coordinates in _blockCoordinates)
        {
            _blockShader.SetUniform("mvpMatrix", Matrix4x4.CreateTranslation(coordinates) * view * projection);
            _blockMesh?.Draw();
        }

        // draw skybox
        _skybox?.Draw(in view, in projection);

        ImGui.ShowDemoWindow();
        ShowDebugMenu(deltaTime);

        _imGuiController?.Render();

        TraceMethodProgress(LogLevel.Trace, "Completed");
    }

    private void OnUpdate(double deltaTime)
    {
        TraceMethodProgress(LogLevel.Trace, "Started");

        _client.PollEvents();

        if (_isWindowFocused)
        {
            _inputManager?.Update((float)deltaTime);

            _window.FramesPerSecond = _isFpsCapped switch
            {
                false when _window.FramesPerSecond != 0 => 0,
                true when _targetFps != _window.FramesPerSecond => _targetFps,
                _ => _window.FramesPerSecond
            };
        }

        TraceMethodProgress(LogLevel.Trace, "Completed");
    }

    private void OnFramebufferResize(Vector2D<int> newSize)
    {
        TraceMethodProgress(LogLevel.Trace, "Started");

        _gl?.Viewport(newSize);

        _camera?.AspectRatio = newSize.X / (float)newSize.Y;

        TraceMethodProgress(LogLevel.Trace, "Completed");
    }

    private void OnFocusChanged(bool focused)
    {
        _isWindowFocused = focused;
    }

    private void OnClose()
    {
        TraceMethodProgress(LogLevel.Debug, "Started");

        _client.Stop();
        Dispose();

        TraceMethodProgress(LogLevel.Debug, "Completed");
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        _logger.LogInformation("Received message: {Message}", reader.GetString(100));
        reader.Recycle();
    }

    private void ShowDebugMenu(double deltaTime)
    {
        if (!ImGui.Begin("Debug Menu"))
        {
            ImGui.End();
            return;
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

        const float minFps = 30, maxFps = 240;
        if (ImGui.CollapsingHeader("Settings"))
        {
            ImGui.Checkbox("Cap FPS", ref _isFpsCapped);
            if (!_isFpsCapped)
                ImGui.BeginDisabled();
            ImGui.SliderFloat("FPS Limit", ref _targetFps, minFps, maxFps);
            if (!_isFpsCapped)
                ImGui.EndDisabled();
        }
    }

    private void OnGLDebugMessage(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        string messageString = Marshal.PtrToStringAnsi(message) ?? "";

        LogLevel level = severity switch
        {
            GLEnum.DebugSeverityHigh => LogLevel.Error,
            GLEnum.DebugSeverityMedium => LogLevel.Warning,
            GLEnum.DebugSeverityLow => LogLevel.Information,
            GLEnum.DebugSeverityNotification => LogLevel.Trace,
            _ => LogLevel.Information
        };

        LogGLDebugMessage(level, source, type, id, messageString);
    }

    [LoggerMessage(Message = "{Phase} execution of method {Method}")]
    private partial void TraceMethodProgress(LogLevel level, string phase, [CallerMemberName] string? method = null);

    [LoggerMessage(Message = "OpenGL Debug Message: {Source}, {Type}, {ID}, {Message}")]
    private partial void LogGLDebugMessage(LogLevel level, GLEnum source, GLEnum type, int id, string message);

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
                _blockShader?.Dispose();
                _skybox?.Dispose();
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