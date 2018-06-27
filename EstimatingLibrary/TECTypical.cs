using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using EstimatingLibrary.Utilities.WatcherFilters;

namespace EstimatingLibrary
{
    public class TECTypical : TECSystem
    {
        #region Fields
        private ObservableCollection<TECSystem> _instances;

        private ObservableListDictionary<ITECObject> _typicalInstanceDictionary;

        private ChangeWatcher watcher;
        #endregion

        #region Constructors
        public TECTypical(Guid guid) : base(guid)
        {
            this.IsTypical = true;
            _instances = new ObservableCollection<TECSystem>();

            TypicalInstanceDictionary = new ObservableListDictionary<ITECObject>();

            _instances.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Instances");

            watcher = new ChangeWatcher(this);
            //watcher.Changed += handleSystemChanged;
            new TypicalWatcherFilter(watcher).TypicalChanged += handleSystemChanged;


        }
        
        public TECTypical() : this(Guid.NewGuid()) { }

        public TECTypical(TECTypical source, Dictionary<Guid, Guid> guidDictionary = null,
            ObservableListDictionary<ITECObject> characteristicReference = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = source.Guid; }
            foreach (TECEquipment equipment in source.Equipment)
            {
                var toAdd = new TECEquipment(equipment, guidDictionary, characteristicReference);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(equipment, toAdd);
                }
                Equipment.Add(toAdd);
            }
            foreach (TECController controller in source.Controllers)
            {
                var toAdd = controller.CopyController(guidDictionary);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(controller, toAdd);
                }
                AddController(toAdd);
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
            this.copyPropertiesFromLocated(source);
        }

        public TECTypical(TECSystem system, TECScopeManager manager) : this()
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            guidDictionary[_guid] = system.Guid;
            foreach (TECEquipment equipment in system.Equipment)
            {
                var toAdd = new TECEquipment(equipment, guidDictionary);
                Equipment.Add(toAdd);
            }
            foreach (TECController controller in system.Controllers)
            {
                var toAdd = controller.CopyController(guidDictionary);
                AddController(toAdd);
            }
            foreach (TECPanel panel in system.Panels)
            {
                var toAdd = new TECPanel(panel, guidDictionary);
                Panels.Add(toAdd);
            }
            foreach (TECMisc misc in system.MiscCosts)
            {
                var toAdd = new TECMisc(misc);
                MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in system.ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch);
                ScopeBranches.Add(toAdd);
            }
            this.copyPropertiesFromLocated(system);
            ModelLinkingHelper.LinkSystem(this, manager, guidDictionary);
        }
        #endregion

        #region Properties
        public ObservableCollection<TECSystem> Instances
        {
            get { return _instances; }
            set
            {
                var old = _instances;
                _instances.CollectionChanged -= (sender, args) => handleCollectionChanged(sender, args, "Instances");
                _instances = value;
                notifyTECChanged(Change.Edit, "Instances", this, value, old);
                _instances.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Instances");
            }
        }

        public ObservableListDictionary<ITECObject> TypicalInstanceDictionary
        {
            get
            {
                return _typicalInstanceDictionary;
            }
            set
            {
                if (_typicalInstanceDictionary != null)
                {
                    _typicalInstanceDictionary.CollectionChanged -= typicalInstanceDictionary_CollectionChanged;
                }
                _typicalInstanceDictionary = value;
                if (_typicalInstanceDictionary != null)
                {
                    _typicalInstanceDictionary.CollectionChanged += typicalInstanceDictionary_CollectionChanged;
                }
            }
        }
        #endregion

        #region Methods
        public TECSystem AddInstance(TECBid bid)
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            var newSystem = new TECSystem();
            newSystem.Name = Name;
            newSystem.Description = Description;
            foreach (TECEquipment equipment in Equipment)
            {
                var toAdd = new TECEquipment(equipment, guidDictionary, TypicalInstanceDictionary);
                _typicalInstanceDictionary.AddItem(equipment, toAdd);
                newSystem.Equipment.Add(toAdd);
            }
            foreach (TECController controller in Controllers)
            {
                var toAdd = controller.CopyController(guidDictionary);
                _typicalInstanceDictionary.AddItem(controller, toAdd);
                newSystem.AddController(toAdd);
            }
            foreach (TECPanel panel in Panels)
            {
                var toAdd = new TECPanel(panel, guidDictionary);
                _typicalInstanceDictionary.AddItem(panel, toAdd);
                newSystem.Panels.Add(toAdd);
            }
            foreach (TECMisc misc in MiscCosts)
            {
                var toAdd = new TECMisc(misc);
                _typicalInstanceDictionary.AddItem(misc, toAdd);
                newSystem.MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch);
                _typicalInstanceDictionary.AddItem(branch, toAdd);
                newSystem.ScopeBranches.Add(toAdd);
            }
            foreach (TECAssociatedCost cost in AssociatedCosts)
            {
                newSystem.AssociatedCosts.Add(cost);
            }
            ModelLinkingHelper.LinkSystem(newSystem, bid, guidDictionary);
            
            Instances.Add(newSystem);

            return (newSystem);
        }

        public void UpdateInstanceConnections()
        {
            foreach(TECController controller in this.Controllers)
            {
                foreach (TECController instance in this.TypicalInstanceDictionary.GetInstances(controller))
                {
                    instance.RemoveAllChildConnections();
                    foreach (IControllerConnection connection in controller.ChildrenConnections)
                    {
                        List<IControllerConnection> instanceConnections = new List<IControllerConnection>();
                        if(connection is TECNetworkConnection netConnection)
                        {
                            TECNetworkConnection netInstanceConnection = instance.AddNetworkConnection(netConnection.NetworkProtocol);
                            
                            foreach (IConnectable child in netConnection.Children)
                            {
                                foreach (IConnectable instanceChild in this.TypicalInstanceDictionary.GetInstances(child))
                                {
                                    foreach (TECSystem system in this.Instances)
                                    {
                                        if (system.IsDirectDescendant(instanceChild))
                                        {
                                            netInstanceConnection.AddChild(instanceChild);
                                        }
                                    }
                                }
                            }
                            instanceConnections.Add(netInstanceConnection);
                        }
                        else if(connection is TECHardwiredConnection hardwired)
                        {
                            foreach (IConnectable instanceChild in this.TypicalInstanceDictionary.GetInstances(hardwired.Child))
                            {
                                foreach (TECSystem system in this.Instances)
                                {
                                    if (system.IsDirectDescendant(instanceChild))
                                    {
                                        instanceConnections.Add(instance.Connect(instanceChild, connection.Protocol));
                                    }
                                }
                            }
                        }
                        instanceConnections.ForEach(x => x.UpdatePropertiesBasedOn(connection));
                    }
                }
            }
            foreach(TECInterlockConnection interlock in this.GetAllSubScope().SelectMany(x => x.Interlocks))
            {
                foreach(var instanceInterlock in TypicalInstanceDictionary.GetInstances(interlock))
                {
                    instanceInterlock.UpdatePropertiesBasedOn(interlock);
                }
            }
        }
        public bool CanUpdateInstanceConnections()
        {
            bool canExecute = this.Instances.Count > 0;

            return canExecute;
        }

        public List<IControllerConnection> CreateTypicalAndInstanceConnections(TECController typicalController, TECSubScope typicalSubScope, IProtocol protocol)
        {
            if (!this.GetAllSubScope().Contains(typicalSubScope))
            {
                throw new Exception("SubScope does not exist in typical.");
            }
            if (!this.Controllers.Contains(typicalController))
            {
                throw new Exception("Controller does not exist in typical.");
            }

            List<IControllerConnection> outConnections = new List<IControllerConnection>();
            outConnections.Add(typicalController.Connect(typicalSubScope, protocol));

            foreach (TECController instanceController
                    in this.TypicalInstanceDictionary.GetInstances(typicalController))
            {
                foreach (TECSubScope instanceSubScope
                    in this.TypicalInstanceDictionary.GetInstances(typicalSubScope))
                {
                    bool found = false;
                    foreach (TECSystem instance in this.Instances)
                    {
                        if (instance.Controllers.Contains(instanceController) &&
                            instance.GetAllSubScope().Contains(instanceSubScope))
                        {
                            outConnections.Add(instanceController.Connect(instanceSubScope, protocol));
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }

            return outConnections;
        }
        public List<T> GetInstancesFromTypical<T>(T typical) where T : TECObject
        {
            return this.TypicalInstanceDictionary.GetInstances(typical);
        }
        
        internal void RefreshRegistration()
        {
            watcher.Changed -= handleSystemChanged;
            watcher = new ChangeWatcher(this);
            watcher.Changed += handleSystemChanged;
        }

        public override object DragDropCopy(TECScopeManager scopeManager)
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            TECTypical outSystem = new TECTypical(this, guidDictionary);
            ModelLinkingHelper.LinkSystem(outSystem, scopeManager, guidDictionary);
            return outSystem;
        }

        protected override CostBatch getCosts()
        {
            CostBatch costs = new CostBatch();
            foreach (TECSystem instance in Instances)
            {
                costs += instance.CostBatch;
            }
            return costs;
        }
        protected override SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(this.Instances, "Instances");
            saveList.Add("TypicalInstances");
            return saveList;
        }

        private void typicalInstanceDictionary_CollectionChanged(Tuple<Change, ITECObject, ITECObject> obj)
        {
            notifyTECChanged(obj.Item1, "TypicalInstanceDictionary", obj.Item2, obj.Item3);
        }
        private void removeFromDictionary(IEnumerable<ITECObject> typicalList, IEnumerable<ITECObject> instanceList)
        {
            foreach (TECObject typical in typicalList)
            {
                foreach (TECObject instance in instanceList)
                {
                    if (TypicalInstanceDictionary.GetInstances(typical).Contains(instance))
                    {
                        TypicalInstanceDictionary.RemoveItem(typical, instance);
                    }
                }
            }
        }

        protected override int points()
        {
            int pointNum = 0;
            foreach(TECSystem instance in Instances)
            {
                pointNum += instance.PointNumber;
            }
            return pointNum;
        }

        #region Event Handlers
        private void handleSystemChanged(TECChangedEventArgs args)
        {
            if (Instances.Count > 0 && args.Value.GetType() != typeof(TECSystem))
            {
                if (args.Change == Change.Add)
                {
                    handleAdd(args);
                }
                else if (args.Change == Change.Remove)
                {
                    handleRemove(args);
                }
                else if (args.Sender is TECPoint point)
                {
                    handlePointChanged(point, args.PropertyName);
                }
                else if (args.Sender is TECMisc misc)
                {
                    handleMiscChanged(misc, args.PropertyName);
                }
                else if (args.Sender is TECController controller)
                {
                    handleControllerChaned(controller, args.PropertyName);
                }
            }
        }
        
        protected override void handleCollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                CostBatch costs = new CostBatch();
                int pointNum = 0;
                bool raiseEvents = false;
                foreach (object item in e.NewItems)
                {
                    if (item != null)
                    {
                        if (item is TECSystem sys)
                        {
                            costs += sys.CostBatch;
                            pointNum += sys.PointNumber;
                            raiseEvents = true;
                        }
                        else if (item is TECEquipment equip)
                        {
                            equip.SubScopeCollectionChanged += handleSubScopeCollectionChanged;
                        }
                        notifyTECChanged(Change.Add, propertyName, this, item);
                    }
                }
                if (raiseEvents)
                {
                    invokeCostChanged(costs);
                    invokePointChanged(pointNum);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                CostBatch costs = new CostBatch();
                int pointNum = 0;
                bool raiseEvents = false;
                foreach (object item in e.OldItems)
                {
                    if (item != null)
                    {
                        if (item is TECSystem sys)
                        {
                            costs += sys.CostBatch;
                            pointNum += sys.PointNumber;
                            raiseEvents = true;
                            handleInstanceRemoved(sys);
                        }
                        else if (item is TECEquipment equip)
                        {
                            equip.SubScopeCollectionChanged -= handleSubScopeCollectionChanged;
                            foreach (TECSubScope ss in equip.SubScope)
                            {
                                handleSubScopeRemoval(ss);
                            }
                        }
                        notifyTECChanged(Change.Remove, propertyName, this, item);
                    }
                }
                if (raiseEvents)
                {
                    invokeCostChanged(costs * -1);
                    invokePointChanged(-pointNum);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyTECChanged(Change.Edit, propertyName, this, sender, sender);
            }
        }
        protected override void scopeCollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
            //Is overridden so that TECTypical doesn't raise cost changed when an associated cost is added or removed.
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    notifyCombinedChanged(Change.Add, propertyName, this, item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    notifyCombinedChanged(Change.Remove, propertyName, this, item);
                }
            }
        }

        private void handleInstanceRemoved(TECSystem instance)
        {
            foreach (TECSubScope subScope in instance.GetAllSubScope())
            {
                if (subScope.Connection != null && !instance.Controllers.Contains(subScope.Connection.ParentController))
                {
                    subScope.Connection.ParentController.Disconnect(subScope);
                }
            }
            foreach(TECController controller in instance.Controllers)
            {
                if(controller.ParentConnection != null && !instance.Controllers.Contains(controller.ParentConnection.ParentController))
                {
                    controller.ParentConnection.ParentController.Disconnect(controller);
                }
                foreach(TECConnection connection in controller.ChildrenConnections)
                {
                    if(connection is TECHardwiredConnection hardwired)
                    {
                        if (instance.GetAllSubScope().Contains(hardwired.Child))
                        {
                            hardwired.Child.SetParentConnection(null);
                        }
                    } else if(connection is TECNetworkConnection netConnect)
                    {
                        foreach(var child in netConnect.Children)
                        {
                            if(child is TECController childController)
                            {
                                if (instance.Controllers.Contains(childController))
                                {
                                    break;
                                }
                            }
                            if(child is TECSubScope childSub)
                            {
                                if (instance.GetAllSubScope().Contains(childSub))
                                {
                                    break;
                                }
                            }
                            child.SetParentConnection(null);
                        }
                    }
                }
            }
            removeFromDictionary(Panels, instance.Panels);
            removeFromDictionary(Equipment, instance.Equipment);
            foreach(TECEquipment instanceEquip in instance.Equipment)
            {
                removeFromDictionary(GetAllSubScope(), instanceEquip.SubScope);
                foreach (TECSubScope instanceSubScope in instanceEquip.SubScope)
                {
                    foreach(TECSubScope subScope in GetAllSubScope())
                    {
                        removeFromDictionary(subScope.Points, instanceSubScope.Points);
                        removeFromDictionary(subScope.Interlocks, instanceSubScope.Interlocks);
                    }
                }
            }
            removeFromDictionary(Controllers, instance.Controllers);
            removeFromDictionary(MiscCosts, instance.MiscCosts);
            removeFromDictionary(ScopeBranches, instance.ScopeBranches);
        }
        private void handlePointChanged(TECPoint point, string propertyName)
        {
            PropertyInfo property = typeof(TECPoint).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite && TypicalInstanceDictionary.ContainsKey(point))
            {
                foreach (TECPoint instance in TypicalInstanceDictionary.GetInstances(point))
                {
                    property.SetValue(instance, property.GetValue(point), null);
                }
            }
        }
        private void handleMiscChanged(TECMisc misc, string propertyName)
        {
            PropertyInfo property = typeof(TECMisc).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite && TypicalInstanceDictionary.ContainsKey(misc))
            {
                foreach (TECMisc instance in TypicalInstanceDictionary.GetInstances(misc))
                {
                    property.SetValue(instance, property.GetValue(misc), null);
                }
            }
        }
        private void handleControllerChaned(TECController controller, string propertyName)
        {
            if (propertyName == "Type" && controller is TECProvidedController provided)
            {
                foreach (var instance in this.GetInstancesFromTypical(provided))
                {
                    if (instance.CanChangeType(provided.Type))
                    {
                        instance.ChangeType(provided.Type);
                    }
                }
            }
        }

        private void handleAdd(TECChangedEventArgs args)
        {
            ITypicalable sender = args.Sender as ITypicalable;
            List<ITECObject> parentInstances = new List<ITECObject>();
            if (args.Sender is TECTypical typ) { parentInstances.AddRange(this.Instances); }
            else { parentInstances = TypicalInstanceDictionary.GetInstances(sender as ITECObject); }
            foreach (ITECObject parentInstance in parentInstances)
            {
                ITypicalable instanceSender = parentInstance as ITypicalable;

                if (instanceSender == null)
                {
                    throw new Exception("Change occured from object which is not typicalable");
                }
                ITECObject instanceValue = args.Value as ITECObject;
                if (instanceValue == null)
                {
                    throw new Exception("Value to add is not ITECObject");
                }
                if (args.Value is ITypicalable typicalChild)
                {
                    instanceValue = typicalChild.CreateInstance(TypicalInstanceDictionary);
                }

                instanceSender.AddChildForProperty(args.PropertyName, instanceValue);

                TypicalInstanceDictionary.AddItem(args.Value as ITECObject, instanceValue);
            }
            
            
        }
        private void handleRemove(TECChangedEventArgs args)
        {
            ITypicalable sender = args.Sender as ITypicalable;
            List<ITECObject> parentInstances = new List<ITECObject>();
            if (args.Sender is TECTypical typ) { parentInstances.AddRange(this.Instances); }
            else { parentInstances = TypicalInstanceDictionary.GetInstances(sender as ITECObject); }
            foreach (ITECObject parentInstance in parentInstances)
            {
                ITypicalable instanceSender = parentInstance as ITypicalable;

                if (instanceSender == null)
                {
                    throw new Exception("Change occured from object which is not typicalable");
                }
                ITECObject instanceValue = TypicalInstanceDictionary.GetInstances(args.Value as ITECObject)
                    .Where(x => instanceSender.ContainsChildForProperty(args.PropertyName, x)).First();
                if (instanceValue == null)
                {
                    throw new Exception("Value to add is not ITECObject");
                }

                instanceSender.RemoveChildForProperty(args.PropertyName, instanceValue);

                TypicalInstanceDictionary.RemoveItem(args.Value as ITECObject, instanceValue);
            }
        }

        #endregion
        #endregion
    }
}
