﻿using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;

namespace EstimateBuilder.MVVM
{
    public class EstimateManager : AppManager
    {
        private TECBid bid;
        private TECTemplates templates;

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
        override protected string defaultDirectory
        {
            get
            {
                return Properties.Settings.Default.DefaultDirectory;
            }
            set
            {
                Properties.Settings.Default.DefaultDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
        override protected string defaultFileName
        {
            get
            {
                throw new NotImplementedException("Need to construct file name from bid.");
            }
        }

        public EstimateManager() : base(new EstimateSplashVM(), new EstimateMenuVM(), new EstimateEditorVM())
        {

        }

        private void setupCommands()
        {

        }
        private void saveDeltaExecute()
        {
            if (databaseManager != null)
            {
                databaseManager.Save(deltaStack.CleansedStack());
            }
            else
            {
                string savePath = UIHelpers.GetSavePath(workingFileParameters, defaultFileName, defaultDirectory, workingFileDirectory);
                throw new NotImplementedException("Need to handle save path return.");
            }

            throw new NotImplementedException("Need a method for clearing the delta stack.");
        }
        private bool canSaveDelta()
        {
            return deltaStack.CleansedStack().Count > 0;
        }
    }
}
