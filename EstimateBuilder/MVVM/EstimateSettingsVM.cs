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
        public bool OpenOnExport
        {
            get { return Properties.Settings.Default.OpenFileOnExport; }
            set { Properties.Settings.Default.OpenFileOnExport = value; }
        }

        public ICommand ApplyCommand { get; private set; }

        public EstimateSettingsVM()
        {
            ApplyCommand = new RelayCommand(applySettingsExecute);
        }

        public void applySettingsExecute()
        {
            Properties.Settings.Default.Save();
        }
    }
}
