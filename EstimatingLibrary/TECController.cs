using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EstimatingLibrary
{
    public abstract class TECController : TECLocated, IConnectable, ITypicalable
    {
        private bool _isServer;
        #region Properties
        public TECNetworkConnection ParentConnection { get; private set; }
        public ObservableCollection<IControllerConnection> ChildrenConnections { get; } = new ObservableCollection<IControllerConnection>();
        
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

        public bool IsTypical
        {
            get; protected set;
        }

        //---Derived---
        public abstract IOCollection IO
        {
            get;
        }
        public IOCollection UsedIO
        {
            get
            {
                return ChildrenConnections.Aggregate(new IOCollection(), (total, next) => total += next.IO);
            }
        }
        public virtual IOCollection AvailableIO
        {
            get
            {
                IOCollection io = new IOCollection(this.IO);
                IOCollection usedIO = new IOCollection(this.UsedIO);
                if (!io.Remove(usedIO.ToList())) throw new Exception("This controller has used more IO than has available.");
                return io;
            }
        }
        public List<IProtocol> AvailableProtocols
        {
            get { return AvailableIO.Protocols; }
        }
        #endregion

        #region Constructors
        public TECController(Guid guid) : base(guid)
        {
            _isServer = false;
            IsTypical = false;
            ChildrenConnections.CollectionChanged += handleChildrenChanged;
        }

        public TECController() : this(Guid.NewGuid()) { }
        public TECController(TECController controllerSource, Dictionary<Guid, Guid> guidDictionary = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = controllerSource.Guid; }
            copyPropertiesFromLocated(controllerSource);
            foreach (IControllerConnection connection in controllerSource.ChildrenConnections)
            {
                if (connection is TECHardwiredConnection)
                {
                    TECHardwiredConnection connectionToAdd = new TECHardwiredConnection(connection as TECHardwiredConnection, this, guidDictionary);
                    ChildrenConnections.Add(connectionToAdd);
                }
                else if (connection is TECNetworkConnection)
                {

                    TECNetworkConnection connectionToAdd = new TECNetworkConnection(connection as TECNetworkConnection, this, guidDictionary);
                    ChildrenConnections.Add(connectionToAdd);
                }
            }
        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Returns the list of Protocols that can be used in both this controller and the connectable
        /// </summary>
        public List<IProtocol> CompatibleProtocols(IConnectable connectable)
        {
            List<IProtocol> compatProtocols = new List<IProtocol>();
            if(this == connectable) { return compatProtocols; }
            foreach(IProtocol protocol in connectable.AvailableProtocols)
            {
                if (protocol is TECHardwiredProtocol)
                {
                    if (this.AvailableIO.Contains(connectable.HardwiredIO))
                    {
                        compatProtocols.Add(protocol);
                    }
                }
                else if (this.AvailableProtocols.Contains(protocol))
                {
                    compatProtocols.Add(protocol);
                }
            }  
            return compatProtocols;
        }
        public bool CanConnect(IConnectable connectable, IProtocol protocol)
        {
            bool connectableIsNull = connectable == null;
            bool containsnetworkConnection = this.ChildrenConnections.OfType<TECNetworkConnection>().Select(x => x.Protocol).Contains(protocol);
            bool canAcceptProtocol = CompatibleProtocols(connectable).Contains(protocol);
            return connectable != null &&
                (CompatibleProtocols(connectable).Contains(protocol) || this.ChildrenConnections.OfType<TECNetworkConnection>().Select(x => x.Protocol).Contains(protocol));
        }
        public bool CanConnect(IConnectable connectable)
        {
            return connectable != null && CompatibleProtocols(connectable).Count > 0;
        }
        public virtual IControllerConnection Connect(IConnectable connectable, IProtocol protocol)
        {
            if (!CanConnect(connectable, protocol))
                return null;
            IControllerConnection connection;
            bool isNew = true;
            bool isTypical = (connectable as ITypicalable)?.IsTypical ?? false;
            if (protocol is TECHardwiredProtocol wired)
            {
                connection = new TECHardwiredConnection(connectable, this, wired);
            }
            else if (protocol is TECProtocol network)
            {
                TECNetworkConnection netConnect = ChildrenConnections.Where(x => x.Protocol == protocol).FirstOrDefault() as TECNetworkConnection;
                isNew = netConnect == null;
                if (isNew)
                {
                    netConnect = new TECNetworkConnection(this, network);
                }
                netConnect.AddChild(connectable);
                connection = netConnect;
            }
            else
            {
                throw new NotSupportedException("Unrecognized type implements IProtocol");
            }
            if (isNew)
            {
                this.ChildrenConnections.Add(connection);
            }
            return connection;
        }
        /// <summary>
        /// Removes the connectable from controller and parent connection.
        /// </summary>
        /// <param name="connectable">Child</param>
        /// <returns>Parent network connnection if applicable</returns>
        public TECNetworkConnection Disconnect(IConnectable connectable)
        {
            IControllerConnection connectionToRemove = null;
            foreach (IControllerConnection connection in ChildrenConnections)
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
                this.ChildrenConnections.Remove(connectionToRemove);
            }
            return null;
        }

        public bool CanAddNetworkConnection(TECProtocol protocol)
        {
            return AvailableProtocols.Contains(protocol);
        }
        public TECNetworkConnection AddNetworkConnection(TECProtocol protocol)
        {
            if (!CanAddNetworkConnection(protocol)) return null;
            TECNetworkConnection netConnect = new TECNetworkConnection(this, protocol);
            this.ChildrenConnections.Add(netConnect);
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
                this.ChildrenConnections.Remove(connection);
            }
            else
            {
                throw new InvalidOperationException("Network connection doesn't exist in controller.");
            }
        }

        /// <summary>
        /// Disconnects all connections in this controller including parent connection.
        /// </summary>
        public void DisconnectAll()
        {
            //Remove child connections
            RemoveAllChildConnections();

            //Remove parent connection
            this.ParentConnection?.RemoveChild(this);
        }
        public void RemoveAllChildNetworkConnections()
        {
            List<IControllerConnection> networkConnections = new List<IControllerConnection>(ChildrenConnections.Where(connection => connection is TECNetworkConnection));
            networkConnections.ForEach(netConnect => RemoveNetworkConnection(netConnect as TECNetworkConnection));
        }
        public void RemoveAllChildHardwiredConnections()
        {
            List<IConnectable> connectables = new List<IConnectable>();
            foreach (TECHardwiredConnection connection in this.ChildrenConnections.Where(connect => { return connect is TECHardwiredConnection; }))
            {
                connectables.Add(connection.Child);
            }
            foreach (IConnectable connectable in connectables)
            {
                Disconnect(connectable);
            }

        }
        public void RemoveAllChildConnections()
        {
            //Remove network connections
            RemoveAllChildNetworkConnections();

            //Remove hardwired connections
            RemoveAllChildHardwiredConnections();
        }
        
        #endregion

        #region Methods
        public abstract TECController CopyController(Dictionary<Guid, Guid> guidDictionary = null);

        #region Event Handlers
        protected virtual void collectionChanged(object sender, NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged, notifyCostChanged);

            if (propertyName == "ChildrenConnections")
            {
                raisePropertyChanged("ChildNetworkConnections");
            }
        }
        #endregion
        
        protected override CostBatch getCosts()
        {
            if (!IsTypical)
            {
                CostBatch costs = base.getCosts();
                foreach (IControllerConnection connection in
                    ChildrenConnections.Where(connection => !connection.IsTypical))
                {
                    costs += connection.CostBatch;
                }
                return costs;
            }
            else
            {
                return new CostBatch();
            }
        }
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.ChildrenConnections, "ChildrenConnections");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            return saveList;
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }

        private void handleChildrenChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            collectionChanged(sender, e, "ChildrenConnections");
        }
        #endregion
        
        #region IConnectable
        List<IProtocol> IConnectable.AvailableProtocols
        {
            get
            {
                return (this as IConnectable).GetParentConnection() == null ? this.AvailableProtocols : new List<IProtocol>();
            }
        }
        IOCollection IConnectable.HardwiredIO
        {
            get { return new IOCollection(); }
        }
        
        IConnectable IConnectable.Copy(Dictionary<Guid, Guid> guidDictionary)
        {
            return CopyController(guidDictionary);
        }
        IControllerConnection IConnectable.GetParentConnection()
        {
            return this.ParentConnection;
        }
        void IConnectable.SetParentConnection(IControllerConnection connection)
        {
            if (connection == null)
            {
                ParentConnection = null;
                raisePropertyChanged("ParentConnection");
            }
            else if (connection is TECNetworkConnection networkConnection)
            {
                ParentConnection = networkConnection;
                raisePropertyChanged("ParentConnection");
            }
            else
            {
                throw new Exception("Controller must have network parent connection.");
            }
        }
        bool IConnectable.CanSetParentConnection(IControllerConnection connection)
        {
            return (connection is TECNetworkConnection);
        }
        #endregion

        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            return this.createInstance(typicalDictionary);
        }
        protected abstract ITECObject createInstance(ObservableListDictionary<ITECObject> typicalDictionary);

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            this.addChildForProperty(property, item);
        }
        protected abstract void addChildForProperty(string property, ITECObject item);

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            return this.removeChildForProperty(property, item);
        }
        protected abstract bool removeChildForProperty(string property, ITECObject item);

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            return this.containsChildForProperty(property, item);
        }
        protected abstract bool containsChildForProperty(string property, ITECObject item);

        void ITypicalable.MakeTypical()
        {
            this.makeTypical();
        }
        protected abstract void makeTypical();
        #endregion

    }
}
