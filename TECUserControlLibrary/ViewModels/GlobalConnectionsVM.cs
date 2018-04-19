using EstimatingLibrary;
using EstimatingLibrary.Utilities;
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
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class GlobalConnectionsVM : ViewModelBase, IDropTarget
    {
        private readonly TECBid bid;
        private Dictionary<TECHardwriredConnection, SubScopeConnectionItem> subScopeConnectionDictionary;
        private TECElectricalMaterial noneConduit;

        private TECController _selectedController;
        private TECSystem _selectedSystem;
        private TECEquipment _selectedEquipment;
        private SubScopeConnectionItem _selectedConnectedSubScope;
        private TECSubScope _selectedUnconnectedSubScope;

        private Double _defaultLength = 30;
        private Double _defaultConduitLength = 20;
        private bool _defaultPlenum = false;
        private TECElectricalMaterial _defaultConduitType;

        public ObservableCollection<TECController> GlobalControllers { get; }
        public ObservableCollection<SubScopeConnectionItem> ConnectedSubScope { get; }
        public ObservableCollection<TECSystem> UnconnectedSystems { get; }
        public ObservableCollection<TECEquipment> UnconnectedEquipment { get; }
        public ObservableCollection<TECSubScope> UnconnectedSubScope { get; }

        public ObservableCollection<TECElectricalMaterial> ConduitTypes { get; }

        public TECController SelectedController
        {
            get
            {
                return _selectedController;
            }
            set
            {
                _selectedController = value;
                RaisePropertyChanged("SelectedController");
                handleSelectedControllerChanged();
                Selected?.Invoke(SelectedController);
            }
        }
        public TECSystem SelectedSystem
        {
            get
            {
                return _selectedSystem;
            }
            set
            {
                _selectedSystem = value;
                RaisePropertyChanged("SelectedSystem");
                handleSelectedSystemChanged();
                Selected?.Invoke(SelectedSystem);
            }
        }
        public TECEquipment SelectedEquipment
        {
            get
            {
                return _selectedEquipment;
            }
            set
            {
                _selectedEquipment = value;
                RaisePropertyChanged("SelectedEquipment");
                handleSelectedEquipmentChanged();
                Selected?.Invoke(SelectedEquipment);
            }
        }
        public SubScopeConnectionItem SelectedConnectedSubScope
        {
            get
            {
                return _selectedConnectedSubScope;
            }
            set
            {
                _selectedConnectedSubScope = value;
                RaisePropertyChanged("SelectedConnectedSubScope");
                Selected?.Invoke(SelectedConnectedSubScope?.SubScope.Connection);
            }
        }
        public TECSubScope SelectedUnconnectedSubScope
        {
            get
            {
                return _selectedUnconnectedSubScope;
            }
            set
            {
                _selectedUnconnectedSubScope = value;
                RaisePropertyChanged("SelectedUnconnectedSubScope");
                Selected?.Invoke(SelectedUnconnectedSubScope);
            }
        }

        public Double DefaultLength
        {
            get { return _defaultLength; }
            set
            {
                _defaultLength = value;
                RaisePropertyChanged("DefaultLength");
            }
        }
        public Double DefaultConduitLength
        {
            get { return _defaultConduitLength; }
            set
            {
                _defaultConduitLength = value;
                RaisePropertyChanged("DefaultConduitLength");
            }
        }
        public bool DefaultPlenum
        {
            get { return _defaultPlenum; }
            set
            {
                _defaultPlenum = value;
                RaisePropertyChanged("DefaultPlenum");
            }
        }
        public TECElectricalMaterial DefaultConduitType
        {
            get { return _defaultConduitType; }
            set
            {
                _defaultConduitType = value;
                RaisePropertyChanged("DefaultConduitType");
            }
        }

        public RelayCommand<SubScopeConnectionItem> DisconnectSubScopeCommand { get; }

        public event Action<TECObject> Selected;

        public GlobalConnectionsVM(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            subScopeConnectionDictionary = new Dictionary<TECHardwriredConnection, SubScopeConnectionItem>();
            noneConduit = new TECElectricalMaterial();
            noneConduit.Name = "None";

            GlobalControllers = new ObservableCollection<TECController>();
            ConnectedSubScope = new ObservableCollection<SubScopeConnectionItem>();
            UnconnectedSystems = new ObservableCollection<TECSystem>();
            UnconnectedEquipment = new ObservableCollection<TECEquipment>();
            UnconnectedSubScope = new ObservableCollection<TECSubScope>();

            ConduitTypes = new ObservableCollection<TECElectricalMaterial>(bid.Catalogs.ConduitTypes);
            ConduitTypes.Insert(0, noneConduit);

            DisconnectSubScopeCommand = new RelayCommand<SubScopeConnectionItem>(disconnectSubScopeExecute);

            filterSystems(bid);

            foreach(TECController controller in bid.Controllers)
            {
                GlobalControllers.Add(controller);
            }

            watcher.InstanceChanged += handleInstanceChanged;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            UIHelpers.DragOver(dropInfo, (data, sourceType, targetType) =>
            {
                bool allow = false;
                if (data is TECSubScope ss)
                {
                    allow = checkCompatible(ss);
                }
                else if (data is TECEquipment equipment)
                {
                    allow = true;
                    foreach (TECSubScope sub in equipment.SubScope.
                        Where(item => item.Connection == null && item.ParentConnection == null))
                    {
                        if (!checkCompatible(sub))
                        {
                            allow = false;
                            break;
                        }
                    }
                }
                else if (data is TECSystem system)
                {
                    allow = true;
                    foreach (TECSubScope sub in system.GetAllSubScope().
                        Where(item => item.Connection == null && item.ParentConnection == null))
                    {
                        if (!checkCompatible(sub))
                        {
                            allow = false;
                            break;
                        }
                    }
                }
                return allow;
            },()=> { });

            bool checkCompatible(TECSubScope subScope)
            {
                if (dropInfo.TargetCollection == ConnectedSubScope)
                {
                    if (subScope.IsNetwork || SelectedController.CanConnectSubScope(subScope))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            UIHelpers.Drop(dropInfo, data => {
                if (data is TECSubScope ss && !ss.IsNetwork)
                {
                    connectSubScope(ss);
                }
                else if (data is TECEquipment equip)
                {
                    foreach (TECSubScope item in equip.SubScope.
                        Where(thing => thing.ParentConnection == null && thing.Connection == null && !thing.IsNetwork))
                    {
                        connectSubScope(item);
                    }
                }
                else if (data is TECSystem system)
                {
                    foreach (TECSubScope item in system.GetAllSubScope().
                        Where(thing => thing.ParentConnection == null && thing.Connection == null && !thing.IsNetwork))
                    {
                        connectSubScope(item);
                    }
                }
                return null;
            }, false);
            
            filterSystems(bid);
            void connectSubScope(TECSubScope subScope)
            {
                if (dropInfo.TargetCollection == ConnectedSubScope)
                {
                    if (SelectedController.CanConnectSubScope(subScope))
                    {
                        var connection = SelectedController.AddSubScopeConnection(subScope);
                        setConnectionDefaults(connection);
                    }
                }
            }
        }

        private void filterSystems(TECBid bid)
        {
            var previousSystem = SelectedSystem;
            var previousEquipment = SelectedEquipment;

            UnconnectedSystems.ObservablyClear();
            foreach (TECTypical typ in bid.Systems)
            {
                foreach (TECSystem sys in typ.Instances)
                {
                    if (systemHasUnconnected(sys))
                    {
                        UnconnectedSystems.Add(sys);
                    }
                }
            }
            SelectedSystem = previousSystem;
            SelectedEquipment = previousEquipment;
        }
        
        private void handleSelectedControllerChanged()
        {
            clearConnectedSubScope();

            if (SelectedController != null)
            {
                foreach (TECHardwriredConnection ssConnect in SelectedController.ChildrenConnections.Where(
                (connection) => connection is TECHardwriredConnection))
                {
                    TECEquipment parent = ssConnect.SubScope.FindParentEquipment(bid);
                    addSubScopeConnectionItem(ssConnect);
                }
            }
        }
        private void handleSelectedSystemChanged()
        {
            UnconnectedEquipment.ObservablyClear();
            UnconnectedSubScope.ObservablyClear();
            SelectedEquipment = null;

            if (SelectedSystem != null)
            {
                foreach (TECEquipment equip in SelectedSystem.Equipment)
                {
                    if (equipHasUnconnected(equip))
                    {
                        UnconnectedEquipment.Add(equip);
                    }
                }
            }
        }
        private void handleSelectedEquipmentChanged()
        {
            UnconnectedSubScope.ObservablyClear();

            if (SelectedEquipment != null)
            {
                foreach (TECSubScope ss in SelectedEquipment.SubScope)
                {
                    if (ssIsUnconnected(ss))
                    {
                        UnconnectedSubScope.Add(ss);
                    }
                }
            }
        }

        private bool systemHasUnconnected(TECSystem sys)
        {
            foreach (TECEquipment equip in sys.Equipment)
            {
                if (equipHasUnconnected(equip))
                {
                    return true;
                }
            }
            return false;
        }
        private bool equipHasUnconnected(TECEquipment equip)
        {
            foreach (TECSubScope ss in equip.SubScope)
            {
                if (ssIsUnconnected(ss))
                {
                    return true;
                }
            }
            return false;
        }
        private bool ssIsUnconnected(TECSubScope ss)
        {
            //Only want unconnected non-network subscope.
            bool ssUnconnected = (!ss.IsNetwork && ss.Connection == null);
            return ssUnconnected;
        }

        private void handleInstanceChanged(TECChangedEventArgs args)
        {
            Change change = args.Change;
            object obj = args.Value;
            TECObject sender = args.Sender;

            bool refresh = false;

            if (change == Change.Add)
            {
                if (obj is TECController controller && sender is TECBid)
                {
                    GlobalControllers.Add(controller);
                    refresh = true;
                }
                else if (obj is TECSystem sys)
                {
                    if (systemHasUnconnected(sys))
                    {
                        UnconnectedSystems.Add(sys);
                        refresh = true;
                    }
                }
                else if (obj is TECEquipment equip && sender == SelectedSystem)
                {
                    if (equipHasUnconnected(equip))
                    {
                        UnconnectedEquipment.Add(equip);
                        refresh = true;
                    }
                }
                else if (obj is TECSubScope ss && sender == SelectedEquipment)
                {
                    if (ssIsUnconnected(ss))
                    {
                        UnconnectedSubScope.Add(ss);
                        refresh = true;
                    }
                }
                else if (obj is TECHardwriredConnection ssConnect && sender == SelectedController)
                {
                    if (UnconnectedSubScope.Contains(ssConnect.SubScope))
                    {
                        UnconnectedSubScope.Remove(ssConnect.SubScope);
                    }
                    TECEquipment parent = ssConnect.SubScope.FindParentEquipment(bid);
                    addSubScopeConnectionItem(ssConnect);
                    refresh = true;
                }
            }
            else if (change == Change.Remove)
            {
                if (obj is TECController controller && sender is TECBid)
                {
                    if (controller == SelectedController)
                    {
                        SelectedController = null;
                    }
                    GlobalControllers.Remove(controller);
                    refresh = true;
                }
                else if (obj is TECSystem sys && UnconnectedSystems.Contains(sys))
                {
                    if (sys == SelectedSystem)
                    {
                        SelectedSystem = null;
                    }
                }
                else if (obj is TECEquipment equip && UnconnectedEquipment.Contains(equip))
                {
                    if (equip == SelectedEquipment)
                    {
                        SelectedEquipment = null;
                    }
                }
                else if (obj is TECSubScope ss && UnconnectedSubScope.Contains(ss))
                {
                    if (ss == SelectedUnconnectedSubScope)
                    {
                        SelectedUnconnectedSubScope = null;
                    }
                }
                else if (obj is TECHardwriredConnection ssConnect)
                {
                    if (subScopeConnectionDictionary.ContainsKey(ssConnect) &&
                        ConnectedSubScope.Contains(subScopeConnectionDictionary[ssConnect]))
                    {
                        removeSubScopeConnectionItem(ssConnect);
                        refresh = true;
                    }
                }
            }

            if (refresh) filterSystems(bid);
        }

        private void disconnectSubScopeExecute(SubScopeConnectionItem ssConnect)
        {
            ssConnect.SubScope.Connection.ParentController.RemoveSubScope(ssConnect.SubScope);
        }

        private void addSubScopeConnectionItem(TECHardwriredConnection ssConnect)
        {
            SubScopeConnectionItem newItem = new SubScopeConnectionItem(ssConnect.SubScope, noneConduit, 
                ssConnect.SubScope.FindParentEquipment(bid).FindParentSystem(bid), 
                ssConnect.SubScope.FindParentEquipment(bid));
            if(!subScopeConnectionDictionary.ContainsKey(ssConnect))
            {
                subScopeConnectionDictionary.Add(ssConnect, newItem);
                ConnectedSubScope.Add(newItem);
            }

        }
        private void removeSubScopeConnectionItem(TECHardwriredConnection ssConnect)
        {
            ConnectedSubScope.Remove(subScopeConnectionDictionary[ssConnect]);
            subScopeConnectionDictionary.Remove(ssConnect);
        }

        private void clearConnectedSubScope()
        {
            ConnectedSubScope.ObservablyClear();
            subScopeConnectionDictionary.Clear();
        }
        private void setConnectionDefaults(TECConnection connection)
        {
            connection.Length = DefaultLength;
            connection.ConduitType = DefaultConduitType;
            connection.ConduitLength = DefaultConduitLength;
            connection.IsPlenum = DefaultPlenum;
        }

    }
}
