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
        public ICommand AddManufacturerCommand { get; private set; }
        public ICommand AddTagCommand { get; private set; }
        #endregion
        
        public event Action<Object> SelectionChanged;

        #region ViewModels
        public DevicesCatalogVM DeviceVM { get; }
        public ValvesCatalogVM ValveVM { get; }
        public ConnectionTypesCatalogVM ConnectionTypeVM { get; }
        public ConduitTypesCatalogVM ConduitTypeVM { get; }
        public ControllerTypesCatalogVM ControllerTypeVM { get; }
        public PanelTypesCatalogVM PanelTypeVM { get; }
        public AssociatedCostsCatalogVM AssociatedCostVM { get; }
        public IOModulesCatalogVM IOModuleVM { get; }
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
            subscribeToVM(PanelTypeVM = new PanelTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(AssociatedCostVM = new AssociatedCostsCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(IOModuleVM = new IOModulesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(MiscVM = new MiscCostsVM(templates));
        }
        
        #region Methods
        private void setupCommands()
        {
            AddManufacturerCommand = new RelayCommand(addManufacturerExecute, canAddManufacturer);
            AddTagCommand = new RelayCommand(addTagExecute, canAddTag);
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