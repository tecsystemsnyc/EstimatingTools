using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Models
{
    public class ScopeGroup
    {
        public string Name { get; }

        public ITECScope Scope { get; }
        public ObservableCollection<ScopeGroup> ChildrenGroups { get; }

        public ScopeGroup(string name)
        {
            this.Name = name;

            ChildrenGroups = new ObservableCollection<ScopeGroup>();
        }
        public ScopeGroup(ITECScope scope) : this(scope.Name)
        {
            this.Scope = scope;
        }

        public void Add(ITECScope child)
        {
            this.Add(new ScopeGroup(child));
        }
        public void Add(ScopeGroup child)
        {
            this.ChildrenGroups.Add(child);
        }

        public bool Remove(ITECScope child)
        {
            ScopeGroup groupToRemove = null;

            foreach (ScopeGroup childGroup in this.ChildrenGroups)
            {
                if (childGroup.Scope == child)
                {
                    groupToRemove = childGroup;
                    break;
                }
            }

            if (groupToRemove == null)
            {
                return false;
            }
            else
            {
                this.ChildrenGroups.Remove(groupToRemove);
                return true;
            }
        }
    }
}
