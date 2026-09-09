namespace LizardBot.Configuration;

/// <summary>
/// Configuration for background server monitoring.
/// </summary>
public sealed class MonitoringOptions
{
    /// <summary>
    /// Interval between server status checks, in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;
}