using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECInterlockConnection : TECScope, IConnection
    {
        private TECConnection connection;
        
        public TECInterlockConnection(Guid guid, IProtocol protocol, bool isTypical) : base(guid)
        {
            this.connection = new TECConnection(guid, protocol, isTypical);
        }
        public TECInterlockConnection(IProtocol protocol, bool isTypical) : this(Guid.NewGuid(), protocol, isTypical)
        { }
        public TECInterlockConnection(TECInterlockConnection source, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(source.Guid)
        {
            this.connection = new TECConnection(source.connection, isTypical, guidDictionary);
        }

        #region IConnection
        public double ConduitLength { get => ((IConnection)connection).ConduitLength; set => ((IConnection)connection).ConduitLength = value; }
        public TECElectricalMaterial ConduitType { get => ((IConnection)connection).ConduitType; set => ((IConnection)connection).ConduitType = value; }
        public bool IsPlenum { get => ((IConnection)connection).IsPlenum; set => ((IConnection)connection).IsPlenum = value; }
        public double Length { get => ((IConnection)connection).Length; set => ((IConnection)connection).Length = value; }

        public IProtocol Protocol => ((IConnection)connection).Protocol;
        #endregion
    }
}
