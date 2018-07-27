using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace EstimatingLibrary
{
    public class TECSystem : TECLocated, INotifyPointChanged, IDDCopiable, ITypicalable
    {
        #region Fields
        private ObservableCollection<TECController> _controllers = new ObservableCollection<TECController>();

        private bool _isSingleton = false;
        #endregion

        #region Constructors
        public TECSystem(Guid guid) : base(guid)
        {
            IsTypical = false;
            
            Equipment.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Equipment");
            Panels.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Panels");
            MiscCosts.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "MiscCosts");
            ScopeBranches.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "ScopeBranches");
            ProposalItems.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "ProposalItems");
        }

        public TECSystem() : this(Guid.NewGuid()) { }

        public TECSystem(TECSystem source, TECScopeManager manager, Dictionary<Guid, Guid> guidDictionary = null,
            ObservableListDictionary<ITECObject> characteristicReference = null, Tuple<TemplateSynchronizer<TECEquipment>, TemplateSynchronizer<TECSubScope>> synchronizers = null) : this()
        {
            if (guidDictionary == null)
            { guidDictionary = new Dictionary<Guid, Guid>();  }

            guidDictionary[_guid] = source.Guid;
            foreach (TECEquipment equipment in source.Equipment)
            {
                var toAdd = new TECEquipment(equipment, guidDictionary, characteristicReference, ssSynchronizer: synchronizers?.Item2);
                if (synchronizers != null && synchronizers.Item1.Contains(equipment))
                {
                    synchronizers.Item1.LinkNew(synchronizers.Item1.GetTemplate(equipment), toAdd);
                }
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(equipment, toAdd);
                }
                Equipment.Add(toAdd);
            }
            foreach (TECController controller in source._controllers)
            {
                var toAdd = controller.CopyController(guidDictionary);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(controller, toAdd);
                }
                _controllers.Add(toAdd);
            }
            foreach (TECPanel panel in source.Panels)
            {
                var toAdd = new TECPanel(panel, guidDictionary);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(panel, toAdd);
                }
                Panels.Add(toAdd);
            }
            foreach (TECMisc misc in source.MiscCosts)
            {
                var toAdd = new TECMisc(misc);
                MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in source.ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch);
                ScopeBranches.Add(toAdd);
            }
            foreach(TECProposalItem item in source.ProposalItems)
            {
                var toAdd = new TECProposalItem(item, guidDictionary);
                ProposalItems.Add(toAdd);
            }
            this.copyPropertiesFromLocated(source);
            ModelLinkingHelper.LinkSystem(this, manager, guidDictionary);
        }
        #endregion

        #region Events
        public event Action<int> PointChanged;
        #endregion

        #region Properties
        public ObservableCollection<TECEquipment> Equipment { get; } = new ObservableCollection<TECEquipment>();
        public ReadOnlyObservableCollection<TECController> Controllers
        {
            get { return new ReadOnlyObservableCollection<TECController>(_controllers); }
        }
        public ObservableCollection<TECPanel> Panels { get; } = new ObservableCollection<TECPanel>();
        public ObservableCollection<TECMisc> MiscCosts { get; } = new ObservableCollection<TECMisc>();
        public ObservableCollection<TECScopeBranch> ScopeBranches { get; } = new ObservableCollection<TECScopeBranch>();
        public ObservableCollection<TECProposalItem> ProposalItems { get; } = new ObservableCollection<TECProposalItem>();

        public bool IsSingleton
        {
            get { return _isSingleton; }
            set
            {
                var old = IsSingleton;
                _isSingleton = value;
                notifyTECChanged(Change.Edit, "IsSingleton", this, value, old);
            }
        }
        public bool IsTypical
        {
            get; protected set;
        }
        public int PointNumber
        {
            get
            {
                return points();
            }
        }

        #endregion

        #region Methods
        public void AddController(TECController controller)
        {
            _controllers.Add(controller);
            if (this.IsTypical) { ((ITypicalable)controller).MakeTypical(); }
            notifyTECChanged(Change.Add, "Controllers", this, controller);
            notifyCostChanged(controller.CostBatch);
        }
        public bool RemoveController(TECController controller)
        {
            controller.DisconnectAll();
            bool success = _controllers.Remove(controller);
            foreach(TECPanel panel in this.Panels)
            {
                if (panel.Controllers.Contains(controller)) { panel.Controllers.Remove(controller); }
            }
            notifyTECChanged(Change.Remove, "Controllers", this, controller);
            notifyCostChanged(-controller.CostBatch);
            return success;
        }
        public void SetControllers(IEnumerable<TECController> newControllers)
        {
            IEnumerable<TECController> oldControllers = Controllers;
            if (this.IsTypical) { newControllers.ForEach((x => ((ITypicalable)x).MakeTypical())); }
            _controllers = new ObservableCollection<TECController>(newControllers);
            notifyTECChanged(Change.Edit, "Controllers", this, newControllers, oldControllers);
        }

        public virtual object DragDropCopy(TECScopeManager scopeManager)
        {
            TECSystem outSystem = new TECSystem(this, scopeManager);
            outSystem.IsTypical = this.IsTypical;
            return outSystem;
        }

        public List<TECSubScope> GetAllSubScope()
        {
            var outSubScope = new List<TECSubScope>();
            foreach (TECEquipment equip in Equipment)
            {
                foreach (TECSubScope sub in equip.SubScope)
                {
                    outSubScope.Add(sub);
                }
            }
            return outSubScope;
        }
        protected virtual int points()
        {
            var totalPoints = 0;
            foreach (TECEquipment equipment in Equipment)
            {
                totalPoints += equipment.PointNumber;
            }
            return totalPoints;
        }

        protected override CostBatch getCosts()
        {
            CostBatch costs = base.getCosts();
            foreach (TECEquipment item in Equipment)
            {
                costs += item.CostBatch;
            }
            foreach (TECController item in Controllers)
            {
                costs += item.CostBatch;
            }
            foreach (TECPanel item in Panels)
            {
                costs += item.CostBatch;
            }
            foreach (TECMisc item in MiscCosts)
            {
                costs += item.CostBatch;
            }
            return costs;
        }

        protected void notifyPointChanged(int pointNum)
        {
            if (!IsTypical)
            {
                PointChanged?.Invoke(pointNum);
            }
        }
        protected override void notifyCostChanged(CostBatch costs)
        {
            if (!IsTypical)
            {
                base.notifyCostChanged(costs);
            }
        }

        protected void invokeCostChanged(CostBatch costs)
        {
            base.notifyCostChanged(costs);
        }
        protected void invokePointChanged(int pointNum)
        {
            PointChanged?.Invoke(pointNum);
        }
        
        #region Event Handlers
        protected virtual void handleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, string propertyName)
        {
            CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this, 
                notifyCombinedChanged, notifyCostChanged, notifyPointChanged,
                onAdd, onRemove);

            void onAdd(object obj)
            {
                if (obj is TECEquipment equip)
                {
                    equip.SubScopeCollectionChanged += handleSubScopeCollectionChanged;
                }
            }
            void onRemove(object obj)
            {
                if (obj is TECEquipment equip)
                {
                    equip.SubScopeCollectionChanged -= handleSubScopeCollectionChanged;
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        handleSubScopeRemoval(ss);
                    }
                }
            }
        }

        protected virtual void handleSubScopeCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(TECSubScope item in args.OldItems)
                {
                    handleSubScopeRemoval(item);
                }
            }
        }

        protected void handleSubScopeRemoval(TECSubScope removed)
        {
            TECController controller = removed.Connection?.ParentController;
            if(controller != null)
            {
                controller.Disconnect(removed);
            }
        }

        #endregion
        #endregion

        #region IRelatable
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.Equipment, "Equipment");
            saveList.AddRange(this.Panels, "Panels");
            saveList.AddRange(this.Controllers, "Controllers");
            saveList.AddRange(this.MiscCosts, "MiscCosts");
            saveList.AddRange(this.ScopeBranches, "ScopeBranches");
            saveList.AddRange(this.ProposalItems, "ProposalItems");
            return saveList;
        }
        #endregion

        #region ITypicalable
        ITECObject ITypicalable.CreateInstance(ObservableListDictionary<ITECObject> typicalDictionary)
        {
            throw new NotImplementedException();
        }

        void ITypicalable.AddChildForProperty(string property, ITECObject item)
        {
            if (property == "Controllers" && item is TECController controller)
            {
                AddController(controller);
            }
            else if (property == "Equipment" && item is TECEquipment equipment)
            {
                Equipment.Add(equipment);
            }
            else if (property == "Panels" && item is TECPanel panel)
            {
                Panels.Add(panel);
            }
            else if (property == "MiscCosts" && item is TECMisc misc)
            {
                MiscCosts.Add(misc);
            }
            else if (property == "ScopeBranch" && item is TECScopeBranch branch)
            {
                ScopeBranches.Add(branch);
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
                return RemoveController(controller);
            }
            else if (property == "Equipment" && item is TECEquipment equipment)
            {
                return Equipment.Remove(equipment);
            }
            else if (property == "Panels" && item is TECPanel panel)
            {
                return Panels.Remove(panel);
            }
            else if (property == "MiscCosts" && item is TECMisc misc)
            {
                return MiscCosts.Remove(misc);
            }
            else if (property == "ScopeBranch" && item is TECScopeBranch branch)
            {
                return ScopeBranches.Remove(branch);
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
            else if (property == "Equipment" && item is TECEquipment equipment)
            {
                return Equipment.Contains(equipment);
            }
            else if (property == "Panels" && item is TECPanel panel)
            {
                return Panels.Contains(panel);
            }
            else if (property == "MiscCosts" && item is TECMisc misc)
            {
                return MiscCosts.Contains(misc);
            }
            else if (property == "ScopeBranch" && item is TECScopeBranch branch)
            {
                return ScopeBranches.Contains(branch);
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
