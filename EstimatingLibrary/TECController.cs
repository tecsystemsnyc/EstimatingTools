﻿using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public enum NetworkType
    {
        Unitary = 1, DDC, Server
    };

    public class TECController : TECCost, CostComponent
    {
        #region Properties
        //---Stored---
        private TECNetworkConnection _parentConnection;
        private ObservableCollection<TECConnection> _childrenConnections;
        private ObservableCollection<TECIO> _io;
        private TECManufacturer _manufacturer;
        private NetworkType _networkType;
        
        public TECNetworkConnection ParentConnection
        {
            get { return _parentConnection; }
            set
            {
                var temp = Copy();
                _parentConnection = value;
                NotifyPropertyChanged("ParentConnection", temp, this);
                RaisePropertyChanged("NetworkIO");
            }
        }
        public ObservableCollection<TECConnection> ChildrenConnections
        {
            get { return _childrenConnections; }
            set
            {
                var temp = this.Copy();
                ChildrenConnections.CollectionChanged -= collectionChanged;
                _childrenConnections = value;
                ChildrenConnections.CollectionChanged += collectionChanged;
                NotifyPropertyChanged("ChildrenConnections", temp, this);
                RaisePropertyChanged("ChildNetworkConnections");
            }
        }
        public ObservableCollection<TECIO> IO
        {
            get { return _io; }
            set
            {
                var temp = this.Copy();
                IO.CollectionChanged -= IO_CollectionChanged;
                _io = value;
                NotifyPropertyChanged("IO", temp, this);
                IO.CollectionChanged += IO_CollectionChanged;
            }
        }
        public TECManufacturer Manufacturer
        {
            get { return _manufacturer; }
            set
            {
                var temp = this.Copy();
                _manufacturer = value;
                NotifyPropertyChanged("Manufacturer", temp, this);
                NotifyPropertyChanged("ChildChanged", (object)this, (object)value);
            }
        }
        public NetworkType NetworkType
        {
            get { return _networkType; }
            set
            {
                var temp = Copy();
                _networkType = value;
                NotifyPropertyChanged("NetworkType", temp, this);
            }
        }

        public List<TECCost> Costs
        {
            get
            {
                return getCosts();
            }
        }

        private List<TECCost> getCosts()
        {
            var outCosts = new List<TECCost>();
            outCosts.Add(this);
            foreach(TECCost cost in AssociatedCosts)
            {
                outCosts.Add(cost);
            }
            foreach(TECConnection connection in ChildrenConnections)
            {
                foreach(TECCost cost in connection.Costs)
                {
                    outCosts.Add(cost);
                }
            }
            return outCosts;
        }

        public override double ExtendedCost
        {
            get { return Cost * Manufacturer.Multiplier; }
        }
        //---Derived---
        public ObservableCollection<IOType> AvailableIO
        {
            get { return getAvailableIO(); }
        }

        public ObservableCollection<IOType> NetworkIO
        {
            get
            { return getNetworkIO(); }
        }

        public bool IsGlobal;

        #endregion

        #region Constructors
        public TECController(Guid guid, TECManufacturer manufacturer, bool isGlobal = true) : base(guid)
        {
            IsGlobal = isGlobal;
            _cost = 0;
            _io = new ObservableCollection<TECIO>();
            _childrenConnections = new ObservableCollection<TECConnection>();
            _manufacturer = manufacturer;
            ChildrenConnections.CollectionChanged += collectionChanged;
            IO.CollectionChanged += IO_CollectionChanged;
            _type = CostType.TEC;
        }

        public TECController(TECManufacturer manufacturer, bool isGlobal = true) : this(Guid.NewGuid(), manufacturer, isGlobal) { }
        public TECController(TECController controllerSource, Dictionary<Guid, Guid> guidDictionary = null) : this(controllerSource.Manufacturer)
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = controllerSource.Guid; }
            copyPropertiesFromScope(controllerSource);
            foreach (TECIO io in controllerSource.IO)
            {
                TECIO ioToAdd = new TECIO(io);
                _io.Add(new TECIO(io));
            }
            foreach (TECConnection connection in controllerSource.ChildrenConnections)
            {
                if (connection is TECSubScopeConnection)
                {
                    TECSubScopeConnection connectionToAdd = new TECSubScopeConnection(connection as TECSubScopeConnection, guidDictionary);
                    connectionToAdd.ParentController = this;
                    _childrenConnections.Add(connectionToAdd);
                }
                else if (connection is TECNetworkConnection)
                {

                    TECNetworkConnection connectionToAdd = new TECNetworkConnection(connection as TECNetworkConnection, guidDictionary);
                    connectionToAdd.ParentController = this;
                    _childrenConnections.Add(connectionToAdd);
                }
            }
            _cost = controllerSource.Cost;
        }

        #endregion

        #region Event Handlers
        private void IO_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Object item in e.NewItems)
                {
                    if (item is TECIO)
                    {
                        NotifyPropertyChanged("Add", this, (item as TECObject).Copy());
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Object item in e.OldItems)
                {
                    if (item is TECIO)
                    {
                        NotifyPropertyChanged("Remove", this, (item as TECObject).Copy());
                    }
                }
            }
        }

        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    NotifyPropertyChanged("Add", this, item, typeof(TECController), typeof(TECConnection));
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    NotifyPropertyChanged("Remove", this, item, typeof(TECController), typeof(TECConnection));
                }
            }
            if (sender == ChildrenConnections)
            {
                RaisePropertyChanged("ChildNetworkConnections");
            }
        }
        #endregion

        #region Connection Methods
        public TECNetworkConnection AddController(TECController controller, TECNetworkConnection connection)
        {
            if (controller != this)
            {
                foreach (TECConnection conn in ChildrenConnections)
                {
                    if (conn is TECNetworkConnection)
                    {
                        TECNetworkConnection netConn = conn as TECNetworkConnection;
                        if (connection == netConn)
                        {
                            bool ioMatches = false;
                            foreach(IOType ioType in controller.NetworkIO)
                            {
                                if (connection.IOType == ioType)
                                {
                                    ioMatches = true;
                                    break;
                                }
                            }
                            if (ioMatches)
                            {
                                netConn.ChildrenControllers.Add(controller);
                                controller.ParentConnection = netConn;
                                return netConn;
                            }
                            else
                            {
                                throw new ArgumentException("Controller and connection do not have a matching IOType.");
                            }
                        }
                    }
                }
                throw new ArgumentOutOfRangeException("Passed connection does not exist in controller.");
            }
            else
            {
                return null;
            }
        }
        public TECNetworkConnection AddController(TECController controller, TECConnectionType connectionType)
        {
            if (controller != this)
            {
                IOType ioType = 0;
                foreach (IOType thisType in this.NetworkIO)
                {
                    foreach (IOType otherType in controller.NetworkIO)
                    {
                        if (thisType == otherType)
                        {
                            ioType = thisType;
                            break;
                        }
                    }
                    if (ioType != 0) { break; }
                }
                if (ioType == 0)
                {
                    throw new ArgumentException("Controller and parent do not have a matching IOType.");
                }

                TECNetworkConnection netConnect = new TECNetworkConnection();
                netConnect.ParentController = this;
                netConnect.ChildrenControllers.Add(controller);
                netConnect.IOType = ioType;
                netConnect.ConnectionType = connectionType;
                ChildrenConnections.Add(netConnect);
                controller.ParentConnection = netConnect;
                return netConnect;
            }
            else
            {
                return null;
            }
        }
        public TECSubScopeConnection AddSubScope(TECSubScope subScope)
        {
            TECSubScopeConnection connection = new TECSubScopeConnection();
            connection.ParentController = this;
            connection.SubScope = subScope;
            ChildrenConnections.Add(connection);
            subScope.Connection = connection;
            return connection;
        }
        public void RemoveController(TECController controller)
        {
            bool exists = false;
            TECNetworkConnection connectionToRemove = null;
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECNetworkConnection)
                {
                    var netConnect = connection as TECNetworkConnection;
                    if (netConnect.ChildrenControllers.Contains(controller))
                    {
                        exists = true;
                        controller.ParentConnection = null;
                        netConnect.ChildrenControllers.Remove(controller);
                    }
                    if (netConnect.ChildrenControllers.Count < 1)
                    {
                        connectionToRemove = netConnect;
                    }
                }

            }
            if (connectionToRemove != null)
            {
                ChildrenConnections.Remove(connectionToRemove);
            }
            if (!exists)
            {
                throw new ArgumentOutOfRangeException("Passed controller does not exist in any connection in controller.");
            }
        }
        public void RemoveSubScope(TECSubScope subScope)
        {
            TECSubScopeConnection connectionToRemove = null;
            foreach (TECConnection connection in ChildrenConnections)
            {
                if (connection is TECSubScopeConnection)
                {
                    var subConnect = connection as TECSubScopeConnection;
                    if (subConnect.SubScope == subScope)
                    {
                        connectionToRemove = subConnect;
                    }
                }
            }
            if (connectionToRemove != null)
            {
                ChildrenConnections.Remove(connectionToRemove);
                subScope.Connection = null;
                connectionToRemove.SubScope = null;
                connectionToRemove.ParentController = null;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Passed subscope does not exist in any connection in controller.");
            }
        }

        public void RemoveAllConnections()
        {
            ObservableCollection<TECConnection> connectionsToRemove = new ObservableCollection<TECConnection>();
            foreach(TECConnection connection in ChildrenConnections)
            {
                connectionsToRemove.Add(connection);
            }
            foreach(TECConnection connectToRemove in connectionsToRemove)
            {
                if (connectToRemove is TECNetworkConnection)
                {
                    ObservableCollection<TECController> controllersToRemove = new ObservableCollection<TECController>();
                    foreach(TECController controller in (connectToRemove as TECNetworkConnection).ChildrenControllers)
                    {
                        controller.ParentConnection = null;
                        controllersToRemove.Add(controller);
                    }
                    foreach(TECController controller in controllersToRemove)
                    {
                        (connectToRemove as TECNetworkConnection).ChildrenControllers.Remove(controller);
                    }
                }
                else if (connectToRemove is TECSubScopeConnection)
                {
                    (connectToRemove as TECSubScopeConnection).SubScope.Connection = null;
                    (connectToRemove as TECSubScopeConnection).SubScope = null;
                    connectToRemove.ParentController = null;
                }
                else
                {
                    throw new NotImplementedException();
                }
                ChildrenConnections.Remove(connectToRemove);
            }
            SetParentController(null, null);
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
        public void SetParentController(TECController controller, TECConnectionType connectionType)
        {
            if (ParentConnection != null)
            {
                GetParentController().RemoveController(this);
            }

            if (controller != null)
            {
                controller.AddController(this, connectionType);
            }
        }

        private ObservableCollection<TECNetworkConnection> getNetworkConnections()
        {
            ObservableCollection<TECNetworkConnection> networkConnections = new ObservableCollection<TECNetworkConnection>();
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

        #region Methods
        public override Object Copy()
        {
            TECController outController = new TECController(this.Guid, Manufacturer);
            outController.copyPropertiesFromScope(this);
            outController._cost = Cost;
            outController._type = Type;
            foreach (TECIO io in this.IO)
            {
                outController.IO.Add(io.Copy() as TECIO);
            }
            foreach (TECConnection connection in ChildrenConnections)
            {
                var outConnection = connection.Copy() as TECConnection;
                outConnection.ParentController = outController;
                outController.ChildrenConnections.Add(outConnection);
            }

            return outController;
        }
        public override Object DragDropCopy()
        {
            var outController = new TECController(this);
            return outController;
        }
        private ObservableCollection<IOType> getAvailableIO()
        {
            var availableIO = new ObservableCollection<IOType>();
            foreach (TECIO type in this.IO)
            {
                for (var x = 0; x < type.Quantity; x++)
                {
                    availableIO.Add(type.Type);
                }
            }

            foreach (TECSubScopeConnection connected in ChildrenConnections)
            {
                foreach (TECDevice device in connected.SubScope.Devices)
                {
                    availableIO.Remove(device.IOType);
                }
            }
            return availableIO;
        }
        private ObservableCollection<IOType> getNetworkIO()
        {
            var outIO = new ObservableCollection<IOType>();
            foreach (TECIO io in this.IO)
            {
                var type = io.Type;
                if (type != IOType.AI && type != IOType.AO && type != IOType.DI && type != IOType.DO)
                {
                    for (var x = 0; x < io.Quantity; x++)
                    {
                        outIO.Add(type);
                    }
                }
            }

            return outIO;
        }
        public int NumberOfIOType(IOType ioType)
        {
            int outNum = 0;

            foreach (TECIO type in IO)
            {
                if (type.Type == ioType)
                {
                    outNum = type.Quantity;
                }
            }

            return outNum;
        }
        public List<IOType> getUniqueIO()
        {
            var outList = new List<IOType>();

            foreach (TECIO io in this.IO)
            {
                if (!outList.Contains(io.Type))
                {
                    outList.Add(io.Type);
                }
            }
            return outList;
        }
        #endregion
    }
}
