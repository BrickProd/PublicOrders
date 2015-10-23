using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<CustomerLevel> CustomerLevels { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<PriceType> PriceTypes { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<LawType> LawTypes { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Winner> Winners { get; set; }

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

            /*modelBuilder.Entity<Customer>().HasMany(o => o.Orders).WithMany(d => d.Customers).Map(m =>
            {
                m.ToTable("Customers_Orders");
                m.MapLeftKey("CustomerId");
                m.MapRightKey("OrderId");
            });*/

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

    public class Customer
    {
        [Key]
        public long CustomerId { get; set; }

        [ForeignKey("CustomerLevel"), Required]
        public int CustomerLevelId { get; set; }
        virtual public CustomerLevel CustomerLevel { get; set; }

        [Column(TypeName = "varchar"), MaxLength(400), Required]
        [Index]
        public string Name { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100), Required]
        [Index]
        public string Vatin { get; set; }

        [Column(TypeName = "varchar"), MaxLength(500), Required]
        [Index]
        public string Address { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Law_44_94_ID { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Law_223_ID { get; set; }

        [Index, Required]
        public DateTime CreateDateTime { get; set; }

        // Транзакционные таблицы
        private ICollection<CustomerType> _сustomerTypes;
        public virtual ICollection<CustomerType> CustomerTypes
        {
            get { return _сustomerTypes ?? (_сustomerTypes = new HashSet<CustomerType>()); } // Try HashSet<N>
            set { _сustomerTypes = value; }
        }

        /*private ICollection<Order> _orders;
        public virtual ICollection<Order> Orders
        {
            get { return _orders ?? (_orders = new HashSet<Order>()); } // Try HashSet<N>
            set { _orders = value; }
        }*/

        public Customer()
        {

        }
    }

    public class CustomerType
    {
        [Key]
        public int CustomerTypeId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string CustomerTypeCode { get; set; }

        // Транзакционные таблицы
        private ICollection<Customer> _customers;
        public virtual ICollection<Customer> Customers
        {
            get { return _customers ?? (_customers = new HashSet<Customer>()); } // Try HashSet<N>
            set { _customers = value; }
        }

        public CustomerType()
        {

        }
    }

    public class CustomerLevel
    {
        [Key]
        public int CustomerLevelId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string CustomerLevelCode { get; set; }

        public CustomerLevel()
        {

        }
    }

    public class Order
    {
        [Key]
        public long OrderId { get; set; }

        [Index]
        public long MaxLotPrice { get; set; }

        [ForeignKey("LawType"), Required]
        public int LawTypeCode { get; set; }
        virtual public LawType LawType { get; set; }

        [ForeignKey("OrderType"), Required]
        public int OrderTypeId { get; set; }
        virtual public OrderType OrderType { get; set; }

        [ForeignKey("Customer"), Required]
        public long CustomerId { get; set; }
        virtual public Customer Customer { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50), Required]
        [Index]
        public string Number { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50), Required]
        [Index]
        public string HrefId { get; set; }

        [Index, Required]
        public DateTime PublishDateTime { get; set; }

        [Index, Required]
        public DateTime CreateDateTime { get; set; }

        // Транзакционные таблицы
        /*private ICollection<Customer> _customers;
        public virtual ICollection<Customer> Customers
        {
            get { return _customers ?? (_customers = new HashSet<Customer>()); } // Try HashSet<N>
            set { _customers = value; }
        }*/

        // Транзакционные таблицы
        /*private ICollection<Lot> _lots;
        public virtual ICollection<Lot> Lots
        {
            get { return _lots ?? (_lots = new HashSet<Lot>()); } // Try HashSet<N>
            set { _lots = value; }
        }*/

        public Order()
        {

        }
    }

    public class PriceType
    {
        [Key]
        public int PriceTypeId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Name { get; set; }

        public PriceType()
        {

        }
    }

    public class OrderType
    {
        [Key]
        public int OrderTypeId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100)]
        [Index]
        public string Name { get; set; }

        public OrderType()
        {

        }
    }

    public class LawType
    {
        [Key]
        public int LawTypeCode { get; set; }

        public LawType()
        {

        }
    }

    public class Lot
    {
        [Key]
        public long LotId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(400), Required]
        [Index]
        public string Name { get; set; }

        [ForeignKey("Order"), Required]
        public long OrderId { get; set; }
        virtual public Order Order { get; set; }

        [Index, Required]
        public long Price { get; set; }

        [Index]
        public long DocumentPrice { get; set; }

        [ForeignKey("PriceType"), Required]
        public int PriceTypeId { get; set; }
        virtual public PriceType PriceType { get; set; }

        [ForeignKey("Winner")]
        public long WinnerId { get; set; }
        virtual public Winner Winner { get; set; }


        [Column(TypeName = "varchar"), MaxLength(300), Required]
        [Index]
        public string OrderHref { get; set; }

        [Index, Required]
        public DateTime DocumentDateTime { get; set; }

        [Index, Required]
        public DateTime CreateDateTime { get; set; }

        // Транзакционные таблицы
        /*private ICollection<Order> _orders;
        public virtual ICollection<Order> Orders
        {
            get { return _orders ?? (_orders = new HashSet<Order>()); } // Try HashSet<N>
            set { _orders = value; }
        }*/

        public Lot()
        {

        }
    }

    public class Winner
    {
        [Key]
        public long WinnerId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256), Required]
        [Index]
        public string Name { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100)]
        [Index]
        public string Email { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Phone { get; set; }

        public Winner()
        {

        }
    }
}
