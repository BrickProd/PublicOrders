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
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        //public DocumentDbContext dc { get; set; }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get
            {
                return _products;
            }
            set
            {
                _products = value;
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
            mvm.dc.Entry(SelectedProduct).State = EntityState.Modified;
            //dc.Entry(SelectedProduct).State = EntityState.Modified;
            mvm.dc.SaveChanges();
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
            if (mvm != null)
            {
                Products = mvm.ProductCollection;
            }
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
