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
        if (input.Keyboards.Count > 0)
        {
            _keyboard = input.Keyboards[0];
            _keyboard.KeyDown += OnKeyDown;
        }

        _camera = camera;
        _window = window;

        foreach (IMouse mouse in input.Mice)
        {
            mouse.Cursor.IsConfined = true;
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseWheel;
        }
    }

    public void OnUpdate(float deltaTime)
    {
        if (_keyboard is null)
            return;

        Vector3 direction = default;
        if (_keyboard.IsKeyPressed(Key.W))              direction += _camera.Front; // Forward
        if (_keyboard.IsKeyPressed(Key.S))              direction -= _camera.Front; // Backward
        if (_keyboard.IsKeyPressed(Key.A))              direction -= _camera.Right; // Left
        if (_keyboard.IsKeyPressed(Key.D))              direction += _camera.Right; // Right
        if (_keyboard.IsKeyPressed(Key.Space))          direction += _camera.Up;    // Up
        if (_keyboard.IsKeyPressed(Key.ControlLeft))    direction -= _camera.Up;    // Down

        float speed = _camera.Speed;
        if (_keyboard.IsKeyPressed(Key.ShiftLeft))
            speed *= 2f;
        
        if (direction != default)
            _camera.Position += Vector3.Normalize(direction) * speed * deltaTime;
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

        _camera.Yaw += Angle<float>.FromDegrees(delta.X * _camera.Sensitivity);
        _camera.Pitch -= Angle<float>.FromDegrees(delta.Y * _camera.Sensitivity);
    }

    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        _camera.FieldOfView -= Angle<float>.FromDegrees(scrollWheel.Y);
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
