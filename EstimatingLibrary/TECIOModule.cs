using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECIOModule : TECHardware, ICatalog<TECIOModule>
    {
        private const CostType COST_TYPE = CostType.TEC;

        private ObservableCollection<TECIO> _io;
        public ObservableCollection<TECIO> IO
        {
            get { return _io; }
            set
            {
                var old = IO;
                IO.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "IO");
                _io = value;
                IO.CollectionChanged += (sender, args) => collectionChanged(sender, args, "IO");
                notifyCombinedChanged(Change.Edit, "IO", this, value, old);
            }
        }

        public IOCollection IOCollection { get { return new IOCollection(IO); } }
        
        public TECIOModule(Guid guid, TECManufacturer manufacturer) : base(guid, manufacturer, COST_TYPE)
        {
            _io = new ObservableCollection<TECIO>();
        }
        public TECIOModule(TECManufacturer manufacturer) : this(Guid.NewGuid(), manufacturer) { }
        public TECIOModule(TECIOModule ioModuleSource) : this(ioModuleSource.Manufacturer)
        {
            copyPropertiesFromHardware(this);
            _io = ioModuleSource.IO;
        }

        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this,
                notifyCombinedChanged, notifyReorder: false);
        }

        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveMap = new SaveableMap();
            saveMap.AddRange(base.propertyObjects());
            saveMap.AddRange(IO, "IO");
            return saveMap;
        }

        public TECIOModule CatalogCopy()
        {
            return new TECIOModule(this);
        }
    }
}
