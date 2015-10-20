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
using System.Windows;
using System.IO;
using PublicOrders.Processors;
using Microsoft.Win32;

namespace PublicOrders.ViewModels
{
    public class LoadProductsViewModel : INotifyPropertyChanged
    {
        DocumentDbContext dc = null;

        private string _docPath;
        public string DocPath
        {
            get { return _docPath; }
            set
            {
                _docPath = value;
                OnPropertyChanged("DocPath");
            }
        }

        private bool _buttonLoadProdsEnabled = true;
        public bool ButtonLoadProdsEnabled
        {
            get { return _buttonLoadProdsEnabled; }
            set
            {
                _buttonLoadProdsEnabled = value;
                OnPropertyChanged("ButtonLoadProdsEnabled");
            }
        }

        public ObservableCollection<Template> Templates { get; set; }
        public Template SelectedTemplate { get; set; }
        
        #region КОМАНДЫ
        private DelegateCommand loadCommand;
        private DelegateCommand openFileCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (loadCommand == null)
                {
                    loadCommand = new DelegateCommand(Load);
                }
                return loadCommand;
            }
        }
        public ICommand OpenFileCommand
        {
            get
            {
                if (openFileCommand == null)
                {
                    openFileCommand = new DelegateCommand(OpenFile);
                }
                return openFileCommand;
            }
        }

        private void Load()
        {
            if (DocPath.Trim() == "")
            {
                MessageBox.Show("Выберите документ для загрузки товара!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!File.Exists(DocPath.Trim()))
            {
                MessageBox.Show("Документа по выбранному пути не существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ButtonLoadProdsEnabled = false;
            LoadProductsDone_delegete done_del = new LoadProductsDone_delegete(LoadProductsDone_Proc);
            LoadProductsProcessor lpProcessor = new LoadProductsProcessor(DocPath, SelectedTemplate, done_del);
            lpProcessor.Operate();
        }

        private void OpenFile()
        {
            Stream myStream = null;
            OpenFileDialog openLoadingFileDialog = new OpenFileDialog();

            if ((DocPath != null) && (DocPath.Trim() != ""))
            {
                FileInfo fi = new FileInfo(DocPath);
                if (!fi.Exists)
                {
                    openLoadingFileDialog.InitialDirectory = "c:\\";
                }
                else {
                    openLoadingFileDialog.InitialDirectory = fi.Directory.FullName;
                }
            }
            else {
                openLoadingFileDialog.InitialDirectory = "c:\\";
            }
            openLoadingFileDialog.Filter = "doc/docx files (*.doc/*.docx)|*.doc*";
            openLoadingFileDialog.FilterIndex = 2;
            openLoadingFileDialog.RestoreDirectory = true;

            if (openLoadingFileDialog.ShowDialog() == true)
            {
                try
                {
                    DocPath = openLoadingFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        #endregion

        private void LoadProductsDone_Proc(ResultType resultType, Template template, int productsAddedCount, int productsRepeatCount, string message) {
            switch (resultType)
            {
                case (ResultType.Done):
                    if (productsAddedCount == 0)
                    {
                        MessageBox.Show("В документе не найдено товара для загрузки!", "Предупреждение",
                           MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("Новый товар по шаблону <" + template.Name + "> загружен!\nДобавлено: " +
                                           productsAddedCount + "\nПовторы: " + productsRepeatCount, "Информация",
                                           MessageBoxButton.OK, MessageBoxImage.Information);
                    }


                    break;
                case (ResultType.Error):
                    MessageBox.Show("Ошибка при загрузке товара!\n" + message, "Ошибка",
                       MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case (ResultType.ErrorDB):
                    MessageBox.Show("Ошибка при загрузке товара!\n" + message, "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    return;
            }

            ButtonLoadProdsEnabled = true;
        }

        public LoadProductsViewModel()
        {
            dc = new DocumentDbContext();
            Templates = new ObservableCollection<Template>(dc.Templates);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
