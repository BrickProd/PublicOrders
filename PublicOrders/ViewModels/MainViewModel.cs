using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;
using PublicOrders.Processors.Main;
using PublicOrders.Processors.Documents.Main;
using System.Linq;

namespace PublicOrders
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Документы
        public DocumentDbContext dc { get; set; }
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
        // Документы
        public CreateDocumentProcessor cdProcessor = null;
        public LoadProductsProcessor lpProcessor = null;
        public CreateWinnersDocProcessor cwProcessor = null;
        // Победители
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
            ProductCollection = new ObservableCollection<Product>(dc.Products);

            // Победители
            wc = new WinnersDbContext();

            CheckProductsRepetition();
        }
		
        public void RefreshProducts()
        {
            this.ProductCollection = new ObservableCollection<Product>(dc.Products);
        }

        public void CheckProductsRepetition()
        {
            this.ProductCollection.ToList().ForEach(m => {
                m.IsRepetition = ProductCollection.Where(p => p.Name == m.Name).Count() > 1;
            });      
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