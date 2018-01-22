using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.SummaryItems;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.ViewModels.Interfaces;

namespace TECUserControlLibrary.ViewModels
{
    public class ValveSummaryVM : ViewModelBase, IComponentSummaryVM
    {
        #region Fields and Properties
        private Dictionary<Guid, HardwareSummaryItem> valveDictionary;
        private Dictionary<Guid, HardwareSummaryItem> actuatorDictionary;
        private Dictionary<Guid, CostSummaryItem> assocCostDictionary;

        private readonly ObservableCollection<HardwareSummaryItem> _valveItems;
        private readonly ObservableCollection<HardwareSummaryItem> _actuatorItems;
        private readonly ObservableCollection<CostSummaryItem> _assocTECItems;
        private readonly ObservableCollection<CostSummaryItem> _assocElecItems;

        private double _valveCost;
        private double _valveLabor;
        private double _actuatorCost;
        private double _actuatorLabor;
        private double _assocTECCostTotal;
        private double _assocTECLaborTotal;
        private double _assocElecCostTotal;
        private double _assocElecLaborTotal;

        public ReadOnlyObservableCollection<HardwareSummaryItem> ValveItems
        {
            get { return new ReadOnlyObservableCollection<HardwareSummaryItem>(_valveItems); }
        }
        public ReadOnlyObservableCollection<HardwareSummaryItem> ActuatorItems
        {
            get { return new ReadOnlyObservableCollection<HardwareSummaryItem>(_actuatorItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocTECItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocTECItems); }
        }
        public ReadOnlyObservableCollection<CostSummaryItem> AssocElecItems
        {
            get { return new ReadOnlyObservableCollection<CostSummaryItem>(_assocElecItems); }
        }

        public double ValveCost
        {
            get { return _valveCost; }
            set
            {
                if (ValveCost != value)
                {
                    _valveCost = value;
                    RaisePropertyChanged("ValveCost");
                    RaisePropertyChanged("TotalTECCost");
                }
            }
        }
        public double ValveLabor
        {
            get { return _valveLabor; }
            set
            {
                if (ValveLabor != value)
                {
                    _valveLabor = value;
                    RaisePropertyChanged("ValveLabor");
                    RaisePropertyChanged("TotalTECLabor");
                }
            }
        }
        public double ActuatorCost
        {
            get { return _actuatorCost; }
            set
            {
                if (ActuatorCost != value)
                {
                    _actuatorCost = value;
                    RaisePropertyChanged("ActuatorCost");
                    RaisePropertyChanged("TotalTECCost");
                }
            }
        }
        public double ActuatorLabor
        {
            get { return _actuatorLabor; }
            set
            {
                if (ActuatorLabor != value)
                {
                    _actuatorLabor = value;
                    RaisePropertyChanged("ActuatorLabor");
                    RaisePropertyChanged("TotalTECLabor");
                }
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
                return (ValveCost + ActuatorCost + AssocTECCostTotal);
            }
        }
        public double TotalTECLabor
        {
            get
            {
                return (ValveLabor + ActuatorLabor + AssocTECLaborTotal);
            }
        }
        public double TotalElecCost
        {
            get
            {
                return (AssocElecCostTotal);
            }
        }
        public double TotalElecLabor
        {
            get
            {
                return (AssocElecLaborTotal);
            }
        }
        #endregion

        public ValveSummaryVM()
        {
            valveDictionary = new Dictionary<Guid, HardwareSummaryItem>();
            actuatorDictionary = new Dictionary<Guid, HardwareSummaryItem>();
            assocCostDictionary = new Dictionary<Guid, CostSummaryItem>();

            _valveItems = new ObservableCollection<HardwareSummaryItem>();
            _actuatorItems = new ObservableCollection<HardwareSummaryItem>();
            _assocTECItems = new ObservableCollection<CostSummaryItem>();
            _assocElecItems = new ObservableCollection<CostSummaryItem>();

            _valveCost = 0;
            _valveLabor = 0;
            _actuatorCost = 0;
            _actuatorLabor = 0;
            _assocTECCostTotal = 0;
            _assocTECLaborTotal = 0;
            _assocElecCostTotal = 0;
            _assocElecLaborTotal = 0;
        }

        #region Methods
        public CostBatch AddValve(TECValve valve)
        {
            CostBatch deltas = new CostBatch();
            bool containsItem = valveDictionary.ContainsKey(valve.Guid);
            if (containsItem)
            {
                HardwareSummaryItem item = valveDictionary[valve.Guid];
                CostBatch delta = item.Increment();
                ValveCost += delta.GetCost(valve.Type);
                ValveLabor += delta.GetLabor(valve.Type);
                deltas += delta;
            }
            else
            {
                HardwareSummaryItem item = new HardwareSummaryItem(valve);
                valveDictionary.Add(valve.Guid, item);
                _valveItems.Add(item);
                ValveCost += item.TotalCost;
                ValveLabor += item.TotalLabor;
                deltas += new CostBatch(item.TotalCost, item.TotalLabor, valve.Type);
            }

            deltas += AddActuator(valve.Actuator);
            foreach(TECCost cost in valve.AssociatedCosts)
            {
                deltas += AddCost(cost);
            }
            return deltas;
        }
        public CostBatch RemoveValve(TECValve valve)
        {
            throw new NotImplementedException();
        }

        public CostBatch AddActuator(TECDevice actuator)
        {
            throw new NotImplementedException();
        }
        public CostBatch RemoveActuator(TECDevice actuator)
        {
            throw new NotImplementedException();
        }

        public CostBatch AddCost(TECCost cost)
        {
            throw new NotImplementedException();
        }
        public CostBatch RemoveCost(TECCost cost)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
