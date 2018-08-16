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
    public class TECHardwiredProtocol : TECObject, IProtocol, IEquatable<TECHardwiredProtocol>
    {
        public string Label => "Hardwired";

        public ObservableCollection<TECConnectionType> ConnectionTypes { get; }

        public TECHardwiredProtocol(IEnumerable<TECConnectionType> connectionTypes) : base(Guid.NewGuid())
        {
            this.ConnectionTypes = new ObservableCollection<TECConnectionType>(connectionTypes);
        }

        #region IProtocol
        List<TECConnectionType> IProtocol.ConnectionTypes
        {
            get { return new List<TECConnectionType>(this.ConnectionTypes); }
        }

        #endregion

        #region Equals Override

        public override bool Equals(object obj)
        {
            return Equals(obj as TECHardwiredProtocol);
        }

        public bool Equals(TECHardwiredProtocol other)
        {
            return other != null &&
                   EqualityComparer<ObservableCollection<TECConnectionType>>.Default.Equals(ConnectionTypes, other.ConnectionTypes);
        }

        public override int GetHashCode()
        {
            var hashCode = -1871948804;
            hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<TECConnectionType>>.Default.GetHashCode(ConnectionTypes);
            return hashCode;
        }

        public static bool operator ==(TECHardwiredProtocol protocol1, TECHardwiredProtocol protocol2)
        {
            return EqualityComparer<TECHardwiredProtocol>.Default.Equals(protocol1, protocol2);
        }

        public static bool operator !=(TECHardwiredProtocol protocol1, TECHardwiredProtocol protocol2)
        {
            return !(protocol1 == protocol2);
        }
        #endregion
    }
}
