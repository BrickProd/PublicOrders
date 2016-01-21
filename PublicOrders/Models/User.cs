using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace PublicOrders.Models
{
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


        private ObservableCollection<Winner> _winners;
        public virtual ObservableCollection<Winner> Winners
        {
            get { return _winners ?? (_winners = new ObservableCollection<Winner>(new HashSet<Winner>())); }
            set { _winners = value; }
        }

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
