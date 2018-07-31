using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.Utilities.DropTargets;

namespace TECUserControlLibrary.ViewModels
{
    public class QuotesVM: ViewModelBase
    {
        ObservableCollection<TECHardware> NeedQuoteHardware { get; } = new ObservableCollection<TECHardware>();
        ObservableCollection<TECHardware> QuotedHardware { get; } = new ObservableCollection<TECHardware>();
        IDropTarget QuoteDropHandler { get; }

        public QuotesVM(TECBid bid)
        {
            NeedQuoteHardware = new ObservableCollection<TECHardware>(bid
                .GetAll<TECSubScope>()
                .SelectMany(ss => ss.Devices
                .Where(x => x is TECHardware ware && ware.RequireQuote && ware.QuotedPrice == -1))
                .OfType<TECHardware>());
            QuotedHardware = new ObservableCollection<TECHardware>(bid.Catalogs.GetAll<TECHardware>().Where(x => x.QuotedPrice != -1));
            var dropHandler = new EmptyDropTarget();
            dropHandler.DragOverAction = info =>
            {
                UIHelpers.DragOver(info, (item, sourceType, targetType) =>
                {
                    var hardware = item as TECHardware;
                    return sourceType == targetType && hardware != null && hardware.QuotedPrice != -1;
                });
            };
            dropHandler.DropAction = info =>
            {
                UIHelpers.Drop(info, item =>
                {
                    var hardware = item as TECHardware;
                    hardware.QuotedPrice = hardware.Cost;
                    return hardware;
                });
            };
        }


    }
}
