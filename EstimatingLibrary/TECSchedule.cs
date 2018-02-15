using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECSchedule : TECObject, IRelatable
    {
        private ObservableCollection<TECScheduleTable> _tables;
        public ObservableCollection<TECScheduleTable> Tables
        {
            get { return _tables; }
        }

        public SaveableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public SaveableMap LinkedObjects
        {
            get { return linkedObjects(); }
        }

        public TECSchedule(Guid guid, IEnumerable<TECScheduleTable> items) : base(guid)
        {
            _tables = new ObservableCollection<TECScheduleTable>(items);
            Tables.CollectionChanged += tables_collectionChanged;
        }
        public TECSchedule() : this(Guid.NewGuid(), new List<TECScheduleTable>()) { }

        private void tables_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach(TECScheduleTable item in e.NewItems)
                    notifyTECChanged(Change.Add, "Tables", this, item);
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TECScheduleTable item in e.OldItems)
                    notifyTECChanged(Change.Remove, "Tables", this, item);
            }
        }
        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.Tables, "Tables");
            return saveList;
        }
        private SaveableMap linkedObjects()
        {
            return new SaveableMap();
        }
        
    }
}
