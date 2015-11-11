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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace PublicOrders.ViewModels
{
    public class LoadProductsViewModel : INotifyPropertyChanged
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string _docPath;
        private bool _isLoadInProcess;
        private ImageSource _selectedTemplateImage;

        public string DocPath
        {
            get { return _docPath; }
            set
            {
                _docPath = value;
                OnPropertyChanged("DocPath");
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

        private Rubric _selectedRubric;
        public Rubric SelectedRubric
        {
            get
            {
                return _selectedRubric;
            }
            set
            {
                _selectedRubric = value;
                OnPropertyChanged("SelectedRubric");
            }
        }

        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get
            {
                return _selectedTemplate;
            }
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged("SelectedTemplate");

                if (!string.IsNullOrEmpty("TheImageYouWantToShow"))
                {

                    BitmapImage yourImage = null;
                    switch (_selectedTemplate)
                    {
                        //case "Комитет":
                        //    yourImage = new BitmapImage(new Uri(Path.GetFullPath("Image/3.png")));

                        //    break;
                        //case "Свобода":
                        //    yourImage = new BitmapImage(new Uri(String.Format("Image/1.png"), UriKind.Relative));

                        //    break;
                        //case "Форма 2":
                        //    yourImage = new BitmapImage(new Uri(String.Format("Image/2.png"), UriKind.Relative));

                        //    break;
                    }

                    //yourImage.Freeze(); // -> to prevent error: "Must create DependencySource on same Thread as the DependencyObject"
                    SelectedTemplateImage = yourImage;
                }
                else
                {
                    SelectedTemplateImage = null;
                }

                
            }
        }


        public ImageSource SelectedTemplateImage {
            get
            {
                return _selectedTemplateImage;
            } set
            {
                _selectedTemplateImage = value;
                OnPropertyChanged("SelectedTemplateImage");
            }
        }

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

        private void Load(object param)
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
            if (SelectedRubric == null)
            {
                MessageBox.Show("Выберите рубрику!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            IsLoadInProcess = true;

            if ((mvm.lpProcessor != null) && (mvm.lpProcessor.isWorking()))
            {
                mvm.lpProcessor.Stop();
            }

            LoadProductsDone_delegete done_del = new LoadProductsDone_delegete(LoadProductsDone_Proc);
            mvm.lpProcessor = new LoadProductsProcessor(DocPath, SelectedTemplate, SelectedRubric, done_del);
            mvm.lpProcessor.Operate();
        }

        private void OpenFile(object param)
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

        private void LoadProductsDone_Proc(ResultType_enum ResultType_enum, string templateStr, int productsAddedCount, int productsRepeatCount, int productsMergeCount, string message) {
            switch (ResultType_enum)
            {
                case (ResultType_enum.Done):
                    if ((productsAddedCount == 0) && (productsMergeCount == 0))
                    {
                        MessageBox.Show("В документе не найдено товара для загрузки!", "Предупреждение",
                           MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("Новый товар по шаблону <" + templateStr.Trim() + "> загружен!\nДобавлено: " +
                                           productsAddedCount + ";\nСлитие: " + productsMergeCount + ";\nПовторы: " +
                                           productsRepeatCount + ".", "Информация",
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
            this.IsLoadInProcess = false;
        }

        public LoadProductsViewModel()
        {
            this.DocPath = "";

            //Rubrics = new ObservableCollection<Rubric>(mvm.RubricCollection);
            if (SelectedRubric == null) {
                SelectedRubric = mvm.RubricCollection.FirstOrDefault(m => m.Name == "--Без рубрики--");
            }

            //Templates = mvm.TemplateCollection;
            if (SelectedTemplate == null)
            {
                SelectedTemplate = mvm.TemplateCollection[0];
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
