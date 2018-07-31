using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace EstimateBuilder.MVVM
{
    public class EstimateSplashVM : SplashVM
    {
        const string SPLASH_TITLE = "Welcome to Estimate Builder";
        const string SPLASH_SUBTITLE = "Please select an existing file or create a new bid";

        private List<string> fileExtensions = new List<string> { ".edb", ".tdb" };

        private string defaultTemplatesDirectory;
        
        public override string FileText { get { return "Bid File:"; } }

        public ICommand GetDefaultTemplatesPathCommand { get; protected set; }

        public string DefaultTemplatesPath
        {
            get { return EBSettings.DefaultTemplatesPath; }
            set
            {
                EBSettings.DefaultTemplatesPath = value;
                RaisePropertyChanged("DefaultTemplatesPath");
                EBSettings.Save();
            }
        }

        public override string FirstRecentFile
        {
            get { return EBSettings.FirstRecentBid; }
        }
        public override string SecondRecentFile
        {
            get { return EBSettings.SecondRecentBid; }
        }
        public override string ThirdRecentFile
        {
            get { return EBSettings.ThirdRecentBid; }
        }
        public override string FourthRecentFile
        {
            get { return EBSettings.FourthRecentBid; }
        }
        public override string FifthRecentFile
        {
            get { return EBSettings.FifthRecentBid; }
        }
        
        public event Action<string, string> EditorStarted;

        public EstimateSplashVM(string defaultDirectory, string defaultTemplatesDirectory) :
            base(SPLASH_TITLE, SPLASH_SUBTITLE, defaultDirectory)
        {
            this.defaultTemplatesDirectory = defaultTemplatesDirectory;
            
            GetPathCommand = new RelayCommand(getBidPathExecute);
            ClearPathCommand = new RelayCommand(clearBidPathExecute);
            GetDefaultTemplatesPathCommand= new RelayCommand(getTemplatesPathExecute);
            OpenExistingCommand = new RelayCommand(openExistingExecute, openExistingCanExecute);
            CreateNewCommand = new RelayCommand(createNewExecute, createNewCanExecute);
        }
        
        private void getBidPathExecute()
        {
            string path = UIHelpers.GetLoadPath(FileDialogParameters.EstimateFileParameters, defaultDirectory);
            if(path != null)
            {
                FilePath = path;
            }
        }
        private void clearBidPathExecute()
        {
            FilePath = "";
        }
        private void getTemplatesPathExecute()
        {
            string path = UIHelpers.GetLoadPath(FileDialogParameters.TemplatesFileParameters, defaultTemplatesDirectory);
            if(path != null)
            {
                DefaultTemplatesPath = path;
            }
        }

        private void openExistingExecute()
        {
            LoadingText = "Loading...";
            if (!File.Exists(FilePath))
            {
                MessageBox.Show("Bid file no longer exists at that path.");
                LoadingText = "";
            }
            else if (!File.Exists(DefaultTemplatesPath))
            {
                MessageBox.Show("Templates file no longer exist at that path.");
                LoadingText = "";
            }
            else
            {
                EditorStarted?.Invoke(FilePath, DefaultTemplatesPath);
            }
        }
        private bool openExistingCanExecute()
        {
            return (FilePath != "" && FilePath != null);
        }
    
        private void createNewExecute()
        {
            LoadingText = "Loading...";
            if (!File.Exists(DefaultTemplatesPath))
            {
                MessageBox.Show("Templates file no longer exist at that path.");
                LoadingText = "";
            }
            else
            {
                EditorStarted?.Invoke("", DefaultTemplatesPath);
            }
        }
        private bool createNewCanExecute()
        {
            return true;
        }

        private void chooseRecentBidExecute(string path)
        {
            FilePath = path;
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.FileDragOver(dropInfo, fileExtensions);
        }

        public override void Drop(IDropInfo dropInfo)
        {
            string path = UIHelpers.FileDrop(dropInfo, fileExtensions);
            if(path != null)
            {
                string ext = Path.GetExtension(path);
                switch(ext)
                {
                    case ".edb":
                        FilePath = path;
                        break;
                    case ".tdb":
                        DefaultTemplatesPath = path;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
