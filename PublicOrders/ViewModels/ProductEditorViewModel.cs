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
using System.Windows.Data;

namespace PublicOrders.ViewModels
{
    public class ProductEditorViewModel : INotifyPropertyChanged
    {
        public MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private CollectionViewSource _products;
        public CollectionViewSource Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private string _productFilterStr;
        public string ProductFilterStr
        {
            get { return _productFilterStr; }
            set
            {
                _productFilterStr = value;
                this.Products.View.Refresh();
                OnPropertyChanged("ProductFilterStr");
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

        private void UpdateProduct()
        {
            //string sss = "";
            mvm.dc.Entry(SelectedProduct).State = EntityState.Modified;
            //dc.Entry(SelectedProduct).State = EntityState.Modified;
            mvm.dc.SaveChanges();
        }

        public ObservableCollection<Rubric> Rubrics { get; set; }
        public string NewRubricName { get; set; }


        public ObservableCollection<Instruction> Instructions { get; set; }





        #region КОМАНДЫ
        private DelegateCommand addProductCommand;
        private DelegateCommand addRubricCommand;
        private DelegateCommand saveProductCommand;


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
        public ICommand AddRubricCommand
        {
            get
            {
                if (addRubricCommand == null)
                {
                    addRubricCommand = new DelegateCommand(AddRubric);
                }
                return addRubricCommand;
            }
        }
        public ICommand SaveProductCommand
        {
            get
            {
                if (saveProductCommand == null)
                {
                    saveProductCommand = new DelegateCommand(SaveProduct);
                }
                return saveProductCommand;
            }
        }

        #endregion

        public ProductEditorViewModel()
        {
            if (mvm != null)
            {
                Products = new CollectionViewSource();
                Products.Source = this.mvm.ProductCollection;
                Products.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));
                Products.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                Products.Filter += ProductFilter;
                Products.View.Refresh();

                Rubrics = new ObservableCollection<Rubric>(mvm.dc.Rubrics);
                Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions);
            }
        }





        //Фильтр
        public void ProductFilter(object sender, FilterEventArgs args)
        {
            if (string.IsNullOrEmpty(this.ProductFilterStr))
            {
                args.Accepted = true;
            }
            else
            {
                Product p = args.Item as Product;
                if (p.Name.ToLower().Contains(this.ProductFilterStr.ToLower()))
                {
                    args.Accepted = true;
                }
                else
                {
                    args.Accepted = false;
                }
            }
        }


        #region МЕТОДЫ
        private void AddProduct()
        {
            //Products.Add(new Product());
            //dc.Products.Add(new Product());
        }
        private void AddRubric()
        {
            var newRubric = new Rubric { Name = NewRubricName };

            //mvm.dc.Entry(newRubric);
            //mvm.dc.SaveChanges();

            this.Rubrics.Add(newRubric);
        }
        private void SaveProduct()
        {
            var task = new Task(new Action(() =>
            {
                if (SelectedProduct != null)
                {

                    mvm.dc.Entry(SelectedProduct).State = EntityState.Modified;
                    mvm.dc.SaveChanges();
                }
            }));
            task.Start();

            this.Products.View.Refresh();

        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
