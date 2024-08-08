using LizardBot.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace LizardBot.Data
{
    /// <summary>
    /// LizardBot용 DbContext.
    /// </summary>
    public class LizardBotDbContext(DbContextOptions options)
        : DbContext(options)
    {
        public DbSet<PlatformUser> Users { get; set; }

        public DbSet<BotChannel> BotChannels { get; set; }

        public DbSet<GptThread> GptThreads { get; set; }
    }
}
