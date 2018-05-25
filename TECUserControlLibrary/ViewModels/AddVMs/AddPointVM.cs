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
        private bool isTypical;

        public TECPoint ToAdd
        {
            get { return toAdd; }
            private set
            {
                toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        public List<IOType> PossibleTypes { get; }

        public AddPointVM(TECSubScope parentSubScope, TECScopeManager scopeManager) : base(scopeManager)
        {
            parent = parentSubScope;
            isTypical = parentSubScope.IsTypical;
            toAdd = new TECPoint(parentSubScope.IsTypical);
            ToAdd.Quantity = 1;

            //Set the IOTypes that the user can use for the new Point.
            PossibleTypes = TECIO.PointIO;

            if (PossibleTypes.Count > 0)
            {
                ToAdd.Type = PossibleTypes[0];
            }
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
            var newPoint = new TECPoint(ToAdd, isTypical);
            parent.Points.Add(newPoint);
            Added?.Invoke(newPoint);
        }
        

        internal void SetTemplate(TECPoint point)
        {
            ToAdd = new TECPoint(point, isTypical);
        }
    }
}
