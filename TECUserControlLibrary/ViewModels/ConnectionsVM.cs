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
using System.Diagnostics;
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
        private bool _defaultPlenum = true;
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
                ConnectableFilter.RaiseFilter();
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

        public DisconnectDropTarget DisconnectDropTarget { get; } = new DisconnectDropTarget();

        public InterlocksVM InterlocksVM { get; }

        public RelayCommand<IControllerConnection> DeleteCommand { get; private set; }

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

            ConnectableFilter.basePredicate = connectable =>
            {
                if (!connectable.IsConnected() && connectable.AvailableProtocols.Count == 0)
                {
                    return false;
                }
                if(connectable == SelectedController)
                {
                    return false;
                }
                return true;
            };

            this.InterlocksVM = new InterlocksVM(root, watcher, catalogs, filterPredicate);

            this.root = root;
            this.Catalogs = catalogs;
            this.Locations = locations != null ? new ObservableCollection<TECLocation>(locations) : new ObservableCollection<TECLocation>();
            if(this.Catalogs.ConduitTypes.Count > 0)
            {
                this.DefaultConduitType = this.Catalogs.ConduitTypes[0];
            }

            watcher.Changed += parentChanged;
            new DirectRelationshipChangedFilter(watcher).DirectRelationshipChanged += parentScopeChanged;

            this.rootConnectableGroup = new FilteredConnectablesGroup("root", this.ConnectableFilter);
            this.rootControllerGroup = new FilteredConnectablesGroup("root", this.ControllerFilter);

            repopulateGroups(null, root, addConnectable);

            SelectProtocolCommand = new RelayCommand(selectProtocolExecute, selectProtocolCanExecute);
            CancelProtocolSelectionCommand = new RelayCommand(cancelProtocolSelectionExecute);

            ConnectionDropHandler = new NetworkConnectionDropTarget(this);

            this.ControllerFilter.FilterChanged += () => 
            { if (SelectedControllerGroup?.PassesFilter == false) SelectedControllerGroup = null; };
            this.ConnectableFilter.FilterChanged += () => 
            { if (SelectedConnectableGroup?.PassesFilter == false) SelectedConnectableGroup = null; };

            DeleteCommand = new RelayCommand<IControllerConnection>(deleteConnectionExecute, canDeleteConnection);
        }

        private void deleteConnectionExecute(IControllerConnection obj)
        {
            if(obj is TECHardwiredConnection hardConn)
            {
                SelectedController.Disconnect(hardConn.Child);
            }
            else if (obj is TECNetworkConnection netConn)
            {
                SelectedController.RemoveNetworkConnection(netConn);
            }
        }

        private bool canDeleteConnection(IControllerConnection arg)
        {
            return arg != null && SelectedController != null;
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
        
        private void repopulateGroups(ITECObject parent, ITECObject item, Action<FilteredConnectablesGroup, 
            IConnectable, IEnumerable<ITECObject>> action, List<ITECObject> parentPath = null)
        {
            parentPath = parentPath ?? new List<ITECObject>();
            if(parent != null) { parentPath.Add(parent); }
            if(item is IConnectable connectable)
            {
                parentPath.Add(item);
                execute(this.rootConnectableGroup);
                if (connectable is TECController)
                {
                    execute(this.rootControllerGroup);
                }

                void execute(FilteredConnectablesGroup relevantRoot)
                {
                    var closestRoot = relevantRoot;
                    var thisPath = new List<ITECObject>();
                    thisPath.Add(connectable);
                    var start = item;
                    var toRemove = new List<ITECObject>();
                    for (int x = parentPath.Count - 2; x >= 0; x--)
                    {
                        if (parentPath.Count == 0 || x < 0)
                        {
                            logger.Error("Connectable path had some issue getting the path to {0} from {1}", item, parent);
                            return;
                        }
                        if (!thisPath.Contains(parentPath[x]) && (parentPath[x] as IRelatable).GetDirectChildren().Contains(start))
                        {
                            thisPath.Insert(0, parentPath[x]);
                            start = parentPath[x];
                            if (relevantRoot.GetGroup(parentPath[x] as ITECScope) != null && closestRoot == relevantRoot)
                            {
                                closestRoot = relevantRoot.GetGroup(parentPath[x] as ITECScope);
                            }
                        }
                    }
                    action(closestRoot, connectable, thisPath);
                }
            }
            else if (item is IRelatable relatable)
            {
                foreach (ITECObject child in relatable.GetDirectChildren().Where(filterPredicate))
                {
                    repopulateGroups(item, child, action, parentPath);
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
            else if (obj.Value is IControllerConnection)
            {
                ControllerFilter.RaiseFilter();
                ConnectableFilter.RaiseFilter();
            }
        }
        private void parentScopeChanged(TECChangedEventArgs obj)
        {
            if (obj.Value is ITECObject tecObj)
            {
                if (obj.Change == Change.Add)
                {
                    repopulateGroups(obj.Sender, tecObj, addConnectable);
                }
                else if (obj.Change == Change.Remove)
                {
                    repopulateGroups(obj.Sender, tecObj, removeConnectable);
                }
            }
        }
        
        private void addConnectable(FilteredConnectablesGroup rootGroup, IConnectable connectable, IEnumerable<ITECObject> parentPath)
        {
            if (!filterPredicate(connectable)) return;
            IRelatable rootScope = rootGroup.Scope as IRelatable ?? this.root;
            List<ITECObject> path = new List<ITECObject>(parentPath);
            if (rootScope != parentPath.First())
            {
                path = rootScope.GetObjectPath(parentPath.First());
                path.Remove(parentPath.First());
                path.AddRange(parentPath);
            }

            if (path.Count == 0)
            {
                logger.Error("New connectable doesn't exist in root object.");
                return;
            }
            
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
        private void removeConnectable(FilteredConnectablesGroup rootGroup, IConnectable connectable, IEnumerable<ITECObject> parentPath)
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
            else if (dropInfo.Data is IEnumerable dropList && dropList.GetItemType() == typeof(FilteredConnectablesGroup))
            {
                foreach(FilteredConnectablesGroup item in dropList)
                {
                    connectables.AddRange(ConnectionHelper.GetConnectables(item.Scope, filterPredicate)); 
                }
            }
            if (ConnectionHelper.CanConnectToController(connectables, SelectedController))
            {
                DragDropHelpers.SetDragAdorners(dropInfo);
            }
        }
        public void Drop(IDropInfo dropInfo)
        {
            if(dropInfo.Data is FilteredConnectablesGroup group && group.Scope is IConnectable)
            {
                IConnectable connectable = group.Scope as IConnectable;
                var compatibleProtocols = SelectedController.CompatibleProtocols(connectable).Distinct().ToList();
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
                    if (dropList.GetItemType() == typeof(FilteredConnectablesGroup))
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

                var connectionProperties = new ConnectionProperties
                {
                    Length = this.DefaultWireLength,
                    ConduitLength = this.DefaultConduitLength,
                    ConduitType = this.DefaultConduitType,
                    IsPlenum = this.DefaultPlenum

                };

                var connections = ConnectionHelper.ConnectToController(connectables.Where(y => y != SelectedController), SelectedController, connectionProperties);
                
            }
        }
    }

    public class DisconnectDropTarget : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IControllerConnection connection)
            {
                DragDropHelpers.SetDragAdorners(dropInfo);
            }
            else if (dropInfo.Data is IEnumerable dropList && (dropList.GetItemType() == typeof(TECConnection)
                || dropList.GetItemType().IsImplementationOf(typeof(IControllerConnection))))
            {
                DragDropHelpers.SetDragAdorners(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IControllerConnection connection)
            {
                removeConnection(connection);   
            }
            else if (dropInfo.Data is IEnumerable dropList && (dropList.GetItemType() == typeof(TECConnection)
                || dropList.GetItemType().IsImplementationOf(typeof(IControllerConnection))))
            {
                foreach (IControllerConnection dropConn in dropList)
                {
                    removeConnection(dropConn);
                }
            }

            void removeConnection(IControllerConnection item)
            {
                if(item is TECHardwiredConnection hardConnect)
                {
                    hardConnect.ParentController.Disconnect(hardConnect.Child);
                }
                else if (item is TECNetworkConnection netConnect)
                {
                    netConnect.ParentController.RemoveNetworkConnection(netConnect);
                }
            }
        }
    }


    public class ConnectableFilter: ViewModelBase
    {
        private bool _omitConnected = true;
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

        public Func<IConnectable, bool> basePredicate;

        public event Action FilterChanged;

        public bool PassesFilter(IConnectable connectable)
        {
            //No Connection Methods
            if (basePredicate != null && !basePredicate(connectable))
            {
                return false;
            }

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
        public void RaiseFilter()
        {
            FilterChanged?.Invoke();
        }
    }

}
