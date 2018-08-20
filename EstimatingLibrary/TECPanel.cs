using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECPanel : TECLocated, IDragDropable, ITypicalable, ICatalogContainer
    {
        #region Properties
        private TECPanelType _type;
        public TECPanelType Type
        {
            get { return _type; }
            set
            {
                var old= Type;
                _type = value;
                notifyCombinedChanged(Change.Edit, "Type", this, value, old);
                notifyCostChanged(value.CostBatch - old.CostBatch);
            }
        }

        public ObservableCollection<TECController> Controllers { get; } = new ObservableCollection<TECController>();
        
        public bool IsTypical { get; private set; }

        #endregion

        public TECPanel(Guid guid, TECPanelType type) : base(guid)
        {
            IsTypical = false;
            _guid = guid;
            _type = type;
            Controllers.CollectionChanged += controllersCollectionChanged;
        }
        public TECPanel(TECPanelType type) : this(Guid.NewGuid(), type) { }
        public TECPanel(TECPanel panel, Dictionary<Guid, Guid> guidDictionary = null) : this(panel.Type)
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = panel.Guid; }
            copyPropertiesFromScope(panel);
            foreach (TECController controller in panel.Controllers)
            {
                Controllers.Add(controller.CopyController(guidDictionary));
            }
        }
        public object DropData()
        {
            var outPanel = new TECPanel(this);
            return outPanel;
        }
        
        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts() + Type.CostBatch;
            return costs;
        }
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Type, "Type");
            saveList.AddRange(this.Controllers, "Controllers");
            return saveList;
        }
        protected override RelatableMap linkedObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.linkedObjects());
            saveList.Add(this.Type, "Type");
            saveList.AddRange(this.Controllers, "Controllers");
            return saveList;
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }

        private void controllersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, "Controllers", this,
                notifyCombinedChanged);
        }

        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            if (!this.IsTypical)
            {
                throw new Exception("Attempted to create an instance of an object which is already instanced.");
            }
            else
            {
                return new TECPanel(this);
            }
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            if (property == "Controllers" && item is TECController controller)
            {
                Controllers.Add(controller);
            }
            else
            {
                this.AddChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.RemoveChildForProperty(string property, ITECObject item)
        {
            if (property == "Controllers" && item is TECController controller)
            {
                return Controllers.Remove(controller);
            }
            else
            {
                return this.RemoveChildForScopeProperty(property, item);
            }
        }

        bool ITypicalable.ContainsChildForProperty(string property, ITECObject item)
        {
            if (property == "Controllers" && item is TECController controller)
            {
                return Controllers.Contains(controller);
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

        #region ICatalogContainer
        public override bool RemoveCatalogItem<T>(T item, T replacement)
        {
            bool alreadyRemoved = base.RemoveCatalogItem(item, replacement);

            bool replacedType = false;
            if (item == this.Type)
            {
                if (replacement is TECPanelType type)
                {
                    this.Type = type;
                    replacedType = true;
                }
                else throw new ArgumentException("Replacement PanelType cannot be null.");
            }
            return (replacedType || alreadyRemoved);
        }
        #endregion
    }
}
