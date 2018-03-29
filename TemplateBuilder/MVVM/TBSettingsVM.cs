using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TemplateBuilder.MVVM
{
    public class TBSettingsVM
    {
        private bool settingsChanged = false;

        public string DefaultTemplatesDirectory
        {
            get { return TBSettings.TemplatesDirectory; }
            set
            {
                TBSettings.TemplatesDirectory = value;
                settingsChanged = true;
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
            ApplyCommand = new RelayCommand(applySettingsExecute, applySettingsCanExecute);
        }

        private void chooseTemplatesDirectoryExecute()
        {
            throw new NotImplementedException();
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
