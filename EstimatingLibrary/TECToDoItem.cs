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
        private string _url = "";
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
        public string URL
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    var old = _url;
                    _url = value;
                    notifyCombinedChanged(Change.Edit, "URL", this, value, old);
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

        public TECToDoItem(string desc, string url) : base(Guid.NewGuid())
        {
            this.Description = desc;
            this.URL = url;
        }
        public TECToDoItem(Guid guid) : base(guid) { }
    }
}
