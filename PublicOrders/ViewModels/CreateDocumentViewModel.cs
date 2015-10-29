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
using PublicOrders.Processors.Main;
using System.Windows.Data;

namespace PublicOrders.ViewModels
{
    public class CreateDocumentViewModel : INotifyPropertyChanged
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
        private bool _buttonCreateDocEnabled;

        public bool ButtonCreateDocEnabled
        {
            get { return _buttonCreateDocEnabled; }
            set
            {
                _buttonCreateDocEnabled = value;
                OnPropertyChanged("ButtonCreateDocEnabled");
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<string> Templates { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public CollectionViewSource FilteredProducts { get; set; }

        private string _selectedTemplate = null;
        public string SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                _selectedTemplate = value;
                OnPropertyChanged("SelectedTemplate");
                this.FilteredProducts.View.Refresh();
            }
        }

        public ObservableCollection<Instruction> Instructions { get; set; }
        public Instruction SelectedInstruction { get; set; }

        #region КОМАНДЫ
        private DelegateCommand createDocumentCommand;

        public event PropertyChangedEventHandler PropertyChanged;

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


        private void CreateDocument()
        {
            ButtonCreateDocEnabled = false;
            if ((mvm.cdProcessor != null) && (mvm.cdProcessor.isWorking())) {
                mvm.cdProcessor.Stop();
            }
            CreateDocumentDone_delegete done_del = new CreateDocumentDone_delegete(CreateDocumentDone_Proc);
            mvm.cdProcessor = new CreateDocumentProcessor(FilteredProducts.View.Cast<Product>().ToList(), SelectedInstruction, SelectedTemplate, done_del);
            mvm.cdProcessor.Operate();
        }
        #endregion

        private void CreateDocumentDone_Proc(ResultType_enum ResultType_enum, string message) {
            switch (ResultType_enum) {
                case (ResultType_enum.Done):
                    MessageBox.Show("Документ создан успешно!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    MessageBox.Show("Ошибка при создании документа!\n" + message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            ButtonCreateDocEnabled = true;
        }

        public CreateDocumentViewModel()
        {
            this.ButtonCreateDocEnabled = false;

            if (mvm != null)
            {
                Templates = new ObservableCollection<string>(new List<string> {
                        "Комитет",
                        "Свобода",
                        "Форма 2"
                }); ;
                Products = mvm.ProductCollection;
            }
            Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions);
            FilteredProducts = new CollectionViewSource();
            FilteredProducts.Source = this.Products;
            FilteredProducts.Filter += ProductFilter;
        }

        private void ProductFilter(object sender, FilterEventArgs e)
        {
            Product p = e.Item as Product;
            if (p != null)
            {
                switch (SelectedTemplate)
                {
                    case "Комитет": e.Accepted = (p.CommitteeProperties.Any());
                        break;
                    case "Свобода":
                        e.Accepted = (p.FreedomProperties.Any());
                        break;
                    case "Форма 2":
                        e.Accepted = (p.Form2Properties.Any());
                        break;
                    default:
                        e.Accepted = false;
                        break;
                }
            }
        }
    }
}
