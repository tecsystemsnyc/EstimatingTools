using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECNetworkConnection : TECConnection
    {
        #region Properties
        //---Stored---
        private ObservableCollection<INetworkConnectable> _children = new ObservableCollection<INetworkConnectable>();
        private TECProtocol _protocol;

        public ObservableCollection<INetworkConnectable> Children
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
        public TECProtocol Protocol
        {
            get { return _protocol; }
            set
            {
                var old = Protocol;
                _protocol = value;
                notifyCombinedChanged(Change.Edit, "Protocol", this, value, old);
            }
        }
        
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
        public TECNetworkConnection(Guid guid, bool isTypical) : base(guid, isTypical)
        {
            Children.CollectionChanged += Children_CollectionChanged;
        }
        public TECNetworkConnection(bool isTypical) : this(Guid.NewGuid(), isTypical) { }
        public TECNetworkConnection(TECNetworkConnection connectionSource, bool isTypical, Dictionary<Guid, Guid> guidDictionary = null) : base(connectionSource, isTypical, guidDictionary)
        {
            Children.CollectionChanged += Children_CollectionChanged;
            foreach (INetworkConnectable item in connectionSource.Children)
            {
                _children.Add(item.Copy(item, isTypical, guidDictionary));
            }
            _protocol = connectionSource.Protocol;
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
        public bool CanAddINetworkConnectable(INetworkConnectable connectable)
        {
            return connectable.AvailableProtocols.Contains(this.Protocol.ToIO());
        }
        public void AddINetworkConnectable(INetworkConnectable connectable)
        {
            if (CanAddINetworkConnectable(connectable))
            {
                connectable.ParentConnection = this;
                Children.Add(connectable);
            }
            else
            {
                throw new InvalidOperationException("Connectable not compatible with Network Connection.");
            }
        }
        public void RemoveINetworkConnectable(INetworkConnectable connectable)
        {
            if (Children.Contains(connectable))
            {
                Children.Remove(connectable);
                connectable.ParentConnection = null;
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
            foreach(INetworkConnectable netconnect in Children)
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
            foreach (INetworkConnectable netconnect in Children)
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
