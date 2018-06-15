using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingLibrary.Utilities.WatcherFilters;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.Utilities.DropTargets;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectionsVM : ViewModelBase, IDropTarget, NetworkConnectionDropTargetDelegate
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IRelatable root;
        private readonly FilteredConnectablesGroup rootConnectableGroup;
        private readonly FilteredConnectablesGroup rootControllerGroup;
        private readonly Func<ITECObject, bool> filterPredicate;

        private FilteredConnectablesGroup _selectedControllerGroup;
        private FilteredConnectablesGroup _selectedConnectableGroup;
        private IControllerConnection _selectedConnection;
        private Double _defaultWireLength = 50.0;
        private Double _defaultConduitLength = 30.0;
        private TECElectricalMaterial _defaultConduitType;
        private bool _defaultPlenum = false;
        private bool _selectionNeeded = false;
        private IProtocol _selectedProtocol;
        private List<IProtocol> _compatibleProtocols;
        
        public ObservableCollection<FilteredConnectablesGroup> Connectables
        {
            get
            {
                return rootConnectableGroup.ChildrenGroups;
            }
        }
        public ObservableCollection<FilteredConnectablesGroup> Controllers
        {
            get
            {
                return rootControllerGroup.ChildrenGroups;
            }
        }

        public FilteredConnectablesGroup SelectedControllerGroup
        {
            get { return _selectedControllerGroup; }
            set
            {
                _selectedControllerGroup = value;
                RaisePropertyChanged("SelectedControllerGroup");
                RaisePropertyChanged("SelectedController");
                Selected?.Invoke(value?.Scope as TECObject);
            }
        }
        public FilteredConnectablesGroup SelectedConnectableGroup
        {
            get { return _selectedConnectableGroup; }
            set
            {
                _selectedConnectableGroup = value;
                RaisePropertyChanged("SelectedConnectableGroup");
                RaisePropertyChanged("SelectedConnectable");
                Selected?.Invoke(value?.Scope as TECObject);
            }
        }

        public TECController SelectedController
        {
            get { return SelectedControllerGroup?.Scope as TECController; }
        }
        public IConnectable SelectedConnectable
        {
            get { return SelectedConnectableGroup?.Scope as IConnectable; }
        }
        public IControllerConnection SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                RaisePropertyChanged("SelectedConnection");
                Selected?.Invoke(value as TECObject);
            }
        }

        public Double DefaultWireLength
        {
            get { return _defaultWireLength; }
            set
            {
                _defaultWireLength = value;
                RaisePropertyChanged("DefaultWireLength");
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
        public TECElectricalMaterial DefaultConduitType
        {
            get { return _defaultConduitType; }
            set
            {
                _defaultConduitType = value;
                RaisePropertyChanged("DefaultConduitType");
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
        public TECCatalogs Catalogs { get; }
        public ObservableCollection<TECLocation> Locations { get; }
        public bool SelectionNeeded
        {
            get { return _selectionNeeded; }
            set
            {
                _selectionNeeded = value;
                RaisePropertyChanged("SelectionNeeded");
            }
        }
        public IProtocol SelectedProtocol
        {
            get { return _selectedProtocol; }
            set
            {
                _selectedProtocol = value;
                RaisePropertyChanged("SelectedProtocol");
            }
        }
        
        public List<IProtocol> CompatibleProtocols
        {
            get { return _compatibleProtocols; }
            set
            {
                _compatibleProtocols = value;
                RaisePropertyChanged("CompatibleProtocols");
            }
        }
        
        public RelayCommand SelectProtocolCommand { get; private set; }
        public RelayCommand CancelProtocolSelectionCommand { get; private set; }

        public event Action<TECObject> Selected;

        public ConnectableFilter ControllerFilter { get; } = new ConnectableFilter();
        public ConnectableFilter ConnectableFilter { get; } = new ConnectableFilter();

        public NetworkConnectionDropTarget ConnectionDropHandler { get; }
        TECNetworkConnection NetworkConnectionDropTargetDelegate.SelectedConnection => SelectedConnection as TECNetworkConnection;
        
        public InterlocksVM InterlocksVM { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="watcher"></param>
        /// <param name="includeFilter">Predicate for "where" clause of direct children of root.</param>
        public ConnectionsVM(IRelatable root, ChangeWatcher watcher, TECCatalogs catalogs, IEnumerable<TECLocation> locations = null, 
            Func<ITECObject, bool> filterPredicate = null)
        {
            if (filterPredicate == null)
            {
                filterPredicate = item => true;
            }
            this.filterPredicate = filterPredicate;
            this.InterlocksVM = new InterlocksVM(root, watcher, catalogs, filterPredicate);

            this.root = root;
            this.Catalogs = catalogs;
            this.Locations = new ObservableCollection<TECLocation>(locations);
            if(this.Catalogs.ConduitTypes.Count > 0)
            {
                this.DefaultConduitType = this.Catalogs.ConduitTypes[0];
            }

            watcher.Changed += parentChanged;
            new DirectRelationshipChangedFilter(watcher).DirectRelationshipChanged += parentScopeChanged;

            this.rootConnectableGroup = new FilteredConnectablesGroup("root", this.ConnectableFilter);
            this.rootControllerGroup = new FilteredConnectablesGroup("root", this.ControllerFilter);

            repopulateGroups(root, addConnectable);

            SelectProtocolCommand = new RelayCommand(selectProtocolExecute, selectProtocolCanExecute);
            CancelProtocolSelectionCommand = new RelayCommand(cancelProtocolSelectionExecute);

            ConnectionDropHandler = new NetworkConnectionDropTarget(this);

            this.ControllerFilter.FilterChanged += () => 
            { if (SelectedControllerGroup?.PassesFilter == false) SelectedControllerGroup = null; };
            this.ConnectableFilter.FilterChanged += () => 
            { if (SelectedConnectableGroup?.PassesFilter == false) SelectedConnectableGroup = null; };
        }

        private void cancelProtocolSelectionExecute()
        {
            SelectedProtocol = null;
            SelectionNeeded = false;
        }
        private void selectProtocolExecute()
        {
            SelectedController.Connect(SelectedConnectable, SelectedProtocol);
            cancelProtocolSelectionExecute();
        }
        private bool selectProtocolCanExecute()
        {
            return SelectedProtocol != null && SelectedConnectable != null && SelectedController != null;
        }
        
        private void repopulateGroups(ITECObject item, Action<FilteredConnectablesGroup, IConnectable> action)
        {
            if(item is IConnectable connectable)
            {
                action(this.rootConnectableGroup, connectable);
                if (connectable is TECController)
                {
                    action(this.rootControllerGroup, connectable);
                }
            }
            else if (item is IRelatable relatable)
            {
                foreach (ITECObject child in relatable.GetDirectChildren().Where(filterPredicate))
                {
                    repopulateGroups(child, action);
                }
            }
        }

        private static bool containsConnectable(FilteredConnectablesGroup group)
        {
            if (group.Scope is IConnectable)
            {
                return true;
            }
            
            foreach(FilteredConnectablesGroup childGroup in group.ChildrenGroups)
            {
                if (containsConnectable(childGroup))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void parentChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is TECLocation location)
            {
                if (obj.Change == Change.Add)
                {
                    this.Locations.Add(location);
                }
                else if (obj.Change == Change.Remove)
                {
                    this.Locations.Remove(location);
                }
            }
        }
        private void parentScopeChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is ITECObject tecObj)
            {
                if (obj.Change == Change.Add)
                {
                    repopulateGroups(tecObj, addConnectable);
                }
                else if (obj.Change == Change.Remove)
                {
                    repopulateGroups(tecObj, removeConnectable);
                }
            }
        }
        
        private void addConnectable(FilteredConnectablesGroup rootGroup, IConnectable connectable)
        {
            if (!filterPredicate(connectable)) return;
            bool isDescendant = root.IsDirectDescendant(connectable);
            if (!root.IsDirectDescendant(connectable))
            {
                logger.Error("New connectable doesn't exist in root object.");
                return;
            }

            List<ITECObject> path = this.root.GetObjectPath(connectable);

            FilteredConnectablesGroup lastGroup = rootGroup;
            int lastIndex = 0;

            for(int i = path.Count - 1; i > 0; i--)
            {
                if (path[i] is ITECScope scope)
                {
                    FilteredConnectablesGroup group = rootGroup.GetGroup(scope);

                    if (group != null)
                    {
                        lastGroup = group;
                        lastIndex = i;
                        break;
                    }
                }
                else
                {
                    logger.Error("Object in path to connectable isn't ITECScope, cannot build group hierarchy.");
                    return;
                }
            }
            
            FilteredConnectablesGroup currentGroup = lastGroup;
            for(int i = lastIndex + 1; i < path.Count; i++)
            {
                if (path[i] is ITECScope scope)
                {
                    currentGroup = currentGroup.Add(scope);
                }
                else
                {
                    logger.Error("Object in path to connectable isn't ITECScope, cannot build group hierarchy.");
                    return;
                }
            }
        }
        private void removeConnectable(FilteredConnectablesGroup rootGroup, IConnectable connectable)
        {
            if (!filterPredicate(connectable)) return;
            List<FilteredConnectablesGroup> path = rootGroup.GetPath(connectable);

            path[path.Count - 2].Remove(path.Last());

            FilteredConnectablesGroup parentGroup = rootGroup;
            for(int i = 1; i < path.Count; i++)
            {
                if (!containsConnectable(path[i]))
                {
                    parentGroup.Remove(path[i]);
                    return;
                }
                else
                {
                    parentGroup = path[i];
                }
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if(SelectedController == null)
            {
                return;
            }
            List<IConnectable> connectables = new List<IConnectable>();
            if (dropInfo.Data is FilteredConnectablesGroup group)
            {
                if(group.Scope == SelectedController)
                {
                    return;
                }
                connectables = ConnectionHelper.GetConnectables(group.Scope, filterPredicate);
                
            }
            else if (dropInfo.Data is IEnumerable dropList && UIHelpers.GetItemType(dropList) == typeof(FilteredConnectablesGroup))
            {
                foreach(FilteredConnectablesGroup item in dropList)
                {
                    connectables.AddRange(ConnectionHelper.GetConnectables(item.Scope, filterPredicate)); 
                }
            }
            if (ConnectionHelper.CanConnectToController(connectables, SelectedController))
            {
                UIHelpers.SetDragAdorners(dropInfo);
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            if(dropInfo.Data is FilteredConnectablesGroup group && group.Scope is IConnectable)
            {
                IConnectable connectable = group.Scope as IConnectable;
                var compatibleProtocols = SelectedController.CompatibleProtocols(connectable);
                if (compatibleProtocols.Count == 1)
                {
                    var connection = SelectedController.Connect(connectable, compatibleProtocols.First());
                    connection.Length = this.DefaultWireLength;
                    connection.ConduitType = this.DefaultConduitType;
                    connection.ConduitLength = this.DefaultConduitLength;
                    connection.IsPlenum = this.DefaultPlenum;
                }
                else
                {
                    SelectionNeeded = true;
                    CompatibleProtocols = compatibleProtocols;
                }
            }
            else
            {
                List<IConnectable> connectables = new List<IConnectable>();
                if (dropInfo.Data is IList dropList)
                {
                    if (UIHelpers.GetItemType(dropList) == typeof(FilteredConnectablesGroup))
                    {
                        foreach (FilteredConnectablesGroup item in dropList)
                        {
                            connectables.AddRange(ConnectionHelper.GetConnectables(item.Scope, filterPredicate));
                        }

                    }
                }
                else if (dropInfo.Data is FilteredConnectablesGroup parentGroup)
                {
                    connectables = ConnectionHelper.GetConnectables(parentGroup.Scope, filterPredicate);
                }
                else
                {
                    return;
                }

                var connections = ConnectionHelper.ConnectToController(connectables, SelectedController);
                foreach (IControllerConnection connection in connections)
                {
                    connection.Length += this.DefaultWireLength;
                    connection.ConduitLength += this.DefaultConduitLength;
                }
                foreach (IControllerConnection connection in connections.Distinct())
                {
                    connection.ConduitType = this.DefaultConduitType;
                    connection.IsPlenum = this.DefaultPlenum;
                }
            }
            
        }
    }

    public class ConnectableFilter: ViewModelBase
    {
        private bool _omitConnected = false;
        private TECProtocol _filterProtocol;
        private TECLocation _filterLocation;

        public bool OmitConnected
        {
            get { return _omitConnected; }
            set
            {
                if (_omitConnected != value)
                {
                    _omitConnected = value;
                    RaisePropertyChanged("OmitConnected");
                    FilterChanged?.Invoke();
                }
            }
        }
        public TECProtocol FilterProtocol
        {
            get { return _filterProtocol; }
            set
            {
                if (_filterProtocol != value)
                {
                    _filterProtocol = value;
                    RaisePropertyChanged("FilterProtocol");
                    FilterChanged?.Invoke();
                }
            }
        }
        public TECLocation FilterLocation
        {
            get { return _filterLocation; }
            set
            {
                if (_filterLocation != value)
                {
                    _filterLocation = value;
                    RaisePropertyChanged("FilterLocation");
                    FilterChanged?.Invoke();
                }
            }
        }

        public event Action FilterChanged;

        public bool PassesFilter(IConnectable connectable)
        {
            //Omit connected
            if (OmitConnected && connectable.IsConnected())
            {
                return false;
            }

            //Protocol
            if (FilterProtocol != null && !connectable.AvailableProtocols.Contains(FilterProtocol))
            {
                return false;
            }

            //Location
            if (FilterLocation != null && (connectable as TECLocated)?.Location != FilterLocation)
            {
                return false;
            }
            return true;
        }
    }

    internal static class ConnectionHelper
    {
        internal static bool CanConnectToController(IEnumerable<IConnectable> items, TECController controller)
        {

            var connectables = items
                .Where(x => x.GetParentConnection() == null);
            if(connectables.Count() == 0)
            {
                return false;
            }

            var availableIO = controller.AvailableIO + ExistingNetworkIO(controller);
            foreach(var connectable in connectables.Where(x => x.AvailableProtocols.Count == 1))
            {
                var protocol = connectable.AvailableProtocols.First();

                if (protocol is TECProtocol netProtocol)
                {
                    if (!availableIO.Contains(netProtocol))
                    {
                        return false;
                    }
                }
                else
                {
                    if (availableIO.Contains(connectable.HardwiredIO))
                    {
                        availableIO -= connectable.HardwiredIO;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            foreach(var connectable in connectables.Where(x => x.AvailableProtocols.Count > 1))
            {
                var canConnect = false;
                foreach(var protocol in connectable.AvailableProtocols)
                {
                    if (protocol is TECProtocol networkProtocol)
                    {
                        if (availableIO.Contains(networkProtocol))
                        {
                            canConnect = true;
                            break;
                        }
                    }
                }
                if(canConnect == false)
                {
                    if(connectable.AvailableProtocols.Any(x => x is TECHardwiredProtocol) 
                        && availableIO.Contains(connectable.HardwiredIO))
                    {
                        availableIO -= connectable.HardwiredIO;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        internal static List<IControllerConnection> ConnectToController(IEnumerable<IConnectable> items, TECController controller)
        {
            var connectables = items
                .Where(x => x.GetParentConnection() == null);

            var availableIO = controller.AvailableIO + ExistingNetworkIO(controller);
            List<IControllerConnection> connections = new List<IControllerConnection>();

            foreach (var connectable in connectables)
            {
                var protocols = connectable.AvailableProtocols;
                if(protocols.Count == 1)
                {
                    connections.Add(controller.Connect(connectable, protocols.First(), true));
                }
                else
                {
                    var connected = false;
                    foreach (var protocol in connectable.AvailableProtocols
                        .Where(x => x is TECProtocol networkProtocol && availableIO.Contains(networkProtocol)))
                    {
                        connected = true;
                        connections.Add(controller.Connect(connectable, protocol, true));
                        break;
                    }
                    if (!connected && connectable.AvailableProtocols.Any(x => x is TECHardwiredProtocol)
                            && availableIO.Contains(connectable.HardwiredIO))
                    {
                        connections.Add(controller.Connect(connectable, connectable.AvailableProtocols.First(x => x is TECHardwiredProtocol), true));
                        
                    }
                    else
                    {
                        throw new Exception("Not able to connect connectable to controller");
                    }
                }
            }
            return connections;

        }
        
        internal static List<IConnectable> GetConnectables(ITECObject parent, Func<ITECObject, bool> predicate)
        {
            List<IConnectable> outList = new List<IConnectable>();
            if(parent is IConnectable connectable && predicate(parent))
            {
                outList.Add(connectable);
            }
            if(parent is IRelatable relatable)
            {
                relatable.GetDirectChildren().Where(predicate).ForEach(x => outList.AddRange(GetConnectables(x, predicate)));
            }
            return outList;
        }

        static IOCollection ExistingNetworkIO(TECController controller)
        {
            IOCollection existingNetwork = new IOCollection();
            foreach(TECNetworkConnection connection in controller.ChildrenConnections.Where(x => x is TECNetworkConnection))
            {
                existingNetwork.Add(connection.NetworkProtocol);
            }
            return existingNetwork;
        }
    }
}
