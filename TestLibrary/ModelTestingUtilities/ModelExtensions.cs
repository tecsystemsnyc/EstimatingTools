using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.ModelTestingUtilities
{
    public static class ModelExtensions
    {
        public static void AssignScopeProperties(this TECScope scope, TECCatalogs catalogs)
        {
            if (scope.Tags.Count == 0)
            {
                scope.Tags.Add(catalogs.Tags[0]);
            }
            bool tecAdded = false;
            bool elecAdded = false;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.TEC)
                {
                    if (!tecAdded)
                    {
                        scope.AssociatedCosts.Add(cost);
                        tecAdded = true;
                    }
                }
                else if (cost.Type == CostType.Electrical)
                {
                    if (!elecAdded)
                    {
                        scope.AssociatedCosts.Add(cost);
                        elecAdded = true;
                    }
                }
                if (tecAdded && elecAdded) break;
            }
        }
    }
}
