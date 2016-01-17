using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using PublicOrders.Annotations;
using PublicOrders.Commands;
using PublicOrders.Models;
using PublicOrders.Data;

namespace PublicOrders.ViewModels
{
    public class WinnersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Winner> _winners;
        private Winner _selectedWinner;


        public ObservableCollection<Winner> Winners
        {
            get { return _winners; }
            set
            {
                _winners = value;
                OnPropertyChanged();
            }
        }

        public CollectionViewSource ToView { get; set; }
        public CollectionViewSource Favorites { get; set; }
        public CollectionViewSource BlackList { get; set; }

        public ObservableCollection<WinnerStatus> WinnerStatuses { get; set; } 

        public Winner SelectedWinner
        {
            get { return _selectedWinner; }
            set
            {
                _selectedWinner = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get
            {
                if (_refreshListCommand == null)
                {
                    _refreshListCommand = new DelegateCommand(RefreshList);
                }
                return _refreshListCommand;
            }
        }


        public WinnersViewModel()
        {
            Winners = new ObservableCollection<Winner>(DataService.WinnersDbContext.Winners);
            //Winners = new ObservableCollection<Winner>(new List<Winner>() { new Winner() {Name = "Jnbbui", WinnerStatus = DataService.WinnersDbContext.WinnerStatuses.Find(1), Rating = 2} });

            WinnerStatuses = new ObservableCollection<WinnerStatus>(DataService.WinnersDbContext.WinnerStatuses);

            ToView = new CollectionViewSource();
            ToView.Source = Winners;
            ToView.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;
                args.Accepted = w.WinnerStatus == DataService.WinnersDbContext.WinnerStatuses.Find(1);
            };

            Favorites = new CollectionViewSource();
            Favorites.Source = Winners;
            Favorites.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;
                args.Accepted = w.WinnerStatus == DataService.WinnersDbContext.WinnerStatuses.Find(2);
            };

            BlackList = new CollectionViewSource();
            BlackList.Source = Winners;
            BlackList.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;
                args.Accepted = w.WinnerStatus == DataService.WinnersDbContext.WinnerStatuses.Find(3);
            };
        }

        public void RefreshList(object param)
        {
            Winners = new ObservableCollection<Winner>(DataService.WinnersDbContext.Winners);

            ToView.Source = Winners;
            ToView.View.Refresh();

            Favorites.Source = Winners;
            Favorites.View.Refresh();

            BlackList.Source = Winners;
            BlackList.View.Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
