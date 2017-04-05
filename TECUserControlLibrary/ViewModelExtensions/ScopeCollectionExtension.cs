﻿using EstimatingLibrary;
using EstimatingUtilitiesLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModelExtensions
{
    public class ScopeCollectionExtension : ViewModelBase, IDropTarget
    {
        #region Properties
        private TECTemplates _templates;
        public TECTemplates Templates
        {
            get { return _templates; }
            set
            {
                unsubscribeTemplatesCollections();
                _templates = value;
                subscribeTemplatesCollections();
                RaisePropertyChanged("Templates");
            }
        }
        
        public ScopeCollectionIndex TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                RaisePropertyChanged("TabIndex");
            }
        }
        private ScopeCollectionIndex _tabIndex;

        public string TagName
        {
            get { return _tagName; }
            set
            {
                _tagName = value;
                RaisePropertyChanged("TagName");
            }
        }
        private string _tagName;

        private string _manufacturerName;
        public string ManufacturerName
        {
            get { return _manufacturerName; }
            set
            {
                _manufacturerName = value;
                RaisePropertyChanged("ManufacturerName");
            }
        }

        private double _manufacturerMultiplier;
        public double ManufacturerMultiplier
        {
            get { return _manufacturerMultiplier; }
            set
            {
                _manufacturerMultiplier = value;
                RaisePropertyChanged("ManufacturerMultiplier");
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
            }
        }

        private TECAssociatedCost _selectedAssociatedCost;
        public TECAssociatedCost SelectedAssociatedCost
        {
            get { return _selectedAssociatedCost; }
            set
            {
                _selectedAssociatedCost = value;
                RaisePropertyChanged("SelectedAssociatedCost");
            }
        }

        private ObservableCollection<TECAssociatedCost> _associatedCostSelections;
        public ObservableCollection<TECAssociatedCost> AssociatedCostSelections
        {
            get { return _associatedCostSelections; }
            set
            {
                _associatedCostSelections = value;
                RaisePropertyChanged("AssociatedCostSelections");
            }
        }

        private ObservableCollection<TECTag> _tagSelections;
        public ObservableCollection<TECTag> TagSelections
        {
            get { return _tagSelections; }
            set
            {
                _tagSelections = value;
                RaisePropertyChanged("TagSelections");
            }
        }
        #region Delegates
        public Action<IDropInfo> DragHandler;
        public Action<IDropInfo> DropHandler;
        #endregion

        #region Commands
        public ICommand SearchCollectionCommand { get; private set; }
        public ICommand EndSearchCommand { get; private set; }
        public ICommand AddTagCommand { get; private set; }
        public ICommand AddDeviceCommand { get; private set; }
        public ICommand AddManufacturerCommand { get; private set; }
        public ICommand AddControllerCommand { get; private set; }
        public ICommand AddIOToControllerCommand { get; private set; }
        public ICommand AddTagToDeviceCommand { get; private set; }
        public ICommand AddTagToControllerCommand { get; private set; }
        public ICommand AddTagToPanelCommand { get; private set; }
        public ICommand AddAssociatedCostToPanelCommand { get; private set; }
        public ICommand AddPanelCommand { get; private set; }
        #endregion

        #region Visibility Properties
        public Visibility SystemsVisibility
        {
            get { return _SystemsVisibility; }
            set
            {
                _SystemsVisibility = value;
                RaisePropertyChanged("SystemsVisibility");
            }
        }
        private Visibility _SystemsVisibility;

        public Visibility EquipmentVisibility
        {
            get { return _EquipmentVisibility; }
            set
            {
                _EquipmentVisibility = value;
                RaisePropertyChanged("EquipmentVisibility");
            }
        }
        private Visibility _EquipmentVisibility;

        public Visibility SubScopeVisibility
        {
            get { return _SubScopeVisibility; }
            set
            {
                _SubScopeVisibility = value;
                RaisePropertyChanged("SubScopeVisibility");
            }
        }
        private Visibility _SubScopeVisibility;

        public Visibility DevicesVisibility
        {
            get { return _DevicesVisibility; }
            set
            {
                _DevicesVisibility = value;
                RaisePropertyChanged("DevicesVisibility");
            }
        }
        private Visibility _DevicesVisibility;

        public Visibility DevicesEditVisibility
        {
            get { return _DevicesEditVisibility; }
            set
            {
                _DevicesEditVisibility = value;
                RaisePropertyChanged("DevicesEditVisibility");
            }
        }
        private Visibility _DevicesEditVisibility;

        public Visibility ManufacturerVisibility
        {
            get { return _ManufacturerVisibility; }
            set
            {
                _ManufacturerVisibility = value;
                RaisePropertyChanged("ManufacturerVisibility");
            }
        }
        private Visibility _ManufacturerVisibility;

        public Visibility TagsVisibility
        {
            get { return _tagsVisibility; }
            set
            {
                _tagsVisibility = value;
                RaisePropertyChanged("TagsVisibility");
            }
        }
        private Visibility _tagsVisibility;

        public Visibility ControllerEditVisibility
        {
            get { return _controllerEditVisibility; }
            set
            {
                _controllerEditVisibility = value;
                RaisePropertyChanged("ControllerEditVisibility");
            }
        }
        private Visibility _controllerEditVisibility;

        public Visibility ControllerVisibility
        {
            get { return _controllerVisibility; }
            set
            {
                _controllerVisibility = value;
                RaisePropertyChanged("ControllerVisibility");
            }
        }
        private Visibility _controllerVisibility;

        public Visibility AssociatedCostsVisibility
        {
            get { return _associatedCostsVisibility; }
            set
            {
                _associatedCostsVisibility = value;
                RaisePropertyChanged("AssociatedCostsVisibility");
            }
        }
        private Visibility _associatedCostsVisibility;

        public Visibility ControlledScopeVisibility
        {
            get { return _controlledScopeVisibility; }
            set
            {
                _controlledScopeVisibility = value;
                RaisePropertyChanged("ControlledScopeVisibility");
            }
        }
        private Visibility _controlledScopeVisibility;

        public Visibility PanelsVisibility
        {
            get { return _panelsVisibility; }
            set
            {
                _panelsVisibility = value;
                RaisePropertyChanged("PanelsVisibility");
            }
        }
        private Visibility _panelsVisibility;

        public Visibility AddPanelVisibility
        {
            get { return _addPanelVisibility; }
            set
            {
                _addPanelVisibility = value;
                RaisePropertyChanged("AddPanelVisibility");
            }
        }
        private Visibility _addPanelVisibility;
        #endregion //Visibility Properties

        #region Device Interface Properties
        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                RaisePropertyChanged("DeviceName");
            }
        }
        private string _deviceName;

        public string DeviceDescription
        {
            get { return _deviceDescription; }
            set
            {
                _deviceDescription = value;
                RaisePropertyChanged("DeviceDescription");
            }
        }
        private string _deviceDescription;
        
        public double DeviceCost
        {
            get { return _deviceCost; }
            set
            {
                _deviceCost = value;
                RaisePropertyChanged("DeviceCost");
            }
        }
        private double _deviceCost;

        public TECConnectionType DeviceConnectionType
        {
            get { return _deviceConnectionType; }
            set
            {
                _deviceConnectionType = value;
                RaisePropertyChanged("DeviceConnectionType");
            }
        }
        private TECConnectionType _deviceConnectionType;

        public string DeviceButtonContent
        {
            get { return _deviceButtonContent; }
            set
            {
                _deviceButtonContent = value;
                RaisePropertyChanged("DeviceButtonContent");
            }
        }
        private string _deviceButtonContent;

        public TECManufacturer DeviceManufacturer
        {
            get { return _deviceManufacturer; }
            set
            {
                _deviceManufacturer = value;
                RaisePropertyChanged("DeviceManufacturer");
            }
        }
        private TECManufacturer _deviceManufacturer;

        public ObservableCollection<TECTag> DeviceTags
        {
            get { return _deviceTags; }
            set
            {
                _deviceTags = value;
                RaisePropertyChanged("DeviceTags");
            }
        }
        private ObservableCollection<TECTag> _deviceTags;
        #endregion //Device Interface Properties

        #region Controller Interface Properties
        public string ControllerName
        {
            get { return _controllerName; }
            set
            {
                _controllerName = value;
                RaisePropertyChanged("ControllerName");
            }
        }
        private string _controllerName;

        public string ControllerDescription
        {
            get { return _controllerDescription; }
            set
            {
                _controllerDescription = value;
                RaisePropertyChanged("ControllerDescription");
            }
        }
        private string _controllerDescription;

        public double ControllerCost
        {
            get { return _controllerCost; }
            set
            {
                _controllerCost = value;
                RaisePropertyChanged("ControllerCost");
            }
        }
        private double _controllerCost;

        public IOType ControllerIOType
        {
            get { return _controllerIOType; }
            set
            {
                _controllerIOType = value;
                RaisePropertyChanged("ControllerIOType");
            }
        }
        private IOType _controllerIOType;

        private int _controllerIOQTY;
        public int ControllerIOQTY
        {
            get { return _controllerIOQTY; }
            set
            {
                _controllerIOQTY = value;
                RaisePropertyChanged("ControllerIOQTY");
            }
        }
        
        public ObservableCollection<TECIO> ControllerIO
        {
            get { return _controllerIO; }
            set
            {
                _controllerIO = value;
                RaisePropertyChanged("ControllerIO");
            }
        }
        private ObservableCollection<TECIO> _controllerIO;

        public ObservableCollection<TECTag> ControllerTags
        {
            get { return _controllerTags; }
            set {
                _controllerTags = value;
                RaisePropertyChanged("ControllerTags");
            }
        }
        private ObservableCollection<TECTag> _controllerTags;

        public TECManufacturer ControllerManufacturer
        {
            get { return _controllerManufacturer; }
            set
            {
                _controllerManufacturer = value;
                RaisePropertyChanged("ControllerManufacturer");
            }
        }
        private TECManufacturer _controllerManufacturer;

        #endregion //Device Interface Properties

        #region Panel Interface Properties

        private TECPanelType _selectedPanelType;
        public TECPanelType SelectedPanelType
        {
            get { return _selectedPanelType; }
            set
            {
                _selectedPanelType = value;
                RaisePropertyChanged("SelectedPanelType");
            }
        }
        private string _panelName;
        public string PanelName
        {
            get { return _panelName; }
            set
            {
                _panelName = value;
                RaisePropertyChanged("PanelName");
            }
        }
        private string _panelDescription;
        public string PanelDescription
        {
            get { return _panelDescription; }
            set
            {
                _panelDescription = value;
                RaisePropertyChanged("PanelDescription");
            }
        }
        private ObservableCollection<TECAssociatedCost> _panelAssociatedCosts;
        public ObservableCollection<TECAssociatedCost> PanelAssociatedCosts
        {
            get { return _panelAssociatedCosts; }
            set
            {
                _panelAssociatedCosts = value;
                RaisePropertyChanged("PanelAssociatedCosts");
            }
        }
        private ObservableCollection<TECTag> _panelTags;
        public ObservableCollection<TECTag> PanelTags
        {
            get { return _panelTags; }
            set
            {
                _panelTags = value;
                RaisePropertyChanged("PanelTags");
            }
        }
        private ObservableCollection<TECPanelType> _panelTypeSelections;
        public ObservableCollection<TECPanelType> PanelTypeSelections
        {
            get { return _panelTypeSelections; }
            set
            {
                _panelTypeSelections = value;
                RaisePropertyChanged("PanelTypeSelections");
            }
        }

        #endregion

        #region Scope Collections
        private ObservableCollection<TECSystem> _systemItemsCollection;
        public ObservableCollection<TECSystem> SystemItemsCollection
        {
            get { return _systemItemsCollection; }
            set
            {
                _systemItemsCollection = value;
                RaisePropertyChanged("SystemItemsCollection");
            }

        }

        private ObservableCollection<TECEquipment> _equipmentItemsCollection;
        public ObservableCollection<TECEquipment> EquipmentItemsCollection
        {
            get { return _equipmentItemsCollection; }
            set
            {
                _equipmentItemsCollection = value;
                RaisePropertyChanged("EquipmentItemsCollection");
            }
        }

        private ObservableCollection<TECSubScope> _subScopeItemsCollection;
        public ObservableCollection<TECSubScope> SubScopeItemsCollection
        {
            get { return _subScopeItemsCollection; }
            set
            {
                _subScopeItemsCollection = value;
                RaisePropertyChanged("SubScopeItemsCollection");
            }
        }

        private ObservableCollection<TECDevice> _devicesItemsCollection;
        public ObservableCollection<TECDevice> DevicesItemsCollection
        {
            get { return _devicesItemsCollection; }
            set
            {
                _devicesItemsCollection = value;
                RaisePropertyChanged("DevicesItemsCollection");
            }
        }

        private ObservableCollection<TECController> _controllersItemsCollection;
        public ObservableCollection<TECController> ControllersItemsCollection
        {
            get { return _controllersItemsCollection; }
            set
            {
                _controllersItemsCollection = value;
                RaisePropertyChanged("ControllersItemsCollection");
            }
        }

        private ObservableCollection<TECAssociatedCost> _associatedCostsItemsCollection;
        public ObservableCollection<TECAssociatedCost> AssociatedCostsItemsCollection
        {
            get { return _associatedCostsItemsCollection; }
            set
            {
                _associatedCostsItemsCollection = value;
                RaisePropertyChanged("AssociatedCostsItemsCollection");
            }
        }

        private ObservableCollection<TECControlledScope> _controlledScopeItemsCollection;
        public ObservableCollection<TECControlledScope> ControlledScopeItemsCollection
        {
            get { return _controlledScopeItemsCollection; }
            set
            {
                _controlledScopeItemsCollection = value;
                RaisePropertyChanged("ControlledScopeItemsCollection");
            }
        }

        private ObservableCollection<TECPanel> _panelsItemsCollection;
        public ObservableCollection<TECPanel> PanelsItemsCollection
        {
            get { return _panelsItemsCollection; }
            set
            {
                _panelsItemsCollection = value;
                RaisePropertyChanged("PanelsItemsCollection");
            }
        }
        #endregion

        #region Search
        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
            }
        }
        #endregion
        #endregion

        #region Intializers
        public ScopeCollectionExtension(TECTemplates templates)
        {
            Templates = templates;
            SearchCollectionCommand = new RelayCommand(SearchCollectionExecute);
            EndSearchCommand = new RelayCommand(EndSearchExecute);
            AddTagCommand = new RelayCommand(AddTagExecute);
            AddManufacturerCommand = new RelayCommand(AddManufacturerExecute);
            AddDeviceCommand = new RelayCommand(AddDeviceExecute, CanAddDevice);
            AddControllerCommand = new RelayCommand(AddControllerExecute, CanAddController);
            AddIOToControllerCommand = new RelayCommand(AddIOToControllerExecute, canAddIO);
            AddTagToDeviceCommand = new RelayCommand(AddTagToDeviceExecute, CanAddTagToDevice);
            AddTagToControllerCommand = new RelayCommand(AddTagToControllerExecute, CanAddTagToController);
            AddTagToPanelCommand = new RelayCommand(AddTagToPanelExecute, CanAddTagToPanel);
            AddAssociatedCostToPanelCommand = new RelayCommand(AddAssociatedCostToPanelExecute);
            AddPanelCommand = new RelayCommand(AddPanelExecute, AddPanelCanExecute);

            populateItemsCollections();

            PanelTypeSelections = templates.PanelTypeCatalog;

            DeviceTags = new ObservableCollection<TECTag>();
            ControllerTags = new ObservableCollection<TECTag>();
            PanelTags = new ObservableCollection<TECTag>();
            PanelAssociatedCosts = new ObservableCollection<TECAssociatedCost>();
            ControllerIO = new ObservableCollection<TECIO>();

            setupInterfaceDefaults();
        }
        #endregion

        #region Methods

        public void Refresh(TECTemplates templates)
        {
            Templates = templates;
            PanelTypeSelections = templates.PanelTypeCatalog;
            populateItemsCollections();
            DeviceTags = new ObservableCollection<TECTag>();
            ControllerTags = new ObservableCollection<TECTag>();
            PanelTags = new ObservableCollection<TECTag>();
            PanelAssociatedCosts = new ObservableCollection<TECAssociatedCost>();
            ControllerIO = new ObservableCollection<TECIO>();
        }

        #region Commands

        private void SearchCollectionExecute()
        {
            if (SearchString != null)
            {
                switch (TabIndex)
                {
                    case ScopeCollectionIndex.System:
                        SystemItemsCollection = new ObservableCollection<TECSystem>();
                        foreach (TECSystem item in Templates.SystemTemplates)
                        {
                            if (item.Name.ToUpper().Contains(SearchString.ToUpper()))
                            {
                                SystemItemsCollection.Add(item);
                            }
                            foreach (TECTag tag in item.Tags)
                            {
                                if (tag.Text.ToUpper().Contains(SearchString.ToUpper()))
                                {
                                    SystemItemsCollection.Add(item);
                                }
                            }
                        }
                        break;
                    case ScopeCollectionIndex.Equipment:
                        EquipmentItemsCollection = new ObservableCollection<TECEquipment>();
                        foreach (TECEquipment item in Templates.EquipmentTemplates)
                        {
                            if (item.Name.ToUpper().Contains(SearchString.ToUpper()))
                            {
                                EquipmentItemsCollection.Add(item);
                            }
                            foreach (TECTag tag in item.Tags)
                            {
                                if (tag.Text.ToUpper().Contains(SearchString.ToUpper()))
                                {
                                    EquipmentItemsCollection.Add(item);
                                }
                            }
                        }
                        break;
                    case ScopeCollectionIndex.SubScope:
                        SubScopeItemsCollection = new ObservableCollection<TECSubScope>();
                        foreach (TECSubScope item in Templates.SubScopeTemplates)
                        {
                            if (item.Name.ToUpper().Contains(SearchString.ToUpper()))
                            {
                                SubScopeItemsCollection.Add(item);
                            }
                            foreach (TECTag tag in item.Tags)
                            {
                                if (tag.Text.ToUpper().Contains(SearchString.ToUpper()))
                                {
                                    SubScopeItemsCollection.Add(item);
                                }
                            }
                        }
                        break;
                    default:
                        break;

                }
            }
        }
        private void EndSearchExecute()
        {
            populateItemsCollections();
            SearchString = "";
        }

        private void AddTagExecute()
        {
            TECTag newTag = new TECTag();
            newTag.Text = TagName;
            Templates.Tags.Add(newTag);
        }
        private bool canAddTag()
        {
            if(TagName != "") { return true; }
            else { return false; }
        }

        private void AddManufacturerExecute()
        {
            TECManufacturer newMan = new TECManufacturer();
            newMan.Name = ManufacturerName;
            newMan.Multiplier = ManufacturerMultiplier;
            Templates.ManufacturerCatalog.Add(newMan);
        }
        private bool CanAddManufacturer()
        {
            if(ManufacturerName != "")
            { return true; }
            else { return false; }
        }

        private void AddDeviceExecute()
        {
            var newDevice = new TECDevice();
            newDevice.Name = DeviceName;
            newDevice.Description = DeviceDescription;
            newDevice.Cost = DeviceCost;
            newDevice.ConnectionType = DeviceConnectionType;
            newDevice.Manufacturer = DeviceManufacturer;
            newDevice.Tags = DeviceTags;
            Templates.DeviceCatalog.Add(newDevice);
            DeviceName = "";
            DeviceDescription = "";
            DeviceCost = 0;
            DeviceConnectionType = null;
            DeviceManufacturer = null;
            DeviceTags = new ObservableCollection<TECTag>();
        }
        private bool CanAddDevice()
        {
            if (DeviceManufacturer != null
                && DeviceConnectionType != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddControllerExecute()
        {
            var newController = new TECController();
            newController.Name = ControllerName;
            newController.Description = ControllerDescription;
            newController.Cost = ControllerCost;
            newController.IO = ControllerIO;
            newController.Tags = ControllerTags;
            newController.Manufacturer = ControllerManufacturer;
            Templates.ControllerTemplates.Add(newController);
            ControllerName = "";
            ControllerDescription = "";
            ControllerCost = 0;
            ControllerIO = new ObservableCollection<TECIO>();
            ControllerTags = new ObservableCollection<TECTag>();
            ControllerManufacturer = null;
        }
        private bool CanAddController()
        {
            if(ControllerManufacturer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddIOToControllerExecute()
        {
            var newIO = new TECIO();
            newIO.Type = ControllerIOType;
            newIO.Quantity = ControllerIOQTY;
            ControllerIO.Add(newIO);
        }
        private bool canAddIO()
        {
            bool hasIO = false;
            if (ControllerIO != null && ControllerIOType != 0)
            {
                foreach (TECIO io in ControllerIO)
                {
                    if (io.Type == ControllerIOType)
                    {
                        hasIO = true;
                        break;
                    }
                }
                return !hasIO;
            }
            else
            {
                return false;
            }
        }

        private void AddTagToDeviceExecute()
        {
            DeviceTags.Add(SelectedTag);
            SelectedTag = null;
        }
        private bool CanAddTagToDevice()
        {
            if (SelectedTag != null
                && !DeviceTags.Contains(SelectedTag))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void AddTagToControllerExecute()
        {
            ControllerTags.Add(SelectedTag);
            SelectedTag = null;
        }
        private bool CanAddTagToController()
        {
            if (SelectedTag != null
                && !ControllerTags.Contains(SelectedTag))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void AddTagToPanelExecute()
        {
            PanelTags.Add(SelectedTag);
            SelectedTag = null;
        }
        private bool CanAddTagToPanel()
        {
            if (SelectedTag != null
                && !PanelTags.Contains(SelectedTag))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void AddAssociatedCostToPanelExecute()
        {
            PanelAssociatedCosts.Add(SelectedAssociatedCost);
            SelectedAssociatedCost = null;
        }
        private void AddPanelExecute()
        {
            var panel = new TECPanel();
            panel.Type = SelectedPanelType;
            panel.Name = PanelName;
            panel.Description = PanelDescription;
            panel.Tags = PanelTags;
            panel.AssociatedCosts = PanelAssociatedCosts;
            Templates.PanelTemplates.Add(panel);
            SelectedPanelType = null;
            PanelName = "";
            PanelDescription = "";
            PanelTags = new ObservableCollection<TECTag>();
            PanelAssociatedCosts = new ObservableCollection<TECAssociatedCost>(); 
        }
        private bool AddPanelCanExecute()
        {
            if(SelectedPanelType != null && PanelName != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        private void setupInterfaceDefaults()
        {
            _deviceName = "";
            _deviceDescription = "";
            _deviceCost = 0;

            _controllerName = "";
            _controllerDescription = "";
            _controllerCost = 0;
            _controllerIOQTY = 0;

            _panelName = "";
            _panelDescription = "";
        }

        public void populateItemsCollections()
        {
            SystemItemsCollection = new ObservableCollection<TECSystem>();
            EquipmentItemsCollection = new ObservableCollection<TECEquipment>();
            SubScopeItemsCollection = new ObservableCollection<TECSubScope>();
            DevicesItemsCollection = new ObservableCollection<TECDevice>();
            ControllersItemsCollection = new ObservableCollection<TECController>();
            AssociatedCostsItemsCollection = new ObservableCollection<TECAssociatedCost>();
            ControlledScopeItemsCollection = new ObservableCollection<TECControlledScope>();
            PanelsItemsCollection = new ObservableCollection<TECPanel>();

            foreach (TECSystem sys in Templates.SystemTemplates)
            {
                SystemItemsCollection.Add(sys);
            }
            foreach (TECEquipment equip in Templates.EquipmentTemplates)
            {
                EquipmentItemsCollection.Add(equip);
            }
            foreach(TECSubScope ss in Templates.SubScopeTemplates)
            {
                SubScopeItemsCollection.Add(ss);
            }
            foreach(TECDevice dev in Templates.DeviceCatalog)
            {
                DevicesItemsCollection.Add(dev);
            }
            foreach(TECController control in Templates.ControllerTemplates)
            {
                ControllersItemsCollection.Add(control);
            }
            foreach(TECAssociatedCost assCost in Templates.AssociatedCostsCatalog)
            {
                AssociatedCostsItemsCollection.Add(assCost);
            }
            foreach(TECControlledScope controlScope in Templates.ControlledScopeTemplates)
            {
                ControlledScopeItemsCollection.Add(controlScope);
            }
            foreach(TECPanel panel in Templates.PanelTemplates)
            {
                PanelsItemsCollection.Add(panel);
            }
        }

        private void unsubscribeTemplatesCollections()
        {
            if (Templates != null)
            {
                Templates.SystemTemplates.CollectionChanged -= SystemTemplates_CollectionChanged;
                Templates.EquipmentTemplates.CollectionChanged -= EquipmentTemplates_CollectionChanged;
                Templates.SubScopeTemplates.CollectionChanged -= SubScopeTemplates_CollectionChanged;
                Templates.DeviceCatalog.CollectionChanged -= DeviceCatalog_CollectionChanged;
                Templates.ControllerTemplates.CollectionChanged -= ControllerTemplates_CollectionChanged;
                Templates.AssociatedCostsCatalog.CollectionChanged -= AssociatedCostsCatalog_CollectionChanged;
                Templates.ControlledScopeTemplates.CollectionChanged -= ControlledScopeTemplates_CollectionChanged;
                Templates.PanelTemplates.CollectionChanged -= PanelTemplates_CollectionChanged;
            }
        }

        private void subscribeTemplatesCollections()
        {
            Templates.SystemTemplates.CollectionChanged += SystemTemplates_CollectionChanged;
            Templates.EquipmentTemplates.CollectionChanged += EquipmentTemplates_CollectionChanged;
            Templates.SubScopeTemplates.CollectionChanged += SubScopeTemplates_CollectionChanged;
            Templates.DeviceCatalog.CollectionChanged += DeviceCatalog_CollectionChanged;
            Templates.ControllerTemplates.CollectionChanged += ControllerTemplates_CollectionChanged;
            Templates.AssociatedCostsCatalog.CollectionChanged += AssociatedCostsCatalog_CollectionChanged;
            Templates.ControlledScopeTemplates.CollectionChanged += ControlledScopeTemplates_CollectionChanged;
            Templates.PanelTemplates.CollectionChanged += PanelTemplates_CollectionChanged;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragHandler(dropInfo);
        }
        public void Drop(IDropInfo dropInfo)
        {
            DropHandler(dropInfo);
        }
        

        #endregion

        #region Event Handlers
        private void SystemTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach(object item in e.NewItems)
                {
                    SystemItemsCollection.Add(item as TECSystem);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach(object item in e.OldItems)
                {
                    SystemItemsCollection.Remove(item as TECSystem);
                }
            }
        }

        private void EquipmentTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    EquipmentItemsCollection.Add(item as TECEquipment);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    EquipmentItemsCollection.Remove(item as TECEquipment);
                }
            }
        }

        private void SubScopeTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    SubScopeItemsCollection.Add(item as TECSubScope);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    SubScopeItemsCollection.Remove(item as TECSubScope);
                }
            }
        }

        private void DeviceCatalog_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    DevicesItemsCollection.Add(item as TECDevice);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    DevicesItemsCollection.Remove(item as TECDevice);
                }
            }
        }

        private void ControllerTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    ControllersItemsCollection.Add(item as TECController);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    ControllersItemsCollection.Remove(item as TECController);
                }
            }
        }

        private void AssociatedCostsCatalog_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    AssociatedCostsItemsCollection.Add(item as TECAssociatedCost);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    AssociatedCostsItemsCollection.Remove(item as TECAssociatedCost);
                }
            }
        }

        private void ControlledScopeTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    ControlledScopeItemsCollection.Add(item as TECControlledScope);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    ControlledScopeItemsCollection.Remove(item as TECControlledScope);
                }
            }
        }

        private void PanelTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    PanelsItemsCollection.Add(item as TECPanel);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    PanelsItemsCollection.Remove(item as TECPanel);
                }
            }
        }
        #endregion
    }
}
