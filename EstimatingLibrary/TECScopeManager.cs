using EstimatingLibrary.Interfaces;
using System;

namespace EstimatingLibrary
{
    public abstract class TECScopeManager : TECObject, IRelatable
    {
        #region Properties
        private TECScopeTemplates _templates = new TECScopeTemplates();
        public TECScopeTemplates Templates
        {
            get { return _templates; }
            set
            {
                var old = Templates;
                _templates = value;
                notifyCombinedChanged(Change.Edit, "Templates", this, value, old);
            }
        }

        protected TECCatalogs _catalogs = new TECCatalogs();
        virtual public TECCatalogs Catalogs
        {
            get { return _catalogs; }
            set
            {
                var old = Catalogs;
                _catalogs = value;
                notifyCombinedChanged(Change.Edit, "Catalogs", this, value, old);
            }
        }

        #endregion

        protected TECScopeManager(Guid guid): base(guid) { }
        protected TECScopeManager() : this(Guid.NewGuid()) { }

        public 

        #region IRelatable

        public RelatableMap PropertyObjects { get { return propertyObjects(); } }
        public RelatableMap LinkedObjects { get { return new RelatableMap(); } }

        protected virtual RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.Add(this.Catalogs, "Catalogs");
            saveList.Add(this.Templates, "Templates");
            return saveList;
        }
        #endregion

    }
}
