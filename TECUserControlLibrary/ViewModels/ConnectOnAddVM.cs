using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectOnAddVM : ViewModelBase
    {
        private List<TECSubScope> toConnect;
        private TECSystem parent;
        private double _length;
        private double _conduitLength;
        private TECElectricalMaterial _conduitType;
        private bool _isPlenum;
        private bool _connect = false;
        private bool _connectNetwork = false;
        private TECController _selectedController;

        public List<TECController> ParentControllers { get; private set; }
        public List<TECElectricalMaterial> ConduitTypes { get; private set; }
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
        public bool Connect
        {
            get { return _connect; }
            set
            {
                _connect = value;
                RaisePropertyChanged("Connect");
            }
        }
        public bool ConnectNetwork
        {
            get { return _connectNetwork; }
            set
            {
                if (ConnectNetwork != value)
                {
                    _connectNetwork = value;
                    RaisePropertyChanged("ConnectNetwork");
                    ParentControllers = getCompatibleControllers(parent);
                    RaisePropertyChanged("ParentControllers");
                }
            }
        }
        public TECController SelectedController
        {
            get { return _selectedController; }
            set
            {
                _selectedController = value;
                RaisePropertyChanged("SelectedController");
            }
        }

        public List<IOTypeConnection> NewNetConnections { get; }
        public List<TECConnectionType> ConnectionTypes { get; }
        
        public ConnectOnAddVM(IEnumerable<TECSubScope> toConnect, TECSystem parent, IEnumerable<TECElectricalMaterial> conduitTypes, IEnumerable<TECConnectionType> connectionTypes)
        {
            this.toConnect = new List<TECSubScope>(toConnect);
            this.parent = parent;
            this.ConduitTypes =  new List<TECElectricalMaterial>(conduitTypes);
            ParentControllers = getCompatibleControllers(parent);

            NewNetConnections = new List<IOTypeConnection>();
            ConnectionTypes = new List<TECConnectionType>(connectionTypes);
            parseNetworkIOTypes(toConnect);
        }

        private List<TECController> getCompatibleControllers(TECSystem parent)
        {
            List<TECController> result = new List<TECController>();
            foreach(TECController controller in parent.Controllers)
            {
                bool containsIO = true;
                if (ConnectNetwork)
                {
                    foreach(IOTypeConnection type in NewNetConnections)
                    {
                        if (!controller.AvailableNetworkIO.Contains(type.IOType))
                        {
                            containsIO = false;
                            break;
                        }
                    }
                }
                if (controller.CanConnectSubScope(toConnect) && containsIO)
                {
                    result.Add(controller);
                }
            }
            return result;
        }

        public void Update(IEnumerable<TECSubScope> toConnect)
        {
            this.toConnect = new List<TECSubScope>(toConnect);
            ParentControllers = getCompatibleControllers(parent);
            NewNetConnections.Clear();
            parseNetworkIOTypes(toConnect);
            RaisePropertyChanged("NewNetworkConnections");
            RaisePropertyChanged("ParentControllers");
        }

        public void ExecuteConnection(IEnumerable<TECSubScope> finalToConnect)
        {
            if (Connect && SelectedController != null)
            {
                foreach (TECSubScope subScope in finalToConnect)
                {
                    ExecuteConnection(subScope);
                }
            }
            
        }
        public void ExecuteConnection(TECSubScope finalToConnect)
        {
            var connection = SelectedController.AddSubScope(finalToConnect);
            connection.ConduitLength = ConduitLength;
            connection.Length = Length;
            connection.ConduitType = ConduitType;
            connection.IsPlenum = IsPlenum;
        }
        public bool CanConnect()
        {
            if (Connect)
            {
                bool connectionTypesChosen = true;
                if (ConnectNetwork)
                {
                    foreach(IOTypeConnection type in NewNetConnections)
                    {
                        if (type.WireType == null)
                        {
                            connectionTypesChosen = false;
                            break;
                        }
                    }
                }
                return (SelectedController != null && connectionTypesChosen);
            }
            else
            {
                return true;
            }
        }

        private void parseNetworkIOTypes(IEnumerable<TECSubScope> subScope)
        {
            Dictionary<IOType, int> includedIO = new Dictionary<IOType, int>();
            foreach(TECSubScope ss in subScope)
            {
                if (ss.IsNetwork)
                {
                    if (ss.IO.ListIO().Count() > 1) throw new DataMisalignedException("SubScope has more than one network IO type.");

                    IOType ssType = ss.IO.ListIO()[0].Type;
                    if (!includedIO.ContainsKey(ssType))
                    {
                        includedIO.Add(ssType, 1);
                    }
                    else
                    {
                        includedIO[ssType]++;
                    }
                }
            }
            foreach(KeyValuePair<IOType, int> incIO in includedIO)
            {
                IOTypeConnection ioConnect = new IOTypeConnection(incIO.Key, incIO.Value);
                NewNetConnections.Add(ioConnect);
            }
        }

        public class IOTypeConnection
        {
            public IOType IOType { get; }
            public int NumDevices { get; }

            public TECConnectionType WireType { get; set; }

            public IOTypeConnection(IOType type, int numDevices)
            {
                IOType = type;
                NumDevices = numDevices;
            }
        }
    }
}
