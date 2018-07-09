using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;

namespace EstimatingLibrary
{
    public class TECMisc : TECCost, IDDCopiable, ITypicalable
    {
        #region Fields
        private int _quantity;
        #endregion

        #region Constructors
        public TECMisc(Guid guid, CostType type) : base(guid, type)
        {
            IsTypical = false;
            _quantity = 1;
        }
        public TECMisc(CostType type) : this(Guid.NewGuid(), type) { }
        public TECMisc(TECMisc miscSource) : this(miscSource.Type)
        {
            copyPropertiesFromCost(miscSource);
            _quantity = miscSource.Quantity;
        }
        public TECMisc(TECCost costSource) : this(costSource.Type)
        {
            copyPropertiesFromCost(costSource);
        }
        #endregion
        
        #region Properties
        public override double Cost
        {
            get
            {
                return base.Cost;
            }
            set
            {
                var old = new TECMisc(this);
                base._cost = value;
                notifyCombinedChanged(Change.Edit, "Cost", this, value, old.Cost);
                NotifyMiscChanged(this, old);
            }
        }
        public override double Labor
        {
            get
            {
                return base.Labor;
            }
            set
            {
                var old = new TECMisc(this);
                base._labor = value;
                notifyCombinedChanged(Change.Edit, "Labor", this, value, old.Labor);
                NotifyMiscChanged(this, old);
            }
        }
        public override CostType Type
        {
            get
            {
                return base.Type;
            }
            set
            {
                var old = new TECMisc(this);
                base._type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old.Type);
                NotifyMiscChanged(this, old);
            }
        }
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                var oldMisc = this;
                var old = Quantity;
                _quantity = value;
                notifyCombinedChanged(Change.Edit, "Quantity", this, value, old);
                NotifyMiscChanged(this, oldMisc);
            }
        }
        
        public bool IsTypical { get; private set; }
        #endregion

        #region Methods
        public object DragDropCopy(TECScopeManager scopeManager)
        {
            return new TECMisc(this);
        }

        private void NotifyMiscChanged(TECMisc newMisc, TECMisc oldMisc)
        {
            CostBatch delta = newMisc.CostBatch - oldMisc.CostBatch;
            notifyCostChanged(delta);
        }

        protected override CostBatch getCosts()
        {
            return base.getCosts() * Quantity;
        }

        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }
        
        #endregion

        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECMisc(this);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            this.AddChildForScopeProperty(property, item);
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            return this.RemoveChildForScopeProperty(property, item);
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            return this.ContainsChildForScopeProperty(property, item);
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
        }
        #endregion
    }
}
