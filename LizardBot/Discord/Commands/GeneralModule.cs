using Discord.Interactions;


namespace LizardBot.Discord.Commands;

/// <summary>
/// Provides basic utility commands for LizardBot.
/// </summary>
public sealed class GeneralModule
    : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>
    /// Checks whether LizardBot is online and responding to commands.
    /// </summary>
    [SlashCommand("ping", "LizardBot이 정상적으로 동작 중인지 확인합니다.")]
    public async Task PingAsync()
    {
        await RespondAsync("Pong! 🦎 LizardBot이 정상적으로 동작 중입니다.");
    }
}