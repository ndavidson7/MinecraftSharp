using System.ComponentModel.DataAnnotations;

namespace MinecraftSharp.Client.Configuration;

internal sealed record Settings
{
    public const string SectionName = "Settings";

    [Range(20, 240)]
    public float MaxFps { get; set; } = 60;
}