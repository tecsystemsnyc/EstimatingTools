using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Models
{
    public class ValveScopeItem
    {
        public TECEquipment Equipment { get; }
        public TECSubScope SubScope { get; }
        public TECValve Valve { get; }

        public ValveScopeItem(TECEquipment equipment, TECSubScope subScope, TECValve valve)
        {
            this.Equipment = equipment;
            this.SubScope = subScope;
            this.Valve = valve;
        }
    }
}
