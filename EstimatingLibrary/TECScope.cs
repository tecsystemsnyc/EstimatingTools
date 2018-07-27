using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public abstract class TECScope : TECTagged, INotifyCostChanged, IRelatable, ITECScope, ICatalogContainer
    {
        #region Properties
        
        public ObservableCollection<TECAssociatedCost> AssociatedCosts { get; } = new ObservableCollection<TECAssociatedCost>();

        public virtual CostBatch CostBatch
        {
            get
            {
                return getCosts();
            }
        }

        public event Action<CostBatch> CostChanged;

        #endregion

        #region Constructors
        public TECScope(Guid guid) : base(guid)
        {
            AssociatedCosts.CollectionChanged += (sender, args) => scopeCollectionChanged(sender, args, "AssociatedCosts");
        }

        #endregion 

        #region Methods
        protected void copyPropertiesFromScope(TECScope scope)
        {
            base.copyPropertiesFromTagged(scope);
            AssociatedCosts.ObservablyClear();
            scope.AssociatedCosts.ForEach(item => this.AssociatedCosts.Add(item));
        }
        protected virtual void scopeCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
            //Is virtual so that it can be overridden in TECTypical
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged, notifyCostChanged);
        }

        protected virtual void notifyCostChanged(CostBatch costs)
        {
            CostChanged?.Invoke(costs);
        }

        protected virtual CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
            this.AssociatedCosts.ForEach(item => costs += item.CostBatch);
            return costs;
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = base.propertyObjects();
            saveList.AddRange(this.AssociatedCosts.Distinct(), "AssociatedCosts");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap relatedList = base.linkedObjects();
            relatedList.AddRange(this.AssociatedCosts.Distinct(), "AssociatedCosts");
            return relatedList;
        }
        #endregion Methods
    }
}
