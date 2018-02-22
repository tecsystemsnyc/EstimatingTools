using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECLocation : TECLabeled
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    string oldName = _name;
                    _name = value;
                    notifyCombinedChanged(Change.Edit, "Name", this, value, oldName);
                }
            }
        }

        public TECLocation(Guid guid) : base(guid)
        {
            _name = "";
        }
        public TECLocation() : this(Guid.NewGuid()) { }
        public TECLocation(TECLocation source) : base(source)
        {
            _name = source.Name;
        }
    }
}
