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

namespace PublicOrders.ViewModels
{
    public class WinnerSearchViewModel : INotifyPropertyChanged
    {
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

        private bool _isWinnerSearching;
        public bool IsWinnerSearching
        {
            get { return _isWinnerSearching; }
            set
            {
                _isWinnerSearching = value;
                OnPropertyChanged("IsWinnerSearching");
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

        public ObservableCollection<object> Winners { get; set; }

        #region КОМАНДЫ
        private DelegateCommand searchCommand;
        private DelegateCommand searchStopCommand;
        private DelegateCommand createReportCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                {
                    searchCommand = new DelegateCommand(Search);
                }
                return searchCommand;
            }
        }

        public ICommand SearchStopCommand
        {
            get
            {
                if (searchStopCommand == null)
                {
                    searchStopCommand = new DelegateCommand(SearchStop);
                }
                return searchStopCommand;
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



        private void Search()
        {
            /*SearchDone_delegete searchDone_delege = new SearchDone_delegete(SearchDone_proc);
            LotSearched_delegate lotSearched_delegate = new LotSearched_delegate(LotSearched_proc);
            CustomerCkeck_delegate customerCkeck_delegate = new CustomerCkeck_delegate(CustomerCkeck_proc);*/
        }

        private void SearchDone_proc(string message)
        {
            SearchingProgress = 0;
            IsWinnerSearching = false;
            MessageBox.Show("Поиск завершен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchStop()
        {
            //метод
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
