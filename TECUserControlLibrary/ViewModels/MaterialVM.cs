using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels.CatalogVMs;

namespace TECUserControlLibrary.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MaterialVM : ViewModelBase
    {
        #region Properties
        public ReferenceDropper ReferenceDropHandler { get; }

        private TECTemplates _templates;
        public TECTemplates Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
                RaisePropertyChanged("Templates");
            }
        }

        private TECObject _selected;
        public TECObject Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
                SelectionChanged?.Invoke(value);
            }
        }
        
        #region Associated Costs
        private string _associatedCostName;
        public string AssociatedCostName
        {
            get { return _associatedCostName; }
            set
            {
                _associatedCostName = value;
                RaisePropertyChanged("AssociatedCostName");
            }
        }
        private CostType _associatedCostType;
        public CostType AssociatedCostType
        {
            get { return _associatedCostType; }
            set
            {
                _associatedCostType = value;
                RaisePropertyChanged("AssociatedCostType");
            }
        }
        private double _associatedCostCost;
        public double AssociatedCostCost
        {
            get { return _associatedCostCost; }
            set
            {
                _associatedCostCost = value;
                RaisePropertyChanged("AssociatedCostCost");
            }
        }
        private double _associatedCostLabor;
        public double AssociatedCostLabor
        {
            get { return _associatedCostLabor; }
            set
            {
                _associatedCostLabor = value;
                RaisePropertyChanged("AssociatedCostLabor");
            }
        }


        private TECCost _selectedAssociatedCost;

        public TECCost SelectedAssociatedCost
        {
            get { return _selectedAssociatedCost; }
            set
            {
                _selectedAssociatedCost = value;
                RaisePropertyChanged("SelectedAssociatedCost");
                Selected = value;
            }
        }
        #endregion
        #region Panel Types
        private string _panelTypeName;
        public string PanelTypeName
        {
            get { return _panelTypeName; }
            set
            {
                _panelTypeName = value;
                RaisePropertyChanged("PanelTypeName");
            }
        }
        private string _panelTypeDescription;
        public string PanelTypeDescription
        {
            get { return _panelTypeDescription; }
            set
            {
                _panelTypeDescription = value;
                RaisePropertyChanged("PanelTypeDescription");
            }
        }
        private double _panelTypeCost;
        public double PanelTypeCost
        {
            get { return _panelTypeCost; }
            set
            {
                _panelTypeCost = value;
                RaisePropertyChanged("PanelTypeCost");
            }
        }
        private double _panelTypeLabor;
        public double PanelTypeLabor
        {
            get { return _panelTypeLabor; }
            set
            {
                _panelTypeLabor = value;
                RaisePropertyChanged("PanelTypeLabor");
            }
        }
        private TECManufacturer _panelTypeManufacturer;
        public TECManufacturer PanelTypeManufacturer
        {
            get { return _panelTypeManufacturer; }
            set
            {
                _panelTypeManufacturer = value;
                RaisePropertyChanged("PanelTypeManufacturer");
            }
        }


        private TECPanelType _selectedPanelType;

        public TECPanelType SelectedPanelType
        {
            get { return _selectedPanelType; }
            set
            {
                _selectedPanelType = value;
                RaisePropertyChanged("SelectedPanelType");
                Selected = value;
            }
        }
        #endregion
        #region IO Modules
        private string _ioModuleName;
        public string IOModuleName
        {
            get { return _ioModuleName; }
            set
            {
                _ioModuleName = value;
                RaisePropertyChanged("IOModuleName");
            }
        }
        private string _ioModuleDescription;
        public string IOModuleDescription
        {
            get { return _ioModuleDescription; }
            set
            {
                _ioModuleDescription = value;
                RaisePropertyChanged("IOModuleDescription");
            }
        }
        private double _ioModuleCCost;
        public double IOModuleCost
        {
            get { return _ioModuleCCost; }
            set
            {
                _ioModuleCCost = value;
                RaisePropertyChanged("IOModuleCost");
            }
        }
        public IOType SelectedIO { get; set; }
        public int SelectedIOQty { get; set; }
        private TECManufacturer _ioModuleManufacturer;
        public TECManufacturer IOModuleManufacturer
        {
            get { return _ioModuleManufacturer; }
            set
            {
                _ioModuleManufacturer = value;
                RaisePropertyChanged("IOModuleManufacturer");
            }
        }
        private ObservableCollection<TECIO> _moduleIO;
        public ObservableCollection<TECIO> ModuleIO
        {
            get { return _moduleIO; }
            set
            {
                _moduleIO = value;
                RaisePropertyChanged("ModuleIO");
            }
        }
        public ICommand AddIOToModuleCommand { get; private set; }

        private TECIOModule _selectedIOModule;

        public TECIOModule SelectedIOModule
        {
            get { return _selectedIOModule; }
            set
            {
                _selectedIOModule = value;
                RaisePropertyChanged("SelectedIOModule");
                Selected = value;
            }
        }
        #endregion
        #region Manufacturer
        private TECManufacturer _manufacturerToAdd;
        public TECManufacturer ManufacturerToAdd
        {
            get { return _manufacturerToAdd; }
            set
            {
                _manufacturerToAdd = value;
                RaisePropertyChanged("ManufacturerToAdd");
            }
        }

        private TECManufacturer _selectedManufacturer;

        public TECManufacturer SelectedManufacturer
        {
            get { return _selectedManufacturer; }
            set
            {
                _selectedManufacturer = value;
                RaisePropertyChanged("SelectedManufacturer");
                Selected = value;
            }
        }
        #endregion
        #region Tag
        private TECTag _tagToAdd;
        public TECTag TagToAdd
        {
            get { return _tagToAdd; }
            set
            {
                _tagToAdd = value;
                RaisePropertyChanged("TagToAdd");
            }
        }

        private TECTag _selectedTag;

        public TECTag SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                RaisePropertyChanged("SelectedTag");
                Selected = value;
            }
        }
        #endregion
        #region Command Properties
        public ICommand AddAssociatedCostCommand { get; private set; }
        public ICommand AddPanelTypeCommand { get; private set; }
        public ICommand AddIOModuleCommand { get; private set; }
        public ICommand AddManufacturerCommand { get; private set; }
        public ICommand AddTagCommand { get; private set; }
        #endregion

        #region Delegates
        public event Action<Object> SelectionChanged;
        #endregion

        #region ViewModels
        public DevicesCatalogVM DeviceVM { get; }
        public ValvesCatalogVM ValveVM { get; }
        public ConnectionTypesCatalogVM ConnectionTypeVM { get; }
        public ConduitTypesCatalogVM ConduitTypeVM { get; }
        public ControllerTypesCatalogVM ControllerTypeVM { get; }
        public MiscCostsVM MiscVM { get; }

        private ViewModelBase _modalVM;
        public ViewModelBase ModalVM
        {
            get { return _modalVM; }
            set
            {
                if (_modalVM != value)
                {
                    _modalVM = value;
                    RaisePropertyChanged("ModalVM");
                }
            }
        }
        #endregion
        #endregion

        public MaterialVM(TECTemplates templates)
        {
            ReferenceDropHandler = new ReferenceDropper(templates);
            Templates = templates;
            setupCommands();
            setupInterfaceDefaults();

            //Setup VMs
            subscribeToVM(DeviceVM = new DevicesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ValveVM = new ValvesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ConnectionTypeVM = new ConnectionTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ConduitTypeVM = new ConduitTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ControllerTypeVM = new ControllerTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(MiscVM = new MiscCostsVM(templates));
        }
        
        #region Methods
        private void setupCommands()
        {
            AddAssociatedCostCommand = new RelayCommand(addAsociatedCostExecute, canAddAssociatedCost);
            AddPanelTypeCommand = new RelayCommand(addPanelTypeExecute, canAddPanelTypeExecute);
            AddIOModuleCommand = new RelayCommand(addIOModuleExecute, canAddIOModuleExecute);
            AddManufacturerCommand = new RelayCommand(addManufacturerExecute, canAddManufacturer);
            AddTagCommand = new RelayCommand(addTagExecute, canAddTag);
            AddIOToModuleCommand = new RelayCommand(addIOToModuleExecute, canAddIOToModule);
        }

        private void addIOToModuleExecute()
        {
            bool wasAdded = false;
            for(int x = 0; x < SelectedIOQty; x++)
            {
                foreach (TECIO io in ModuleIO)
                {
                    if (io.Type == SelectedIO)
                    {
                        io.Quantity++;
                        wasAdded = true;
                        break;
                    }
                }
                if (!wasAdded)
                {
                    ModuleIO.Add(new TECIO(SelectedIO));
                }
            }
        }
        private bool canAddIOToModule()
        {
            return true;
        }
        
        private void addAsociatedCostExecute()
        {
            var associatedCost = new TECCost(AssociatedCostType);
            associatedCost.Name = AssociatedCostName;
            associatedCost.Cost = AssociatedCostCost;
            associatedCost.Labor = AssociatedCostLabor;
            Templates.Catalogs.AssociatedCosts.Add(associatedCost);
            AssociatedCostName = "";
            AssociatedCostCost = 0;
            AssociatedCostLabor = 0;
        }
        private bool canAddAssociatedCost()
        {
            if (AssociatedCostName == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void addPanelTypeExecute()
        {
            var panelType = new TECPanelType(PanelTypeManufacturer);
            panelType.Name = PanelTypeName;
            panelType.Description = PanelTypeDescription;
            panelType.Price = PanelTypeCost;
            panelType.Labor = PanelTypeLabor;

            Templates.Catalogs.PanelTypes.Add(panelType);
            PanelTypeName = "";
            PanelTypeDescription = "";
            PanelTypeCost = 0;
            PanelTypeLabor = 0;
            PanelTypeManufacturer = null;
        }
        private bool canAddPanelTypeExecute()
        {
            if (PanelTypeName != "" && PanelTypeManufacturer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void addIOModuleExecute()
        {
            var ioModule = new TECIOModule(IOModuleManufacturer);
            ioModule.Name = IOModuleName;
            ioModule.Price = IOModuleCost;
            ioModule.Description = IOModuleDescription;
            ioModule.IO = ModuleIO;

            Templates.Catalogs.IOModules.Add(ioModule);
            ModuleIO = new ObservableCollection<TECIO>();
            IOModuleName = "";
            IOModuleDescription = "";
            IOModuleCost = 0;
            IOModuleManufacturer = null;
        }
        private bool canAddIOModuleExecute()
        {
            if (IOModuleName != "" && IOModuleManufacturer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void addManufacturerExecute()
        {
            Templates.Catalogs.Manufacturers.Add(ManufacturerToAdd);
            ManufacturerToAdd = new TECManufacturer();
        }

        private bool canAddManufacturer()
        {
            if(ManufacturerToAdd.Label != "")
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void addTagExecute()
        {
            Templates.Catalogs.Tags.Add(TagToAdd);
            TagToAdd = new TECTag();
        }

        private bool canAddTag()
        {
            if(TagToAdd.Label != "")
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void setupInterfaceDefaults()
        {
            AssociatedCostName = "";
            AssociatedCostCost = 0;
            AssociatedCostLabor = 0;

            PanelTypeName = "";
            PanelTypeDescription = "";
            PanelTypeCost = 0;
            PanelTypeLabor = 0;

            IOModuleName = "";
            IOModuleDescription = "";
            IOModuleCost = 0;
            ModuleIO = new ObservableCollection<TECIO>();

            ManufacturerToAdd = new TECManufacturer();

            TagToAdd = new TECTag();
        }

        private void subscribeToVM(TECVMBase tecVM)
        {
            tecVM.ModalVMStarted += handleModal;
            tecVM.ObjectSelected += handleSelected;

            void handleModal(ViewModelBase vm)
            {
                ModalVM = vm;
            }

            void handleSelected(TECObject obj)
            {
                Selected = obj;
            }
        }
        #endregion

        public class ReferenceDropper : IDropTarget
        {
            private TECTemplates templates;

            public ReferenceDropper(TECTemplates templates)
            {
                this.templates = templates;
            }

            public void DragOver(IDropInfo dropInfo)
            {
                UIHelpers.StandardDragOver(dropInfo);
            }

            public void Drop(IDropInfo dropInfo)
            {
                if (dropInfo.Data is TECIOModule module &&
                    dropInfo.TargetCollection is QuantityCollection<TECIOModule> collection)
                {
                    collection.Add(module);
                }
                else
                {
                    UIHelpers.StandardDrop(dropInfo, templates);
                }
            }
        }
    }

}