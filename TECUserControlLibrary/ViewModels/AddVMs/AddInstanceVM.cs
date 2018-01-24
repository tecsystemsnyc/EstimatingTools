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
        public bool LabelInstances
        {
            get { return _labelInstances; }
            set
            {
                _labelInstances = value;
                RaisePropertyChanged("LabelInstances");
            }
        }


        public AddInstanceVM(TECTypical typical, TECBid bid) : base(bid)
        {
            toAdd = new TECSystem(typical, false, bid);
            parent = typical;
            this.bid = bid; 
            AddCommand = new RelayCommand(addExecute, canAdd);
        }

        private void addExecute()
        {
            foreach(NameConatiner item in Names)
            {
                TECSystem newSystem = parent.AddInstance(bid);
                newSystem.Name = item.Name;
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
        
    }

    public class NameConatiner : ViewModelBase
    {

        private String _name;

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
    }
}
