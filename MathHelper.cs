namespace MinecraftSharp;

internal static class MathHelper
{
    public static float DegreesToRadians(float degrees) => MathF.PI / 180f * degrees;

    public static float RadiansToDegrees(float radians) => 180f / MathF.PI * radians;
}
