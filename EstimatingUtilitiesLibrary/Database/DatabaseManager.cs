﻿using EstimatingLibrary;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace EstimatingUtilitiesLibrary.Database
{
    public class DatabaseManager<T> where T:TECScopeManager
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();

        private string path;

        public event Action<bool> SaveComplete;
        public event Action<T> LoadComplete;
        public bool IsBusy = false;
        
        public DatabaseManager(string databasePath)
        {
            path = databasePath;
        }
        
        internal bool Save(List<UpdateItem> updates)
        {
            if (!UtilitiesMethods.IsFileLocked(path))
            {
                bool success = DatabaseUpdater.Update(path, updates);
                if (!success)
                {
                    MessageBox.Show("Some items might not have saved properly, check logs for more details.");
                }
                return success;
            }
            else
            {
                logger.Error("Could not open file " + path + " File is open elsewhere.");
                return false;
            }
            
        }
        public void AsyncSave(List<UpdateItem> updates)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                e.Result = catchOnRelease("Save delta failed", () =>
                {
                    Save(updates);
                });

            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                notifySaveComplete((bool)e.Result);
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        internal bool New(TECScopeManager scopeManager)
        {
            if (!UtilitiesMethods.IsFileLocked(path))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (scopeManager is TECBid)
                {
                    DatabaseGenerator.CreateBidDatabase(path);
                }
                else if (scopeManager is TECTemplates)
                {
                    DatabaseGenerator.CreateTemplateDatabase(path);
                }
                else
                {
                    throw new Exception("Generator can only create bid or template DBs");
                }
                List<UpdateItem> newStack = DatabaseNewStacker.NewStack(scopeManager);
                bool success = DatabaseUpdater.Update(path, newStack);
                if (!success)
                {
                    MessageBox.Show("Not all items saved properly, check logs for more details.");
                }
                return true;
            }
            else
            {
                logger.Error("Could not open file " + path + " File is open elsewhere.");
                return false;
            }
            
        }
        public void AsyncNew(TECScopeManager scopeManager)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                e.Result = catchOnRelease("Save New failed", () =>
                {
                    New(scopeManager);
                });

            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                notifySaveComplete((bool)e.Result);
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        internal T Load()
        {
            string appFolder = "EstimateBuilder";
            if(Path.GetExtension(path) == ".tdb")
            {
                appFolder = "TemplateBuilder";
            }
            if(!File.Exists(String.Format("{0}\\{1}\\{2}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appFolder,
                "backups"))){
                Directory.CreateDirectory(String.Format("{0}\\{1}\\{2}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appFolder,
                "backups"));
            }

            string backupPath = String.Format("{0}\\{1}\\{2}\\{3} {4}{5}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appFolder,
                "backups",
                Path.GetFileNameWithoutExtension(path),
                String.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now),
                Path.GetExtension(path));
            File.Copy(path, backupPath);

            bool needsSave;
            TECScopeManager scopeManager;

            DatabaseVersionManager.UpdateStatus status = DatabaseVersionManager.CheckAndUpdate(path);
            if (status == DatabaseVersionManager.UpdateStatus.Updated)
            {
                (scopeManager, needsSave) = DatabaseLoader.Load(path, true);
            }
            else if (status == DatabaseVersionManager.UpdateStatus.NotUpdated)
            {
                (scopeManager, needsSave) = DatabaseLoader.Load(path);
            }
            else if (status == DatabaseVersionManager.UpdateStatus.Incompatible)
            {
                System.Windows.MessageBox.Show("Database is incompatible with this version of the program.", "Incompatible", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
            else
            {
                throw new NotImplementedException("UpdateStatus not recognized.");
            }

            if (needsSave)
            {
                New(scopeManager);
            }

            return scopeManager as T;
        }
        public void AsyncLoad()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {

                bool success = catchOnRelease("Load failed",
                    () => { e.Result = Load(); });
                if (!success)
                {
                    e.Result = success;
                }
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    logger.Error("Error occured whille loadinng :" + path + " , Error: " + e.Error);
                    notifyLoadComplete(null);
                }
                else if (e.Result is T scopeManager)
                {
                    notifyLoadComplete(scopeManager);
                }
                else
                {
                    notifyLoadComplete(null);
                }
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void notifySaveComplete(bool success)
        {
            IsBusy = false;
            SaveComplete?.Invoke(success);
        }
        private void notifyLoadComplete(T loaded)
        {
            IsBusy = false;
            LoadComplete?.Invoke(loaded);
        }

        private bool catchOnRelease(string message, Action action)
        {
#if DEBUG
            action();
            return true;
#else
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(message);
                logger.Error("Exception: {0}", ex.Message);
                logger.Error("Internal Exception: {0}", ex.InnerException.Message);
                logger.Error("Stack Trace: {0}", ex.StackTrace);
                return false;
            }
#endif

        }

        public static void ExportDef()
        {
            foreach(TableBase table in AllTables.Tables)
            {
                foreach(TableField field in table.Fields)
                {
                    Console.WriteLine(String.Format("{0},{1}", table.NameString, field.Name));
                }
            }
        }

        public string GetPath()
        {
            return path;
        }
    }
}
