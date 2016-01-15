using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;
using PublicOrders.Processors.Main;
using PublicOrders.Processors.Documents.Main;
using System.Linq;
using System.Collections.Generic;
using PublicOrders.Processors.Internet;

namespace PublicOrders
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public bool SecretTabIsEnabled { get
            {
                return this.currentUserStatus.StatusName.ToLower() == "admin";
            }
        }

        // Глобальные коллекции
        #region Глобальные коллекции
        // Товар
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

        // Инструкции
        private ObservableCollection<Instruction> _instructions;
        public ObservableCollection<Instruction> InstructionCollection
        {
            get
            {
                return _instructions;
            }
            set
            {
                _instructions = value;
                OnPropertyChanged("InstructionCollection");
            }
        }

        // Рубрики
        private ObservableCollection<Rubric> _rubrics;
        public ObservableCollection<Rubric> RubricCollection
        {
            get
            {
                return _rubrics;
            }
            set
            {
                _rubrics = value;
                OnPropertyChanged("RubricCollection");
            }
        }

        // Шаблоны
        private ObservableCollection<string> _templates;
        public ObservableCollection<string> TemplateCollection
        {
            get
            {
                return _templates;
            }
            set
            {
                _templates = value;
                OnPropertyChanged("TemplateCollection");
            }
        }
        #endregion


        #region Документы
        public DocumentDbContext dc { get; set; }
        #endregion

        #region Победители
        public WinnersDbContext wc { get; set; }
        #endregion

        #region Пользователи
        private UserStatus _currentUserStatus;
        public UserStatus currentUserStatus { get
            {
                return _currentUserStatus;
            }
            set {
                _currentUserStatus = value;
                OnPropertyChanged("currentUserStatus");
            }
        }
        #endregion

        #region Процессоры
        // Документы
        public CreateDocumentProcessor cdProcessor = null;
        public LoadProductsProcessor lpProcessor = null;
        public CreateWinnersDocProcessor cwProcessor = null;
        // Победители
        public CustomersSearchProcessor csProcessor = null;
        public WinnersSearchProcessor wsProcessor = null;
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
            this.ProductCollection = new ObservableCollection<Product>(dc.Products);
            this.RubricCollection = new ObservableCollection<Rubric>(dc.Rubrics);
            this.InstructionCollection = new ObservableCollection<Instruction>(dc.Instructions);
            this.TemplateCollection = new ObservableCollection<string>(new List<string> {
                        "Комитет",
                        "Свобода",
                        "Форма 2"
                    });

            // Победители
            wc = new WinnersDbContext();

            CheckProductsRepetition();
        }

        public void RefreshRubrics() {
            this.RubricCollection = new ObservableCollection<Rubric>(dc.Rubrics);
        }

        public void RefreshInstructions()
        {
            this.InstructionCollection = new ObservableCollection<Instruction>(dc.Instructions);
        }

        public void RefreshProducts()
        {
            this.ProductCollection = new ObservableCollection<Product>(dc.Products);
        }

        public void CheckProductsRepetition()
        {
            this.ProductCollection.ToList().ForEach(m => {
                m.IsRepetition = ProductCollection.Where(p => ((p.Name == m.Name) && (p.TradeMark == m.TradeMark))).Count() > 1;
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