using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicOrders.Models
{
    public class Instruction
    {
        [Key]
        public int InstructionId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256)]
        [Index]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Path { get; set; }

        [NotMapped]
        public string Text
        {
            get
            {
                var text = System.IO.File.ReadAllText(this.Path);
                return text;
            }
        }

        public Instruction()
        {

        }
    }
}
