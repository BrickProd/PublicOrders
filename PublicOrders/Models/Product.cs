﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Annotations;

namespace PublicOrders.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int ProductId { get; set; }

        
        
        private string _name;
        [Index]
        [Column(TypeName = "nvarchar"), MaxLength(256)]
        public string Name { get { return _name; }
                set {
                _name = value;
                OnPropertyChanged("Name");
            } }

        [Column(TypeName = "nvarchar"), MaxLength(256)]
        [Index]
        public string TradeMark { get; set; }

        [ForeignKey("Rubric")]
        public int? RubricId { get; set; }
        virtual public Rubric Rubric { get; set; }

        private ICollection<Template> _templates;
        public virtual ICollection<Template> Templates
        {
            get { return _templates ?? (_templates = new HashSet<Template>()); } // Try HashSet<N>
            set { _templates = value; }
        }

        private ICollection<Document> _documents;
        public virtual ICollection<Document> Documents
        {
            get { return _documents ?? (_documents = new HashSet<Document>()); } // Try HashSet<N>
            set { _documents = value; }
        }

        private ICollection<Property> _properties;
        public virtual ICollection<Property> Properties
        {
            get { return _properties ?? (_properties = new HashSet<Property>()); } // Try HashSet<N>
            set { _properties = value; }
        }

        //[NotMapped]
        //public List<ParamValue> ParamValue
        //{
        //    get
        //    {
        //        var list = new List<ParamValue>();
        //        this.Properties.ForEach(m => list.AddRange(m.ParamValues));
        //        return list;
        //    }
        //}
        //[NotMapped]
        //public List<Template> Templates
        //{
        //    get
        //    {
        //        var templates = this.ParamValue.Select(m => m.Param.Template).ToList();
        //        var d = templates.Distinct().ToList();
        //        return d;
        //        //return templates.GroupBy(m=>m.TemplateId).Select(m=>m.;
        //    }
        //}

        //public IEnumerable<IEnumerable<int>> Template
        //{
        //    get { return this.Properties.Select(m => m.ParamValues.Select(t => t.Param.Template.TemplateId)); }
        //}

        public Product()
        {
            
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
