using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECProtocol : TECObject, IRelatable
    {
        private String _name = "";
        private ObservableCollection<TECConnectionType> _connectionTypes = new ObservableCollection<TECConnectionType>();

        public String Name
        {
            get { return _name; }
            set
            {
                if (value != Name)
                {
                    var old = Name;
                    _name = value;
                    notifyCombinedChanged(Change.Edit, "Name", this, value, old);
                }
            }
        }
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

        public SaveableMap PropertyObjects {
            get
            {
                SaveableMap map = new SaveableMap();
                map.AddRange(this.ConnectionTypes, "ConnectionTypes");
                return map;
            }
        }

        public SaveableMap LinkedObjects
        {
            get
            {
                SaveableMap map = new SaveableMap();
                map.AddRange(this.ConnectionTypes, "ConnectionTypes");
                return map;
            }
        }

        public TECProtocol(Guid guid) : base(guid)
        {
            ConnectionTypes.CollectionChanged += connectionTypes_CollectionChanged;
        }
        public TECProtocol() : this(Guid.NewGuid()) { }
        
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
                notifyCombinedChanged(Change.Edit, propertyName, this, sender, sender);
            }
        }

    }
}
