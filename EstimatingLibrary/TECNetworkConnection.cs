using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECNetworkConnection : TECConnection, IControllerConnection
    {
        #region Properties
        //---Stored---
        private ObservableCollection<IConnectable> _children = new ObservableCollection<IConnectable>();

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
        public TECProtocol Protocol { get; }
        
        public override List<TECConnectionType> ConnectionTypes
        {
            get { return new List<TECConnectionType>(Protocol.ConnectionTypes); }
        }
        public override IOCollection IO
        {
            get
            {
                return new IOCollection(new List<TECIO> { Protocol.ToIO() });
            }
        }
        #endregion

        #region Constructors
        public TECNetworkConnection(Guid guid, TECController parent, TECProtocol protocol, bool isTypical) : base(guid, parent, isTypical)
        {
            Protocol = protocol;
            Children.CollectionChanged += Children_CollectionChanged;
        }
        public TECNetworkConnection(TECController parent, TECProtocol protocol, bool isTypical) : this(Guid.NewGuid(), parent, protocol, isTypical) { }
        public TECNetworkConnection(TECNetworkConnection connectionSource, TECController parent, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) 
            : base(connectionSource, parent, isTypical, guidDictionary)
        {
            Children.CollectionChanged += Children_CollectionChanged;
            foreach (IConnectable item in connectionSource.Children)
            {
                IConnectable newChild = item.Copy(isTypical, guidDictionary);
                newChild.SetParentConnection(this);
                _children.Add(newChild);
            }
            Protocol = connectionSource.Protocol;
        }
        #endregion

        #region Event Handlers
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, "Children", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, "Children", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, "Children", this, sender, sender);
            }
        }
        #endregion

        #region Methods
        public bool CanAddChild(IConnectable connectable)
        {
            return connectable.AvailableProtocols.Contains(this.Protocol.ToIO());
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

        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            List<TECObject> objects = new List<TECObject>();
            foreach(IConnectable netconnect in Children)
            {
                objects.Add(netconnect as TECObject);
            }
            saveList.AddRange(objects, "Children");
            saveList.Add(Protocol, "Protocol");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            List<TECObject> objects = new List<TECObject>();
            foreach (IConnectable netconnect in Children)
            {
                objects.Add(netconnect as TECObject);
            }
            saveList.AddRange(objects, "Children");
            saveList.Add(Protocol, "Protocol");
            return saveList;
        }
        
        #endregion 

    }
}
