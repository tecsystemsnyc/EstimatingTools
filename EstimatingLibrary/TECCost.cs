using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;

namespace EstimatingLibrary
{
    public enum CostType { TEC, Electrical }

    public abstract class TECCost : TECScope
    { 
        #region Properties
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
        
        #endregion

        #region Constructors
        public TECCost(Guid guid, CostType type) : base(guid)
        {
            _type = type;
        }
        public TECCost(TECCost cost) : this(cost.Type)
        {
            copyPropertiesFromCost(cost);
        }

        public TECCost(CostType type) : this(Guid.NewGuid(), type) { }
        #endregion

        protected override CostBatch getCosts()
        {
            return base.getCosts() + new CostBatch(this);
        }

        protected void copyPropertiesFromCost(TECCost cost)
        {
            copyPropertiesFromScope(cost);
            _cost = cost.Cost;
            _labor = cost.Labor;
            _type = cost.Type;
        }
    }
}
