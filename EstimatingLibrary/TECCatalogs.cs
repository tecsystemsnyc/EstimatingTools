using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static EstimatingLibrary.Utilities.CommonUtilities;

namespace EstimatingLibrary
{
    public class TECCatalogs : TECObject, IRelatable, ICatalogContainer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ObservableCollection<TECIOModule> _ioModules = new ObservableCollection<TECIOModule>();
        private readonly ObservableCollection<TECDevice> _devices = new ObservableCollection<TECDevice>();
        private readonly ObservableCollection<TECValve> _valves = new ObservableCollection<TECValve>();
        private readonly ObservableCollection<TECManufacturer> _manufacturers = new ObservableCollection<TECManufacturer>();
        private readonly ObservableCollection<TECPanelType> _panelTypes = new ObservableCollection<TECPanelType>();
        private readonly ObservableCollection<TECControllerType> _controllerTypes = new ObservableCollection<TECControllerType>();
        private readonly ObservableCollection<TECConnectionType> _connectionTypes = new ObservableCollection<TECConnectionType>();
        private readonly ObservableCollection<TECElectricalMaterial> _conduitTypes = new ObservableCollection<TECElectricalMaterial>();
        private readonly ObservableCollection<TECAssociatedCost> _associatedCosts = new ObservableCollection<TECAssociatedCost>();
        private readonly ObservableCollection<TECTag> _tags = new ObservableCollection<TECTag>();
        private readonly ObservableCollection<TECProtocol> _protocols = new ObservableCollection<TECProtocol>();

        public ReadOnlyObservableCollection<TECIOModule> IOModules { get; }
        public ReadOnlyObservableCollection<TECDevice> Devices { get; }
        public ReadOnlyObservableCollection<TECValve> Valves { get; }
        public ReadOnlyObservableCollection<TECManufacturer> Manufacturers { get; }
        public ReadOnlyObservableCollection<TECPanelType> PanelTypes { get; }
        public ReadOnlyObservableCollection<TECControllerType> ControllerTypes { get; }
        public ReadOnlyObservableCollection<TECConnectionType> ConnectionTypes { get; }
        public ReadOnlyObservableCollection<TECElectricalMaterial> ConduitTypes { get; }
        public ReadOnlyObservableCollection<TECAssociatedCost> AssociatedCosts { get; }
        public ReadOnlyObservableCollection<TECTag> Tags { get; }
        public ReadOnlyObservableCollection<TECProtocol> Protocols { get; }

        public RelatableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public RelatableMap LinkedObjects
        {
            get { return linkedObjects(); }
        }

        public Action<TECObject> ScopeChildRemoved;

        public TECCatalogs() : base(Guid.NewGuid())
        {
            this.IOModules = new ReadOnlyObservableCollection<TECIOModule>(this._ioModules);
            this.Devices = new ReadOnlyObservableCollection<TECDevice>(this._devices);
            this.Valves = new ReadOnlyObservableCollection<TECValve>(this._valves);
            this.Manufacturers = new ReadOnlyObservableCollection<TECManufacturer>(this._manufacturers);
            this.PanelTypes = new ReadOnlyObservableCollection<TECPanelType>(this._panelTypes);
            this.ControllerTypes = new ReadOnlyObservableCollection<TECControllerType>(this._controllerTypes);
            this.ConnectionTypes = new ReadOnlyObservableCollection<TECConnectionType>(this._connectionTypes);
            this.ConduitTypes = new ReadOnlyObservableCollection<TECElectricalMaterial>(this._conduitTypes);
            this.AssociatedCosts = new ReadOnlyObservableCollection<TECAssociatedCost>(this._associatedCosts);
            this.Tags = new ReadOnlyObservableCollection<TECTag>(this._tags);
            this.Protocols = new ReadOnlyObservableCollection<TECProtocol>(this._protocols);

            registerInitialCollectionChanges();
        }
        
        private void registerInitialCollectionChanges()
        {
            _conduitTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConduitTypes");
            _connectionTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConnectionTypes");
            _associatedCosts.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "AssociatedCosts");
            _panelTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "PanelTypes");
            _controllerTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ControllerTypes");
            _ioModules.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "IOModules");
            _devices.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Devices");
            _valves.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Valves");
            _manufacturers.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Manufacturers");
            _tags.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Tags");
            _protocols.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Protocols");

            _associatedCosts.CollectionChanged += ScopeChildren_CollectionChanged;
            _tags.CollectionChanged += ScopeChildren_CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged);
        }

        private void ScopeChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TECObject item in e.OldItems)
                {
                    ScopeChildRemoved?.Invoke(item);
                }
            }
        }
        private RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(this.IOModules, "IOModules");
            saveList.AddRange(this.Devices, "Devices");
            saveList.AddRange(this.Valves, "Valves");
            saveList.AddRange(this.Manufacturers, "Manufacturers");
            saveList.AddRange(this.PanelTypes, "PanelTypes");
            saveList.AddRange(this.ControllerTypes, "ControllerTypes");
            saveList.AddRange(this.ConnectionTypes, "ConnectionTypes");
            saveList.AddRange(this.ConduitTypes, "ConduitTypes");
            saveList.AddRange(this.AssociatedCosts, "AssociatedCosts");
            saveList.AddRange(this.Tags, "Tags");
            saveList.AddRange(this.Protocols, "Protocols");
            return saveList;
        }
        private RelatableMap linkedObjects()
        {
            RelatableMap relatedList = new RelatableMap();
            return relatedList;
        }

        public void Unionize(TECCatalogs catalogToAdd)
        {
            UnionizeScopeCollection(this._connectionTypes, catalogToAdd.ConnectionTypes);
            UnionizeScopeCollection(this._conduitTypes, catalogToAdd.ConduitTypes);
            UnionizeScopeCollection(this._associatedCosts, catalogToAdd.AssociatedCosts);
            UnionizeScopeCollection(this._panelTypes, catalogToAdd.PanelTypes, setQuote);
            UnionizeScopeCollection(this._controllerTypes, catalogToAdd.ControllerTypes, setQuote);
            UnionizeScopeCollection(this._ioModules, catalogToAdd.IOModules, setQuote);
            UnionizeScopeCollection(this._devices, catalogToAdd.Devices, setQuote);
            UnionizeScopeCollection(this._valves, catalogToAdd.Valves, setQuote);
            UnionizeScopeCollection(this._manufacturers, catalogToAdd.Manufacturers);
            UnionizeScopeCollection(this._tags, catalogToAdd.Tags);
            UnionizeScopeCollection(this._protocols, catalogToAdd.Protocols);
           

            void setQuote(TECHardware original, TECHardware newItem){
                if (original.RequireQuote) newItem.RequireQuote = true;
                if (original.QuotedPrice != -1) newItem.QuotedPrice = original.QuotedPrice;
            }
        }

        public void Fill(TECCatalogs catalogToAdd)
        {
            FillScopeCollection(this._connectionTypes, catalogToAdd.ConnectionTypes);
            FillScopeCollection(this._conduitTypes, catalogToAdd.ConduitTypes);
            FillScopeCollection(this._associatedCosts, catalogToAdd.AssociatedCosts);
            FillScopeCollection(this._panelTypes, catalogToAdd.PanelTypes);
            FillScopeCollection(this._controllerTypes, catalogToAdd.ControllerTypes);
            FillScopeCollection(this._ioModules, catalogToAdd.IOModules);
            FillScopeCollection(this._devices, catalogToAdd.Devices);
            FillScopeCollection(this._valves, catalogToAdd.Valves);
            FillScopeCollection(this._manufacturers, catalogToAdd.Manufacturers);
            FillScopeCollection(this._tags, catalogToAdd.Tags);
            FillScopeCollection(this._protocols, catalogToAdd.Protocols);
        }

        public void Add<T>(T item) where T : ICatalog<T>
        {
            IList<T> collection = getCollectionForObject(item);
            if (collection != null)
            {
                collection.Add(item);
            } 
            else
            {
                logger.Error("Collection for catalog item not found. Item: {0}",
                    item);
            }
        }
        public void AddRange<T>(IEnumerable<T> range) where T : ICatalog<T>
        {
            if (range.Count() < 1) return;
            IList<T> collection = getCollectionForObject(range.First());
            if (collection != null)
            {
                range.ForEach(item => collection.Add(item));
            }
            else
            {
                logger.Error("Collection for catalog range not found.");
            }
        }

        private IList<T> getCollectionForObject<T>(T obj) where T : ICatalog<T>
        {
            return (IList<T>)getCatalogCollections().FirstOrDefault(col => col is IList<T>);
        }

        private List<IList> getCatalogCollections()
        {
            return new List<IList>()
            {
                this._ioModules,
                this._devices,
                this._valves,
                this._manufacturers,
                this._panelTypes,
                this._controllerTypes,
                this._connectionTypes,
                this._conduitTypes,
                this._associatedCosts,
                this._tags,
                this._protocols
            };
        }

        bool ICatalogContainer.RemoveCatalogItem<T>(T item, T replacement)
        {
            bool replacedItem = false;
            foreach(IList collection in getCatalogCollections())
            {

            }
        }
    }
}
