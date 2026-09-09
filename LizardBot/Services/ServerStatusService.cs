using System.Net.NetworkInformation;
using System.Net.Sockets;
using LizardBot.Configuration;
using LizardBot.Models;
using Microsoft.Extensions.Options;

namespace LizardBot.Services;

/// <summary>
/// Checks the availability of managed servers and their configured services.
/// </summary>
public sealed class ServerStatusService(
    IOptionsMonitor<ServerManagementOptions> options,
    ILogger<ServerStatusService> logger)
{

    /// <summary>
    /// Returns the current status of a server identified by its configured ID.
    /// </summary>
    public async Task<ServerStatus?> GetStatusAsync(
        string serverId,
        CancellationToken cancellationToken = default)
    {
        // CurrentValue is read for every request so that configuration changes
        // detected by IOptionsMonitor can be reflected without recreating this service.
        var server = options.CurrentValue.Servers
            .FirstOrDefault(x =>
                string.Equals(x.Id, serverId, StringComparison.OrdinalIgnoreCase));

        if (server is null)
            return null;

        var online = await CheckPingAsync(server.Address, cancellationToken);

        // If the host itself cannot be reached, there is no need to probe
        // the individual services running on it.
        if (!online)
        {
            return new ServerStatus(server.Id, server.Name, false, []);
        }

        // Service checks are independent from each other,
        // so they can safely run in parallel.
        var checkTasks = server.HealthChecks.Select(
            check => CheckServiceAsync(server, check, cancellationToken));

        var services = await Task.WhenAll(checkTasks);

        return new ServerStatus(server.Id, server.Name, true, services);
    }

    /// <summary>
    /// Returns the current status of every configured server.
    /// </summary>
    public async Task<IReadOnlyList<ServerStatus>> GetAllStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var serverIds = options.CurrentValue.Servers.Select(x => x.Id).ToArray();

        var tasks = serverIds.Select(id => GetStatusAsync(id, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return [.. results
            .Where(x => x is not null)
            .Cast<ServerStatus>()];
    }

    /// <summary>
    /// Executes the configured health check for a single service
    /// and returns its current availability.
    /// </summary>
    private async Task<ServiceStatus> CheckServiceAsync(
        ManagedServerOptions server,
        HealthCheckOptions check,
        CancellationToken cancellationToken)
    {
        var online = check.Type.ToLowerInvariant() switch
        {
            "tcp" => await CheckTcpAsync(server.Address, check.Port, cancellationToken),
            _ => HandleUnsupportedCheckType(server, check)
        };

        return new ServiceStatus(check.Name, online);
    }

    /// <summary>
    /// Handles an unsupported health check type without interrupting monitoring.
    /// </summary>
    private bool HandleUnsupportedCheckType(
        ManagedServerOptions server,
        HealthCheckOptions check)
    {
        logger.LogWarning(
            "Unsupported health check type '{CheckType}' for service " +
            "'{ServiceName}' on server '{ServerName}'.",
            check.Type,
            check.Name,
            server.Name);

        return false;
    }

    /// <summary>
    /// Checks whether a server responds to an ICMP echo request.
    /// </summary>
    private static async Task<bool> CheckPingAsync(
        string address,
        CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();

            var reply = await ping.SendPingAsync(address, timeout: 2000);

            cancellationToken.ThrowIfCancellationRequested();

            return reply.Status == IPStatus.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Network failures are treated as an offline result
            // instead of interrupting the monitoring flow.
            return false;
        }
    }

    /// <summary>
    /// Tests whether a TCP connection can be established to a specific port.
    /// </summary>
    private static async Task<bool> CheckTcpAsync(
        string address,
        int port,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();

            // Use a linked token so the caller can still cancel the operation,
            // while also preventing an unreachable service from blocking too long.
            using var timeout =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            await client.ConnectAsync(
                address,
                port,
                timeout.Token);

            return true;
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            // The connection attempt timed out.
            return false;
        }
        catch (OperationCanceledException)
        {
            // Preserve intentional cancellation from the caller.
            throw;
        }
        catch
        {
            return false;
        }
    }
}