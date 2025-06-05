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
    private Color _blockAmbient = Color.FromArgb(255, 255, 128, 79);
    private Color _blockDiffuse = Color.FromArgb(255, 255, 128, 79);
    private Color _blockSpecular = Color.FromArgb(255, 128, 128, 128);
    private float _blockShininess = 32;

    private ShaderProgram? _lightShader;
    private VertexArrayObject<float, uint>? _lightVao;
    private Color _lightAmbient = Color.FromArgb(255, 51, 51, 51);
    private Color _lightDiffuse = Color.FromArgb(255, 128, 128, 128);
    private Color _lightSpecular = Color.FromArgb(255, 255, 255, 255);
    private readonly Vector4 _lightOrbit = new(3, 3, 3, 1);

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
        _gl.Enable(GLEnum.DepthTest);

        float[] vertices =
        [
            // positions            normals
            -0.5f, -0.5f, -0.5f,    0.0f,  0.0f, -1.0f,
            0.5f, -0.5f, -0.5f,     0.0f,  0.0f, -1.0f,
            0.5f,  0.5f, -0.5f,     0.0f,  0.0f, -1.0f,
            -0.5f,  0.5f, -0.5f,    0.0f,  0.0f, -1.0f,

            -0.5f, -0.5f,  0.5f,    0.0f,  0.0f,  1.0f,
            0.5f, -0.5f,  0.5f,     0.0f,  0.0f,  1.0f,
            0.5f,  0.5f,  0.5f,     0.0f,  0.0f,  1.0f,
            -0.5f,  0.5f,  0.5f,    0.0f,  0.0f,  1.0f,

            -0.5f,  0.5f,  0.5f,    -1.0f,  0.0f,  0.0f,
            -0.5f,  0.5f, -0.5f,    -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,    -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f,  0.5f,    -1.0f,  0.0f,  0.0f,

            0.5f,  0.5f,  0.5f,     1.0f,  0.0f,  0.0f,
            0.5f,  0.5f, -0.5f,     1.0f,  0.0f,  0.0f,
            0.5f, -0.5f, -0.5f,     1.0f,  0.0f,  0.0f,
            0.5f, -0.5f,  0.5f,     1.0f,  0.0f,  0.0f,

            -0.5f, -0.5f, -0.5f,    0.0f, -1.0f,  0.0f,
            0.5f, -0.5f, -0.5f,     0.0f, -1.0f,  0.0f,
            0.5f, -0.5f,  0.5f,     0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f,  0.5f,    0.0f, -1.0f,  0.0f,

            -0.5f,  0.5f, -0.5f,    0.0f,  1.0f,  0.0f,
            0.5f,  0.5f, -0.5f,     0.0f,  1.0f,  0.0f,
            0.5f,  0.5f,  0.5f,     0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f,  0.5f,    0.0f,  1.0f,  0.0f,
        ];

        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0,

            4, 5, 6,
            6, 7, 4,

            8, 9, 10,
            10, 11, 8,

            12, 13, 14,
            14, 15, 12,

            16, 17, 18,
            18, 19, 16,

            20, 21, 22,
            22, 23, 20,
        ];


        BufferObject<float> vbo = new(_gl, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw, vertices);
        BufferObject<uint> ebo = new(_gl, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, indices);

        _blockVao = new(_gl, vbo, ebo);
        _blockVao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 6, 0);
        _blockVao.AddVertexAttribute(1, 3, VertexAttribPointerType.Float, false, 6, 3);

        _lightVao = new(_gl, vbo, ebo);
        _lightVao.AddVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 6, 0);

        _camera = new(new(0, 0, 5), _window.Size.X / (float)_window.Size.Y);
        _inputManager = new(_window.CreateInput(), _window, _camera!);

        //_blockShader = new(_gl, File.ReadAllText("Content\\simple_block_shader_vertex.glsl"), File.ReadAllText("Content\\simple_block_shader_fragment.glsl"));
        _blockShader = new(_gl, File.ReadAllText(Path.Combine("Content", "simple_block_shader_vertex.glsl")), """
            #version 410 core
            in vec3 fragPosition;
            in vec3 worldNormal;

            uniform vec3 objectColor;
            uniform vec3 lightColor;
            uniform vec3 lightPosition;
            uniform vec3 viewPosition;

            out vec4 FragColor;

            void main()
            {
                  float ambientStrength = 0.1;
                  vec3 ambient = ambientStrength * lightColor;

                  vec3 lightDirection = normalize(lightPosition - fragPosition);
                  float diff = max(dot(worldNormal, lightDirection), 0.0);
                  vec3 diffuse = diff * lightColor;

                  float specularStrength = 0.5;
                  vec3 viewDirection = normalize(viewPosition - fragPosition);
                  vec3 reflectDirection = reflect(-lightDirection, worldNormal);
                  float spec = pow(max(dot(viewDirection, reflectDirection), 0.0), 32);
                  vec3 specular = specularStrength * spec * lightColor;

                  //The resulting colour should be the amount of ambient colour + the amount of additional colour provided by the diffuse of the lamp + the specular amount
                  vec3 result = (ambient + diffuse + specular) * objectColor;

                  FragColor = vec4(result, 1.0);
            }
            """);
        _lightShader = new(_gl, """
            #version 410 core

            layout (location = 0) in vec3 position;

            uniform mat4 mvpMatrix;

            void main()
            {
                gl_Position = mvpMatrix * vec4(position, 1.0);
            }
            """,
            """
            #version 410 core

            out vec4 fragColor;

            uniform vec4 color;

            void main()
            {
                fragColor = color;
            }
            """);
        
        _window.Center();
        _window.IsVisible = true;
    }

    private void OnRender(double deltaTime)
    {
        _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        float totalElapsedSeconds = (float)_window.Time;

        // get uniforms for light source
        float lightX = _lightOrbit.X * MathF.Sin(totalElapsedSeconds);
        float lightY = _lightOrbit.Y * -MathF.Sin(totalElapsedSeconds);
        float lightZ = _lightOrbit.Z * MathF.Cos(totalElapsedSeconds);
        Vector3 lightPosition = new(lightX, lightY, lightZ);
        Matrix4x4 model = Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(lightPosition);
        Matrix4x4 view = _camera!.GetViewMatrix();
        Matrix4x4 projection = _camera!.GetProjectionMatrix();
        Matrix4x4 mvp = model * view * projection;

        // draw light source
        _lightShader!.Use();
        _lightShader.SetUniform("mvpMatrix", mvp);
        _lightShader.SetUniform("color", _lightSpecular);
        _lightVao!.Draw();

        // get uniforms for block
        model = Matrix4x4.Identity;
        //model = Matrix4.CreateTranslation(1, 0, 0);
        mvp = model * view * projection;
        Matrix4x4.Invert(Matrix4x4.Transpose(model), out Matrix4x4 transInvModel);
        //Matrix4x4.Invert(model, out Matrix4x4 transInvModel);
        //transInvModel = Matrix4x4.Transpose(transInvModel);

        // draw block
        _blockShader!.Use();
        //_blockShader.SetUniform("modelMatrix", model);
        //_blockShader.SetUniform("transInvModelMatrix", transInvModel);
        //_blockShader.SetUniform("mvpMatrix", mvp);
        //_blockShader.SetUniform("material.ambient", _blockAmbient);
        //_blockShader.SetUniform("material.diffuse", _blockDiffuse);
        //_blockShader.SetUniform("material.specular", _blockSpecular);
        //_blockShader.SetUniform("material.shininess", _blockShininess);
        //_blockShader.SetUniform("light.position", lightPosition);
        //_blockShader.SetUniform("light.ambient", _lightAmbient);
        //_blockShader.SetUniform("light.diffuse", _lightDiffuse);
        //_blockShader.SetUniform("light.specular", _lightSpecular);
        //_blockShader.SetUniform("viewPosition", _camera.Position);
        _blockShader.SetUniform("modelMatrix", model);
        _blockShader.SetUniform("transInvModelMatrix", transInvModel);
        _blockShader.SetUniform("mvpMatrix", mvp);
        _blockShader.SetUniform("objectColor", new Vector3(1, 0, 0));
        _blockShader.SetUniform("lightColor", new Vector3(0, 0, 0));
        _blockShader.SetUniform("lightPosition", lightPosition);
        _blockShader.SetUniform("viewPosition", _camera.Position);
        _blockVao!.Draw();

        //SwapBuffers();
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
        _lightVao?.Dispose();
        _blockShader?.Dispose();
        _lightShader?.Dispose();
    }
}
