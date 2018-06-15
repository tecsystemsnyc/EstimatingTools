using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECHardwiredConnection : TECConnection, IControllerConnection
    {
        #region Properties
        private TECController _parentController;

        public IConnectable Child { get; }

        public List<TECConnectionType> ConnectionTypes { get; }

        public TECController ParentController
        {
            get { return _parentController; }
            set
            {
                _parentController = value;
                raisePropertyChanged("ParentController");
            }
        }

        public IOCollection IO => Child.HardwiredIO;
        public override IProtocol Protocol => new TECHardwiredProtocol(ConnectionTypes);


        #endregion

        #region Constructors
        public TECHardwiredConnection(Guid guid, IConnectable child, TECController controller, TECHardwiredProtocol protocol, bool isTypical) : base(guid, isTypical)
        {
            Child = child;
            ConnectionTypes = new List<TECConnectionType>(protocol.ConnectionTypes);
            ParentController = controller;
            child.SetParentConnection(this);
        }
        public TECHardwiredConnection(IConnectable child, TECController parent, TECHardwiredProtocol protocol, bool isTypical) : this(Guid.NewGuid(), child, parent, protocol, isTypical) { }
        public TECHardwiredConnection(TECHardwiredConnection connectionSource, TECController parent, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) 
            : base(connectionSource, isTypical, guidDictionary)
        {
            Child = connectionSource.Child.Copy(isTypical, guidDictionary);
            ParentController = parent;
            ConnectionTypes = new List<TECConnectionType>(connectionSource.ConnectionTypes);
            Child.SetParentConnection(this);
        }
        public TECHardwiredConnection(TECHardwiredConnection linkingSource, IConnectable child, bool isTypical) : base(linkingSource, isTypical)
        {
            Child = child;
            ParentController = linkingSource.ParentController;
            ConnectionTypes = linkingSource.ConnectionTypes;
            child.SetParentConnection(this);
            _guid = linkingSource.Guid;
            
        }
        #endregion Constructors

        #region Methods
        public void UpdatePropertiesBasedOn(TECInterlockConnection basis)
        {
            this.Length = basis.Length;
            this.ConduitLength = basis.ConduitLength;
            this.ConduitType = basis.ConduitType;
            this.IsPlenum = basis.IsPlenum;
        }

        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Child, "Child");
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Child, "Child");
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        #endregion
    }
}
