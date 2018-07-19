using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingUtilitiesLibrary.SummaryItems
{
    public class ValveSummaryItem : INotifyPropertyChanged
    {
        #region Fields and Properties
        private int _quantity;

        private double _totalCost;
        private double _totalLabor;

        public TECValve Valve { get; }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (Quantity != value)
                {
                    _quantity = value;
                    raisePropertyChanged("Quantity");
                }
            }
        }
        
        public double TotalCost
        {
            get { return _totalCost; }
            set
            {
                if (TotalCost != value)
                {
                    _totalCost = value;
                    raisePropertyChanged("TotalCost");
                }
            }
        }
        public double TotalLabor
        {
            get { return _totalLabor; }
            set
            {
                if (TotalLabor != value)
                {
                    _totalLabor = value;
                    raisePropertyChanged("TotalLabor");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public ValveSummaryItem(TECValve valve)
        {
            Valve = valve;
            _quantity = 1;
            updateTotals();
        }

        #region Methods
        public CostBatch Increment()
        {
            Quantity++;
            return updateTotals();
        }
        public CostBatch Decrement()
        {
            Quantity--;
            return updateTotals();
        }

        private CostBatch updateTotals()
        {
            double newCost = (Valve.Cost * Quantity);
            double newLabor = (Valve.Labor * Quantity);

            double deltaCost = newCost - TotalCost;
            double deltaLabor = newLabor - TotalLabor;

            TotalCost = newCost;
            TotalLabor = newLabor;

            return new CostBatch(deltaCost, deltaLabor, CostType.TEC);
        }

        private void raisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
