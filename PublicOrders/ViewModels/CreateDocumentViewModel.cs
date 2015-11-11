﻿using System;
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
using PublicOrders.Processors.Main;
using System.Windows.Data;

namespace PublicOrders.ViewModels
{
    public class CreateDocumentViewModel : INotifyPropertyChanged
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private bool _isCreateInProcess;

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


        //public ObservableCollection<Product> Products { get; set; }
        /*public ObservableCollection<string> Templates { get; set; }
        public ObservableCollection<Product> Products { get; set; }*/
        public CollectionViewSource FilteredProducts { get; set; }
        public CollectionViewSource ProductsForDocumentGroped { get; set; }
        public ObservableCollection<Product> ProductsForDocument { get; set; }
        public Product SelectedProduct { get; set; }


        private string _selectedTemplate = null;
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
            ProductsForDocument.Remove(SelectedProduct);
            this.FilteredProducts.View.Refresh();
        }
        private void ChooseProduct(object param)
        {
            ProductsForDocument.Add(SelectedProduct);
            this.FilteredProducts.View.Refresh();
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

            /*if (mvm != null)
            {


                Templates = new ObservableCollection<string>(new List<string> {
                        "Комитет",
                        "Свобода",
                        "Форма 2"
                }); ;
                Products = mvm.ProductCollection;
            }*/

            //Products = mvm.ProductCollection;
            //Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions);

            ProductsForDocument = new ObservableCollection<Product>();

            FilteredProducts = new CollectionViewSource();



            FilteredProducts.Source = mvm.ProductCollection;
            FilteredProducts.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));

            FilteredProducts.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            FilteredProducts.Filter += ProductFilter;

            ProductsForDocumentGroped = new CollectionViewSource();
            ProductsForDocumentGroped.Source = ProductsForDocument;
            ProductsForDocumentGroped.GroupDescriptions.Add(new PropertyGroupDescription("Rubric.Name"));

            SelectedTemplate = mvm.TemplateCollection[0];
            FilteredProducts.View.Refresh();

            if (SelectedTemplate == null)
            {
                SelectedTemplate = mvm.TemplateCollection[0];
            }

            if (SelectedInstruction == null) {
                SelectedInstruction = mvm.InstructionCollection[0];
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
