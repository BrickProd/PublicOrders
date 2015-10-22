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

namespace PublicOrders.ViewModels
{
    public class CreateDocumentViewModel : INotifyPropertyChanged
    {
        private bool _buttonCreateDocEnabled = true;
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

        //public DocumentDbContext dc { get; set; }
        public ObservableCollection<Document> Documents { get; set; }
        public ObservableCollection<Template> Templates { get; set; }

        private Template _selectedTemplate = null;
        public Template SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                _selectedTemplate = value;

                // TemplateProducts = new ObservableCollection<Product>(dc.Products.SelectMany(m => m.Templates.FirstOrDefault(l => l.Name.Trim().ToLower() == SelectedTemplate.Name.Trim().ToLower())));
                // Выбираем продукты по шаблону
                TemplateProducts = new ObservableCollection<Product>(Globals.dcGlobal.Templates.Find(SelectedTemplate.TemplateId).Products);

                OnPropertyChanged("SelectedTemplate");
            }
        }

        private void InitObjects() {
        }

        private ObservableCollection<Product> _templateProducts = null;
        public ObservableCollection<Product> TemplateProducts
        {
            get
            {
                return _templateProducts;
            }
            set
            {
                _templateProducts = value;
                OnPropertyChanged("TemplateProducts");
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
            // Добавление документа в БД с инструкцией
            Document document = new Document();
            document.Instruction = SelectedInstruction;

            document.Products = TemplateProducts;

            Globals.dcGlobal.Documents.Add(document);

            ButtonCreateDocEnabled = false;
            CreateDocumentDone_delegete done_del = new CreateDocumentDone_delegete(CreateDocumentDone_Proc);
            CreateDocumentProcessor cdProcessor = new CreateDocumentProcessor(document, SelectedTemplate, done_del);
            cdProcessor.Operate();
        }
        #endregion

        private void CreateDocumentDone_Proc(ResultType resultType, string message) {
            switch (resultType) {
                case (ResultType.Done):
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
            //dc = new DocumentDbContext();

            TemplateProducts = new ObservableCollection<Product>();
            Templates = new ObservableCollection<Template>(Globals.dcGlobal.Templates);
            Instructions = new ObservableCollection<Instruction>(Globals.dcGlobal.Instructions);
        }
    }
}
