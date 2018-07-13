using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECControllerType : TECHardware, ICatalog<TECControllerType>
    {
        private const CostType COST_TYPE = CostType.TEC;

        #region Properties
        private ObservableCollection<TECIO> _io;
        private ObservableCollection<TECIOModule> _ioModules;
        
        public ObservableCollection<TECIO> IO
        {
            get { return _io; }
            set
            {
                var old = IO;
                IO.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "IO");
                _io = value;
                notifyCombinedChanged(Change.Edit,"IO", this, value, old);
                IO.CollectionChanged += (sender, args) => collectionChanged(sender, args, "IO");
            }
        }
        public ObservableCollection<TECIOModule> IOModules
        {
            get { return _ioModules; }
            set
            {
                var old = IOModules;
                IOModules.CollectionChanged -= (sender, args) => collectionChanged(sender, args, "IOModules");
                _ioModules = value;
                IOModules.CollectionChanged += (sender, args) => collectionChanged(sender, args, "IOModules");
                notifyCombinedChanged(Change.Edit, "IOModules", this, value, old);
            }
        }
        #endregion

        public TECControllerType(Guid guid, TECManufacturer manufacturer) : base(guid, manufacturer, COST_TYPE)
        {
            _io = new ObservableCollection<TECIO>();
            _ioModules = new ObservableCollection<TECIOModule>();
            IO.CollectionChanged += (sender, args) => collectionChanged(sender, args, "IO");
            IOModules.CollectionChanged += (sender, args) => collectionChanged(sender, args, "IOModules");

        }
        public TECControllerType(TECManufacturer manufacturer) : this(Guid.NewGuid(), manufacturer) { }
        public TECControllerType(TECControllerType typeSource) : this(typeSource.Manufacturer)
        {
            copyPropertiesFromHardware(typeSource);
            foreach (TECIO io in typeSource.IO)
            {
                TECIO ioToAdd = new TECIO(io);
                _io.Add(new TECIO(io));
            }
            foreach (TECIOModule module in typeSource.IOModules)
            {
                _ioModules.Add(module);
            }
        }

        #region Methods
        private void collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this,
                notifyCombinedChanged, notifyCostChanged);
        }

        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.IO, "IO");
            saveList.AddRange(this.IOModules, "IOModules");
            return saveList;
        }

        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(this.IOModules, "IOModules");
            return saveList;
        }

        public TECControllerType CatalogCopy()
        {
            return new TECControllerType(this);
        }
        #endregion

    }
}
