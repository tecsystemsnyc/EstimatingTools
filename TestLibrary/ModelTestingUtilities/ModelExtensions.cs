using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary.ModelTestingUtilities
{
    public static class ModelExtensions
    {
        #region Random Element Accessors
        /// <summary>
        /// Gets a random element from a source IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Source IEnumerable</param>
        /// <param name="rand">Random object</param>
        /// <returns></returns>
        public static T RandomElement<T>(this IEnumerable<T> items, Random rand)
        {
            if (items.Count() == 0) { return default(T); }
            return items.ElementAt(rand.Next(items.Count()));
        }
        /// <summary>
        /// Gets a list of random elements from a source IEnumberable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Source IEnumerable</param>
        /// <param name="rand">Random object</param>
        /// <param name="unique">Whether or not the return elements should be different elements from "items"</param>
        /// <param name="maxItems">The maximum number of items to be returned</param>
        /// <returns></returns>
        public static List<T> RandomElements<T>(this IEnumerable<T> items, Random rand, bool unique, int maxItems = -1)
        {
            List<T> list = new List<T>();
            if (items.Count() == 0) return list;
            
            if (unique)
            {
                //Make sure the number of items to be returned is a random distribution to the least value
                //comparing maxItems and the count of the source collection
                int numItems;
                if (maxItems < 1)
                {
                    numItems = rand.Next(1, items.Count());
                }
                else
                {
                    numItems = (maxItems < items.Count()) ? rand.Next(1, maxItems) : rand.Next(1, items.Count());
                }

                //Add items to the out list making sure there are no repeats.
                List<T> sourceList = new List<T>(items);
                for(int i = 0; i < numItems; i++)
                {
                    T item = sourceList.RandomElement(rand);
                    list.Add(item);
                    sourceList.Remove(item);
                }
            }
            else
            {
                rand.RepeatAction(() => list.Add(items.RandomElement(rand)), maxItems);
            }
            return list;
        }

        public static TECAssociatedCost RandomCost(this TECCatalogs catalogs, Random rand, CostType type)
        {
            return catalogs.AssociatedCosts.Where(cost => cost.Type == type).RandomElement(rand);
        }
        #endregion

        #region Assign Model Properties Methods
        public static void AssignTestLabel(this TECLabeled labeled)
        {
            labeled.Label = labeled.Guid.ToString().Substring(0, 5);
        }
        public static void AssignRandomTaggedProperties(this TECTagged tagged, TECCatalogs catalogs, Random rand)
        {
            tagged.Name = tagged.Guid.ToString().Substring(0, 5);
            tagged.Tags.Add(catalogs.Tags.RandomElement(rand));
        }
        public static void AssignRandomScopeProperties(this TECScope scope, TECCatalogs catalogs, Random rand)
        {
            scope.AssignRandomTaggedProperties(catalogs, rand);
            TECAssociatedCost randTECCost = catalogs.RandomCost(rand, CostType.TEC);
            TECAssociatedCost randElecCost = catalogs.RandomCost(rand, CostType.Electrical);
            if (randTECCost != null) scope.AssociatedCosts.Add(randTECCost);
            if (randElecCost != null) scope.AssociatedCosts.Add(randElecCost);
        }
        public static void AssignRandomCostProperties(this ICost cost, TECCatalogs catalogs, Random rand)
        {
            if (cost is TECScope scope)
            {
                scope.AssignRandomScopeProperties(catalogs, rand);
            }
            else if (cost is TECTagged tagged)
            {
                tagged.AssignRandomTaggedProperties(catalogs, rand);
            }
            cost.Cost = (rand.NextDouble() * 100);
            cost.Labor = (rand.NextDouble() * 100);
        }
        public static void AssignRandomElectricalMaterialProperties(this TECElectricalMaterial mat, TECCatalogs catalogs, Random rand)
        {
            mat.AssignRandomCostProperties(catalogs, rand);
            TECAssociatedCost randTECCost = catalogs.RandomCost(rand, CostType.TEC);
            TECAssociatedCost randElecCost = catalogs.RandomCost(rand, CostType.Electrical);
            if (randTECCost != null) mat.RatedCosts.Add(randTECCost);
            if (randElecCost != null) mat.RatedCosts.Add(randElecCost);
        }
        public static void AssignRandomSystemProperties(this TECSystem sys, TECCatalogs catalogs, Random rand)
        {
            sys.AssignRandomScopeProperties(catalogs, rand);
            rand.RepeatAction(() => sys.Equipment.Add(ModelCreation.TestEquipment(catalogs, rand)), 5);
            rand.RepeatAction(() => sys.AddController(ModelCreation.TestProvidedController(catalogs, rand)), 5);
            rand.RepeatAction(() => sys.Panels.Add(ModelCreation.TestPanel(catalogs, rand)), 5);
            rand.RepeatAction(() => sys.MiscCosts.Add(ModelCreation.TestMisc(catalogs, rand, CostType.TEC)), 5);
            rand.RepeatAction(() => sys.MiscCosts.Add(ModelCreation.TestMisc(catalogs, rand, CostType.Electrical)), 5);
            rand.RepeatAction(() => sys.ScopeBranches.Add(ModelCreation.TestScopeBranch(rand, 3)), 5);
            rand.RepeatAction(() => ModelCreation.AddSystemConnections(sys, catalogs, rand), 5);
            sys.IsSingleton = rand.NextBool();
        }
        public static void AssignRandomConnectionProperties(this IConnection connection, TECCatalogs catalogs, Random rand)
        {
            connection.ConduitType = catalogs.ConduitTypes.RandomElement(rand);
            connection.Length = (rand.NextDouble() * 100);
            connection.ConduitLength = (rand.NextDouble() * 100);
        }
        #endregion

        #region IRelatableExtension
        
        public static ITECObject FindChild(this IRelatable parent, Guid guid)
        {
            foreach(ITECObject child in parent.GetDirectChildren())
            {
                if(child.Guid == guid)
                {
                    return child;
                }
                else if (child is IRelatable childRel)
                {
                    var found = childRel.FindChild(guid);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
