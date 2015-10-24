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
    }
}
