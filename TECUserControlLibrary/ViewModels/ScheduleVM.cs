using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class ScheduleVM : ViewModelBase
    {
        private String _tableName = "";
        private TECSchedule _schedule;
        private ObservableCollection<TECScope> _scopeCollection;
        
        public String TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                RaisePropertyChanged("TableName");
            }
        }
        public TECSchedule Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
                RaisePropertyChanged("Schedule");
            }
        }
        public ObservableCollection<TECScope> ScopeCollection
        {
            get { return _scopeCollection; }
            set
            {
                _scopeCollection = value;
                RaisePropertyChanged("ScopeCollection");
            }
        }
        public ObservableCollection<TECLocation> Locations { get; }

        public ICommand AddTableCommand { get; private set; }

        public ScheduleVM(TECBid bid, ChangeWatcher watcher)
        {
            _schedule = bid.Schedule;
            AddTableCommand = new RelayCommand(addTableExecute, canAddTable);
            watcher.Changed += changed;
            populateScopeCollection(bid);
            this.Locations = bid.Locations;
        }

        private void populateScopeCollection(TECBid bid)
        {
            ScopeCollection = new ObservableCollection<TECScope>();
            foreach(TECTypical typical in bid.Systems)
            {
                foreach(TECSystem system in typical.Instances)
                {
                    ScopeCollection.Add(system);
                }
            }
        }

        private void changed(TECChangedEventArgs e)
        {
            if(e.Sender is TECTypical && e.Value is TECSystem system)
            {
                if(e.Change == Change.Add)
                {
                    ScopeCollection.Add(system);
                }
                else if(e.Change == Change.Remove)
                {
                    ScopeCollection.Remove(system);
                }
            }
        }

        public void Refresh(TECBid bid, ChangeWatcher watcher)
        {
            Schedule = bid.Schedule;
            watcher.Changed += changed;
            populateScopeCollection(bid);
        }

        private void addTableExecute()
        {
            TECScheduleTable table = new TECScheduleTable();
            table.Name = TableName;
            Schedule.Tables.Add(table);
            TableName = "";

        }

        private bool canAddTable()
        {
            return TableName != "";
        }

    }
}
