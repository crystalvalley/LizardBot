using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LizardBot.Common.Enums;

namespace LizardBot.Data.Model
{
    [Table("bot_channel")]
    public class BotChannel
    {
        [Key]
        public ulong Id { get; set; }

        public ChannelSettingType SettingType { get; set; }
    }
}
