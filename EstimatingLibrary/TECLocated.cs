using EstimatingLibrary.Interfaces;
using System;

namespace EstimatingLibrary
{
    public abstract class TECLocated : TECScope
    {
        #region Properties
        protected TECLocation _location;

        public TECLocation Location
        {
            get { return _location; }
            set
            {
                if(_location != value)
                {
                    var old = Location;
                    _location = value;
                    notifyCombinedChanged(Change.Edit, "Location", this, Location, old);
                }
            }
        }
        #endregion

        public TECLocated(Guid guid) : base(guid) { }

        #region Methods
        protected void copyPropertiesFromLocated(TECLocated scope)
        {
            copyPropertiesFromScope(scope);
            if (scope.Location != null)
            { _location = scope.Location; }
        }

        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            if(this.Location != null)
            {
                saveList.Add(this.Location, "Location");
            }
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            RelatableMap baseMap = base.linkedObjects();
            saveList.AddRange(baseMap);
            if (this.Location != null)
            {
                saveList.Add(this.Location, "Location");
            }
            return saveList;
        }
        #endregion
    }
}
