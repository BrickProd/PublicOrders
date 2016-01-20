using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using PublicOrders.Annotations;

namespace PublicOrders.Models
{
    public class Instruction
    {
        [Key]
        public int InstructionId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256)]
        [Index]
        public string Name { get; set; }

        [Column(TypeName = "ntext")]
        public string Text { get; set; }

        public Instruction()
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

        [Column(TypeName = "nvarchar"), MaxLength(400)]
        [Index]
        public string TradeMark { get; set; }

        [Column(TypeName = "ntext")]
        public string Certification { get; set; }

        [Index, Required]
        [Column(TypeName = "smallint")]
        public Int16 IsNotActualCert { get; set; }

        [ForeignKey("Rubric")]
        public int? RubricId { get; set; }
        public virtual Rubric Rubric { get; set; }

        [Index]
        [Column(TypeName = "datetime2")]
        public DateTime ModifiedDateTime { get; set; }

        [NotMapped]
        public string ModifiedDateTimeRus {
            get { return ModifiedDateTime.ToString("d MMM yyyy"); }
        }

        private ObservableCollection<CommitteeProperty> _committeeProperties;
        public virtual ObservableCollection<CommitteeProperty> CommitteeProperties
        {
            get { return _committeeProperties ?? (_committeeProperties = new ObservableCollection<CommitteeProperty>(new HashSet<CommitteeProperty>())); } // Try HashSet<N>
            set { _committeeProperties = value; }
        }

        private ObservableCollection<Form2Property> _form2Properties;
        public virtual ObservableCollection<Form2Property> Form2Properties
        {
            get { return _form2Properties ?? (_form2Properties = new ObservableCollection<Form2Property>(new HashSet<Form2Property>())); } // Try HashSet<N>
            set { _form2Properties = value; }
        }

        private ObservableCollection<FreedomProperty> _freedomProperties;
        public virtual ObservableCollection<FreedomProperty> FreedomProperties
        {
            get { return _freedomProperties ?? (_freedomProperties = new ObservableCollection<FreedomProperty>(new HashSet<FreedomProperty>())); } // Try HashSet<N>
            set { _freedomProperties = value; }
        }

        public string Komitet
        {
            get
            {
                return CommitteeProperties.Any() ? "К" : "";
            }
        }

        public string Form2
        {
            get
            {
                return Form2Properties.Any() ? "Ф" : "";
            }
        }

        public string Svoboda
        {
            get
            {
                return FreedomProperties.Any() ? "С" : "";
            }
        }

        private bool _isSelected;
        [NotMapped]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        public Product()
        {
            this.IsRepetition = false;
        }

        [NotMapped]
        private bool _isRepetiotion;
        public bool IsRepetition
        {
            get
            {
                return _isRepetiotion;
            }
            set
            {
                _isRepetiotion = value;
                OnPropertyChanged("IsRepetition");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    #region ШАБЛОНЫ
    public class CommitteeProperty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommitteePropertyId { get; set; }

        [ForeignKey("Product"), Required]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }

        [Column(TypeName = "ntext")]
        public string ParamName { get; set; }

        [Column(TypeName = "ntext")]
        public string MinValue { get; set; }

        [Column(TypeName = "ntext")]
        public string MaxValue { get; set; }

        [Column(TypeName = "ntext")]
        public string VariableParam { get; set; }

        [Column(TypeName = "ntext")]
        public string SpecificParam { get; set; }

        [Column(TypeName = "ntext")]
        public string Measure { get; set; }

        public CommitteeProperty()
        {

        }
    }

    public class Form2Property
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Form2PropertyId { get; set; }

        [ForeignKey("Product"), Required]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }


        [Column(TypeName = "ntext")]
        public string RequiredParam { get; set; }

        [Column(TypeName = "ntext")]
        public string RequiredValue { get; set; }

        [Column(TypeName = "ntext")]
        public string OfferValue { get; set; }

        [Column(TypeName = "ntext")]
        public string Measure { get; set; }

        public Form2Property()
        {

        }
    }

    public class FreedomProperty
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FreedomPropertyId { get; set; }

        [ForeignKey("Product"), Required]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }

        [Column(TypeName = "ntext")]
        public string CustomerParam { get; set; }

        [Column(TypeName = "ntext")]
        public string MemberParam { get; set; }

        public FreedomProperty()
        {

        }
    }
    #endregion

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
