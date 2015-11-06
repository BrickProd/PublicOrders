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
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

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

        private Rubric _selectedRubric;
        public Rubric SelectedRubric
        {
            get
            {
                return _selectedRubric;
            }
            set
            {
                mvm.dc.SaveChanges();
                _selectedRubric = value;
                OnPropertyChanged("SelectedRubric");
            }
        }

        private Instruction _selectedInstruction;
        public Instruction SelectedInstruction
        {
            get
            {
                return _selectedInstruction;
            }
            set
            {
                mvm.dc.SaveChanges();
                _selectedInstruction = value;
                OnPropertyChanged("SelectedInstruction");
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

        private void UpdateProduct(object param)
        {
            mvm.dc.Entry(SelectedProduct).State = EntityState.Modified;
            mvm.dc.SaveChanges();
        }

        public ObservableCollection<Rubric> Rubrics { get; set; }
        public string NewRubricName { get; set; }


        public ObservableCollection<Instruction> Instructions { get; set; }





        #region КОМАНДЫ
        private DelegateCommand addProductCommand;
        private DelegateCommand addRubricCommand;
        private DelegateCommand addInstructionCommand;

        private DelegateCommand deleteProductCommand;
        private DelegateCommand deleteRubricCommand;
        private DelegateCommand deleteInstructionCommand;

        private DelegateCommand saveProductCommand;
        private DelegateCommand saveInstructionCommand;

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
        public ICommand AddInstructionCommand
        {
            get
            {
                if (addInstructionCommand == null)
                {
                    addInstructionCommand = new DelegateCommand(AddInstruction);
                }
                return addInstructionCommand;
            }
        }


        public ICommand DeleteProductCommand
        {
            get
            {
                if (deleteProductCommand == null)
                {
                    deleteProductCommand = new DelegateCommand(DeleteProduct);
                }
                return deleteProductCommand;
            }
        }
        public ICommand DeleteRubricCommand
        {
            get
            {
                if (deleteRubricCommand == null)
                {
                    deleteRubricCommand = new DelegateCommand(DeleteRubric);
                }
                return deleteRubricCommand;
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
        public ICommand SaveInstructionCommand
        {
            get
            {
                if (saveInstructionCommand == null)
                {
                    saveInstructionCommand = new DelegateCommand(SaveInstruction);
                }
                return saveInstructionCommand;
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

                Rubrics = new ObservableCollection<Rubric>(mvm.dc.Rubrics.Where(m=>m.RubricId!=1));
                Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions.Where(m=>m.InstructionId!=1));

                mvm.CheckProductsRepetition();
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
        private void AddProduct(object param)
        {
            var newProduct = new Product
            {
                Name = "НОВЫЙ ПРОДУКТ",
                Rubric = Rubrics.FirstOrDefault(m=>m.RubricId==1)
                
            };

            mvm.dc.Entry(newProduct).State = EntityState.Added;
            mvm.dc.SaveChanges();

            mvm.ProductCollection.Add(newProduct);
            mvm.CheckProductsRepetition();
            this.Products.View.Refresh();

            this.SelectedProduct = newProduct;
        }
        private void AddRubric(object param)
        {
            var newRubric = new Rubric { Name = NewRubricName };

            mvm.dc.Entry(newRubric);
            mvm.dc.SaveChanges();

            this.Rubrics.Add(newRubric);
        }
        private void AddInstruction(object param)
        {
            var newInstruction = new Instruction
            {
                Name = "НОВАЯ ИНСТРУКЦИЯ"
            };

            mvm.dc.Entry(newInstruction).State = EntityState.Added;
            mvm.dc.SaveChanges();

            Instructions.Add(newInstruction);

            this.SelectedInstruction = newInstruction;
        }



        private void DeleteProduct(object param)
        {
            if (MessageBox.Show("Удалить выделенные продукты?", "Предупреждение",
               MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                var products = param as IEnumerable<object>;

                products.ToList().ForEach(m =>
                {
                    var p = m as Product;
                    p.CommitteeProperties.Clear();
                    p.FreedomProperties.Clear();
                    p.Form2Properties.Clear();
                    mvm.dc.Entry(p).State = EntityState.Deleted;
                    mvm.dc.SaveChanges();
                    mvm.ProductCollection.Remove(p);
                });
            }
        }
        private void DeleteRubric(object param)
        {
            if (MessageBox.Show("Удалить выделенные рубрики?", "Предупреждение",
               MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {

                    SelectedRubric.Products.ToList().ForEach(p => {
                        p.RubricId = 1;
                        mvm.dc.Entry(p).State = EntityState.Modified;
                    });
                    mvm.dc.SaveChanges();
                    mvm.dc.Entry(SelectedRubric).State = EntityState.Deleted;
                    mvm.dc.SaveChanges();
                    Rubrics.Remove(SelectedRubric);
            }
        }


        private async void SaveProduct(object param)
        {
            if (SelectedProduct.FreedomProperties.Count > 1)
            {
                MessageBox.Show("В шаблоне свобода может быть только 1 свойство");
                return;
            }    

            if (SelectedProduct != null)
            {
                SelectedProduct.ModifiedDateTime = DateTime.Now;
                //mvm.dc.Entry(SelectedProduct).State = EntityState.Modified;
                await mvm.dc.SaveChangesAsync();
                //mvm.CheckProductsRepetition();
            }
            
            this.Products.View.Refresh();
        }
        private void SaveInstruction(object param)
        {
            mvm.dc.Entry(SelectedInstruction).State = EntityState.Modified;
            mvm.dc.SaveChanges();
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
