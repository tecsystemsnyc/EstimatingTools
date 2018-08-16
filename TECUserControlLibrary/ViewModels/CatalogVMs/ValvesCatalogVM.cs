using EstimatingLibrary;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.BaseVMs;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class ValvesCatalogVM : CatalogVMBase
    {
        private string _valveName = "";
        private string _valveDescription = "";
        private double _valveListPrice = 0;
        private double _valveLabor = 0;
        private double _valveCv = 0;
        private double _valveSize = 0;
        private string _valveStyle = "";
        private TECManufacturer _valveManufacturer;
        private TECDevice _valveActuator;
        
        private TECValve _selectedValve;

        public string ValveName
        {
            get { return _valveName; }
            set
            {
                if (_valveName != value)
                {
                    _valveName = value;
                    RaisePropertyChanged("ValveName");
                }
            }
        }
        public string ValveDescription
        {
            get { return _valveDescription; }
            set
            {
                if (_valveDescription != value)
                {
                    _valveDescription = value;
                    RaisePropertyChanged("ValveDescription");
                }
            }
        }
        public double ValveListPrice
        {
            get { return _valveListPrice; }
            set
            {
                if (_valveListPrice != value)
                {
                    _valveListPrice = value;
                    RaisePropertyChanged("ValveListPrice");
                }
            }
        }
        public double ValveLabor
        {
            get { return _valveLabor; }
            set
            {
                if (_valveLabor != value)
                {
                    _valveLabor = value;
                    RaisePropertyChanged("ValveLabor");
                }
            }
        }
        public double ValveCv
        {
            get { return _valveCv; }
            set
            {
                if (_valveCv != value)
                {
                    _valveCv = value;
                    RaisePropertyChanged("ValveCv");
                }
            }
        }
        public double ValveSize
        {
            get { return _valveSize; }
            set
            {
                if (_valveSize != value)
                {
                    _valveSize = value;
                    RaisePropertyChanged("ValveSize");
                }
            }
        }
        public string ValveStyle
        {
            get { return _valveStyle; }
            set
            {
                if (_valveStyle != value)
                {
                    _valveStyle = value;
                    RaisePropertyChanged("ValveStyle");
                }
            }
        }
        public TECManufacturer ValveManufacturer
        {
            get { return _valveManufacturer; }
            set
            {
                if (_valveManufacturer != value)
                {
                    _valveManufacturer = value;
                    RaisePropertyChanged("ValveManufacturer");
                }
            }
        }
        public TECDevice ValveActuator
        {
            get { return _valveActuator; }
            set
            {
                if (_valveActuator != value)
                {
                    _valveActuator = value;
                    RaisePropertyChanged("ValveActuator");
                }
            }
        }

        public TECValve SelectedValve
        {
            get { return _selectedValve; }
            set
            {
                if (_selectedValve != value)
                {
                    _selectedValve = value;
                    RaisePropertyChanged("SelectedValve");
                    RaiseSelectedChanged(SelectedValve);
                }
            }
        }

        public ICommand AddValveCommand { get; private set; }
        public ICommand DeleteValveCommand { get; private set; }
        public ICommand ReplaceActuatorCommand { get; private set; }

        public ValvesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddValveCommand = new RelayCommand(addValveExecute, canAddValve);
            this.DeleteValveCommand = new RelayCommand(deleteValveExecute, canDeleteValve);
            this.ReplaceActuatorCommand = new RelayCommand(replaceActuatorExecute, canReplaceActuator);
        }

        private void addValveExecute()
        {
            TECValve toAdd = new TECValve(ValveManufacturer, ValveActuator);
            toAdd.Name = ValveName;
            toAdd.Description = ValveDescription;
            toAdd.Price = ValveListPrice;
            toAdd.Labor = ValveLabor;
            toAdd.Style = ValveStyle;
            toAdd.Cv = ValveCv;
            toAdd.Size = ValveSize;
            Templates.Catalogs.Add(toAdd);

            ValveName = "";
            ValveDescription = "";
            ValveListPrice = 0;
            ValveLabor = 0;
            ValveCv = 0.0;
            ValveStyle = "";
            ValveSize = 0.0;
            ValveActuator = null;
            ValveManufacturer = null;
        }
        private bool canAddValve()
        {
            if (ValveActuator != null && ValveManufacturer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void deleteValveExecute()
        {
            this.StartModal(new DeleteEndDeviceVM(SelectedValve, Templates));
        }
        private bool canDeleteValve()
        {
            return SelectedValve != null;
        }

        private void replaceActuatorExecute()
        {
            this.StartModal(new ReplaceActuatorVM(SelectedValve, Templates.Catalogs.Devices));
        }
        private bool canReplaceActuator()
        {
            return SelectedValve != null;
        }

    }
}
