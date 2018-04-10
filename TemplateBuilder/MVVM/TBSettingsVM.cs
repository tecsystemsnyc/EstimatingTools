using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace TemplateBuilder.MVVM
{
    public class TBSettingsVM : ViewModelBase
    {
        private bool settingsChanged = false;
        private readonly FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        public string DefaultTemplatesDirectory
        {
            get { return TBSettings.TemplatesDirectory; }
            set
            {
                TBSettings.TemplatesDirectory = value;
                settingsChanged = true;
                RaisePropertyChanged("DefaultTemplatesDirectory");
            }
        }
        public bool OpenOnExport
        {
            get { return TBSettings.OpenFileOnExport; }
            set
            {
                TBSettings.OpenFileOnExport = value;
                settingsChanged = true;
            }
        }

        public ICommand ChooseTemplatesDirectoryCommand { get; }
        public ICommand ApplyCommand { get; }

        public TBSettingsVM()
        {
            ChooseTemplatesDirectoryCommand = new RelayCommand(chooseTemplatesDirectoryExecute);
            ApplyCommand = new RelayCommand(applySettingsExecute, applySettingsCanExecute);
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
            TBSettings.Save();
            settingsChanged = false;
        }
        private bool applySettingsCanExecute()
        {
            return settingsChanged;
        }
    }
}
