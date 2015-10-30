using PublicOrders.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext()
            : base("PublicOrdersDocsConnection")
        {
            Database.SetInitializer<DocumentDbContext>(new DocInitializer());
        }

        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<FreedomProperty> FreedomProperties { get; set; }
        public DbSet<CommitteeProperty> CommitteeProperties { get; set; }
        public DbSet<Form2Property> Form2Properties { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //у документов много атрибутов
            //атрибут может быть в разных документах
           

            //modelBuilder.Entity<Document>().Property(e => e.Name).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));
            //modelBuilder.Entity<Document>().Property(e => e.CreateDateTime).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            //в рубрике много объектов
            //объект может быть в разных рубриках
            //modelBuilder.Entity<Rubric>().HasMany(o => o.Products).WithMany(d => d.Rubrics).Map(m =>
            //{
            //    m.ToTable("Rubrics_Objects");
            //    m.MapLeftKey("RubricId");
            //    m.MapRightKey("ObjectId");
            //});



            //у объекта много параметров
            //параметр может быть у разных объектов
            //modelBuilder.Entity<Product_Param>()
            //    .HasRequired(o => o.Product)
            //    .WithMany(ob => ob.Product_Params)
            //    .HasForeignKey(o => o.ProductId);

            //modelBuilder.Entity<Product_Param>()
            //    .HasRequired(o => o.Param)
            //    .WithMany(ob => ob.Product_Params)
            //    .HasForeignKey(o => o.ParamId);

            //составной первичный ключ
            //modelBuilder.Entity<Product_Param>().HasKey(k => new { k.ProductId, k.ParamId });
        }



    }

    public class DocInitializer : CreateDatabaseIfNotExists<DocumentDbContext>
    {
        protected override void Seed(DocumentDbContext context)
        {
            var instructions = new List<Instruction>{
                new Instruction{ Name = "--Без инструкции--" }
            };
            instructions.ForEach(m => context.Instructions.Add(m));
            context.SaveChanges();

            var rubrics = new List<Rubric>{
                new Rubric{ Name = "--Без рубрики--" }
            };
            rubrics.ForEach(m => context.Rubrics.Add(m));
            context.SaveChanges();
        }
    }

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

        [Column(TypeName = "nvarchar"), MaxLength(400)]
        [Index]
        public string Certification { get; set; }

        [ForeignKey("Rubric")]
        public int? RubricId { get; set; }
        virtual public Rubric Rubric { get; set; }

        [Index]
        public DateTime ModifiedDateTime { get; set; }







        private ICollection<CommitteeProperty> _committeeProperties;
        public virtual ICollection<CommitteeProperty> CommitteeProperties
        {
            get { return _committeeProperties ?? (_committeeProperties = new HashSet<CommitteeProperty>()); } // Try HashSet<N>
            set { _committeeProperties = value; }
        }

        private ICollection<Form2Property> _form2Properties;
        public virtual ICollection<Form2Property> Form2Properties
        {
            get { return _form2Properties ?? (_form2Properties = new HashSet<Form2Property>()); } // Try HashSet<N>
            set { _form2Properties = value; }
        }

        private ICollection<FreedomProperty> _freedomProperties;
        public virtual ICollection<FreedomProperty> FreedomProperties
        {
            get { return _freedomProperties ?? (_freedomProperties = new HashSet<FreedomProperty>()); } // Try HashSet<N>
            set { _freedomProperties = value; }
        }




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


    #region ШАБЛОНЫ
    public class CommitteeProperty
    {
        [Key]
        public int CommitteePropertyId { get; set; }

        [ForeignKey("Product")]
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
        [Key]
        public int Form2PropertyId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }


        [Column(TypeName = "ntext")]
        public string RequiredParam { get; set; }

        [Column(TypeName = "ntext")]
        public string RequiredValue{ get; set; }

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
        [Key]
        public int FreedomPropertyId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        virtual public Product Product { get; set; }

        [Column(TypeName = "ntext")]
        public string CustomerParam{ get; set; }

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
