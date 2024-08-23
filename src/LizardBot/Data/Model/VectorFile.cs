using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardBot.Data.Model
{
    [Table("vector_file", Schema = "lizardbot")]
    public class VectorFile
    {
        [Key]
        public ulong MessageId { get; set; }

        public required string ThreadId { get; set; }

        public required string VectorStorageID { get; set; }

        public string? FileId { get; set; }

        public bool IsAttached { get; set; }
    }
}
