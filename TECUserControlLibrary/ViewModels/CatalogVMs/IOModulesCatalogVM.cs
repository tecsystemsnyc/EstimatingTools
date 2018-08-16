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
using TECUserControlLibrary.Utilities.DropTargets;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class IOModulesCatalogVM : CatalogVMBase
    {
        private string _ioModuleName = "";
        private string _ioModuleDescription = "";
        private double _ioModuleCost = 0;
        private TECManufacturer _ioModuleManufacturer;

        private IOType _selectedIO = 0;
        private int _selectedIOQty = 0;

        private TECIOModule _selectedIOModule;

        public string IOModuleName
        {
            get { return _ioModuleName; }
            set
            {
                if (_ioModuleName != value)
                {
                    _ioModuleName = value;
                    RaisePropertyChanged("IOModuleName");
                }
            }
        }
        public string IOModuleDescription
        {
            get { return _ioModuleDescription; }
            set
            {
                if (_ioModuleDescription != value)
                {
                    _ioModuleDescription = value;
                    RaisePropertyChanged("IOModuleDescription");
                }
            }
        }
        public double IOModuleCost
        {
            get { return _ioModuleCost; }
            set
            {
                if (_ioModuleCost != value)
                {
                    _ioModuleCost = value;
                    RaisePropertyChanged("IOModuleCost");
                }
            }
        }
        public TECManufacturer IOModuleManufacturer
        {
            get { return _ioModuleManufacturer; }
            set
            {
                if (_ioModuleManufacturer != value)
                {
                    _ioModuleManufacturer = value;
                    RaisePropertyChanged("IOModuleManufacturer");
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

        public TECIOModule SelectedIOModule
        {
            get { return _selectedIOModule; }
            set
            {
                if (_selectedIOModule != value)
                {
                    _selectedIOModule = value;
                    RaisePropertyChanged("SelectedIOModule");
                    RaiseSelectedChanged(SelectedIOModule);
                }
            }
        }
        
        public ObservableCollection<TECIO> ModuleIO { get; }

        public ICommand AddIOToModuleCommand { get; }
        public ICommand AddIOModuleCommand { get; }

        public IDropTarget ProtocolToIODropTarget { get; }


        public IOModulesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.ModuleIO = new ObservableCollection<TECIO>();

            this.AddIOToModuleCommand = new RelayCommand(addIOToModuleExecute, canAddIOToModule);
            this.AddIOModuleCommand = new RelayCommand(addIOModuleExecute, canAddIOModuleExecute);

            this.ProtocolToIODropTarget = new ProtocolToIODropTarget();

        }

        private void addIOToModuleExecute()
        {
            bool wasAdded = false;
            foreach (TECIO io in this.ModuleIO)
            {
                if (io.Type == this.SelectedIO)
                {
                    io.Quantity += SelectedIOQty;
                    wasAdded = true;
                    break;
                }
            }
            if (!wasAdded)
            {
                TECIO newIO = new TECIO(SelectedIO);
                newIO.Quantity = SelectedIOQty;
                ModuleIO.Add(newIO);
            }
        }
        private bool canAddIOToModule()
        {
            return (SelectedIOQty > 0);
        }

        private void addIOModuleExecute()
        {
            var ioModule = new TECIOModule(this.IOModuleManufacturer);
            ioModule.Name = this.IOModuleName;
            ioModule.Price = this.IOModuleCost;
            ioModule.Description = this.IOModuleDescription;
            ioModule.IO.AddRange(this.ModuleIO);

            this.Templates.Catalogs.Add(ioModule);
            
            this.IOModuleName = "";
            this.IOModuleDescription = "";
            this.IOModuleCost = 0;
            this.IOModuleManufacturer = null;
            this.ModuleIO.ObservablyClear();
        }
        private bool canAddIOModuleExecute()
        {
            return (IOModuleName != "" && IOModuleManufacturer != null);
        }
    }
}
