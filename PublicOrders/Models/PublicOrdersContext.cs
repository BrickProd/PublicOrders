using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace PublicOrders.Models
{
    public class PublicOrdersContext : DbContext
    {
        public PublicOrdersContext()
            : base("PublicOrders")
        {
            Database.SetInitializer<PublicOrdersContext>(new PublicOrdersInitializer());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }



        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<FreedomProperty> FreedomProperties { get; set; }
        public DbSet<CommitteeProperty> CommitteeProperties { get; set; }
        public DbSet<Form2Property> Form2Properties { get; set; }




        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<CustomerLevel> CustomerLevels { get; set; }
        public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderPriceType> OrderPriceTypes { get; set; }
        public DbSet<LotPriceType> LotPriceTypes { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<LawType> LawTypes { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Winner> Winners { get; set; }
        public DbSet<WinnerStatus> WinnerStatuses { get; set; }
        public DbSet<WinnerNote> WinnerNotes { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommitteeProperty>().HasKey(p => new { p.ProductId, p.CommitteePropertyId });
            modelBuilder.Entity<Form2Property>().HasKey(p => new { p.ProductId, p.Form2PropertyId });
            modelBuilder.Entity<FreedomProperty>().HasKey(p => new { p.ProductId, p.FreedomPropertyId });

            modelBuilder.Entity<Customer>().HasMany(o => o.CustomerTypes).WithMany(d => d.Customers).Map(m =>
            {
                m.ToTable("Customers_CustomerTypes");
                m.MapLeftKey("CustomerId");
                m.MapRightKey("CustomerTypeId");
            });
        }


    }

    public class PublicOrdersInitializer : CreateDatabaseIfNotExists<PublicOrdersContext>
    {
        protected override void Seed(PublicOrdersContext context)
        {
            #region ПОЛЬЗОВАТЕЛИ

            // Инициализация статусов пользователя
            var userStatuses = new List<UserStatus>
            {
                new UserStatus {StatusName = "Admin"},
                new UserStatus {StatusName = "Client"}
            };
            userStatuses.ForEach(m => context.UserStatuses.Add(m));

            context.SaveChanges();

            #endregion


            #region ДОКУМЕНТЫ

            var instructions = new List<Instruction>
            {
                new Instruction {Name = "--Без инструкции--"}
            };
            instructions.ForEach(m => context.Instructions.Add(m));
            context.SaveChanges();

            var rubrics = new List<Rubric>
            {
                new Rubric {Name = "--Без рубрики--"}
            };
            rubrics.ForEach(m => context.Rubrics.Add(m));
            context.SaveChanges();

            #endregion


            #region ПОБЕДИТЕЛИ

            var customerLevels = new List<CustomerLevel>
            {
                new CustomerLevel {CustomerLevelCode = "Federal"},
                new CustomerLevel {CustomerLevelCode = "Subject"},
                new CustomerLevel {CustomerLevelCode = "Municipal"},
                new CustomerLevel {CustomerLevelCode = "Other"}
            };
            customerLevels.ForEach(m => context.CustomerLevels.Add(m));

            var customerTypes = new List<CustomerType>
            {
                new CustomerType {CustomerTypeCode = "Customer"},
                new CustomerType {CustomerTypeCode = "Organization"},
            };
            customerTypes.ForEach(m => context.CustomerTypes.Add(m));

            var lawTypes = new List<LawType>
            {
                new LawType {Name = "44"},
                new LawType {Name = "94"},
                new LawType {Name = "223"},
                new LawType {Name = "None"},
            };
            lawTypes.ForEach(m => context.LawTypes.Add(m));

            var winnerStatuses = new List<WinnerStatus>
            {
                new WinnerStatus {Name = "Для просмотра"},
                new WinnerStatus {Name = "Избранное"},
                new WinnerStatus {Name = "Чёрный список"}
            };
            winnerStatuses.ForEach(m => context.WinnerStatuses.Add(m));

            context.SaveChanges();

            #endregion
        }
    }
}
