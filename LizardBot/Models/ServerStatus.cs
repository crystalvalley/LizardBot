namespace LizardBot.Models;

/// <summary>
/// Represents the current status of a managed server
/// and the services running on it.
/// </summary>
public sealed record ServerStatus(
    string Id,
    string Name,
    bool Online,
    IReadOnlyList<ServiceStatus> Services);