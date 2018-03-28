using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Interfaces;

namespace EstimateBuilder.MVVM
{
    public class EstimateSettingsVM : ViewModelBase
    {
        private bool settingsChanged = false;

        public string DefaultBidDirectory
        {
            get { return Properties.Settings.Default.BidDirectory; }
            set
            {
                Properties.Settings.Default.BidDirectory = value;
                settingsChanged = true;
            }
        }
        public string DefaultTemplatesDirectory
        {
            get { return Properties.Settings.Default.TemplatesDirectory; }
            set
            {
                Properties.Settings.Default.TemplatesDirectory = value;
                settingsChanged = true;
            }
        }
        public bool OpenOnExport
        {
            get { return Properties.Settings.Default.OpenFileOnExport; }
            set
            {
                Properties.Settings.Default.OpenFileOnExport = value;
                settingsChanged = true;
            }
        }

        public ICommand ApplyCommand { get; private set; }

        public EstimateSettingsVM()
        {
            ApplyCommand = new RelayCommand(applySettingsExecute, applySettingsCanExecute);
        }

        private void applySettingsExecute()
        {
            Properties.Settings.Default.Save();
            settingsChanged = false;
        }
        private bool applySettingsCanExecute()
        {
            return settingsChanged;
        }
    }
}
