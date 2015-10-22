using PublicOrders.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class Document
    {
        public int DocumentId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(128)]
        [Index]
        public string Name { get; set; }

        [Index]
        public DateTime CreateDateTime { get; set; }

        private ICollection<Product> _products;
        public virtual ICollection<Product> Products
        {
            get { return _products ?? (_products = new HashSet<Product>()); } // Try HashSet<N>
            set { _products = value; }
        }

        [ForeignKey("Instruction")]
        public int? InstructionId { get; set; }
        virtual public Instruction Instruction { get; set; }

        public Document()
        {

        }
    }

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

    public class Product : INotifyPropertyChanged
    {
        public int ProductId { get; set; }



        private string _name;
        [Index]
        [Column(TypeName = "nvarchar"), MaxLength(256)]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

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
