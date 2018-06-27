using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels.SummaryVMs
{
    public class SystemSummaryVM : ViewModelBase
    {
        private ObservableCollection<SystemSummaryItem> _systems;
        private ObservableCollection<ScopeSummaryItem> _riser;
        private ObservableCollection<ScopeSummaryItem> _misc;
        private TECBid bid;

        private SystemSummaryItem _selectedSystem;
        private ScopeSummaryItem _selectedRiser;
        private ScopeSummaryItem _selectedMisc;
        private ScopeSummaryItem _selected;
        
        public SystemSummaryItem SelectedSystem
        {
            get { return _selectedSystem; }
            set
            {
                _selectedSystem = value;
                RaisePropertyChanged("SelectedSystem");
                Selected = value != null ? new ScopeSummaryItem(new TECSystem(value.Typical, false, bid), bid.Parameters, bid.Duration) : null;
            }
        }
        public ScopeSummaryItem SelectedRiser
        {
            get { return _selectedRiser; }
            set
            {
                _selectedRiser = value;
                RaisePropertyChanged("SelectedRiser");
                Selected = SelectedRiser;
            }
        }
        public ScopeSummaryItem SelectedMisc
        {
            get { return _selectedMisc; }
            set
            {
                _selectedMisc = value;
                RaisePropertyChanged("SelectedMisc");
                Selected = SelectedMisc;
            }
        }
        public ScopeSummaryItem Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }
        
        public ObservableCollection<SystemSummaryItem> Systems
        {
            get { return _systems; }
            set
            {
                _systems = value;
                RaisePropertyChanged("Systems");
            }
        }
        public ObservableCollection<ScopeSummaryItem> Riser
        {
            get { return _riser; }
            set
            {
                _riser = value;
                RaisePropertyChanged("Riser");
            }
        }
        public ObservableCollection<ScopeSummaryItem> Misc
        {
            get { return _misc; }
            set
            {
                _misc = value;
                RaisePropertyChanged("Misc");
            }
        }
    
        public double SystemTotal
        {
            get { return getSystemTotal(); }
        }
        
        public double RiserTotal
        {
            get { return getRiserTotal(); }
        }
        
        public double MiscTotal
        {
            get { return getMiscTotal(); }
        }


        private TECEstimator _extraLaborEstimate;

        public TECEstimator ExtraLaborEstimate
        {
            get { return _extraLaborEstimate; }
            set
            {
                _extraLaborEstimate = value;
                RaisePropertyChanged("ExtraLaborEstimate");
            }
        }
        
        public SystemSummaryVM(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            setupExtraLaborEstimate(bid);
            populateAll(bid);
            watcher.Changed += changed;
        }
        
        private void setupExtraLaborEstimate(TECBid bid)
        {
            ExtraLaborEstimate = new TECEstimator(new TECPoint(), bid.Parameters, bid.ExtraLabor, bid.Duration, new ChangeWatcher(bid.ExtraLabor));
        }
        private void populateAll(TECBid bid)
        {
            populateSystems(bid.Systems);
            populateRiser(bid.Controllers, bid.Panels);
            populateMisc(bid.MiscCosts);
        }

        private void changed(TECChangedEventArgs e)
        {
            if(e.Value is TECTypical typical)
            {
                if (e.Change == Change.Add)
                {
                    SystemSummaryItem summaryItem = new SystemSummaryItem(typical, bid.Parameters);
                    summaryItem.Estimate.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "TotalPrice")
                        {
                            RaisePropertyChanged("SystemTotal");
                        }
                    };
                    Systems.Add(summaryItem);

                }
                else if (e.Change == Change.Remove)
                {
                    SystemSummaryItem toRemove = null;
                    foreach(var item in Systems)
                    {
                        if(item.Typical == typical)
                        {
                            toRemove = item;
                            break;
                        }
                    }
                    if(toRemove != null )
                        Systems.Remove(toRemove);
                }
                RaisePropertyChanged("SystemTotal");
            }
            else if(e.Sender is TECBid)
            {
                if(e.Change == Change.Add)
                {
                    if (e.Value is TECController || e.Value is TECPanel)
                    {
                        ScopeSummaryItem summaryItem = new ScopeSummaryItem(e.Value as TECScope, bid.Parameters);
                        summaryItem.Estimate.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == "TotalPrice")
                            {
                                RaisePropertyChanged("RiserTotal");
                            }
                        };
                        Riser.Add(summaryItem);
                    }
                    else if (e.Value is TECMisc misc)
                    {
                        ScopeSummaryItem summaryItem = new ScopeSummaryItem(misc, bid.Parameters);
                        summaryItem.Estimate.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == "TotalPrice")
                            {
                                RaisePropertyChanged("MiscTotal");
                            }
                        };
                        Misc.Add(summaryItem);
                    }
                }
                else if (e.Change == Change.Remove)
                {
                    if (e.Value is TECController || e.Value is TECPanel)
                    {
                        removeFromCollection(Riser, e.Value as TECScope);
                    }
                    else if (e.Value is TECMisc misc)
                    {
                        removeFromCollection(Misc, misc);
                    }
                }
                if(e.PropertyName == "Duration")
                {
                    populateAll(bid);
                    setupExtraLaborEstimate(bid);
                }
                RaisePropertyChanged("RiserTotal");
                RaisePropertyChanged("MiscTotal");
            }
        }

        private void removeFromCollection(ObservableCollection<ScopeSummaryItem> list, TECScope scope)
        {
            ScopeSummaryItem toRemove = null;
            foreach (var item in list)
            {
                if (item.Scope == scope)
                {
                    toRemove = item;
                    break;
                }
            }
            if (toRemove != null)
                list.Remove(toRemove);
        }

        private void populateSystems(ObservableCollection<TECTypical> typicals)
        {
            ObservableCollection<SystemSummaryItem> systemItems = new ObservableCollection<SystemSummaryItem>();
            foreach(TECTypical typical in typicals)
            {
                SystemSummaryItem summaryItem = new SystemSummaryItem(typical, bid.Parameters, bid.Duration);
                summaryItem.Estimate.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "TotalPrice")
                    {
                        RaisePropertyChanged("SystemTotal");
                    }
                };
                systemItems.Add(summaryItem);
            }
            Systems = systemItems;
            RaisePropertyChanged("SystemTotal");
        }
        private void populateMisc(ObservableCollection<TECMisc> miscCosts)
        {
            ObservableCollection<ScopeSummaryItem> miscItems = new ObservableCollection<ScopeSummaryItem>();
            foreach(TECMisc misc in miscCosts)
            {
                ScopeSummaryItem summaryItem = new ScopeSummaryItem(misc, bid.Parameters, bid.Duration);
                summaryItem.Estimate.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "TotalPrice")
                    {
                        RaisePropertyChanged("MiscTotal");
                    }
                };
                miscItems.Add(summaryItem);
            }
            Misc = miscItems;
            RaisePropertyChanged("MiscTotal");
        }
        private void populateRiser(ReadOnlyObservableCollection<TECController> controllers, ObservableCollection<TECPanel> panels)
        {
            ObservableCollection<ScopeSummaryItem> riserItems = new ObservableCollection<ScopeSummaryItem>();
            foreach (TECController controller in controllers)
            {
                ScopeSummaryItem summaryItem = new ScopeSummaryItem(controller, bid.Parameters, bid.Duration);
                summaryItem.Estimate.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "TotalPrice")
                    {
                        RaisePropertyChanged("RiserTotal");
                    }
                };
                riserItems.Add(summaryItem);
            }
            foreach(TECPanel panel in panels)
            {
                ScopeSummaryItem summaryItem = new ScopeSummaryItem(panel, bid.Parameters);
                summaryItem.Estimate.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "TotalPrice")
                    {
                        RaisePropertyChanged("RiserTotal");
                    }
                };
                riserItems.Add(summaryItem);
            }
            Riser = riserItems;
            RaisePropertyChanged("RiserTotal");
        }

        private double getSystemTotal()
        {
            return Systems.Sum(item => item.Estimate.TotalPrice);
        }
        private double getRiserTotal()
        {
            return Riser.Sum(item => item.Estimate.TotalPrice);
        }
        private double getMiscTotal()
        {
            return Misc.Sum(item => item.Estimate.TotalPrice);
        }
    }
}
