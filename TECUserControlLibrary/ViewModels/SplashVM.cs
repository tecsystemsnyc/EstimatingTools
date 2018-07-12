using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using System.Windows.Input;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels
{
    public abstract class SplashVM : ViewModelBase, IDropTarget
    {
        protected string defaultDirectory;

        private string _titleText;
        private string _subtitleText;
        private string _loadingText;
        private string _filePath;

        public string TitleText
        {
            get { return _titleText; }
            set
            {
                _titleText = value;
                RaisePropertyChanged("TitleText");
            }
        }
        public string SubtitleText
        {
            get { return _subtitleText; }
            set
            {
                _subtitleText = value;
                RaisePropertyChanged("SubtitleText");
            }
        }
        public string LoadingText
        {
            get { return _loadingText; }
            set
            {
                _loadingText = value;
                RaisePropertyChanged("LoadingText");
            }
        }
        public string Version { get; set; }
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }
        abstract public string FileText { get; }

        public ICommand GetPathCommand { get; protected set; }
        public ICommand ClearPathCommand { get; protected set; }
        public ICommand OpenExistingCommand { get; protected set; }
        public ICommand CreateNewCommand { get; protected set; }

        #region Recent Files Properties
        public abstract string FirstRecentFile{ get; }
        public abstract string SecondRecentFile{ get; }
        public abstract string ThirdRecentFile { get; }
        public abstract string FourthRecentFile { get; }
        public abstract string FifthRecentFile { get; }

        public ICommand ChooseRecentFileCommand { get; }
        #endregion

        public SplashVM(string titleText, string subtitleText, string defaultDirectory)
        {
            LoadingText = "";
            this.defaultDirectory = defaultDirectory;
            _titleText = titleText;
            _subtitleText = subtitleText;

            ChooseRecentFileCommand = new RelayCommand<string>(chooseRecentFileExecute, chooseRecentCanExecute);
        }

        public abstract void DragOver(IDropInfo dropInfo);
        public abstract void Drop(IDropInfo dropInfo);

        private void chooseRecentFileExecute(string path)
        {
            FilePath = path;
        }
        protected bool chooseRecentCanExecute(string path)
        {
            return (path != null && path != "");
        }
    }
}