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
    public class ProtocolAdapaterCatalogVM : CatalogVMBase
    {
        private String _name = "";
        private String _description ="";
        private Double _listPrice = 0.0;
        private Double _labor = 0.0;
        private TECProtocol _protocol;
        private TECManufacturer _manufacturer;
        private TECProtocolAdapter _selected;

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public String Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }
        public Double ListPrice
        {
            get { return _listPrice; }
            set
            {
                _listPrice = value;
                RaisePropertyChanged("ListPrice");
            }
        }
        public Double Labor
        {
            get { return _labor; }
            set
            {
                _labor = value;
                RaisePropertyChanged("Labor");
            }
        }
        public TECProtocol Protocol
        {
            get { return _protocol; }
            set
            {
                _protocol = value;
                RaisePropertyChanged("Protocol");
            }
        }
        public TECManufacturer Manufacturer
        {
            get { return _manufacturer; }
            set
            {
                _manufacturer = value;
                RaisePropertyChanged("Manufacturer");
            }
        }
        public TECProtocolAdapter Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }
        
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public ProtocolAdapaterCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddCommand = new RelayCommand(addExecute, canAdd);
            this.DeleteCommand = new RelayCommand(deleteExecute, canDelete);
        }

        private void addExecute()
        {
            TECProtocolAdapter toAdd = new TECProtocolAdapter(Manufacturer, Protocol);
            toAdd.Name = this.Name;
            toAdd.Description = this.Description;
            toAdd.Price = this.ListPrice;
            toAdd.Labor = this.Labor;

            this.Templates.Catalogs.ProtocolAdapters.Add(toAdd);

            this.Name = "";
            this.Description = "";
            this.ListPrice = 0.0;
            this.Labor = 0.0;
            this.Manufacturer = null;
            this.Protocol = null;
        }
        private bool canAdd()
        {
            return (Manufacturer != null && Protocol != null);
        }

        private void deleteExecute()
        {
            this.StartModal(new DeleteEndDeviceVM(Selected, Templates));
        }
        private bool canDelete()
        {
            return Selected != null;
        }
    }
}
