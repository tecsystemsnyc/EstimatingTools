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

namespace TECUserControlLibrary.Models
{
    public class ScopeGroup : ViewModelBase, IDragDropable
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

        public string Type
        {
            get
            {
                return Scope.ToTECTypeString();
            }
        }

        public ITECScope Scope { get; }
        public ObservableCollection<ScopeGroup> ChildrenGroups { get; } = new ObservableCollection<ScopeGroup>();
        private Dictionary<ITECScope, ScopeGroup> scopeDictionary = new Dictionary<ITECScope, ScopeGroup>();

        public ScopeGroup(string name)
        {
            this.Name = name;
        }
        public ScopeGroup(ITECScope scope) : this(scope.Name)
        {
            this.Scope = scope;
            scopeDictionary.Add(scope, this);
            this.Scope.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Name")
                {
                    Name = scope.Name;
                }
            };
        }

        public ScopeGroup Add(ITECScope child)
        {
            ScopeGroup newGroup = new ScopeGroup(child);
            this.Add(newGroup);
            return newGroup;
        }
        public void Add(ScopeGroup child)
        {
            foreach(var pair in child.scopeDictionary)
            {
                this.scopeDictionary.Add(pair.Key, pair.Value);
            }
            child.scopeDictionary = this.scopeDictionary;
            this.ChildrenGroups.Add(child);
        }

        public bool Remove(ITECScope child)
        {
            ScopeGroup groupToRemove = scopeDictionary.ContainsKey(child) ? scopeDictionary[child] : null;
            
            if (groupToRemove == null)
            {
                return false;
            }
            else
            {
                this.Remove(groupToRemove);
                return true;
            }
        }
        public bool Remove(ScopeGroup child)
        {
            var moreChildren = new List<ScopeGroup>(child.ChildrenGroups);
            foreach (var childGroup in moreChildren)
            {
                child.Remove(childGroup);
            }
            child.scopeDictionary = null;
            this.scopeDictionary.Remove(child.Scope);
            return this.ChildrenGroups.Remove(child);
        }

        public ScopeGroup GetGroup(ITECScope scope)
        {
            //if (this.Scope == scope)
            //{
            //    return this;
            //}

            //foreach (ScopeGroup group in this.ChildrenGroups)
            //{
            //    ScopeGroup childGroup = group.GetGroup(scope);
            //    if (childGroup != null)
            //    {
            //        return childGroup;
            //    }
            //}
            if (scope == null) return null;
            if (this.scopeDictionary.ContainsKey(scope))
            {
                return scopeDictionary[scope];
            }
            else
            {
                return null;
            }            
        }
        public List<ScopeGroup> GetPath(ITECScope scope)
        {
            List<ScopeGroup> path = new List<ScopeGroup>();
            if (this.Scope == scope)
            {
                path.Add(this);
                return path;
            }

            foreach(ScopeGroup childGroup in this.ChildrenGroups)
            {
                List<ScopeGroup> childPath = childGroup.GetPath(scope);
                if (childPath.Count > 0)
                {
                    path.Add(this);
                    path.AddRange(childPath);
                    return path;
                }
            }

            return path;
        }

        object IDragDropable.DropData()
        {
            return ChildrenGroups.Count == 0 ? Scope as object : allScope();
        }

        private List<ITECScope> allScope()
        {
            List<ITECScope> scope = new List<ITECScope>();
            if(this.Scope != null)
            {
                scope.Add(this.Scope);
            }
            foreach(var child in ChildrenGroups)
            {
                scope.AddRange(child.allScope());
            }
            return scope;
        }
    }
}
