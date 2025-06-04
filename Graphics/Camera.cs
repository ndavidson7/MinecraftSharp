using System.Numerics;

namespace MinecraftSharp.Graphics;

internal class Camera
{
    private const float MinSpeed = 0.5f;
    private const float MaxSpeed = 10f;
    private const float MinSensitivity = 0f;
    private const float MaxSensitivity = 100f;
    private const float MinFov = 60f;
    private const float MaxFov = 90f;
    private const float PiOver2 = MathF.PI / 2f;
    private const float NearPlaneDistance = 0.01f;

    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;

    // Rotation around the X axis (radians)
    private float _pitch;

    // Rotation around the Y axis (radians)
    private float _yaw = -PiOver2; // Without this, you would be started rotated 90 degrees right.

    // The field of view of the camera (radians)
    private float _fov = PiOver2; // 90 degrees

    private float _farPlaneDistance = 100f;

    private float _speed = 1.5f;
    private float _sensitivity = 0.2f;

    public Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;
    }

    // The position of the camera
    public Vector3 Position { get; set; }

    // This is simply the aspect ratio of the viewport, used for the projection matrix.
    public float AspectRatio { get; set; }

    public Vector3 Front { get => _front; }

    public Vector3 Up { get => _up; }
    
    public Vector3 Right { get => _right; }

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
            // of weird "bugs" when you are using euler angles for rotation.
            // If you want to read more about this you can try researching a topic called gimbal lock
            var angle = Math.Clamp(value, -89f, 89f);
            _pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    // The field of view (FOV) is the vertical angle of the camera view.
    // This has been discussed more in depth in a previous tutorial,
    // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
    // We convert from degrees to radians as soon as the property is set to improve performance.
    public float FieldOfView
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            var angle = Math.Clamp(value, MinFov, MaxFov);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    //public float FarPlaneDistance
    //{
    //    get => _farPlaneDistance;
    //    set
    //    {
    //        _farPlaneDistance = Math.Clamp(value, 
    //    }
    //}

    public float Speed
    {
        get => _speed;
        set
        {
            _speed = Math.Clamp(value, MinSpeed, MaxSpeed);
        }
    }

    public float Sensitivity
    {
        get => _sensitivity;
        set
        {
            _sensitivity = Math.Clamp(value, MinSensitivity, MaxSensitivity);
        }
    }

    public Matrix4x4 GetViewMatrix() => Matrix4x4.CreateLookAt(Position, Position + _front, _up);

    public Matrix4x4 GetProjectionMatrix() => Matrix4x4.CreatePerspectiveFieldOfView(_fov, AspectRatio, NearPlaneDistance, _farPlaneDistance);

    // This function is going to update the direction vertices using some of the math learned in the web tutorials.
    private void UpdateVectors()
    {
        // First, the front matrix is calculated using some basic trigonometry.
        _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        _front.Y = MathF.Sin(_pitch);
        _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        _front = Vector3.Normalize(_front);

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}
