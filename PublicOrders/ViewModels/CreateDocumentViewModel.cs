using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PublicOrders.Commands;
using PublicOrders.Models;
using System.Threading;
using PublicOrders.Processors;
using System.Windows;
using System.ComponentModel;
using PublicOrders.Annotations;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using PublicOrders.Processors.Main;
using System.Windows.Data;
using PublicOrders.Data;

namespace PublicOrders.ViewModels
{
    public class CreateDocumentViewModel : INotifyPropertyChanged
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private bool _isCreateInProcess;
        private string _selectedTemplate = null;


        public bool IsCreateInProcess
        {
            get {
                return _isCreateInProcess;
            }
            set
            {
                _isCreateInProcess = value;
                OnPropertyChanged("IsCreateInProcess");
            }
        }

        public CollectionViewSource FilteredProducts { get; set; }
        public CollectionViewSource ProductsForDocumentGroped { get; set; }
        public ObservableCollection<Product> ProductsForDocument { get; set; }
        public Product SelectedProduct { get; set; }

        public string SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                _selectedTemplate = value;
                OnPropertyChanged("SelectedTemplate");
                ProductsForDocument.Clear();
                this.FilteredProducts.View.Refresh();      
            }
        }

        //public ObservableCollection<Instruction> Instructions { get; set; }
        public Instruction SelectedInstruction { get; set; }

        #region КОМАНДЫ
        private DelegateCommand createDocumentCommand;
        private DelegateCommand chooseProductCommand;
        private DelegateCommand unchooseProductCommand;
        private DelegateCommand choseProductsInRubricCommand;

        public ICommand CreateDocumentCommand
        {
            get
            {
                if (createDocumentCommand == null)
                {
                    createDocumentCommand = new DelegateCommand(CreateDocument);
                }
                return createDocumentCommand;
            }
        }
        public ICommand ChooseProductCommand
        {
            get
            {

                if (chooseProductCommand == null)
                {

                    chooseProductCommand = new DelegateCommand(ChooseProduct);
                }
                return chooseProductCommand;
            }
        }
        public ICommand UnchooseProductCommand
        {
            get
            {
                if (unchooseProductCommand == null)
                {
                    unchooseProductCommand = new DelegateCommand(UnchooseProduct);
                }
                return unchooseProductCommand;
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

        private void CreateDocument(object param)
        {
            if (ProductsForDocument.Count == 0) {
                MessageBox.Show("Веберите товар!", "Информация", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (SelectedInstruction == null)
            {
                MessageBox.Show("Веберите инструкцию!", "Информация", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            IsCreateInProcess = true;
            if ((mvm.cdProcessor != null) && (mvm.cdProcessor.isWorking())) {
                mvm.cdProcessor.Stop();
            }
            CreateDocumentDone_delegete done_del = new CreateDocumentDone_delegete(CreateDocumentDone_Proc);
            mvm.cdProcessor = new CreateDocumentProcessor(/*FilteredProducts.View.Cast<Product>().ToList()*/ProductsForDocument.ToList(), SelectedInstruction, SelectedTemplate, done_del);
            mvm.cdProcessor.Operate();
        }
        private void UnchooseProduct(object param)
        {
            var products = param as IEnumerable<object>;
            products?.ToList().ForEach(m =>
            {
                var p = m as Product;

                ProductsForDocument.Remove(p);
            });
            this.FilteredProducts.View.Refresh();
        }
        private void ChooseProduct(object param)
        {
            var products = param as IEnumerable<object>;
            products?.ToList().ForEach(m =>
            {
                var p = m as Product;

                ProductsForDocument.Add(p);
            });
            this.FilteredProducts.View.Refresh();
        }
        private void ChoseProductsInRubric(object param)
        {
            var listView = param as ListView;

            if (listView != null)
            {
                var listSelectedProduct = listView.SelectedItem as Product;

                var productsInRubric = FilteredProducts.View.Cast<Product>().ToList().Where(m => listSelectedProduct != null && m.Rubric == listSelectedProduct.Rubric);
                listView.SelectedItems.Clear();
                productsInRubric.ToList().ForEach(m => listView.SelectedItems.Add(m));
            }
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreateDocumentDone_Proc(ResultType_enum ResultType_enum, string message) {
            switch (ResultType_enum) {
                case (ResultType_enum.Done):
                    MessageBox.Show("Документ создан успешно!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    MessageBox.Show("Ошибка при создании документа!\n" + message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            IsCreateInProcess = false;
        }

        public CreateDocumentViewModel()
        {
            this.IsCreateInProcess = false;


            ProductsForDocument = new ObservableCollection<Product>();

            FilteredProducts = new CollectionViewSource();



            FilteredProducts.Source = DataService.Products; /*mvm.ProductCollection;*/
            FilteredProducts.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));
            FilteredProducts.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
            FilteredProducts.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            FilteredProducts.Filter += ProductFilter;

            ProductsForDocumentGroped = new CollectionViewSource();
            ProductsForDocumentGroped.Source = ProductsForDocument;
            ProductsForDocumentGroped.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));
            ProductsForDocumentGroped.SortDescriptions.Add(new SortDescription("Rubric.Name", ListSortDirection.Ascending));
            ProductsForDocumentGroped.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            SelectedTemplate = mvm.TemplateCollection[0];
            FilteredProducts.View.Refresh();

            if (SelectedTemplate == null)
            {
                SelectedTemplate = mvm.TemplateCollection[0];
            }

            if (SelectedInstruction == null) {
                SelectedInstruction = DataService.Instructions[0];
            }
        }

        private void ProductFilter(object sender, FilterEventArgs e)
        {
            Product p = e.Item as Product;
            var isChosen = this.ProductsForDocument.Contains(p);
            if (p != null)
            {
                switch (SelectedTemplate)
                {
                    case "Комитет": e.Accepted = (p.CommitteeProperties.Any() && !isChosen);
                        break;
                    case "Свобода":
                        e.Accepted = (p.FreedomProperties.Any() && !isChosen);
                        break;
                    case "Форма 2":
                        e.Accepted = (p.Form2Properties.Any() && !isChosen);
                        break;
                    default:
                        e.Accepted = false;
                        break;
                }
            }
        }
    }
}
