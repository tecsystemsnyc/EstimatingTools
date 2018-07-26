using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.SummaryItems;
using GalaSoft.MvvmLight;
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
    public class ValveSummaryVM : HardwareSummaryVM, IComponentSummaryVM
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Fields and Properties
        private readonly Dictionary<Guid, ValveSummaryItem> valveDictionary;

        private readonly ObservableCollection<ValveSummaryItem> _valveItems;

        private double _valveCost;
        private double _valveLabor;

        public ReadOnlyObservableCollection<ValveSummaryItem> ValveItems
        {
            get { return new ReadOnlyObservableCollection<ValveSummaryItem>(_valveItems); }
        }
        public ReadOnlyObservableCollection<HardwareSummaryItem> ActuatorItems
        {
            get { return new ReadOnlyObservableCollection<HardwareSummaryItem>(_hardwareItems); }
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
            get { return _hardwareCost; }
            set
            {
                if (ActuatorCost != value)
                {
                    _hardwareCost = value;
                    RaisePropertyChanged("ActuatorCost");
                    RaisePropertyChanged("TotalTECCost");
                }
            }
        }
        public double ActuatorLabor
        {
            get { return _hardwareLabor; }
            set
            {
                if (ActuatorLabor != value)
                {
                    _hardwareLabor = value;
                    RaisePropertyChanged("ActuatorLabor");
                    RaisePropertyChanged("TotalTECLabor");
                }
            }
        }

        public override double TotalTECCost
        {
            get
            {
                return (ValveCost + ActuatorCost + AssocTECCostTotal);
            }
        }
        public override double TotalTECLabor
        {
            get
            {
                return (ValveLabor + ActuatorLabor + AssocTECLaborTotal);
            }
        }
        #endregion

        public ValveSummaryVM()
        {
            valveDictionary = new Dictionary<Guid, ValveSummaryItem>();

            _valveItems = new ObservableCollection<ValveSummaryItem>();

            _valveCost = 0;
            _valveLabor = 0;
        }

        #region Methods
        public CostBatch AddValve(TECValve valve)
        {
            CostBatch deltas = new CostBatch();
            bool containsItem = valveDictionary.ContainsKey(valve.Guid);
            if (containsItem)
            {
                ValveSummaryItem item = valveDictionary[valve.Guid];
                CostBatch delta = item.Increment();
                ValveCost += delta.GetCost(valve.Type);
                ValveLabor += delta.GetLabor(valve.Type);
                deltas += delta;
            }
            else
            {
                ValveSummaryItem item = new ValveSummaryItem(valve);
                valveDictionary.Add(valve.Guid, item);
                _valveItems.Add(item);
                ValveCost += item.TotalCost;
                ValveLabor += item.TotalLabor;
                deltas += new CostBatch(item.TotalCost, item.TotalLabor, valve.Type);
            }

            deltas += AddActuator(valve.Actuator);
            foreach(ICost cost in valve.AssociatedCosts)
            {
                deltas += AddCost(cost);
            }
            return deltas;
        }
        public CostBatch RemoveValve(TECValve valve)
        {
            bool containsItem = valveDictionary.ContainsKey(valve.Guid);
            if (containsItem)
            {
                CostBatch deltas = new CostBatch();
                ValveSummaryItem item = valveDictionary[valve.Guid];
                CostBatch delta = item.Decrement();
                deltas += delta;
                ValveCost += delta.GetCost(valve.Type);
                ValveLabor += delta.GetLabor(valve.Type);

                if (item.Quantity < 1)
                {
                    _valveItems.Remove(item);
                    valveDictionary.Remove(valve.Guid);
                }

                deltas += RemoveActuator(valve.Actuator);
                foreach(ICost cost in valve.AssociatedCosts)
                {
                    deltas += RemoveCost(cost);
                }
                return deltas;
            }
            else
            {
                logger.Error("Valve not present. Cannot remove valve. Valve: {0}", valve.Name);
                return new CostBatch();
            }
        }

        public CostBatch AddActuator(TECDevice actuator)
        {
            return AddHardware(actuator);
        }
        public CostBatch RemoveActuator(TECDevice actuator)
        {
            return RemoveHardware(actuator);
        }
        #endregion
    }
}
