using System.Text.Json;
using LizardBot.Configuration;
using LizardBot.Models;
using Microsoft.Extensions.Options;

namespace LizardBot.Services;

/// <summary>
/// Loads and persists runtime state used by the Discord dashboard.
/// </summary>
public sealed class DashboardStateStore(
    IOptionsMonitor<DashboardRuntimeOptions> options,
    ILogger<DashboardStateStore> logger,
    IConfiguration configuration)
{

    // Prevent multiple dashboard operations from reading or writing
    // the state file at the same time.
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Loads the persisted dashboard state, or creates an initial state
    /// from application configuration when no state file exists.
    /// </summary>
    public async Task<DashboardState> LoadOrCreateAsync(
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            var path = options.CurrentValue.StateFilePath;

            if (File.Exists(path))
            {
                await using var stream = File.OpenRead(path);

                return await JsonSerializer.DeserializeAsync<DashboardState>(
                           stream,
                           JsonOptions,
                           cancellationToken)
                       ?? throw new InvalidOperationException(
                           "Dashboard state file could not be deserialized.");
            }

            var channelIdText =
                configuration["Discord:DashboardChannelId"];

            if (!ulong.TryParse(channelIdText, out var channelId))
            {
                throw new InvalidOperationException(
                    "Discord DashboardChannelId is not configured.");
            }

            // DashboardChannelId acts only as the initial value.
            // Once dashboard.json exists, the persisted state becomes
            // the source of truth and can later be changed at runtime.
            var state = new DashboardState
            {
                ChannelId = channelId,
                MessageId = null
            };

            await SaveStateFileAsync(
                path,
                state,
                cancellationToken);

            logger.LogInformation(
                "Created initial dashboard state for Discord channel {ChannelId}.",
                channelId);

            return state;
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Failed to deserialize the dashboard state file.");

            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Persists the current dashboard state to disk.
    /// </summary>
    public async Task SaveAsync(
        DashboardState state,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            var path = options.CurrentValue.StateFilePath;

            await SaveStateFileAsync(
                path,
                state,
                cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Writes dashboard state to disk while the caller holds the file lock.
    /// </summary>
    private static async Task SaveStateFileAsync(
        string path,
        DashboardState state,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(
            Path.GetFullPath(path));

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = $"{path}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                state,
                JsonOptions,
                cancellationToken);
        }

        File.Move(
            temporaryPath,
            path,
            overwrite: true);
    }
}