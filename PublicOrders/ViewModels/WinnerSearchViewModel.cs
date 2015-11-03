using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PublicOrders.Annotations;
using PublicOrders.Commands;
using System.Windows;
using PublicOrders.Processors;
using PublicOrders.Models;
using PublicOrders.Processors.Main;
using PublicOrders.Processors.Documents.Main;

namespace PublicOrders.ViewModels
{
    public class WinnerSearchViewModel : INotifyPropertyChanged
    {
        #region Переменные
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private Customer _selectedCustomer = null;
        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged("SelectedCustomer");
            }
        }

        private string _searchInput;
        public string SearchInput
        {
            get { return _searchInput; }
            set
            {
                _searchInput = value;
                OnPropertyChanged("SearchInput");
            }
        }

        private bool _isCustomersSearching;
        public bool IsCustomersSearching
        {
            get { return _isCustomersSearching; }
            set
            {
                _isCustomersSearching = value;
                OnPropertyChanged("IsCustomersSearching");
            }
        }

        private bool _isWinnerLotsSearching;
        public bool IsWinnerLotsSearching
        {
            get { return _isWinnerLotsSearching; }
            set
            {
                _isWinnerLotsSearching = value;
                OnPropertyChanged("IsWinnerLotsSearching");
            }
        }

        private bool _isWinnerLotsSearchingPause;
        public bool IsWinnerLotsSearchingPause
        {
            get { return _isWinnerLotsSearchingPause; }
            set
            {
                _isWinnerLotsSearchingPause = value;
                OnPropertyChanged("IsWinnerLotsSearchingPause");
            }
        }

        private bool _reportCreating;
        public bool ReportCreating
        {
            get { return _reportCreating; }
            set
            {
                _reportCreating = value;
                OnPropertyChanged("ReportCreating");
            }
        }

        // Число
        private int _searchingProgress;
        public int SearchingProgress
        {
            get { return _searchingProgress; }
            set
            {
                _searchingProgress = value;
                OnPropertyChanged("SearchingProgress");
            }
        }

        // Текст в прогрессе
        private string _searchingProgressText;
        public string SearchingProgressText
        {
            get { return _searchingProgressText; }
            set
            {
                _searchingProgressText = value;
                OnPropertyChanged("SearchingProgressText");
            }
        }
        #endregion

        #region Коллекции
        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get
            {
                return _customers;
            }
            set
            {
                _customers = value;
                OnPropertyChanged("Customers");
            }
        }

        private ObservableCollection<Winner> _winners;
        public ObservableCollection<Winner> Winners
        {
            get
            {
                return _winners;
            }
            set
            {
                _winners = value;
                OnPropertyChanged("Winners");
            }
        }
        #endregion

        #region Команды
        private DelegateCommand customersSearchCommand;
        private DelegateCommand customersSearchStopCommand;
        private DelegateCommand winnerLotsSearchPausePlayCommand;
        private DelegateCommand winnerLotsSearchCommand;
        private DelegateCommand winnerLotsSearchStopCommand;
        private DelegateCommand createReportCommand;
        private DelegateCommand createReportStopCommand;
        public ICommand CustomersSearchCommand
        {
            get
            {
                if (customersSearchCommand == null)
                {
                    customersSearchCommand = new DelegateCommand(CustomersSearch);
                }
                return customersSearchCommand;
            }
        }


        public ICommand WinnerLotsSearchCommand
        {
            get
            {
                if (winnerLotsSearchCommand == null)
                {
                    winnerLotsSearchCommand = new DelegateCommand(WinnerLotsSearch);
                }
                return winnerLotsSearchCommand;
            }
        }

        public ICommand WinnerLotsSearchStopCommand
        {
            get
            {
                if (winnerLotsSearchStopCommand == null)
                {
                    winnerLotsSearchStopCommand = new DelegateCommand(WinnerLotsSearchStop);
                }
                return winnerLotsSearchStopCommand;
            }
        }

        public ICommand WinnerLotsSearchPausePlayCommand
        {
            get
            {
                if (winnerLotsSearchPausePlayCommand == null)
                {
                    winnerLotsSearchPausePlayCommand = new DelegateCommand(WinnerLotsSearchPausePlay);
                }
                return winnerLotsSearchPausePlayCommand;
            }
        }

        public ICommand CreateReportCommand
        {
            get
            {
                if (createReportCommand == null)
                {
                    createReportCommand = new DelegateCommand(CreateReport);
                }
                return createReportCommand;
            }
        }

        public ICommand CreateReportStopCommand
        {
            get
            {
                if (createReportStopCommand == null)
                {
                    createReportStopCommand = new DelegateCommand(CreateReportStop);
                }
                return createReportStopCommand;
            }
        }
        #endregion

        #region Методы
        private void CustomersSearch()
        {
            // Очищаем список заказчиков
            if (Customers != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Customers.Clear();
                }));
            }
            else
            {
                Customers = new ObservableCollection<Customer>();
            }

            // Очищаем список победителей
            if (Winners != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Winners.Clear();
                }));
            }
            else
            {
                Winners = new ObservableCollection<Winner>();
            }

            // Останавливаем поиск победителей
            if ((mvm.lsProcessor != null) && (mvm.lsProcessor.isWorking()))
            {
                mvm.lsProcessor.Stop();
            }

            // Останавливаем поиск заказчиков
            if ((mvm.csProcessor != null) && (mvm.csProcessor.isWorking()))
            {
                mvm.csProcessor.Stop();
            }

            SearchingProgressText = "Поиск заказчиков..";
            SearchingProgress = 0;

            // Переводим значения из Properties в enum
            // CustomerType
            CustomerType_enum customerType_enum;
            switch (Properties.Settings.Default.CustomerType.Trim().ToLower()) {
                case ("customer"):
                    customerType_enum = CustomerType_enum.Customer;
                    break;
                case ("organization"):
                    customerType_enum = CustomerType_enum.Organization;
                    break;
                default:
                    customerType_enum = CustomerType_enum.Customer;
                    break;
            }

            // LawType
            LawType_enum lawType_enum;
            switch (Properties.Settings.Default.LawType.Trim().ToLower())
            {
                case ("_44_94_223"):
                    lawType_enum = LawType_enum._44_94_223;
                    break;
                case ("_44_94"):
                    lawType_enum = LawType_enum._44_94;
                    break;
                case ("_223"):
                    lawType_enum = LawType_enum._223;
                    break;
                default:
                    lawType_enum = LawType_enum._44_94_223;
                    break;
            }

            CustomersSearchDone_delegate customerSearchDone_delege = new CustomersSearchDone_delegate(CustomersSearchDone_proc);
            mvm.csProcessor = new CustomersSearchProcessor(SearchInput,
                                                           customerType_enum,
                                                           Properties.Settings.Default.MinPrice,
                                                           Properties.Settings.Default.MaxPrice,
                                                           Properties.Settings.Default.CustomerCity,
                                                           Properties.Settings.Default.MinPublicDate,
                                                           Properties.Settings.Default.MaxPublicDate,
                                                           lawType_enum, 
                                                           customerSearchDone_delege);
            mvm.csProcessor.Operate();

            IsCustomersSearching = true;
            currentCustomerSearching = "";
        }



        private void CustomersSearchDone_proc(ObservableCollection<Customer> serchedCustomers, ResultType_enum resultSearch, string message)
        {
            if (resultSearch == ResultType_enum.ErrorNetwork) {
                MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else {
                if (serchedCustomers != null)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Customers = serchedCustomers;
                    }));
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchingProgress = 0;
                SearchingProgressText = "";
                IsCustomersSearching = false;
            }));



            //MessageBox.Show("Поиск заказчиков завершен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            //WinnerLotsSearch();
        }

        private string currentCustomerSearching = "";
        private void WinnerLotsSearch() {
            if (SelectedCustomer != null)
            {
                if (SelectedCustomer.Name == currentCustomerSearching) return;
                currentCustomerSearching = SelectedCustomer.Name;
            }

            if (Winners != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Winners.Clear();
                }));
            }
            else {
                Winners = new ObservableCollection<Winner>();
            }


            if (SelectedCustomer == null) {
                MessageBox.Show("Выберите заказчика!", "Информация", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if ((mvm.lsProcessor != null) && (mvm.lsProcessor.isWorking()))
            {
                mvm.lsProcessor.Stop();
            }


            // Переводим значения из Properties в enum
            // CustomerType
            CustomerType_enum customerType_enum;
            switch (Properties.Settings.Default.CustomerType.Trim().ToLower())
            {
                case ("customer"):
                    customerType_enum = CustomerType_enum.Customer;
                    break;
                case ("organization"):
                    customerType_enum = CustomerType_enum.Organization;
                    break;
                default:
                    customerType_enum = CustomerType_enum.Customer;
                    break;
            }

            // LawType
            LawType_enum lawType_enum;
            switch (Properties.Settings.Default.LawType.Trim().ToLower())
            {
                case ("_44_94_223"):
                    lawType_enum = LawType_enum._44_94_223;
                    break;
                case ("_44_94"):
                    lawType_enum = LawType_enum._44_94;
                    break;
                case ("_223"):
                    lawType_enum = LawType_enum._223;
                    break;
                default:
                    lawType_enum = LawType_enum._44_94_223;
                    break;
            }

            AllLotsSearched_delegete allLotsSearched_delegete = new AllLotsSearched_delegete(AllLotsSearched_proc);
            LotSearched_delegate lotSearched_delegate = new LotSearched_delegate(LotSearched_proc);
            LotSearchProgress_delegate lotSearchProgress_delegate = new LotSearchProgress_delegate(LotSearchProgress_proc);
            mvm.lsProcessor = new LotsSearchProcessor(SelectedCustomer,
                                                      customerType_enum,
                                                      lawType_enum,
                                                      Properties.Settings.Default.MinPrice,
                                                      Properties.Settings.Default.MaxPrice,
                                                      Properties.Settings.Default.MinPublicDate,
                                                      Properties.Settings.Default.MaxPublicDate,
                                                      lotSearched_delegate,
                                                      allLotsSearched_delegete,
                                                      lotSearchProgress_delegate
                                                      );
            mvm.lsProcessor.Operate();

            IsWinnerLotsSearchingPause = false;
            IsWinnerLotsSearching = true;
        }

        private void LotSearchProgress_proc(Customer customer, string text, int intValue) {
            if (customer != SelectedCustomer) return;
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchingProgressText = text;
                    SearchingProgress = intValue;
                }));
            }
            catch {

            }
        }

        private void AllLotsSearched_proc(Customer customer, ResultType_enum resultType_enum, string message) {
            if (customer != SelectedCustomer) return;
            if (resultType_enum == ResultType_enum.ErrorNetwork)
            {
                MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    IsWinnerLotsSearching = false;
                    SearchingProgressText = "";
                    SearchingProgress = 0;
                }));
            }
            catch {

            }
            //MessageBox.Show("Поиск завершен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LotSearched_proc(Winner winner) {
            if (IsCustomersSearching) return;
            if (winner.Name != "")
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Winners.Add(winner);
                }));
        }

        private void WinnerLotsSearchStop() {
            IsWinnerLotsSearchingPause = false;
            mvm.lsProcessor.Stop();
        }

        private void WinnerLotsSearchPausePlay()
        {
            IsWinnerLotsSearchingPause = !IsWinnerLotsSearchingPause;
            mvm.lsProcessor.PausePlay();
        }

        private void CreateReport()
        {
            ReportCreating = true;
            if ((mvm.cwProcessor != null) && (mvm.cwProcessor.isWorking()))
            {
                mvm.cwProcessor.Stop();
            }

            CreateWinnersDocumentDone_delegete createWinnersDocumentDone_delegete = new CreateWinnersDocumentDone_delegete(CreateWinnersDocumentDone_proc);
            mvm.cwProcessor = new CreateWinnersDocProcessor (Winners.ToList(), createWinnersDocumentDone_delegete);
            mvm.cwProcessor.Operate();
        }

        private void CreateReportStop()
        {
            mvm.cwProcessor.Stop();
        }

        private void CreateWinnersDocumentDone_proc(ResultType_enum resultCreate, string message) {
            ReportCreating = false;
            MessageBox.Show("Документ создан!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion



        public WinnerSearchViewModel()
        {
            IsWinnerLotsSearchingPause = false;
            //Winners = new ObservableCollection<object>(база);

            Customers = new ObservableCollection<Customer>();
            Customers.Add(new Customer() { Name = "wsefasgfsdg" });
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
