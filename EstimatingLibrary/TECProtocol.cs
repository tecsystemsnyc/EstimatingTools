using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECProtocol : TECLabeled, IRelatable, IProtocol, ICatalogContainer, ICatalog<TECProtocol>
    {
        private ObservableCollection<TECConnectionType> _connectionTypes;
        
        public ObservableCollection<TECConnectionType> ConnectionTypes
        {
            get { return _connectionTypes; }
            set
            {
                if (ConnectionTypes != null)
                {
                    ConnectionTypes.CollectionChanged -= connectionTypes_CollectionChanged;
                }
                var old = ConnectionTypes;
                _connectionTypes = value;
                if (ConnectionTypes != null)
                {
                    ConnectionTypes.CollectionChanged += connectionTypes_CollectionChanged;
                }
                notifyCombinedChanged(Change.Edit, "ConnectionTypes", this, value, old);
            }
        }

        public RelatableMap PropertyObjects {
            get
            {
                RelatableMap map = new RelatableMap();
                map.AddRange(this.ConnectionTypes, "ConnectionTypes");
                return map;
            }
        }

        public RelatableMap LinkedObjects
        {
            get
            {
                RelatableMap map = new RelatableMap();
                map.AddRange(this.ConnectionTypes, "ConnectionTypes");
                return map;
            }
        }

        public TECProtocol(Guid guid, IEnumerable<TECConnectionType> connectionTypes) : base(guid)
        {
            _connectionTypes = new ObservableCollection<TECConnectionType>(connectionTypes);
            ConnectionTypes.CollectionChanged += connectionTypes_CollectionChanged;
        }
        public TECProtocol(IEnumerable<TECConnectionType> connectionTypes) : this(Guid.NewGuid(), connectionTypes) { }
        
        private void connectionTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, "ConnectionTypes", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, "ConnectionTypes", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, "ConnectionTypes", this, sender, sender);
            }
        }

        #region IProtocol
        List<TECConnectionType> IProtocol.ConnectionTypes
        {
            get { return new List<TECConnectionType>(this.ConnectionTypes); }
        }
        #endregion

        #region ICatalog
        public TECProtocol CatalogCopy()
        {
            return new TECProtocol(this.Guid, this.ConnectionTypes);
        }
        #endregion

        #region ICatalogContainer
        public bool RemoveCatalogItem<T>(T item, T replacement) where T : class, ICatalog<T>
        {
            bool removedConnectionType = false;
            if (item is TECConnectionType type)
            {
                removedConnectionType = CommonUtilities.OptionallyReplaceAll(type, this.ConnectionTypes, replacement as TECConnectionType);
            }
            return removedConnectionType;
        }
        #endregion
    }
}
