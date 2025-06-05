using MinecraftSharp.Graphics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;

namespace MinecraftSharp;

internal class InputManager
{
    private readonly IWindow _window;
    private readonly IKeyboard? _keyboard;
    private readonly Camera _camera;
    private Vector2 _lastMousePosition;

    public InputManager(IInputContext input, IWindow window, Camera camera)
    {
        _keyboard = input.Keyboards.FirstOrDefault();
        if (_keyboard is not null)
            _keyboard.KeyDown += OnKeyDown;

        _camera = camera;
        _window = window;

        foreach (var mouse in input.Mice)
        {
            mouse.Cursor.IsConfined = true;
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseWheel;
        }
    }

    public void OnUpdate(float deltaTime)
    {
        //if (input.IsKeyDown(Keys.LeftShift))

        if (_keyboard!.IsKeyPressed(Key.W))
            _camera.Position += _camera.Front * _camera.Speed * deltaTime; // Forward
        if (_keyboard.IsKeyPressed(Key.S))
            _camera.Position -= _camera.Front * _camera.Speed * deltaTime; // Backward
        if (_keyboard.IsKeyPressed(Key.A))
            _camera.Position -= _camera.Right * _camera.Speed * deltaTime; // Left
        if (_keyboard.IsKeyPressed(Key.D))
            _camera.Position += _camera.Right * _camera.Speed * deltaTime; // Right
        if (_keyboard.IsKeyPressed(Key.Space))
            _camera.Position += _camera.Up * _camera.Speed * deltaTime; // Up
        if (_keyboard.IsKeyPressed(Key.ControlLeft))
            _camera.Position -= _camera.Up * _camera.Speed * deltaTime; // Down
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (_lastMousePosition == default)
        {
            _lastMousePosition = position;
            return;
        }

        Vector2 delta = position - _lastMousePosition;
        _lastMousePosition = position;

        _camera.Yaw += delta.X * _camera.Sensitivity;
        _camera.Pitch -= delta.Y * _camera.Sensitivity;
    }

    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        _camera.FieldOfView -= scrollWheel.Y;
    }

    /// <summary>
    /// Used to respond to input that is invariant under delta time
    /// </summary>
    /// <param name="keyboard">The keyboard that fired the event</param>
    /// <param name="key">The key that was pressed</param>
    /// <param name="arg3"></param>
    private void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
            _window.Close();
    }
}
