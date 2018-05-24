using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECInterlockConnection : TECScope, IConnection, INotifyCostChanged, IRelatable
    {
        private readonly TECConnection connection;
        
        public TECInterlockConnection(Guid guid, IProtocol protocol, bool isTypical) : base(guid)
        {
            this.connection = new TECConnection(guid, protocol, isTypical);
            subscribeToConnection();
        }

        public TECInterlockConnection(IProtocol protocol, bool isTypical) : this(Guid.NewGuid(), protocol, isTypical)
        { }
        public TECInterlockConnection(TECInterlockConnection source, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(source.Guid)
        {
            this.connection = new TECConnection(source.connection, isTypical, guidDictionary);
            subscribeToConnection();
        }

        private void subscribeToConnection()
        {
            this.connection.CostChanged += notifyCostChanged;
            this.connection.TECChanged += (args) => notifyTECChanged(args.Change, args.PropertyName, args.Sender, args.Value, args.OldValue);
            this.connection.PropertyChanged += (sender, args) => raisePropertyChanged(args.PropertyName);
        }

        #region IConnection
        public double ConduitLength { get => ((IConnection)connection).ConduitLength; set => ((IConnection)connection).ConduitLength = value; }
        public TECElectricalMaterial ConduitType { get => ((IConnection)connection).ConduitType; set => ((IConnection)connection).ConduitType = value; }
        public bool IsPlenum { get => ((IConnection)connection).IsPlenum; set => ((IConnection)connection).IsPlenum = value; }
        public double Length { get => ((IConnection)connection).Length; set => ((IConnection)connection).Length = value; }

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
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(connection.PropertyObjects);
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(connection.PropertyObjects);
            return saveList;
        }
        #endregion
    }
}
