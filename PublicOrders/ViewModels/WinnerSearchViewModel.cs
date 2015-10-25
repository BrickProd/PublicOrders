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

namespace PublicOrders.ViewModels
{
    public class WinnerSearchViewModel : INotifyPropertyChanged
    {
        #region Переменные
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

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

        private ObservableCollection<Lot> _winnerLots;
        public ObservableCollection<Lot> WinnerLots
        {
            get
            {
                return _winnerLots;
            }
            set
            {
                _winnerLots = value;
                OnPropertyChanged("WinnerLots");
            }
        }
        #endregion

        #region Команды
        private DelegateCommand customersSearchCommand;
        private DelegateCommand customersSearchStopCommand;
        private DelegateCommand winnerLotsSearchCommand;
        private DelegateCommand winnerLotsSearchStopCommand;
        private DelegateCommand createReportCommand;
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

        public ICommand CustomersSearchStopCommand
        {
            get
            {
                if (customersSearchStopCommand == null)
                {
                    customersSearchStopCommand = new DelegateCommand(CustomersSearchStop);
                }
                return customersSearchStopCommand;
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
        #endregion

        #region Методы
        private void CustomersSearch()
        {


            IsCustomersSearching = true;

            if ((mvm.csProcessor != null) && (mvm.csProcessor.isWorking()))
            {
                mvm.csProcessor.Stop();
            }

            CustomersSearchDone_delegate customerSearchDone_delege = new CustomersSearchDone_delegate(CustomersSearchDone_proc);
            mvm.csProcessor = new CustomersSearchProcessor(SearchInput, 
                                                           CustomerType_enum.Customer, 
                                                           100, 
                                                           100000000, 
                                                           "", 
                                                           Convert.ToDateTime("2010.01.01"), 
                                                           DateTime.Now, 
                                                           LawType_enum._44_94_223, 
                                                           customerSearchDone_delege, 
                                                           SearchingProgress, 
                                                           Customers);
            mvm.csProcessor.Operate();
        }



        private void CustomersSearchStop()
        {
            //метод
        }

        private void CustomersSearchDone_proc(ResultType_enum resultSearch, string message)
        {
            SearchingProgress = 0;
            IsCustomersSearching = false;
            MessageBox.Show("Поиск завершен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WinnerLotsSearch() {

        }

        private void WinnerLotsSearchStop() {

        }

        private void CreateReport()
        {
            //метод
        }
        #endregion

        public WinnerSearchViewModel()
        {
            //Winners = new ObservableCollection<object>(база);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
