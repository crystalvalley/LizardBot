namespace LizardBot.Configuration;

/// <summary>
/// Defines storage settings for persistent dashboard runtime state.
/// </summary>
public sealed class DashboardRuntimeOptions
{
    /// <summary>
    /// Path used to persist runtime state such as the Discord Dashboard Message ID.
    /// </summary>
    public string StateFilePath { get; set; } =
        "./data/dashboard-state.json";
}