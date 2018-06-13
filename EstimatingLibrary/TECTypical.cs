using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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
        public TECTypical(Guid guid) : base(guid, true)
        {
            _instances = new ObservableCollection<TECSystem>();

            TypicalInstanceDictionary = new ObservableListDictionary<ITECObject>();

            _instances.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Instances");

            watcher = new ChangeWatcher(this);
            watcher.Changed += handleSystemChanged;
        }

        public TECTypical() : this(Guid.NewGuid()) { }

        public TECTypical(TECTypical source, Dictionary<Guid, Guid> guidDictionary = null,
            ObservableListDictionary<ITECObject> characteristicReference = null) : this()
        {
            if (guidDictionary != null)
            { guidDictionary[_guid] = source.Guid; }
            foreach (TECEquipment equipment in source.Equipment)
            {
                var toAdd = new TECEquipment(equipment, true, guidDictionary, characteristicReference);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(equipment, toAdd);
                }
                Equipment.Add(toAdd);
            }
            foreach (TECController controller in source.Controllers)
            {
                var toAdd = new TECController(controller, true, guidDictionary);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(controller, toAdd);
                }
                AddController(toAdd);
            }
            foreach (TECPanel panel in source.Panels)
            {
                var toAdd = new TECPanel(panel, true, guidDictionary);
                if (characteristicReference != null)
                {
                    characteristicReference.AddItem(panel, toAdd);
                }
                Panels.Add(toAdd);
            }
            foreach (TECMisc misc in source.MiscCosts)
            {
                var toAdd = new TECMisc(misc, true);
                MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in source.ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch, true);
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
                var toAdd = new TECEquipment(equipment, true, guidDictionary);
                Equipment.Add(toAdd);
            }
            foreach (TECController controller in system.Controllers)
            {
                var toAdd = new TECController(controller, true, guidDictionary);
                AddController(toAdd);
            }
            foreach (TECPanel panel in system.Panels)
            {
                var toAdd = new TECPanel(panel, true, guidDictionary);
                Panels.Add(toAdd);
            }
            foreach (TECMisc misc in system.MiscCosts)
            {
                var toAdd = new TECMisc(misc, true);
                MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in system.ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch, true);
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
            var newSystem = new TECSystem(false);
            newSystem.Name = Name;
            newSystem.Description = Description;
            foreach (TECEquipment equipment in Equipment)
            {
                var toAdd = new TECEquipment(equipment, false, guidDictionary, TypicalInstanceDictionary);
                _typicalInstanceDictionary.AddItem(equipment, toAdd);
                newSystem.Equipment.Add(toAdd);
            }
            foreach (TECController controller in Controllers)
            {
                var toAdd = new TECController(controller, false, guidDictionary);
                _typicalInstanceDictionary.AddItem(controller, toAdd);
                newSystem.AddController(toAdd);
            }
            foreach (TECPanel panel in Panels)
            {
                var toAdd = new TECPanel(panel, false, guidDictionary);
                _typicalInstanceDictionary.AddItem(panel, toAdd);
                newSystem.Panels.Add(toAdd);
            }
            foreach (TECMisc misc in MiscCosts)
            {
                var toAdd = new TECMisc(misc, false);
                _typicalInstanceDictionary.AddItem(misc, toAdd);
                newSystem.MiscCosts.Add(toAdd);
            }
            foreach (TECScopeBranch branch in ScopeBranches)
            {
                var toAdd = new TECScopeBranch(branch, false);
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
                        foreach(IControllerConnection instanceConnection in instanceConnections)
                        {
                            instanceConnection.Length = connection.Length;
                            instanceConnection.ConduitType = connection.ConduitType;
                            instanceConnection.ConduitLength = connection.ConduitLength;
                            instanceConnection.IsPlenum = connection.IsPlenum;
                        }
                        
                    }
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
            if (Instances.Count > 0)
            {
                if (args.Change == Change.Add)
                {
                    handleAdd(args.Value as TECObject, args.Sender);
                }
                else if (args.Change == Change.Remove)
                {
                    handleRemove(args.Value as TECObject, args.Sender);
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
            if (propertyName == "Type")
            {
                foreach (var instance in this.GetInstancesFromTypical(controller))
                {
                    if (instance.CanChangeType(controller.Type))
                    {
                        instance.ChangeType(controller.Type);
                    }
                }
            }
        }

        private void handleAdd(ITECObject value, ITECObject sender)
        {
            if (value is TECController && sender is TECTypical)
            {
                var characteristicController = value as TECController;
                foreach (TECSystem system in Instances)
                {
                    var controllerToAdd = new TECController(characteristicController, false);
                    _typicalInstanceDictionary.AddItem(characteristicController, controllerToAdd);
                    system.AddController(controllerToAdd);
                }
            }
            else if (value is TECPanel && sender is TECTypical)
            {
                var characteristicPanel = value as TECPanel;
                foreach (TECSystem system in Instances)
                {
                    var panelToAdd = new TECPanel(characteristicPanel, false);
                    _typicalInstanceDictionary.AddItem(characteristicPanel, panelToAdd);
                    system.Panels.Add(panelToAdd);
                }
            }
            else if (value is TECEquipment && sender is TECTypical)
            {
                var characteristicEquipment = value as TECEquipment;
                foreach (TECSystem system in Instances)
                {
                    var equipmentToAdd = new TECEquipment(characteristicEquipment, false, characteristicReference: TypicalInstanceDictionary);
                    _typicalInstanceDictionary.AddItem(characteristicEquipment, equipmentToAdd);
                    system.Equipment.Add(equipmentToAdd);
                }
            }
            else if (value is TECMisc misc && sender is TECTypical)
            {
                foreach (TECSystem system in Instances)
                {
                    var miscToAdd = new TECMisc(misc, false);
                    _typicalInstanceDictionary.AddItem(misc, miscToAdd);
                    system.MiscCosts.Add(miscToAdd);
                }
            }
            else if (value is TECScopeBranch branch && sender is TECTypical)
            {
                foreach (TECSystem system in Instances)
                {
                    var branchToAdd = new TECScopeBranch(branch, false);
                    _typicalInstanceDictionary.AddItem(branch, branchToAdd);
                    system.ScopeBranches.Add(branchToAdd);
                }
            }
            else if (value is TECSubScope && sender is TECEquipment)
            {
                var characteristicEquipment = sender as TECEquipment;
                var characteristicSubScope = value as TECSubScope;
                if (TypicalInstanceDictionary.ContainsKey(characteristicEquipment))
                {
                    foreach (TECEquipment equipment in TypicalInstanceDictionary.GetInstances(characteristicEquipment))
                    {
                        var subScopeToAdd = new TECSubScope(characteristicSubScope, false, characteristicReference: TypicalInstanceDictionary);
                        _typicalInstanceDictionary.AddItem(characteristicSubScope, subScopeToAdd);
                        equipment.SubScope.Add(subScopeToAdd);
                    }
                }
            }
            else if (value is IEndDevice && sender is TECSubScope)
            {
                var characteristicSubScope = sender as TECSubScope;
                var device = value as IEndDevice;
                if (TypicalInstanceDictionary.ContainsKey(characteristicSubScope))
                {
                    foreach (TECSubScope subScope in TypicalInstanceDictionary.GetInstances(characteristicSubScope))
                    {
                        subScope.Devices.Add(device);
                    }
                }   
            }
            else if (value is TECPoint && sender is TECSubScope)
            {
                var characteristicSubScope = sender as TECSubScope;
                var characteristicPoint = value as TECPoint;
                if (TypicalInstanceDictionary.ContainsKey(characteristicSubScope))
                {
                    foreach (TECSubScope subScope in TypicalInstanceDictionary.GetInstances(characteristicSubScope))
                    {
                        var pointToAdd = new TECPoint(characteristicPoint, false);
                        _typicalInstanceDictionary.AddItem(characteristicPoint, pointToAdd);
                        subScope.Points.Add(pointToAdd);
                    }
                }
            }
            else if (value is TECController && sender is TECPanel)
            {
                var characteristicController = value as TECController;
                var characteristicPanel = sender as TECPanel;
                if (TypicalInstanceDictionary.ContainsKey(characteristicPanel) && TypicalInstanceDictionary.ContainsKey(characteristicController))
                {
                    foreach (TECSystem system in Instances)
                    {
                        TECController controllerToConnect = null;
                        foreach (TECController controller in TypicalInstanceDictionary.GetInstances(characteristicController))
                        {
                            if (system.Controllers.Contains(controller))
                            {
                                controllerToConnect = controller;
                                break;
                            }
                        }
                        if (controllerToConnect != null)
                        {
                            foreach (TECPanel panel in TypicalInstanceDictionary.GetInstances(characteristicPanel))
                            {
                                if (system.Panels.Contains(panel))
                                {
                                    panel.Controllers.Add(controllerToConnect);
                                }
                            }
                        }

                    }
                }
            }
            else if (value is TECIOModule && sender is TECController)
            {
                var characteristicController = sender as TECController;
                if (TypicalInstanceDictionary.ContainsKey(characteristicController))
                {
                    foreach (TECController instance in TypicalInstanceDictionary.GetInstances(characteristicController))
                    {
                        instance.IOModules.Add(value as TECIOModule);
                    }
                }
            }
            else if (value is TECAssociatedCost assCost && sender is TECScope)
            {
                if (sender is TECTypical)
                {
                    foreach (TECSystem system in Instances)
                    {
                        system.AssociatedCosts.Add(assCost);
                    }
                }
                else
                {
                    var characteristicScope = sender as TECScope;
                    if (TypicalInstanceDictionary.ContainsKey(characteristicScope))
                    {
                        foreach (TECScope scope in TypicalInstanceDictionary.GetInstances(characteristicScope))
                        {
                            scope.AssociatedCosts.Add(assCost);
                        }
                    }
                }
            }
        }
        private void handleRemove(ITECObject value, ITECObject sender)
        {
            if (value is TECController && sender is TECTypical)
            {
                var characteristicController = value as TECController;
                foreach (TECSystem system in Instances)
                {
                    var controllersToRemove = new List<TECController>();
                    foreach (TECController controller in system.Controllers)
                    {
                        if (TypicalInstanceDictionary.GetInstances(characteristicController).Contains(controller))
                        {
                            controllersToRemove.Add(controller);
                            _typicalInstanceDictionary.RemoveItem(characteristicController, controller);
                        }
                    }
                    foreach (TECController controller in controllersToRemove)
                    {
                        system.RemoveController(controller);
                    }
                }
            }
            else if (value is TECPanel && sender is TECTypical)
            {
                var characteristicPanel = value as TECPanel;
                foreach (TECSystem system in Instances)
                {
                    var panelsToRemove = new List<TECPanel>();
                    foreach (TECPanel panel in system.Panels)
                    {
                        if (TypicalInstanceDictionary.GetInstances(characteristicPanel).Contains(panel))
                        {
                            panelsToRemove.Add(panel);
                            _typicalInstanceDictionary.RemoveItem(characteristicPanel, panel);
                        }
                    }
                    foreach (TECPanel panel in panelsToRemove)
                    {
                        system.Panels.Remove(panel);
                    }
                }
            }
            else if (value is TECEquipment && sender is TECTypical)
            {
                var characteristicEquipment = value as TECEquipment;
                foreach (TECSystem system in Instances)
                {
                    var equipmentToRemove = new List<TECEquipment>();
                    foreach (TECEquipment equipment in system.Equipment)
                    {
                        if (TypicalInstanceDictionary.GetInstances(characteristicEquipment).Contains(equipment))
                        {
                            equipmentToRemove.Add(equipment);
                            _typicalInstanceDictionary.RemoveItem(characteristicEquipment, equipment);
                        }
                    }
                    foreach (TECEquipment equipment in equipmentToRemove)
                    {
                        system.Equipment.Remove(equipment);
                    }
                }
            }
            else if (value is TECMisc misc && sender is TECTypical)
            {
                foreach (TECSystem system in Instances)
                {
                    var miscToRemove = new List<TECMisc>();
                    foreach (TECMisc instanceMisc in system.MiscCosts)
                    {
                        if (TypicalInstanceDictionary.GetInstances(misc).Contains(instanceMisc))
                        {
                            miscToRemove.Add(instanceMisc);
                            _typicalInstanceDictionary.RemoveItem(misc, instanceMisc);
                        }
                    }
                    foreach (TECMisc instanceMisc in miscToRemove)
                    {
                        system.MiscCosts.Remove(instanceMisc);
                    }
                }
            }
            else if (value is TECScopeBranch branch && sender is TECTypical)
            {
                foreach (TECSystem system in Instances)
                {
                    var branchesToRemove = new List<TECScopeBranch>();
                    foreach (TECScopeBranch instanceBranch in system.ScopeBranches)
                    {
                        if (TypicalInstanceDictionary.GetInstances(branch).Contains(instanceBranch))
                        {
                            branchesToRemove.Add(instanceBranch);
                            _typicalInstanceDictionary.RemoveItem(branch, instanceBranch);
                        }
                    }
                    foreach (TECScopeBranch instanceBranch in branchesToRemove)
                    {
                        system.ScopeBranches.Remove(instanceBranch);
                    }
                }
            }
            else if (value is TECSubScope && sender is TECEquipment)
            {
                var characteristicEquipment = sender as TECEquipment;
                var characteristicSubScope = value as TECSubScope;
                if (TypicalInstanceDictionary.ContainsKey(characteristicEquipment))
                {
                    foreach (TECEquipment equipment in TypicalInstanceDictionary.GetInstances(characteristicEquipment))
                    {
                        var subScopeToRemove = new List<TECSubScope>();
                        foreach (TECSubScope subScope in equipment.SubScope)
                        {
                            if (TypicalInstanceDictionary.GetInstances(characteristicSubScope).Contains(subScope))
                            {
                                subScopeToRemove.Add(subScope);
                                _typicalInstanceDictionary.RemoveItem(characteristicSubScope, subScope);
                            }
                        }
                        foreach (TECSubScope subScope in subScopeToRemove)
                        {
                            equipment.SubScope.Remove(subScope);
                        }
                    }
                }
            }
            else if (value is IEndDevice && sender is TECSubScope)
            {
                var characteristicSubScope = sender as TECSubScope;
                var device = value as IEndDevice;
                if (TypicalInstanceDictionary.ContainsKey(characteristicSubScope))
                {
                    foreach (TECSubScope subScope in TypicalInstanceDictionary.GetInstances(characteristicSubScope))
                    {
                        subScope.Devices.Remove(device);
                    }
                }
            }
            else if (value is TECPoint && sender is TECSubScope)
            {
                var characteristicSubScope = sender as TECSubScope;
                var characteristicPoint = value as TECPoint;
                if (TypicalInstanceDictionary.ContainsKey(characteristicSubScope))
                {
                    foreach (TECSubScope subScope in TypicalInstanceDictionary.GetInstances(characteristicSubScope))
                    {
                        var pointsToRemove = new List<TECPoint>();
                        foreach (TECPoint point in subScope.Points)
                        {
                            if (TypicalInstanceDictionary.GetInstances(characteristicPoint).Contains(point))
                            {
                                pointsToRemove.Add(point);
                                _typicalInstanceDictionary.RemoveItem(characteristicPoint, point);
                            }
                        }
                        foreach (TECPoint point in pointsToRemove)
                        {
                            subScope.Points.Remove(point);
                        }
                    }
                }
            }
            else if (value is TECController && sender is TECPanel)
            {
                var characteristicController = value as TECController;
                var characteristicPanel = sender as TECPanel;
                if (TypicalInstanceDictionary.ContainsKey(characteristicController) && TypicalInstanceDictionary.ContainsKey(characteristicPanel))
                {
                    foreach (TECSystem system in Instances)
                    {
                        TECController controllerToRemove = null;
                        foreach (TECController controller in TypicalInstanceDictionary.GetInstances(characteristicController))
                        {
                            foreach (TECPanel panel in system.Panels)
                            {
                                if (panel.Controllers.Contains(controller))
                                {
                                    controllerToRemove = controller;
                                    break;
                                }
                            }
                        }
                        if (controllerToRemove != null)
                        {
                            foreach (TECPanel panel in TypicalInstanceDictionary.GetInstances(characteristicPanel))
                            {
                                if (system.Panels.Contains(panel))
                                {
                                    panel.Controllers.Remove(controllerToRemove);
                                    break;
                                }
                            }
                        }

                    }
                }
            }
            else if (value is TECIOModule && sender is TECController)
            {
                var characteristicController = sender as TECController;
                if (TypicalInstanceDictionary.ContainsKey(characteristicController))
                {
                    foreach (TECController instance in TypicalInstanceDictionary.GetInstances(characteristicController))
                    {
                        instance.IOModules.Remove(value as TECIOModule);
                    }
                }
            }
            else if (value is TECAssociatedCost cost && sender is TECScope)
            {
                if (sender is TECTypical)
                {
                    foreach (TECSystem system in Instances)
                    {
                        system.AssociatedCosts.Remove(cost);
                    }
                }
                else
                {
                    var characteristicScope = sender as TECScope;
                    if (TypicalInstanceDictionary.ContainsKey(characteristicScope))
                    {
                        foreach (TECScope scope in TypicalInstanceDictionary.GetInstances(characteristicScope))
                        {
                            scope.AssociatedCosts.Remove(cost);
                        }
                    }
                }
            }
        }
        
        #endregion
        #endregion
    }
}
