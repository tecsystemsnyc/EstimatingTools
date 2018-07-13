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
        
        public String TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                RaisePropertyChanged("TableName");
            }
        }
        public TECSchedule Schedule { get; }
        public ObservableCollection<TECScope> ScopeCollection { get; }
        public ObservableCollection<TECLocation> Locations { get; }

        public ICommand AddTableCommand { get; private set; }

        public ScheduleVM(TECBid bid, ChangeWatcher watcher)
        {
            this.Schedule = bid.Schedule;
            this.AddTableCommand = new RelayCommand(addTableExecute, canAddTable);
            watcher.Changed += changed;
            this.ScopeCollection = populateScopeCollection(bid);
            this.Locations = bid.Locations;
        }

        private ObservableCollection<TECScope> populateScopeCollection(TECBid bid)
        {
            var outCollection = new ObservableCollection<TECScope>();
            foreach(TECTypical typical in bid.Systems)
            {
                foreach(TECSystem system in typical.Instances)
                {
                    outCollection.Add(system);
                }
            }
            return outCollection;
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
