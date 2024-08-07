using Discord.Commands;

namespace LizardBot.DiscordBot.Command
{
    /// <summary>
    /// ping 커맨드.
    /// </summary>
    public class PingCommand : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// ping을 받으면 pong을 돌려줌.
        /// </summary>
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("pong!!");
        }
    }
}
