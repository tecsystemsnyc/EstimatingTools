using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.ViewModels
{
    public class InterlocksVM : ViewModelBase
    {
        private TECElectricalMaterial noneConduit;
        private double _length = 0.0;
        private double _conduitLength = 0.0;
        private TECElectricalMaterial _conduitType;
        private bool _isPlenum = false;
        private string _name = "";

        public IInterlockable Parent { get; }
        public RelayCommand AddInterlock { get; private set; }
        public ObservableCollection<TECConnectionType> ConnectionTypes { get; } 
            = new ObservableCollection<TECConnectionType>();
        public List<TECElectricalMaterial> ConduitTypes { get; }

        public double Length
        {
            get { return _length; }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
            } 
        }
        public double ConduitLength
        {
            get { return _conduitLength; }
            set
            {
                _conduitLength = value;
                RaisePropertyChanged("ConduitLength");
            }
        }
        public TECElectricalMaterial ConduitType
        {
            get { return _conduitType; }
            set
            {
                _conduitType = value;
                RaisePropertyChanged("ConduitType");
            }
        } 
        public bool IsPlenum
        {
            get { return _isPlenum; }
            set
            {
                _isPlenum = value;
                RaisePropertyChanged("IsPlenum");
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        public InterlocksVM(IInterlockable interlockable, IEnumerable<TECElectricalMaterial> conduitTypes)
        {
            noneConduit = new TECElectricalMaterial();
            noneConduit.Name = "None";
            this.Parent = interlockable; 
            this.AddInterlock = new RelayCommand(addInterlockExecute, canAddInterlock);
            this.ConduitTypes = new List<TECElectricalMaterial>(conduitTypes);
            this.ConduitTypes.Add(noneConduit);
            this.ConduitType = noneConduit;

        }

        private void addInterlockExecute()
        {
            var protocol = new TECHardwiredProtocol(ConnectionTypes);
            var typical = Parent as ITypicalable;
            TECInterlockConnection connection = new TECInterlockConnection(protocol, typical?.IsTypical ?? false);
            connection.Name = Name;
            connection.Length = Length;
            if(ConduitType != noneConduit) { connection.ConduitType = ConduitType; }
            connection.ConduitLength = ConduitLength;
            connection.IsPlenum = IsPlenum;

            Parent.Interlocks.Add(connection);

        }

        private bool canAddInterlock()
        {
            return ConnectionTypes.Count != 0;
        }
    }
}
