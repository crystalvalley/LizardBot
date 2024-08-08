using Discord;
using Discord.WebSocket;
using LizardBot.Common.Enums;
using LizardBot.Data.Model;
using LizardBot.DiscordBot.Service;
using LizardBot.WebClient.ChatGpt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LizardBot.DiscordBot.DiscordHandler
{
    public class ChatBotHandler : IHostedService
    {
        private readonly ILogger _logger;
        private readonly LizardBotClient _client;
        private readonly ChatBotService _chatBotService;
        private readonly GeneralService _generalService;

        private readonly Dictionary<string, string> _assistantDic = [];
        private readonly string _selectorCustomId = "ai-selector";

        private readonly Dictionary<ulong, BotChannel> _chatBotChannels = [];

        public ChatBotHandler(ILogger<ChatBotHandler> logger, LizardBotClient client, ChatBotService chatBotService, GeneralService generalService)
        {
            _logger = logger;
            _client = client;
            _chatBotService = chatBotService;
            _generalService = generalService;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 이벤트 설정
            _client.Connected += OnConnected;
            _client.SelectMenuExecuted += OnSelectMenuExecuted;
            _client.MessageReceived += OnMessageReceived;

            // 채널 로딩
            var list = (await _generalService.GetCahnnelsAsync(ChannelSettingType.ChatBot)).ToList();
            foreach (var item in list)
                _chatBotChannels.Add(item.Id, item);
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopAsync(cancellationToken);
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            _logger.LogInformation("입력된 메시지 : {}", message);
            if (message.Channel.GetChannelType() != ChannelType.PublicThread) return;
            if (message.Author.IsBot) return;

            var channelId = message.Channel.Id;

            if (await _chatBotService.GetThreadIdAsync(channelId) is null) return;
            if (message.Content == "&답")
            {
                var delTask = message.DeleteAsync();
                var answer = await _chatBotService.CreateRunAsync(channelId);

                await message.Channel.SendMessageAsync(answer);
                delTask.GetAwaiter().GetResult();
                return;
            }

            await _chatBotService.AddMessageAsync(message.Content, message.Channel.Id);
        }

        /// <summary>
        /// 접속 시점에 챗봇 공지사항을 작성하여 전송하거나 기존 메시지를 수정함.
        /// </summary>
        private async Task OnConnected()
        {
            var builder = new EmbedBuilder()
                    .WithAuthor("CrystalValley")
                    .WithTitle("🎉(실험중인 기능) 챗봇과 대화하기")
                    .WithDescription("개별 학습이 가능한 여러 어시스턴트와 대화하는 것이 가능합니다." + System.Environment.NewLine +
                    "또한, 메시지를 보내자 마자 무조건 답변하는 것이 아니라 메시지를 여러번 전송한 다음에 원할 때 답변을 받을 수 있습니다." + System.Environment.NewLine +
                    "답변을 원하시면 [&답] 이라고 입력해주세요.")
                    .WithFooter("갱신일자 - 2024/08/07");

            var selectmenuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("대화할 어시스턴트를 골라주세요.")
                .WithCustomId(_selectorCustomId)
                .WithMinValues(1)
                .WithMaxValues(1);

            var assistants = await _chatBotService.GetAssistantsAsync();

            foreach (var assistant in assistants)
            {
                _assistantDic.Add(assistant.Id, assistant.Name ?? assistant.Id);
                selectmenuBuilder.AddOption(assistant.Name, assistant.Id, assistant.Description);
            }

            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(selectmenuBuilder);

            foreach (var ch in _chatBotChannels)
            {
                if (ch.Value.NoticeId == 0)
                {
                    var msg = ((IMessageChannel)await _client.GetChannelAsync(ch.Key))
                        .SendMessageAsync(embed: builder.Build(), components: componentBuilder.Build());
                    ch.Value.NoticeId = (ulong)msg.Id;
                    await _generalService.UpdateChannelAsync(ch.Value);
                }
                else
                {
                    await ((IMessageChannel)await _client.GetChannelAsync(ch.Key))
                        .ModifyMessageAsync(ch.Value.NoticeId, msgProp =>
                        {
                            msgProp.Embed = builder.Build();
                            msgProp.Components = componentBuilder.Build();
                        });
                }
            }
        }

        /// <summary>
        /// 어시스턴트가 선택되었을 경우에만.
        /// </summary>
        /// <param name="component">선택된 컴포넌트.</param>
        private async Task OnSelectMenuExecuted(SocketMessageComponent component)
        {
            var channel = component.Channel as SocketTextChannel;
            ArgumentNullException.ThrowIfNull(channel);
            if (component.Data.CustomId != _selectorCustomId) return;
            string assistantId = component.Data.Values.First();

            var threadStartMessage =
                await channel.SendMessageAsync("대화를 시작해보세요.");
            var thread = await channel.CreateThreadAsync($"{component.User.Username}의 대화, 담당봇 : {_assistantDic[assistantId]}", message: threadStartMessage);

            await _chatBotService.CreateThreadAsync(assistantId, component.User.Id, thread.Id);
            await component.RespondAsync("대화를 시작해보세요!😉", ephemeral: true);
        }
    }
}
