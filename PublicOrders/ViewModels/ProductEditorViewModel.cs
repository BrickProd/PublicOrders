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

        public ObservableCollection<Product> Products
        {
            get;
            set;
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

        public ObservableCollection<Rubric> Rubrics { get; set; }
        public string NewRubricName { get; set; }


        public ObservableCollection<Instruction> Instructions { get; set; }

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


        private DelegateCommand addRubricCommand;
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
        private void AddRubric()
        {
            var newRubric = new Rubric {Name = NewRubricName};

            //mvm.dc.Entry(newRubric);
            //mvm.dc.SaveChanges();

            this.Rubrics.Add(newRubric);
        }
        #endregion

        public ProductEditorViewModel()
        {
            if (mvm != null)
            {
                Products = mvm.ProductCollection;
                Rubrics = new ObservableCollection<Rubric>(mvm.dc.Rubrics);
                Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions);
            }
        }


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
