using System.Net;
using System.Net.Sockets;
using LizardBot.Configuration;
using Microsoft.Extensions.Options;

namespace LizardBot.Services;

/// <summary>
/// Sends Wake-on-LAN Magic Packets to configured managed servers.
/// </summary>
public sealed class WakeOnLanService
{
    private const int WakeOnLanPort = 9;

    private readonly IOptionsMonitor<ServerManagementOptions> _options;
    private readonly ILogger<WakeOnLanService> _logger;

    public WakeOnLanService(
        IOptionsMonitor<ServerManagementOptions> options,
        ILogger<WakeOnLanService> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Sends a Wake-on-LAN Magic Packet to the server identified by its configured ID.
    /// </summary>
    public async Task<bool> WakeAsync(
        string serverId,
        CancellationToken cancellationToken = default)
    {
        var server = _options.CurrentValue.Servers
            .FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    serverId,
                    StringComparison.OrdinalIgnoreCase));

        if (server is null)
            return false;

        if (string.IsNullOrWhiteSpace(server.MacAddress) ||
            string.IsNullOrWhiteSpace(server.BroadcastAddress))
        {
            _logger.LogWarning(
                "Wake-on-LAN configuration is missing for server '{ServerName}'.",
                server.Name);

            return false;
        }

        var macAddress = ParseMacAddress(server.MacAddress);
        var packet = CreateMagicPacket(macAddress);

        var broadcastAddress = IPAddress.Parse(server.BroadcastAddress);
        var endpoint = new IPEndPoint(
            broadcastAddress,
            WakeOnLanPort);

        using var client = new UdpClient
        {
            EnableBroadcast = true
        };

        await client.SendAsync(
            packet,
            endpoint,
            cancellationToken);

        _logger.LogInformation(
            "Wake-on-LAN Magic Packet sent to server '{ServerName}'.",
            server.Name);

        return true;
    }

    /// <summary>
    /// Converts a human-readable MAC address into its six-byte representation.
    /// </summary>
    private static byte[] ParseMacAddress(string macAddress)
    {
        // Accept common MAC address formats such as
        // AA:BB:CC:DD:EE:FF and AA-BB-CC-DD-EE-FF.
        var normalized = macAddress
            .Replace(":", string.Empty)
            .Replace("-", string.Empty);

        if (normalized.Length != 12)
        {
            throw new FormatException(
                $"Invalid MAC address: {macAddress}");
        }

        return Convert.FromHexString(normalized);
    }

    /// <summary>
    /// Creates the standard Wake-on-LAN Magic Packet payload.
    /// </summary>
    private static byte[] CreateMagicPacket(byte[] macAddress)
    {
        // A Magic Packet consists of six 0xFF bytes followed by
        // the target MAC address repeated sixteen times.
        var packet = new byte[6 + (16 * macAddress.Length)];

        Array.Fill(packet, (byte)0xFF, 0, 6);

        for (var i = 0; i < 16; i++)
        {
            Buffer.BlockCopy(
                macAddress,
                0,
                packet,
                6 + (i * macAddress.Length),
                macAddress.Length);
        }

        return packet;
    }
}