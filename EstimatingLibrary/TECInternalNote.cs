using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECInternalNote : TECLabeled
    {

        private String _body = "";

        public String Body
        {
            get { return _body; }
            set
            {
                var old = Body;
                _body = value;
                notifyCombinedChanged(Change.Edit, "Body", this, value, old);
            }
        }

        public TECInternalNote() : this(Guid.NewGuid())
        {

        }
        public TECInternalNote(Guid guid) : base(guid)
        {

        }
    }
}
