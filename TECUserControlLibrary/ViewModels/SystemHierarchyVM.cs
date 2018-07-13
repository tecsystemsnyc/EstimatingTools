using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels.AddVMs;

namespace TECUserControlLibrary.ViewModels
{
    public class SystemHierarchyVM : ViewModelBase, IDropTarget
    {
        private TECCatalogs catalogs;
        private TECScopeManager scopeManager;
        private ViewModelBase selectedVM;
        private TECSystem selectedSystem;
        private TECEquipment selectedEquipment;
        private TECSubScope selectedSubScope;
        private IEndDevice selectedDevice;
        private TECPoint selectedPoint;
        private TECController selectedController;
        private TECPanel selectedPanel;
        private TECInterlockConnection selectedInterlock;
        private MiscCostsVM miscVM;
        private ControllersPanelsVM controllersPanelsVM;
        private ValveSelectionVM valveVM;
        private ConnectionsVM _connectionsVM;

        public ViewModelBase SelectedVM
        {
            get { return selectedVM; }
            set
            {
                selectedVM = value;
                RaisePropertyChanged("SelectedVM");
            }
        }
        public TECSystem SelectedSystem
        {
            get { return selectedSystem; }
            set
            {
                selectedSystem = value;
                RaisePropertyChanged("SelectedSystem");
                Selected?.Invoke(value);
                SystemSelected(value);
            }
        }
        public TECEquipment SelectedEquipment
        {
            get { return selectedEquipment; }
            set
            {
                selectedEquipment = value;
                RaisePropertyChanged("SelectedEquipment");
                Selected?.Invoke(value);
            }
        }
        public TECSubScope SelectedSubScope
        {
            get { return selectedSubScope; }
            set
            {
                selectedSubScope = value;
                RaisePropertyChanged("SelectedSubScope");
                Selected?.Invoke(value);
            }
        }
        public IEndDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
                Selected?.Invoke(value as TECObject);
            }
        }
        public TECPoint SelectedPoint
        {
            get { return selectedPoint; }
            set
            {
                selectedPoint= value;
                RaisePropertyChanged("SelectedPoint");
                Selected?.Invoke(value);
            }
        }
        public TECController SelectedController
        {
            get { return selectedController; }
            set
            {
                selectedController = value;
                RaisePropertyChanged("SelectedController");
                Selected?.Invoke(value);
            }
        }
        public TECPanel SelectedPanel
        {
            get { return selectedPanel; }
            set
            {
                selectedPanel = value;
                RaisePropertyChanged("SelectedPanel");
                Selected?.Invoke(value);
            }
        }
        public TECInterlockConnection SelectedInterlock
        {
            get { return selectedInterlock; }
            set
            {
                selectedInterlock = value;
                RaisePropertyChanged("SelectedInterlock");
                Selected?.Invoke(value);
            }
        }

        public bool CanEdit { get; }
        public bool IsTemplates { get; }
        //public bool 

        public RelayCommand AddSystemCommand { get; private set; }
        public RelayCommand<TECSystem> AddEquipmentCommand { get; private set; }
        public RelayCommand<TECEquipment> AddSubScopeCommand { get; private set; }
        public RelayCommand<TECSubScope> AddPointCommand { get; private set; }
        public RelayCommand<TECSystem> AddControllerCommand { get; private set; }
        public RelayCommand<TECSystem> AddPanelCommand { get; private set; }
        public RelayCommand<TECSystem> AddMiscCommand { get; private set; }
        public RelayCommand<IInterlockable> AddInterlockCommand { get; private set; }
        public RelayCommand<object> BackCommand { get; private set; }
        public RelayCommand<TECScopeBranch> AddScopeBranchCommand { get; private set; }

        public RelayCommand<TECSystem> DeleteSystemCommand { get; private set; }
        public RelayCommand<TECEquipment> DeleteEquipmentCommand { get; private set; }
        public RelayCommand<TECSubScope> DeleteSubScopeCommand { get; private set; }
        public RelayCommand<IEndDevice> DeleteDeviceCommand { get; private set; }
        public RelayCommand<TECPoint> DeletePointCommand { get; private set; }
        public RelayCommand<TECController> DeleteControllerCommand { get; private set; }
        public RelayCommand<TECPanel> DeletePanelCommand { get; private set; }
        public RelayCommand<TECInterlockConnection> DeleteInterlockCommand { get; private set; }

        public RelayCommand UpdateInstanceConnectionsCommand { get; private set; }

        public MiscCostsVM MiscVM
        {
            get { return miscVM; }
            set
            {
                miscVM = value;
                RaisePropertyChanged("MiscVM");
                miscVM.SelectionChanged += raiseSelected;
            }
        }
        public ControllersPanelsVM ControllersPanelsVM
        {
            get { return controllersPanelsVM; }
            set
            {
                controllersPanelsVM = value;
                RaisePropertyChanged("ControllersPanelsVM");
                controllersPanelsVM.SelectionChanged += raiseSelected;
            }
        }
        public ValveSelectionVM ValveVM
        {
            get { return valveVM; }
            set
            {
                valveVM = value;
                RaisePropertyChanged("ValveVM");
            }
        }
        public ConnectionsVM ConnectionsVM
        {
            get { return _connectionsVM; }
            set
            {
                _connectionsVM = value;
                RaisePropertyChanged("ConnectionsVM");
                _connectionsVM.Selected += raiseSelected;
            }
        }

        public SystemHierarchyVM(TECScopeManager scopeManager, bool canEdit)
        {
            IsTemplates = scopeManager is TECTemplates;
            CanEdit = canEdit;

            AddSystemCommand = new RelayCommand(addSystemExecute, canAddSystem);
            AddEquipmentCommand = new RelayCommand<TECSystem>(addEquipmentExecute, canAddEquipment);
            AddSubScopeCommand = new RelayCommand<TECEquipment>(addSubScopeExecute, canAddSubScope);
            AddPointCommand = new RelayCommand<TECSubScope>(addPointExecute, canAddPoint);
            AddControllerCommand = new RelayCommand<TECSystem>(addControllerExecute, canAddController);
            AddPanelCommand = new RelayCommand<TECSystem>(addPanelExecute, canAddPanel);
            AddMiscCommand = new RelayCommand<TECSystem>(addMiscExecute, canAddMisc);
            AddInterlockCommand = new RelayCommand<IInterlockable>(addInterlockExecute, canAddInterlock);
            BackCommand = new RelayCommand<object>(backExecute);
            AddScopeBranchCommand = new RelayCommand<TECScopeBranch>(addBranchExecute);

            DeleteSystemCommand = new RelayCommand<TECSystem>(deleteSystemExecute, canDeleteSystem);
            DeleteEquipmentCommand = new RelayCommand<TECEquipment>(deleteEquipmentExecute, canDeleteEquipment);
            DeleteSubScopeCommand = new RelayCommand<TECSubScope>(deleteSubScopeExecute, canDeleteSubScope);
            DeleteDeviceCommand = new RelayCommand<IEndDevice>(deleteDeviceExecute, canDeleteDevice);
            DeletePointCommand = new RelayCommand<TECPoint>(deletePointExecute, canDeletePoint);
            DeletePanelCommand = new RelayCommand<TECPanel>(deletePanelExecute, canDeletePanel);
            DeleteControllerCommand = new RelayCommand<TECController>(deleteControllerExecute, canDeleteController);
            DeleteInterlockCommand = new RelayCommand<TECInterlockConnection>(deleteInterlockExecute, canDeleteInterlock);
            catalogs = scopeManager.Catalogs;
            this.scopeManager = scopeManager;
        }
        
        public event Action<TECObject> Selected;

        public void Refresh(TECScopeManager scopeManager)
        {
            catalogs = scopeManager.Catalogs;
            this.scopeManager = scopeManager;

        }
        public void SetDeleteCommand(Action<TECSystem> deleteExecute, Func<TECSystem, bool> canDelete)
        {
            DeleteSystemCommand = new RelayCommand<TECSystem>(deleteExecute, canDelete);
            RaisePropertyChanged("DeleteSystemCommand");
        }
        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.DragOver(dropInfo, dropCondition);
            
            bool dropCondition(object sourceItem, Type sourceType, Type targetType)
            {
                if(sourceType == typeof(TECSystem) && targetType == typeof(TECTypical))
                {
                    return true;
                }
                else if(sourceType == typeof(TECCost) && targetType == typeof(TECMisc))
                {
                    return true;
                }
                else
                {
                    return UIHelpers.StandardDropCondition(sourceItem, sourceType, targetType);
                }
            }
            
        }
        public void Drop(IDropInfo dropInfo)
        {
            if(dropInfo.VisualTarget == dropInfo.DragInfo.VisualSource)
            {
                UIHelpers.Drop(dropInfo, x => { return x; });
            }
            else
            {
                object dropped = null;
                if(!IsTemplates && dropInfo.Data is IDDCopiable dropable)
                {
                    dropped = dropable.DragDropCopy(scopeManager);
                } else
                {
                    dropped = dropInfo.Data;
                }
                if (dropped is TECEquipment equipment)
                {
                    SelectedVM = new AddEquipmentVM(SelectedSystem, scopeManager);
                    ((AddEquipmentVM)SelectedVM).SetTemplate(equipment);
                }
                else if (dropped is TECSubScope subScope)
                {
                    SelectedVM = new AddSubScopeVM(SelectedEquipment, scopeManager);
                    ((AddSubScopeVM)SelectedVM).SetTemplate(subScope);
                    ((AddSubScopeVM)SelectedVM).SetParentSystem(SelectedSystem, scopeManager);
                }
                else if (dropped is TECPoint point)
                {
                    SelectedVM = new AddPointVM(SelectedSubScope, scopeManager);
                    ((AddPointVM)SelectedVM).SetTemplate(point);
                }
                else if (dropped is IEndDevice)
                {
                    UIHelpers.StandardDrop(dropInfo, scopeManager);
                }
                else if (dropped is TECMisc || dropped is TECCost)
                {
                    TECMisc misc = dropped as TECMisc;
                    SelectedVM = new AddMiscVM(SelectedSystem, scopeManager);
                    TECMisc newMisc = misc != null ? new TECMisc(misc) : new TECMisc(dropped as TECCost);
                    ((AddMiscVM)SelectedVM).SetTemplate(misc);
                }
                else if (dropped is TECSystem system)
                {
                    SelectedVM = new AddSystemVM(scopeManager);
                    ((AddSystemVM)SelectedVM).SetTemplate(system);
                }
            }
        }

        protected void NotifySelected(TECObject item)
        {
            Selected?.Invoke(item);
        }

        private void SystemSelected(TECSystem value)
        {
            if (value != null)
            {
                MiscVM = new MiscCostsVM(value);
                ControllersPanelsVM = new ControllersPanelsVM(value, scopeManager);
                ValveVM = new ValveSelectionVM(value, scopeManager.Catalogs.Valves);
                ConnectionsVM = new ConnectionsVM(value, new ChangeWatcher(value), catalogs, locations: (scopeManager as TECBid)?.Locations, filterPredicate: connectionFilter);
                if(value is TECTypical typical)
                {
                    UpdateInstanceConnectionsCommand = new RelayCommand(typical.UpdateInstanceConnections, typical.CanUpdateInstanceConnections);
                }
                else
                {
                    UpdateInstanceConnectionsCommand = null;
                }
                RaisePropertyChanged("UpdateInstanceConnectionsCommand");
                bool connectionFilter(ITECObject obj)
                {
                    if(obj is ITypicalable typ && typ.IsTypical == value.IsTypical)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        private void raiseSelected(TECObject item)
        {
            Selected?.Invoke(item);
        }
        private void backExecute(object obj)
        {
            if(obj is TECEquipment)
            {
                SelectedEquipment = null;
            } else if (obj is TECSubScope)
            {
                SelectedSubScope = null;
            }
        }

        private void addSystemExecute()
        {
            SelectedVM = new AddSystemVM(scopeManager);
        }
        private bool canAddSystem()
        {
            return true;
        }

        private void addEquipmentExecute(TECSystem system)
        {
            SelectedVM = new AddEquipmentVM(system, scopeManager);
        }
        private bool canAddEquipment(TECSystem system)
        {
            return system != null;
        }

        private void addSubScopeExecute(TECEquipment equipment)
        {
            SelectedVM = new AddSubScopeVM(equipment,scopeManager);
            ((AddSubScopeVM)SelectedVM).SetParentSystem(SelectedSystem, scopeManager);
        }
        private bool canAddSubScope(TECEquipment equipment)
        {
            return equipment != null;
        }

        private void addPointExecute(TECSubScope subScope)
        {
            SelectedVM = new AddPointVM(subScope, scopeManager);
        }
        private bool canAddPoint(TECSubScope subScope)
        {
            return subScope != null;
        }

        private void addControllerExecute(TECSystem system)
        {
            SelectedVM = new AddControllerVM(system, catalogs.ControllerTypes, scopeManager);
        }
        private bool canAddController(TECSystem system)
        {
            return catalogs.ControllerTypes.Count > 0;
        }

        private void addPanelExecute(TECSystem system)
        {
            SelectedVM = new AddPanelVM(system, catalogs.PanelTypes, scopeManager);
        }
        private bool canAddPanel(TECSystem system)
        {
            return catalogs.PanelTypes.Count > 0;
        }

        private void addMiscExecute(TECSystem system)
        {
            SelectedVM = new AddMiscVM(system, scopeManager);
        }
        private bool canAddMisc(TECSystem system)
        {
            return true;
        }

        private void addInterlockExecute(IInterlockable interlockable)
        {
            SelectedVM = new AddInterlockVM(interlockable, scopeManager);
        }
        private bool canAddInterlock(IInterlockable arg)
        {
            return true;
        }

        private void addBranchExecute(TECScopeBranch obj)
        {
            if(obj == null)
            {
                SelectedSystem.ScopeBranches.Add(new TECScopeBranch());
            } else
            {
                obj.Branches.Add(new TECScopeBranch());
            }
        }
        
        private void deleteSystemExecute(TECSystem obj)
        {
            if(scopeManager is TECBid bid)
            {
                bid.Systems.Remove(obj as TECTypical);
            } else if(scopeManager is TECTemplates templates)
            {
                templates.Templates.SystemTemplates.Remove(obj);
            }
        }
        private bool canDeleteSystem(TECSystem arg)
        {
            return scopeManager != null;
        }

        private void deleteEquipmentExecute(TECEquipment obj)
        {
            SelectedSystem.Equipment.Remove(obj);
        }
        private bool canDeleteEquipment(TECEquipment arg)
        {
            return true;
        }

        private void deleteSubScopeExecute(TECSubScope obj)
        {
            SelectedEquipment.SubScope.Remove(obj);
        }
        private bool canDeleteSubScope(TECSubScope arg)
        {
            return true;
        }

        private void deleteDeviceExecute(IEndDevice obj)
        {
            SelectedSubScope.Devices.Remove(obj);
        }
        private bool canDeleteDevice(IEndDevice arg)
        {
            return true;
        }

        private void deletePointExecute(TECPoint obj)
        {
            SelectedSubScope.Points.Remove(obj);
        }
        private bool canDeletePoint(TECPoint arg)
        {
            return true;
        }

        private void deletePanelExecute(TECPanel obj)
        {
            SelectedSystem.Panels.Remove(obj);
        }
        private bool canDeletePanel(TECPanel arg)
        {
            return true;
        }

        private void deleteControllerExecute(TECController obj)
        {
            obj.DisconnectAll();
            SelectedSystem.RemoveController(obj);
        }
        private bool canDeleteController(TECController arg)
        {
            return true;
        }

        private void deleteInterlockExecute(TECInterlockConnection obj)
        {
            SelectedSubScope.Interlocks.Remove(obj);
        }

        private bool canDeleteInterlock(TECInterlockConnection arg)
        {
            return true;
        }
    }
}
