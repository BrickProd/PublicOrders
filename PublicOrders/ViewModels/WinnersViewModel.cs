using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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
using System.Windows;
using PublicOrders.Processors.Internet;

namespace PublicOrders.ViewModels
{
    public class WinnersViewModel : INotifyPropertyChanged
    {
        public UserStatus CurentStatus { get; set; }

        private ObservableCollection<Winner> _winners;
        private Winner _selectedWinner;

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
                GetWinnerActivity(null);
            }
        }

        public WinnerNote SelectedNote
        {
            get { return _selectedNote; }
            set
            {
                _selectedNote = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<User> ClientUsers { get; set; }

        //private DelegateCommand _deleteWinnerCommand;
        //public ICommand DeleteWinnerCommand
        //{
        //    get
        //    {
        //        if (_deleteWinnerCommand == null)
        //        {
        //            _deleteWinnerCommand = new DelegateCommand(DeleteWinner);
        //        }
        //        return _deleteWinnerCommand;
        //    }
        //}

        private DelegateCommand _addNoteCommand;
        public ICommand AddNoteCommand
        {
            get
            {
                if (_addNoteCommand == null)
                {
                    _addNoteCommand = new DelegateCommand(AddNote);
                }
                return _addNoteCommand;
            }
        }

        private DelegateCommand _deleteNoteCommand;
        public ICommand DeleteNoteCommand
        {
            get
            {
                if (_deleteNoteCommand == null)
                {
                    _deleteNoteCommand = new DelegateCommand(DeleteNote);
                }
                return _deleteNoteCommand;
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

        private DelegateCommand _saveNoteCommand;
        private WinnerNote _selectedNote;

        public ICommand SaveNoteCommand
        {
            get
            {
                if (_saveNoteCommand == null)
                {
                    _saveNoteCommand = new DelegateCommand(SaveNote);
                }
                return _saveNoteCommand;
            }
        }

        private DelegateCommand _getWinnerActivityCommand;
        private ObservableCollection<WinnerActivity> _winnerActivities;
        private int _maxAxis;

        public ICommand GetWinnerActivityCommand
        {
            get
            {
                if (_getWinnerActivityCommand == null)
                {
                    _getWinnerActivityCommand = new DelegateCommand(GetWinnerActivity);
                }
                return _getWinnerActivityCommand;
            }
        }

        public ObservableCollection<WinnerActivity> WinnerActivities
        {
            get { return _winnerActivities; }
            set
            {
                _winnerActivities = value;
                OnPropertyChanged();
            }
        }

        public int MaxAxis
        {
            get { return _maxAxis; }
            set
            {
                _maxAxis = value;
                OnPropertyChanged();
            }
        }

        public void GetWinnerActivity(object param)
        {
            WinnerActivities.Clear();
            //WinnerActivities.Add(new WinnerActivity() { Date = DateTime.Now, Value = 300});
            //WinnerActivities.Add(new WinnerActivity() { Date = DateTime.Now.AddMonths(2), Value = 130 });
            //WinnerActivities.Add(new WinnerActivity() { Date = DateTime.Now.AddMonths(3), Value = 240 });
            //WinnerActivities.Add(new WinnerActivity() { Date = DateTime.Now.AddMonths(5), Value = 30 });

            WinnerDatesSearched_delegete wds_delegate = new WinnerDatesSearched_delegete(ActivityReady_proc);
            WinnerActiveProcessor proc = new WinnerActiveProcessor(wds_delegate);
            proc.OperateWinDates(SelectedWinner.Name);
        }

        private void ActivityReady_proc(List<DateTime> dates, ResultType_enum resultType_enum, string message)
        {
            DateTime i = DateTime.Now.AddYears(-10);
            while (i.Date.ToString("yy-MM")!=DateTime.Now.ToString("yy-MM"))
            {
                WinnerActivities.Add(new WinnerActivity() {Date = i, Value = dates.Count(m => m.Year == i.Year && m.Month == i.Month)});
                i = i.AddMonths(1);
            }

            MaxAxis = WinnerActivities.Max(m => m.Value);

        }

        public WinnersViewModel()
        {
            //Winners = new ObservableCollection<Winner>(DataService.WinnersDbContext.Winners);
            //Winners = new ObservableCollection<Winner>(new List<Winner>() { new Winner() {Name = "Jnbbui", WinnerStatus = DataService.WinnersDbContext.WinnerStatuses.Find(1), Rating = 2} });
            //Winners = DataService.Winners;

            WinnerStatuses = DataService.WinnerStatuses;

            var isAdmin = DataService.CurrentUser.UserStatus.UserStatusId == 1;

            ToView = new CollectionViewSource();
            ToView.Source = DataService.Winners;
            ToView.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;

                var status = w.WinnerStatus == DataService.Context.WinnerStatuses.Find(1);
                args.Accepted = isAdmin ?  status: w.User?.Login == DataService.CurrentUser.Login && status;
            };

            Favorites = new CollectionViewSource();
            Favorites.Source = DataService.Winners;
            Favorites.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;

                var status = w.WinnerStatus == DataService.Context.WinnerStatuses.Find(2);
                args.Accepted = isAdmin ? status : w.User?.Login == DataService.CurrentUser.Login && status;
            };

            BlackList = new CollectionViewSource();
            BlackList.Source = DataService.Winners;
            BlackList.Filter += (sender, args) =>
            {
                Winner w = args.Item as Winner;

                var status = w.WinnerStatus == DataService.Context.WinnerStatuses.Find(3);
                args.Accepted = isAdmin ? status : w.User?.Login == DataService.CurrentUser.Login && status;
            };

            ClientUsers = DataService.ClientUsers;

            CurentStatus = DataService.CurrentUser.UserStatus;



            WinnerActivities = new ObservableCollection<WinnerActivity>();



        }






        public void RefreshList(object param)
        {
            DataService.UpdateWinnerContext();
            //DataService.UpdateNotesContext();

            ToView.View.Refresh();
            Favorites.View.Refresh();

            BlackList.View.Refresh();
        }

        public void AddNote(object param)
        {
            var newNote = new WinnerNote
            {
                CreateDateTime = DateTime.Now,
                Name = "новая заметка",
                AlertDateTime = DateTime.Now,
                UserName = Properties.Settings.Default.UserName,
            };

            SelectedWinner.WinnerNotes.Add(newNote);
            SelectedNote = newNote;

            DataService.Context.SaveChanges();
        }

        public void DeleteNote(object param)
        {
            DataService.Context.WinnerNotes.Remove(SelectedNote);
            DataService.Context.SaveChanges();
        }
        public void SaveNote(object param)
        {

            DataService.Context.Entry(SelectedNote).State = EntityState.Modified;
            DataService.Context.SaveChanges();
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
