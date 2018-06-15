using EstimatingLibrary;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class ConnectionTypesCatalogVM : CatalogVMBase
    {
        private string _connectionTypeName = "";
        private double _connectionTypeCost = 0;
        private double _connectionTypeLabor = 0;
        private bool _hasPlenum = false;
        private double _connectionTypePlenumCost = 0;
        private double _connectionTypePlenumLabor = 0;

        private TECConnectionType _selectedConnectionType;

        public string ConnectionTypeName
        {
            get { return _connectionTypeName; }
            set
            {
                if (_connectionTypeName != value)
                {
                    _connectionTypeName = value;
                    RaisePropertyChanged("ConnectionTypeName");
                }
            }
        }
        public double ConnectionTypeCost
        {
            get { return _connectionTypeCost; }
            set
            {
                if (_connectionTypeCost != value)
                {
                    _connectionTypeCost = value;
                    RaisePropertyChanged("ConnectionTypeCost");
                }
            }
        }
        public double ConnectionTypeLabor
        {
            get { return _connectionTypeLabor; }
            set
            {
                if (_connectionTypeLabor != value)
                {
                    _connectionTypeLabor = value;
                    RaisePropertyChanged("ConnectionTypeLabor");
                }
            }
        }
        public bool HasPlenum
        {
            get { return _hasPlenum; }
            set
            {
                if (_hasPlenum != value)
                {
                    _hasPlenum = value;
                    RaisePropertyChanged("HasPlenum");
                }
            }
        }
        public double ConnectionTypePlenumCost
        {
            get { return _connectionTypePlenumCost; }
            set
            {
                if (_connectionTypePlenumCost != value)
                {
                    _connectionTypePlenumCost = value;
                    RaisePropertyChanged("ConnectionTypePlenumCost");
                }
            }
        }
        public double ConnectionTypePlenumLabor
        {
            get { return _connectionTypePlenumLabor; }
            set
            {
                if (_connectionTypePlenumLabor != value)
                {
                    _connectionTypePlenumLabor = value;
                    RaisePropertyChanged("ConnectionTypePlenumLabor");
                }
            }
        }
        
        public TECConnectionType SelectedConnectionType
        {
            get { return _selectedConnectionType; }
            set
            {
                if (_selectedConnectionType != value)
                {
                    _selectedConnectionType = value;
                    RaisePropertyChanged("SelectedConnectionType");
                    RaiseSelectedChanged(SelectedConnectionType);
                }
            }
        }

        public ICommand AddConnectionTypeCommand { get; }

        public ConnectionTypesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddConnectionTypeCommand = new RelayCommand(addConnectionTypeExecute);
        }

        private void addConnectionTypeExecute()
        {
            var connectionType = new TECConnectionType();
            connectionType.Name = ConnectionTypeName;
            connectionType.Cost = ConnectionTypeCost;
            connectionType.Labor = ConnectionTypeLabor;

            if (HasPlenum)
            {
                //Setting marginal plenum cost and labor
                connectionType.PlenumCost = (ConnectionTypePlenumCost - ConnectionTypeCost);
                connectionType.PlenumLabor = (ConnectionTypePlenumLabor - ConnectionTypeLabor);
            }
            else
            {
                connectionType.PlenumCost = 0;
                connectionType.PlenumLabor = 0;
            }
            
            this.Templates.Catalogs.ConnectionTypes.Add(connectionType);

            this.ConnectionTypeName = "";
            this.ConnectionTypeCost = 0;
            this.ConnectionTypeLabor = 0;
            this.ConnectionTypePlenumCost = 0;
            this.ConnectionTypePlenumLabor = 0;
        }
        private bool canAddConnectionType()
        {
            return this.ConnectionTypeName != "";
        }
    }
}
