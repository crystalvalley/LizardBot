using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardBot.Data.Model
{
    [Table("gpt_thread", Schema = "lizardbot")]
    public class GptThread
    {
        [Key]
        public required string Id { get; set; }

        public required string AssistantId { get; set; }

        public Guid OwnerId { get; set; }

        public int InputUsage { get; set; }

        public int OutputUsage { get; set; }

        public ulong ChannelId { get; set; }

        public bool IsEnd { get; set; }
    }
}
