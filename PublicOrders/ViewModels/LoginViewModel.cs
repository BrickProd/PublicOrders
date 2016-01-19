using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PublicOrders.Commands;
using PublicOrders.Models;
using System.Linq;
using PublicOrders.Data;

namespace PublicOrders.ViewModels
{
	public class LoginViewModel
	{
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        //private AutenDbContext adc = null;

        public string UserStr { get; set; }
        public string Password { get; set; }

        #region КОМАНДЫ
        private DelegateCommand enterCommand;
        public ICommand EnterCommand
        {
            get
            {
                if (enterCommand == null)
                {
                    enterCommand = new DelegateCommand(Enter);
                }
                return enterCommand;
            }
        }

        private DelegateCommand exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                {
                    exitCommand = new DelegateCommand(Exit);
                }
                return exitCommand;
            }
        }
        #endregion

        #region МЕТОДЫ
        private void Enter(object param)
        {
            if (UserStr.Trim() == "") {
                MessageBox.Show("Введите логин!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if ((Password == null) || (Password.Trim() == ""))
            {
                MessageBox.Show("Введите пароль!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            User currentUser = DataService.Context.Users.FirstOrDefault(m => (m.Login == UserStr) && (m.Password == Password));
            if (currentUser == null) {
                MessageBox.Show("Неверно введенные данные!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (currentUser.UserStatus == null)
            {
                MessageBox.Show("Введите в БД статус пользователя <" + currentUser.Login + ">!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            MainWindow mv = new MainWindow();

            var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
            mvm.currentUserStatus = currentUser.UserStatus;
            mv.DataContext = mvm;
            mv.Show();

            Properties.Settings.Default.UserName = UserStr;
            Properties.Settings.Default.Save();

            var window = param as Window;
            window.Close();
        }

        private void Exit(object param)
        {
            Application.Current.Shutdown();
        }
        #endregion

        public LoginViewModel()
		{
            //adc = new AutenDbContext();

            UserStr = Properties.Settings.Default.UserName;
        }
	}
}