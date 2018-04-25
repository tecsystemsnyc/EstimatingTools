using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectionsVM : ViewModelBase
    {
        private readonly List<TECController> allControllers;
        private readonly List<IConnectable> allConnectables;

        public ObservableCollection<ScopeGroup> Controllers { get; }
        public ObservableCollection<ScopeGroup> Connectables { get; }

        public ConnectionsVM(TECBid bid)
        {
            this.Controllers = new ObservableCollection<ScopeGroup>();
            this.Connectables = new ObservableCollection<ScopeGroup>();

            foreach(TECController controller in bid.Controllers)
            {
                this.Controllers.Add(new ScopeGroup(controller));
                this.Connectables.Add(new ScopeGroup(controller));
            }

            foreach(TECSystem sys in bid.Systems)
            {
                ScopeGroup systemControllerGroup = new ScopeGroup(sys.Name);
                ScopeGroup systemConnectableGroup = new ScopeGroup(sys.Name);
                foreach (TECController controller in sys.Controllers)
                {
                    systemControllerGroup.Add(controller);
                    systemConnectableGroup.Add(controller);
                }
                foreach (TECEquipment equip in sys.Equipment)
                {
                    ScopeGroup equipConnectableGroup = new ScopeGroup(equip.Name);
                    equip.SubScope.ForEach(ss => equipConnectableGroup.Add(ss));
                    systemConnectableGroup.Add(equipConnectableGroup);
                }
                this.Controllers.Add(systemControllerGroup);
                this.Connectables.Add(systemConnectableGroup);
            }
        }
        public ConnectionsVM(TECSystem system)
        {

        }
    }
}
