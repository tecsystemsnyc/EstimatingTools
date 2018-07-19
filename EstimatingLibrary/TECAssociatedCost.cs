using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECAssociatedCost : TECTagged, INotifyCostChanged, ICatalog<TECAssociatedCost>, IDDCopiable, ICost
    {
        protected double _cost = 0;
        protected double _labor = 0;
        protected CostType _type;

        public virtual double Cost
        {
            get { return _cost; }
            set
            {
                var old = this.CostBatch;
                _cost = value;
                notifyCombinedChanged(Change.Edit, "Cost", this, value, old);
                notifyCostChanged(this.CostBatch - old);
            }
        }
        public virtual double Labor
        {
            get { return _labor; }
            set
            {
                var old = this.CostBatch;
                _labor = value;
                notifyCombinedChanged(Change.Edit, "Labor", this, value, old);
                notifyCostChanged(this.CostBatch - old);
            }
        }
        public virtual CostType Type
        {
            get { return _type; }
            set
            {
                var old = this.CostBatch;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                notifyCostChanged(this.CostBatch - old);
            }
        }

        //Constructors built to match TECCost. TECAssociated cost broken out to be own class that implements ICatalog
        public TECAssociatedCost(Guid guid, CostType type) : base(guid)
        {
            this.Type = type;
        }
        public TECAssociatedCost(TECAssociatedCost cost) : this(cost.Guid, cost.Type)
        {
            this.Cost = cost.Cost;
            this.Labor = cost.Labor;
            this.copyPropertiesFromTagged(cost);
        }
        public TECAssociatedCost(CostType type) : this(Guid.NewGuid(), type) { }

        public CostBatch CostBatch => new CostBatch(this.Cost, this.Labor, this.Type);

        public TECAssociatedCost CatalogCopy()
        {
            return new TECAssociatedCost(this);
        }

        public object DragDropCopy(TECScopeManager scopeManager)
        {
            return this;
        }
    }
}
