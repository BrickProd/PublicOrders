﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using PublicOrders.ViewModels;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PublicOrders
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			// Required to initialize variables
			InitializeComponent();
            
		}

        private void CreateDocumentTabItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CreateDocumentPanel.DataContext = new CreateDocumentViewModel();
            }
            catch (Exception ex)
            {
                //
            }
        }

        private  void ProductEditorTabItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {

                /*var vm = this.ProductEditorPanel.DataContext as ProductEditorViewModel;
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

                mvm.RefreshProducts();
                vm.Products.Source = mvm.ProductCollection;
                vm.Products.View.Refresh();*/
                this.ProductEditorPanel.DataContext = new ProductEditorViewModel();
            }
            catch (Exception ex)
            {
                //
            }

        }

        private  void UpdateAll()
        {
            
            
        }
    }
}