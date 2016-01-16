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
using PublicOrders.Data;
using PublicOrders.Processors;
using PublicOrders.Models;
using PublicOrders.Processors.Main;
using PublicOrders.Processors.Documents.Main;
using PublicOrders.Processors.Internet;

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

        private Lot _selectedLot = null;
        public Lot SelectedLot
        {
            get { return _selectedLot; }
            set
            {
                _selectedLot = value;
                OnPropertyChanged("SelectedLot");
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

        private ObservableCollection<Lot> _lots;
        public ObservableCollection<Lot> Lots
        {
            get
            {
                return _lots;
            }
            set
            {
                _lots = value;
                OnPropertyChanged("Lots");
            }
        }

        public ObservableCollection<WinnerStatus> WinnerStatuses
        {
            get { return _winnerStatuses; }
            set
            {
                _winnerStatuses = value;
                OnPropertyChanged();
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
        private void CustomersSearch(object param)
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
            if (Lots != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Lots.Clear();
                }));
            }
            else
            {
                Lots = new ObservableCollection<Lot>();
            }

            // Останавливаем поиск победителей
            if ((mvm.wsProcessor != null) && (mvm.wsProcessor.isWorking()))
            {
                mvm.wsProcessor.Stop();
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

            AllCustomersSearched_delegete allCustomersSearched_delegete = new AllCustomersSearched_delegete(AllCustomersSearched_proc);
            CustomerSearched_delegate customerSearched_delegate = new CustomerSearched_delegate(CustomerSearched_proc);
            CustomerSearchProgress_delegate customerSearchProgress_delegate = new CustomerSearchProgress_delegate(CustomerSearchProgress_proc);
            mvm.csProcessor = new CustomersSearchProcessor(SearchInput,
                                                           customerType_enum,
                                                           Properties.Settings.Default.MinPrice,
                                                           Properties.Settings.Default.MaxPrice,
                                                           Properties.Settings.Default.CustomerCity,
                                                           Properties.Settings.Default.MinPublicDate,
                                                           Properties.Settings.Default.MaxPublicDate,
                                                           lawType_enum,
                                                           allCustomersSearched_delegete,
                                                           customerSearched_delegate,
                                                           customerSearchProgress_delegate);
            mvm.csProcessor.Operate();

            IsCustomersSearching = true;
            currentCustomerSearching = "";
        }


        private void AllCustomersSearched_proc(ResultType_enum resultType_enum, string message) {
            if (resultType_enum != ResultType_enum.Done)
            {
                MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            /*else
            {
                if (serchedCustomers != null)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Customers = serchedCustomers;
                    }));
            }*/

            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchingProgress = 0;
                    SearchingProgressText = "";
                    IsCustomersSearching = false;
                }));
            }
            catch {

            }

        }

        private void CustomerSearched_proc(Customer customer) {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Customers.Add(customer);
                }));
            }
            catch {

            }

        }

        private void CustomerSearchProgress_proc(string text, int intValue) {
            /*try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SearchingProgressText = text;
                    SearchingProgress = intValue;
                }));
            }
            catch
            {

            }*/
        }


        private string currentCustomerSearching = "";
        private ObservableCollection<WinnerStatus> _winnerStatuses;

        private void WinnerLotsSearch(object param) {
            if (SelectedCustomer != null)
            {
                if (SelectedCustomer.Name == currentCustomerSearching) return;
                currentCustomerSearching = SelectedCustomer.Name;
            }

            if (Lots != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Lots.Clear();
                }));
            }
            else {
                Lots = new ObservableCollection<Lot>();
            }


            if (SelectedCustomer == null) {
                MessageBox.Show("Выберите заказчика!", "Информация", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if ((mvm.wsProcessor != null) && (mvm.wsProcessor.isWorking()))
            {
                mvm.wsProcessor.Stop();
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

            AllWinersSearched_delegete allWinersSearched_delegete = new AllWinersSearched_delegete(AllLotsSearched_proc);
            WinnerSearched_delegate winnerSearched_delegate = new WinnerSearched_delegate(LotSearched_proc);
            WinnerSearchProgress_delegate winnerSearchProgress_delegate = new WinnerSearchProgress_delegate(LotSearchProgress_proc);
            mvm.wsProcessor = new WinnersSearchProcessor(SelectedCustomer,
                                          customerType_enum,
                                          lawType_enum,
                                          Properties.Settings.Default.MinPrice,
                                          Properties.Settings.Default.MaxPrice,
                                          Properties.Settings.Default.MinPublicDate,
                                          Properties.Settings.Default.MaxPublicDate,
                                          allWinersSearched_delegete,
                                          winnerSearched_delegate,
                                          winnerSearchProgress_delegate
                                          );
            mvm.wsProcessor.Operate();


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

        private void LotSearched_proc(Lot lot) {
            if (IsCustomersSearching) return;
            try
            {
                if (lot.Winner.Name != "")
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Lots.Add(lot);
                    }));
            }
            catch {

            }

        }

        private void WinnerLotsSearchStop(object param) {
            IsWinnerLotsSearchingPause = false;
            mvm.wsProcessor.Stop();
        }

        private void WinnerLotsSearchPausePlay(object param)
        {
            IsWinnerLotsSearchingPause = !IsWinnerLotsSearchingPause;
            mvm.wsProcessor.PausePlay();
        }

        private void CreateReport(object param)
        {
            ReportCreating = true;
            if ((mvm.cwProcessor != null) && (mvm.cwProcessor.isWorking()))
            {
                mvm.cwProcessor.Stop();
            }

            CreateWinnersDocumentDone_delegete createWinnersDocumentDone_delegete = new CreateWinnersDocumentDone_delegete(CreateWinnersDocumentDone_proc);
            mvm.cwProcessor = new CreateWinnersDocProcessor (SelectedCustomer, Lots.Select(m=>m.Winner).Where(m=>m.IsChoosen).ToList(), createWinnersDocumentDone_delegete);
            mvm.cwProcessor.Operate();
        }

        private void CreateReportStop(object param)
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
            //Lots = new ObservableCollection<Lot>();
            Lots = new ObservableCollection<Lot>(new List<Lot>()
            {
                new Lot() { Name = "LotName", ContractNumber = "394030", DocumentPrice = 10, LotPrice=234, Winner = new Winner() {Name = "WinnerName", Address = "Address", Email = "mail", Phone = "phone", Vatin = "inn"}}
            });

            Customers = new ObservableCollection<Customer>();

            WinnerStatuses = new ObservableCollection<WinnerStatus>(DataService.WinnersDbContext.WinnerStatuses);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
