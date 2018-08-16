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
    public class ConduitTypesCatalogVM : CatalogVMBase
    {
        private string _conduitTypeName = "";
        private double _conduitTypeCost = 0;
        private double _conduitTypeLabor = 0;

        private TECElectricalMaterial _selectedConduitType;

        public string ConduitTypeName
        {
            get { return _conduitTypeName; }
            set
            {
                if (_conduitTypeName != value)
                {
                    _conduitTypeName = value;
                    RaisePropertyChanged("ConduitTypeName");
                }
            }
        }
        public double ConduitTypeCost
        {
            get { return _conduitTypeCost; }
            set
            {
                if (_conduitTypeCost != value)
                {
                    _conduitTypeCost = value;
                    RaisePropertyChanged("ConduitTypeCost");
                }
            }
        }
        public double ConduitTypeLabor
        {
            get { return _conduitTypeLabor; }
            set
            {
                if (_conduitTypeLabor != value)
                {
                    _conduitTypeLabor = value;
                    RaisePropertyChanged("ConduitTypeLabor");
                }
            }
        }
        
        public TECElectricalMaterial SelectedConduitType
        {
            get { return _selectedConduitType; }
            set
            {
                if (_selectedConduitType != value)
                {
                    _selectedConduitType = value;
                    RaisePropertyChanged("SelectedConduitType");
                    RaiseSelectedChanged(SelectedConduitType);
                }
            }
        }

        public ICommand AddConduitTypeCommand { get; }
        
        public ConduitTypesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddConduitTypeCommand = new RelayCommand(addConduitTypeExecute, canAddConduitType);
        }

        private void addConduitTypeExecute()
        {
            TECElectricalMaterial conduitType = new TECElectricalMaterial();
            conduitType.Name = ConduitTypeName;
            conduitType.Cost = ConduitTypeCost;
            conduitType.Labor = ConduitTypeLabor;

            this.Templates.Catalogs.Add(conduitType);

            this.ConduitTypeName = "";
            this.ConduitTypeCost = 0;
            this.ConduitTypeLabor = 0;
        }
        private bool canAddConduitType()
        {
            return this.ConduitTypeName != "";
        }
    }
}
