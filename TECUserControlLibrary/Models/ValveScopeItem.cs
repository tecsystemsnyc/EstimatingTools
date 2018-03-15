using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Models
{
    public class ValveScopeItem : ViewModelBase
    {
        private TECValve _valve;

        public TECEquipment Equipment { get; }
        public TECSubScope SubScope { get; }
        public TECValve Valve {
            get { return _valve; }
            set
            {
                _valve = value;
                RaisePropertyChanged("Valve");
            }
        }

        public ValveScopeItem(TECEquipment equipment, TECSubScope subScope, TECValve valve)
        {
            this.Equipment = equipment;
            this.SubScope = subScope;
            this.Valve = valve;
        }
    }
}
