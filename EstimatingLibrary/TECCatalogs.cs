using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECCatalogs : TECObject, IRelatable
    {
        private ObservableCollection<TECConnectionType> _connectionTypes = new ObservableCollection<TECConnectionType>();
        private ObservableCollection<TECElectricalMaterial> _conduitTypes = new ObservableCollection<TECElectricalMaterial>();
        private ObservableCollection<TECAssociatedCost> _associatedCosts = new ObservableCollection<TECAssociatedCost>();
        private ObservableCollection<TECPanelType> _panelTypes = new ObservableCollection<TECPanelType>();
        private ObservableCollection<TECControllerType> _controllerTypes = new ObservableCollection<TECControllerType>();
        private ObservableCollection<TECIOModule> _ioModules = new ObservableCollection<TECIOModule>();
        private ObservableCollection<TECDevice> _devices = new ObservableCollection<TECDevice>();
        private ObservableCollection<TECValve> _valves = new ObservableCollection<TECValve>();
        private ObservableCollection<TECManufacturer> _manufacturers = new ObservableCollection<TECManufacturer>();
        private ObservableCollection<TECTag> _tags = new ObservableCollection<TECTag>();
        private ObservableCollection<TECProtocol> _protocols = new ObservableCollection<TECProtocol>();

        public ObservableCollection<TECIOModule> IOModules
        {
            get { return _ioModules; }
            set
            {
                var old = IOModules;
                _ioModules = value;
                IOModules.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "IOModules");
                notifyCombinedChanged(Change.Edit, "IOModules", this, value, old);
            }
        }
        public ObservableCollection<TECDevice> Devices
        {
            get { return _devices; }
            set
            {
                var old = Devices;
                _devices = value;
                Devices.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Devices");
                notifyCombinedChanged(Change.Edit, "Devices", this, value, old);
            }
        }
        public ObservableCollection<TECValve> Valves
        {
            get { return _valves; }
            set
            {
                var old = Valves;
                _valves = value;
                Valves.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Valves");
                notifyCombinedChanged(Change.Edit, "Valves", this, value, old);
            }
        }
        public ObservableCollection<TECManufacturer> Manufacturers
        {
            get { return _manufacturers; }
            set
            {
                var old = Manufacturers;
                _manufacturers = value;
                Manufacturers.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Manufacturers");
                notifyCombinedChanged(Change.Edit, "Manufacturers", this, value, old);
            }
        }
        public ObservableCollection<TECPanelType> PanelTypes
        {
            get { return _panelTypes; }
            set
            {
                var old = PanelTypes;
                _panelTypes = value;
                PanelTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "PanelTypes");
                notifyCombinedChanged(Change.Edit, "PanelTypes", this, value, old);
            }
        }
        public ObservableCollection<TECControllerType> ControllerTypes
        {
            
            get { return _controllerTypes; }
            set
            {
                var old = ControllerTypes;
                _controllerTypes = value;
                ControllerTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ControllerTypes");
                notifyCombinedChanged(Change.Edit, "ControllerTypes", this, value, old);
            }
        }
        public ObservableCollection<TECConnectionType> ConnectionTypes
        {
            get { return _connectionTypes; }
            set
            {
                var old = ConnectionTypes;
                _connectionTypes = value;
                ConnectionTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConnectionTypes");
                notifyCombinedChanged(Change.Edit, "ConnectionTypes", this, value, old);
            }
        }
        public ObservableCollection<TECElectricalMaterial> ConduitTypes
        {
            get { return _conduitTypes; }
            set
            {
                var old = ConduitTypes;
                _conduitTypes = value;
                ConduitTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConduitTypes");
                notifyCombinedChanged(Change.Edit, "ConduitTypes", this, value, old);
            }
        }
        public ObservableCollection<TECAssociatedCost> AssociatedCosts
        {
            get { return _associatedCosts; }
            set
            {
                var old = AssociatedCosts;
                _associatedCosts = value;
                AssociatedCosts.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "AssociatedCosts");
                AssociatedCosts.CollectionChanged += ScopeChildren_CollectionChanged;
                notifyCombinedChanged(Change.Edit, "AssociatedCosts", this, value, old);
            }
        }
        public ObservableCollection<TECTag> Tags
        {
            get { return _tags; }
            set
            {
                var old = Tags;
                _tags = value;
                Tags.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Tags");
                Tags.CollectionChanged += ScopeChildren_CollectionChanged;
                notifyCombinedChanged(Change.Edit, "Tags", this, value, old);
            }
        }
        public ObservableCollection<TECProtocol> Protocols
        {
            get { return _protocols; }
            set
            {
                var old = Protocols;
                _protocols = value;
                Protocols.CollectionChanged += (sender, e) => CollectionChanged(sender, e,  "Protocols");
                notifyCombinedChanged(Change.Edit, "Protocols", this, value, old);
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

        public Action<TECObject> ScopeChildRemoved;

        public TECCatalogs() : base(Guid.NewGuid())
        {
            registerInitialCollectionChanges();
        }
        
        private void registerInitialCollectionChanges()
        {
            ConduitTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConduitTypes");
            ConnectionTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ConnectionTypes");
            AssociatedCosts.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "AssociatedCosts");
            PanelTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "PanelTypes");
            ControllerTypes.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "ControllerTypes");
            IOModules.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "IOModules");
            Devices.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Devices");
            Valves.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Valves");
            Manufacturers.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Manufacturers");
            Tags.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Tags");
            Protocols.CollectionChanged += (sender, e) => CollectionChanged(sender, e, "Protocols");

            AssociatedCosts.CollectionChanged += ScopeChildren_CollectionChanged;
            Tags.CollectionChanged += ScopeChildren_CollectionChanged;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, propertyName, this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, propertyName, this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, propertyName, this, sender, sender);
            }
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
        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
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
        private SaveableMap linkedObjects()
        {
            SaveableMap relatedList = new SaveableMap();
            return relatedList;
        }

        public void Unionize(TECCatalogs catalogToAdd)
        {
            unionizeScope(this.ConnectionTypes, catalogToAdd.ConnectionTypes);
            unionizeScope(this.ConduitTypes, catalogToAdd.ConduitTypes);
            unionizeScope(this.AssociatedCosts, catalogToAdd.AssociatedCosts);
            unionizeScope(this.PanelTypes, catalogToAdd.PanelTypes);
            unionizeScope(this.ControllerTypes, catalogToAdd.ControllerTypes);
            unionizeScope(this.IOModules, catalogToAdd.IOModules);
            unionizeScope(this.Devices, catalogToAdd.Devices);
            unionizeScope(this.Valves, catalogToAdd.Valves);
            unionizeScope(this.Manufacturers, catalogToAdd.Manufacturers);
            unionizeScope(this.Tags, catalogToAdd.Tags);
            unionizeScope(this.Protocols, catalogToAdd.Protocols);
        }
        private static void unionizeScope<T>(IList<T> bidItems, IList<T> templateItems) where T : TECObject
        {                       
            List<T> itemsToRemove = new List<T>();

            foreach (T templateItem in templateItems)
            {
                foreach (T item in bidItems)
                {
                    if (item.Guid == templateItem.Guid)
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }
            foreach (T item in itemsToRemove)
            {
                bidItems.Remove(item);
            }
            foreach (T item in templateItems)
            {
                bidItems.Add(item);
            }
        }

        public void Fill(TECCatalogs catalogToAdd)
        {
            fillScope(this.ConnectionTypes, catalogToAdd.ConnectionTypes);
            fillScope(this.ConduitTypes, catalogToAdd.ConduitTypes);
            fillScope(this.AssociatedCosts, catalogToAdd.AssociatedCosts);
            fillScope(this.PanelTypes, catalogToAdd.PanelTypes);
            fillScope(this.ControllerTypes, catalogToAdd.ControllerTypes);
            fillScope(this.IOModules, catalogToAdd.IOModules);
            fillScope(this.Devices, catalogToAdd.Devices);
            fillScope(this.Valves, catalogToAdd.Valves);
            fillScope(this.Manufacturers, catalogToAdd.Manufacturers);
            fillScope(this.Tags, catalogToAdd.Tags);
            fillScope(this.Protocols, catalogToAdd.Protocols);
        }
        private static void fillScope<T>(IList<T> bidItems, IList<T> templateItems) where T : TECObject
        {
            foreach(T templateItem in templateItems)
            {
                if(!bidItems.Any(item => item.Guid == templateItem.Guid))
                {
                    bidItems.Add(templateItem);
                }
            }
        }
    }
}
