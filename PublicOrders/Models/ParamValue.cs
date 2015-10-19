using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class ParamValue
    {
        [Key]
        public int ParamValueId { get; set; }

        [ForeignKey("Param")]
        public int ParamId { get; set; }
        virtual public Param Param { get; set; }

        [ForeignKey("Property")]
        public int PropertyId { get; set; }
        virtual public Property Property { get; set; }

        [Column(TypeName = "nvarchar"), MaxLength]
        public string Value { get; set; }


        public ParamValue()
        {

        }
    }
}
