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
            return this.Equals(obj as TECHardwiredProtocol);
        }

        public bool Equals(TECHardwiredProtocol other)
        {
            if(other == null)
            {
                return false;
            }
            bool sameConnectionTypes = ConnectionTypes.SequenceEqual(other.ConnectionTypes);
            return other != null && ConnectionTypes.SequenceEqual(other.ConnectionTypes);
        }

        public override int GetHashCode()
        {
            var hashCode = -1871948804;
            var typeList = new List<TECConnectionType>(ConnectionTypes);
            typeList.OrderBy(x => x.Guid);
            foreach(TECConnectionType type in typeList)
            {
                hashCode = hashCode * -1521134295 + type.GetHashCode();
            }
            return hashCode;
        }
        
        public static bool operator ==(TECHardwiredProtocol protocol1, TECHardwiredProtocol protocol2)
        {
            if (object.ReferenceEquals(protocol1, null))
            {
                return object.ReferenceEquals(protocol2, null);
            }

            return protocol1.Equals(protocol2);
        }

        public static bool operator !=(TECHardwiredProtocol protocol1, TECHardwiredProtocol protocol2)
        {
            return !(protocol1 == protocol2);
        }
        #endregion
    }
    

}
