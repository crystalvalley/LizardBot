using Discord.Commands;
using LizardBot.WebClient.ChatGpt;

namespace LizardBot.DiscordBot.Command
{
    /// <summary>
    /// ping 커맨드.
    /// </summary>
    public class PingCommand : ModuleBase<SocketCommandContext>
    {
        private readonly ChatGptRestClient _client;

        public PingCommand(ChatGptRestClient client)
        {
            _client = client;
        }

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
