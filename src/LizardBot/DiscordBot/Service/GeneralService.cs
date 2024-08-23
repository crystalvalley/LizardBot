using LizardBot.Common.Enums;
using LizardBot.Data;
using LizardBot.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace LizardBot.DiscordBot.Service
{
    /// <summary>
    /// 미분류 상태의 함수들용 서비스.
    /// </summary>
    public class GeneralService
    {
        private readonly IDbContextFactory<LizardBotDbContext> _dbContextFactory;

        public GeneralService(IDbContextFactory<LizardBotDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Guid> AddUserDataAsync(ulong discordId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            var result = Guid.NewGuid();
            dbContext.Users.Add(new PlatformUser()
            {
                DiscordId = discordId,
                Id = result,
            });
            await dbContext.SaveChangesAsync();
            return result;
        }

        public async Task<Guid> GetUserGuidAsync(ulong discordId)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.Users.Where(u => u.DiscordId == discordId).Select(u => u.Id).SingleOrDefaultAsync();
        }

        public Task<List<BotChannel>> GetCahnnelsAsync(ChannelSettingType type)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.BotChannels.Where(c => c.SettingType == type).ToListAsync();
        }

        public async Task UpdateChannelAsync(BotChannel ch)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Update(ch);
            await dbContext.SaveChangesAsync();
        }
    }
}
