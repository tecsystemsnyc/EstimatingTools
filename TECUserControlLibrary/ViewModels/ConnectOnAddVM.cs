using EstimatingLibrary;
using EstimatingLibrary.Utilities;
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
        public TECController SelectedController
        {
            get { return _selectedController; }
            set
            {
                _selectedController = value;
                RaisePropertyChanged("SelectedController");
            }
        }
        
        public ConnectOnAddVM(IEnumerable<TECSubScope> toConnect, TECSystem parent, IEnumerable<TECElectricalMaterial> conduitTypes, IEnumerable<TECConnectionType> connectionTypes)
        {
            this.toConnect = new List<TECSubScope>(toConnect);
            this.parent = parent;
            this.ConduitTypes =  new List<TECElectricalMaterial>(conduitTypes);
            ParentControllers = getCompatibleControllers(parent);
        }

        private List<TECController> getCompatibleControllers(TECSystem parent)
        {
            List<TECController> result = new List<TECController>();
            foreach(TECController controller in parent.Controllers)
            {
                bool allCompatible = true;

                foreach(TECSubScope ss in toConnect)
                {
                    if (ss.IsNetwork)
                    {
                        IOType ioType = ss.IO.ListIO()[0].Type;
                        if (!controller.TotalIO.Contains(ioType))
                        {
                            allCompatible = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!controller.CanConnectSubScope(ss))
                        {
                            allCompatible = false;
                            break;
                        }
                    }
                }

                if (allCompatible)
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
            RaisePropertyChanged("ParentControllers");
        }
        public void ExecuteConnection(IEnumerable<TECSubScope> finalToConnect)
        {
            foreach(TECSubScope subScope in finalToConnect)
            {
                ExecuteConnection(subScope);
            }
        }
        public void ExecuteConnection(TECSubScope finalToConnect)
        {
            connectControllerToSubScope(SelectedController, finalToConnect);
            if(parent is TECTypical typical && typical.Instances.Count > 0)
            {
                foreach (TECController instanceController
                    in typical.TypicalInstanceDictionary.GetInstances(SelectedController))
                {
                    foreach(TECSubScope instanceSubScope
                        in typical.TypicalInstanceDictionary.GetInstances(finalToConnect))
                    {
                        bool found = false;
                        foreach (TECSystem instance in typical.Instances)
                        {
                            if(instance.Controllers.Contains(instanceController) &&
                                instance.GetAllSubScope().Contains(instanceSubScope))
                            {
                                connectControllerToSubScope(instanceController, instanceSubScope);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                }
            }
        }
        
        private void connectControllerToSubScope(TECController controller, TECSubScope finalToConnect)
        {
            if (Connect && controller != null)
            {
                if (finalToConnect.IsNetwork)
                {
                    IOType ioType = finalToConnect.IO.ListIO()[0].Type;

                    bool compatibleConnectionExists = false;
                    foreach (TECNetworkConnection netConnect in controller.ChildNetworkConnections)
                    {
                        if (netConnect.CanAddINetworkConnectable(finalToConnect))
                        {
                            compatibleConnectionExists = true;
                            netConnect.AddINetworkConnectable(finalToConnect);
                            netConnect.Length += Length;
                            netConnect.ConduitLength += ConduitLength;
                            if (netConnect.ConduitType == null)
                            {
                                netConnect.ConduitType = ConduitType;
                            }
                            if (!netConnect.IsPlenum)
                            {
                                netConnect.IsPlenum = IsPlenum;
                            }
                            break;
                        }
                    }

                    if (!compatibleConnectionExists)
                    {
                        TECNetworkConnection newConnection = controller.AddNetworkConnection(parent.IsTypical, finalToConnect.ConnectionTypes, ioType);
                        newConnection.AddINetworkConnectable(finalToConnect);
                        newConnection.ConduitLength = ConduitLength;
                        newConnection.Length = Length;
                        newConnection.ConduitType = ConduitType;
                        newConnection.IsPlenum = IsPlenum;
                    }
                }
                else
                {
                    TECSubScopeConnection connection = controller.AddSubScope(finalToConnect);
                    connection.ConduitLength = ConduitLength;
                    connection.Length = Length;
                    connection.ConduitType = ConduitType;
                    connection.IsPlenum = IsPlenum;
                }
            }
        }
        public bool CanConnect()
        {
            if (Connect)
            {
                return (SelectedController != null);
            }
            else
            {
                return true;
            }
        }
    }
}
