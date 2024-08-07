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
        private readonly LizardBotDbContext _dbContext;
        private readonly ChatGptRestClient _client;
        private readonly GeneralService _generalService;
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public ChatBotService(ILogger<ChatBotService> logger, LizardBotDbContext dbContext, ChatGptRestClient client, GeneralService generalService)
        {
            _client = client;
            _dbContext = dbContext;
            _logger = logger;
            _generalService = generalService;
        }

        public async Task CreateThreadAsync(string assistantId, ulong userDiscordId, ulong channelId)
        {
            Guid userId;
            var user = await _dbContext.Users
                .Where(u => u.DiscordId == userDiscordId)
                .FirstOrDefaultAsync();
            if (user is null) userId = await _generalService.AddUserDataAsync(userDiscordId);
            else userId = user.Id;
            var thread = await _client.CreateThreadAsync();
            ArgumentNullException.ThrowIfNull(thread);
            await _dbContext.GptThreads
                .AddAsync(new GptThreadSet()
                {
                    AssistantId = assistantId,
                    Id = thread.Id,
                    OwnerId = userId,
                    ChannelId = channelId,
                });
            _cache.Set(channelId, thread.Id);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddMessageAsync(string message, ulong channelId)
            => await _client.CreateMessageAsync((await GetThreadIdAsync(channelId) ?? throw new ArgumentNullException("threadId")).Id, message);

        public async Task<string> CreateRunAsync(ulong channelId)
        {
            var thread = await GetThreadIdAsync(channelId);
            ArgumentNullException.ThrowIfNull(thread);
            var gptMessage = await _client.CreateRunAsync(thread.Id, thread.AssistantId);

            var str = JObject.Parse(gptMessage.Content[0].ToString())["text"]["value"].ToString();
            return str;
        }

        public async Task<List<GptAssistant>> GetAssistantsAsync()
            => await _client.GetAssistantsAsync();

        public async Task<GptThreadSet?> GetThreadIdAsync(ulong channelId)
        {
            GptThreadSet? thread;
            if (!_cache.TryGetValue(channelId, out thread))
            {
                thread = await _dbContext.GptThreads
                    .Where(t => t.ChannelId == channelId)
                    .FirstOrDefaultAsync();
            }

            return thread;
        }
    }
}
