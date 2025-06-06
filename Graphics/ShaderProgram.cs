using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;

namespace MinecraftSharp.Graphics;

internal class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations;
    private bool _disposed;

    public ShaderProgram(GL gl, string vertexShaderSource, string fragmentShaderSource)
    {
        _gl = gl;

        // TODO: If the need to reuse a vertex or fragment shader between multiple shader programs
        // ever arises, create IDisposable shader class(es), convert the following lines to
        // `using (Shader vert/frag = new(...)) { ... }`, and then make a separate ShaderProgram
        // constructor that has Shader parameters.
        uint vertexShaderId = _gl.CreateShader(ShaderType.VertexShader);
        CompileShader(vertexShaderId, vertexShaderSource);

        uint fragmentShaderId = _gl.CreateShader(ShaderType.FragmentShader);
        CompileShader(fragmentShaderId, fragmentShaderSource);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertexShaderId);
        _gl.AttachShader(_handle, fragmentShaderId);

        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int code);
        if (code != (int) GLEnum.True)
        {
            string infoLog = _gl.GetProgramInfoLog(_handle);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }

        _gl.GetProgram(_handle, ProgramPropertyARB.ActiveUniforms, out int numUniforms);
        _uniformLocations = [];
        for (uint i = 0; i < numUniforms; i++)
        {
            string name = _gl.GetActiveUniform(_handle, i, out _, out _);
            int location = _gl.GetUniformLocation(_handle, name);
            _uniformLocations.Add(name, location);
        }

        _gl.DetachShader(_handle, vertexShaderId);
        _gl.DetachShader(_handle, fragmentShaderId);
        _gl.DeleteShader(vertexShaderId);
        _gl.DeleteShader(fragmentShaderId);
    }

    public void Use() => _gl.UseProgram(_handle);

    public int GetUniformLocation(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int location))
            throw new ArgumentOutOfRangeException(nameof(name), name, "Nonexistent uniform name");

        return location;
    }

    public unsafe void SetUniform<T>(string name, T value)
    {
        int location = GetUniformLocation(name);

        Use();
        switch (value)
        {
            case int i:
                _gl.Uniform1(location, i);
                break;
            case float f:
                _gl.Uniform1(location, f);
                break;
            case double d:
                _gl.Uniform1(location, (float)d);
                break;
            case bool b:
                _gl.Uniform1(location, b ? 1 : 0);
                break;
            case Vector2 v:
                _gl.Uniform2(location, v);
                break;
            case Vector3 v:
                _gl.Uniform3(location, v);
                break;
            case Vector4 v:
                _gl.Uniform4(location, v);
                break;
            case Color c:
                _gl.Uniform4(location, c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
                break;
            //case Matrix3X3 m:
            //    _gl.UniformMatrix3(location, false, ref m);
            //    break;
            case Matrix4x4 m:
                _gl.UniformMatrix4(location, 1, false, (float*) &m);
                break;
            default:
                throw new ArgumentException($"Unsupported uniform type: {typeof(T).Name}");
        }
    }

    private void CompileShader(uint shaderId, string shaderSource)
    {
        _gl.ShaderSource(shaderId, shaderSource);
        _gl.CompileShader(shaderId);
        _gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int success);
        if (success == (int) GLEnum.False)
        {
            string infoLog = _gl.GetShaderInfoLog(shaderId);
            throw new Exception($"Shader compilation failed: {infoLog}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _gl.DeleteProgram(_handle);
            }

            _disposed = true;
        }
    }

    ~ShaderProgram()
    {
        if (!_disposed)
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
