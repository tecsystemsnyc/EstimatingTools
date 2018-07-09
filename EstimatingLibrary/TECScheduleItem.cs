using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECScheduleItem : TECObject, IRelatable
    {
        private String _tag = "";
        private TECLocation _location = null;
        private String _service = "";
        private TECScope _scope = null;

        public String Tag
        {
            get { return _tag; }
            set
            {
                if(value != _tag)
                {
                    var old = _tag;
                    _tag = value;
                    notifyCombinedChanged(Change.Edit, "Tag", this, value, old);
                }
            }
        }
        public TECLocation Location
        {
            get { return _location; }
            set
            {
                if(value != _location)
                {
                    var old = _location;
                    _location = value;
                    notifyCombinedChanged(Change.Edit, "Location", this, value, old);
                }
            }
        }
        public String Service
        {
            get { return _service; }
            set
            {
                if(value != _service)
                {
                    var old = _service;
                    _service = value;
                    notifyCombinedChanged(Change.Edit, "Service", this, value, old);
                }                
            }
        }
        public TECScope Scope
        {
            get { return _scope; }
            set
            {
                if (value != _scope)
                {
                    var old = _scope;
                    _scope = value;
                    notifyCombinedChanged(Change.Edit, "Scope", this, value, old);
                }
            }
        }

        public SaveableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public SaveableMap LinkedObjects
        {
            get { return linkedObjects(); }
        }

        public TECScheduleItem(Guid guid) : base(guid) { }
        public TECScheduleItem() : this(Guid.NewGuid()) { }

        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            if (this.Scope != null)
            {
                saveList.Add(this.Scope, "Scope");
            }
            if(this.Location !=null)
            {
                saveList.Add(this.Location, "Location");
            }
            return saveList;
        }
        private SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            if (this.Scope != null)
            {
                saveList.Add(this.Scope, "Scope");
            }
            if (this.Location != null)
            {
                saveList.Add(this.Location, "Location");
            }
            return saveList;
        }

    }
}
