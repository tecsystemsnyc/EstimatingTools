using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.Utilities.DropTargets;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class ControllerTypesCatalogVM : CatalogVMBase
    {
        private string _controllerTypeName = "";
        private string _controllerTypeDescription = "";
        private double _controllerTypeCost = 0;
        private double _controllerTypeLabor = 0;
        private TECManufacturer _controllerTypeManufacturer;

        private IOType _selectedIO = 0;
        private int _selectedIOQty = 0;

        private TECControllerType _selectedControllerType;

        public string ControllerTypeName
        {
            get { return _controllerTypeName; }
            set
            {
                if (_controllerTypeName != value)
                {
                    _controllerTypeName = value;
                    RaisePropertyChanged("ControllerTypeName");
                }
            }
        }
        public string ControllerTypeDescription
        {
            get { return _controllerTypeDescription; }
            set
            {
                if (_controllerTypeDescription != value)
                {
                    _controllerTypeDescription = value;
                    RaisePropertyChanged("ControllerTypeDescription");
                }
            }
        }
        public double ControllerTypeCost
        {
            get { return _controllerTypeCost; }
            set
            {
                if (_controllerTypeCost != value)
                {
                    _controllerTypeCost = value;
                    RaisePropertyChanged("ControllerTypeCost");
                }
            }
        }
        public double ControllerTypeLabor
        {
            get { return _controllerTypeLabor; }
            set
            {
                if (_controllerTypeLabor != value)
                {
                    _controllerTypeLabor = value;
                    RaisePropertyChanged("ControllerTypeLabor");
                }
            }
        }
        public TECManufacturer ControllerTypeManufacturer
        {
            get { return _controllerTypeManufacturer; }
            set
            {
                if (_controllerTypeManufacturer != value)
                {
                    _controllerTypeManufacturer = value;
                    RaisePropertyChanged("ControllerTypeManufacturer");
                }
            }
        }

        public IOType SelectedIO
        {
            get { return _selectedIO; }
            set
            {
                if (_selectedIO != value)
                {
                    _selectedIO = value;
                    RaisePropertyChanged("SelectedIO");
                }
            }
        }
        public int SelectedIOQty
        {
            get { return _selectedIOQty; }
            set
            {
                if (_selectedIOQty != value)
                {
                    _selectedIOQty = value;
                    RaisePropertyChanged("SelectedIOQty");
                }
            }
        }

        public TECControllerType SelectedControllerType
        {
            get { return _selectedControllerType; }
            set
            {
                if (_selectedControllerType != value)
                {
                    _selectedControllerType = value;
                    RaisePropertyChanged("SelectedControllerType");
                    RaiseSelectedChanged(SelectedControllerType);
                }
            }
        }

        public ObservableCollection<TECIO> ControllerTypeIO { get; }
        public QuantityCollection<TECIOModule> ControllerTypeModules { get; }

        public ICommand AddIOCommand { get; }
        public ICommand AddControllerTypeCommand { get; private set; }

        public IDropTarget ProtocolToIODropTarget { get; }

        public ControllerTypesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.ControllerTypeIO = new ObservableCollection<TECIO>();
            this.ControllerTypeModules = new QuantityCollection<TECIOModule>();

            this.AddIOCommand = new RelayCommand(addIOToControllerTypeExecute, canAddIOToControllerType);
            this.AddControllerTypeCommand = new RelayCommand(addControllerTypeExecute, canAddControllerType);

            this.ProtocolToIODropTarget = new ProtocolToIODropTarget();
        }

        private void addIOToControllerTypeExecute()
        {
            bool wasAdded = false;
            for (int x = 0; x < this.SelectedIOQty; x++)
            {
                foreach (TECIO io in this.ControllerTypeIO)
                {
                    if (io.Type == this.SelectedIO)
                    {
                        io.Quantity++;
                        wasAdded = true;
                        break;
                    }
                }
                if (!wasAdded)
                {
                    this.ControllerTypeIO.Add(new TECIO(SelectedIO));
                }
            }
        }
        private bool canAddIOToControllerType()
        {
            return (SelectedIOQty > 0);
        }
        
        private void addControllerTypeExecute()
        {
            TECControllerType toAdd = new TECControllerType(ControllerTypeManufacturer);
            toAdd.Name = this.ControllerTypeName;
            toAdd.Description = this.ControllerTypeDescription;
            toAdd.Price = this.ControllerTypeCost;
            toAdd.Labor = this.ControllerTypeLabor;
            foreach (TECIO io in this.ControllerTypeIO)
            {
                toAdd.IO.Add(io);
            }
            foreach (QuantityItem<TECIOModule> quantItem in this.ControllerTypeModules)
            {
                for (int i = 0; i < quantItem.Quantity; i++)
                {
                    toAdd.IOModules.Add(quantItem.Item);
                }
            }

            Templates.Catalogs.Add(toAdd);
            
            this.ControllerTypeName = "";
            this.ControllerTypeDescription = "";
            this.ControllerTypeCost = 0;
            this.ControllerTypeLabor = 0;
            this.ControllerTypeManufacturer = null;
            this.ControllerTypeIO.ObservablyClear();
            this.ControllerTypeModules.ObservablyClear();
        }
        private bool canAddControllerType()
        {
            return (ControllerTypeName != "" && ControllerTypeManufacturer != null);
        }
    }
}
