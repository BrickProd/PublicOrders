using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using PublicOrders.Annotations;

namespace PublicOrders.Models
{
    // З А К А З Ч И К
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

        [Column(TypeName = "varchar"), MaxLength(100)]
        [Index]
        public string Internet44Id { get; set; }

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
        private ObservableCollection<CustomerType> _сustomerTypes;
        public virtual ObservableCollection<CustomerType> CustomerTypes
        {
            get { return _сustomerTypes ?? (_сustomerTypes = new ObservableCollection<CustomerType>(new HashSet<CustomerType>())); } // Try HashSet<N>
            set { _сustomerTypes = value; }
        }

        private ObservableCollection<Order> _orders;
        public virtual ObservableCollection<Order> Orders
        {
            get { return _orders ?? (_orders = new ObservableCollection<Order>(new HashSet<Order>())); } // Try HashSet<N>
            set { _orders = value; }
        }

        public Customer()
        {

        }
    }

    // Т И П   З А К А З Ч И К А
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

    // У Р О В Е Н Ь   З А К А З Ч И К А
    public class CustomerLevel
    {
        [Key]
        public int CustomerLevelId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string CustomerLevelCode { get; set; }

        private ICollection<Customer> _customers;
        public virtual ICollection<Customer> Customers
        {
            get { return _customers ?? (_customers = new HashSet<Customer>()); } // Try HashSet<N>
            set { _customers = value; }
        }

        public CustomerLevel()
        {

        }
    }

    // З А К А З
    public class Order
    {
        [Key]
        public long OrderId { get; set; }

        [ForeignKey("LawType"), Required]
        public int LawTypeId { get; set; }
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

        /*[Column(TypeName = "varchar"), MaxLength(50), Required]
        [Index]
        public string HrefId { get; set; }*/

        [Index, Required]
        public DateTime PublishDateTime { get; set; }

        [Index, Required]
        public DateTime CreateDateTime { get; set; }

        /*[Index]
        public DateTime? WinnersSearchDateTime { get; set; }*/

        private ObservableCollection<Lot> _lots;
        public virtual ObservableCollection<Lot> Lots
        {
            get { return _lots ?? (_lots = new ObservableCollection<Lot>(new HashSet<Lot>())); } // Try HashSet<N>
            set { _lots = value; }
        }

        public Order()
        {

        }
    }

    
    public class LotPriceType
    {
        [Key]
        public int LotPriceTypeId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Name { get; set; }

        public LotPriceType()
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
        public int LawTypeId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(10)]
        [Index]
        public string Name { get; set; }

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

        [ForeignKey("Winner")]
        public long WinnerId { get; set; }
        virtual public Winner Winner { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100), Required]
        [Index]
        public string ContractNumber { get; set; }

        [Index]
        public long LotPrice { get; set; }

        [Index]
        public long DocumentPrice { get; set; }

        [ForeignKey("LotPriceType"), Required]
        public int LotPriceTypeId { get; set; }
        virtual public LotPriceType LotPriceType { get; set; }

        [Column(TypeName = "varchar"), MaxLength(300), Required]
        [Index]
        public string OrderHref { get; set; }

        [Index, Required]
        public DateTime DocumentDateTime { get; set; }

        [Index, Required]
        public DateTime CreateDateTime { get; set; }

        public Lot()
        {

        }
    }



    // П О Б Е Д И Т Е Л Ь
    public class Winner : INotifyPropertyChanged
    {
        private ObservableCollection<WinnerNote> _winnerNotes;
        private bool _isChoosen;
        private short _rating;

        [Key]
        public long WinnerId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(400), Required]
        [Index]
        public string Name { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100)]
        [Index]
        public string Vatin { get; set; }

        [Column(TypeName = "varchar"), MaxLength(500), Required]
        [Index]
        public string Address { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100)]
        [Index]
        public string Email { get; set; }

        [Column(TypeName = "varchar"), MaxLength(50)]
        [Index]
        public string Phone { get; set; }

        private ObservableCollection<Lot> _lots;
        public virtual ObservableCollection<Lot> Lots
        {
            get { return _lots ?? (_lots = new ObservableCollection<Lot>(new HashSet<Lot>())); } // Try HashSet<N>
            set { _lots = value; }
        }

        [NotMapped]
        public bool IsChoosen
        {
            get { return _isChoosen; }
            set
            {
                _isChoosen = value;
                OnPropertyChanged();
            }
        }

        [ForeignKey("WinnerStatus")]
        public short? WinnerStatusId { get; set; }
        virtual public WinnerStatus WinnerStatus { get; set; }


        [ForeignKey("User")]
        public int? UserId { get; set; }
        virtual public User User { get; set; }



        public virtual ObservableCollection<WinnerNote> WinnerNotes
        {
            get { return _winnerNotes ?? (_winnerNotes = new ObservableCollection<WinnerNote>(new HashSet<WinnerNote>())); }
            set
            {
                _winnerNotes = value;
                OnPropertyChanged();
            }
        } //поедители

        public short Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
                OnPropertyChanged();
            }
        }

        public Winner()
        {
            IsChoosen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // С Т А Т У С   П О Б Е Д И Т Е Л Я
    public class WinnerStatus : INotifyPropertyChanged
    {
        private string _name;
        private ObservableCollection<Winner> _winners;

        [Key]
        public short WinnerStatusId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256), Required]
        [Index]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public virtual ObservableCollection<Winner> Winners
        {
            get { return _winners ?? (_winners = new ObservableCollection<Winner>(new HashSet<Winner>())); }
            set
            {
                _winners = value;
                OnPropertyChanged();
            }
        } //поедители


        public WinnerStatus()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // З А М Е Т К А   Н А   П О Б Е Д И Т Е Л Я
    public class WinnerNote : INotifyPropertyChanged
    {
        private string _name;
        private string _text;
        private DateTime _alertDateTime;

        [Key]
        public short WinnerNoteId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(256), Required]
        [Index]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        [ForeignKey("Winner")]
        public long WinnerId { get; set; }
        virtual public Winner Winner { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        [Index, Required, Column(TypeName = "datetime2")]
        public DateTime CreateDateTime { get; set; }

        [Column(TypeName = "varchar"), MaxLength(120)]
        [Index]
        public string UserName { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime AlertDateTime
        {
            get { return _alertDateTime; }
            set { _alertDateTime = value;
                OnPropertyChanged(); }
        }

        public WinnerNote()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
