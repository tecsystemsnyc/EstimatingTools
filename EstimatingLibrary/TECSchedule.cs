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
    public class TECSchedule : TECObject, IRelatable
    {
        private ObservableCollection<TECScheduleTable> _tables;
        public ObservableCollection<TECScheduleTable> Tables
        {
            get { return _tables; }
        }

        public RelatableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public RelatableMap LinkedObjects
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
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Tables", this,
                notifyCombinedChanged, notifyReorder: false);
        }
        private RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(this.Tables, "Tables");
            return saveList;
        }
        private RelatableMap linkedObjects()
        {
            return new RelatableMap();
        }
        
    }
}
