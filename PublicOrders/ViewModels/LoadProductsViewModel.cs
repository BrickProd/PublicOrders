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
using PublicOrders.Processors.Main;

namespace PublicOrders.ViewModels
{
    public class LoadProductsViewModel : INotifyPropertyChanged
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string _docPath;
        private bool _buttonLoadProdsEnabled;
        private bool _isLoadInProcess;

        public string DocPath
        {
            get { return _docPath; }
            set
            {
                _docPath = value;
                OnPropertyChanged("DocPath");
                this.ButtonLoadProdsEnabled = this.DocPath != "";
            }
        }
        public bool ButtonLoadProdsEnabled
        {
            get { return _buttonLoadProdsEnabled; }
            set
            {
                _buttonLoadProdsEnabled = value;
                OnPropertyChanged("ButtonLoadProdsEnabled");
            }
        }
        public bool IsLoadInProcess
        {
            get { return _isLoadInProcess; }
            set
            {
                _isLoadInProcess = value;
                OnPropertyChanged("IsLoadInProcess");
            }
        }
        public Visibility BusyVisibility
        {
            get
            {
                return IsLoadInProcess ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ObservableCollection<string> Templates { get; set; }
        public string SelectedTemplate { get; set; }
        
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
            IsLoadInProcess = true;

            if ((mvm.lpProcessor != null) && (mvm.lpProcessor.isWorking()))
            {
                mvm.lpProcessor.Stop();
            }

            LoadProductsDone_delegete done_del = new LoadProductsDone_delegete(LoadProductsDone_Proc);
            mvm.lpProcessor = new LoadProductsProcessor(DocPath, SelectedTemplate, done_del);
            mvm.lpProcessor.Operate();
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

        private void LoadProductsDone_Proc(ResultType_enum ResultType_enum, string templateStr, int productsAddedCount, int productsRepeatCount, string message) {
            switch (ResultType_enum)
            {
                case (ResultType_enum.Done):
                    if (productsAddedCount == 0)
                    {
                        MessageBox.Show("В документе не найдено товара для загрузки!", "Предупреждение",
                           MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("Новый товар по шаблону <" + templateStr.Trim() + "> загружен!\nДобавлено: " +
                                           productsAddedCount + "\nПовторы: " + productsRepeatCount, "Информация",
                                           MessageBoxButton.OK, MessageBoxImage.Information);
                    }


                    break;
                case (ResultType_enum.Error):
                    MessageBox.Show("Ошибка при загрузке товара!\n" + message, "Ошибка",
                       MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    return;
            }

            ButtonLoadProdsEnabled = true;
            this.IsLoadInProcess = false;
        }

        public LoadProductsViewModel()
        {
            this.ButtonLoadProdsEnabled = false;
            this.IsLoadInProcess = false;

            Templates = new ObservableCollection<string>(new List<string> {
                        "Комитет",
                        "Свобода",
                        "Форма 2"
                    });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
