﻿using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using TECUserControlLibrary.BaseVMs;
using TECUserControlLibrary.ViewModels;
using TECUserControlLibrary.ViewModels.SummaryVMs;

namespace EstimateBuilder.MVVM
{
    public class EstimateEditorVM : EditorVM
    {
        public ScopeEditorVM ScopeEditorVM { get; }
        public LaborVM LaborVM { get; }
        public ReviewVM ReviewVM { get; }
        public ProposalVM ProposalVM { get; }
        public ItemizedSummaryVM ItemizedSummaryVM { get; }
        public MaterialSummaryVM MaterialSummaryVM { get; }
        public RiserVM RiserVM { get; }
        
        public EstimateEditorVM(TECBid bid, TECTemplates templates, ChangeWatcher watcher, TECEstimator estimate)
        {
            ScopeEditorVM = new ScopeEditorVM(bid, templates, watcher);
            LaborVM = new LaborVM(bid, templates, estimate);
            ReviewVM = new ReviewVM(bid, estimate);
            ProposalVM = new ProposalVM(bid);
            ItemizedSummaryVM = new ItemizedSummaryVM(bid, watcher);
            MaterialSummaryVM = new MaterialSummaryVM(bid, watcher);
            RiserVM = new RiserVM(bid, watcher);
        }

        public void Refresh(TECBid bid, TECTemplates templates, ChangeWatcher watcher, TECEstimator estimate)
        {
            ScopeEditorVM.Refresh(bid, templates, watcher);
            LaborVM.Refresh(bid, estimate, templates);
            ReviewVM.Refresh(estimate, bid);
            ProposalVM.Refresh(bid);
            ItemizedSummaryVM.Refresh(bid, watcher);
            MaterialSummaryVM.Refresh(bid, watcher);
            RiserVM.Refresh(bid, watcher);
        }
    }
}
