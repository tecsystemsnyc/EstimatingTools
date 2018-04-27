using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Models
{
    public class ScopeGroup : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

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
            this.Scope.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Name")
                {
                    Name = scope.Name;
                }
            };
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
        public bool Remove(ScopeGroup child)
        {
            return this.ChildrenGroups.Remove(child);
        }

        public ScopeGroup GetGroup(ITECScope scope)
        {
            if (this.Scope == scope)
            {
                return this;
            }

            foreach (ScopeGroup group in this.ChildrenGroups)
            {
                ScopeGroup childGroup = group.GetGroup(scope);
                if (childGroup != null)
                {
                    return childGroup;
                }
            }

            return null;
        }
    }
}
