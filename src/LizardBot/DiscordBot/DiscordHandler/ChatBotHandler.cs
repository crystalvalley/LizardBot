using Discord;
using Discord.WebSocket;
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

        private readonly Dictionary<string, string> _assistantDic = [];
        private readonly string _selectorCustomId = "ai-selector";

        public ChatBotHandler(ILogger<ChatBotHandler> logger, LizardBotClient client, ChatBotService chatBotService)
        {
            _logger = logger;
            _client = client;
            _chatBotService = chatBotService;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Connected += OnConnected;
            _client.SelectMenuExecuted += OnSelectMenuExecuted;
            _client.MessageReceived += OnMessageReceived;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopAsync(cancellationToken);
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Channel.GetChannelType() != ChannelType.PublicThread) return;
            if (message.Author.IsBot) return;

            var channelId = message.Channel.Id;

            if (await _chatBotService.GetThreadIdAsync(channelId) is null) return;
            if (message.Content == "&답")
            {
                var answer = await _chatBotService.CreateRunAsync(channelId);
                Console.WriteLine(answer);
                await message.Channel.SendMessageAsync(answer);
                await message.DeleteAsync();
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

            await ((IMessageChannel)await _client.GetChannelAsync(1270673734889639936))
                .ModifyMessageAsync(1270750153627926681, msgProp =>
                {
                    msgProp.Embed = builder.Build();
                    msgProp.Components = componentBuilder.Build();
                });
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
