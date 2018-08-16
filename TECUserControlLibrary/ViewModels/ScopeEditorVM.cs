using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScopeEditorVM : ViewModelBase, IDropTarget
    {
        #region Properties
        private TECObject selected;
        private GridIndex _dGTabIndex;

        public TECObject Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                RaisePropertyChanged("Selected");
                PropertiesVM.Selected = value;
            }
        }
        public GridIndex DGTabIndex
        {
            get { return _dGTabIndex; }
            set
            {
                _dGTabIndex = value;
                RaisePropertyChanged("DGTabIndex");
                Selected = null;
            }
        }

        #region Extensions
        public ScopeCollectionsTabVM ScopeCollection { get; set; }
        public ControllersPanelsVM ControllersPanelsTab { get; set; }
        public MiscCostsVM MiscVM { get; set; }
        public SystemHierarchyVM TypicalEditVM { get; set; }
        public TypicalHierarchyVM InstanceEditVM { get; set; }
        public PropertiesVM PropertiesVM { get; }
        public WorkBoxVM WorkBoxVM { get; }
        public ConnectionsVM ConnectionsVM { get; }
        #endregion

        #region Interface Properties

        #region Scope Properties
        public TECScopeTemplates Templates { get { return Bid.Templates; } }

        public TECBid Bid { get; }
        #endregion Scope Properties

        #endregion //Interface Properties

        #region Visibility Properties
        private Visibility _templatesVisibility;
        public Visibility TemplatesVisibility
        {
            get { return _templatesVisibility; }
            set
            {
                if (value != _templatesVisibility)
                {
                    _templatesVisibility = value;
                    RaisePropertyChanged("TemplatesVisibility");
                }
            }
        }
        #endregion Visibility Properties
        #endregion //Properties

        //Initializer
        public ScopeEditorVM(TECBid bid, ChangeWatcher watcher)
        {
            Bid = bid;

            setupScopeCollection();
            setupControllersPanelsTab();
            setupMiscVM();
            TypicalEditVM = new SystemHierarchyVM(bid, true);
            TypicalEditVM.Selected += item =>
            {
                Selected = item;
            };
            InstanceEditVM = new TypicalHierarchyVM(bid, watcher);
            InstanceEditVM.Selected += item => {
                Selected = item;
            };
            PropertiesVM = new PropertiesVM(bid.Catalogs, bid);
            WorkBoxVM = new WorkBoxVM(bid);
            ConnectionsVM = new ConnectionsVM(bid, watcher, bid.Catalogs, locations: bid.Locations, filterPredicate: filterPredicate);

            bool filterPredicate(ITECObject obj)
            {
                if (obj is ITypicalable typable)
                {
                    return (typable is TECTypical || !typable.IsTypical);
                }
                else
                {
                    return true;
                }
            }

            ConnectionsVM.Selected += item =>
            {
                Selected = item;
            };
            DGTabIndex = GridIndex.Systems;
            TemplatesVisibility = Visibility.Visible;
        }
        
        #region Methods

        #region Setup Extensions

        private void setupScopeCollection()
        {
            ScopeCollection = new ScopeCollectionsTabVM(Bid);
            ScopeCollection.DragHandler += DragOver;
            ScopeCollection.DropHandler += Drop;
        }
        private void setupControllersPanelsTab()
        {
            ControllersPanelsTab = new ControllersPanelsVM(Bid);
            ControllersPanelsTab.SelectionChanged += obj =>
            {
                Selected = obj as TECObject;

            };
        }
        private void setupMiscVM()
        {
            MiscVM = new MiscCostsVM(Bid);
            MiscVM.SelectionChanged += misc =>
            {
                Selected = misc;
            };
        }
        #endregion

        #region Drag Drop
        public void DragOver(IDropInfo dropInfo)
        {
            if(dropInfo.Data is TECSystem)
            {
                DragDropHelpers.SystemToTypicalDragOver(dropInfo);
            }
            else
            {
                DragDropHelpers.StandardDragOver(dropInfo);
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TECSystem)
            {
                DragDropHelpers.SystemToTypicalDrop(dropInfo, Bid);
            }
            else
            {
                DragDropHelpers.StandardDrop(dropInfo, Bid);
            }
        }
        #endregion

        #region Helper Methods
        #endregion //Helper Methods

        #region Event Handlers
        #endregion
        #endregion //Methods
    }
}