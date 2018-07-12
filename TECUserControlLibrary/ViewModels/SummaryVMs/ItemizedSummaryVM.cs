using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;

namespace TECUserControlLibrary.ViewModels.SummaryVMs
{
    public class ItemizedSummaryVM : ViewModelBase
    {
        public SystemSummaryVM SystemVM { get; }

        public ItemizedSummaryVM(TECBid bid, ChangeWatcher watcher)
        {
            SystemVM = new SystemSummaryVM(bid, watcher);
        }
        
    }
}
