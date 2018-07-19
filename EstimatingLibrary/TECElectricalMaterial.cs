using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECElectricalMaterial : TECCost, ICatalog<TECElectricalMaterial>, IDDCopiable
    {
        #region Properties
        public ObservableCollection<TECAssociatedCost> RatedCosts { get; } = new ObservableCollection<TECAssociatedCost>();
        #endregion

        public TECElectricalMaterial(Guid guid) : base(guid, CostType.Electrical)
        {
            RatedCosts.CollectionChanged += (sender, args) => RatedCosts_CollectionChanged(sender, args, "RatedCosts");
        }
        public TECElectricalMaterial() : this(Guid.NewGuid()) { }
        public TECElectricalMaterial(TECElectricalMaterial materialSource) : this()
        {
            copyPropertiesFromCost(materialSource);
            var ratedCosts = new ObservableCollection<TECAssociatedCost>(materialSource.RatedCosts);
            RatedCosts.CollectionChanged -= (sender, args) => RatedCosts_CollectionChanged(sender, args, "RatedCosts");
            RatedCosts = ratedCosts;
            RatedCosts.CollectionChanged += (sender, args) => RatedCosts_CollectionChanged(sender, args, "RatedCosts");
        }

        public object DragDropCopy(TECScopeManager scopeManager)
        {
            return this;
        }

        public CostBatch GetCosts(double length)
        {
            CostBatch costBatch = new CostBatch(Cost, Labor, Type);
            foreach (ICost ratedCost in RatedCosts)
            {
                costBatch.AddCost(ratedCost);
            }
            costBatch *= length;
            foreach (TECAssociatedCost assocCost in AssociatedCosts)
            {
                costBatch.AddCost(assocCost);
            }
            return costBatch;
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.RatedCosts.Distinct(), "RatedCosts");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.AddRange(this.RatedCosts.Distinct(), "RatedCosts");
            return saveList;
        }

        private void RatedCosts_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this,
                notifyCombinedChanged);
        }

        public TECElectricalMaterial CatalogCopy()
        {
            return new TECElectricalMaterial(this);
        }
    }
}
