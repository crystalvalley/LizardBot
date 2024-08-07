using LizardBot.Data;
using LizardBot.Data.Model;

namespace LizardBot.DiscordBot.Service
{
    /// <summary>
    /// 미분류 상태의 함수들용 서비스.
    /// </summary>
    public class GeneralService
    {
        private readonly LizardBotDbContext _dbContext;

        public GeneralService(LizardBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> AddUserDataAsync(ulong discordId)
        {
            var result = Guid.NewGuid();
            _dbContext.Users.Add(new User()
            {
                DiscordId = discordId,
                Id = result,
            });
            await _dbContext.SaveChangesAsync();
            return result;
        }
    }
}
