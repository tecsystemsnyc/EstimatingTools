using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TECUserControlLibrary.Models;

namespace TECUserControlLibrary.ViewModels
{
    public class ValveSelectionVM
    {
        public List<TECValve> Catalog { get; }
        public List<ValveScopeItem> Valves { get; }

        public ValveSelectionVM(TECSystem system, IEnumerable<TECValve> valveCatalog)
        {
            Catalog = new List<TECValve>(valveCatalog);
            Valves = getValveItems(system);
        }

        private List<ValveScopeItem> getValveItems(TECSystem system)
        {
            List<ValveScopeItem> valveItems = new List<ValveScopeItem>();
            foreach(TECEquipment equipment in system.Equipment)
            {
                foreach(TECSubScope subScope in equipment.SubScope)
                {
                    foreach(IEndDevice device in subScope.Devices)
                    {
                        if(device is TECValve valve)
                        {
                            valveItems.Add(new ValveScopeItem(equipment, subScope, valve));
                        }
                    }
                }
            }
            return valveItems;
        }
    }
}
