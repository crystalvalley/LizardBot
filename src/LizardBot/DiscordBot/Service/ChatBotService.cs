using LizardBot.Data;
using LizardBot.Data.Model;
using LizardBot.WebClient.ChatGpt;
using LizardBot.WebClient.ChatGpt.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LizardBot.DiscordBot.Service
{
    /// <summary>
    /// 챗봇 관련 서비스.
    /// </summary>
    public class ChatBotService
    {
        private readonly ILogger _logger;
        private readonly IDbContextFactory<LizardBotDbContext> _dbContextFactory;
        private readonly ChatGptRestClient _client;
        private readonly GeneralService _generalService;
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());

        public ChatBotService(ILogger<ChatBotService> logger, IDbContextFactory<LizardBotDbContext> dbContextFactory, ChatGptRestClient client, GeneralService generalService)
        {
            _client = client;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _generalService = generalService;
        }

        public async Task CreateThreadAsync(string assistantId, ulong userDiscordId, ulong channelId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            var threadJob = _client.CreateThreadAsync();
            Guid userId;
            var user = await dbContext.Users
                .Where(u => u.DiscordId == userDiscordId)
                .FirstOrDefaultAsync();
            if (user is null) userId = await _generalService.AddUserDataAsync(userDiscordId);
            else userId = user.Id;

            var thread = threadJob.GetAwaiter().GetResult();
            ArgumentNullException.ThrowIfNull(thread);
            await dbContext.GptThreads
                .AddAsync(new GptThread()
                {
                    AssistantId = assistantId,
                    Id = thread.Id,
                    OwnerId = userId,
                    ChannelId = channelId,
                });
            _cache.Set(channelId, thread.Id);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddMessageAsync(string message, ulong channelId)
            => await _client.CreateMessageAsync((await GetThreadIdAsync(channelId) ?? throw new ArgumentNullException("threadId")).Id, message);

        public async Task<string> CreateRunAsync(ulong channelId)
        {
            _logger.LogInformation("{}", channelId);
            var dbContext = _dbContextFactory.CreateDbContext();
            var thread = await GetThreadIdAsync(channelId);
            ArgumentNullException.ThrowIfNull(thread);
            var (gptMessage, run) = await _client.CreateRunAsync(thread.Id, thread.AssistantId);
            ArgumentNullException.ThrowIfNull(gptMessage);
            ArgumentNullException.ThrowIfNull(run);
            var str = JObject.Parse(gptMessage.Content[0].ToString()!)["text"]!["value"]!.ToString();
            thread.InputUsage += run.Usage!.PromptTokens;
            thread.OutputUsage += run.Usage!.CompletionTokens;
            dbContext.Update(thread);
            await dbContext.SaveChangesAsync();
            return str;
        }

        public async Task<List<GptAssistantObj>> GetAssistantsAsync()
            => await _client.GetAssistantsAsync();

        public async Task<GptThread?> GetThreadIdAsync(ulong channelId)
        {
            _logger.LogDebug("??");
            var dbContext = _dbContextFactory.CreateDbContext();
            GptThread? thread;
            if (!_cache.TryGetValue(channelId, out thread))
            {
                thread = await dbContext.GptThreads
                    .Where(t => t.ChannelId == channelId)
                    .FirstOrDefaultAsync();
            }

            return thread;
        }
    }
}
