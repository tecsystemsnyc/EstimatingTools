using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.SummaryItems;
using GalaSoft.MvvmLight;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.ViewModels.Interfaces;

namespace TECUserControlLibrary.ViewModels
{
    public class MiscCostsSummaryVM : ViewModelBase, IComponentSummaryVM
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fields
        private readonly Dictionary<Guid, CostSummaryItem> costDictionary;

        private readonly ObservableCollection<CostSummaryItem> _miscTECItems;
        private readonly ObservableCollection<CostSummaryItem> _miscElecItems;
        private readonly ObservableCollection<CostSummaryItem> _assocTECItems;
        private readonly ObservableCollection<CostSummaryItem> _assocElecItems;

        private double _miscTECCostTotal;
        private double _miscTECLaborTotal;
        private double _miscElecCostTotal;
        private double _miscElecLaborTotal;
        private double _assocTECCostTotal;
        private double _assocTECLaborTotal;
        private double _assocElecCostTotal;
        private double _assocElecLaborTotal;

        public ReadOnlyObservableCollection<CostSummaryItem> MiscTECItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_miscTECItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> MiscElecItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_miscElecItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocTECItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocTECItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocElecItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocElecItems); }
        }

        public double MiscTECCostTotal
        {
            get { return _miscTECCostTotal; }
            private set
            {
                _miscTECCostTotal = value;
                RaisePropertyChanged("MiscTECCostTotal");
                RaisePropertyChanged("TotalTECCost");
            }
        }
        public double MiscTECLaborTotal
        {
            get { return _miscTECLaborTotal; }
            private set
            {
                _miscTECLaborTotal = value;
                RaisePropertyChanged("MiscTECLaborTotal");
                RaisePropertyChanged("TotalTECLabor");
            }
        }
        public double MiscElecCostTotal
        {
            get { return _miscElecCostTotal; }
            private set
            {
                _miscElecCostTotal = value;
                RaisePropertyChanged("MiscElecCostTotal");
                RaisePropertyChanged("TotalElecCost");
            }
        }
        public double MiscElecLaborTotal
        {
            get { return _miscElecLaborTotal; }
            private set
            {
                _miscElecLaborTotal = value;
                RaisePropertyChanged("MiscElecLaborTotal");
                RaisePropertyChanged("TotalElecLabor");
            }
        }
        public double AssocTECCostTotal
        {
            get { return _assocTECCostTotal; }
            private set
            {
                _assocTECCostTotal = value;
                RaisePropertyChanged("AssocTECCostTotal");
                RaisePropertyChanged("TotalTECCost");
            }
        }
        public double AssocTECLaborTotal
        {
            get { return _assocTECLaborTotal; }
            private set
            {
                _assocTECLaborTotal = value;
                RaisePropertyChanged("AssocTECLaborTotal");
                RaisePropertyChanged("TotalTECLabor");
            }
        }
        public double AssocElecCostTotal
        {
            get { return _assocElecCostTotal; }
            private set
            {
                _assocElecCostTotal = value;
                RaisePropertyChanged("AssocElecCostTotal");
                RaisePropertyChanged("TotalElecCost");
            }
        }
        public double AssocElecLaborTotal
        {
            get { return _assocElecLaborTotal; }
            private set
            {
                _assocElecLaborTotal = value;
                RaisePropertyChanged("AssocElecLaborTotal");
                RaisePropertyChanged("TotalElecLabor");
            }
        }

        public double TotalTECCost
        {
            get
            {
                return (MiscTECCostTotal + AssocTECCostTotal);
            }
        }
        public double TotalTECLabor
        {
            get
            {
                return (MiscTECLaborTotal + AssocTECLaborTotal);
            }
        }
        public double TotalElecCost
        {
            get
            {
                return (MiscElecCostTotal + AssocElecCostTotal);
            }
        }
        public double TotalElecLabor
        {
            get
            {
                return (MiscElecLaborTotal + AssocElecLaborTotal);
            }
        }
        #endregion
        
        public MiscCostsSummaryVM()
        {
            costDictionary = new Dictionary<Guid, CostSummaryItem>();

            _miscTECItems = new ObservableCollection<CostSummaryItem>();
            _miscElecItems = new ObservableCollection<CostSummaryItem>();
            _assocTECItems = new ObservableCollection<CostSummaryItem>();
            _assocElecItems = new ObservableCollection<CostSummaryItem>();

            MiscTECCostTotal = 0;
            MiscTECLaborTotal = 0;
            MiscElecCostTotal = 0;
            MiscElecLaborTotal = 0;
            AssocTECCostTotal = 0;
            AssocTECLaborTotal = 0;
            AssocElecCostTotal = 0;
            AssocElecLaborTotal = 0;
        }
        
        #region Methods
        public CostBatch AddCost(ICost cost)
        {
            bool containsItem = costDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = costDictionary[cost.Guid];
                if (cost is TECMisc misc)
                {
                    CostBatch delta = item.AddQuantity(misc.Quantity);
                    if (cost.Type == CostType.TEC)
                    {
                        MiscTECCostTotal += delta.GetCost(CostType.TEC);
                        MiscTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        MiscElecCostTotal += delta.GetCost(CostType.Electrical);
                        MiscElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                    
                    foreach (TECAssociatedCost extraCost in misc.AssociatedCosts)
                    {
                        for (int i = 0; i < misc.Quantity; i++)
                        {
                            delta += AddCost(extraCost);
                        }
                    }
                    return delta;
                }
                else
                {
                    CostBatch delta = item.AddQuantity(1);
                    if (cost.Type == CostType.TEC)
                    {
                        AssocTECCostTotal += delta.GetCost(CostType.TEC);
                        AssocTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        AssocElecCostTotal += delta.GetCost(CostType.Electrical);
                        AssocElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                    return delta;
                }
            }
            else
            {
                CostSummaryItem item = new CostSummaryItem(cost);
                CostBatch delta = new CostBatch(item.TotalCost, item.TotalLabor, cost.Type);
                costDictionary.Add(cost.Guid, item);
                if (cost is TECMisc misc)
                {
                    if (cost.Type == CostType.TEC)
                    {
                        _miscTECItems.Add(item);
                        MiscTECCostTotal += item.TotalCost;
                        MiscTECLaborTotal += item.TotalLabor;
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        _miscElecItems.Add(item);
                        MiscElecCostTotal += item.TotalCost;
                        MiscElecLaborTotal += item.TotalLabor;
                    }
                    foreach(TECAssociatedCost assocCost in misc.AssociatedCosts)
                    {
                        for(int i = 0; i < misc.Quantity; i++)
                        {
                            delta += AddCost(assocCost);
                        }
                    }
                }
                else
                {
                    if (cost.Type == CostType.TEC)
                    {
                        _assocTECItems.Add(item);
                        AssocTECCostTotal += item.TotalCost;
                        AssocTECLaborTotal += item.TotalLabor;
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        _assocElecItems.Add(item);
                        AssocElecCostTotal += item.TotalCost;
                        AssocElecLaborTotal += item.TotalLabor;
                    }
                }
                return delta;
            }
        }
        public CostBatch RemoveCost(ICost cost)
        {
            CostBatch delta;
            bool containsItem = costDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = costDictionary[cost.Guid];
                if (cost is TECMisc misc)
                {
                    delta = item.RemoveQuantity(misc.Quantity);
                    if (cost.Type == CostType.TEC)
                    {
                        MiscTECCostTotal += delta.GetCost(CostType.TEC);
                        MiscTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        MiscElecCostTotal += delta.GetCost(CostType.Electrical);
                        MiscElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                    foreach(TECAssociatedCost assocCost in misc.AssociatedCosts)
                    {
                        for(int i = 0; i < misc.Quantity; i++)
                        {
                            delta += RemoveCost(assocCost);
                        }
                    }
                }
                else
                {
                    delta = item.RemoveQuantity(1);
                    if (cost.Type == CostType.TEC)
                    {
                        AssocTECCostTotal += delta.GetCost(CostType.TEC);
                        AssocTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        AssocElecCostTotal += delta.GetCost(CostType.Electrical);
                        AssocElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                }
                if (item.Quantity < 1)
                {
                    costDictionary.Remove(cost.Guid);
                    if (cost is TECMisc miscToRemove)
                    {
                        if (cost.Type == CostType.TEC)
                        {
                            _miscTECItems.Remove(item);
                        }
                        else if (cost.Type == CostType.Electrical)
                        {
                            _miscElecItems.Remove(item);
                        }
                    }
                    else
                    {
                        if (cost.Type == CostType.TEC)
                        {
                            _assocTECItems.Remove(item);
                        }
                        else if (cost.Type == CostType.Electrical)
                        {
                            _assocElecItems.Remove(item);
                        }
                    }
                }
                return delta;
            }
            else
            {
                logger.Error("Cost not found. Cannot remove cost. Cost: {0}", cost.Name);
                return new CostBatch();
            }
        }
        public CostBatch ChangeQuantity(TECCost cost, int deltaQuantity)
        {
            bool containsItem = costDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = costDictionary[cost.Guid];
                if (item.Quantity + deltaQuantity < 0)
                {
                    logger.Error("Cannot remove more quantity than exists in CostSummaryItem. Removing all that exists." +
                        "Cost: {0}", cost.Name);
                    deltaQuantity = item.Quantity;
                }
                CostBatch delta = item.AddQuantity(deltaQuantity);
                if (cost is TECMisc)
                {
                    if (cost.Type == CostType.TEC)
                    {
                        MiscTECCostTotal += delta.GetCost(CostType.TEC);
                        MiscTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        MiscElecCostTotal += delta.GetCost(CostType.Electrical);
                        MiscElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                }
                else
                {
                    if (cost.Type == CostType.TEC)
                    {
                        AssocTECCostTotal += delta.GetCost(CostType.TEC);
                        AssocTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        AssocElecCostTotal += delta.GetCost(CostType.Electrical);
                        AssocElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                }
                return delta;
            }
            else
            {
                logger.Error("Cost not found. Cannot change quantity. Cost: {0}", cost.Name);
                return new CostBatch();
            }
        }
        public CostBatch UpdateCost(TECCost cost)
        {
            bool containsItem = costDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = costDictionary[cost.Guid];
                CostBatch delta = item.Refresh();
                if (cost is TECMisc)
                {
                    if (cost.Type == CostType.TEC)
                    {
                        MiscTECCostTotal += delta.GetCost(CostType.TEC);
                        MiscTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        MiscElecCostTotal += delta.GetCost(CostType.Electrical);
                        MiscElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                }
                else
                {
                    if (cost.Type == CostType.TEC)
                    {
                        AssocTECCostTotal += delta.GetCost(CostType.TEC);
                        AssocTECLaborTotal += delta.GetLabor(CostType.TEC);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        AssocElecCostTotal += delta.GetCost(CostType.Electrical);
                        AssocElecLaborTotal += delta.GetLabor(CostType.Electrical);
                    }
                }
                return delta;
            }
            else
            {
                logger.Error("Cost not found, cannot update. Cost: {0}", cost.Name);
                return new CostBatch();
            }
        }
        #endregion
    }
}