using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary;
using EstimatingUtilitiesLibrary.Database;
using EstimatingUtilitiesLibrary.Exports;
using NLog;
using System;
using System.Deployment.Application;
using System.IO;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.Windows;

namespace TemplateBuilder.MVVM
{
    public class TemplatesManager : AppManager<TECTemplates>
    {
        #region Fields and Properties
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private TECTemplates templates;

        private TemplatesMenuVM menuVM
        {
            get { return MenuVM as TemplatesMenuVM; }
        }
        private TemplatesEditorVM editorVM
        {
            get { return EditorVM as TemplatesEditorVM; }
        }
        private TemplatesSplashVM splashVM
        {
            get { return SplashVM as TemplatesSplashVM; }
        }

        override protected FileDialogParameters workingFileParameters
        {
            get
            {
                return FileDialogParameters.TemplatesFileParameters;
            }
        }
        override protected string defaultFileName
        {
            get
            {
                return string.Format("Templates v{0}", Version);
            }
        }
        override protected string defaultDirectory
        {
            get { return TBSettings.TemplatesDirectory; }
        }
        #endregion

        public TemplatesManager() : base("Template Builder",
            new TemplatesSplashVM(TBSettings.FirstRecentTemplates, TBSettings.TemplatesDirectory), new TemplatesMenuVM())
        {
            string startUpFilePath = getStartUpFilePath();
            if (startUpFilePath != null && startUpFilePath != "")
            {
                splashVM.TemplatesPath = startUpFilePath;
            }
            splashVM.EditorStarted += userStartedEditorHandler;
            TitleString = "Template Builder";
            setupCommands();
        }

        private void userStartedEditorHandler(string path)
        {
            updateRecentTemplatesSettings(path);
            buildTitleString(path, "TemplateBuilder");
            if(path != "")
            {
                databaseManager = new DatabaseManager<TECTemplates>(path);
                databaseManager.LoadComplete += handleLoaded;
                ViewEnabled = false;
                databaseManager.AsyncLoad();
            }
            else
            {
                handleLoaded(new TECTemplates());
            }
        }

        protected override void handleLoaded(TECTemplates loaded)
        {
            templates = loaded;
            watcher = new ChangeWatcher(templates);
            doStack = new DoStacker(watcher);
            deltaStack = new DeltaStacker(watcher, templates);

            EditorVM = new TemplatesEditorVM(templates);
            CurrentVM = EditorVM;
            ViewEnabled = true;
        }

        #region Menu Commands Methods
        private void setupCommands()
        {
            menuVM.SetRefreshTemplatesCommand(refreshExecute, canRefresh);
            menuVM.SetExportTemplatesCommand(exportTemplatesExecute);
        }

        //Export Templates
        private void exportTemplatesExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.ExcelFileParameters,
                defaultFileName, defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Templates.Export(path, templates);
                    logger.Info("Exported templates spreadsheet.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        //Settings
        protected override void settingsExecute()
        {
            TBSettingsWindow settingsWindow = new TBSettingsWindow();
            settingsWindow.Show();
        }
        //Report Bug
        protected override void reportBugExecute()
        {
            string reportPrompt = "Please describe the bug and how to reproduce it.";

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TemplateBuilder\\logs");
            string logPath = UtilitiesMethods.GetMostRecentFilePathFromDirectoryPath(logDirectory);

            BugReportWindow reportWindow = new BugReportWindow("Template Builder Bug", reportPrompt, logPath);
            reportWindow.ShowDialog();
        }
        #endregion

        private string getStartUpFilePath()
        {
            string startUpFilePath = Properties.Settings.Default.StartUpFilePath;
            Properties.Settings.Default.StartUpFilePath = null;
            Properties.Settings.Default.Save();
            return startUpFilePath;
        }
        protected override TECTemplates getWorkingScope()
        {
            return templates;
        }
        protected override TECTemplates getNewWorkingScope()
        {
            return new TECTemplates();
        }

        private void updateRecentTemplatesSettings(string templatesPath)
        {
            if (templatesPath != null && templatesPath != "")
            {
                string first = TBSettings.FirstRecentTemplates;
                string second = TBSettings.SecondRecentTemplates;
                string third = TBSettings.ThirdRecentTemplates;
                string fourth = TBSettings.FourthRecentTemplates;
                string fifth = TBSettings.FifthRecentTemplates;

                string limbo = templatesPath;

                if (limbo == first)
                {
                    TBSettings.Save();
                    return;
                }
                else
                {
                    TBSettings.FirstRecentTemplates = limbo;
                    limbo = first;
                }

                if (limbo == second)
                {
                    TBSettings.Save();
                    return;
                }
                else
                {
                    TBSettings.SecondRecentTemplates = limbo;
                    limbo = second;
                }

                if (limbo == third)
                {
                    TBSettings.Save();
                    return;
                }
                else
                {
                    TBSettings.ThirdRecentTemplates = limbo;
                    limbo = third;
                }

                if (limbo == fourth)
                {
                    TBSettings.Save();
                    return;
                }
                else
                {
                    TBSettings.FourthRecentTemplates = limbo;
                    limbo = fourth;
                }

                if (limbo == fifth)
                {
                    TBSettings.Save();
                    return;
                }
                else
                {
                    TBSettings.FifthRecentTemplates = limbo;
                }

                TBSettings.Save();
            }
        }

        protected override void setDefaultDirectory(string path)
        {
            TBSettings.TemplatesDirectory = Path.GetDirectoryName(path);
        }
    }
}
