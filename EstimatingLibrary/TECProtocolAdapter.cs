using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECProtocolAdapter : TECHardware, IEndDevice
    {
        #region Constants
        private const CostType COST_TYPE = CostType.TEC;
        #endregion

        private TECProtocol _protocol;
        public TECProtocol Protocol
        {
            get { return _protocol; }
            set
            {
                if (_protocol != value)
                {
                    var old = Manufacturer;
                    _protocol = value;
                    notifyCombinedChanged(Change.Edit, "Protocol", this, value, old);
                }
            }
        }

        public ObservableCollection<TECConnectionType> ConnectionTypes
        {
            get { return Protocol.ConnectionTypes; }
        }

        public TECProtocolAdapter(Guid guid, TECManufacturer manufacturer, TECProtocol protocol) : base(guid, manufacturer, COST_TYPE)
        {
            _protocol = protocol;
        }
        public TECProtocolAdapter(TECManufacturer manufacturer, TECProtocol protocol) : this(Guid.NewGuid(), manufacturer, protocol) { }
    }
}
