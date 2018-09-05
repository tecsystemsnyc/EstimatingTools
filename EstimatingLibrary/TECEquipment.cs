using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECEquipment : TECLocated, INotifyPointChanged, IDragDropable, ITypicalable, ICatalogContainer
    {
        #region Properties

        public event Action<int> PointChanged;

        public ObservableCollection<TECSubScope> SubScope { get; } = new ObservableCollection<TECSubScope>();
        
        public int PointNumber
        {
            get
            {
                return getPointNumber();
            }
        }

        public bool IsTypical { get; private set; }

        #endregion //Properties

        #region Constructors
        public TECEquipment(Guid guid) : base(guid)
        {
            IsTypical = false;
            SubScope.CollectionChanged += SubScope_CollectionChanged;
            base.PropertyChanged += TECEquipment_PropertyChanged;
        }

        public TECEquipment() : this(Guid.NewGuid()) { }

        //Copy Constructor
        public TECEquipment(TECEquipment equipmentSource, Dictionary<Guid, Guid> guidDictionary = null,
            ObservableListDictionary<ITECObject> characteristicReference = null, TemplateSynchronizer<TECSubScope> ssSynchronizer = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = equipmentSource.Guid; }
            foreach (TECSubScope subScope in equipmentSource.SubScope)
            {
                var toAdd = new TECSubScope(subScope, guidDictionary, characteristicReference);
                if (ssSynchronizer != null && ssSynchronizer.Contains(subScope))
                {
                    ssSynchronizer.LinkNew(ssSynchronizer.GetTemplate(subScope), toAdd);
                }
                characteristicReference?.AddItem(subScope,toAdd);
                SubScope.Add(toAdd);
            }
            copyPropertiesFromScope(equipmentSource);
        }
        #endregion //Constructors

        #region Methods
        public object DropData()
        {
            TECEquipment outEquip = new TECEquipment(this);
            return outEquip;
        }

        private void SubScope_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "SubScope", this, notifyCombinedChanged, notifyCostChanged, notifyPointChanged);
        }

        private void TECEquipment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Location")
            {
                var args = e as TECChangedEventArgs;
                var location = args.Value as TECLabeled;
                foreach (TECSubScope subScope in this.SubScope)
                {
                    if (subScope.Location == location)
                    {
                        subScope.Location = this.Location;
                    }
                }
            }
        }
        private int getPointNumber()
        {
            var totalPoints = 0;
            foreach (TECSubScope subScope in SubScope)
            {
                totalPoints += subScope.PointNumber;
            }
            return totalPoints;
        }
        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts();
            foreach(TECSubScope subScope in SubScope)
            {
                costs += subScope.CostBatch;
            }
            return costs;
        }
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.SubScope, "SubScope");
            return saveList;
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }

        private void notifyPointChanged(int numPoints)
        {
            if (!IsTypical)
            {
                PointChanged?.Invoke(numPoints);
            }
        }

        #endregion

        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECEquipment(this, characteristicReference: typicalDictionary);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            if(property == "SubScope" && item is TECSubScope subScope)
            {
                this.SubScope.Add(subScope);
            }
            else
            {
                this.AddChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            if (property == "SubScope" && item is TECSubScope subScope)
            {
                return this.SubScope.Remove(subScope);
            }
            else
            {
                return this.RemoveChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            if (property == "SubScope" && item is TECSubScope subScope)
            {
                return this.SubScope.Contains(subScope);
            }
            else
            {
                return this.ContainsChildForScopeProperty(property, item);
            }
        }

        void ITypicalable.MakeTypical()
        {
            this.IsTypical = true;
            TypicalableUtilities.MakeChildrenTypical(this);
        }
        #endregion
    }
}
