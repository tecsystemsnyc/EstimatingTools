﻿using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class DeleteDeviceVM : ViewModelBase
    {
        private readonly TECTemplates templates;

        public TECDevice Device { get; }
        public List<TECDevice> AllDevices { get; }

        private TECDevice _selectedReplacement;
        public TECDevice SelectedReplacement
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

        public DeleteDeviceVM(TECDevice device, TECTemplates templates)
        {
            this.templates = templates;
            Device = device;
            AllDevices = new List<TECDevice>(templates.Catalogs.Devices);

            DeleteCommand = new RelayCommand(deleteExecute);
            DeleteAndReplaceCommand = new RelayCommand(deleteAndReplaceExecute, deleteAndReplaceCanExecute);
        }

        private void deleteExecute()
        {
            foreach(TECSubScope ss in templates.SubScopeTemplates)
            {
                while (ss.Devices.Contains(Device))
                {
                    ss.Devices.Remove(Device);
                }
            }

            foreach(TECEquipment equip in templates.EquipmentTemplates)
            {
                foreach(TECSubScope ss in equip.SubScope)
                {
                    while (ss.Devices.Contains(Device))
                    {
                        ss.Devices.Remove(Device);
                    }
                }
            }

            foreach(TECSystem sys in templates.SystemTemplates)
            {
                foreach(TECEquipment equip in sys.Equipment)
                {
                    foreach(TECSubScope ss in equip.SubScope)
                    {
                        while (ss.Devices.Contains(Device))
                        {
                            ss.Devices.Remove(Device);
                        }
                    }
                }
            }

            templates.Catalogs.Devices.Remove(Device);
        }
        private void deleteAndReplaceExecute()
        {
            foreach (TECSubScope ss in templates.SubScopeTemplates)
            {
                while (ss.Devices.Contains(Device))
                {
                    ss.Devices.Remove(Device);
                    ss.Devices.Add(SelectedReplacement);
                }
            }

            foreach (TECEquipment equip in templates.EquipmentTemplates)
            {
                foreach (TECSubScope ss in equip.SubScope)
                {
                    while (ss.Devices.Contains(Device))
                    {
                        ss.Devices.Remove(Device);
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
                        while (ss.Devices.Contains(Device))
                        {
                            ss.Devices.Remove(Device);
                            ss.Devices.Add(SelectedReplacement);
                            //To Do: Figure out connection stuff
                            throw new NotImplementedException();
                        }
                    }
                }
            }

            templates.Catalogs.Devices.Remove(Device);
        }
        private bool deleteAndReplaceCanExecute()
        {
            if (SelectedReplacement != null)
            {
                bool hasNetworkConnection = false;
                foreach(TECSystem sys in templates.SystemTemplates)
                {
                    foreach(TECEquipment equip in sys.Equipment)
                    {
                        foreach(TECSubScope ss in equip.SubScope)
                        {
                            if (ss.IsNetwork && ss.Connection != null)
                            {
                                hasNetworkConnection = true;
                                break;
                            }
                        }
                        if (hasNetworkConnection) break;
                    }
                    if (hasNetworkConnection) break;
                }

                if (hasNetworkConnection)
                {
                    return connectionTypesAreSame(Device, SelectedReplacement);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }



        private bool connectionTypesAreSame(TECDevice dev1, TECDevice dev2)
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
