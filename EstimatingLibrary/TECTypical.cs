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
        private TypicalWatcherFilter watcher;
        #endregion

        #region Constructors
        public TECTypical(Guid guid) : base(guid)
        {
            this.IsTypical = true;
            
            Instances.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Instances");
            TypicalInstanceDictionary.CollectionChanged += typicalInstanceDictionary_CollectionChanged;

            watcher = new TypicalWatcherFilter(new ChangeWatcher(this));
            watcher.TypicalChanged += handleSystemChanged;
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
            foreach(TECProposalItem item in system.ProposalItems)
            {
                var toAdd = new TECProposalItem(item, guidDictionary);
                ProposalItems.Add(toAdd);
            }
            this.copyPropertiesFromLocated(system);
            ModelLinkingHelper.LinkSystem(this, manager, guidDictionary);
        }
        #endregion

        #region Properties
        public ObservableCollection<TECSystem> Instances { get; } = new ObservableCollection<TECSystem>();
        public ObservableListDictionary<ITECObject> TypicalInstanceDictionary { get; } = new ObservableListDictionary<ITECObject>();
        #endregion

        #region Methods
        public TECSystem AddInstance(TECBid bid)
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            var newSystem = new TECSystem();
            newSystem.CopyPropertiesFromScope(this);
            foreach (TECEquipment equipment in Equipment)
            {
                var toAdd = new TECEquipment(equipment, guidDictionary, TypicalInstanceDictionary);
                TypicalInstanceDictionary.AddItem(equipment, toAdd);
                newSystem.Equipment.Add(toAdd);
            }
            foreach (TECController controller in Controllers)
            {
                var toAdd = controller.CopyController(guidDictionary);
                TypicalInstanceDictionary.AddItem(controller, toAdd);
                newSystem.AddController(toAdd);
            }
            foreach (TECPanel panel in Panels)
            {
                var toAdd = new TECPanel(panel, guidDictionary);
                TypicalInstanceDictionary.AddItem(panel, toAdd);
                newSystem.Panels.Add(toAdd);
            }
            foreach (TECMisc misc in MiscCosts)
            {
                var toAdd = new TECMisc(misc);
                TypicalInstanceDictionary.AddItem(misc, toAdd);
                newSystem.MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch);
                TypicalInstanceDictionary.AddItem(branch, toAdd);
                newSystem.ScopeBranches.Add(toAdd);
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
        public List<T> GetInstancesFromTypical<T>(T typical) where T : ITECObject
        {
            return this.TypicalInstanceDictionary.GetInstances(typical);
        }
        
        internal void RefreshRegistration()
        {
            watcher.TypicalChanged -= handleSystemChanged;
            watcher = new TypicalWatcherFilter(new ChangeWatcher(this));
            watcher.TypicalChanged += handleSystemChanged;
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
            if (Instances.Count > 0 && args.Value?.GetType() != typeof(TECSystem))
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
                    handleValueChanged(point, args.PropertyName);
                }
                else if (args.Sender is TECMisc misc)
                {
                    handleValueChanged(misc, args.PropertyName);
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
                        if (this.IsTypical && item is ITypicalable typ && !(item is TECSystem)) { typ.MakeTypical(); }
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
                            handleInstanceRemoved(sys);
                            costs += sys.CostBatch;
                            pointNum += sys.PointNumber;
                            raiseEvents = true;
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
            instance.Controllers.ForEach(x => x.DisconnectAll());
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.Panels, Panels);
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.Equipment, Equipment);

            var typicalSubScope = GetAllSubScope();
            foreach(TECEquipment instanceEquip in instance.Equipment)
            {
                TypicalInstanceDictionary.RemoveValuesForKeys(instanceEquip.SubScope, typicalSubScope);
                foreach (TECSubScope instanceSubScope in instanceEquip.SubScope)
                {
                    if (instanceSubScope.Connection != null && !instance.Controllers.Contains(instanceSubScope.Connection.ParentController))
                    {
                        instanceSubScope.Connection.ParentController.Disconnect(instanceSubScope);
                    }
                    foreach (TECSubScope subScope in typicalSubScope)
                    {
                        TypicalInstanceDictionary.RemoveValuesForKeys(instanceSubScope.Points, subScope.Points);
                        TypicalInstanceDictionary.RemoveValuesForKeys(instanceSubScope.Interlocks, subScope.Interlocks);
                    }
                }
            }
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.Controllers, Controllers);
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.MiscCosts, MiscCosts);
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.ScopeBranches, ScopeBranches);
        }
        private void handleValueChanged<T>(T item, string propertyName) where T: ITECObject
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite && TypicalInstanceDictionary.ContainsKey(item))
            {
                foreach (T instance in TypicalInstanceDictionary.GetInstances(item))
                {
                    property.SetValue(instance, property.GetValue(item), null);
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
                    if(instanceValue != null)
                    {
                        TypicalInstanceDictionary.AddItem(args.Value as ITECObject, instanceValue);
                    }
                }
                if(instanceValue != null)
                {
                    instanceSender.AddChildForProperty(args.PropertyName, instanceValue);
                }

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
                ITECObject instanceValue = args.Value as ITECObject;
                if (instanceSender == null)
                {
                    throw new Exception("Change occured from object which is not typicalable");
                }
                if (instanceValue == null)
                {
                    throw new Exception("Value to add is not ITECObject");
                }

                if(instanceValue is ITypicalable)
                {
                    instanceValue = TypicalInstanceDictionary.GetInstances(instanceValue)
                    .Where(x => instanceSender.ContainsChildForProperty(args.PropertyName, x)).FirstOrDefault();
                    if(instanceValue != null)
                    {
                        TypicalInstanceDictionary.RemoveItem(args.Value as ITECObject, instanceValue);
                    }
                }

                if(instanceValue != null)
                {
                    instanceSender.RemoveChildForProperty(args.PropertyName, instanceValue);
                }

            }
        }

        #endregion
        #endregion
    }
}
