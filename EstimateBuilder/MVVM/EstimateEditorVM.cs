using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows.Input;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.ViewModels;
using TECUserControlLibrary.ViewModels.SummaryVMs;

namespace EstimateBuilder.MVVM
{
    public class EstimateEditorVM : ViewModelBase, EditorVM
    {
        public ScopeEditorVM ScopeEditorVM { get; }
        public LaborVM LaborVM { get; }
        public ReviewVM ReviewVM { get; }
        public ProposalVM ProposalVM { get; }
        public ItemizedSummaryVM ItemizedSummaryVM { get; }
        public MaterialSummaryVM MaterialSummaryVM { get; private set; }
        public RiserVM RiserVM { get; }
        public ScheduleVM ScheduleVM { get; }
        public BidPropertiesVM BidPropertiesVM { get; }
        public InternalNotesVM InternalNotesVM { get; }
        public QuotesVM QuotesVM { get; }

        public ICommand RefreshMaterialSummaryCommand { get; private set; }

        private TECBid bid;
        private ChangeWatcher watcher;
        
        public EstimateEditorVM(TECBid bid, ChangeWatcher watcher, TECEstimator estimate)
        {
            this.bid = bid;
            this.watcher = watcher;

            ScopeEditorVM = new ScopeEditorVM(bid, watcher);
            LaborVM = new LaborVM(bid, estimate);
            ReviewVM = new ReviewVM(bid, estimate);
            ProposalVM = new ProposalVM(bid);
            ItemizedSummaryVM = new ItemizedSummaryVM(bid, watcher);
            MaterialSummaryVM = new MaterialSummaryVM(bid, watcher);
            RiserVM = new RiserVM(bid, watcher);
            ScheduleVM = new ScheduleVM(bid, watcher);
            BidPropertiesVM = new BidPropertiesVM(bid);
            InternalNotesVM = new InternalNotesVM(bid);
            QuotesVM = new QuotesVM(bid, watcher);

            RefreshMaterialSummaryCommand = new RelayCommand(executeMaterialRefersh);
        }

        private void executeMaterialRefersh()
        {
            MaterialSummaryVM = new MaterialSummaryVM(bid, watcher);
            RaisePropertyChanged("MaterialSummaryVM");
        }
    }
}
