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
        public ManufacturersCatalogVM ManufacturerVM { get; }
        public TagsCatalogVM TagVM { get; }
        public MiscCostsVM MiscVM { get; }
        public ProtocolsCatalogVM ProtocolVM { get; }

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

        public MaterialVM(TECTemplates templates)
        {
            ReferenceDropHandler = new ReferenceDropper(templates);
            Templates = templates;

            //Setup VMs
            subscribeToVM(DeviceVM = new DevicesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ValveVM = new ValvesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ConnectionTypeVM = new ConnectionTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ConduitTypeVM = new ConduitTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ControllerTypeVM = new ControllerTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(PanelTypeVM = new PanelTypesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(AssociatedCostVM = new AssociatedCostsCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(IOModuleVM = new IOModulesCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(ManufacturerVM = new ManufacturersCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(TagVM = new TagsCatalogVM(templates, ReferenceDropHandler));
            subscribeToVM(MiscVM = new MiscCostsVM(templates));
            subscribeToVM(ProtocolVM = new ProtocolsCatalogVM(templates, ReferenceDropHandler));
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