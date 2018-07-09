using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.ViewModels.AddVMs
{
    public class AddSubScopeVM :  AddVM
    {
        private TECEquipment parent;
        private TECSubScope toAdd;
        private int quantity = 1;
        private Action<TECSubScope> add;
        private bool isTypical = false;
        private string _pointName = "";
        private int _pointQuantity = 1;
        private IOType _pointType = IOType.AI;
        private TECSubScope underlyingTemplate;
        private List<TECPoint> originalPoints = new List<TECPoint>();
        private List<IEndDevice> originalDevices = new List<IEndDevice>();
        private bool _displayReferenceProperty = false;
        private ConnectOnAddVM _connectVM;
        public List<IOType> PossibleTypes { get; private set; } = TECIO.PointIO;

        public TECSubScope ToAdd
        {
            get { return toAdd; }
            private set
            {
                toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                RaisePropertyChanged("Quantity");
                updateConnectVMWithQuantity(value);
            }
        }
        public string PointName
        {
            get { return _pointName; }
            set
            {
                _pointName = value;
                RaisePropertyChanged("PointName");
            }
        }
        public int PointQuantity
        {
            get { return _pointQuantity; }
            set
            {
                _pointQuantity = value;
                RaisePropertyChanged("PointQuantity");
            }
        }
        public IOType PointType
        {
            get { return _pointType; }
            set
            {
                _pointType = value;
                RaisePropertyChanged("PointType");
            }
        }
        public ICommand AddPointCommand { get; private set; }
        public RelayCommand<IEndDevice> DeleteDeviceCommand { get; private set; }
        public RelayCommand<TECPoint> DeletePointCommand { get; private set; }
        public bool DisplayReferenceProperty
        {
            get { return _displayReferenceProperty; }
            private set
            {
                _displayReferenceProperty = value;
                RaisePropertyChanged("DisplayReferenceProperty");
            }
        }
        public ConnectOnAddVM ConnectVM
        {
            get { return _connectVM; }
            set
            {
                _connectVM = value;
                RaisePropertyChanged("ConnectVM");
            }
        }
        
        public AddSubScopeVM(TECEquipment parentEquipment, TECScopeManager scopeManager) : base(scopeManager)
        {
            parent = parentEquipment;
            toAdd = new TECSubScope();
            add = subScope =>
            {
                parent.SubScope.Add(subScope);

            };
            setup();
        }

        public AddSubScopeVM(Action<TECSubScope> addMethod, TECScopeManager scopeManager): base(scopeManager)
        {
            add = addMethod;
            toAdd = new TECSubScope();
            setup();
            PropertiesVM.DisplayReferenceProperty = false;
        }

        public void SetParentSystem(TECSystem system, TECScopeManager scopeManager)
        {
            ConnectVM = new ConnectOnAddVM(new List<TECSubScope>(),
                system, scopeManager.Catalogs.ConduitTypes, scopeManager.Catalogs.ConnectionTypes);
        }

        private void setup()
        {
            AddCommand = new RelayCommand(addExecute, addCanExecute);
            AddPointCommand = new RelayCommand(addPointExecute, canAddPoint);
            DeletePointCommand = new RelayCommand<TECPoint>(deletePointExecute);
            DeleteDeviceCommand = new RelayCommand<IEndDevice>(deleteDeviceExecute);
        }

        private void addPointExecute()
        {
            TECPoint newPoint = new TECPoint();
            newPoint.Type = PointType;
            newPoint.Quantity = PointQuantity;
            newPoint.Label = PointName;
            ToAdd.Points.Add(newPoint);
            PointName = "";
            updateConnectVMWithQuantity(Quantity);
        }
        private bool canAddPoint()
        {
            return (PointQuantity != 0);
        }

        private void deletePointExecute(TECPoint point)
        {
            toAdd.Points.Remove(point);
            updateConnectVMWithQuantity(Quantity);

        }

        private void deleteDeviceExecute(IEndDevice device)
        {
            toAdd.Devices.Remove(device);
        }
        
        private bool addCanExecute()
        {
            if(ConnectVM != null)
            {
                return ConnectVM.CanConnect();
            } else
            {
                return true;
            }
        }
        private void addExecute()
        {
            for (int x = 0; x < Quantity; x++)
            {
                TECSubScope subScope = null;
                if(underlyingTemplate != null)
                {
                    subScope = AsReference ? templates.SubScopeSynchronizer.NewItem(underlyingTemplate) : new TECSubScope(underlyingTemplate);
                    subScope.CopyPropertiesFromScope(ToAdd);
                    foreach (IEndDevice device in ToAdd.Devices.Where(item => !originalDevices.Contains(item)))
                    {
                        subScope.Devices.Add(device);
                    }
                    foreach (TECPoint point in ToAdd.Points.Where(item => !originalPoints.Contains(item)))
                    {
                        subScope.Points.Add(point);
                    }
                }
                else
                {
                    subScope = new TECSubScope(ToAdd);
                }
                
                add(subScope);
                if (ConnectVM != null && ConnectVM.Connect)
                {
                    ConnectVM.ExecuteConnection(subScope);
                }
                Added?.Invoke(subScope);
            }
        }
        private void updateConnectVMWithQuantity(int value)
        {
            if (ConnectVM == null)
            {
                return;
            }
            List<TECSubScope> toConnect = new List<TECSubScope>();
            for (int x = 0; x < quantity; x++)
            {
                toConnect.Add(ToAdd);
            }
            ConnectVM.Update(toConnect);
        }
        
        internal void SetTemplate(TECSubScope subScope)
        {
            ToAdd = new TECSubScope(subScope);
            originalDevices = new List<IEndDevice>(ToAdd.Devices);
            originalPoints = new List<TECPoint>(ToAdd.Points);
            underlyingTemplate = subScope;
            if (IsTemplates)
            {
                DisplayReferenceProperty = true;
            }
        }
        
    }
}
