using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;
using PublicOrders.Processors.Main;

namespace PublicOrders
{
    public enum CustomerType
    {
        Customer = 0,
        Organization = 1
    }
    public enum LawType
    {
        _44 = 0,
        _94 = 1,
        _223 = 2,
        _44_94 = 3,
        _44_94_223 = 4,
        None = 5
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Документы
        public DocumentDbContext dc { get; set; }
        public ObservableCollection<Template> TemplateCollection {get;set;}
        public ObservableCollection<Product> ProductCollection { get; set; }
        #endregion

        #region Победители
        public WinnersDbContext wc { get; set; }
        public ObservableCollection<Customer> CustomerCollection { get; set; }
        #endregion

        #region Процессоры
        public CreateDocumentProcessor cdProcessor = null;
        public LoadProductsProcessor lpProcessor = null;
        #endregion

        #region Фильтр
        // Тип заказчика
        private CustomerType _customerTypeFilter;
        public CustomerType CustomerTypeFilter
        {
            get { return _customerTypeFilter; }
            set
            {
                _customerTypeFilter = value;
                OnPropertyChanged("CustomerTypeFilter");
            }
        }
        // Минимальная цена
        private long _priceBegFilter;
        public long PriceBegFilter
        {
            get { return _priceBegFilter; }
            set
            {
                _priceBegFilter = value;
                OnPropertyChanged("PriceBegFilter");
            }
        }

        // Максимальная цена
        private long _priceEndFilter;
        public long PriceEndFilter
        {
            get { return _priceEndFilter; }
            set
            {
                _priceEndFilter = value;
                OnPropertyChanged("PriceEndFilter");
            }
        }

        // Адрес
        private string _addressFilter;
        public string AddressFilter
        {
            get { return _addressFilter; }
            set
            {
                _addressFilter = value;
                OnPropertyChanged("AddressFilter");
            }
        }

        // Дата публикации начало
        private DateTime _publishDateBegFilter;
        public DateTime PublishDateBegFilter
        {
            get { return _publishDateBegFilter; }
            set
            {
                _publishDateBegFilter = value;
                OnPropertyChanged("PublishDateBegFilter");
            }
        }

        // Дата публикации конец
        private DateTime _publishDateEndFilter;
        public DateTime PublishDateEndFilter
        {
            get { return _publishDateEndFilter; }
            set
            {
                _publishDateEndFilter = value;
                OnPropertyChanged("PublishDateEndFilter");
            }
        }

        // Федеральный закон
        private LawType _lawTypeFilter;
        public LawType LawTypeFilter
        {
            get { return _lawTypeFilter; }
            set
            {
                _lawTypeFilter = value;
                OnPropertyChanged("LawTypeFilter");
            }
        }
        #endregion

        public MainViewModel()
		{
            // Документы
            dc = new DocumentDbContext();
            TemplateCollection = new ObservableCollection<Template>(dc.Templates);
            ProductCollection = new ObservableCollection<Product>(dc.Products);

            // Победители
            wc = new WinnersDbContext();
            CustomerCollection = new ObservableCollection<Customer>(wc.Customers);
        }
		
		
		
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

	}
}