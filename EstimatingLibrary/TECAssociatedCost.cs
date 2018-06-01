using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECAssociatedCost : TECCost, ICatalog<TECAssociatedCost>, IDDCopiable
    {
        //Constructors built to match TECCost. TECAssociated cost broken out to be own class that implements ICatalog
        public TECAssociatedCost(Guid guid, CostType type) : base(guid, type) { }
        public TECAssociatedCost(TECAssociatedCost cost) : base(cost) { }
        public TECAssociatedCost(CostType type) : base(type) { }

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
