using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECScopeBranch : TECLabeled, IRelatable, ITypicalable
    {//TECScopeBranch exists as an alternate object to TECSystem. It's purpose is to serve as a non-specific scope object with unlimited branches in both depth and breadth.
        #region Properties
        private ObservableCollection<TECScopeBranch> _branches;
        public ObservableCollection<TECScopeBranch> Branches
        {
            get { return _branches; }
            set
            {
                var old = Branches;
                _branches = value;
                notifyCombinedChanged(Change.Edit, "Branches", this, value, old);
                Branches.CollectionChanged += Branches_CollectionChanged;
            }
        }

        public SaveableMap PropertyObjects
        {
            get { return propertyObjects(); }
        }
        public SaveableMap LinkedObjects
        {
            get { return new SaveableMap(); }
        }

        public bool IsTypical { get; private set; }

        #endregion //Properites

        #region Constructors
        public TECScopeBranch(Guid guid) : base(guid)
        {
            IsTypical = false;
            _branches = new ObservableCollection<TECScopeBranch>();
            Branches.CollectionChanged += Branches_CollectionChanged;
        }

        public TECScopeBranch() : this(Guid.NewGuid()) { }

        //Copy Constructor
        public TECScopeBranch(TECScopeBranch scopeBranchSource) : this()
        {
            foreach (TECScopeBranch branch in scopeBranchSource.Branches)
            {
                Branches.Add(new TECScopeBranch(branch));
            }
            _label = scopeBranchSource.Label;
        }
        #endregion //Constructors
        
        private void Branches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, "Branches", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, "Branches", this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, "Branches", this, sender, sender);
            }
        }

        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.Branches, "Branches");
            return saveList;
        }
        
        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECScopeBranch(this);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            throw new NotImplementedException();
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
            throw new NotImplementedException();
        }
        #endregion

    }
}
