using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EstimatingLibrary
{
    public class TECPanel : TECLocated, IDDCopiable, ITypicalable
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

        private ObservableCollection<TECController> _controllers;
        public ObservableCollection<TECController> Controllers
        {
            get { return _controllers; }
            set
            {
                var old = Controllers;
                _controllers = value;
                Controllers.CollectionChanged -= controllersCollectionChanged;
                notifyCombinedChanged(Change.Edit, "Controllers", this, value, old);
                Controllers.CollectionChanged += controllersCollectionChanged;
            }
        }
        
        public bool IsTypical { get; private set; }

        #endregion

        public TECPanel(Guid guid, TECPanelType type) : base(guid)
        {
            IsTypical = false;
            _guid = guid;
            _controllers = new ObservableCollection<TECController>();
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
                _controllers.Add(controller.CopyController(guidDictionary));
            }
        }
        public object DragDropCopy(TECScopeManager scopeManager)
        {
            var outPanel = new TECPanel(this);
            ModelLinkingHelper.LinkScopeItem(outPanel, scopeManager);
            return outPanel;
        }
        
        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts();
            costs += Type.CostBatch;
            return costs;
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.Add(this.Type, "Type");
            saveList.AddRange(this.Controllers, "Controllers");
            return saveList;
        }
        protected override SaveableMap linkedObjects()
        {
            SaveableMap saveList = new SaveableMap();
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
    }
}
