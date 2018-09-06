using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TECUserControlLibrary.Interfaces;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class DeleteEndDeviceVM : ViewModelBase
    {
        private readonly TECTemplates templates;
        internal IUserConfirmable messageBox;
        private List<IProtocol> requiredProtocols = new List<IProtocol>();

        public IEndDevice EndDevice { get; }
        public List<IEndDevice> PotentialReplacements { get; }

        private IEndDevice _selectedReplacement;
        public IEndDevice SelectedReplacement
        {
            get { return _selectedReplacement; }
            set
            {
                _selectedReplacement = value;
                RaisePropertyChanged("SelectedReplacement");
            }
        }

        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteAndReplaceCommand { get; private set; }

        public DeleteEndDeviceVM(IEndDevice endDevice, TECTemplates templates)
        {
            messageBox = new MessageBoxService();
            this.templates = templates;
            this.EndDevice = endDevice;
            PotentialReplacements = new List<IEndDevice>();
            populatePotentialReplacements();

            DeleteCommand = new RelayCommand(deleteExecute);
            DeleteAndReplaceCommand = new RelayCommand(deleteAndReplaceExecute, deleteAndReplaceCanExecute);
        }

        private void deleteExecute()
        {
            MessageBoxResult result = messageBox.Show(
                "Deleting a device will remove it from all template Systems, Equipment and Points. Are you sure?", 
                "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                templates.RemoveCatalogItem(EndDevice, null);

            }
        }
        private void deleteAndReplaceExecute()
        {
            templates.RemoveCatalogItem(EndDevice, SelectedReplacement);
        }
        private bool deleteAndReplaceCanExecute()
        {
            return (SelectedReplacement != null);
        }

        private void populatePotentialReplacements()
        {
            foreach (TECSystem sys in templates.Templates.SystemTemplates)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope.Where(x => x.Devices.Contains(EndDevice)))
                    {
                        if (ss.Connection != null)
                        {
                            requiredProtocols.Add(ss.Connection.Protocol);
                        }
                    }
                }
            }

            List<IEndDevice> endDevices = new List<IEndDevice>(templates.Catalogs.Devices);
            endDevices.AddRange(templates.Catalogs.Valves);
            foreach (IEndDevice dev in endDevices)
            {
                if (hasRequiredProtocols(dev, requiredProtocols))
                {
                    PotentialReplacements.Add(dev);
                }
            }
            PotentialReplacements.Remove(EndDevice);
            
        }

        private bool hasRequiredProtocols(IEndDevice device, IEnumerable<IProtocol> protocols)
        {
            foreach (IProtocol protocol in protocols)
            {
                if (!device.ConnectionMethods.Contains(protocol))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
