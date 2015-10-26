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
    }
}
