using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardBot.Data.Model
{
    [Table("gpt_thread")]
    public class GptThreadSet
    {
        [Key]
        public required string Id { get; set; }

        public required string AssistantId { get; set; }

        public Guid OwnerId { get; set; }

        public int TokenUsage { get; set; }

        public ulong ChannelId { get; set; }
    }
}
