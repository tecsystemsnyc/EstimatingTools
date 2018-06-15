using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.BaseVMs;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class DevicesCatalogVM : CatalogVMBase
    {
        private string _deviceName = "";
        private string _deviceDescription = "";
        private double _deviceListPrice = 0;
        private double _deviceLabor = 0;
        private TECManufacturer _deviceManufacturer;

        private TECDevice _selectedDevice;

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    RaisePropertyChanged("DeviceName");
                }
            }
        }
        public string DeviceDescription
        {
            get { return _deviceDescription; }
            set
            {
                if (_deviceDescription != value)
                {
                    _deviceDescription = value;
                    RaisePropertyChanged("DeviceDescription");
                }
            }
        }
        public double DeviceListPrice
        {
            get { return _deviceListPrice; }
            set
            {
                if (_deviceListPrice != value)
                {
                    _deviceListPrice = value;
                    RaisePropertyChanged("DeviceListPrice");
                }
            }
        }
        public double DeviceLabor
        {
            get { return _deviceLabor; }
            set
            {
                if (_deviceLabor != value)
                {
                    _deviceLabor = value;
                    RaisePropertyChanged("DeviceLabor");
                }
            }
        }
        public TECManufacturer DeviceManufacturer
        {
            get { return _deviceManufacturer; }
            set
            {
                if (_deviceManufacturer != value)
                {
                    _deviceManufacturer = value;
                    RaisePropertyChanged("DeviceManufacturer");
                }
            }
        }

        public TECDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    RaisePropertyChanged("SelectedDevice");
                    RaiseSelectedChanged(SelectedDevice);
                }
            }
        }

        public ObservableCollection<TECConnectionType> DeviceConnectionTypes { get; }
        public ObservableCollection<TECProtocol> DeviceProtocols { get; }
        
        public ICommand AddDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        
        public DevicesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.DeviceConnectionTypes = new ObservableCollection<TECConnectionType>();
            this.DeviceProtocols = new ObservableCollection<TECProtocol>();

            this.AddDeviceCommand = new RelayCommand(addDeviceExecute, canAddDevice);
            this.DeleteDeviceCommand = new RelayCommand(deleteDeviceExecute, canDeleteDevice);
        }

        private void addDeviceExecute()
        {
            TECDevice toAdd = new TECDevice(DeviceConnectionTypes, DeviceProtocols, DeviceManufacturer);
            toAdd.Name = this.DeviceName;
            toAdd.Description = this.DeviceDescription;
            toAdd.Price = this.DeviceListPrice;
            toAdd.Labor = this.DeviceLabor;

            this.Templates.Catalogs.Devices.Add(toAdd);

            this.DeviceName = "";
            this.DeviceDescription = "";
            this.DeviceListPrice = 0;
            this.DeviceLabor = 0;
            this.DeviceConnectionTypes.ObservablyClear();
            this.DeviceProtocols.ObservablyClear();
            this.DeviceManufacturer = null;
        }
        private bool canAddDevice()
        {
            return (DeviceManufacturer != null && 
                DeviceConnectionTypes.Count > 0);
        }

        private void deleteDeviceExecute()
        {
            this.StartModal(new DeleteEndDeviceVM(SelectedDevice, Templates));
        }
        private bool canDeleteDevice()
        {
            return SelectedDevice != null;
        }
    }
}
