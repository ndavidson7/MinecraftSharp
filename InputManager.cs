using MinecraftSharp.Graphics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;

namespace MinecraftSharp;

internal class InputManager : IDisposable
{
    private readonly IWindow _window;
    private readonly IInputContext _inputContext;
    private readonly IKeyboard? _keyboard;
    private readonly Camera _camera;
    private Vector2? _lastMousePosition;
    private bool _disposed;

    public InputManager(IInputContext input, IWindow window, Camera camera)
    {
        _inputContext = input;
        _window = window;
        _camera = camera;

        if (input.Keyboards.Count > 0)
        {
            _keyboard = input.Keyboards[0];
            _keyboard.KeyDown += OnKeyDown;
        }

        foreach (IMouse mouse in input.Mice)
        {
            mouse.Cursor.IsConfined = true;
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseWheel;
        }
    }

    public void Update(float deltaTime)
    {
        if (_keyboard is null)
            return;

        Vector3 direction = default;
        if (_keyboard.IsKeyPressed(Key.W) || _keyboard.IsKeyPressed(Key.S))
        {
            // Project the camera's front vector onto the XZ plane (Y=0) and normalize
            Vector3 forward = Vector3.Normalize(new(_camera.Front.X, 0, _camera.Front.Z));
            if (_keyboard.IsKeyPressed(Key.W))
                direction += forward;
            if (_keyboard.IsKeyPressed(Key.S))
                direction -= forward;
        }
        if (_keyboard.IsKeyPressed(Key.A) || _keyboard.IsKeyPressed(Key.D))
        {
            // Project the camera's right vector onto the XZ plane (Y=0) and normalize
            Vector3 right = Vector3.Normalize(new(_camera.Right.X, 0, _camera.Right.Z));
            if (_keyboard.IsKeyPressed(Key.A))
                direction -= right;
            if (_keyboard.IsKeyPressed(Key.D))
                direction += right;
        }
        if (_keyboard.IsKeyPressed(Key.Space))          direction += Vector3.UnitY;    // Up
        if (_keyboard.IsKeyPressed(Key.ControlLeft))    direction -= Vector3.UnitY;    // Down

        float speed = _camera.Speed;
        if (_keyboard.IsKeyPressed(Key.ShiftLeft))
            speed *= 2f;
        
        if (direction != default)
            _camera.Position += Vector3.Normalize(direction) * speed * deltaTime;
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (_lastMousePosition is null)
        {
            _lastMousePosition = position;
            return;
        }

        Vector2 delta = position - _lastMousePosition!.Value;
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
            ToggleCursor();
    }

    private void ToggleCursor()
    {
        _lastMousePosition = default; // reset the last mouse position to avoid sudden jumps

        foreach (IMouse mouse in _inputContext.Mice)
        {
            // toggle the cursor mode
            mouse.Cursor.CursorMode = mouse.Cursor.CursorMode == CursorMode.Normal
                ? CursorMode.Raw
                : CursorMode.Normal;

            mouse.Position = (Vector2)_window.Size / 2; // center the cursor in the window

            // add or remove the event handlers
            if (mouse.Cursor.CursorMode == CursorMode.Raw)
            {
                mouse.MouseMove += OnMouseMove;
                mouse.Scroll += OnMouseWheel;
            }
            else
            {
                mouse.MouseMove -= OnMouseMove;
                mouse.Scroll -= OnMouseWheel;
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_keyboard is not null)
                    _keyboard.KeyDown -= OnKeyDown;

                foreach (IMouse mouse in _inputContext.Mice)
                {
                    mouse.MouseMove -= OnMouseMove;
                    mouse.Scroll -= OnMouseWheel;
                }

                _inputContext?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~InputManager()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
