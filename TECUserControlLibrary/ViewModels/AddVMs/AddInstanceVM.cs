using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels.AddVMs
{
    public class AddInstanceVM : AddVM
    {
        private TECTypical parent;
        private TECBid bid;
        private TECSystem toAdd;
        private ObservableCollection<NameConatiner> _names = new ObservableCollection<NameConatiner>();
        private bool _labelInstances = true;

        private string _patternName = "";
        private int _patternStart = 1;
        private int _patternEnd = 1;
        private TECLocation _patternLocation = null;
        private bool _includeLocationTag = true;
        
        public TECSystem ToAdd
        {
            get { return toAdd; }
            private set
            {
                toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        
        public ObservableCollection<NameConatiner> Names
        {
            get { return _names; }
            set
            {
                _names = value;
                RaisePropertyChanged("Names");
            }
        }
        public ObservableCollection<TECLocation> Locations { get; }
        public bool LabelInstances
        {
            get { return _labelInstances; }
            set
            {
                _labelInstances = value;
                RaisePropertyChanged("LabelInstances");
            }
        }

        public string PatternName
        {
            get { return _patternName; }
            set
            {
                if (_patternName != value)
                {
                    _patternName = value;
                    RaisePropertyChanged("PatternName");
                }
            }
        }
        public int PatternStart
        {
            get { return _patternStart; }
            set
            {
                if (_patternStart != value)
                {
                    _patternStart = value;
                    RaisePropertyChanged("PatternStart");
                }
            }
        }
        public int PatternEnd
        {
            get { return _patternEnd; }
            set
            {
                if (_patternEnd != value)
                {
                    _patternEnd = value;
                    RaisePropertyChanged("PatternEnd");
                }
            }
        }
        public TECLocation PatternLocation
        {
            get { return _patternLocation; }
            set
            {
                if (PatternLocation != value)
                {
                    _patternLocation = value;
                    RaisePropertyChanged("PatternLocation");
                }
            }
        }
        public bool IncludeLocationTag
        {
            get { return _includeLocationTag; }
            set
            {
                if (_includeLocationTag != value)
                {
                    _includeLocationTag = value;
                    RaisePropertyChanged("IncludeLocationTag");
                }
            }
        }

        public ICommand AddPatternCommand { get; }
        
        public AddInstanceVM(TECTypical typical, TECBid bid) : base(bid)
        {
            toAdd = new TECSystem(typical, bid);
            parent = typical;
            this.bid = bid;
            Locations = this.bid.Locations;
            AddCommand = new RelayCommand(addExecute, canAdd);
            AddPatternCommand = new RelayCommand(addPatternExecute, canAddPattern);
        }

        private void addExecute()
        {
            foreach(NameConatiner item in Names)
            {
                TECSystem newSystem = parent.AddInstance();
                newSystem.Name = item.Name;
                if (item.Location != null)
                {
                    newSystem.Location = item.Location;
                    foreach (var equip in newSystem.Equipment)
                    {
                        equip.Location = item.Location;
                        foreach(var subScope in equip.SubScope)
                        {
                            subScope.Location = item.Location;
                        }
                    }
                    foreach(var controller in newSystem.Controllers)
                    {
                        controller.Location = item.Location;
                    }
                    foreach(var panel in newSystem.Panels)
                    {
                        panel.Location = item.Location;
                    }
                }
                if (LabelInstances)
                {
                    foreach(TECController controller in newSystem.Controllers)
                    {
                        controller.Name += String.Format(" ({0})", item.Name);
                    }
                    foreach(TECPanel panel in newSystem.Panels)
                    {
                        panel.Name += String.Format(" ({0})", item.Name);
                    }
                }
            }
        }
        private bool canAdd()
        {
            if(Names.Count > 0)
            {
                return true;
            }
            return false;
        }

        private void addPatternExecute()
        {
            for(int x = PatternStart; x <= PatternEnd; x++)
            {
                NameConatiner newItem = new NameConatiner();
                if (IncludeLocationTag && PatternLocation != null && PatternLocation.Label != "")
                {
                    newItem.Name = String.Format("{0}-{1}-{2}", PatternName, PatternLocation.Label, x);
                }
                else
                {
                    newItem.Name = String.Format("{0}-{1}", PatternName, x);
                }
                newItem.Location = PatternLocation;                
                Names.Add(newItem);
            }
            int nextIndex = Locations.IndexOf(PatternLocation) + 1;
            if (nextIndex > 0 && nextIndex < Locations.Count)
            {
                PatternLocation = Locations[nextIndex];
            }

        }
        private bool canAddPattern()
        {
            bool start = PatternStart >= 0;
            bool end = PatternEnd >= PatternStart;
            bool name = (PatternName != null && PatternName != "");
            return (start && end && name);
        }
    }

    public class NameConatiner : ViewModelBase
    {

        private String _name = "";
        private TECLocation _location = null;

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public TECLocation Location
        {
            get { return _location; }
            set
            {
                _location = value;
                RaisePropertyChanged("Location");
            }
        }
    }
}
