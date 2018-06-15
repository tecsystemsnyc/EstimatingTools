using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimateBuilder.MVVM
{
    internal static class EBSettings
    {
        #region Settings
        public static string BidDirectory
        {
            get { return Properties.Settings.Default.BidDirectory; }
            set { Properties.Settings.Default.BidDirectory = value; }
        }
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
        //Bid Files
        public static string FirstRecentBid
        {
            get { return Properties.Settings.Default.FirstRecentBid; }
            set { Properties.Settings.Default.FirstRecentBid = value; }
        }
        public static string SecondRecentBid
        {
            get { return Properties.Settings.Default.SecondRecentBid; }
            set { Properties.Settings.Default.SecondRecentBid = value; }
        }
        public static string ThirdRecentBid
        {
            get { return Properties.Settings.Default.ThirdRecentBid; }
            set { Properties.Settings.Default.ThirdRecentBid = value; }
        }
        public static string FourthRecentBid
        {
            get { return Properties.Settings.Default.FourthRecentBid; }
            set { Properties.Settings.Default.FourthRecentBid = value; }
        }
        public static string FifthRecentBid
        {
            get { return Properties.Settings.Default.FifthRecentBid; }
            set { Properties.Settings.Default.FifthRecentBid = value; }
        }

        //Template Files
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
