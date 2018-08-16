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
