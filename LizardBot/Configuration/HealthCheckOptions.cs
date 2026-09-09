namespace LizardBot.Configuration;

/// <summary>
/// Defines a single service health check for a managed server.
/// </summary>
public sealed class HealthCheckOptions
{
    /// <summary>
    /// Name displayed in status responses.
    /// Example: "Web", "Minecraft".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of health check to perform.
    /// Currently supported: "Tcp".
    /// Additional types such as HTTP can be added later.
    /// </summary>
    public string Type { get; set; } = "Tcp";

    /// <summary>
    /// Network port used by the health check.
    /// </summary>
    public int Port { get; set; }
}