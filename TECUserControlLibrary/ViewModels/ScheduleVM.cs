using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
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

        public ICommand AddTableCommand { get; private set; }

        public ScheduleVM(TECSchedule schedule)
        {
            _schedule = schedule;
            AddTableCommand = new RelayCommand(addTableExecute, canAddTable);
        }
        public void Refresh(TECSchedule schedule)
        {
            Schedule = schedule;
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
