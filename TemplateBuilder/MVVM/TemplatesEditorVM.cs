using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Windows.Input;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TemplateBuilder.MVVM
{
    public class TemplatesEditorVM : ViewModelBase, EditorVM, IDropTarget
    {
        private TECScopeTemplates _templates;
        private object _selected;
        private TECTemplates manager;

        public ScopeCollectionsTabVM ScopeCollection { get; }
        public MaterialVM MaterialsTab { get; }
        public SystemHierarchyVM SystemHierarchyVM { get; }
        public EquipmentHierarchyVM EquipmentHierarchyVM { get; }
        public SubScopeHierarchyVM SubScopeHierarchyVM { get; }
        public PropertiesVM PropertiesVM { get; }
        public MiscCostsVM MiscVM { get; }

        public TECScopeTemplates Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
                RaisePropertyChanged("Templates");
            }
        }
        public object Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
                SelectionChanged?.Invoke(value);
                PropertiesVM.Selected = value as TECObject;
            }
        }
        public ICommand AddParameterCommand { get; private set; }

        public TemplatesEditorVM(TECTemplates templatesManager)
        {
            manager = templatesManager;
            Templates = templatesManager.Templates;
            ScopeCollection = new ScopeCollectionsTabVM(templatesManager);
            MaterialsTab = new MaterialVM(templatesManager);
            MaterialsTab.SelectionChanged += obj => {
                Selected = obj;
            };
            SystemHierarchyVM = new SystemHierarchyVM(templatesManager, true);
            SystemHierarchyVM.Selected += obj => { Selected = obj; };
            EquipmentHierarchyVM = new EquipmentHierarchyVM(templatesManager);
            EquipmentHierarchyVM.Selected += obj => { Selected = obj; };
            SubScopeHierarchyVM = new SubScopeHierarchyVM(templatesManager);
            SubScopeHierarchyVM.Selected += obj => { Selected = obj; };

            MiscVM = new MiscCostsVM(templatesManager);
            MiscVM.SelectionChanged += obj => { Selected = obj; };
            
            PropertiesVM = new PropertiesVM(templatesManager.Catalogs, templatesManager);

            AddParameterCommand = new RelayCommand(AddParametersExecute);

        }

        public event Action<Object> SelectionChanged;

        public void DragOver(IDropInfo dropInfo)
        {
            DragDropHelpers.StandardDragOver(dropInfo);
        }
        public void Drop(IDropInfo dropInfo)
        {
            DragDropHelpers.StandardDrop(dropInfo, manager);
        }

        private void AddParametersExecute()
        {
            Templates.Parameters.Add(new TECParameters(Guid.NewGuid()));
        }
    }
}
