using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECNetworkConnection : TECConnection, IControllerConnection, ICatalogContainer
    {
        #region Properties
        //---Stored---
        private ObservableCollection<IConnectable> _children = new ObservableCollection<IConnectable>();
        private TECController _parentController;
        private TECProtocol protocol;

        public ObservableCollection<IConnectable> Children
        {
            get { return _children; }
            set
            {
                var old = Children;
                Children.CollectionChanged -= Children_CollectionChanged;
                _children = value;
                Children.CollectionChanged += Children_CollectionChanged;
                notifyCombinedChanged(Change.Edit, "Children", this, value, old);
            }
        }
        public TECController ParentController
        {
            get { return _parentController; }
            set
            {
                _parentController = value;
                raisePropertyChanged("ParentController");
            }
        }

        public IOCollection IO => protocol.ToIOCollection();

        public override IProtocol Protocol => protocol;
        public TECProtocol NetworkProtocol => protocol;

        public bool IsTypical { get; private set; }
        #endregion

        #region Constructors
        public TECNetworkConnection(Guid guid, TECController parent, TECProtocol protocol) : base(guid)
        {
            this.IsTypical = false;
            ParentController = parent;
            this.protocol = protocol;
            Children.CollectionChanged += Children_CollectionChanged;
        }
        public TECNetworkConnection(TECController parent, TECProtocol protocol) : this(Guid.NewGuid(), parent, protocol) { }
        public TECNetworkConnection(TECNetworkConnection connectionSource, TECController parent, Dictionary<Guid, Guid> guidDictionary = null) 
            : base(connectionSource, guidDictionary)
        {
            Children.CollectionChanged += Children_CollectionChanged;
            foreach (IConnectable item in connectionSource.Children)
            {
                IConnectable newChild = item.Copy(guidDictionary);
                newChild.SetParentConnection(this);
                _children.Add(newChild);
            }
            ParentController = parent;
            this.protocol = connectionSource.protocol;
        }
        #endregion

        #region Event Handlers
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Children", this,
                notifyCombinedChanged);
        }

        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!this.IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }
        #endregion

        #region Methods
        public bool CanAddChild(IConnectable connectable)
        {
            return connectable.AvailableProtocols.Contains(this.Protocol);
        }
        public void AddChild(IConnectable connectable)
        {
            if (CanAddChild(connectable))
            {
                connectable.SetParentConnection(this);
                Children.Add(connectable);
            }
            else
            {
                throw new InvalidOperationException("Connectable not compatible with Network Connection.");
            }
        }
        public void RemoveChild(IConnectable connectable)
        {
            if (Children.Contains(connectable))
            {
                Children.Remove(connectable);
                connectable.SetParentConnection(null);
            }
            else
            {
                throw new InvalidOperationException("INetworkConnectable doesn't exist in Network Connection.");
            }
        }

        public void SetProtocol(TECProtocol prot)
        {
            if (this.protocol != prot)
            {
                var old = this.protocol;
                var originalCost = CostBatch;
                this.protocol = prot;
                notifyCombinedChanged(Change.Edit, "Protocol", this, prot, old);
                notifyCostChanged(CostBatch - originalCost);
            }
        }
        
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(Children, "Children");
            saveList.Add(protocol, "Protocol");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(Children, "Children");
            saveList.Add(protocol, "Protocol");
            return saveList;
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
                //Can be typical, but is not kept in sync.
                return null;
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            if (property == "Children" && item is IConnectable child) { }
            else
            {
                throw new Exception(String.Format("There is no compatible add method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            if (property == "Children" && item is IConnectable child) {
                return true;
            }
            else
            {
                throw new Exception(String.Format("There is no compatible remove method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            if (property == "Children" && item is IConnectable child)
            {
                return Children.Contains(child);
            }
            else
            {
                throw new Exception(String.Format("There is no compatible property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
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

            bool replacedProt = false;
            if (item == this.protocol)
            {
                if (replacement is TECProtocol prot)
                {
                    SetProtocol(prot);
                }
                else throw new ArgumentException("Replacement Protocol cannot be null.");
            }
            return (replacedProt || alreadyRemoved);
        }
        #endregion
    }
}
