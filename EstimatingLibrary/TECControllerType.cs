using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECControllerType : TECHardware, ICatalog<TECControllerType>, ICatalogContainer
    {
        private const CostType COST_TYPE = CostType.TEC;

        #region Properties

        public ObservableCollection<TECIO> IO { get; } = new ObservableCollection<TECIO>();
        public ObservableCollection<TECIOModule> IOModules { get; } = new ObservableCollection<TECIOModule>();
        #endregion

        public TECControllerType(Guid guid, TECManufacturer manufacturer) : base(guid, manufacturer, COST_TYPE)
        {
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
                IO.Add(new TECIO(io));
            }
            foreach (TECIOModule module in typeSource.IOModules)
            {
                IOModules.Add(module);
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

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyReplaced = base.RemoveCatalogItem(item, replacement);

            bool replacedMod = false;
            if (item is TECIOModule mod)
            {
                replacedMod = CommonUtilities.OptionallyReplaceAll(mod, this.IOModules, replacement as TECIOModule);
            }

            return (replacedMod || alreadyReplaced);
        }
        #endregion
    }
}
