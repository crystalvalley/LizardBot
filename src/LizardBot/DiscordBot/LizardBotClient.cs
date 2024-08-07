using Discord;
using Discord.WebSocket;
using LizardBot.Common.Exceptions;
using Microsoft.Extensions.Configuration;

namespace LizardBot.DiscordBot
{
    /// <summary>
    /// 커스텀 DiscordSocketClient.
    /// <see cref="DiscordSocketClient"/>를 참고.
    /// </summary>
    public class LizardBotClient : DiscordSocketClient
    {
        private readonly string _token;

        /// <summary>
        /// 생성자.
        /// </summary>
        /// <param name="socketConfig">The <see cref="DiscordSocketConfig"/> that shoud be inject.</param>
        /// <param name="config">appsettings.json의 설정값.</param>
        /// <exception cref="NoSettingDataException">appsettings.json에 SecretKey값이 없을 경우 발생.</exception>
        public LizardBotClient(DiscordSocketConfig socketConfig, IConfiguration config)
            : base(socketConfig)
        {
            _token = config["Token"] ?? throw new NoSettingDataException("Token");
            Initialize();
        }

        private void Initialize()
        {
            LoginAsync(TokenType.Bot, _token).ConfigureAwait(false);
            StartAsync().ConfigureAwait(false);
        }
    }
}
