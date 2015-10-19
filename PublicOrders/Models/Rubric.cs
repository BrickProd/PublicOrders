using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class Rubric
    {
        public int RubricId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(128)]
        [Index]
        public string Name { get; set; }

        private ICollection<Product> _products;
        public virtual ICollection<Product> Products
        {
            get { return _products ?? (_products = new HashSet<Product>()); } // Try HashSet<N>
            set { _products = value; }
        }
        public Rubric()
        {
            
        }
    }
}
