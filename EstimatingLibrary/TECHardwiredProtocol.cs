using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECHardwiredProtocol : TECObject, IProtocol
    {

        public string Name => "Hardwired";

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
    }
}
