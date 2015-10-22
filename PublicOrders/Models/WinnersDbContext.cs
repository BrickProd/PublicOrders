using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace PublicOrders.Models
{
    public class WinnersDbContext : DbContext
    {
        public WinnersDbContext()
            : base("PublicOrdersWinnersConnection")
        {
            Database.SetInitializer<WinnersDbContext>(new WinnersInitializer());
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerLevel> CustomerLevels { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //у документов много атрибутов
            //атрибут может быть в разных документах
            modelBuilder.Entity<Customer>().HasMany(o => o.CustomerTypes).WithMany(d => d.Customers).Map(m =>
            {
                m.ToTable("Customers_CustomerTypes");
                m.MapLeftKey("CustomerId");
                m.MapRightKey("CustomerTypeId");
            });

            modelBuilder.Entity<Customer>().HasMany(o => o.Orders).WithMany(d => d.Customers).Map(m =>
            {
                m.ToTable("Customers_Orders");
                m.MapLeftKey("CustomerId");
                m.MapRightKey("OrderId");
            });

            /*modelBuilder.Entity<Order>().HasMany(o => o.Lots).WithMany(d => d.Orders).Map(m =>
            {
                m.ToTable("Orders_Lots");
                m.MapLeftKey("OrderId");
                m.MapRightKey("LotId");
            });*/
        }

        public class WinnersInitializer : CreateDatabaseIfNotExists<WinnersDbContext>
        {
            protected override void Seed(WinnersDbContext context)
            {
                var customerLevels = new List<CustomerLevel>
                {
                new CustomerLevel{ CustomerLevelCode = "Федеральный" },
                new CustomerLevel{ CustomerLevelCode = "Уровень субъекта РФ" },
                new CustomerLevel{ CustomerLevelCode = "Муниципальный" },
                new CustomerLevel{ CustomerLevelCode = "Иное" }
                };
                customerLevels.ForEach(m => context.CustomerLevels.Add(m));
                context.SaveChanges();
            }
        }
    }
}
