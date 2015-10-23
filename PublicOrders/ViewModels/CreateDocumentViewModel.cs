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

        //public DocumentDbContext dc { get; set; }
        public ObservableCollection<Document> Documents { get; set; }
        public ObservableCollection<Template> Templates { get; set; }

        private Template _selectedTemplate = null;
        public Template SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                _selectedTemplate = value;
                OnPropertyChanged("SelectedTemplate");
            }
        }

        private void InitObjects() {
        }

        private ObservableCollection<Product> _templateProducts = null;

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

            document.Products = SelectedTemplate.Products;

            mvm.dc.Documents.Add(document);

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
            this.ButtonCreateDocEnabled = false;

            if (mvm != null)
            {
                Templates = mvm.TemplateCollection;
            }
            Instructions = new ObservableCollection<Instruction>(mvm.dc.Instructions);
        }
    }
}
