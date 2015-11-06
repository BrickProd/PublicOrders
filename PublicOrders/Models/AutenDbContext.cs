using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Models
{
    public class AutenDbContext : DbContext
    {
        public AutenDbContext()
            : base("PublicOrdersAutenConnection")
        {
            Database.SetInitializer<AutenDbContext>(new AutenInitializer());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    public class AutenInitializer : CreateDatabaseIfNotExists<AutenDbContext>
    {
        protected override void Seed(AutenDbContext context)
        {
            // Инициализация статусов пользователя
            var userStatuses = new List<UserStatus>
                {
                    new UserStatus{ StatusName = "Admin" },
                    new UserStatus{ StatusName = "Client" }
                };
            userStatuses.ForEach(m => context.UserStatuses.Add(m));

            context.SaveChanges();
        }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(120), Required]
        [Index]
        public string Login { get; set; }

        [Column(TypeName = "varchar"), MaxLength(120), Required]
        [Index]
        public string Password { get; set; }

        [ForeignKey("UserStatus")]
        public int? UserStatusId { get; set; }
        public virtual UserStatus UserStatus { get; set; }

        public User()
        {

        }
    }

    public class UserStatus
    {
        [Key]
        public int UserStatusId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50), Required]
        [Index]
        public string StatusName { get; set; }

        private ObservableCollection<User> _users;
        public virtual ObservableCollection<User> Users
        {
            get { return _users ?? (_users = new ObservableCollection<User>(new HashSet<User>())); }
            set { _users = value; }
        }

        public UserStatus()
        {

        }
    }
}
