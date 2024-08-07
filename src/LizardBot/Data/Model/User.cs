using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardBot.Data.Model
{
    [Table("user")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public ulong DiscordId { get; set; }
    }
}
