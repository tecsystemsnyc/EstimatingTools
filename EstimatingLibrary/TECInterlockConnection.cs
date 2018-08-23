using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECInterlockConnection : TECScope, IConnection, INotifyCostChanged, IRelatable, ITypicalable, ICatalogContainer
    {
        private readonly ConnectionWrapper connection;
        public bool IsTypical { get; private set; } = false;
        
        public ObservableCollection<TECConnectionType> ConnectionTypes { get; }
        
        public TECInterlockConnection(Guid guid, IEnumerable<TECConnectionType> connectionTypes) : base(guid)
        {
            this.connection = new ConnectionWrapper(guid, new TECHardwiredProtocol(connectionTypes));
            this.ConnectionTypes = new ObservableCollection<TECConnectionType>(connectionTypes);
            ConnectionTypes.CollectionChanged += connectionTypesCollectionChanged;
            subscribeToConnection();
        }

        public TECInterlockConnection(IEnumerable<TECConnectionType> connectionTypes) : this(Guid.NewGuid(), connectionTypes) { }
        public TECInterlockConnection(TECInterlockConnection source, Dictionary<Guid, Guid> guidDictionary = null) : this(source.Guid, source.ConnectionTypes)
        {
            this.copyPropertiesFromScope(source);
            this.connection = new ConnectionWrapper(source.connection, guidDictionary);
            subscribeToConnection();
        }
        
        private void subscribeToConnection()
        {
            this.connection.CostChanged += notifyCostChanged;
            this.connection.TECChanged += (args) => notifyTECChanged(args.Change, args.PropertyName, args.Sender, args.Value, args.OldValue);
            this.connection.PropertyChanged += (sender, args) => raisePropertyChanged(args.PropertyName);
        }
        private void connectionTypesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            connection.UpdateProtocol(new TECHardwiredProtocol(ConnectionTypes));
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "ConnectionTypes", this, notifyCombinedChanged, notifyCostChanged);
        }

        #region IConnection
        public double ConduitLength { get => connection.ConduitLength; set => connection.ConduitLength = value; }
        public TECElectricalMaterial ConduitType { get => connection.ConduitType; set => connection.ConduitType = value; }
        public bool IsPlenum { get => connection.IsPlenum; set => connection.IsPlenum = value; }
        public double Length { get => connection.Length; set => connection.Length = value; }

        public IProtocol Protocol => ((IConnection)connection).Protocol;

        #endregion

        #region INotifyCostChanged
        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts();
            costs += connection.CostBatch;
            return costs;
        }
        #endregion

        #region IRelatable
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(connection.PropertyObjects);
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(connection.LinkedObjects);
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");

            return saveList;
        }


        #endregion

        #region ITypicalable

        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            return new TECInterlockConnection(this);
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            this.AddChildForScopeProperty(property, item);
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            return this.RemoveChildForScopeProperty(property, item);
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            return this.ContainsChildForScopeProperty(property, item);
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
        }
        #endregion

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool removedConnectionType = false;
            if (item is TECConnectionType type)
            {
                removedConnectionType = CommonUtilities.OptionallyReplaceAll(type, this.ConnectionTypes, replacement as TECConnectionType);
            }

            return (removedConnectionType || alreadyRemoved);
        }
        #endregion

        private class ConnectionWrapper : TECConnection
        {
            private IProtocol protocol;
            public override IProtocol Protocol
            {
                get
                {
                    return protocol;
                }
            }
            
            public ConnectionWrapper(IProtocol protocol) : base()
            {
                this.protocol = protocol;
            }

            public ConnectionWrapper(Guid guid, IProtocol protocol) : base(guid)
            {
                this.protocol = protocol;
            }

            public ConnectionWrapper(TECConnection connectionSource, Dictionary<Guid, Guid> guidDictionary = null) : base(connectionSource, guidDictionary)
            {
                this.protocol = connectionSource.Protocol;
            }

            public void UpdateProtocol(IProtocol protocol)
            {
                this.protocol = protocol;
                raisePropertyChanged("Protocol");
            }
        }
    }
}
