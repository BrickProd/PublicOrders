using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;
using PublicOrders.Processors.Main;

namespace PublicOrders
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Документы
        public DocumentDbContext dc { get; set; }
        private ObservableCollection<Template> _templates;
        public ObservableCollection<Template> TemplateCollection
        {
            get
            {
                return _templates;
            }
            set
            {
                _templates = value;
                OnPropertyChanged("Templates");
            }

        }
        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> ProductCollection
        {
            get
            {
                return _products;
            }
            set
            {
                _products = value;
                OnPropertyChanged("ProductsCollection");
            }

        }
        #endregion

        #region Победители
        public WinnersDbContext wc { get; set; }
        //public ObservableCollection<Customer> CustomerCollection { get; set; }
        #endregion

        #region Процессоры
        public CreateDocumentProcessor cdProcessor = null;
        public LoadProductsProcessor lpProcessor = null;
        public CustomersSearchProcessor csProcessor = null;
        public LotsSearchProcessor lsProcessor = null;
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
            //CustomerCollection = new ObservableCollection<Customer>(wc.Customers);
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