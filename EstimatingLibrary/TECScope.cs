using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public abstract class TECScope : TECObject, INotifyCostChanged, IRelatable, ITECScope
    {
        #region Properties

        protected string _name;
        protected string _description;

        protected ObservableCollection<TECTag> _tags;
        protected ObservableCollection<TECAssociatedCost> _associatedCosts;

        public event Action<CostBatch> CostChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != Name)
                {
                    var old = Name;
                    _name = value;
                    notifyCombinedChanged(Change.Edit, "Name", this, value, old);
                }
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != Description)
                {
                    var old = Description;
                    _description = value;
                    notifyCombinedChanged(Change.Edit, "Description", this, value, old);
                }
            }
        }

        public ObservableCollection<TECTag> Tags
        {
            get { return _tags; }
            set
            {
                var old = Tags;
                Tags.CollectionChanged -= (sender, args) => scopeCollectionChanged(sender, args, "Tags");
                _tags = value;
                notifyCombinedChanged(Change.Edit, "Tags", this, value, old);
                Tags.CollectionChanged += (sender, args) => scopeCollectionChanged(sender, args, "Tags");
            }
        }
        public ObservableCollection<TECAssociatedCost> AssociatedCosts
        {
            get { return _associatedCosts; }
            set
            {
                var old = AssociatedCosts;
                AssociatedCosts.CollectionChanged -= (sender, args) => scopeCollectionChanged(sender, args, "AssociatedCosts");
                _associatedCosts = value;
                notifyCombinedChanged(Change.Edit, "AssociatedCosts", this, value, old);
                AssociatedCosts.CollectionChanged += (sender, args) => scopeCollectionChanged(sender, args, "AssociatedCosts");
            }
        }

        public virtual CostBatch CostBatch
        {
            get
            {
                return getCosts();
            }
        }

        public SaveableMap PropertyObjects
        {
            get
            {
                return propertyObjects();
            }
        }
        public SaveableMap LinkedObjects
        {
            get
            {
                SaveableMap map = linkedObjects();
                return linkedObjects();
            }
        }
        #endregion

        #region Constructors
        public TECScope(Guid guid) : base(guid)
        {
            _name = "";
            _description = "";
            _guid = guid;

            _tags = new ObservableCollection<TECTag>();
            _associatedCosts = new ObservableCollection<TECAssociatedCost>();
            Tags.CollectionChanged += (sender, args) => scopeCollectionChanged(sender, args, "Tags");
            AssociatedCosts.CollectionChanged += (sender, args) => scopeCollectionChanged(sender, args, "AssociatedCosts");
        }

        #endregion 

        #region Methods
        protected void copyPropertiesFromScope(TECScope scope)
        {
            _name = scope.Name;
            _description = scope.Description;
            var tags = new ObservableCollection<TECTag>();
            foreach (TECTag tag in scope.Tags)
            { tags.Add(tag); }
            Tags = tags;
            var associatedCosts = new ObservableCollection<TECAssociatedCost>(scope.AssociatedCosts);
            AssociatedCosts = associatedCosts;
        }
        protected virtual void scopeCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
            //Is virtual so that it can be overridden in TECTypical
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, notifyCombinedChanged, notifyCostChanged);
        }
        
        protected virtual void notifyCostChanged(CostBatch costs)
        {
            CostChanged?.Invoke(costs);
        }

        protected virtual CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
            foreach (TECCost assocCost in AssociatedCosts)
            {
                costs.AddCost(assocCost);
            }
            return costs;
        }
        protected virtual SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(this.Tags, "Tags");
            saveList.AddRange(this.AssociatedCosts.Distinct(), "AssociatedCosts");
            return saveList;
        }
        protected virtual SaveableMap linkedObjects()
        {
            SaveableMap relatedList = new SaveableMap();
            relatedList.AddRange(this.Tags, "Tags");
            relatedList.AddRange(this.AssociatedCosts.Distinct(), "AssociatedCosts");
            return relatedList;
        }
        #endregion Methods
    }
}
