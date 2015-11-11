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
                var vm = this.ProductEditorPanel.DataContext as ProductEditorViewModel;
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

                mvm.RefreshProducts();
                vm.Products.Source = mvm.ProductCollection;
                vm.Products.View.Refresh();
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
                var vm = this.LoadProductsPanel.DataContext as LoadProductsViewModel;
                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

                mvm.RefreshRubrics();
                vm.Rubrics = mvm.RubricCollection;

                //this.ProductEditorPanel.DataContext = new ProductEditorViewModel();
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


                var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
                mvm.dc.SaveChanges();
            }
        }
    }
}