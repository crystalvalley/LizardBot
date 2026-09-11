namespace LizardBot.Configuration;

/// <summary>
/// Defines a single label and value displayed inside a dashboard notice.
/// </summary>
public sealed class DashboardFieldOptions
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}