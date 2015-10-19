using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext()
            : base("DefaultConnection")
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
}
