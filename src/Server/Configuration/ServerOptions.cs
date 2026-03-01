using System.ComponentModel.DataAnnotations;

namespace MinecraftSharp.Server.Configuration;

/// <summary>
/// The configuration options for the game server
/// </summary>
internal sealed record ServerOptions
{
    /// <summary>
    /// The name of the section containing these options in the configuration
    /// </summary>
    public const string SectionName = "Server";

    /// <summary>
    /// The port to listen for incoming connections on
    /// </summary>
    [Range(1024, 50000)]
    public int Port { get; init; } = 25565;

    [Range(1, double.MaxValue)]
    public double TicksPerSecond { get; init; } = 20;

    [Range(0, int.MaxValue)]
    public int MaxPlayers { get; init; } = 10;
}