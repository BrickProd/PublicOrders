using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class Param
    {
        [Key]
        public int ParamId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256)]
        [Index]
        public string Name { get; set; }

        [ForeignKey("Template")]
        public int TemplateId { get; set; }
        virtual public Template Template { get; set; }

        public Param()
        {
            
        }
    }
}
