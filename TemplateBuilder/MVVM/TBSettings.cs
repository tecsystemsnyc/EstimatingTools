using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateBuilder.MVVM
{
    internal static class TBSettings
    {
        #region Settings
        public static string TemplatesDirectory
        {
            get { return Properties.Settings.Default.TemplatesDirectory; }
            set { Properties.Settings.Default.TemplatesDirectory = value; }
        }
        public static bool OpenFileOnExport
        {
            get { return Properties.Settings.Default.OpenFileOnExport; }
            set { Properties.Settings.Default.OpenFileOnExport = value; }
        }
        #endregion

        #region Recent Files
        public static string FirstRecentTemplates
        {
            get { return Properties.Settings.Default.FirstRecentTemplates; }
            set { Properties.Settings.Default.FirstRecentTemplates = value; }
        }
        public static string SecondRecentTemplates
        {
            get { return Properties.Settings.Default.SecondRecentTemplates; }
            set { Properties.Settings.Default.SecondRecentTemplates = value; }
        }
        public static string ThirdRecentTemplates
        {
            get { return Properties.Settings.Default.ThirdRecentTemplates; }
            set { Properties.Settings.Default.ThirdRecentTemplates = value; }
        }
        public static string FourthRecentTemplates
        {
            get { return Properties.Settings.Default.FourthRecentTemplates; }
            set { Properties.Settings.Default.FourthRecentTemplates = value; }
        }
        public static string FifthRecentTemplates
        {
            get { return Properties.Settings.Default.FifthRecentTemplates; }
            set { Properties.Settings.Default.FifthRecentTemplates = value; }
        }
        #endregion

        #region Program Utility
        public static string StartUpFilePath
        {
            get { return Properties.Settings.Default.StartUpFilePath; }
            set { Properties.Settings.Default.StartUpFilePath = value; }
        }
        #endregion

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
