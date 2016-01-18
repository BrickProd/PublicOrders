﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PublicOrders.Data;
using PublicOrders.Models;
using PublicOrders.ViewModels;

namespace PublicOrders.Views
{
    /// <summary>
    /// Логика взаимодействия для WinnersPanel.xaml
    /// </summary>
    public partial class WinnersPanel : UserControl
    {
        public WinnersPanel()
        {
            InitializeComponent();
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

            var vm = DataContext as WinnersViewModel;

            ListViewItem listViewItem = sender as ListViewItem;
            vm.SelectedWinner = listViewItem.DataContext as Winner;;
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

            DataService.WinnersDbContextSaveChanges();

            var vm = DataContext as WinnersViewModel;
            vm.ToView.View.Refresh();
            vm.Favorites.View.Refresh();
            vm.BlackList.View.Refresh();

            DataService.WinnersDbContext.SaveChanges();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.NoteTextGrid.Visibility = Visibility.Visible;
            ThicknessAnimation anim = new ThicknessAnimation
            {
                From = new Thickness(50, 0, -50, 0),
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

            this.NoteTextGrid.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.NoteTextGrid.BeginAnimation(FrameworkElement.OpacityProperty, anim2);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation anim = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(50, 0, -50, 0),
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
                this.NoteTextGrid.Visibility = Visibility.Collapsed;
            };

            this.NoteTextGrid.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.NoteTextGrid.BeginAnimation(FrameworkElement.OpacityProperty, anim2);
        }
    }
}
