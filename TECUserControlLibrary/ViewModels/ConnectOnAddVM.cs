﻿using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectOnAddVM : ViewModelBase
    {
        private List<IConnectable> toConnect;
        private List<TECController> controllers;
        private double _length = 0.0;
        private double _conduitLength = 0.0;
        private TECElectricalMaterial _conduitType;
        private bool _isPlenum = false;
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
        
        public ConnectOnAddVM(IEnumerable<IConnectable> toConnect, IEnumerable<TECController> controllers, 
            IEnumerable<TECElectricalMaterial> conduitTypes)
        {
            this.toConnect = new List<IConnectable>(toConnect);
            this.controllers = new List<TECController>(controllers);
            this.ConduitTypes =  new List<TECElectricalMaterial>(conduitTypes);
            ParentControllers = getCompatibleControllers(controllers);
        }

        private List<TECController> getCompatibleControllers(IEnumerable<TECController> controllers)
        {
            List<TECController> result = new List<TECController>();
            foreach(TECController controller in controllers)
            {
                if (ConnectionHelper.CanConnectToController(toConnect, controller))
                {
                    result.Add(controller);
                }
            }
            return result;
        }

        public void Update(IEnumerable<IConnectable> toConnect)
        {
            this.toConnect = new List<IConnectable>(toConnect);
            ParentControllers = getCompatibleControllers(controllers);
            if (!ParentControllers.Contains(SelectedController))
            {
                SelectedController = null;
            }
            RaisePropertyChanged("ParentControllers");
        }
        public void ExecuteConnection(IEnumerable<IConnectable> finalToConnect)
        {
            var connections = ConnectionHelper.ConnectToController(finalToConnect, SelectedController);

            foreach (IControllerConnection conn in connections)
            {
                setConnectionProperties(conn);
            }
        }
        public void ExecuteConnection(TECSubScope finalToConnect)
        {
            ExecuteConnection(new List<IConnectable> { finalToConnect });
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

        private void setConnectionProperties(IControllerConnection connection)
        {
            connection.Length += Length;
            connection.ConduitLength += ConduitLength;
            if (connection.ConduitType == null)
            {
                connection.ConduitType = ConduitType;
            }
            if (!connection.IsPlenum)
            {
                connection.IsPlenum = IsPlenum;
            }
        }
    }
}
