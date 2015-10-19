using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicOrders.Models
{
    public class Template
    {
        [Key]
        public int TemplateId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256)]
        [Index]
        public string Name { get; set; }

        private ICollection<Product> _products;
        public virtual ICollection<Product> Products
        {
            get { return _products ?? (_products = new HashSet<Product>()); } // Try HashSet<N>
            set { _products = value; }
        }

        private ICollection<Param> _params;
        public virtual ICollection<Param> Param
        {
            get { return _params ?? (_params = new HashSet<Param>()); } // Try HashSet<N>
            set { _params = value; }
        }
        public Template()
        {
           
        }
    }
}
