namespace LizardBot.Models;

/// <summary>
/// Stores persistent runtime state required by the Discord dashboard.
/// </summary>
public sealed class DashboardState
{
    /// <summary>
    /// Discord channel containing the dashboard message.
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Discord message currently used as the server dashboard.
    /// </summary>
    public ulong? MessageId { get; set; }
}