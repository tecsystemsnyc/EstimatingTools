using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.ViewModels
{
    public class BidParametersVM : ViewModelBase
    {

        private TECBid _bid;

        public TECBid Bid
        {
            get { return _bid; }
            set
            {
                _bid = value;
                RaisePropertyChanged("Bid");
            }
        }

        public BidParametersVM(TECBid bid)
        {
            Bid = bid;
        }

        public void Refresh(TECBid bid)
        {
            Bid = bid;
        }
    }
}
