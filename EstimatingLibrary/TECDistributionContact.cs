using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECDistributionContact : TECObject
    {
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                var old = Name;
                _name = value;
                notifyCombinedChanged(Change.Edit, "Name", this, value, old);
            }
        }

        public TECDistributionContact(Guid guid) : base(guid)
        {

        }
        public TECDistributionContact() : this(Guid.NewGuid()) { }        
    }
}
