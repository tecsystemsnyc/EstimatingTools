﻿using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECHardwiredConnection : TECConnection, IControllerConnection, ICatalogContainer
    {
        #region Properties
        private TECController _parentController;

        public IConnectable Child { get; }

        public ObservableCollection<TECConnectionType> ConnectionTypes { get; }

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
        
        public bool IsTypical { get; private set; }
        #endregion

        #region Constructors
        public TECHardwiredConnection(Guid guid, IConnectable child, TECController controller, TECHardwiredProtocol protocol) : base(guid)
        {
            this.IsTypical = false;
            Child = child;
            ConnectionTypes = new ObservableCollection<TECConnectionType>(protocol.ConnectionTypes);
            ConnectionTypes.CollectionChanged += connectionTypesCollectionChanged;
            ParentController = controller;
            child.SetParentConnection(this);
        }

        private void connectionTypesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "ConnectionTypes", this, notifyCombinedChanged, notifyCostChanged);
        }

        public TECHardwiredConnection(IConnectable child, TECController parent, TECHardwiredProtocol protocol) : this(Guid.NewGuid(), child, parent, protocol) { }
        public TECHardwiredConnection(TECHardwiredConnection connectionSource, TECController parent, Dictionary<Guid, Guid> guidDictionary = null) 
            : base(connectionSource, guidDictionary)
        {
            Child = connectionSource.Child.Copy(guidDictionary);
            ParentController = parent;
            ConnectionTypes = new ObservableCollection<TECConnectionType>(connectionSource.ConnectionTypes);
            ConnectionTypes.CollectionChanged += connectionTypesCollectionChanged;
            Child.SetParentConnection(this);
        }
        public TECHardwiredConnection(TECHardwiredConnection linkingSource, IConnectable child, bool isTypical) : base(linkingSource)
        {
            Child = child;
            ParentController = linkingSource.ParentController;
            ConnectionTypes = linkingSource.ConnectionTypes;
            child.SetParentConnection(this);
            _guid = linkingSource.Guid;
            
        }
        #endregion Constructors

        #region Methods

        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Child, "Child");
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Child, "Child");
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            return saveList;
        }

        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!this.IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }
        #endregion

        #region ITypicalable

        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                //Can be typical, but is not kept in sync
                return null;
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible add method for the property {0} with an object of type {1}", property, item.GetType().ToString()));

        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible remove method for the property {0} with an object of type {1}", property, item.GetType().ToString()));

        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            throw new Exception(String.Format("There is no compatible property {0} with an object of type {1}", property, item.GetType().ToString()));

        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
        }
        #endregion

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool removedConnectionType = false;
            if (item is TECConnectionType type)
            {
                removedConnectionType = CommonUtilities.OptionallyReplaceAll(type, this.ConnectionTypes, replacement as TECConnectionType);
            }

            return (removedConnectionType || alreadyRemoved);
        }
        #endregion
    }
}
