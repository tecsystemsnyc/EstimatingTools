using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public static class ScopeExtensions
    {
        public static void CopyPropertiesFromScope(this TECScope scope, TECScope otherScope)
        {
            if(scope == otherScope)
            {
                return;
            }
            scope.Name = otherScope.Name;
            scope.Description = otherScope.Description;
            CopyChildrenFromScope(scope, otherScope);
        }

        public static void CopyChildrenFromScope(this TECScope scope, TECScope otherScope)
        {
            if(scope == otherScope)
            {
                return;
            }
            scope.Tags.ObservablyClear();
            foreach (TECTag tag in otherScope.Tags)
            {
                scope.Tags.Add(tag);
            }
            scope.AssociatedCosts.ObservablyClear();
            foreach (TECAssociatedCost cost in otherScope.AssociatedCosts)
            {
                scope.AssociatedCosts.Add(cost);
            }
        }
        
        public static TECEquipment FindParentEquipment(this TECSubScope subScope, TECScopeManager manager)
        {
            foreach (TECEquipment equip in manager.GetAllEquipment())
            {
                if (equip.SubScope.Contains(subScope))
                {
                    return equip;
                }
            }
            return null;
        }

        public static TECEquipment FindParentEquipment(this TECSubScope subScope, TECSystem system)
        {
            foreach(TECEquipment equip in system.Equipment)
            {
                if (equip.SubScope.Contains(subScope))
                {
                    return equip;
                }
            }
            return null;
        }

        public static List<TECSystem> GetAllSystems(this TECScopeManager manager)
        {
            List<TECSystem> systems = new List<TECSystem>();
            if (manager is TECBid bid)
            {
                foreach(TECTypical typ in bid.Systems)
                {
                    systems.AddRange(typ.Instances);
                }
                systems.AddRange(bid.Systems);
            }
            else if (manager is TECTemplates templates)
            {
                systems.AddRange(templates.SystemTemplates);
            }
            return systems;
        }

        public static List<TECEquipment> GetAllEquipment(this TECScopeManager manager)
        {
            List<TECEquipment> equip = new List<TECEquipment>();
            if (manager is TECBid bid)
            {
                foreach(TECTypical typ in bid.Systems)
                {
                    foreach(TECSystem sys in typ.Instances)
                    {
                        equip.AddRange(sys.Equipment);
                    }
                    equip.AddRange(typ.Equipment);
                }
            }
            else if (manager is TECTemplates templates)
            {
                foreach(TECSystem sys in templates.SystemTemplates)
                {
                    equip.AddRange(sys.Equipment);
                }
                equip.AddRange(templates.EquipmentTemplates);
            }
            return equip;
        }

        public static List<TECController> GetAllInstanceControllers(this TECBid bid)
        {
            List<TECController> instanceControllers = new List<TECController>();
            instanceControllers.AddRange(bid.Controllers);
            foreach(TECTypical typ in bid.Systems)
            {
                foreach(TECSystem sys in typ.Instances)
                {
                    instanceControllers.AddRange(sys.Controllers);
                }
            }
            return instanceControllers;
        }

        public static List<TECSubScope> GetAllInstanceSubScope(this TECBid bid)
        {
            List<TECSubScope> instanceSubScope = new List<TECSubScope>();
            foreach(TECTypical typ in bid.Systems)
            {
                foreach(TECSystem sys in typ.Instances)
                {
                    foreach(TECEquipment equip in sys.Equipment)
                    {
                        instanceSubScope.AddRange(equip.SubScope);
                    }
                }
            }
            return instanceSubScope;
        }

        public static TECSystem FindParentSystem(this TECEquipment equip, TECScopeManager manager)
        {
            foreach (TECSystem sys in manager.GetAllSystems())
            {
                if (sys.Equipment.Contains(equip))
                {
                    return sys;
                }
            }
            return null;
        }
    }

    public static class TypicalScopeExtensions 
    {
        public static void AddChildForScopeProperty<T>(this T typ, String property, ITECObject item) where T : TECScope, ITypicalable
        {
            if (property == "AssociatedCosts" && item is TECAssociatedCost cost)
            {
                typ.AssociatedCosts.Add(cost);
            }
            else if (property == "Tags" && item is TECTag tag)
            {
                typ.Tags.Add(tag);
            }
            else
            {
                throw new Exception(String.Format("There is no compatible add method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
        }

        public static bool RemoveChildForScopeProperty<T>(this T typ, String property, ITECObject item) where T : TECScope, ITypicalable
        {
            if (property == "AssociatedCosts" && item is TECAssociatedCost cost)
            {
                return typ.AssociatedCosts.Remove(cost);
            }
            else if (property == "Tags" && item is TECTag tag)
            {
                return typ.Tags.Remove(tag);
            }
            else
            {
                throw new Exception(String.Format("There is no compatible remove method for the property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
        }

        public static bool ContainsChildForScopeProperty<T>(this T typ, string property, ITECObject item) where T : TECScope, ITypicalable
        {
            if (property == "AssociatedCosts" && item is TECAssociatedCost cost)
            {
                return typ.AssociatedCosts.Contains(cost);
            }
            else if (property == "Tags" && item is TECTag tag)
            {
                return typ.Tags.Contains(tag);
            }
            else
            {
                throw new Exception(String.Format("There is no compatible property {0} with an object of type {1}", property, item.GetType().ToString()));
            }
        }
    }
}
