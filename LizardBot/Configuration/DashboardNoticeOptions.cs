namespace LizardBot.Configuration;

/// <summary>
/// Defines a static notice or service information section
/// displayed on the Discord dashboard.
/// </summary>
public sealed class DashboardNoticeOptions
{
    /// <summary>
    /// Title of the notice.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional descriptive text shown below the title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional managed server ID used to link this notice
    /// with live server status information.
    /// </summary>
    public string? ServerId { get; set; }

    /// <summary>
    /// Optional health check name used to link this notice
    /// with a specific service status.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Additional static information displayed in the notice.
    /// </summary>
    public List<DashboardFieldOptions> Fields { get; set; } = [];

    /// <summary>
    /// Optional note displayed at the bottom of the notice.
    /// </summary>
    public string? Footer { get; set; }
}