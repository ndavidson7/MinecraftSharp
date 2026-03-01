using System;
using System.Numerics;

namespace MinecraftSharp.Engine.Graphics;

public class Camera
{
    private const float MinSpeed = 0.5f;
    private const float MaxSpeed = 10f;
    private const float MinSensitivity = 0f;
    private const float MaxSensitivity = 100f;
    private static readonly Angle<float> MinPitch = Angle<float>.FromDegrees(-89);
    private static readonly Angle<float> MaxPitch = Angle<float>.FromDegrees(89);
    private static readonly Angle<float> MinFov = Angle<float>.FromDegrees(60);
    private static readonly Angle<float> MaxFov = Angle<float>.FromDegrees(90);
    private const float NearPlaneDistance = 0.01f;

    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;

    private Angle<float> _pitch;
    private Angle<float> _yaw = Angle<float>.FromDegrees(-90); // Without this, you would be started rotated 90 degrees right.
    private Angle<float> _fov = MaxFov;

    private readonly float _farPlaneDistance = 100f;

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

    /// <summary>
    /// Rotation around the X axis, clamped between <see cref="MinPitch"/> and <see cref="MaxPitch"/> to avoid gimbal lock.
    /// </summary>
    public Angle<float> Pitch
    {
        get => _pitch;
        set
        {
            _pitch = value.Clamp(MinPitch, MaxPitch);
            UpdateVectors();
        }
    }

    /// <summary>
    /// Rotation around the Y axis
    /// </summary>
    public Angle<float> Yaw
    {
        get => _yaw;
        set
        {
            _yaw = value;
            UpdateVectors();
        }
    }

    /// <summary>
    /// Vertical angle of the camera's view
    /// </summary>
    public Angle<float> FieldOfView
    {
        get => _fov;
        set
        {
            _fov = value.Clamp(MinFov, MaxFov);
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

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    public Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView.Radians, AspectRatio, NearPlaneDistance, _farPlaneDistance);

    private void UpdateVectors()
    {
        _front.X = MathF.Cos(Pitch.Radians) * MathF.Cos(Yaw.Radians);
        _front.Y = MathF.Sin(Pitch.Radians);
        _front.Z = MathF.Cos(Pitch.Radians) * MathF.Sin(Yaw.Radians);

        _front = Vector3.Normalize(_front);

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}