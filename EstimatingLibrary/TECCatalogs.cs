using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static EstimatingLibrary.Utilities.CommonUtilities;

namespace EstimatingLibrary
{
    public class TECCatalogs : TECObject, IRelatable
    {
        public ObservableCollection<TECIOModule> IOModules { get; } = new ObservableCollection<TECIOModule>();
        public ObservableCollection<TECDevice> Devices { get; } = new ObservableCollection<TECDevice>();
        public ObservableCollection<TECValve> Valves { get; } = new ObservableCollection<TECValve>();
        public ObservableCollection<TECManufacturer> Manufacturers { get; } = new ObservableCollection<TECManufacturer>();
        public ObservableCollection<TECPanelType> PanelTypes { get; } = new ObservableCollection<TECPanelType>();
        public ObservableCollection<TECControllerType> ControllerTypes { get; } = new ObservableCollection<TECControllerType>();
        public ObservableCollection<TECConnectionType> ConnectionTypes { get; } = new ObservableCollection<TECConnectionType>();
        public ObservableCollection<TECElectricalMaterial> ConduitTypes { get; } = new ObservableCollection<TECElectricalMaterial>();
        public ObservableCollection<TECAssociatedCost> AssociatedCosts { get; } = new ObservableCollection<TECAssociatedCost>();
        public ObservableCollection<TECTag> Tags { get; } = new ObservableCollection<TECTag>();
        public ObservableCollection<TECProtocol> Protocols { get; } = new ObservableCollection<TECProtocol>();

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
            UnionizeScopeColelction(this.ConnectionTypes, catalogToAdd.ConnectionTypes);
            UnionizeScopeColelction(this.ConduitTypes, catalogToAdd.ConduitTypes);
            UnionizeScopeColelction(this.AssociatedCosts, catalogToAdd.AssociatedCosts);
            UnionizeScopeColelction(this.PanelTypes, catalogToAdd.PanelTypes);
            UnionizeScopeColelction(this.ControllerTypes, catalogToAdd.ControllerTypes);
            UnionizeScopeColelction(this.IOModules, catalogToAdd.IOModules);
            UnionizeScopeColelction(this.Devices, catalogToAdd.Devices);
            UnionizeScopeColelction(this.Valves, catalogToAdd.Valves);
            UnionizeScopeColelction(this.Manufacturers, catalogToAdd.Manufacturers);
            UnionizeScopeColelction(this.Tags, catalogToAdd.Tags);
            UnionizeScopeColelction(this.Protocols, catalogToAdd.Protocols);
        }

        public void Fill(TECCatalogs catalogToAdd)
        {
            FillScopeCollection(this.ConnectionTypes, catalogToAdd.ConnectionTypes);
            FillScopeCollection(this.ConduitTypes, catalogToAdd.ConduitTypes);
            FillScopeCollection(this.AssociatedCosts, catalogToAdd.AssociatedCosts);
            FillScopeCollection(this.PanelTypes, catalogToAdd.PanelTypes);
            FillScopeCollection(this.ControllerTypes, catalogToAdd.ControllerTypes);
            FillScopeCollection(this.IOModules, catalogToAdd.IOModules);
            FillScopeCollection(this.Devices, catalogToAdd.Devices);
            FillScopeCollection(this.Valves, catalogToAdd.Valves);
            FillScopeCollection(this.Manufacturers, catalogToAdd.Manufacturers);
            FillScopeCollection(this.Tags, catalogToAdd.Tags);
            FillScopeCollection(this.Protocols, catalogToAdd.Protocols);
        }
    }
}
