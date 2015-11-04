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

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var vm = this.DataContext as WinnerSearchViewModel;
                vm.CustomersSearchCommand.Execute(new object());
            }
        }

        private void CustomerDoubleClick(object sender, MouseEventArgs e)
        {
            var vm = this.DataContext as WinnerSearchViewModel;
            vm.WinnerLotsSearchCommand.Execute(new object());




            //сдвиг панелей
            DoubleAnimation daTo100 = new DoubleAnimation();
            daTo100.To = 100;
            daTo100.Duration = TimeSpan.FromSeconds(0.2);
            daTo100.EasingFunction = new CubicEase();

            DoubleAnimation daTo555 = new DoubleAnimation();
            daTo555.To = 555;
            daTo555.Duration = TimeSpan.FromSeconds(0.2);
            daTo555.EasingFunction = new CubicEase();


            this.CustomersSide.BeginAnimation(WidthProperty, daTo100);
            this.WinnersSide.BeginAnimation(WidthProperty, daTo555);

            this.OpenCustomerSide.Visibility = Visibility.Visible;
            this.CustomersListBox.Visibility = Visibility.Collapsed;
        }

        private void OpenCustomerSideBtn_Click(object sender, RoutedEventArgs e)
        {
            //сдвиг панелей
            DoubleAnimation daTo100 = new DoubleAnimation();
            daTo100.To = 100;
            daTo100.Duration = TimeSpan.FromSeconds(0.2);
            daTo100.EasingFunction = new CubicEase();

            DoubleAnimation daTo555 = new DoubleAnimation();
            daTo555.To = 555;
            daTo555.Duration = TimeSpan.FromSeconds(0.2);
            daTo555.EasingFunction = new CubicEase();


            this.CustomersSide.BeginAnimation(WidthProperty, daTo555);
            this.WinnersSide.BeginAnimation(WidthProperty, daTo100);

            this.OpenCustomerSide.Visibility = Visibility.Collapsed;
            this.CustomersListBox.Visibility = Visibility.Visible;
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

        }

    }
}
