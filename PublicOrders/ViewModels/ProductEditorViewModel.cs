using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PublicOrders.Annotations;
using PublicOrders.Commands;
using PublicOrders.Models;

namespace PublicOrders.ViewModels
{
    public class ProductEditorViewModel : INotifyPropertyChanged
    {

        public DocumentDbContext dc { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<object> Rubrics { get; set; }
        public ObservableCollection<object> Instructions { get; set; }

        #region КОМАНДЫ
        private DelegateCommand addProductCommand;
        public ICommand AddProductCommand
        {
            get
            {
                if (addProductCommand == null)
                {
                    addProductCommand = new DelegateCommand(AddProduct);
                }
                return addProductCommand;
            }
        }

        private void AddProduct()
        {
            //Products.Add(new Product());
            //dc.Products.Add(new Product());
        }
        #endregion

        public ProductEditorViewModel()
        {
            dc = new DocumentDbContext();

            Products = new ObservableCollection<Product>(dc.Products);
            //Rubrics = new ObservableCollection<object>(база);
            //Instructions = new ObservableCollection<object>(база);
        }








        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
