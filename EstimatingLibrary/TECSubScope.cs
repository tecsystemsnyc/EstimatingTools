using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECSubScope : TECLocated, INotifyPointChanged, IDDCopiable, ITypicalable, IConnectable, IInterlockable, ICatalogContainer
    {
        #region Properties
        public ObservableCollection<IEndDevice> Devices { get; } = new ObservableCollection<IEndDevice>();
        public ObservableCollection<TECPoint> Points { get; } = new ObservableCollection<TECPoint>();
        public ObservableCollection<TECInterlockConnection> Interlocks { get; } = new ObservableCollection<TECInterlockConnection>();
        public ObservableCollection<TECScopeBranch> ScopeBranches { get; } = new ObservableCollection<TECScopeBranch>();
        
        public IControllerConnection Connection { get; private set; }
        public int PointNumber
        {
            get
            {
                return getPointNumber();
            }
        }
        
        public bool IsTypical { get; private set; }

        //Derived
        public bool IsConnected
        {
            get
            {
                return Connection != null;
            }
        }
        
        #endregion //Properties

        #region Constructors
        public TECSubScope(Guid guid) : base(guid)
        {
            IsTypical = false;
            Devices.CollectionChanged += devicesCollectionChanged;
            Points.CollectionChanged += pointsCollectionChanged;
            Interlocks.CollectionChanged += interlocksCollectionChanged;
            ScopeBranches.CollectionChanged += scopeBranchesCollectionChanged;
        }
        
        public TECSubScope() : this(Guid.NewGuid()) { }

        //Copy Constructor
        public TECSubScope(TECSubScope sourceSubScope, Dictionary<Guid, Guid> guidDictionary = null,
            ObservableListDictionary<ITECObject> characteristicReference = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = sourceSubScope.Guid; }
            foreach (IEndDevice device in sourceSubScope.Devices)
            { Devices.Add(device); }
            foreach (TECPoint point in sourceSubScope.Points)
            {
                var toAdd = new TECPoint(point);
                characteristicReference?.AddItem(point,toAdd);
                Points.Add(toAdd);
            }
            foreach (TECInterlockConnection interlock in sourceSubScope.Interlocks)
            {
                var toAdd = new TECInterlockConnection(interlock);
                characteristicReference?.AddItem(interlock, toAdd);
                Interlocks.Add(toAdd);
            }
            foreach(TECScopeBranch branch in sourceSubScope.ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch);
                characteristicReference?.AddItem(branch, toAdd);
                ScopeBranches.Add(toAdd);
            }
            this.copyPropertiesFromScope(sourceSubScope);
        }
        #endregion //Constructors

        #region Events
        public event Action<int> PointChanged;
        #endregion
        
        #region Event Handlers
        private void pointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Points", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
        }
        private void devicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Devices", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if(this.Connection != null && !this.AvailableProtocols.Contains(this.Connection.Protocol))
                {
                    this.Connection.ParentController.Disconnect(this);
                }
            }
        }
        private void interlocksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Interlocks", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
        }
        private void scopeBranchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {            
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "ScopeBranches", this, notifyCombinedChanged);
        }
        #endregion

        #region Methods

        public object DragDropCopy(TECScopeManager scopeManager)
        {
            TECSubScope outScope = new TECSubScope(this);
            outScope.IsTypical = this.IsTypical;
            ModelLinkingHelper.LinkScopeItem(outScope, scopeManager);
            return outScope;
        }
        
        public bool AddPoint(TECPoint point)
        {
            if (this.Connection == null || !(this.Connection is TECHardwiredConnection))
            {
                this.Points.Add(point);
                return true;
            }
            else
            {
                TECIO io = new TECIO(point.Type) { Quantity = point.Quantity };
                if (this.Connection.ParentController.AvailableIO.Contains(io))
                {
                    this.Points.Add(point);
                    return true;
                }
            }
            return false;
        }
        public bool RemovePoint(TECPoint point)
        {
            return Points.Remove(point);
        }

        public bool AddDevice(IEndDevice device)
        {
            if (this.Connection == null)
            {
                this.Devices.Add(device);
                return true;
            }
            else
            {
                if (Connection.Protocol is TECProtocol && device.ConnectionMethods.Contains(Connection.Protocol))
                {
                    this.Devices.Add(device);
                    return true;
                }
            }
            return false;
        }
        public bool RemoveDevice(IEndDevice device)
        {
            return Devices.Remove(device);
        }

        public bool CanChangeDevice(IEndDevice oldDevice, IEndDevice newDevice)
        {
            if (this.Connection == null) return true;
            if (Connection.Protocol is TECProtocol && newDevice.ConnectionMethods.Contains(Connection.Protocol))
            {
                return true;
            }
            else if (Connection.Protocol is TECHardwiredProtocol &&
                oldDevice.HardwiredConnectionTypes.SequenceEqual(newDevice.HardwiredConnectionTypes))
            {
                return true;
            }
            return false;
        }

        public bool CanConnectToNetwork(TECNetworkConnection netConnect)
        {
            return this.AvailableProtocols.Contains(netConnect.Protocol);
        }
        
        private int getPointNumber()
        {
            var totalPoints = 0;
            foreach (TECPoint point in Points)
            {
                totalPoints += point.Quantity;
            }
            return totalPoints;
        }

        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts();
            foreach (IEndDevice dev in Devices)
            {
                if(dev is INotifyCostChanged costDev)
                {
                    costs += costDev.CostBatch;
                }
                else
                {
                    throw new Exception("This contains an unsupported end device.");
                }
            }
            return costs;
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }
        private void notifyPointChanged(int numPoints)
        {
            if (!IsTypical)
            {
                PointChanged?.Invoke(numPoints);
            }
        }

        public IConnectable Copy(Dictionary<Guid, Guid> guidDictionary)
        {
            return new TECSubScope(this, guidDictionary);
        }
        #endregion

        #region IReltable
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            List<TECObject> deviceList = new List<TECObject>();
            foreach (IEndDevice item in this.Devices)
            {
                deviceList.Add(item as TECObject);
            }
            saveList.AddRange(deviceList, "Devices");
            saveList.AddRange(this.Points, "Points");
            saveList.AddRange(this.Interlocks, "Interlocks");
            saveList.AddRange(this.ScopeBranches, "ScopeBranches");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            List<TECObject> deviceList = new List<TECObject>();
            foreach (IEndDevice item in this.Devices)
            {
                deviceList.Add(item as TECObject);
            }
            saveList.AddRange(deviceList.Distinct(), "Devices");
            return saveList;
        }

        #endregion

        #region IConnectable
        /// <summary>
        /// Returns the intersect of connection methods in this TECSubScope's devices.
        /// </summary>
        public List<IProtocol> AvailableProtocols
        {
            get
            {
                List<IProtocol> protocols = new List<IProtocol>();
                if (Connection != null) return protocols;
                List<TECConnectionType> hardConnectionTypes = new List<TECConnectionType>();
                List<TECProtocol> catalogProtocols = new List<TECProtocol>();
                bool allDevsHaveHard = true;
                foreach(IEndDevice endDev in this.Devices)
                {
                    if (endDev.HardwiredConnectionTypes.Count < 1) allDevsHaveHard = false;
                    hardConnectionTypes.AddRange(endDev.HardwiredConnectionTypes);
                    if (catalogProtocols.Count <= 0)
                    {
                        catalogProtocols.AddRange(endDev.PossibleProtocols);
                    }
                    else
                    {
                        List<TECProtocol> toRemove = new List<TECProtocol>();
                        foreach(TECProtocol protocol in catalogProtocols)
                        {
                            if (!endDev.PossibleProtocols.Contains(protocol))
                            {
                                toRemove.Add(protocol);
                            }
                        }
                        foreach(TECProtocol protocol in toRemove)
                        {
                            catalogProtocols.Remove(protocol);
                        }
                    }
                }
                if (allDevsHaveHard) protocols.Add(new TECHardwiredProtocol(hardConnectionTypes));
                protocols.AddRange(catalogProtocols);
                return protocols;
            }
        }
        IOCollection IConnectable.HardwiredIO
        {
            get
            {
                return this.Points.ToIOCollection();
            }
        }

        bool IConnectable.CanSetParentConnection(IControllerConnection connection)
        {
            return ((IConnectable)this).AvailableProtocols.Contains(connection.Protocol);            
        }
        void IConnectable.SetParentConnection(IControllerConnection connection)
        {
            Connection = connection;
            raisePropertyChanged("Connection");
        }
        IControllerConnection IConnectable.GetParentConnection()
        {
            return this.Connection;
        }

        #endregion

        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            } 
            else
            {
                return new TECSubScope(this, characteristicReference: typicalDictionary);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            if(property == "Points" && item is TECPoint point)
            {
                AddPoint(point);
            }
            else if (property == "Devices" && item is IEndDevice device)
            {
                Devices.Add(device);
            }
            else if (property == "Interlocks" && item is TECInterlockConnection interlock)
            {
                Interlocks.Add(interlock);
            }
            else if (property == "ScopeBranches" && item is TECScopeBranch branch)
            {
                ScopeBranches.Add(branch);
            }
            else
            {
                this.AddChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            if (property == "Points" && item is TECPoint point)
            {
                return RemovePoint(point);
            }
            else if (property == "Devices" && item is IEndDevice device)
            {
                return Devices.Remove(device);
            }
            else if (property == "Interlocks" && item is TECInterlockConnection interlock)
            {
                return Interlocks.Remove(interlock);
            }
            else if (property == "ScopeBranches" && item is TECScopeBranch branch)
            {
               return ScopeBranches.Remove(branch);
            }
            else
            {
                return this.RemoveChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            if (property == "Points" && item is TECPoint point)
            {
                return Points.Contains(point);
            }
            else if (property == "Devices" && item is IEndDevice device)
            {
                return Devices.Contains(device);
            }
            else if (property == "Interlocks" && item is TECInterlockConnection interlock)
            {
                return Interlocks.Contains(interlock);
            }
            else if (property == "ScopeBranches" && item is TECScopeBranch branch)
            {
                return ScopeBranches.Contains(branch);
            }
            else
            {
                return this.ContainsChildForScopeProperty(property, item);
            }
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
            TypicalableUtilities.MakeChildrenTypical(this);
        }
        #endregion

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool removedEndDevice = false;
            if (item is IEndDevice oldDev && replacement is IEndDevice newDev)
            {
                if (this.Devices.Contains(oldDev))
                {
                    if (CanChangeDevice(oldDev, newDev))
                    {
                        removedEndDevice = CommonUtilities.OptionallyReplaceAll(oldDev, this.Devices, newDev);
                    }
                    else throw new ArgumentException("Replacement Device must be compatible.");
                }
            }

            return (removedEndDevice || alreadyRemoved);
        }
        #endregion
    }
}
