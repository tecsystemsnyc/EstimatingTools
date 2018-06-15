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
        public static void AssignScopeProperties(this TECScope scope, Random rand, TECCatalogs catalogs)
        {
            scope.Tags.Add(catalogs.Tags.RandomElement(rand));
            scope.AssociatedCosts.Add(catalogs.RandomCost(rand, CostType.TEC));
            scope.AssociatedCosts.Add(catalogs.RandomCost(rand, CostType.Electrical));
        }

        public static T RandomElement<T>(this IEnumerable<T> items, Random rand)
        {
            return items.ElementAt(rand.Next(items.Count()));
        }

        public static TECAssociatedCost RandomCost(this TECCatalogs catalogs, Random rand, CostType type)
        {
            return catalogs.AssociatedCosts.Where(cost => cost.Type == type).RandomElement(rand);
        }
    }
}
