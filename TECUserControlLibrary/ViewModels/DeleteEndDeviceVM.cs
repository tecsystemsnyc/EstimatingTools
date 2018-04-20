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
                foreach (TECSubScope ss in templates.SubScopeTemplates)
                {
                    while (ss.Devices.Contains(EndDevice))
                    {
                        ss.Devices.Remove(EndDevice);
                    }
                }

                foreach (TECEquipment equip in templates.EquipmentTemplates)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        while (ss.Devices.Contains(EndDevice))
                        {
                            ss.Devices.Remove(EndDevice);
                        }
                    }
                }

                foreach (TECSystem sys in templates.SystemTemplates)
                {
                    foreach (TECEquipment equip in sys.Equipment)
                    {
                        foreach (TECSubScope ss in equip.SubScope)
                        {
                            while (ss.Devices.Contains(EndDevice))
                            {
                                ss.Devices.Remove(EndDevice);
                            }
                        }
                    }
                }

                if (EndDevice is TECDevice dev)
                {
                    templates.Catalogs.Devices.Remove(dev);
                }
                else if (EndDevice is TECValve valve)
                {
                    templates.Catalogs.Valves.Remove(valve);
                }
                else throw new InvalidCastException("End device is not TECDevice or TECValve");
            }
        }
        private void deleteAndReplaceExecute()
        {
            foreach (TECSubScope ss in templates.SubScopeTemplates)
            {
                while (ss.Devices.Contains(EndDevice))
                {
                    ss.Devices.Remove(EndDevice);
                    ss.Devices.Add(SelectedReplacement);
                }
            }

            foreach (TECEquipment equip in templates.EquipmentTemplates)
            {
                foreach (TECSubScope ss in equip.SubScope)
                {
                    while (ss.Devices.Contains(EndDevice))
                    {
                        ss.Devices.Remove(EndDevice);
                        ss.Devices.Add(SelectedReplacement);
                    }
                }
            }

            foreach (TECSystem sys in templates.SystemTemplates)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        while (ss.Devices.Contains(EndDevice))
                        {
                            ss.Devices.Remove(EndDevice);
                            ss.Devices.Add(SelectedReplacement);
                        }
                    }
                }
            }

            if (EndDevice is TECDevice dev)
            {
                templates.Catalogs.Devices.Remove(dev);
            }
            else if (EndDevice is TECValve valve)
            {
                templates.Catalogs.Valves.Remove(valve);
            }
            else throw new InvalidCastException("End device is not TECDevice or TECValve");
        }
        private bool deleteAndReplaceCanExecute()
        {
            return (SelectedReplacement != null);
        }

        private void populatePotentialReplacements()
        {
            bool hasNetworkConnection = false;
            foreach (TECSystem sys in templates.SystemTemplates)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (((IConnectable)ss).IsNetwork && ss.Connection != null)
                        {
                            hasNetworkConnection = true;
                            break;
                        }
                    }
                    if (hasNetworkConnection) break;
                }
                if (hasNetworkConnection) break;
            }

            List<IEndDevice> endDevices = new List<IEndDevice>(templates.Catalogs.Devices);
            endDevices.AddRange(templates.Catalogs.Valves);
            foreach (IEndDevice dev in endDevices)
            {
                if (hasNetworkConnection)
                {
                    if (connectionTypesAreSame(EndDevice, dev))
                    {
                        PotentialReplacements.Add(dev);
                    }
                }
                else
                {
                    PotentialReplacements.Add(dev);
                }
            }
            PotentialReplacements.Remove(EndDevice);
            
        }

        private bool connectionTypesAreSame(IEndDevice dev1, IEndDevice dev2)
        {
            bool oneContainsTwo = true;
            foreach (TECConnectionType type in dev2.ConnectionTypes)
            {
                if (!dev1.ConnectionTypes.Contains(type))
                {
                    oneContainsTwo = false;
                    break;
                }
            }
            bool twoContainsOne = true;
            foreach (TECConnectionType type in dev1.ConnectionTypes)
            {
                if (!dev2.ConnectionTypes.Contains(type))
                {
                    twoContainsOne = false;
                    break;
                }
            }

            return (oneContainsTwo && twoContainsOne);
        }
    }
}
