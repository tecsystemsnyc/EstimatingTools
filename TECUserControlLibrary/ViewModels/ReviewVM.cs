using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TECUserControlLibrary.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ReviewVM : ViewModelBase
    {
        public TECEstimator Estimate { get; }
        public TECBid Bid { get; }

        private double _userPrice;
        public double UserPrice
        {
            get { return _userPrice; }
            set
            {
                _userPrice = value;
                var newMarkup = value / Estimate.TotalCost - 1;
                Bid.Parameters.Markup = newMarkup * 100;
                RaisePropertyChanged("UserPrice");
            }
        }

        public ObservableCollection<CostContainer> Costs
        {
            get { return getCosts(); }
        }

        public ReviewVM(TECBid bid, TECEstimator estimate)
        {
            Bid = bid;
            Estimate = estimate;
            Estimate.PropertyChanged += estimatePropertyChanged;
        }
        

        private void estimatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TECMaterialCost" ||
                e.PropertyName == "TECLaborCost" ||
                e.PropertyName == "SubcontractorLaborCost" ||
                e.PropertyName == "ElectricalMaterialCost")
            {
                RaisePropertyChanged("Costs");
            }
        }
        private ObservableCollection<CostContainer> getCosts()
        {
            return new ObservableCollection<CostContainer>() 
            {
                new CostContainer("Material Cost", Estimate.TECMaterialCost),
                new CostContainer("Labor Cost", Estimate.TECLaborCost),
                new CostContainer("Sub. Labor Cost", Estimate.SubcontractorLaborCost),
                new CostContainer("Sub. Material Cost", Estimate.ElectricalMaterialCost)
            };
        }
    }

    public class CostContainer
    {
        public string Name { get; set; }
        public double Cost { get; set; }
        public CostContainer(string name, double cost)
        {
            Name = name;
            Cost = cost;
        }
    }
}