using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECSubScope : TECLocated, INotifyPointChanged, IDDCopiable, ITypicalable, IConnectable, IInterlockable
    {
        #region Properties
        public ObservableCollection<IEndDevice> Devices { get; } = new ObservableCollection<IEndDevice>();
        public ObservableCollection<TECPoint> Points { get; } = new ObservableCollection<TECPoint>();
        public ObservableCollection<TECInterlockConnection> Interlocks { get; } = new ObservableCollection<TECInterlockConnection>();
        
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
            Devices.CollectionChanged += DevicesCollectionChanged;
            Points.CollectionChanged += PointsCollectionChanged;
            Interlocks.CollectionChanged += InterlocksCollectionChanged;
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
            this.copyPropertiesFromScope(sourceSubScope);
        }
        #endregion //Constructors

        #region Events
        public event Action<int> PointChanged;
        #endregion
        
        #region Event Handlers
        private void PointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Points", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
        }
        private void DevicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        private void InterlocksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Interlocks", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
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
        
        public void AddPoint(TECPoint point)
        {
            Points.Add(point);
        }
        public bool RemovePoint(TECPoint point)
        {
            return Points.Remove(point);
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
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            List<TECObject> deviceList = new List<TECObject>();
            foreach (IEndDevice item in this.Devices)
            {
                deviceList.Add(item as TECObject);
            }
            saveList.AddRange(deviceList, "Devices");
            saveList.AddRange(this.Points, "Points");
            saveList.AddRange(this.Interlocks, "Interlocks");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            List<TECObject> deviceList = new List<TECObject>();
            foreach (IEndDevice item in this.Devices)
            {
                deviceList.Add(item as TECObject);
            }
            saveList.AddRange(deviceList.Distinct(), "Devices");
            return saveList;
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
                foreach(IEndDevice endDev in this.Devices)
                {
                    if (protocols.Count <= 0)
                    {
                        protocols.AddRange(endDev.ConnectionMethods);
                    }
                    else
                    {
                        List<IProtocol> toRemove = new List<IProtocol>();
                        foreach(IProtocol protocol in protocols)
                        {
                            if (!endDev.ConnectionMethods.Contains(protocol))
                            {
                                toRemove.Add(protocol);
                            }
                        }
                        foreach(IProtocol protocol in toRemove)
                        {
                            protocols.Remove(protocol);
                        }
                    }
                }
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
    }
}
