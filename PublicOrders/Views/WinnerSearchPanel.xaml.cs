using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PublicOrders.ViewModels;
using System.Windows.Media.Animation;
using System.Diagnostics;
using PublicOrders.Data;

namespace PublicOrders.Views
{
    /// <summary>
    /// Логика взаимодействия для WinnerSearchPanel.xaml
    /// </summary>
    public partial class WinnerSearchPanel : UserControl
    {
        public WinnerSearchPanel()
        {
            InitializeComponent();
        }

        private void SlideInCustomers()
        {
            //сдвиг панелей
            ThicknessAnimation slideIn = new ThicknessAnimation();
            slideIn.To = new Thickness(0,0,10,0);
            slideIn.Duration = TimeSpan.FromSeconds(0.2);
            slideIn.EasingFunction = new CubicEase();

            this.CustomersSide.BeginAnimation(MarginProperty, slideIn);

            this.OpenCustomerSide.Visibility = Visibility.Collapsed;
            this.CustomersListBox.Visibility = Visibility.Visible;
        }

        private void SlideOutCustomers()
        {
            //сдвиг панелей
            ThicknessAnimation slideOut = new ThicknessAnimation();
            slideOut.To = new Thickness(0, 0, this.MainPanelGrid.ActualWidth-100, 0);
            slideOut.Duration = TimeSpan.FromSeconds(0.2);
            slideOut.EasingFunction = new CubicEase();
            
            this.CustomersSide.BeginAnimation(MarginProperty, slideOut);

            this.OpenCustomerSide.Visibility = Visibility.Visible;
            this.CustomersListBox.Visibility = Visibility.Collapsed;
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var vm = this.DataContext as WinnerSearchViewModel;
                vm.CustomersSearchCommand.Execute(new object());

                SlideInCustomers();
            }
        }

        private void CustomerDoubleClick(object sender, MouseEventArgs e)
        {
            var vm = this.DataContext as WinnerSearchViewModel;
            vm.WinnerLotsSearchCommand.Execute(new object());

            SlideOutCustomers();
        }

        private void OpenCustomerSideBtn_Click(object sender, RoutedEventArgs e)
        {
            SlideInCustomers();
        }

        private void WinnerDoubleClick(object sender, MouseEventArgs e)
        {

            ThicknessAnimation anim = new ThicknessAnimation
            {
                From = new Thickness(0, 50, 0, -50),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };


            DoubleAnimation anim2 = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };
            this.WinnerInfoPanel.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.WinnerInfoPanel.BeginAnimation(FrameworkElement.OpacityProperty, anim2);

            this.WinnerInfoPanel.Visibility = Visibility.Visible;
        }

        //закрыть панель победителя 
        private void CloseWinnerInfoPanel_Click(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation anim = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, 50, 0, -50),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };

            DoubleAnimation anim2 = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };
            anim2.Completed += (o, args) =>
            {
                WinnerInfoPanel.Visibility = Visibility.Collapsed;
            };

            this.WinnerInfoPanel.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.WinnerInfoPanel.BeginAnimation(FrameworkElement.OpacityProperty, anim2);

            MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
            mvm.wc.SaveChanges();
        }

        private void CustomersSide_MouseLeave(object sender, MouseEventArgs e)
        {
            SlideOutCustomers();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SlideInCustomers();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
