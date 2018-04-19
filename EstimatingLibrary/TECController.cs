using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECController : TECLocated, IDragDropable, ITypicalable, IConnectable
    {
        #region Properties
        //---Stored---
        private TECNetworkConnection _parentConnection;
        private ObservableCollection<TECConnection> _childrenConnections = new ObservableCollection<TECConnection>();
        private TECControllerType _type;
        private bool _isServer;
        private ObservableCollection<TECIOModule> _ioModules = new ObservableCollection<TECIOModule>();
        
        public TECNetworkConnection ParentConnection
        {
            get { return _parentConnection; }
            set
            {
                if (ParentConnection != value)
                {
                    _parentConnection = value;
                    raisePropertyChanged("ParentConnection");
                }
            }
        }
        public ObservableCollection<TECConnection> ChildrenConnections
        {
            get { return _childrenConnections; }
            set
            {
                var old = ChildrenConnections;
                ChildrenConnections.CollectionChanged -= handleChildrenChanged;
                _childrenConnections = value;
                ChildrenConnections.CollectionChanged += handleChildrenChanged;
                notifyCombinedChanged(Change.Edit, "ChildrenConnections", this, value, old);
                raisePropertyChanged("ChildNetworkConnections");
            }
        }
        public TECControllerType Type
        {
            get { return _type; }
            set
            {
                var old = Type;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                notifyCostChanged(value.CostBatch - old.CostBatch);
            }
        }
        public bool IsServer
        {
            get { return _isServer; }
            set
            {
                var old = IsServer;
                _isServer = value;
                notifyCombinedChanged(Change.Edit, "IsServer", this, value, old);
            }
        }
        public ObservableCollection<TECIOModule> IOModules
        {
            get { return _ioModules; }
            set
            {
                var old = IOModules;
                IOModules.CollectionChanged -= handleModulesChanged;
                _ioModules = value;
                IOModules.CollectionChanged += handleModulesChanged;
                notifyCombinedChanged(Change.Edit, "IOModules", this, value, old);
            }
        }

        public bool IsTypical
        {
            get; private set;
        }

        //---Derived---
        public IEnumerable<TECNetworkConnection> ChildNetworkConnections
        {
            get
            {
                return getNetworkConnections();
            }
        }
        public IOCollection IO
        {
            get
            {
                IOCollection allIO = new IOCollection(this.Type.IO);
                List<TECIO> moduleIO = new List<TECIO>();
                this.IOModules.ForEach(x => moduleIO.AddRange(x.IO));
                allIO.Add(moduleIO);
                return allIO;
            }
        }
        public IOCollection UsedIO
        {
            get
            {
                return ChildrenConnections.Aggregate(new IOCollection(), (total, next) => total += next.IO);
            }
        }
        public IOCollection AvailableIO
        {
            get { return IO - UsedIO; }
        }
        public IOCollection AvailableProtocols
        {
            get { return AvailableIO.Protocols; }
        }
        
        #endregion

        #region Constructors
        public TECController(Guid guid, TECControllerType type, bool isTypical) : base(guid)
        {
            _isServer = false;
            IsTypical = isTypical;
            _type = type;
            _childrenConnections = new ObservableCollection<TECConnection>();
            _ioModules = new ObservableCollection<TECIOModule>();
            ChildrenConnections.CollectionChanged += handleChildrenChanged;
            IOModules.CollectionChanged += handleModulesChanged;
        }

        public TECController(TECControllerType type, bool isTypical) : this(Guid.NewGuid(), type, isTypical) { }
        public TECController(TECController controllerSource, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : this(controllerSource.Type, isTypical)
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = controllerSource.Guid; }
            copyPropertiesFromLocated(controllerSource);
            foreach (TECConnection connection in controllerSource.ChildrenConnections)
            {
                if (connection is TECHardwriredConnection)
                {
                    TECHardwriredConnection connectionToAdd = new TECHardwriredConnection(connection as TECHardwriredConnection, this, isTypical, guidDictionary);
                    _childrenConnections.Add(connectionToAdd);
                }
                else if (connection is TECNetworkConnection)
                {

                    TECNetworkConnection connectionToAdd = new TECNetworkConnection(connection as TECNetworkConnection, this, isTypical, guidDictionary);
                    _childrenConnections.Add(connectionToAdd);
                }
            }
            foreach (TECIOModule module in controllerSource.IOModules)
            {
                this.IOModules.Add(module);
            }
        }
        #endregion

        #region Connection Methods
        public bool CanConnect(IConnectable connectable)
        {
            if(connectable.IsNetwork)
            {
                return this.AvailableProtocols.Contains(connectable.AvailableProtocols);
            }
            else
            {
                return this.AvailableIO.Contains(connectable.HardwiredIO);
            }
        }
        public TECConnection Connect(IConnectable connectable)
        {
            if (connectable.IsNetwork)
            {
                foreach(TECIO io in connectable.AvailableProtocols.ToList())
                {
                    if (this.AvailableProtocols.Contains(io))
                    {
                        TECNetworkConnection connection = new TECNetworkConnection(this, io.Protocol, this.IsTypical);
                        connection.AddChild(connectable);
                        addChildConnection(connection);
                        return connection;
                    }   
                }
                throw new Exception("No matching protocols");
            }
            else
            {
                TECHardwriredConnection connection = new TECHardwriredConnection(connectable, this, this.IsTypical);
                addChildConnection(connection);
                return connection;
            }
        }
        
        public bool CanAddNetworkConnection(TECProtocol protocol)
        {
            return (AvailableProtocols.Contains(new TECIO(protocol)));
        }
        public TECNetworkConnection AddNetworkConnection(TECProtocol protocol)
        {
            TECNetworkConnection netConnect = new TECNetworkConnection(this, protocol, this.IsTypical);
            addChildConnection(netConnect);
            return netConnect;
        }
        public void RemoveNetworkConnection(TECNetworkConnection connection)
        {
            if (this.ChildrenConnections.Contains(connection))
            {
                List<IConnectable> children = new List<IConnectable>(connection.Children);
                foreach(IConnectable child in children)
                {
                    connection.RemoveChild(child);
                }
                removeChildConnection(connection);
            }
            else
            {
                throw new InvalidOperationException("Network connection doesn't exist in controller.");
            }
        }
        
        private bool canTakeIO(IOCollection collection)
        {
            bool hasIO = AvailableIO.Contains(collection);
            bool canHasIO = getPotentialIO().Contains(collection);
            return hasIO || canHasIO;
        }
        public bool CanConnectSubScope(IEnumerable<TECSubScope> subScope)
        {
            IOCollection collection = new IOCollection();
            foreach(TECSubScope item in subScope)
            {
                collection += item.IO;
            }
            return canTakeIO(collection);
        }
        public TECConnection AddSubScope(TECSubScope subScope, bool attemptConnectionToExisting)
        {
            if (subScope.IsNetwork)
            {
                return AddSubScopeToNetwork(subScope, attemptConnectionToExisting);
            } else
            {
                return AddSubScopeConnection(subScope);
            }
        }
        public TECHardwriredConnection AddSubScopeConnection(TECSubScope subScope)
        {
            if (CanConnectSubScope(subScope))
            {
                bool connectionIsTypical = (this.IsTypical || subScope.IsTypical);
                if (!subScope.IsNetwork)
                {
                    if (getAvailableIO().Contains(subScope.IO))
                    {
                        return addConnection(subScope, connectionIsTypical);
                    }
                    else if(getPotentialIO().Contains(subScope.IO))
                    {
                        foreach(TECIO io in subScope.IO.ListIO())
                        {
                            for (int i = 0; i < io.Quantity; i++)
                            {
                                bool foundIO = false;
                                if (!AvailableIO.Contains(io.Type))
                                {
                                    foreach (TECIOModule module in Type.IOModules)
                                    {
                                        if (this.IOModules.Count(item => { return item == module; }) <
                                        Type.IOModules.Count(item => { return item == module; }))
                                        {
                                            if (new IOCollection(module.IO).Contains(io.Type))
                                            {
                                                this.IOModules.Add(module);
                                                if (getAvailableIO().Contains(subScope.IO))
                                                {
                                                    return addConnection(subScope, connectionIsTypical);
                                                }
                                                foundIO = true;
                                                break;
                                                
                                            }
                                        }
                                    }
                                    if (foundIO)
                                    {
                                        break;
                                    }
                                }
                                
                            }
                        }
                        
                        foreach (TECIOModule module in Type.IOModules)
                        {
                            if (this.IOModules.Count(item => { return item == module; }) <
                                Type.IOModules.Count(item => { return item == module; }))
                            {
                                this.IOModules.Add(module);
                            }
                        }
                        return addConnection(subScope, connectionIsTypical);
                    }
                    else
                    {
                        throw new InvalidOperationException("Attempted to connect subscope which could not be connected.");

                    }
                }
                else
                {
                    throw new InvalidOperationException("Can't connect network subscope without a known connection.");
                }
            }
            else
            {
                throw new InvalidOperationException("Subscope incompatible.");
            }

            TECHardwriredConnection addConnection(TECSubScope toConnect, bool isTypical)
            {
                TECHardwriredConnection connection = new TECHardwriredConnection(isTypical);
                connection.ParentController = this;
                connection.SubScope = subScope;
                subScope.Connection = connection;
                addChildConnection(connection);

                return connection;
            }
        }
        public TECNetworkConnection AddSubScopeToNetwork(TECSubScope subScope,
            bool attemptConnectionToExisting)
        {
            if (!subScope.IsNetwork)
            {
                throw new Exception("Connectable must be networkcompatible");
            }

            IOType ioType = subScope.AvailableNetworkIO.ListIO()[0].Type;

            bool compatibleConnectionExists = false;
            TECNetworkConnection outConnection = null;
            if (attemptConnectionToExisting)
            {
                foreach (TECNetworkConnection netConnect in this.ChildNetworkConnections)
                {
                    if (netConnect.CanAddINetworkConnectable(subScope))
                    {
                        compatibleConnectionExists = true;
                        netConnect.AddINetworkConnectable(subScope);
                        outConnection = netConnect;
                        break;
                    }
                }
            }
            if (!compatibleConnectionExists)
            {
                TECNetworkConnection newConnection = this.AddNetworkConnection(this.IsTypical, subScope.ConnectionTypes, ioType);
                newConnection.AddINetworkConnectable(subScope);
                outConnection = newConnection;
            }
            return outConnection;
        }

        public void RemoveSubScope(TECSubScope subScope)
        {
            TECHardwriredConnection connectionToRemove = null;
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECHardwriredConnection)
                {
                    var subConnect = connection as TECHardwriredConnection;
                    if (subConnect.SubScope == subScope)
                    {
                        connectionToRemove = subConnect;
                    }
                }
                else if (connection is TECNetworkConnection netConnect)
                {
                    if (netConnect.Children.Contains(subScope))
                    {
                        netConnect.RemoveINetworkConnectable(subScope);
                    }
                }
            }
            if (connectionToRemove != null)
            {
                removeChildConnection(connectionToRemove);
                subScope.Connection = null;
            }
        }
        public void RemoveController(TECController controller)
        {
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECNetworkConnection netConnect)
                {
                    if (netConnect.Children.Contains(controller))
                    {
                        netConnect.RemoveINetworkConnectable(controller);
                    }
                }
            }
        }
        public void RemoveAllConnections()
        {
            RemoveAllChildConnections();
            if(ParentConnection != null)
            {
                ParentConnection.RemoveINetworkConnectable(this);
            }
        }
        public void RemoveAllChildNetworkConnections()
        {
            ObservableCollection<TECConnection> connectionsToRemove = new ObservableCollection<TECConnection>();
            foreach (TECNetworkConnection connection in ChildrenConnections.Where(item => item is TECNetworkConnection))
            {
                connectionsToRemove.Add(connection);
            }
            foreach (TECNetworkConnection connectToRemove in connectionsToRemove)
            {
                if (connectToRemove is TECNetworkConnection netConnect)
                {
                    RemoveNetworkConnection(netConnect);
                }
                else
                {
                    throw new NotImplementedException();
                }
                removeChildConnection(connectToRemove);
            }
        }
        public void RemoveAllChildSubScopeConnections()
        {
            ObservableCollection<TECConnection> connectionsToRemove = new ObservableCollection<TECConnection>();
            foreach (TECHardwriredConnection connection in ChildrenConnections.Where(item => item is TECHardwriredConnection))
            {
                connectionsToRemove.Add(connection);
            }
            foreach (TECHardwriredConnection connectToRemove in connectionsToRemove)
            {
                if (connectToRemove is TECHardwriredConnection)
                {
                    (connectToRemove as TECHardwriredConnection).SubScope.Connection = null;
                    (connectToRemove as TECHardwriredConnection).SubScope = null;
                    connectToRemove.ParentController = null;
                }
                else
                {
                    throw new NotImplementedException();
                }
                removeChildConnection(connectToRemove);
            }
        }
        public void RemoveAllChildConnections()
        {
            RemoveAllChildNetworkConnections();
            RemoveAllChildSubScopeConnections();
        }

        public TECController GetParentController()
        {
            if (ParentConnection == null)
            {
                return null;
            }
            else
            {
                return ParentConnection.ParentController;
            }
        }

        private List<TECNetworkConnection> getNetworkConnections()
        {
            List<TECNetworkConnection> networkConnections = new List<TECNetworkConnection>();
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECNetworkConnection)
                {
                    networkConnections.Add(connection as TECNetworkConnection);
                }
            }
            return networkConnections;
        }
        #endregion

        #region Module Methods
        public bool CanAddModule(TECIOModule module)
        {
            return (this.Type.IOModules.Count(mod => (mod == module)) >
                this.IOModules.Count(mod => (mod == module)));
        }
        public void AddModule(TECIOModule module)
        {
            if (CanAddModule(module))
            {
                IOModules.Add(module);
            } 
            else
            {
                throw new InvalidOperationException("Controller can't accept IOModule.");
            }
        }
        #endregion

        #region Methods
        #region Event Handlers
        private void collectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, propertyName, this, item);
                    if (item is INotifyCostChanged cost && !(item is TECConnection) && !this.IsTypical)
                    {
                        notifyCostChanged(cost.CostBatch);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, propertyName, this, item);
                    if (item is INotifyCostChanged cost && !(item is TECConnection) && !this.IsTypical)
                    {
                        notifyCostChanged(cost.CostBatch * -1);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, propertyName, this, sender, sender);
            }
            if (propertyName == "ChildrenConnections")
            {
                raisePropertyChanged("ChildNetworkConnections");
            }
        }
        #endregion
        public Object DragDropCopy(TECScopeManager scopeManager)
        {
            var outController = new TECController(this, this.IsTypical);
            ModelLinkingHelper.LinkScopeItem(outController, scopeManager);
            return outController;
        }
        public IConnectable Copy(bool isTypical, Dictionary<Guid, Guid> guidDictionary)
        {
            return new TECController(this, isTypical, guidDictionary);
        }
        public bool CanChangeType(TECControllerType newType)
        {
            if (newType == null)
                return false;
            TECController possibleController = new TECController(newType, false);
            IOCollection necessaryIO = this.getUsedIO();
            IOCollection possibleIO = possibleController.getPotentialIO() + possibleController.getTotalIO();
            return possibleIO.Contains(necessaryIO);
        }
        public void ChangeType(TECControllerType newType)
        {
            if (CanChangeType(newType))
            {
                this.IOModules.ObservablyClear();
                this.Type = newType;
                ModelCleanser.addRequiredIOModules(this);
            }
            else
            {
                return;
            }
        }
        
        protected override CostBatch getCosts()
        {
            if (!IsTypical)
            {
                CostBatch costs = base.getCosts();
                costs += Type.CostBatch;
                foreach (TECConnection connection in
                    ChildrenConnections.Where(connection => !connection.IsTypical))
                {
                    costs += connection.CostBatch;
                }
                foreach(TECIOModule module in IOModules)
                {
                    costs += module.CostBatch;
                }
                return costs;
            } else
            {
                return new CostBatch();
            }
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.ChildrenConnections, "ChildrenConnections");
            saveList.AddRange(this.IOModules, "IOModules");
            saveList.Add(this.Type, "Type");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(this.IOModules, "IOModules");
            saveList.Add(this.Type, "Type");
            return saveList;
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }

        private void addChildConnection(TECConnection connection)
        {
            ChildrenConnections.Add(connection);
            if (!connection.IsTypical && !this.IsTypical)
            {
                notifyCostChanged(connection.CostBatch);
            }
        }
        private void removeChildConnection(TECConnection connection)
        {
            ChildrenConnections.Remove(connection);
            if (!connection.IsTypical && !this.IsTypical)
            {
                notifyCostChanged(connection.CostBatch * -1);
            }
        }

        private void handleChildrenChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            collectionChanged(sender, e, "ChildrenConnections");
        }
        private void handleModulesChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            collectionChanged(sender, e, "IOModules");
        }
        
        private IOCollection getPotentialIO()
        {
            IOCollection potentialIO = new IOCollection();
            foreach(TECIOModule module in Type.IOModules)
            {
                foreach(TECIO io in module.IO)
                {
                    potentialIO.Add(io);
                }
            }
            foreach(TECIOModule module in IOModules)
            {
                foreach(TECIO io in module.IO)
                {
                    potentialIO.Remove(io);
                }
            }
            return potentialIO;
        }

        
        #endregion
    }
}
