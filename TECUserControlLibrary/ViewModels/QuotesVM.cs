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
        public ObservableCollection<TECHardware> NeedQuoteHardware { get; } = new ObservableCollection<TECHardware>();
        public ObservableCollection<TECHardware> QuotedHardware { get; } = new ObservableCollection<TECHardware>();
        public IDropTarget QuoteDropHandler { get; }
        public ScopeCollectionsTabVM CollectionsVM { get; }

        public QuotesVM(TECBid bid)
        {
            NeedQuoteHardware = new ObservableCollection<TECHardware>(bid
                .GetAll<TECSubScope>()
                .SelectMany(ss => ss.Devices
                .Where(x => x is TECHardware ware && ware.RequireQuote && ware.QuotedPrice == -1))
                .Distinct()
                .OfType<TECHardware>());
            QuotedHardware = new ObservableCollection<TECHardware>(bid.Catalogs.GetAll<TECHardware>().Where(x => x.QuotedPrice != -1).Distinct());
            QuotedHardware.CollectionChanged += quotedHardware_CollectionChanged;
            var dropHandler = new EmptyDropTarget();
            dropHandler.DragOverAction = info =>
            {
                UIHelpers.DragOver(info, (item, sourceType, targetType) =>
                {
                    var hardware = item as TECHardware;
                    return hardware != null && hardware.QuotedPrice == -1;
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
            QuoteDropHandler = dropHandler;
            CollectionsVM = new ScopeCollectionsTabVM(bid);
            CollectionsVM.OmitCollections(new List<AllSearchableObjects>() {
                AllSearchableObjects.System,
                AllSearchableObjects.Equipment,
                AllSearchableObjects.SubScope,
                AllSearchableObjects.Controllers,
                AllSearchableObjects.Panels,
                AllSearchableObjects.MiscCosts,
                AllSearchableObjects.MiscWiring,
                AllSearchableObjects.Tags,
                AllSearchableObjects.AssociatedCosts,
                AllSearchableObjects.Wires,
                AllSearchableObjects.Conduits,
                AllSearchableObjects.Protocols
            });
        }

        private void quotedHardware_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach(TECHardware item in e.NewItems)
                {
                    NeedQuoteHardware.Remove(item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach(TECHardware item in e.OldItems)
                {
                    if(item.RequireQuote) NeedQuoteHardware.Add(item);
                }
            }
        }
        
    }
}
