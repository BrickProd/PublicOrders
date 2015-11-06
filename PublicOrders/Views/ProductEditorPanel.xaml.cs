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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PublicOrders.Models;

namespace PublicOrders.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductEditorPanel.xaml
    /// </summary>
    public partial class ProductEditorPanel : UserControl
    {
        ListBox dragSource = null;

        public ProductEditorPanel()
        {
            InitializeComponent();
        }

        private void AddNewRubricBtn_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewRubricPanel.Visibility = Visibility.Visible;
        }

        private void CancelNewRubricBtn_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewRubricPanel.Visibility = Visibility.Collapsed;
        }

        private void OkNewRubricBtn_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewRubricPanel.Visibility = Visibility.Collapsed;
        }

        private void productsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            dragSource = parent;
            object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            object data = e.Data.GetData(typeof(string));
            //((IList)dragSource.ItemsSource).Remove(data);
            //parent.Items.Add(data);
        }

        //открыть редактирование продукта 
        private void MouseDoubleClickHandler(object sender, MouseButtonEventArgs e)
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
            this.ProductEditPanel.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.ProductEditPanel.BeginAnimation(FrameworkElement.OpacityProperty, anim2);

            this.ProductEditPanel.Visibility = Visibility.Visible;
            this.EditTabControl.IsEnabled = false;
        }

        //закрыть редактирование продукта 
        private void CloseEditProductPanel_Click(object sender, RoutedEventArgs e)
        {
            ThicknessAnimation anim = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, 50, 0, -50),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };

            this.EditTabControl.IsEnabled = true;

            DoubleAnimation anim2 = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CircleEase()
            };
            anim2.Completed += (o, args) =>
            {
                ProductEditPanel.Visibility = Visibility.Collapsed;
            };

            this.ProductEditPanel.BeginAnimation(FrameworkElement.MarginProperty, anim);
            this.ProductEditPanel.BeginAnimation(FrameworkElement.OpacityProperty, anim2);

        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }
                    if (element == source)
                    {
                        return null;
                    }
                }
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }
            return null;
        }

        #endregion

        private void SaveChanges_LostFocus(object sender, RoutedEventArgs e)
        {
            var mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
            mvm.dc.SaveChanges();
        }
    }
}
