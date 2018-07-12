using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels.AddVMs;

namespace TECUserControlLibrary.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ControllersPanelsVM : ViewModelBase, IDropTarget
    {
        #region Properties
        private Action<TECController> addControllerMethod;
        private Action<TECPanel> addPanelMethod;
        private Action<TECController> deleteControllerMethod;
        private Action<TECPanel> deletePanelMethod;

        private TECObject parent;

        private TECBid _bid;
        public TECBid Bid
        {
            get { return _bid; }
            private set
            {
                _bid = value;
                RaisePropertyChanged("Bid");
            }
        }
        private TECTemplates _templates;
        public TECTemplates Templates
        {
            get { return _templates; }
            private set
            {
                _templates = value;
                RaisePropertyChanged("Templates");
            }
        }
        private TECSystem _selectedSystem;
        public TECSystem SelectedSystem
        {
            get { return _selectedSystem; }
            private set
            {
                _selectedSystem = value;
                RaisePropertyChanged("SelectedSystem");
            }
        }

        public bool PanelSelectionReadOnly { get; private set; }
        public Visibility PanelSelectionVisibility { get; private set; }

        private ObservableCollection<TECController> sourceControllers;
        private ObservableCollection<TECPanel> sourcePanels;
        public ObservableCollection<TECPanel> PanelsSource
        {
            get { return sourcePanels; }
            set
            {
                sourcePanels = value;
                RaisePropertyChanged("PanelsSource");
            }
        }

        private TECController _selectedController;
        public TECController SelectedController
        {
            get { return _selectedController; }
            set
            {
                _selectedController = value;
                RaisePropertyChanged("SelectedController");
                SelectionChanged?.Invoke(value);
            }
        }
        private TECPanel _selectedPanel;
        public TECPanel SelectedPanel
        {
            get { return _selectedPanel; }
            set
            {
                _selectedPanel = value;
                RaisePropertyChanged("SelectedPanel");
                SelectionChanged?.Invoke(value);
            }
        }

        private ObservableCollection<ControllerInPanel> _controllerCollection;
        public ObservableCollection<ControllerInPanel> ControllerCollection
        {
            get
            {
                return _controllerCollection;
            }
            set
            {
                _controllerCollection = value;
                ControllerCollection.CollectionChanged -= collectionChanged;
                RaisePropertyChanged("ControllerCollection");
                ControllerCollection.CollectionChanged += collectionChanged;
            }
        }

        private ObservableCollection<TECPanel> _panelSelections;
        public ObservableCollection<TECPanel> PanelSelections
        {
            get { return _panelSelections; }
            set
            {
                _panelSelections = value;
                RaisePropertyChanged("PanelSelections");
            }
        }

        private ControllerInPanel _selectedControllerInPanel;
        public ControllerInPanel SelectedControllerInPanel
        {
            get { return _selectedControllerInPanel; }
            set
            {
                _selectedControllerInPanel = value;
                RaisePropertyChanged("SelectedControllerInPanel");
                if (value != null)
                {
                    SelectionChanged?.Invoke(value.Controller);
                }
            }
        }
        
        private TECPanel _nonePanel;
        public TECPanel NonePanel
        {
            get { return _nonePanel; }
            set
            {
                _nonePanel = value;
                RaisePropertyChanged("NonePanel");
            }
        }

        private Dictionary<TECController, ControllerInPanel> controllersIndex;

        private ViewModelBase selectedVM;
        public ViewModelBase SelectedVM
        {
            get { return selectedVM; }
            set
            {
                selectedVM = value;
                RaisePropertyChanged("SelectedVM");
            }
        }

        public RelayCommand AddControllerCommand { get; private set; }
        public RelayCommand AddPanelCommand { get; private set; }
        public RelayCommand<TECController> DeleteControllerCommand { get; private set; }
        public RelayCommand<TECPanel> DeletePanelCommand { get; private set; }
        public RelayCommand<TECProvidedController> ChangeTypeCommand { get; private set; }

        #region Delegates
        public Action<IDropInfo> DragHandler;
        public Action<IDropInfo> DropHandler;

        public Action<TECObject> SelectionChanged;
        #endregion

        #endregion

        #region Constructor
        private ControllersPanelsVM(TECObject parent, IEnumerable<TECController> controllers, ObservableCollection<TECPanel> panels)
        {
            
            AddControllerCommand = new RelayCommand(addControllerExecute, canAddController);
            AddPanelCommand = new RelayCommand(addPanelExecute, canAddPanel);
            DeleteControllerCommand = new RelayCommand<TECController>(deleteControllerExecute);
            DeletePanelCommand = new RelayCommand<TECPanel>(deletePanelExecute);
            ChangeTypeCommand = new RelayCommand<TECProvidedController>(changeTypeExecute, canChangeType);

            Refresh(parent, controllers, panels);
        }

        private void changeTypeExecute(TECProvidedController obj)
        {
            ObservableCollection<TECControllerType> types = Bid != null ? Bid.Catalogs.ControllerTypes : Templates.Catalogs.ControllerTypes;

            SelectedVM = new ChangeControllerTypeVM(obj, types);
        }

        private bool canChangeType(TECController arg)
        {
            if (arg is TECProvidedController provided)
            {
                ObservableCollection<TECControllerType> types = Bid != null ? Bid.Catalogs.ControllerTypes : Templates.Catalogs.ControllerTypes;

                return provided != null && types.Any(x => provided.CanChangeType(x));
            }
            else
            {
                return false;
            }
        }

        public ControllersPanelsVM(TECBid bid) : this(bid, bid.Controllers, bid.Panels)
        {
            PanelSelectionReadOnly = false;
            PanelSelectionVisibility = Visibility.Visible;
            Bid = bid;
        }
        public ControllersPanelsVM(TECTemplates templates) 
            : this(templates, templates.Templates.ControllerTemplates, templates.Templates.PanelTemplates)
        {
            PanelSelectionReadOnly = false;
            PanelSelectionVisibility = Visibility.Collapsed;
            Templates = templates;
            
        }
        public ControllersPanelsVM(TECSystem system, TECScopeManager manager,
            bool canSelectPanel = true) : this(system, system.Controllers, system.Panels)
        {
            if(manager is TECBid)
            {
                _bid = manager as TECBid;
            }else
            {
                _templates = manager as TECTemplates;
            }
            PanelSelectionReadOnly = !canSelectPanel;
            PanelSelectionVisibility = Visibility.Visible;
            SelectedSystem = system;
            
        }
        #endregion

        #region Methods
        private void Refresh(TECObject parent, IEnumerable<TECController> controllers, ObservableCollection<TECPanel> panels)
        {
            setupAddRemoveMethods(parent);

            unregisterChanges();
            sourceControllers = new ObservableCollection<TECController>(controllers);
            PanelsSource = panels;

            populateControllerCollection();
            populatePanelSelections();
            registerChanges();
            this.parent = parent;
            parent.TECChanged += parentChanged;
        }
        public void Refresh(TECBid bid)
        {
            Bid.TECChanged -= parentChanged;
            Bid = bid;
            Refresh(bid, bid.Controllers, bid.Panels);
        }
        public void Refresh(TECTemplates templates)
        {
            Templates.TECChanged -= parentChanged;
            Templates = templates;
            Refresh(templates, templates.Templates.ControllerTemplates, templates.Templates.PanelTemplates);
        }
        public void Refresh(TECSystem system, TECScopeManager manager = null)
        {
            parent.TECChanged -= parentChanged;
            if (manager != null)
            {
                if (manager is TECBid)
                {
                    _bid = manager as TECBid;
                }
                else
                {
                    _templates = manager as TECTemplates;
                }
            }
            SelectedSystem = system;
            Refresh(system, system.Controllers, system.Panels);
        }

        private void setupAddRemoveMethods(TECObject obj)
        {
            if(obj is TECSystem system)
            {
                setupAddRemoveMethods(system);
            }
            else if (obj is TECBid bid)
            {
                setupAddRemoveMethods(bid);
            }
            else if (obj is TECTemplates templates)
            {
                setupAddRemoveMethods(templates);
            }
            else
            {
                throw new Exception("Cannot setup methods for object: " + obj);
            }
        }
        private void setupAddRemoveMethods(TECBid bid)
        {
            addControllerMethod = bid.AddController;
            addPanelMethod = bid.Panels.Add;
            deleteControllerMethod = controller => {
                bid.RemoveController(controller);
            };
            deletePanelMethod = panel => { bid.Panels.Remove(panel); };
        }
        private void setupAddRemoveMethods(TECTemplates templates)
        {
            addControllerMethod = templates.Templates.ControllerTemplates.Add;
            addPanelMethod = templates.Templates.PanelTemplates.Add;
            deleteControllerMethod = controller => {
                controller.DisconnectAll();
                templates.Templates.ControllerTemplates.Remove(controller);
            };
            deletePanelMethod = panel => { templates.Templates.PanelTemplates.Remove(panel); };
        }
        private void setupAddRemoveMethods(TECSystem system)
        {
            deleteControllerMethod = controller =>
            {
                system.RemoveController(controller);
            };
            deletePanelMethod = panel => { system.Panels.Remove(panel); };
        }

        private void addControllerExecute()
        {
            var controllerTypes = Templates == null ? Bid.Catalogs.ControllerTypes: Templates.Catalogs.ControllerTypes;
            TECScopeManager scopeManager = Templates as TECScopeManager ?? Bid as TECScopeManager;
            SelectedVM = addPanelMethod != null ? new AddControllerVM(addControllerMethod, controllerTypes, scopeManager) :
                SelectedVM = new AddControllerVM(SelectedSystem, controllerTypes, scopeManager);
        }
        private bool canAddController()
        {
            var controllerTypes = Templates == null ? Bid.Catalogs.ControllerTypes : Templates.Catalogs.ControllerTypes;
            return controllerTypes.Count > 0;
        }

        private void addPanelExecute()
        {
            var panelTypes = Templates == null ? Bid.Catalogs.PanelTypes : Templates.Catalogs.PanelTypes;
            TECScopeManager scopeManager = Templates as TECScopeManager ?? Bid as TECScopeManager;
            SelectedVM = addPanelMethod != null ? new AddPanelVM(addPanelMethod, panelTypes, scopeManager) :
                SelectedVM = new AddPanelVM(SelectedSystem, panelTypes, scopeManager);
            
        }
        private bool canAddPanel()
        {
            var panelTypes = Templates == null ? Bid.Catalogs.PanelTypes : Templates.Catalogs.PanelTypes;
            return panelTypes.Count > 0;
        }

        private void deleteControllerExecute(TECController controller)
        {
            deleteControllerMethod(controller);
        }
        private void deletePanelExecute(TECPanel panel)
        {
            deletePanelMethod(panel);
        }

        private void populateControllerCollection()
        {
            ControllerCollection = new ObservableCollection<ControllerInPanel>();
            controllersIndex = new Dictionary<TECController, ControllerInPanel>();
            foreach (TECController controller in sourceControllers)
            {
                TECController controllerToAdd = controller;
                TECPanel panelToAdd = sourcePanels.FirstOrDefault(panel =>
                    { return panel.Controllers.Contains(controller); });
                var controllerInPanelToAdd = new ControllerInPanel(controllerToAdd, panelToAdd);
                ControllerCollection.Add(controllerInPanelToAdd);
                controllersIndex[controller] = controllerInPanelToAdd;
            }
        }
        private void populatePanelSelections()
        {
            PanelSelections = new ObservableCollection<TECPanel>();
            var nonePanel = new TECPanel(new TECPanelType(new TECManufacturer()));
            nonePanel.Name = "None";
            NonePanel = nonePanel;
            PanelSelections.Add(NonePanel);
            foreach (TECPanel panel in sourcePanels)
            {
                PanelSelections.Add(panel);
            }
        }
        
        private void registerChanges()
        {
            sourceControllers.CollectionChanged += collectionChanged;
            sourcePanels.CollectionChanged += collectionChanged;
        }
        private void unregisterChanges()
        {
            if(sourceControllers != null && sourcePanels != null)
            {
                sourceControllers.CollectionChanged -= collectionChanged;
                sourcePanels.CollectionChanged -= collectionChanged;
            }
        }

        private void collectionChanged(object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is TECController)
                    {
                        addController(item as TECController);
                    }
                    else if (item is TECPanel)
                    {
                        addPanel(item as TECPanel);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    if (item is TECController)
                    {
                        removeController(item as TECController);
                    }
                    else if (item is TECPanel)
                    {
                        removePanel(item as TECPanel);
                    }
                }
            }
        }
        private void parentChanged(TECChangedEventArgs e)
        {
            if (e.Change == Change.Add && e.Value is TECController controller && e.Sender == parent)
            {
                sourceControllers.Add(controller);
            }
            else if (e.Change == Change.Remove && e.Value is TECController oldController && e.Sender == parent)
            {
                sourceControllers.Remove(oldController);
            }
        }
        
        public void DragOver(IDropInfo dropInfo)
        {
            if (!PanelSelectionReadOnly)
            {
                UIHelpers.DragOver(dropInfo, (item, sourceType, targetType) =>
                {
                    bool controllerDrag = (sourceType == typeof(TECController) && targetType == typeof(ControllerInPanel));
                    bool typesMatch = sourceType == targetType;
                    bool controllerTypeDrag = (sourceType == typeof(TECControllerType) && targetType == typeof(ControllerInPanel));
                    bool panelTypeDrag = (sourceType == typeof(TECPanelType) && targetType == typeof(TECPanel));
                    return controllerDrag || typesMatch || controllerTypeDrag || panelTypeDrag;
                },
                () => { });
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            TECScopeManager scopeManager = Templates != null ? Templates as TECScopeManager : Bid as TECScopeManager;
            UIHelpers.Drop(dropInfo, (item) =>
            {
                if (item is TECProvidedController controller)
                {
                    var controllerTypes = Templates == null ? Bid.Catalogs.ControllerTypes : Templates.Catalogs.ControllerTypes;
                    SelectedVM = addControllerMethod != null ? new AddControllerVM(addControllerMethod, controllerTypes, scopeManager) :
                        new AddControllerVM(SelectedSystem, controllerTypes, scopeManager);
                    TECProvidedController dropped = (TECProvidedController)((IDDCopiable)controller).DragDropCopy(scopeManager);
                    ((AddControllerVM)SelectedVM).SetTemplate(dropped);
                    ((AddControllerVM)SelectedVM).SelectedType = dropped.Type;
                }
                else if (item is TECPanel panel)
                {
                    var panelTypes = Templates == null ? Bid.Catalogs.PanelTypes : Templates.Catalogs.PanelTypes;
                    SelectedVM = addPanelMethod != null ? new AddPanelVM(addPanelMethod, panelTypes, scopeManager) :
                        new AddPanelVM(SelectedSystem, panelTypes, scopeManager);
                    ((AddPanelVM)SelectedVM).SetTemplate(panel);
                }
                else if (item is TECControllerType controllerType)
                {
                    var controllerTypes = Templates == null ? Bid.Catalogs.ControllerTypes : Templates.Catalogs.ControllerTypes;
                    SelectedVM = addControllerMethod != null ? new AddControllerVM(addControllerMethod, controllerTypes, scopeManager) :
                        new AddControllerVM(SelectedSystem, controllerTypes, scopeManager);
                    ((AddControllerVM)SelectedVM).SelectedType = controllerType;
                }
                else if (item is TECPanelType panelType)
                {
                    var panelTypes = Templates == null ? Bid.Catalogs.PanelTypes : Templates.Catalogs.PanelTypes;
                    SelectedVM = addPanelMethod != null ? new AddPanelVM(addPanelMethod, panelTypes, scopeManager) :
                        SelectedVM = new AddPanelVM(SelectedSystem, panelTypes, scopeManager);
                    ((AddPanelVM)SelectedVM).ToAdd.Type = panelType;
                }
                return null;
            },
            false);
            
        }

        private void addController(TECController controller)
        {
            TECController controllerToAdd = controller;
            TECPanel panelToAdd = null;
            
            var controllerInPanelToAdd = new ControllerInPanel(controllerToAdd, panelToAdd);
            ControllerCollection.Add(controllerInPanelToAdd);
            controllersIndex[controller] = controllerInPanelToAdd;
        }
        private void addPanel(TECPanel panel)
        {
            PanelSelections.Add(panel);
            foreach (TECController controller in panel.Controllers)
            {
                controllersIndex[controller].UpdatePanel(panel);
            }
        }
        private void removeController(TECController controller)
        {
            ControllerCollection.Remove(controllersIndex[controller]);
            controllersIndex.Remove(controller);
        }
        private void removePanel(TECPanel panel)
        {
            PanelSelections.Remove(panel);
        }
        #endregion
    }
}