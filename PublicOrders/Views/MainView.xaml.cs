using System;
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
using System.Collections.ObjectModel;
using PublicOrders.Models;
using System.Linq;
using PublicOrders.Data;

namespace PublicOrders
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			// Required to initialize variables
			InitializeComponent();

            this.MainTabControl.SelectedIndex = 3;
            
		}

        private void CreateDocumentTabItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = this.CreateDocumentPanel.DataContext as CreateDocumentViewModel;
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

                mvm.RefreshProducts();
                //vm.FilteredProducts.Source = mvm.ProductCollection;
                vm.FilteredProducts.View.Refresh();

                mvm.RefreshInstructions();
                //ObservableCollection<Product> pppp = vm.ProductsForDocument;
                //vm.ProductsForDocumentGroped.View.Refresh();
                //vm.ProductsForDocumentGroped.Source = ProductsForDocument;

                //this.CreateDocumentPanel.DataContext = new CreateDocumentViewModel();
            }
            catch (Exception ex)
            {
                string sfsdf = "dsff";
            }
        }

        private  void ProductEditorTabItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = this.ProductEditorPanel.DataContext as ProductEditorViewModel;
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

                //mvm.RefreshProducts();
                //vm.Products.Source = mvm.ProductCollection;
                //vm.Products.View.Refresh();
                //mvm.CheckProductsRepetition();

                //mvm.RefreshRubrics();
                //vm.CustomRubrics.Source = mvm.RubricCollection.Where(m => m.RubricId != 1);
                //vm.CustomRubrics.View.Refresh();

                //mvm.RefreshInstructions();
                //vm.CustomInstructions.Source = mvm.InstructionCollection.Where(m => m.InstructionId != 1);
                //vm.CustomInstructions.View.Refresh();
                //this.ProductEditorPanel.DataContext = new ProductEditorViewModel();
            }
            catch (Exception ex)
            {
                //
            }

        }

        private void LoadProductsTabItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
                mvm.RefreshRubrics();
            }
            catch (Exception ex)
            {
                //
            }

        }

        private  void UpdateAll()
        {
            
            
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.ProductEditorTabItem.IsSelected)
            {
                ThicknessAnimation anim = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 0, 0),
                    To = new Thickness(0, 50, 0, -50),
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CircleEase()
                };

                this.ProductEditorPanel.EditTabControl.IsEnabled = true;

                DoubleAnimation anim2 = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CircleEase()
                };
                anim2.Completed += (o, args) =>
                {
                    this.ProductEditorPanel.ProductEditPanel.Visibility = Visibility.Collapsed;
                };

                this.ProductEditorPanel.ProductEditPanel.BeginAnimation(FrameworkElement.MarginProperty, anim);
                this.ProductEditorPanel.ProductEditPanel.BeginAnimation(FrameworkElement.OpacityProperty, anim2);


                //var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
                //mvm.dc.SaveChanges();
                try
                {
                    DataService.Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    
                }
                
            }
        }

	    private void Selector_OnSelected(object sender, RoutedEventArgs e)
	    {
	        var vm=  WinnersPanel.DataContext as WinnersViewModel;

            vm.RefreshListCommand.Execute(null);
	    }
	}
}