using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECDevice : TECHardware, IDDCopiable, IEndDevice, ICatalog<TECDevice>, ICatalogContainer
    {
        #region Constants
        private const CostType COST_TYPE = CostType.TEC;
        #endregion

        public ObservableCollection<TECConnectionType> HardwiredConnectionTypes { get; }
        public ObservableCollection<TECProtocol> PossibleProtocols { get; }

        #region Constructors
        public TECDevice(Guid guid,
            IEnumerable<TECConnectionType> hardwiredConnectionTypes,
            IEnumerable<TECProtocol> possibleProtocols,
            TECManufacturer manufacturer) : base(guid, manufacturer, COST_TYPE)
        {
            this.HardwiredConnectionTypes = new ObservableCollection<TECConnectionType>(hardwiredConnectionTypes);
            this.PossibleProtocols = new ObservableCollection<TECProtocol>(possibleProtocols);

            this.HardwiredConnectionTypes.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "HardwiredConnectionTypes");
            this.PossibleProtocols.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "PossibleProtocols");
        }
        public TECDevice(IEnumerable<TECConnectionType> connectionTypes,
            IEnumerable<TECProtocol> possibleProtocols,
            TECManufacturer manufacturer) : this(Guid.NewGuid(), connectionTypes, possibleProtocols, manufacturer) { }

        //Copy Constructor
        public TECDevice(TECDevice deviceSource)
            : this(deviceSource.HardwiredConnectionTypes, deviceSource.PossibleProtocols, deviceSource.Manufacturer)
        {
            this.copyPropertiesFromHardware(deviceSource);
        }
        #endregion //Constructors

        #region Methods
        public new Object DragDropCopy(TECScopeManager scopeManager)
        {
            foreach(TECDevice device in scopeManager.Catalogs.Devices)
            {
                if(device.Guid == this.Guid)
                {
                    return device;
                }
            }
            throw new Exception();
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this,
                notifyCombinedChanged, notifyReorder: true);
        }

        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.HardwiredConnectionTypes, "HardwiredConnectionTypes");
            saveList.AddRange(this.PossibleProtocols, "PossibleProtocols");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(this.HardwiredConnectionTypes, "HardwiredConnectionTypes");
            saveList.AddRange(this.PossibleProtocols, "PossibleProtocols");
            return saveList;
        }

        public TECDevice CatalogCopy()
        {
            return new TECDevice(this);
        }
        #endregion

        #region IEndDevice
        List<IProtocol> IEndDevice.ConnectionMethods
        {
            get
            {
                List<IProtocol> connectionMethods = new List<IProtocol>(PossibleProtocols);
                if (HardwiredConnectionTypes.Count > 0) connectionMethods.Add(new TECHardwiredProtocol(HardwiredConnectionTypes));
                return connectionMethods;
            }
        }
        #endregion

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool removedConnectionType = false;
            bool removedProtocol = false;
            if (item is TECConnectionType type)
            {
                removedConnectionType = CommonUtilities.OptionallyReplaceAll(type, this.HardwiredConnectionTypes, replacement as TECConnectionType);
            }
            else if (item is TECProtocol prot)
            {
                removedProtocol = CommonUtilities.OptionallyReplaceAll(prot, this.PossibleProtocols, replacement as TECProtocol);
            }

            return (removedConnectionType || removedProtocol || alreadyRemoved);
        }
        #endregion
    }
}
