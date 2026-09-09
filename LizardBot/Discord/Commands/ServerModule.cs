using Discord.Interactions;
using LizardBot.Services;
using System.Text;

namespace LizardBot.Discord.Commands;

/// <summary>
/// Provides Discord commands for checking and managing configured servers.
/// </summary>
public sealed class ServerModule(
    ServerStatusService statusService,
    WakeOnLanService wakeOnLanService) : InteractionModuleBase<SocketInteractionContext>
{

    /// <summary>
    /// Checks the current status of a configured server and its services.
    /// </summary>
    [SlashCommand("status", "관리 중인 서버와 서비스의 상태를 확인합니다.")]
    public async Task StatusAsync(string server)
    {
        // Health checks may take longer than Discord's initial interaction
        // response window, so acknowledge the command before starting them.
        await DeferAsync();

        var status = await statusService.GetStatusAsync(server);

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
    
    /// <summary>
     /// Wakes a configured server using Wake-on-LAN
     /// and waits for it to become reachable.
     /// </summary>
    [SlashCommand(
        "wake",
        "절전 상태인 서버를 Wake-on-LAN으로 깨웁니다.")]
    public async Task WakeAsync(string server)
    {
        await DeferAsync();

        var status = await statusService.GetStatusAsync(server);

        if (status is null)
        {
            await FollowupAsync($"⚠️ 서버 `{server}`를 찾을 수 없습니다.");

            return;
        }

        if (status.Online)
        {
            await FollowupAsync($"🟢 **{status.Name}**은(는) 이미 실행 중입니다.");

            return;
        }

        var sent = await wakeOnLanService.WakeAsync(server);

        if (!sent)
        {
            await FollowupAsync($"❌ **{status.Name}**에 Wake-on-LAN 패킷을 전송할 수 없습니다.");

            return;
        }

        await FollowupAsync(
            $"💤 **{status.Name}**에 Wake-on-LAN 패킷을 전송했습니다.\n" +
            "서버가 깨어나는 중입니다...");

        // The server itself may become reachable before Docker services
        // such as Minecraft have finished starting.
        var timeout = TimeSpan.FromMinutes(2);
        var interval = TimeSpan.FromSeconds(5);
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            await Task.Delay(interval);

            status = await statusService.GetStatusAsync(server);

            if (status?.Online == true)
            {
                await FollowupAsync($"🟢 **{status.Name} ONLINE**");

                return;
            }
        }

        await FollowupAsync($"⚠️ **{status.Name}**이(가) 제한 시간 내에 응답하지 않았습니다.");
    }
}