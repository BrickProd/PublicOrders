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

namespace PublicOrders.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadProductsPanel.xaml
    /// </summary>
    public partial class LoadProductsPanel : UserControl
    {
        protected LoadProductsViewModel VM
        {
            get { return (LoadProductsViewModel)Resources["LoadProductsViewModel"]; }
        }

        public LoadProductsPanel()
        {
            InitializeComponent();
        }
    }
}
