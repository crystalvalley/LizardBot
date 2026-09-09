namespace LizardBot.Configuration;

/// <summary>
/// Root configuration containing all servers managed by LizardBot.
/// This section is loaded from "ServerManagement" in appsettings.json.
/// </summary>
public sealed class ServerManagementOptions
{
    /// <summary>
    /// Servers available for monitoring and remote management.
    /// </summary>
    public List<ManagedServerOptions> Servers { get; set; } = [];
}