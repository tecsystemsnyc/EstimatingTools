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
    public class ManufacturersCatalogVM : CatalogVMBase
    {
        private string _newManName = "";
        private double _newManMultiplier = 0;

        private TECManufacturer _selectedManufacturer;
        
        public string NewManName
        {
            get { return _newManName; }
            set
            {
                if (_newManName != value)
                {
                    _newManName = value;
                    RaisePropertyChanged("NewManName");
                }
            }
        }
        public double NewManMultiplier
        {
            get { return _newManMultiplier; }
            set
            {
                if (_newManMultiplier != value)
                {
                    _newManMultiplier = value;
                    RaisePropertyChanged("NewManMultipier");
                }
            }
        }

        public TECManufacturer SelectedManufacturer
        {
            get { return _selectedManufacturer; }
            set
            {
                if (_selectedManufacturer != value)
                {
                    _selectedManufacturer = value;
                    RaisePropertyChanged("SelectedManufacturer");
                    RaiseSelectedChanged(SelectedManufacturer);
                }
            }
        }
        
        public ICommand AddManufacturerCommand { get; }

        public ManufacturersCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddManufacturerCommand = new RelayCommand(addManufacturerExecute, canAddManufacturer);
        }

        private void addManufacturerExecute()
        {
            TECManufacturer newMan = new TECManufacturer();
            newMan.Label = this.NewManName;
            newMan.Multiplier = this.NewManMultiplier;

            this.Templates.Catalogs.Add(newMan);

            this.NewManName = "";
            this.NewManMultiplier = 0;
        }
        private bool canAddManufacturer()
        {
            return (this.NewManName != "");
        }
    }
}
