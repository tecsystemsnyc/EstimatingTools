using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECScheduleTable : TECObject, IRelatable
    {
        private ObservableCollection<TECScheduleItem> _items;
        protected string _name;

        public ObservableCollection<TECScheduleItem> Items
        {
            get { return _items; }
        }
        public string Name
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

        public SaveableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public SaveableMap LinkedObjects
        {
            get { return linkedObjects(); }
        }

        public TECScheduleTable(Guid guid, IEnumerable<TECScheduleItem> items) : base(guid)
        {
            _items = new ObservableCollection<TECScheduleItem>(items);
            Items.CollectionChanged += items_collectionChanged;
        }
        public TECScheduleTable() : this(Guid.NewGuid(), new List<TECScheduleItem>()) { }
        
        private void items_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TECScheduleItem item in e.NewItems)
                    notifyTECChanged(Change.Add, "Items", this, item);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TECScheduleItem item in e.OldItems)
                    notifyTECChanged(Change.Remove, "Items", this, item);
            }
        }
        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.Items, "Items");
            return saveList;
        }
        private SaveableMap linkedObjects()
        {
            return new SaveableMap();
        }
    }
}
