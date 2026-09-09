using Discord.Interactions;
using LizardBot.Services;
using System.Text;

namespace LizardBot.Discord.Commands;

/// <summary>
/// Provides Discord commands for checking and managing configured servers.
/// </summary>
public sealed class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ServerStatusService _statusService;

    public ServerModule(ServerStatusService statusService)
    {
        _statusService = statusService;
    }

    /// <summary>
    /// Checks the current status of a configured server and its services.
    /// </summary>
    [SlashCommand("status", "관리 중인 서버와 서비스의 상태를 확인합니다.")]
    public async Task StatusAsync(string server)
    {
        // Health checks may take longer than Discord's initial interaction
        // response window, so acknowledge the command before starting them.
        await DeferAsync();

        var status = await _statusService.GetStatusAsync(server);

        if (status is null)
        {
            await FollowupAsync($"⚠️ 서버 `{server}`를 찾을 수 없습니다.");

            return;
        }

        if (!status.Online)
        {
            await FollowupAsync($"🔴 **{status.Name} OFFLINE**");

            return;
        }

        var message = new StringBuilder();

        message.AppendLine($"🟢 **{status.Name} ONLINE**");
        message.AppendLine();

        // Service names come from configuration, so the response automatically
        // adapts when health checks are added or removed.
        foreach (var service in status.Services)
        {
            message.AppendLine($"{service.Name,-12} {(service.Online ? "✅" : "❌")}");
        }

        await FollowupAsync(message.ToString());
    }
}