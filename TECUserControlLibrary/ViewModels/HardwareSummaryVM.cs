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
    public class HardwareSummaryVM : ViewModelBase, IComponentSummaryVM
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fields and Properties
        protected readonly Dictionary<Guid, HardwareSummaryItem> hardwareDictionary;
        protected readonly Dictionary<Guid, CostSummaryItem> assocCostDictionary;

        protected readonly ObservableCollection<HardwareSummaryItem> _hardwareItems;
        protected readonly ObservableCollection<CostSummaryItem> _assocTECItems;
        protected readonly ObservableCollection<CostSummaryItem> _assocElecItems;

        protected double _hardwareCost;
        protected double _hardwareLabor;
        protected double _assocTECCostTotal;
        protected double _assocTECLaborTotal;
        protected double _assocElecCostTotal;
        protected double _assocElecLaborTotal;

        public ReadOnlyObservableCollection<HardwareSummaryItem> HardwareItems
        {
            get { return new ReadOnlyObservableCollection<HardwareSummaryItem>(_hardwareItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocTECItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocTECItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocElecItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocElecItems); }
        }

        public double HardwareCost
        {
            get { return _hardwareCost; }
            protected set
            {
                _hardwareCost = value;
                RaisePropertyChanged("HardwareCost");
                RaisePropertyChanged("TotalTECCost");
            }
        }
        public double HardwareLabor
        {
            get { return _hardwareLabor; }
            protected set
            {
                _hardwareLabor = value;
                RaisePropertyChanged("HardwareLabor");
                RaisePropertyChanged("TotalTECLabor");
            }
        }
        public double AssocTECCostTotal
        {
            get { return _assocTECCostTotal; }
            protected set
            {
                _assocTECCostTotal = value;
                RaisePropertyChanged("AssocTECCostTotal");
                RaisePropertyChanged("TotalTECCost");
            }
        }
        public double AssocTECLaborTotal
        {
            get { return _assocTECLaborTotal; }
            protected set
            {
                _assocTECLaborTotal = value;
                RaisePropertyChanged("AssocTECLaborTotal");
                RaisePropertyChanged("TotalTECLabor");
            }
        }
        public double AssocElecCostTotal
        {
            get { return _assocElecCostTotal; }
            protected set
            {
                _assocElecCostTotal = value;
                RaisePropertyChanged("AssocElecCostTotal");
                RaisePropertyChanged("TotalElecCost");
            }
        }
        public double AssocElecLaborTotal
        {
            get { return _assocElecLaborTotal; }
            protected set
            {
                _assocElecLaborTotal = value;
                RaisePropertyChanged("AssocElecLaborTotal");
                RaisePropertyChanged("TotalElecLabor");
            }
        }

        public virtual double TotalTECCost
        {
            get
            {
                return (HardwareCost + AssocTECCostTotal);
            }
        }
        public virtual double TotalTECLabor
        {
            get
            {
                return (HardwareLabor + AssocTECLaborTotal);
            }
        }
        public virtual double TotalElecCost
        {
            get
            {
                return AssocElecCostTotal;
            }
        }
        public virtual double TotalElecLabor
        {
            get
            {
                return AssocElecLaborTotal;
            }
        }
        #endregion

        public HardwareSummaryVM()
        {
            hardwareDictionary = new Dictionary<Guid, HardwareSummaryItem>();
            assocCostDictionary = new Dictionary<Guid, CostSummaryItem>();

            _hardwareItems = new ObservableCollection<HardwareSummaryItem>();
            _assocTECItems = new ObservableCollection<CostSummaryItem>();
            _assocElecItems = new ObservableCollection<CostSummaryItem>();

            _hardwareCost = 0;
            _hardwareLabor = 0;
            _assocTECCostTotal = 0;
            _assocTECLaborTotal= 0;
            _assocElecCostTotal = 0;
            _assocElecLaborTotal = 0;
        }

        #region Methods
        public CostBatch AddHardware(TECHardware hardware)
        {
            CostBatch deltas = new CostBatch();
            bool containsItem = hardwareDictionary.ContainsKey(hardware.Guid);
            if (containsItem)
            {
                HardwareSummaryItem item = hardwareDictionary[hardware.Guid];
                CostBatch delta = item.Increment();
                HardwareCost += delta.GetCost(hardware.Type);
                HardwareLabor += delta.GetLabor(hardware.Type);
                deltas += delta;
            }
            else
            {
                HardwareSummaryItem item = new HardwareSummaryItem(hardware);
                hardwareDictionary.Add(hardware.Guid, item);
                _hardwareItems.Add(item);
                HardwareCost += item.TotalCost;
                HardwareLabor += item.TotalLabor;
                deltas += new CostBatch(item.TotalCost, item.TotalLabor, hardware.Type);
            }
            foreach (ICost cost in hardware.AssociatedCosts)
            {
                deltas += AddCost(cost);
            }
            return deltas;
        }
        public CostBatch RemoveHardware(TECHardware hardware)
        {
            bool containsItem = hardwareDictionary.ContainsKey(hardware.Guid);
            if (containsItem)
            {
                CostBatch deltas = new CostBatch();
                HardwareSummaryItem item = hardwareDictionary[hardware.Guid];
                CostBatch delta = item.Decrement();
                deltas += delta;
                HardwareCost += delta.GetCost(hardware.Type);
                HardwareLabor += delta.GetLabor(hardware.Type);

                if (item.Quantity < 1)
                {
                    _hardwareItems.Remove(item);
                    hardwareDictionary.Remove(hardware.Guid);
                }
                foreach (ICost cost in hardware.AssociatedCosts)
                {
                    deltas += RemoveCost(cost);
                }
                return deltas;
            }
            else
            {
                logger.Error("Hardware not present. Cannot remove hardware. Hardware: {0}", hardware.Name);
                return new CostBatch();
            }
        }

        public CostBatch AddCost(ICost cost)
        {
            bool containsItem = assocCostDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = assocCostDictionary[cost.Guid];
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
            else
            {
                CostSummaryItem item = new CostSummaryItem(cost);
                assocCostDictionary.Add(cost.Guid, item);
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
                return new CostBatch(item.TotalCost, item.TotalLabor, cost.Type);
            }
        }
        public CostBatch RemoveCost(ICost cost)
        {
            bool containsItem = assocCostDictionary.ContainsKey(cost.Guid);
            if (containsItem)
            {
                CostSummaryItem item = assocCostDictionary[cost.Guid];
                CostBatch delta = item.RemoveQuantity(1);
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
                if (item.Quantity < 1)
                {
                    assocCostDictionary.Remove(cost.Guid);
                    if (cost.Type == CostType.TEC)
                    {
                        _assocTECItems.Remove(item);
                    }
                    else if (cost.Type == CostType.Electrical)
                    {
                        _assocElecItems.Remove(item);
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
        #endregion
    }
}
