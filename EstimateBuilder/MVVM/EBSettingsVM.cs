using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using TECUserControlLibrary.Interfaces;

namespace EstimateBuilder.MVVM
{
    public class EBSettingsVM : ViewModelBase
    {
        private bool settingsChanged = false;
        private readonly FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        public string DefaultBidDirectory
        {
            get { return EBSettings.BidDirectory; }
            set
            {
                EBSettings.BidDirectory = value;
                settingsChanged = true;
                RaisePropertyChanged("DefaultBidDirectory");
            }
        }
        public string DefaultTemplatesDirectory
        {
            get { return EBSettings.TemplatesDirectory; }
            set
            {
                EBSettings.TemplatesDirectory = value;
                settingsChanged = true;
                RaisePropertyChanged("DefaultTemplatesDirectory");
            }
        }
        public bool OpenOnExport
        {
            get { return EBSettings.OpenFileOnExport; }
            set
            {
                EBSettings.OpenFileOnExport = value;
                settingsChanged = true;
            }
        }

        public ICommand ChooseBidDirectoryCommand { get; }
        public ICommand ChooseTemplatesDirectoryCommand { get; }
        public ICommand ApplyCommand { get; }

        public EBSettingsVM()
        {
            ChooseBidDirectoryCommand = new RelayCommand(chooseBidDirectoryExecute);
            ChooseTemplatesDirectoryCommand = new RelayCommand(chooseTemplatesDirectoryExecute);
            ApplyCommand = new RelayCommand(applySettingsExecute, applySettingsCanExecute);
        }

        private void chooseBidDirectoryExecute()
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DefaultBidDirectory = folderBrowserDialog.SelectedPath;
            }
        }

        private void chooseTemplatesDirectoryExecute()
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DefaultTemplatesDirectory = folderBrowserDialog.SelectedPath;
            }
        }

        private void applySettingsExecute()
        {
            EBSettings.Save();
            settingsChanged = false;
        }
        private bool applySettingsCanExecute()
        {
            return settingsChanged;
        }
    }
}
