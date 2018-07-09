﻿using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECTemplates : TECScopeManager, IRelatable
    {
        #region Properties

        private ObservableCollection<TECSystem> _systemTemplates = new ObservableCollection<TECSystem>();
        private ObservableCollection<TECEquipment> _equipmentTemplates = new ObservableCollection<TECEquipment>();
        private ObservableCollection<TECSubScope> _subScopeTemplates = new ObservableCollection<TECSubScope>();
        private ObservableCollection<TECController> _controllerTemplates = new ObservableCollection<TECController>();
        private ObservableCollection<TECMisc> _miscCostTemplates = new ObservableCollection<TECMisc>();
        private ObservableCollection<TECPanel> _panelTemplates = new ObservableCollection<TECPanel>();
        private ObservableCollection<TECParameters> _parameters = new ObservableCollection<TECParameters>();

        public ObservableCollection<TECSystem> SystemTemplates
        {
            get { return _systemTemplates; }
            set
            {
                var old = SystemTemplates;
                SystemTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "SystemTemplates");
                _systemTemplates = value;
                SystemTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "SystemTemplates");
                notifyCombinedChanged(Change.Edit, "SystemTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECEquipment> EquipmentTemplates
        {
            get { return _equipmentTemplates; }
            set
            {
                var old = EquipmentTemplates;
                EquipmentTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "EquipmentTemplates");
                _equipmentTemplates = value;
                EquipmentTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "EquipmentTemplates");
                notifyCombinedChanged(Change.Edit, "EquipmentTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECSubScope> SubScopeTemplates
        {
            get { return _subScopeTemplates; }
            set
            {
                var old = SubScopeTemplates;
                SubScopeTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "SubScopeTemplates");
                _subScopeTemplates = value;
                SubScopeTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "SubScopeTemplates");
                notifyCombinedChanged(Change.Edit, "SubScopeTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECController> ControllerTemplates
        {
            get { return _controllerTemplates; }
            set
            {
                var old = ControllerTemplates;
                ControllerTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "ControllerTemplates");
                _controllerTemplates = value;
                ControllerTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "ControllerTemplates");
                notifyCombinedChanged(Change.Edit, "ControllerTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECMisc> MiscCostTemplates
        {
            get { return _miscCostTemplates; }
            set
            {
                var old = MiscCostTemplates;
                MiscCostTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "MiscCostTemplates");
                _miscCostTemplates = value;
                MiscCostTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "MiscCostTemplates");
                notifyCombinedChanged(Change.Edit, "MiscCostTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECPanel> PanelTemplates
        {
            get { return _panelTemplates; }
            set
            {
                var old = PanelTemplates;
                PanelTemplates.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "PanelTemplates");
                _panelTemplates = value;
                PanelTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "PanelTemplates");
                notifyCombinedChanged(Change.Edit, "PanelTemplates", this, value, old);
            }
        }
        public ObservableCollection<TECParameters> Parameters
        {
            get { return _parameters; }
            set
            {
                var old = Parameters;
                Parameters.CollectionChanged -= (sender, args) => CollectionChanged(sender, args, "Parameters");
                _parameters = value;
                Parameters.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "Parameters");
                notifyCombinedChanged(Change.Edit, "Parameters", this, value, old);
            }
        }

        public TemplateSynchronizer<TECSubScope> SubScopeSynchronizer;
        public TemplateSynchronizer<TECEquipment> EquipmentSynchronizer;
        #endregion //Properties

        //For listening to a catalog changing
        public override TECCatalogs Catalogs
        {
            get
            {
                return base.Catalogs;
            }

            set
            {
                base.Catalogs.ScopeChildRemoved -= scopeChildRemoved;
                base.Catalogs = value;
                base.Catalogs.ScopeChildRemoved += scopeChildRemoved;
            }
        }

        public SaveableMap PropertyObjects { get { return propertyObjects(); } }
        public SaveableMap LinkedObjects { get { return new SaveableMap(); } }

        #region Constructors
        public TECTemplates() : this(Guid.NewGuid()) { }
        public TECTemplates(Guid guid) : base(guid)
        {
            SystemTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "SystemTemplates");
            EquipmentTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "EquipmentTemplates");
            SubScopeTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "SubScopeTemplates");
            ControllerTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "ControllerTemplates");
            MiscCostTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "MiscCostTemplates");
            PanelTemplates.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "PanelTemplates");
            Parameters.CollectionChanged += (sender, args) => CollectionChanged(sender, args, "Parameters");

            Catalogs.ScopeChildRemoved += scopeChildRemoved;
            
            SubScopeSynchronizer = new TemplateSynchronizer<TECSubScope>((item => 
            {
                return new TECSubScope(item);
            }), (item => { }),
            syncSubScope, this);
            SubScopeSynchronizer.TECChanged += synchronizerChanged;

            EquipmentSynchronizer = new TemplateSynchronizer<TECEquipment>(
                //Copy
                (item => {
                    TECEquipment newItem = new TECEquipment();
                    newItem.CopyPropertiesFromScope(item);
                    foreach(TECSubScope subScope in item.SubScope)
                    {
                        newItem.SubScope.Add(SubScopeSynchronizer.NewItem(subScope));
                    }
                    return newItem;

                }),
                //Remove
                (item =>
                {
                    foreach(TECSubScope subScope in item.SubScope)
                    {
                        if (SubScopeSynchronizer.GetTemplate(subScope) != null)
                        {
                            SubScopeSynchronizer.RemoveItem(subScope);
                        }
                    }
                }),
                //Sync
                syncEquipment, this);
            EquipmentSynchronizer.TECChanged += synchronizerChanged;
            
        }
        #endregion //Constructors

        private void scopeChildRemoved(TECObject child)
        {
            foreach (TECElectricalMaterial type in Catalogs.ConnectionTypes)
            {
                removeChildFromScope(type, child);
                if (child is TECAssociatedCost cost)
                {
                    type.RatedCosts.Remove(cost);
                }
            }
            foreach (TECElectricalMaterial type in Catalogs.ConduitTypes)
            {
                removeChildFromScope(type, child);
                if (child is TECAssociatedCost cost)
                {
                    type.RatedCosts.Remove(cost);
                }
            }
            foreach (TECPanelType type in Catalogs.PanelTypes)
            {
                removeChildFromScope(type, child);
            }
            foreach (TECControllerType type in Catalogs.ControllerTypes)
            {
                removeChildFromScope(type, child);
            }
            foreach (TECIOModule module in Catalogs.IOModules)
            {
                removeChildFromScope(module, child);
            }
            foreach (TECDevice dev in Catalogs.Devices)
            {
                removeChildFromScope(dev, child);
            }
            foreach (TECSystem sys in SystemTemplates)
            {
                removeChildFromScope(sys, child);
                foreach(TECEquipment equip in sys.Equipment)
                {
                    removeChildFromScope(equip, child);
                    foreach(TECSubScope ss in equip.SubScope)
                    {
                        removeChildFromScope(ss, child);
                    }
                }
            }
            foreach(TECEquipment equip in EquipmentTemplates)
            {
                removeChildFromScope(equip, child);
                foreach (TECSubScope ss in equip.SubScope)
                {
                    removeChildFromScope(ss, child);
                }
            }
            foreach(TECSubScope ss in SubScopeTemplates)
            {
                removeChildFromScope(ss, child);
            }
            foreach(TECController controller in ControllerTemplates)
            {
                removeChildFromScope(controller, child);
            }
            foreach(TECPanel panel in PanelTemplates)
            {
                removeChildFromScope(panel, child);
            }
        }

        private void removeChildFromScope(TECScope scope, TECObject child)
        {
            if (child is TECAssociatedCost cost)
            {
                scope.AssociatedCosts.Remove(cost);
            }
            else if (child is TECTag tag)
            {
                scope.Tags.Remove(tag);
            }
            else
            {
                throw new NotImplementedException("Scope child isn't cost or tag.");
            }
        }
        private SaveableMap propertyObjects()
        {
            SaveableMap saveList = new SaveableMap();
            saveList.Add(this.Catalogs, "Catalogs");
            saveList.AddRange(this.SystemTemplates, "SystemTemplates");
            saveList.AddRange(this.EquipmentTemplates, "EquipmentTemplates");
            saveList.AddRange(this.SubScopeTemplates, "SubScopeTemplates");
            saveList.AddRange(this.ControllerTemplates, "ControllerTemplates");
            saveList.AddRange(this.MiscCostTemplates, "MiscCostTemplates");
            saveList.AddRange(this.PanelTemplates, "PanelTemplates");
            saveList.AddRange(this.Parameters, "Parameters");
            return saveList;
        }

        #region Collection Changed
        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, string propertyName)
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
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                notifyCombinedChanged(Change.Edit, propertyName, this, sender, sender);
            }
        }
        #endregion

        public bool IsTemplateObject(TECObject item)
        {
            if(item is TECSubScope subScope)
            {
                bool isTemplate = SubScopeTemplates.Contains(subScope);
                
                bool isTemplated = SubScopeTemplates.Contains(SubScopeSynchronizer.GetParent(subScope)) ||
                    SubScopeTemplates.Contains(SubScopeSynchronizer.GetParent(SubScopeSynchronizer.GetParent(subScope)));
                return isTemplate || isTemplated;
            }
            else if (item is TECEquipment equipment)
            {
                return EquipmentTemplates.Contains(equipment) || EquipmentSynchronizer.Contains(equipment);
            }
            else if (item is TECController controller)
            {
                return ControllerTemplates.Contains(controller);
            }
            else if (item is TECPanel panel)
            {
                return PanelTemplates.Contains(panel);
            }
            else if (item is TECMisc misc)
            {
                return MiscCostTemplates.Contains(misc);
            }
            else
            {
                return false;
            }
        }

        private void syncSubScope(TemplateSynchronizer<TECSubScope> synchronizer, TECSubScope template, TECSubScope changed, TECChangedEventArgs args)
        {
            if (changed != template)
            {
                syncItem(changed, template);
            }
            foreach (TECSubScope item in synchronizer.GetFullDictionary()[template].Where(item => item != changed))
            {
                syncItem(changed, item);
            }

            void syncItem(TECSubScope newItem, TECSubScope subject)
            {
                subject.CopyChildrenFromScope(newItem);
                
                subject.Points.ObservablyClear();
                subject.Devices.ObservablyClear();
                foreach (TECPoint point in newItem.Points)
                {
                    subject.Points.Add(new TECPoint(point));
                }
                foreach (IEndDevice device in newItem.Devices)
                {
                    subject.Devices.Add(device);
                }
            }
        }
        private void syncEquipment(TemplateSynchronizer<TECEquipment> synchronizer, TECEquipment template, TECEquipment changed, TECChangedEventArgs args)
        {
            if (!(args.Sender is TECEquipment))
            {
                return;
            }
            TECEquipment item = args.Sender as TECEquipment;
            TECSubScope value = args.Value as TECSubScope;
            List<TECEquipment> references = synchronizer.GetFullDictionary()[template];
            if (value != null && args.Change == Change.Add)
            {
                TECSubScope newTemplate = value;
                if (item == template)
                {
                    SubScopeSynchronizer.NewGroup(newTemplate);
                }
                else
                {
                    TECSubScope parentTemplate = SubScopeSynchronizer.GetTemplate(newTemplate);
                    if (parentTemplate != null)
                    {
                        SubScopeSynchronizer.RemoveItem(newTemplate);
                        newTemplate = SubScopeSynchronizer.NewItem(parentTemplate);
                    }
                    else
                    {
                        newTemplate = new TECSubScope(value);
                    }
                    template.SubScope.Add(newTemplate);
                    SubScopeSynchronizer.NewGroup(newTemplate);
                    SubScopeSynchronizer.LinkExisting(newTemplate, value);
                }
                foreach (TECEquipment reference in references.Where(obj=> obj != item))
                {
                    reference.SubScope.Add(SubScopeSynchronizer.NewItem(newTemplate));
                }
            }
            else if (value != null && args.Change == Change.Remove)
            {
                TECSubScope subScopeTemplate = value;
                if (item != template)
                {
                    subScopeTemplate = SubScopeSynchronizer.GetTemplate(value);
                }
                template.SubScope.Remove(subScopeTemplate);
                List<TECSubScope> subScopeReferences = SubScopeSynchronizer.GetFullDictionary()[subScopeTemplate];
                foreach (TECEquipment reference in references)
                {
                    List<TECSubScope> toRemove = new List<TECSubScope>();
                    foreach (TECSubScope referenceSubScope in reference.SubScope)
                    {
                        if (subScopeReferences.Contains(referenceSubScope))
                        {
                            toRemove.Add(referenceSubScope);
                        }
                    }
                    foreach (TECSubScope thing in toRemove)
                    {
                        reference.SubScope.Remove(thing);
                    }
                }
                SubScopeSynchronizer.RemoveGroup(subScopeTemplate);
                if(SubScopeSynchronizer.GetTemplate(subScopeTemplate) != null)
                {
                    SubScopeSynchronizer.RemoveItem(subScopeTemplate);
                }
            }
            else
            {
                if(item != template)
                {
                    template.CopyChildrenFromScope(item);
                }
                foreach (TECEquipment reference in references.Where(obj => obj != item))
                {
                    reference.CopyChildrenFromScope(item);
                }
            }
        }
        private void synchronizerChanged(TECChangedEventArgs obj)
        {
            notifyTECChanged(obj.Change, obj.PropertyName, obj.Sender, obj.Value);
        }
        
    }
}
