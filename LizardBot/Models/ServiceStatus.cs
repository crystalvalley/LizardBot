namespace LizardBot.Models;

/// <summary>
/// Represents the result of a single service health check.
/// </summary>
public sealed record ServiceStatus(
    string Name,
    bool Online);