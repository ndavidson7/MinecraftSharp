using System.Numerics;

namespace MinecraftSharp;

/// <summary>
/// Represents an angle
/// </summary>
/// <typeparam name="T">The type to use for the underlying representation of the angle</typeparam>
public readonly record struct Angle<T> :
    IAdditionOperators<Angle<T>, Angle<T>, Angle<T>>,
    ISubtractionOperators<Angle<T>, Angle<T>, Angle<T>>,
    IMultiplyOperators<Angle<T>, T, Angle<T>>,
    IDivisionOperators<Angle<T>, T, Angle<T>>,
    IDivisionOperators<Angle<T>, Angle<T>, T>,
    IModulusOperators<Angle<T>, T, Angle<T>>,
    IUnaryPlusOperators<Angle<T>, Angle<T>>,
    IUnaryNegationOperators<Angle<T>, Angle<T>>,
    IEqualityOperators<Angle<T>, Angle<T>, bool>,
    IComparisonOperators<Angle<T>, Angle<T>, bool>
    where T : INumber<T>, ITrigonometricFunctions<T>
{
    public static readonly Angle<T> Zero = new(T.Zero);

    /// <summary>
    /// The angle in radians (used as the internal representation)
    /// </summary>
    public T Radians { get; }

    /// <summary>
    /// The angle in degrees
    /// </summary>
    public T Degrees => T.RadiansToDegrees(Radians);

    private Angle(T radians)
    {
        Radians = radians;
    }

    public Angle<T> Abs() => new(T.Abs(Radians));

    public static Angle<T> Abs(Angle<T> a) => new(T.Abs(a.Radians));

    /// <summary>
    /// Creates an angle from radians
    /// </summary>
    /// <param name="radians">The angle in radians</param>
    /// <returns>The angle</returns>
    public static Angle<T> FromRadians(T radians) => new(radians);

    /// <summary>
    /// Creates an angle from degrees
    /// </summary>
    /// <param name="degrees">The angle in degrees</param>
    /// <returns>The angle</returns>
    public static Angle<T> FromDegrees(T degrees) => new(T.DegreesToRadians(degrees));

    /// <summary>
    /// Clamps this angle between a minimum and maximum angle.
    /// </summary>
    public Angle<T> Clamp(Angle<T> min, Angle<T> max)
        => new(T.Clamp(Radians, min.Radians, max.Radians));

    /// <summary>
    /// Clamps the given angle between a minimum and maximum angle.
    /// </summary>
    public static Angle<T> Clamp(Angle<T> value, Angle<T> min, Angle<T> max)
        => new(T.Clamp(value.Radians, min.Radians, max.Radians));

    // TODO: Normalization, trigonometric, etc. functions

    public static Angle<T> operator +(Angle<T> a, Angle<T> b) => new(a.Radians + b.Radians);

    public static Angle<T> operator -(Angle<T> a, Angle<T> b) => new(a.Radians - b.Radians);

    public static Angle<T> operator *(Angle<T> a, T b) => new(a.Radians * b);

    public static Angle<T> operator *(T a, Angle<T> b) => new(a * b.Radians);

    public static Angle<T> operator /(Angle<T> a, T b) => new(a.Radians / b);

    public static T operator /(Angle<T> a, Angle<T> b) => a.Radians / b.Radians;

    public static Angle<T> operator %(Angle<T> a, T b) => new(a.Radians % b);

    public static Angle<T> operator +(Angle<T> a) => a;

    public static Angle<T> operator -(Angle<T> a) => new(-a.Radians);

    public static bool operator >(Angle<T> left, Angle<T> right) => left.Radians > right.Radians;

    public static bool operator >=(Angle<T> left, Angle<T> right) => left.Radians >= right.Radians;

    public static bool operator <(Angle<T> left, Angle<T> right) => left.Radians < right.Radians;

    public static bool operator <=(Angle<T> left, Angle<T> right) => left.Radians <= right.Radians;
}
