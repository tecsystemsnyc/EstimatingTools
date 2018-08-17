using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.SummaryItems;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.ViewModels.Interfaces;

namespace TECUserControlLibrary.ViewModels
{
    public class WireSummaryVM : LengthSummaryVM, IComponentSummaryVM
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<(Guid guid, bool isPlenum), WireSummaryItem> wireDictionary = new Dictionary<(Guid guid, bool isPlenum), WireSummaryItem>();
        
        public CostBatch AddRun(TECConnectionType type, double length, bool isPlenum)
        {
            CostBatch deltas = AddLength(type, length, isPlenum);
            foreach(ICost cost in type.AssociatedCosts)
            {
                deltas += addAssocCost(cost);
            }
            return deltas;
        }
        public CostBatch RemoveRun(TECConnectionType type, double length, bool isPlenum)
        {
            CostBatch deltas = RemoveLength(type, length, isPlenum);
            foreach(ICost cost in type.AssociatedCosts)
            {
                deltas += removeAssocCost(cost);
            }
            return deltas;
        }
        public CostBatch AddLength(TECConnectionType type, double length, bool isPlenum)
        {
            if (length < 0)
            {
                logger.Error("Length needs to be greater than 0 when adding to length summary. " +
                    "Failed to add length. Obj: {0}", type.Name);
                return new CostBatch();
            }

            (Guid, bool) wireTypeKey = (type.Guid, isPlenum);
            CostBatch deltas = new CostBatch();
            bool containsItem = wireDictionary.ContainsKey(wireTypeKey);
            if (containsItem)
            {
                WireSummaryItem item = wireDictionary[wireTypeKey];
                CostBatch delta = item.AddLength(length);
                LengthCostTotal += delta.GetCost(CostType.Electrical);
                LengthLaborTotal += delta.GetLabor(CostType.Electrical);
                deltas += delta;
            }
            else
            {
                WireSummaryItem item = new WireSummaryItem(type, length, isPlenum);
                wireDictionary.Add(wireTypeKey, item);
                _lengthSummaryItems.Add(item);
                LengthCostTotal += item.TotalCost;
                LengthLaborTotal += item.TotalLabor;
                deltas += new CostBatch(item.TotalCost, item.TotalLabor, CostType.Electrical);
            }
            foreach(ICost cost in type.RatedCosts)
            {
                deltas += addRatedCost(cost, length);
            }
            return deltas;
        }
        public CostBatch RemoveLength(TECConnectionType type, double length, bool isPlenum)
        {
            if (length < 0)
            {
                logger.Error("Length needs to be greater than 0 when removing from length summary. " +
                    "Failed to remove length. Obj: {0}", type.Name);
                return new CostBatch();
            }

            (Guid, bool) wireTypeKey = (type.Guid, isPlenum);
            bool containsItem = wireDictionary.ContainsKey(wireTypeKey);
            if (containsItem)
            {
                CostBatch deltas = new CostBatch();
                WireSummaryItem item = wireDictionary[wireTypeKey];
                CostBatch delta = item.RemoveLength(length);
                LengthCostTotal += delta.GetCost(CostType.Electrical);
                LengthLaborTotal += delta.GetLabor(CostType.Electrical);
                deltas += deltas;

                if (item.Length <= 0)
                {
                    _lengthSummaryItems.Remove(item);
                    wireDictionary.Remove(wireTypeKey);
                }
                foreach(ICost cost in type.RatedCosts)
                {
                    deltas += removeRatedCost(cost, length);
                }
                return deltas;
            }
            else
            {
                logger.Error("ConnectionType + Plenum combination not found. Cannot remove length." +
                    "Combination: {0}, {1}", type.Name, (isPlenum ? "Is Plenum" : "Is Not Plenum"));
                return new CostBatch();
            }
        }
    }
}
