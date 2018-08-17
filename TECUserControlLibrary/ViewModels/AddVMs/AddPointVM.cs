using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels.AddVMs
{
    public class AddPointVM : AddVM
    {
        private TECSubScope parent;
        private TECPoint toAdd;

        public TECPoint ToAdd
        {
            get { return toAdd; }
            private set
            {
                toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        public List<IOType> PossibleTypes { get; } = TECIO.PointIO;

        public AddPointVM(TECSubScope parentSubScope, TECScopeManager scopeManager) : base(scopeManager)
        {
            parent = parentSubScope;
            toAdd = new TECPoint();
            ToAdd.Quantity = 1;
            AddCommand = new RelayCommand(addExecute, addCanExecute);
        }

        private bool addCanExecute()
        {
            IControllerConnection connection = parent.Connection;
            if(connection == null)
            {
                return true;
            }
            else
            {
                TECIO proposedIO = new TECIO(ToAdd.Type);
                proposedIO.Quantity = ToAdd.Quantity;
                bool containsIO = connection.ParentController.AvailableIO.Contains(proposedIO);
                bool isNetworkIO = connection is TECNetworkConnection netConnect;
                return containsIO || isNetworkIO;
            }
        }
        private void addExecute()
        {
            var newPoint = new TECPoint(ToAdd);
            parent.AddPoint(newPoint);
            if (parent.Connection != null && parent.Connection is TECHardwiredConnection hardwiredConnection)
            {
                var parentController = parent.Connection.ParentController;
                var length = hardwiredConnection.Length;
                var conduitLength = hardwiredConnection.ConduitLength;
                var conduit = hardwiredConnection.ConduitType;
                var isPlenum = hardwiredConnection.IsPlenum;
                var protocol = hardwiredConnection.Protocol;

                parentController.Disconnect(parent);
                var newConnection = parentController.Connect(parent, protocol);
                newConnection.Length = length;
                newConnection.ConduitLength = conduitLength;
                newConnection.ConduitType = conduit;
                newConnection.IsPlenum = isPlenum;

            }

            Added?.Invoke(newPoint);

        }
        

        internal void SetTemplate(TECPoint point)
        {
            ToAdd = new TECPoint(point);
        }
    }
}
