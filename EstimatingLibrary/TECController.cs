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
                if (connection is TECHardwiredConnection)
                {
                    TECHardwiredConnection connectionToAdd = new TECHardwiredConnection(connection as TECHardwiredConnection, this, isTypical, guidDictionary);
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
            //Determines relevant IO based on if the connectable is networkable
            IOCollection connectableIO = connectable.IsNetwork ? connectable.AvailableProtocols : connectable.HardwiredIO;

            //Collection of current IO and IO from potential modules
            IOCollection possibleIO = this.AvailableIO + getPotentialIO();

            return possibleIO.Contains(connectableIO);
        }
        public TECConnection Connect(IConnectable connectable)
        {
            //TO DO: Potential Modules

            if (connectable.IsNetwork)
            {
                foreach (TECIO io in connectable.AvailableProtocols.ToList())
                {
                    if (this.AvailableProtocols.Contains(io))
                    {
                        return constructNetworkConnection(io.Protocol);
                    }
                }

                foreach(TECIO io in connectable.AvailableProtocols.ToList())
                {
                    if (this.getPotentialIO().Contains(io))
                    {
                        List<TECIOModule> newModules = getModulesForIO(io.ToIOCollection());
                        newModules.ForEach(module => this.IOModules.Add(module));

                        return constructNetworkConnection(io.Protocol);
                    }
                }

                TECNetworkConnection constructNetworkConnection(TECProtocol protocol)
                {
                    TECNetworkConnection connection = new TECNetworkConnection(this, protocol, this.IsTypical);
                    connection.AddChild(connectable);
                    addChildConnection(connection);
                    return connection;
                }

                throw new Exception("No matching protocols");
            }
            else
            {
                if (this.AvailableIO.Contains(connectable.HardwiredIO))
                {
                    return constructHardwiredConnection();
                }
                else
                {
                    List<TECIOModule> newModules = getModulesForIO(connectable.HardwiredIO);
                    newModules.ForEach(module => this.IOModules.Add(module));

                    return constructHardwiredConnection();
                }

                TECHardwiredConnection constructHardwiredConnection()
                {
                    TECHardwiredConnection connection = new TECHardwiredConnection(connectable, this, this.IsTypical);
                    addChildConnection(connection);
                    return connection;
                }
                
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
                foreach (IConnectable child in children)
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

        /// <summary>
        /// Removes the connectable from controller and parent connection.
        /// </summary>
        /// <param name="connectable">Child</param>
        /// <returns>Parent network connnection if applicable</returns>
        public TECNetworkConnection RemoveConnectable(IConnectable connectable)
        {
            TECConnection connectionToRemove = null;
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECHardwiredConnection hardwiredConnection)
                {
                    if (hardwiredConnection.Child == connectable)
                    {
                        connectionToRemove = hardwiredConnection;
                        connectable.SetParentConnection(null);
                        break;
                    }
                }
                else if (connection is TECNetworkConnection netConnect)
                {
                    if (netConnect.Children.Contains(connectable))
                    {
                        netConnect.RemoveChild(connectable);
                        connectable.SetParentConnection(null);
                        return netConnect;
                    }
                }
            }
            if (connectionToRemove != null)
            {
                removeChildConnection(connectionToRemove);
            }
            return null;
        }
        /// <summary>
        /// Disconnects all connections in this controller including parent connection.
        /// </summary>
        public void Disconnect()
        {
            //Remove network connections
            RemoveAllChildNetworkConnections();

            //Remove hardwired connections
            List<IConnectable> connectables = new List<IConnectable>();
            foreach (TECHardwiredConnection connection in this.ChildrenConnections)
            {
                connectables.Add(connection.Child);
            }
            foreach (IConnectable connectable in connectables)
            {
                RemoveConnectable(connectable);
            }

            //Remove parent connection
            this.ParentConnection?.RemoveChild(this);
        }
        public void RemoveAllChildNetworkConnections()
        {
            List<TECConnection> networkConnections = new List<TECConnection>(ChildrenConnections.Where(connection => connection is TECNetworkConnection));
            networkConnections.ForEach(netConnect => RemoveNetworkConnection(netConnect as TECNetworkConnection));
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

        private List<TECIOModule> getPotentialModules()
        {
            List<TECIOModule> modules = new List<TECIOModule>(this.Type.IOModules);
            foreach(TECIOModule module in this.IOModules)
            {
                modules.Remove(module);
            }
            return modules;
        }
        /// <summary>
        /// Gets nessessary modules from potential modules to handle io.
        /// </summary>
        /// <param name="io"></param>
        /// <returns>Nessessary modules. Returns empty list if no collection of modules exists.</returns>
        private List<TECIOModule> getModulesForIO(IOCollection io)
        {
            IOCollection nessessaryIO = io - (io | AvailableIO);

            List<TECIOModule> potentialModules = getPotentialModules();

            //Check that any singular module can cover the nessessary io
            foreach(TECIOModule module in potentialModules)
            {
                if (module.IOCollection.Contains(io)) return new List<TECIOModule>() { module };
            }
            
            //List of modules to return
            List<TECIOModule> returnModules = new List<TECIOModule>();
            foreach(TECIO type in io.ToList())
            {
                TECIO singularIO = new TECIO(type);
                singularIO.Quantity = 1;

                //List of remaining potential modules after return modules is considered
                List<TECIOModule> newPotentialModules = new List<TECIOModule>(potentialModules);
                foreach(TECIOModule module in returnModules)
                {
                    newPotentialModules.Remove(module);
                }

                //Add the first module that contains the IOType we're checking
                foreach(TECIOModule module in newPotentialModules)
                {
                    if (module.IOCollection.Contains(singularIO))
                    {
                        returnModules.Add(module);
                        break;
                    }
                }

                //If return modules satisfies our IO, return them
                if (returnModules.ToIOCollection().Contains(io))
                {
                    return returnModules;
                }
            }

            return new List<TECIOModule>();
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
            if (newType == null) return false;
            TECController possibleController = new TECController(newType, false);
            IOCollection necessaryIO = this.UsedIO;
            IOCollection possibleIO = possibleController.getPotentialIO() + possibleController.AvailableIO;
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
                foreach (TECIOModule module in IOModules)
                {
                    costs += module.CostBatch;
                }
                return costs;
            }
            else
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
            foreach (TECIOModule module in Type.IOModules)
            {
                foreach (TECIO io in module.IO)
                {
                    potentialIO.Add(io);
                }
            }
            foreach (TECIOModule module in IOModules)
            {
                foreach (TECIO io in module.IO)
                {
                    potentialIO.Remove(io);
                }
            }
            return potentialIO;
        }
        #endregion

        #region IConnectable Implementation
        IOCollection IConnectable.HardwiredIO
        {
            get { return new IOCollection(); }
        }
        bool IConnectable.IsNetwork
        {
            get { return true; }
        }
        List<TECConnectionType> IConnectable.RequiredConnectionTypes
        {
            get { return new List<TECConnectionType>(); }
        }

        TECConnection IConnectable.GetParentConnection()
        {
            return this.ParentConnection;
        }
        void IConnectable.SetParentConnection(TECConnection connection)
        {
            if (connection is TECNetworkConnection networkConnection)
            {
                this.ParentConnection = networkConnection;
            }
            else
            {
                throw new Exception("Controller must have network parent connection.");
            }
        }
        bool IConnectable.CanSetParentConnection(TECConnection connection)
        {
            return (connection is TECNetworkConnection);
        }
        #endregion

    }
}
