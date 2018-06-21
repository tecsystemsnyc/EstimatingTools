﻿using EstimatingLibrary;
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
                if (maxItems < 0)
                {
                    numItems = rand.Next(items.Count());
                }
                else
                {
                    numItems = (maxItems < items.Count()) ? rand.Next(maxItems) : rand.Next(items.Count());
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
        public static void AssignRandomScopeProperties(this TECScope scope, TECCatalogs catalogs, Random rand)
        {
            scope.Name = scope.Guid.ToString().Substring(0, 5);
            scope.Tags.Add(catalogs.Tags.RandomElement(rand));
            TECAssociatedCost randTECCost = catalogs.RandomCost(rand, CostType.TEC);
            TECAssociatedCost randElecCost = catalogs.RandomCost(rand, CostType.Electrical);
            if (randTECCost != null) scope.AssociatedCosts.Add(randTECCost);
            if (randElecCost != null) scope.AssociatedCosts.Add(randElecCost);
        }
        public static void AssignRandomCostProperties(this TECCost cost, TECCatalogs catalogs, Random rand)
        {
            cost.AssignRandomScopeProperties(catalogs, rand);
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
        #endregion
    }
}
