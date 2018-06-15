using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class ProtocolsCatalogVM : CatalogVMBase
    {
        private String _name = "";

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public ObservableCollection<TECConnectionType> ConnectionTypes { get; }

        public RelayCommand AddProtocolCommand { get; }

        public ProtocolsCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.ConnectionTypes = new ObservableCollection<TECConnectionType>();
            this.AddProtocolCommand = new RelayCommand(addProtocolExecute, canAddProtocol);

            this.ConnectionTypes.CollectionChanged += connectionTypesCollectionChanged;
        }

        private void connectionTypesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AddProtocolCommand.RaiseCanExecuteChanged();
        }

        private void addProtocolExecute()
        {
            TECProtocol toAdd = new TECProtocol(this.ConnectionTypes);
            toAdd.Label = this.Name;

            this.Templates.Catalogs.Protocols.Add(toAdd);

            this.Name = "";
            this.ConnectionTypes.ObservablyClear();
        }

        private bool canAddProtocol()
        {
            return (ConnectionTypes.Count > 0);
        }
    }
}
