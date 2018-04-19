using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECHardwiredConnection : TECConnection
    {
        #region Properties
        public IConnectable Child { get; }

        //---Derived---
        public override List<TECConnectionType> ConnectionTypes
        {
            get
            {
                return Child.RequiredConnectionTypes;
            }
        }

        public override IOCollection IO
        {
            get
            {
                return Child.HardwiredIO;
            }
        }
        #endregion

        #region Constructors
        public TECHardwiredConnection(Guid guid, IConnectable child, TECController controller, bool isTypical) : base(guid, controller, isTypical)
        {
            Child = child;
            child.SetParentConnection(this);
        }
        public TECHardwiredConnection(IConnectable child, TECController parent, bool isTypical) : this(Guid.NewGuid(), child, parent, isTypical) { }
        public TECHardwiredConnection(TECHardwiredConnection connectionSource, TECController parent, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) 
            : base(connectionSource, parent, isTypical, guidDictionary)
        {
            Child = connectionSource.Child.Copy(isTypical, guidDictionary);
            Child.SetParentConnection(this);
        }
        public TECHardwiredConnection(TECHardwiredConnection linkingSource, IConnectable child, bool isTypical) : base(linkingSource, linkingSource.ParentController, isTypical)
        {
            Child = child;
            child.SetParentConnection(this);
            _guid = linkingSource.Guid;
        }
        #endregion Constructors

        #region Methods
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Child as TECObject, "Child");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Child as TECObject, "Child");
            return saveList;
        }
        #endregion
    }
}
