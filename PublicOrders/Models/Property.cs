using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }

        private ICollection<ParamValue> _paramValues;
        public virtual ICollection<ParamValue> ParamValues
        {
            get { return _paramValues ?? (_paramValues = new HashSet<ParamValue>()); } // Try HashSet<N>
            set { _paramValues = value; }
        }
        public Property()
        {

        }
    }
}
