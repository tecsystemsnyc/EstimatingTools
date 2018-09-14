using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingLibrary.Utilities.WatcherFilters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EstimatingLibrary
{
    public class TECTypical : TECSystem
    {
        #region Fields
        private TypicalWatcherFilter watcher;
        private ObservableListDictionary<IControllerConnection> connectionInstances = new ObservableListDictionary<IControllerConnection>();
        #endregion

        #region Properties
        public ObservableCollection<TECSystem> Instances { get; } = new ObservableCollection<TECSystem>();
        public ObservableListDictionary<ITECObject> TypicalInstanceDictionary { get; } = new ObservableListDictionary<ITECObject>();

        public bool IsSingleton
        {
            get { return Instances.Count == 1; }
        }
        public override bool IsTypical => true;
        #endregion

        #region Constructors
        public TECTypical(Guid guid) : base(guid)
        {
            Instances.CollectionChanged += (sender, args) => handleCollectionChanged(sender, args, "Instances");
            TypicalInstanceDictionary.CollectionChanged += typicalInstanceDictionary_CollectionChanged;

            watcher = new TypicalWatcherFilter(new ChangeWatcher(this));
            watcher.TypicalChanged += handleThisChanged;
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
            copyPropertiesFromLocated(source);
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
            foreach (TECProposalItem item in system.ProposalItems)
            {
                var toAdd = new TECProposalItem(item, guidDictionary);
                ProposalItems.Add(toAdd);
            }
            copyPropertiesFromLocated(system);
            ModelLinkingHelper.LinkSystem(this, guidDictionary);
        }
        #endregion

        #region Methods
        public TECSystem AddInstance()
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            var newSystem = new TECSystem();
            newSystem.CopyPropertiesFromScope(this);
            if (this.Location != null) newSystem.Location = this.Location;
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
            //Proposal items are not currently synced to instances
            //foreach(TECProposalItem propItem in ProposalItems)
            //{
            //    var toAdd = new TECProposalItem(propItem);
            //    TypicalInstanceDictionary.AddItem(propItem, toAdd);
            //    newSystem.ProposalItems.Add(toAdd);
            //}
            ModelLinkingHelper.LinkSystem(newSystem, guidDictionary);

            Instances.Add(newSystem);
            return (newSystem);
        }
        public void UpdateInstanceConnections()
        {
            foreach (TECSystem system in Instances)
            {
                var systemConnectables = system.GetAll<IConnectable>();

                foreach (TECController controller in Controllers)
                {
                    var instanceController = TypicalInstanceDictionary.GetInstances(controller).First(x => system.Controllers.Contains(x));
                    instanceController.RemoveAllChildConnections();
                    foreach (IControllerConnection connection in controller.ChildrenConnections)
                    {
                        List<IControllerConnection> instanceConnections = new List<IControllerConnection>();
                        if (connection is TECNetworkConnection netConnection)
                        {
                            TECNetworkConnection netInstanceConnection = instanceController.AddNetworkConnection(netConnection.NetworkProtocol);
                            var instanceChildren = netConnection.Children.SelectMany(x => TypicalInstanceDictionary.GetInstances(x));
                                
                            foreach (IConnectable instanceChild in instanceChildren)
                            {
                                if (systemConnectables.Contains(instanceChild))
                                {
                                    netInstanceConnection.AddChild(instanceChild);
                                }
                            }
                            instanceConnections.Add(netInstanceConnection);
                        }
                        else if (connection is TECHardwiredConnection hardwired)
                        {
                            var instanceChildren = TypicalInstanceDictionary.GetInstances(hardwired.Child);
                            foreach (IConnectable instanceChild in instanceChildren)
                            {
                                if (systemConnectables.Contains(instanceChild))
                                {
                                    instanceConnections.Add(instanceController.Connect(instanceChild, connection.Protocol));
                                }
                            }

                        }
                        instanceConnections.Where(x => x != null).ForEach(x => x.UpdatePropertiesBasedOn(connection));
                    }
                    
                }
            }
        }
        
        public bool CanUpdateInstanceConnections()
        {
            bool canExecute = Instances.Count > 0;

            return canExecute;
        }

        public List<T> GetInstancesFromTypical<T>(T typical) where T : ITECObject
        {
            return TypicalInstanceDictionary.GetInstances(typical);
        }

        internal void RefreshRegistration()
        {
            watcher.TypicalChanged -= handleThisChanged;
            watcher = new TypicalWatcherFilter(new ChangeWatcher(this));
            watcher.TypicalChanged += handleThisChanged;
        }

        public override object DropData()
        {
            Dictionary<Guid, Guid> guidDictionary = new Dictionary<Guid, Guid>();
            TECTypical outSystem = new TECTypical(this, guidDictionary);
            ModelLinkingHelper.LinkSystem(outSystem, guidDictionary);
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
        protected override RelatableMap propertyObjects()
        {
            RelatableMap saveList = new RelatableMap();
            saveList.AddRange(base.propertyObjects());
            saveList.AddRange(Instances, "Instances");
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
            foreach (TECSystem instance in Instances)
            {
                pointNum += instance.PointNumber;
            }
            return pointNum;
        }

        private void buildConnectionDictionary(TECSystem system, ObservableListDictionary<IControllerConnection> currentDictionary)
        {
            foreach(TECController controller in Controllers)
            {
                var controllerInstances = TypicalInstanceDictionary.GetInstances(controller);
                foreach(IControllerConnection connection in controller.ChildrenConnections)
                {
                    List<IConnectable> instanceConnectables = system.GetAll<IConnectable>();
                    if (connection is TECNetworkConnection netConnect)
                    {
                        var instanceConnection = controllerInstances.Where(x => system.Controllers.Contains(x))
                            .SelectMany(x => x.ChildrenConnections)
                            .OfType<TECNetworkConnection>()
                            .Where(x => x.Children.SequenceEqual(netConnect.Children.SelectMany(y => TypicalInstanceDictionary.GetInstances(y).Where(z => instanceConnectables.Contains(z)))))
                            .FirstOrDefault();
                        currentDictionary.AddItem(connection, instanceConnection);
                    }
                    else if (connection is TECHardwiredConnection hardConnect)
                    {
                        var instanceConnection = controllerInstances.Where(x => system.Controllers.Contains(x))
                            .SelectMany(x => x.ChildrenConnections)
                            .OfType<TECHardwiredConnection>()
                            .Where(x => x.Child == TypicalInstanceDictionary.GetInstances(hardConnect.Child).Where(z => instanceConnectables.Contains(z)).FirstOrDefault())
                            .FirstOrDefault();
                        currentDictionary.AddItem(connection, instanceConnection);
                    }
                }
            }
        }

        #region Event Handlers
        private void handleThisChanged(TECChangedEventArgs args)
        {
            if (Instances.Count > 0 && args.Value?.GetType() != typeof(TECSystem))
            {
                if (args.Change == Change.Add)
                {
                    handleAdd(args);
                    return;
                }
                else if (args.Change == Change.Remove)
                {
                    handleRemove(args);
                    return;
                }
                else if (args.Sender is TECController controller)
                {
                    handleControllerChanged(controller, args.PropertyName);
                }
                else if (args.Sender is TECPoint || args.Sender is TECMisc)
                {
                    handleValueChanged(args.Sender, args.PropertyName);
                    return;
                }

                if (IsSingleton)
                {
                    handleValueChanged(args.Sender, args.PropertyName);
                }
            }
        }
        protected override void handleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (propertyName == "Instances")
            {
                CollectionChangedHandlers.CollectionChangedHandler(sender, e, propertyName, this,
                notifyCombinedChanged, invokeCostChanged, invokePointChanged, onAdd: instanceAdded, onRemove: instanceRemoved, setTypical: false);
            }
            else
            {
                base.handleCollectionChanged(sender, e, propertyName);
            }

            void instanceRemoved(object item)
            {
                if (item is TECSystem instance)
                {
                    handleInstanceRemoved(instance);
                }
            }
            void instanceAdded(object item)
            {
                if (item is TECSystem instance)
                {
                    buildConnectionDictionary(instance, connectionInstances);
                }
            }
        }

        //Overridden so that TECTypical doesn't raise cost changed when an associated cost is added or removed.
        protected override void scopeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, string propertyName)
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
            connectionInstances.RemoveValuesForKeys(instance.Controllers.SelectMany(x => x.ChildrenConnections), this.Controllers.SelectMany(x => x.ChildrenConnections));
            instance.Controllers.ForEach(x => x.DisconnectAll());
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.Panels, Panels);
            TypicalInstanceDictionary.RemoveValuesForKeys(instance.Equipment, Equipment);

            var typicalSubScope = GetAllSubScope();
            foreach (TECEquipment instanceEquip in instance.Equipment)
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
        private void handleValueChanged<T>(T item, string propertyName) where T : ITECObject
        {
            if(item is IControllerConnection connection)
            {
                executeValueChanged(connection, propertyName, connectionInstances);
            }
            else
            {
                executeValueChanged(item, propertyName, TypicalInstanceDictionary);
            }
        }
        private void executeValueChanged<T>(T item, string propertyName, ObservableListDictionary<T> dict) where T : ITECObject
        {
            PropertyInfo property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                if (dict.ContainsKey(item))
                {
                    foreach (T instance in dict.GetInstances(item))
                    {
                        property.SetValue(instance, property.GetValue(item), null);
                    }
                }
                else if (item as TECTypical == this)
                {
                    foreach (var instance in Instances)
                    {
                        property.SetValue(instance, property.GetValue(item), null);
                    }
                }
            }
        }
        private void handleControllerChanged(TECController controller, string propertyName)
        {
            if (propertyName == "Type" && controller is TECProvidedController provided)
            {
                foreach (var instance in GetInstancesFromTypical(provided))
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
            if (args.Sender is TECTypical typ) { parentInstances.AddRange(Instances); }
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
                    if(sender is IRelatable relSender && relSender.IsDirectChildProperty(args.PropertyName))
                    {
                        instanceValue = typicalChild.CreateInstance(TypicalInstanceDictionary);
                        if (instanceValue != null)
                        {
                            TypicalInstanceDictionary.AddItem(args.Value as ITECObject, instanceValue);
                        }
                    }
                    else
                    {
                        var parentSystem = this.Instances
                            .Where(x => x.IsDirectDescendant(parentInstance))
                            .FirstOrDefault();

                        instanceValue = this.TypicalInstanceDictionary.GetInstances(typicalChild)
                            .Where(x => parentSystem.IsDirectDescendant(x)).FirstOrDefault();

                    }
                }
                if (instanceValue != null)
                {
                    instanceSender.AddChildForProperty(args.PropertyName, instanceValue);
                }
            }

            if (IsSingleton)
            {
                if (args.Value is IControllerConnection connection && args.Sender is TECController controller)
                {
                    var instanceController = this.GetInstancesFromTypical(controller).First();
                    if (connection is TECHardwiredConnection hardConnect)
                    {
                        var instanceSubScope = this.GetInstancesFromTypical(hardConnect.Child).First();
                        var instanceConnection = instanceController.Connect(instanceSubScope, instanceSubScope.HardwiredProtocol());
                        if(instanceConnection == null && instanceController is TECProvidedController providedInstanceController)
                        {
                            providedInstanceController.OptimizeModules();
                            instanceConnection = instanceController.Connect(instanceSubScope, instanceSubScope.HardwiredProtocol());
                        }
                        if(instanceConnection != null)
                        {
                            instanceConnection.UpdatePropertiesBasedOn(connection);
                            connectionInstances.AddItem(connection, instanceConnection);
                        }
                        else
                        {
                            UpdateInstanceConnections();
                        }
                    }
                    else if (connection is TECNetworkConnection netConnect)
                    {
                        var instanceSubScope = netConnect.Children.SelectMany(x => this.GetInstancesFromTypical(x));
                        var instanceConnection = instanceController.AddNetworkConnection(netConnect.NetworkProtocol);
                        if (instanceConnection == null && instanceController is TECProvidedController providedInstanceController)
                        {
                            providedInstanceController.OptimizeModules();
                            instanceConnection = instanceController.AddNetworkConnection(netConnect.NetworkProtocol);

                        }
                        if (instanceConnection != null)
                        {
                            instanceSubScope.ForEach(x => instanceConnection.AddChild(x));
                            instanceConnection.UpdatePropertiesBasedOn(connection);
                            connectionInstances.AddItem(connection, instanceConnection);
                        }
                        else
                        {
                            UpdateInstanceConnections();
                        }
                    }

                }
                else if (args.Value is IConnectable connectable && args.Sender is TECNetworkConnection netConnect)
                {
                    var instanceConnection = connectionInstances.GetInstances(netConnect).First();
                    var instanceConnectable = TypicalInstanceDictionary.GetInstances(connectable).First();
                    if(instanceConnection == null || instanceConnectable == null)
                    {
                        UpdateInstanceConnections();
                    }
                    else
                    {
                        instanceConnection.AddChild(instanceConnectable);
                    }
                }
            }
        }
        private void handleRemove(TECChangedEventArgs args)
        {
            ITypicalable sender = args.Sender as ITypicalable;
            List<ITECObject> parentInstances = new List<ITECObject>();
            if (args.Sender is TECTypical typ) { parentInstances.AddRange(Instances); }
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
                if (args.Value is ITypicalable typicalChild)
                {
                    if (sender is IRelatable relSender && relSender.IsDirectChildProperty(args.PropertyName))
                    {
                        instanceValue = TypicalInstanceDictionary.GetInstances(instanceValue)
                        .Where(x => instanceSender.ContainsChildForProperty(args.PropertyName, x)).FirstOrDefault();
                        if (instanceValue != null)
                        {
                            TypicalInstanceDictionary.RemoveItem(args.Value as ITECObject, instanceValue);
                        }
                    }
                    else
                    {
                        var parentSystem = this.Instances
                            .Where(x => x.IsDirectDescendant(parentInstance))
                            .FirstOrDefault();

                        instanceValue = this.TypicalInstanceDictionary.GetInstances(typicalChild)
                            .Where(x => parentSystem.IsDirectDescendant(x)).FirstOrDefault();

                    }

                }

                if (instanceValue != null)
                {
                    instanceSender.RemoveChildForProperty(args.PropertyName, instanceValue);
                }

            }

            if (IsSingleton)
            {
                if (args.Value is IControllerConnection connection && args.Sender is TECController controller)
                {
                    var instanceConnection = connectionInstances.GetInstances(connection).First();
                    if (instanceConnection is TECHardwiredConnection hardConn)
                    {
                        instanceConnection.ParentController.Disconnect(hardConn.Child);
                    }
                    else if (instanceConnection is TECNetworkConnection netConn)
                    {
                        instanceConnection.ParentController.RemoveNetworkConnection(netConn);
                    }
                }
                else if (args.Value is IConnectable connectable && args.Sender is TECNetworkConnection netConnect)
                {
                    var instanceConnection = connectionInstances.GetInstances(netConnect).First();
                    var instanceConnectable = TypicalInstanceDictionary.GetInstances(connectable).First();
                    if (instanceConnection == null || instanceConnectable == null)
                    {
                        UpdateInstanceConnections();
                    }
                    else
                    {
                        instanceConnection.RemoveChild(instanceConnectable);
                    }
                }
            }
        }
        
        #endregion
        #endregion

    }
}
