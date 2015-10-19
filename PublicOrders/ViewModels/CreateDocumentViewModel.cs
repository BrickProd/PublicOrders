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

        public DocumentDbContext dc { get; set; }
        public ObservableCollection<Document> Documents { get; set; }
        public ObservableCollection<Template> Templates { get; set; }
        public Template SelectedTemplate { get; set; }

        public ObservableCollection<Product> SelectedProducts { get; set; }

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

            foreach (Product product in SelectedProducts) {
                document.Products.Add(product);

            }
            dc.Documents.Add(document);

            ButtonCreateDocEnabled = false;
            CreateDocumentDone_delegete done_del = new CreateDocumentDone_delegete(CreateDocumentDone_Proc);
            CreateDocumentProcessor cdProcessor = new CreateDocumentProcessor(document, SelectedTemplate, done_del);
            cdProcessor.Operate();
        }
        #endregion

        private void CreateDocumentDone_Proc(string message) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                MessageBox.Show("Документ создан успешно!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }));
            ButtonCreateDocEnabled = true;
        }

        public CreateDocumentViewModel()
        {
            dc = new DocumentDbContext();

            SelectedProducts = new ObservableCollection<Product>();
            //Documents = new ObservableCollection<Document>(dc.Documents);
            //Templates = new ObservableCollection<object>(база);
            //Products = new ObservableCollection<object>(база);
            //Instructions = new ObservableCollection<object>(база);
        }
    }
}
