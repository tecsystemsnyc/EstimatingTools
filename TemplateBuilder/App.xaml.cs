using EstimatingUtilitiesLibrary;
using GalaSoft.MvvmLight.Threading;
using NLog;
using System;
using System.IO;
using System.Windows;
using TECUserControlLibrary.Windows;
using TemplateBuilder.MVVM;

namespace TemplateBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        public App() : base()
        {
#if !DEBUG
            this.Dispatcher.UnhandledException += logUnhandledException;
#endif
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Debug("Template Builder starting up.");
            if (AppDomain.CurrentDomain.SetupInformation
                .ActivationArguments?.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation
                .ActivationArguments.ActivationData.Length > 0)
            {
                TBSettings.StartUpFilePath = "";
                try
                {
                    string fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

                    // It comes in as a URI; this helps to convert it to a path.
                    Uri uri = new Uri(fname);
                    string startUpFilePath = uri.LocalPath;
                    logger.Debug("StartUp file path: {0}", startUpFilePath);
                    TBSettings.StartUpFilePath = startUpFilePath;
                }
                catch (Exception)
                {
                    logger.Error("Couldn't process startup arguments as a path.");
                }
                TBSettings.Save();
            }
            else
            {
                TBSettings.StartUpFilePath = "";
                TBSettings.Save();

                logger.Debug("No startup arguments passed.");
            }
            base.OnStartup(e);
        }

        private void logUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Fatal("Unhandled exception: {0}", e.Exception.Message);
            logger.Fatal("Inner exception: {0}", e.Exception.InnerException.Message);
            logger.Fatal("Stack trace: {0}", e.Exception.StackTrace);

            string reportPrompt = "A crash has occured. Please describe to the best of your ability the actions leading up to the crash.";

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TemplateBuilder\\logs");
            string logPath = UtilitiesMethods.GetMostRecentFilePathFromDirectoryPath(logDirectory);

            BugReportWindow reportWindow = new BugReportWindow("Template Builder Crash", reportPrompt, logPath);
            reportWindow.ShowDialog();

            //MessageBox.Show("Fatal error occured, view logs for more information.", "Fatal Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            System.Environment.Exit(0);
        }
    }
}
