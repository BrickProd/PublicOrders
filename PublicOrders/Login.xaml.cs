using PublicOrders.ViewModels;
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

namespace PublicOrders
{
	/// <summary>
	/// Логика взаимодействия для Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		public Login()
		{
			this.InitializeComponent();
			
			// Вставьте ниже код, необходимый для создания объекта.
		}

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var vm = this.DataContext as LoginViewModel;
                vm.EnterCommand.Execute(this);
            }
        }
    }
}