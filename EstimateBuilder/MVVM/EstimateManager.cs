using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary;
using EstimatingUtilitiesLibrary.Database;
using EstimatingUtilitiesLibrary.Exports;
using NLog;
using System;
using System.IO;
using System.Windows;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Debug;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.Windows;

namespace EstimateBuilder.MVVM
{
    public class EstimateManager : AppManager<TECBid>
    {
        #region Fields and Properties
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private TECBid bid;
        private TECTemplates templates;
        private TECEstimator estimate;

        private DatabaseManager<TECTemplates> templatesDatabaseManager;

        private string currentBidPath = "";
        private string currentTemplatesPath = "";

        /// <summary>
        /// Estimate-typed splash vm for manipulation
        /// </summary>
        private EstimateMenuVM menuVM
        {
            get { return MenuVM as EstimateMenuVM; }
        }
        /// <summary>
        /// Estimate-typed splash vm for manipulation
        /// </summary>
        private EstimateEditorVM editorVM
        {
            get { return EditorVM as EstimateEditorVM; }
        }
        /// <summary>
        /// Estimate-typed splash vm for manipulation
        /// </summary>
        private EstimateSplashVM splashVM
        {
            get { return SplashVM as EstimateSplashVM; }
        }

        override protected FileDialogParameters workingFileParameters
        {
            get
            {
                return FileDialogParameters.EstimateFileParameters;
            }
        }
        override protected string defaultFileName
        {
            get
            {
                string fileName = "";
                if (bid.Name != null && bid.Name != "")
                {
                    fileName += bid.Name;
                    if (bid.BidNumber != null && bid.BidNumber != "")
                    {
                        fileName += (" - " + bid.BidNumber);
                    }
                }
                return fileName;
            }
        }
        override protected string defaultDirectory
        {
            get { return EBSettings.BidDirectory; }
        }
        private string defaultTemplatesDirectory
        {
            get { return EBSettings.TemplatesDirectory; }
        }
        #endregion

        public EstimateManager() : base("Estimate Builder", 
            new EstimateSplashVM(templatesPath: EBSettings.FirstRecentTemplates, defaultDirectory: EBSettings.BidDirectory, defaultTemplatesDirectory: EBSettings.TemplatesDirectory),
            new EstimateMenuVM())
        {
            splashVM.BidPath = EBSettings.StartUpFilePath;
            splashVM.EditorStarted += userStartedEditorHandler;
            TitleString = "Estimate Builder";
            setupCommands();
        }
        
        private void userStartedEditorHandler(string bidFilePath, string templatesFilePath)
        {
            this.currentBidPath = bidFilePath;
            this.currentTemplatesPath = templatesFilePath;
            
            updateRecentBidSettings(bidFilePath);
            updateRecentTemplatesSettings(templatesFilePath);

            buildTitleString(bidFilePath, "Estimate Builder");
            if(templatesFilePath != "")
            {
                templatesDatabaseManager = new DatabaseManager<TECTemplates>(templatesFilePath);
                templatesDatabaseManager.LoadComplete += assignData;
                ViewEnabled = false;
                templatesDatabaseManager.AsyncLoad();
            }
            else
            {
                assignData(new TECTemplates());
            }

            void assignData(TECTemplates loadedTemplates)
            {
                if (templatesDatabaseManager != null)
                {
                    templatesDatabaseManager.LoadComplete -= assignData;
                }

                templates = loadedTemplates;
                if (bidFilePath != "")
                {
                    ViewEnabled = false;
                    databaseManager = new DatabaseManager<TECBid>(bidFilePath);
                    databaseManager.LoadComplete += handleLoaded;
                    databaseManager.AsyncLoad();
                }
                else
                {
                    handleLoaded(getNewWorkingScope());
                }
            }
        }

        protected override void handleLoaded(TECBid loadedBid)
        {
            if (loadedBid != null && templates != null)
            {
                bid = loadedBid;
                watcher = new ChangeWatcher(bid);
                doStack = new DoStacker(watcher);
                deltaStack = new DeltaStacker(watcher, bid);
                bid.Catalogs.Fill(templates.Catalogs);

                estimate = new TECEstimator(bid, watcher);

                EditorVM = new EstimateEditorVM(bid, templates, watcher, estimate);
                CurrentVM = EditorVM;
            }
            else
            {
                this.splashVM.LoadingText = "";
            }
            ViewEnabled = true;
        }
        private void handleLoadedTemplates(TECTemplates templates)
        {
            this.templates = templates;
            bid.Catalogs.Fill(templates.Catalogs);
            ModelLinkingHelper.LinkBidToCatalogs(bid);
            estimate = new TECEstimator(bid, watcher);
            EditorVM = new EstimateEditorVM(bid, templates, watcher, estimate);
        }
        
        #region Menu Commands Methods
        private void setupCommands()
        {
            menuVM.SetLoadTemplatesCommand(loadTemplatesExecute, canLoadTemplates);
            menuVM.SetRefreshBidCommand(refreshExecute, canRefresh);
            menuVM.SetRefreshTemplatesCommand(refreshTemplatesExecute, canRefreshTemplates);
            menuVM.SetExportProposalCommand(exportProposalExecute, canExportProposal);
            menuVM.SetExportTurnoverCommand(exportTurnoverExecute, canExportTurnover);
            menuVM.SetExportPointsListExcelCommand(exportPointsListExcelExecute, canExportPointsListExcel);
            menuVM.SetExportPointsListCSVCommand(exportPointsListCSVExecute, canExportPointsListCSV);
            menuVM.SetExportSummaryCommand(exportSummaryExecute, canExportSummary);
            menuVM.SetExportBudgetCommand(exportBudgetExecute, canExportBudget);
            menuVM.SetExportBOMCommand(exportBOMExecute, canExportBOM);
            menuVM.SetDebugWindowCommand(debugWindowExecute, canDebugWindow);
            menuVM.SetUpdateCatalogsCommand(updateCatalogsExecute, canUpdateCatalogs);
        }
        
        //Load Templates
        private void loadTemplatesExecute()
        {
            string message = "Would you like to save your changes before loading new templates?";

            checkForChanges(message, loadTemplates);

            void loadTemplates()
            {
                string loadFilePath = UIHelpers.GetLoadPath(FileDialogParameters.TemplatesFileParameters, defaultTemplatesDirectory);
                if (loadFilePath != null)
                {
                    ViewEnabled = false;
                    StatusBarVM.CurrentStatusText = "Loading Templates...";
                    templatesDatabaseManager = new DatabaseManager<TECTemplates>(loadFilePath);
                    templatesDatabaseManager.LoadComplete += handleTemplatesLoadComplete;
                    templatesDatabaseManager.AsyncLoad();
                }
            }
        }
        private bool canLoadTemplates()
        {
            return databaseReady();
        }
        private void handleTemplatesLoadComplete(TECTemplates templates)
        {
            handleLoadedTemplates(templates);
            StatusBarVM.CurrentStatusText = "Ready";
            ViewEnabled = true;
        }
        //Refresh Templates
        private void refreshTemplatesExecute()
        {
            string message = "Would you like to save your changes before refreshing?";
            checkForChanges(message, refreshTemplates, () => { ViewEnabled = true; });

            void refreshTemplates()
            {
                ViewEnabled = false;
                StatusBarVM.CurrentStatusText = "Loading...";
                templatesDatabaseManager.LoadComplete += handleTemplatesLoadComplete;
                templatesDatabaseManager.AsyncLoad();
            }
        }
        private bool canRefreshTemplates()
        {
            return templatesDatabaseManager != null && databaseReady();
        }
        //Update Catalogs
        private void updateCatalogsExecute()
        {
            string message = "Updating catalogs could change pricing. Continue?";
            MessageBoxResult result = MessageBox.Show(message, "Save", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    bid.Catalogs.Unionize(templates.Catalogs);
                    ModelLinkingHelper.LinkBidToCatalogs(bid);
                    estimate = new TECEstimator(bid, watcher);
                    EditorVM = new EstimateEditorVM(bid, templates, watcher, estimate);
                    break;
                case MessageBoxResult.No:
                    return;
                default:
                    return;
            }

        }
        private bool canUpdateCatalogs()
        {
            return templates != null;
        }
        //Export Proposal
        private void exportProposalExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.WordDocumentFileParameters, 
                Exporter.ProposalDefaultName(bid), defaultDirectory);

            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GenerateProposal(path, bid, estimate, EBSettings.OpenFileOnExport);
                    logger.Info("Scope saved to document.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        private bool canExportProposal()
        {
            return true;
        }
        //Export Turnover Sheet
        private void exportTurnoverExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.ExcelFileParameters,
                                        Exporter.TurnoverDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GenerateTurnover(path, bid, estimate, EBSettings.OpenFileOnExport);
                    logger.Info("Exported to turnover document.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        private bool canExportTurnover()
        {
            return true;
        }
        //Export Points List (Excel)
        private void exportPointsListExcelExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.ExcelFileParameters,
                            Exporter.PointsListDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GeneratePointsList(path, bid, EBSettings.OpenFileOnExport);
                    logger.Info("Points saved to Excel.");
                }
                else
                {
                    logger.Warn("Could not open file {0}. File is open elsewhere.", path);
                }
            }
        }
        private bool canExportPointsListExcel()
        {
            return true;
        }
        //Export Points List (CSV)
        private void exportPointsListCSVExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.CSVFileParameters,
                            Exporter.PointsListDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GeneratePointsListCSV(path, bid, EBSettings.OpenFileOnExport);
                    logger.Info("Points saved to csv.");
                }
                else
                {
                    logger.Warn("Could not open file {0}. File is open elsewhere.", path);
                }
            }
        }
        private bool canExportPointsListCSV()
        {
            return true;
        }
        //Export Summary
        private void exportSummaryExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.WordDocumentFileParameters,
                                        Exporter.SummaryDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GenerateTurnover(path, bid, estimate);
                    logger.Info("Exported to summary turnover document.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        private bool canExportSummary()
        {
            return true; 
        }
        //Export Budget
        private void exportBudgetExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.ExcelFileParameters,
                                        Exporter.BudgetDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GenerateBudget(path, bid, EBSettings.OpenFileOnExport);
                    logger.Info("Exported to budget document.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        private bool canExportBudget()
        {
            return true;
        }
        //Export Budget
        private void exportBOMExecute()
        {
            string path = UIHelpers.GetSavePath(FileDialogParameters.ExcelFileParameters,
                                        Exporter.BOMDefaultName(bid), defaultDirectory);
            if (path != null)
            {
                if (!UtilitiesMethods.IsFileLocked(path))
                {
                    Exporter.GenerateBOM(path, bid, EBSettings.OpenFileOnExport);
                    logger.Info("Exported to BOM document.");
                }
                else
                {
                    notifyFileLocked(path);
                }
            }
        }
        private bool canExportBOM()
        {
            return true;
        }
        //Debug Window
        private void debugWindowExecute()
        {
            var dbWindow = new EBDebugWindow(bid);
            dbWindow.Show();
        }
        private bool canDebugWindow()
        {
            return true;
        }
        //Settings
        protected override void settingsExecute()
        {
            EBSettingsWindow settingsWindow = new EBSettingsWindow();
            settingsWindow.Show();
        }
        //Report Bug
        protected override void reportBugExecute()
        {
            string reportPrompt = "Please describe the bug and how to reproduce it.";

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EstimateBuilder\\logs");
            string logPath = UtilitiesMethods.GetMostRecentFilePathFromDirectoryPath(logDirectory);

            BugReportWindow reportWindow = new BugReportWindow("Estimate Builder Bug", reportPrompt, logPath);
            reportWindow.ShowDialog();
        }
        #endregion

        protected override TECBid getWorkingScope()
        {
            return bid;
        }
        protected override TECBid getNewWorkingScope()
        {
            TECBid outBid = new TECBid();
            if(templates!= null && templates.Parameters.Count > 0)
            {
                outBid.Parameters = templates.Parameters[0];
            }
            return outBid;
        }

        private void updateRecentBidSettings(string bidPath)
        {
            if (bidPath != null && bidPath != "")
            {
                string first = EBSettings.FirstRecentBid;
                string second = EBSettings.SecondRecentBid;
                string third = EBSettings.ThirdRecentBid;
                string fourth = EBSettings.FourthRecentBid;
                string fifth = EBSettings.FifthRecentBid;

                string limbo = bidPath;

                if (limbo == first)
                {
                    EBSettings.Save();
                    return;
                } 
                else
                {
                    EBSettings.FirstRecentBid = limbo;
                    limbo = first;
                }

                if (limbo == second)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.SecondRecentBid = limbo;
                    limbo = second;
                }

                if (limbo == third)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.ThirdRecentBid = limbo;
                    limbo = third;
                }

                if (limbo == fourth)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.FourthRecentBid = limbo;
                    limbo = fourth;
                }

                if (limbo == fifth)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.FifthRecentBid = limbo;
                }

                EBSettings.Save();
            }
        }
        private void updateRecentTemplatesSettings(string templatesPath)
        {
            if (templatesPath != null && templatesPath != "")
            {
                string first = EBSettings.FirstRecentTemplates;
                string second = EBSettings.SecondRecentTemplates;
                string third = EBSettings.ThirdRecentTemplates;
                string fourth = EBSettings.FourthRecentTemplates;
                string fifth = EBSettings.FifthRecentTemplates;

                string limbo = templatesPath;

                if (limbo == first)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.FirstRecentTemplates = limbo;
                    limbo = first;
                }

                if (limbo == second)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.SecondRecentTemplates = limbo;
                    limbo = second;
                }

                if (limbo == third)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.ThirdRecentTemplates = limbo;
                    limbo = third;
                }

                if (limbo == fourth)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.FourthRecentTemplates = limbo;
                    limbo = fourth;
                }

                if (limbo == fifth)
                {
                    EBSettings.Save();
                    return;
                }
                else
                {
                    EBSettings.FifthRecentTemplates = limbo;
                }

                EBSettings.Save();
            }
        }
    }
}
