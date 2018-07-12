using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.ViewModels
{
    public class BidPropertiesVM : ViewModelBase
    {
        public TECBid Bid { get; }

        public BidPropertiesVM(TECBid bid)
        {
            Bid = bid;
        }
        
    }
}
