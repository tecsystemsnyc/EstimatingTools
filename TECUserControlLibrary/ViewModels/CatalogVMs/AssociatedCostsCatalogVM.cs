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
    public class AssociatedCostsCatalogVM : CatalogVMBase
    {
        private string _associatedCostName = "";
        private CostType _associatedCostType = 0;
        private double _associatedCostCost = 0;
        private double _associatedCostLabor = 0;

        private TECAssociatedCost _selectedAssociatedCost;

        public string AssociatedCostName
        {
            get { return _associatedCostName; }
            set
            {
                if (_associatedCostName != value)
                {
                    _associatedCostName = value;
                    RaisePropertyChanged("AssociatedCostName");
                }
            }
        }
        public CostType AssociatedCostType
        {
            get { return _associatedCostType; }
            set
            {
                if (_associatedCostType != value)
                {
                    _associatedCostType = value;
                    RaisePropertyChanged("AssociatedCostType");
                }
            }
        }
        public double AssociatedCostCost
        {
            get { return _associatedCostCost; }
            set
            {
                if (_associatedCostCost != value)
                {
                    _associatedCostCost = value;
                    RaisePropertyChanged("AssociatedCostCost");
                }
            }
        }
        public double AssociatedCostLabor
        {
            get { return _associatedCostLabor; }
            set
            {
                if (_associatedCostLabor != value)
                {
                    _associatedCostLabor = value;
                    RaisePropertyChanged("AssociatedCostLabor");
                }
            }
        }

        public TECAssociatedCost SelectedAssociatedCost
        {
            get { return _selectedAssociatedCost; }
            set
            {
                if (_selectedAssociatedCost != value)
                {
                    _selectedAssociatedCost = value;
                    RaisePropertyChanged("SelectedAssociatedCost");
                    RaiseSelectedChanged(SelectedAssociatedCost);
                }
            }
        }

        public ICommand AddAssociatedCostCommand { get; }

        public AssociatedCostsCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddAssociatedCostCommand = new RelayCommand(addAsociatedCostExecute, canAddAssociatedCost);
        }

        private void addAsociatedCostExecute()
        {
            TECAssociatedCost associatedCost = new TECAssociatedCost(AssociatedCostType);
            associatedCost.Name = this.AssociatedCostName;
            associatedCost.Cost = this.AssociatedCostCost;
            associatedCost.Labor = this.AssociatedCostLabor;

            this.Templates.Catalogs.Add(associatedCost);

            this.AssociatedCostName = "";
            this.AssociatedCostCost = 0;
            this.AssociatedCostLabor = 0;
        }
        private bool canAddAssociatedCost()
        {
            return (this.AssociatedCostName != "");
        }
    }
}
