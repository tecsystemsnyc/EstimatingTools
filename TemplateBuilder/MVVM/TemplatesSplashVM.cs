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

        public override string FirstRecentTemplates
        {
            get { return TBSettings.FirstRecentTemplates; }
        }
        public override string SecondRecentTemplates
        {
            get { return TBSettings.SecondRecentTemplates; }
        }
        public override string ThirdRecentTemplates
        {
            get { return TBSettings.ThirdRecentTemplates; }
        }
        public override string FourthRecentTemplates
        {
            get { return TBSettings.FourthRecentTemplates; }
        }
        public override string FifthRecentTemplates
        {
            get { return TBSettings.FifthRecentTemplates; }
        }

        public TemplatesSplashVM(string templatesPath, string defaultDirectory) :
            base(SPLASH_TITLE, SPLASH_SUBTITLE, defaultDirectory)
        {
            TemplatesPath = templatesPath;

            GetTemplatesPathCommand = new RelayCommand(getTemplatesPathExecute);
            ClearTemplatesPathCommand = new RelayCommand(clearTemplatesPathExecute);
            OpenExistingCommand = new RelayCommand(openExistingExecute, openExistingCanExecute);
            CreateNewCommand = new RelayCommand(createNewExecute, createNewCanExecute);
        }

        private void getTemplatesPathExecute()
        {
            string path = UIHelpers.GetLoadPath(FileDialogParameters.TemplatesFileParameters, defaultDirectory);
            if (path != null)
            {
                TemplatesPath = path;
            }
        }
        private void clearTemplatesPathExecute()
        {
            TemplatesPath = "";
        }

        private void openExistingExecute()
        {
            LoadingText = "Loading...";
            if (!File.Exists(TemplatesPath))
            {
                MessageBox.Show("Templates file no longer exist at that path.");
                LoadingText = "";
            }
            else
            {
                EditorStarted?.Invoke(TemplatesPath);
            }
        }
        private bool openExistingCanExecute()
        {
            return (TemplatesPath != "" && TemplatesPath != null);
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
            UIHelpers.FileDragOver(dropInfo, fileExtensions);
        }

        public override void Drop(IDropInfo dropInfo)
        {
            string path = UIHelpers.FileDrop(dropInfo, fileExtensions);
            if (path != null)
            {
                string ext = Path.GetExtension(path);
                switch (ext)
                {
                    case ".tdb":
                        TemplatesPath = path;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
