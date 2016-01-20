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
using System.Windows.Controls;
using System.Windows.Data;
using PublicOrders.Data;
using PublicOrders.Processors.Internet;

namespace PublicOrders.ViewModels
{
    public class ProductEditorViewModel : INotifyPropertyChanged
    {
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        public CollectionViewSource Products { get; set; }
        public CollectionViewSource CustomRubrics { get; set; }
        public CollectionViewSource CustomInstructions { get; set; }

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

        private bool _gostCheckInProcess;
        public bool GostCheckInProcess
        {
            get { return _gostCheckInProcess; }
            set
            {
                _gostCheckInProcess = value;
                OnPropertyChanged("GostCheckInProcess");
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
                DataService.Context.SaveChanges();
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
                DataService.Context.SaveChanges();
                _selectedInstruction = value;
                OnPropertyChanged("SelectedInstruction");
            }
        }

        
        

        private void UpdateProduct(object param)
        {
            DataService.Context.SaveChanges();
        }

        #region КОМАНДЫ
        private DelegateCommand addProductCommand;
        private DelegateCommand checkGostCommand;
        private DelegateCommand addRubricCommand;
        private DelegateCommand addInstructionCommand;
        private DelegateCommand replaceProductsCommand;

        private DelegateCommand deleteProductCommand;
        private DelegateCommand deleteRubricCommand;
        private DelegateCommand deleteInstructionCommand;

        private DelegateCommand saveProductCommand;
        private DelegateCommand saveInstructionCommand;

        private DelegateCommand choseProductsInRubricCommand;

        private DelegateCommand sortProductsByNameCommand;
        private DelegateCommand sortProductsByDateCommand;


        private DelegateCommand updateProductCommand;
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
        public ICommand CheckGostCommand
        {
            get
            {
                if (checkGostCommand == null)
                {
                    checkGostCommand = new DelegateCommand(CheckGost);
                }
                return checkGostCommand;
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
        public ICommand ReplaceProductsCommand
        {
            get
            {
                if (replaceProductsCommand == null)
                {
                    replaceProductsCommand = new DelegateCommand(ReplaceProducts);
                }
                return replaceProductsCommand;
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
        public ICommand DeleteInstructionCommand
        {
            get
            {
                if (deleteInstructionCommand == null)
                {
                    deleteInstructionCommand = new DelegateCommand(DeleteInstruction);
                }
                return deleteInstructionCommand;
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

        public ICommand ChoseProductsInRubricCommand
        {
            get
            {

                if (choseProductsInRubricCommand == null)
                {

                    choseProductsInRubricCommand = new DelegateCommand(ChoseProductsInRubric);
                }
                return choseProductsInRubricCommand;
            }
        }

        public ICommand SortProductsByNameCommand
        {
            get
            {

                if (sortProductsByNameCommand == null)
                {

                    sortProductsByNameCommand = new DelegateCommand(SortProductsByName);
                }
                return sortProductsByNameCommand;
            }
        }
        public ICommand SortProductsByDateCommand
        {
            get
            {

                if (sortProductsByDateCommand == null)
                {

                    sortProductsByDateCommand = new DelegateCommand(SortProductsByDate);
                }
                return sortProductsByDateCommand;
            }
        }


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
        #endregion

        public ProductEditorViewModel()
        {
            if (mvm != null)
            {
                Products = new CollectionViewSource();

                Products.Source = DataService.Products;
                Products.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));
                Products.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
                Products.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                Products.Filter += ProductFilter;

                CustomRubrics = new CollectionViewSource();
                CustomRubrics.Source = DataService.Rubrics;
                CustomRubrics.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

                CustomInstructions = new CollectionViewSource();
                CustomInstructions.Source = DataService.Instructions;
                CustomInstructions.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

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
                Name = "_НОВЫЙ ТОВАР",
                Rubric = DataService.Context.Rubrics.Find(1),
                ModifiedDateTime = DateTime.Now
            };

            DataService.Context.Products.Add(newProduct);
            DataService.Context.SaveChanges();

            DataService.Products.Add(newProduct);

            mvm.CheckProductsRepetition();
            this.SelectedProduct = newProduct;
        }
        private void AddRubric(object param)
        {
            var newRubric = new Rubric { Name = "_НОВАЯ РУБРИКА" };

            // Проверка на повтор
            var repeatRubric = DataService.Context.Rubrics.FirstOrDefault(m => m.Name == newRubric.Name);
            if (repeatRubric != null) {
                MessageBox.Show("Рубрика уже существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }           

            DataService.Context.Rubrics.Add(newRubric);
            DataService.Context.SaveChanges();

            DataService.Rubrics.Add(newRubric);
        }
        private void AddInstruction(object param)
        {
            // Проверка на повтор
            var repeatInstruction = DataService.Context.Instructions.FirstOrDefault(m => m.Name == "НОВАЯ ИНСТРУКЦИЯ");
            if (repeatInstruction != null)
            {
                MessageBox.Show("\"НОВАЯ ИНСТРУКЦИЯ\" уже существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var newInstruction = new Instruction
            {
                Name = "НОВАЯ ИНСТРУКЦИЯ"
            };

            DataService.Context.Instructions.Add(newInstruction);
            DataService.Context.SaveChanges();

            DataService.Instructions.Add(newInstruction);
            this.SelectedInstruction = newInstruction;
        }
        private void ReplaceProducts(object param)
        {
            var products = param as IEnumerable<object>;

            products.ToList().ForEach(m =>
            {
                var p = m as Product;
                p.Rubric = SelectedRubric;
                
            });
            DataService.Context.SaveChanges();
            this.Products.View.Refresh();

        }

        private void AllGOSTsChecked_proc(ResultType_enum resultType_enum, string message)
        {
            GostCheckInProcess = false;
        }

        private void GOSTCheckProgress_proc(string text, int intValue) {

        }

        private void CheckGost(object param)
        {
            var obj = param as IEnumerable<object>;
            var products = obj.Select(m =>
            {
                var p = m as Product;
                return p;
            }).ToList();

            if (products != null && products.Any())
            {
                if ((mvm.gcProcessor != null) && (mvm.gcProcessor.isWorking()))
                {
                    mvm.gcProcessor.Stop();
                }
                AllGOSTsChecked_delegete allGOSTsChecked_delegete = new AllGOSTsChecked_delegete(AllGOSTsChecked_proc);
                GOSTCheckProgress_delegate gostCheckProgress_delegate = new GOSTCheckProgress_delegate(GOSTCheckProgress_proc);
                mvm.gcProcessor = new GOSTsCheckProcessor(allGOSTsChecked_delegete, gostCheckProgress_delegate, products);
                mvm.gcProcessor.Operate();

                GostCheckInProcess = true;
            }            
        }

        private void DeleteProduct(object param)
        {
            if (MessageBox.Show("Удалить выделенный товар?", "Предупреждение",
               MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                var products = param as IEnumerable<object>;

                var prod = products.ToList().Select(m =>
                {
                    return m as Product;                   
                }).ToList();

                DataService.Context.Products.RemoveRange(prod);
                DataService.Context.SaveChanges();

                prod.ForEach(m => DataService.Products.Remove(m));
            }
        }

        private void DeleteRubric(object param)
        {
            if (SelectedRubric == null) return;
            if (MessageBox.Show("Удалить выделенную рубрику?", "Предупреждение",
               MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                SelectedRubric.Products.ToList().ForEach(p => {
                    p.Rubric = DataService.Context.Rubrics.Find(1);
                });

                DataService.Context.Rubrics.Remove(SelectedRubric);
                DataService.Context.SaveChanges();

                DataService.Rubrics.Remove(SelectedRubric);

                this.Products.View.Refresh();
            }
        }

        private void DeleteInstruction(object param)
        {
            if (SelectedInstruction == null) return;
            if (MessageBox.Show("Удалить выделенную инструкцию?", "Предупреждение",
               MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                DataService.Context.Instructions.Remove(SelectedInstruction);
                DataService.Context.SaveChanges();

                DataService.Instructions.Remove(SelectedInstruction);
            }
        }

        private void SaveProduct(object param)
        {
            if (SelectedProduct.FreedomProperties.Count > 1)
            {
                MessageBox.Show("В шаблоне свобода может быть только 1 свойство");
                return;
            }    

            if (SelectedProduct != null)
            {
                SelectedProduct.ModifiedDateTime = DateTime.Now;
                DataService.Context.SaveChanges();
            }
            
            mvm.CheckProductsRepetition();
            this.Products.View.Refresh();
        }
        private void SaveInstruction(object param)
        {
            DataService.Context.SaveChanges();
        }

        private void ChoseProductsInRubric(object param)
        {
            var listView = param as ListView;

            if (listView != null)
            {
                var listSelectedProduct = listView.SelectedItem as Product;

                var productsInRubric = Products.View.Cast<Product>().ToList().Where(m => listSelectedProduct != null && m.Rubric == listSelectedProduct.Rubric);
                listView.SelectedItems.Clear();
                productsInRubric.ToList().ForEach(m => listView.SelectedItems.Add(m));
            }
        }

        private void SortProductsByName(object param)
        {
            var mySort = Products.SortDescriptions.FirstOrDefault(m => m.PropertyName == "Name");
            if (mySort.Direction == ListSortDirection.Ascending)
            {
                Products.SortDescriptions.Clear();
                Products.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
                Products.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
            }
            else
            {
                Products.SortDescriptions.Clear();
                Products.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
                Products.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
        private void SortProductsByDate(object param)
        {
            var mySort = Products.SortDescriptions.FirstOrDefault(m => m.PropertyName == "ModifiedDateTime");
            if (mySort.Direction == ListSortDirection.Ascending)
            {
                Products.SortDescriptions.Clear();
                Products.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
                Products.SortDescriptions.Add(new SortDescription("ModifiedDateTime", ListSortDirection.Descending));
            }
            else
            {
                Products.SortDescriptions.Clear();
                Products.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
                Products.SortDescriptions.Add(new SortDescription("ModifiedDateTime", ListSortDirection.Ascending));
            }
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
