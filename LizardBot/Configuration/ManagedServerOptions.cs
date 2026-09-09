using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LizardBot.Configuration;

/// <summary>
/// Configuration for a single server managed by LizardBot.
/// </summary>
public sealed class ManagedServerOptions
{
    /// <summary>
    /// Unique identifier used internally and by Discord commands.
    /// Example: "lizard", "minecraft-dev".
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable server name displayed in Discord responses.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IP address or hostname used to reach the server.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// MAC address used for Wake-on-LAN.
    /// Leave empty if the server does not support remote wake.
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// Broadcast address used when sending a Wake-on-LAN Magic Packet.
    /// </summary>
    public string? BroadcastAddress { get; set; }

    /// <summary>
    /// Health checks used to determine the status of services
    /// running on this server.
    /// </summary>
    public List<HealthCheckOptions> HealthChecks { get; set; } = [];
}