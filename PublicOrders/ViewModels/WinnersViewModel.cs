using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PublicOrders.Annotations;
using PublicOrders.Models;
using PublicOrders.Data;

namespace PublicOrders.ViewModels
{
    public class WinnersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Winner> _winners;
        private Winner _selectedWinner;


        private ObservableCollection<Winner> Winners
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

        public Winner SelectedWinner
        {
            get { return _selectedWinner; }
            set
            {
                _selectedWinner = value;
                OnPropertyChanged();
            }
        }

        public WinnersViewModel()
        {
            //Winners = new ObservableCollection<Winner>(DataService.WinnersDbContext.Winners);з-----------------------------------------------11111111111111111111
            Winners = new ObservableCollection<Winner>(new List<Winner>()
            {
                new Winner { Name = "Победитель 1" , Email = "sdsd", Phone = "23322", Rating = 0, WinnerStatus = DataService.WinnersDbContext.WinnerStatuses.Find(1) }
            });


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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
