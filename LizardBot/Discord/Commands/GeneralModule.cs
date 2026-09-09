using Discord.Interactions;

namespace LizardBot.Discord.Commands;

public sealed class GeneralModule
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Checks whether LizardBot is running.")]
    public async Task PingAsync()
    {
        await RespondAsync("Pong! 🦎");
    }
}