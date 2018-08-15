using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECToDoItem : TECObject
    {
        private string _description = "";
        private bool _isDone = false;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    var old = _description;
                    _description = value;
                    notifyCombinedChanged(Change.Edit, "Description", this, value, old);
                }
            }
        }
        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    notifyCombinedChanged(Change.Edit, "IsDone", this, value, !value);
                }
            }
        }
        
        public TECToDoItem(Guid guid, string desc) : base(guid)
        {
            this.Description = desc;
        }
        public TECToDoItem(string desc) : this(Guid.NewGuid(), desc) { }
    }
}
