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
using System.Data.Entity;
using System.Windows;

namespace PublicOrders.ViewModels
{
    public class ProductEditorViewModel : INotifyPropertyChanged
    {

        //public DocumentDbContext dc { get; set; }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get
            {
                return Globals.ProductsGlobal;
            }
            set
            {
                Globals.ProductsGlobal = value;
                OnPropertyChanged("Products");
            }

        }


        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
            }

        }

        private DelegateCommand updateProductCommand;
        public ICommand UpdateProductCommand
        {
            get
            {
                if (updateProductCommand == null)
                {
                    updateProductCommand = new DelegateCommand(UpdateProduct);
                }
                return updateProductCommand;
            }
        }

        private void UpdateProduct() {
            //string sss = "";
            Globals.dcGlobal.Entry(SelectedProduct).State = EntityState.Modified;
            //dc.Entry(SelectedProduct).State = EntityState.Modified;
            Globals.dcGlobal.SaveChanges();
        }

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
            //dc = new DocumentDbContext();

            //Products = new ObservableCollection<Product>(dc.Products);

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
