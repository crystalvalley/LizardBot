using Discord;
using Discord.WebSocket;
using LizardBot.Common.Enums;
using LizardBot.Data.Model;
using LizardBot.DiscordBot.Service;
using LizardBot.WebClient.ChatGpt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LizardBot.DiscordBot.DiscordHandler
{
    public class ChatBotHandler : IHostedService
    {
        private readonly ILogger _logger;
        private readonly LizardBotClient _client;
        private readonly ChatBotService _chatBotService;
        private readonly GeneralService _generalService;

        private readonly Dictionary<string, (string, string?)> _assistantDic = [];
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
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;

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
            if (message.Channel.GetChannelType() != ChannelType.PublicThread) return;
            if (message.Author.IsBot) return;

            var channelId = message.Channel.Id;
            Task? reactionTask = Task.CompletedTask;
            Task? updateTask = Task.CompletedTask;
            GptThread? gptThread = await _chatBotService.GetThreadAsync(channelId, false);

            if (gptThread is null) return;
            if (message.Content.Trim().StartsWith('&'))
            {
                _logger.LogInformation("입력된 명령어 : {}", message.Content);
                var delTask = message.DeleteAsync();
                if (message.Content == "&답")
                {
                    var answer = await _chatBotService.CreateRunAsync(channelId);

                    await message.Channel.SendMessageAsync(answer);
                }
                else if (message.Content == "&종료")
                {
                    await _chatBotService.AddMessageAsync("지금까지 한 대화를 요약해줘.", message.Channel.Id);
                    var answer = await _chatBotService.CreateRunAsync(channelId);
                    var msg = await message.Channel.SendMessageAsync(answer);

                    updateTask = _chatBotService.EndThreadAsync(gptThread);

                    reactionTask = msg.AddReactionAsync(new Emoji("📃"));
                }
                else _logger.LogInformation("존재하지 않은 명령어 : {}", message.Content);

                await Task.WhenAll(delTask, reactionTask, updateTask);
                return;
            }

            await _chatBotService.AddMessageAsync(message.Content, message.Channel.Id);
        }

        /// <summary>
        /// 접속 시점에 챗봇 공지사항을 작성하여 전송하거나 기존 메시지를 수정함.
        /// </summary>
        private async Task OnConnected()
        {
            var assistants = await _chatBotService.GetAssistantsAsync();

            foreach (var assistant in assistants)
            {
                _assistantDic.Add(assistant.Id, (assistant.Name ?? assistant.Id, assistant.Description));
            }

            var builder = new EmbedBuilder()
                    .WithAuthor("CrystalValley")
                    .WithTitle("🎉(실험중인 기능) 챗봇과 대화하기")
                    .WithDescription("개별 학습이 가능한 여러 어시스턴트와 대화하는 것이 가능합니다." + Environment.NewLine +
                    "또한, 메시지를 보내자 마자 무조건 답변하는 것이 아니라 메시지를 여러번 전송한 다음에 원할 때 답변을 받을 수 있습니다.")
                    .WithFooter("갱신일자 - 2024/08/09")
                    .AddField(
                        "커맨드 설명",
                        "&답 - 전송된 메시지를 기반으로 챗봇의 답변을 요구합니다." + Environment.NewLine + "&종료 - 현재 대화를 종료하고 대화의 요약본을 받아 봅니다.")
                    .AddField("대화 저장 기능", "요약본의 메시지에 📃 반응을 추가하면 해당 대화는 저장됩니다,");

            var componentBuilder = MakeComponentBuilder();

            foreach (var ch in _chatBotChannels)
            {
                if (ch.Value.NoticeId == 0)
                {
                    var msg = ((IMessageChannel)await _client.GetChannelAsync(ch.Key))
                        .SendMessageAsync(embed: builder.Build(), components: componentBuilder.Build());
                    Console.WriteLine($"id {msg.Id}");
                    ch.Value.NoticeId = Convert.ToUInt64(msg.Id);
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

            var createThreadTask = _chatBotService.CreateThreadAsync(assistantId, component.User.Id, thread.Id);
            var respondTask = component.RespondAsync("대화를 시작해보세요!😉", ephemeral: true);
            var updateTask = component.Message.ModifyAsync(msgProp =>
            {
                msgProp.Components = MakeComponentBuilder().Build();
            });

            await Task.WhenAll(createThreadTask, respondTask, updateTask);
        }

        private ComponentBuilder MakeComponentBuilder()
        {
            var selectmenuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("대화할 어시스턴트를 골라주세요.")
                .WithCustomId(_selectorCustomId)
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var assistant in _assistantDic)
            {
                var (name, description) = assistant.Value;
                selectmenuBuilder.AddOption(name, assistant.Key, description);
            }

            return new ComponentBuilder()
                .WithSelectMenu(selectmenuBuilder);
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
        {
            throw new NotImplementedException();
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
        {
            var messageJob = cachedMessage.GetOrDownloadAsync();

            // 파일 저장용 리액션인가
            if (reaction.Emote.Name != "📃") return;

            // 챗봉용 쓰레드인가
            GptThread? gptThread = await _chatBotService.GetThreadAsync(originChannel.Id);
            if (gptThread is null) return;

            Console.WriteLine("?");

            // 리액션 한 사람이 쓰레드 오너인가
            var id = await _generalService.GetUserGuidAsync(reaction.UserId);
            var message = messageJob.GetAwaiter().GetResult();
            if (id != gptThread.OwnerId)
            {
                await message.RemoveReactionAsync(new Emoji("📃"), reaction.UserId);
                return;
            }

            _logger.LogInformation("유저 {}가 {}메시지를 벡터스토어 저장 하려함.", reaction.UserId, cachedMessage.Id);

            // 파일로 저장
            await _chatBotService.SaveVectorFileAsync(message.Content, message.Id, originChannel.Id);
        }
    }
}
