namespace LizardBot.Configuration;

/// <summary>
/// Defines the content and display options of the Discord server dashboard.
/// </summary>
public sealed class DashboardOptions
{
    /// <summary>
    /// Title displayed at the top of the dashboard.
    /// </summary>
    public string Title { get; set; } = "🦎 LizardBot Server Dashboard";

    /// <summary>
    /// Optional description displayed below the dashboard title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Name displayed as the server administrator or contact person.
    /// </summary>
    public string? Administrator { get; set; }

    /// <summary>
    /// Interval in minutes between periodic dashboard updates.
    /// Server status changes can still trigger an immediate update.
    /// </summary>
    public int RefreshIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Controls how managed servers are displayed on the dashboard.
    /// </summary>
    public List<DashboardServerOptions> Servers { get; set; } = [];

    /// <summary>
    /// Static notices and service information displayed on the dashboard.
    /// </summary>
    public List<DashboardNoticeOptions> Notices { get; set; } = [];
}