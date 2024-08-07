using Microsoft.EntityFrameworkCore;

namespace LizardBot.Data
{
    /// <summary>
    /// LizardBot용 DbContext.
    /// </summary>
    public class LizardBotDbContext(DbContextOptions options)
        : DbContext(options)
    {
    }
}
