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

        public DbSet<Document> Documents { get; set; }
        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Param> Params { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ParamValue> ParamValues { get; set; }
        public DbSet<Instruction> Instructions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //у документов много атрибутов
            //атрибут может быть в разных документах
            modelBuilder.Entity<Document>().HasMany(o => o.Products).WithMany(d => d.Documents).Map(m =>
            {
                m.ToTable("Documents_Products");
                m.MapLeftKey("DocumentId");
                m.MapRightKey("ProductId");
            });

            modelBuilder.Entity<Template>().HasMany(o => o.Products).WithMany(d => d.Templates).Map(m =>
            {
                m.ToTable("Templates_Products");
                m.MapLeftKey("TemplateId");
                m.MapRightKey("ProductId");
            });

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
            var templates = new List<Template>{
                new Template{ Name = "Свобода" },
                new Template{ Name = "Форма 2" },
                new Template{ Name = "Комитет" }
            };
            templates.ForEach(m => context.Templates.Add(m));
            context.SaveChanges();

            var attributes = new List<Param>{
                //в Свобода
                new Param{ TemplateId = 1, Name = @"Требования заказчика" },
                new Param{ TemplateId = 1, Name = @"Требования участника" },
                new Param{ TemplateId = 1, Name = @"Товарный знак" },
                new Param{ TemplateId = 1, Name = @"Сертификация" },

                //в Форма 2
                new Param{ TemplateId = 2, Name = @"Товарный знак" },
                new Param{ TemplateId = 2, Name = @"Требуемый параметр" },
                new Param{ TemplateId = 2, Name = @"Требуемое значение" },
                new Param{ TemplateId = 2, Name = @"Значение, предлагаемое участником" },
                new Param{ TemplateId = 2, Name = @"Единица измерения" },
                new Param{ TemplateId = 2, Name = @"Сертификация" },


                //в Комитет
                new Param{ TemplateId = 3,Name = @"Наименование показателя" },
                new Param{ TemplateId = 3,Name = @"Минимальные значения показателей" },
                new Param{ TemplateId = 3,Name = @"Максимальные значения показателей" },
                new Param{ TemplateId = 3,Name = @"Значения показателей, которые не могут изменяться" },
                new Param{ TemplateId = 3,Name = @"Конкретные показатели" },
                new Param{ TemplateId = 3,Name = @"Единица измерения" }
            };
            attributes.ForEach(m => context.Params.Add(m));

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
                //var text = System.IO.File.ReadAllText(this.Path);
                //return text;
                return "";
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
