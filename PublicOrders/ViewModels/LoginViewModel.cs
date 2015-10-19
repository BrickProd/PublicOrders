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

namespace PublicOrders.ViewModels
{
	public class LoginViewModel
	{	
        public string Server { get; set; }
        public string DataBase { get; set; }
        public string User { get; set; }
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

        private void Enter()
        {
            //метод
            // мессадже
            //dfgsdfgsdfgsdg
        }
        #endregion

        public LoginViewModel()
		{
			// Вставьте ниже код, необходимый для создания объекта.
		}
	}
}