using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.ViewModels.AddVMs
{
    public class AddInterlockVM : AddVM
    {
        private IInterlockable parent;
        private TECInterlockConnection _toAdd = new TECInterlockConnection(new List<TECConnectionType>());
        private int _quantity = 1;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");
            }
        }
        public TECInterlockConnection ToAdd
        {
            get { return _toAdd; }
            set
            {
                _toAdd = value;
                RaisePropertyChanged("ToAdd");
            }
        }
        public TECCatalogs Catalogs { get; }
    

        public AddInterlockVM(IInterlockable parent, TECScopeManager scopeManager) : base(scopeManager)
        {
            this.parent = parent;
            this.Catalogs = scopeManager.Catalogs;
            AddCommand = new RelayCommand(addExecute, canAdd);
        }

        private void addExecute()
        {
            parent.Interlocks.Add(ToAdd);
        }

        private bool canAdd()
        {
            return Quantity > 0;
        }
    }
}
