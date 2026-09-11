using Discord;
using Discord.WebSocket;
using LizardBot.Configuration;
using Microsoft.Extensions.Options;

namespace LizardBot.Services;

/// <summary>
/// Periodically monitors managed servers and sends Discord notifications
/// when their online status changes.
/// </summary>
public sealed class ServerMonitorService(
    ServerStatusService statusService,
    DiscordSocketClient discordClient,
    IOptionsMonitor<MonitoringOptions> options,
    IConfiguration configuration,
    DiscordDashboardService dashboardService,
    ILogger<ServerMonitorService> logger) : BackgroundService
{
    private readonly Dictionary<string, bool> _previousStates =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Continuously checks configured servers until the application stops.
    /// </summary>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckServersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A monitoring failure should not terminate LizardBot.
                // Log the error and try again on the next interval.
                logger.LogError(
                    ex,
                    "An error occurred while monitoring servers.");
            }

            var interval = TimeSpan.FromSeconds(
                Math.Max(5, options.CurrentValue.IntervalSeconds));

            await Task.Delay(interval, stoppingToken);
        }
    }

    /// <summary>
    /// Checks all configured servers and detects online status changes.
    /// </summary>
    private async Task CheckServersAsync(
    CancellationToken cancellationToken)
    {
        var statuses =
            await statusService.GetAllStatusesAsync(cancellationToken);

        var stateChanged = false;

        foreach (var status in statuses)
        {
            if (!_previousStates.TryGetValue(
                    status.Id,
                    out var previousOnline))
            {
                _previousStates[status.Id] = status.Online;

                logger.LogInformation(
                    "Initial server state recorded: {ServerName} = {State}",
                    status.Name,
                    status.Online ? "ONLINE" : "OFFLINE");

                continue;
            }

            if (previousOnline == status.Online)
                continue;

            _previousStates[status.Id] = status.Online;
            stateChanged = true;

            logger.LogInformation(
            "Server status changed: {ServerName} = {State}",
            status.Name,
            status.Online ? "ONLINE" : "OFFLINE");
        }

        // Update periodically, but refresh immediately when a server changes state.
        await dashboardService.UpdateAsync(
            statuses,
            force: stateChanged,
            cancellationToken);
    }
}