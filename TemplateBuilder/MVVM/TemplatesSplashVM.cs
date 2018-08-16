using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TemplateBuilder.MVVM
{
    public class TemplatesSplashVM : SplashVM
    {
        const string SPLASH_TITLE = "Welcome to Template Builder";
        const string SPLASH_SUBTITLE = "Please select object templates or create new templates";

        private List<string> fileExtensions = new List<string> { ".tdb" };
        
        public event Action<string> EditorStarted;

        public override string FirstRecentFile
        {
            get { return TBSettings.FirstRecentTemplates; }
        }
        public override string SecondRecentFile
        {
            get { return TBSettings.SecondRecentTemplates; }
        }
        public override string ThirdRecentFile
        {
            get { return TBSettings.ThirdRecentTemplates; }
        }
        public override string FourthRecentFile
        {
            get { return TBSettings.FourthRecentTemplates; }
        }
        public override string FifthRecentFile
        {
            get { return TBSettings.FifthRecentTemplates; }
        }

        public override string FileText => "Templates File:";
        
        public TemplatesSplashVM(string templatesPath, string defaultDirectory) :
            base(SPLASH_TITLE, SPLASH_SUBTITLE, defaultDirectory)
        {
            FilePath = templatesPath;

            GetPathCommand = new RelayCommand(getTemplatesPathExecute);
            ClearPathCommand = new RelayCommand(clearTemplatesPathExecute);
            OpenExistingCommand = new RelayCommand(openExistingExecute, openExistingCanExecute);
            CreateNewCommand = new RelayCommand(createNewExecute, createNewCanExecute);
        }

        private void getTemplatesPathExecute()
        {
            string path = UIHelpers.GetLoadPath(FileDialogParameters.TemplatesFileParameters, defaultDirectory);
            if (path != null)
            {
                FilePath = path;
            }
        }
        private void clearTemplatesPathExecute()
        {
            FilePath = "";
        }

        private void openExistingExecute()
        {
            LoadingText = "Loading...";
            if (!File.Exists(FilePath))
            {
                MessageBox.Show("Templates file no longer exist at that path.");
                LoadingText = "";
            }
            else
            {
                EditorStarted?.Invoke(FilePath);
            }
        }
        private bool openExistingCanExecute()
        {
            return (FilePath != "" && FilePath != null);
        }

        private void createNewExecute()
        {
            LoadingText = "Loading...";
            EditorStarted?.Invoke("");
        }
        private bool createNewCanExecute()
        {
            return true;
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            DragDropHelpers.FileDragOver(dropInfo, fileExtensions);
        }

        public override void Drop(IDropInfo dropInfo)
        {
            string path = DragDropHelpers.FileDrop(dropInfo, fileExtensions);
            if (path != null)
            {
                string ext = Path.GetExtension(path);
                switch (ext)
                {
                    case ".tdb":
                        FilePath = path;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
