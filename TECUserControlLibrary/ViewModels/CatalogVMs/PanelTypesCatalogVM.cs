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
    public class PanelTypesCatalogVM : CatalogVMBase
    {
        private string _panelTypeName = "";
        private string _panelTypeDescription = "";
        private double _panelTypeCost = 0;
        private double _panelTypeLabor = 0;
        private TECManufacturer _panelTypeManufacturer;

        private TECPanelType _selectedPanelType;

        public string PanelTypeName
        {
            get { return _panelTypeName; }
            set
            {
                if (_panelTypeName != value)
                {
                    _panelTypeName = value;
                    RaisePropertyChanged("PanelTypeName");
                }
            }
        }
        public string PanelTypeDescription
        {
            get { return _panelTypeDescription; }
            set
            {
                if (_panelTypeDescription != value)
                {
                    _panelTypeDescription = value;
                    RaisePropertyChanged("PanelTypeDescription");
                }
            }
        }
        public double PanelTypeCost
        {
            get { return _panelTypeCost; }
            set
            {
                if (_panelTypeCost != value)
                {
                    _panelTypeCost = value;
                    RaisePropertyChanged("PanelTypeCost");
                }
            }
        }
        public double PanelTypeLabor
        {
            get { return _panelTypeLabor; }
            set
            {
                if (_panelTypeLabor != value)
                {
                    _panelTypeLabor = value;
                    RaisePropertyChanged("PanelTypeLabor");
                }
            }
        }
        public TECManufacturer PanelTypeManufacturer
        {
            get { return _panelTypeManufacturer; }
            set
            {
                if (_panelTypeManufacturer != value)
                {
                    _panelTypeManufacturer = value;
                    RaisePropertyChanged("PanelTypeManufacturer");
                }
            }
        }
        
        public TECPanelType SelectedPanelType
        {
            get { return _selectedPanelType; }
            set
            {
                if (_selectedPanelType != value)
                {
                    _selectedPanelType = value;
                    RaisePropertyChanged("SelectedPanelType");
                    RaiseSelectedChanged(SelectedPanelType);
                }
            }
        }

        public ICommand AddPanelTypeCommand { get; }

        public PanelTypesCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddPanelTypeCommand = new RelayCommand(addPanelTypeExecute, canAddPanelType);
        }

        private void addPanelTypeExecute()
        {
            TECPanelType panelType = new TECPanelType(PanelTypeManufacturer);
            panelType.Name = this.PanelTypeName;
            panelType.Description = this.PanelTypeDescription;
            panelType.Price = this.PanelTypeCost;
            panelType.Labor = this.PanelTypeLabor;

            Templates.Catalogs.Add(panelType);

            this.PanelTypeName = "";
            this.PanelTypeDescription = "";
            this.PanelTypeCost = 0;
            this.PanelTypeLabor = 0;
            this.PanelTypeManufacturer = null;
        }
        private bool canAddPanelType()
        {
            return (PanelTypeName != "" && PanelTypeManufacturer != null);
        }
    }
}
