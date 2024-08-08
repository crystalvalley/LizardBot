using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LizardBot.Data.Model
{
    [Table("platform_user", Schema = "lizardbot")]
    public class PlatformUser
    {
        [Key]
        public Guid Id { get; set; }

        public ulong DiscordId { get; set; }
    }
}
