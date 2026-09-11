namespace LizardBot.Configuration;

/// <summary>
/// Defines how a managed server is displayed on the Discord dashboard.
/// </summary>
public sealed class DashboardServerOptions
{
    /// <summary>
    /// ID of the managed server defined in servers.json.
    /// </summary>
    public string ServerId { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether this server is shown on the dashboard.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Determines whether server uptime information is shown.
    /// </summary>
    public bool ShowUptime { get; set; } = true;

    /// <summary>
    /// Determines whether individual service health checks are shown.
    /// </summary>
    public bool ShowServices { get; set; } = true;
}