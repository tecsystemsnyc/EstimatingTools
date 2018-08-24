using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EstimatingLibrary
{
    public class TECTemplates : TECScopeManager
    {
        #region Properties
        
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
        
        #region Constructors
        public TECTemplates() : this(Guid.NewGuid()) { }
        public TECTemplates(Guid guid) : base(guid)
        {
            Catalogs.ScopeChildRemoved += scopeChildRemoved;
            
            SubScopeSynchronizer = new TemplateSynchronizer<TECSubScope>((item => 
            {
                return new TECSubScope(item);
            }), (item => { }),
            syncSubScope, this.Templates);
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
                syncEquipment, this.Templates);
            EquipmentSynchronizer.TECChanged += synchronizerChanged;
            
        }
        #endregion //Constructors

        public bool IsTemplateObject(TECObject item)
        {
            if(item is TECSubScope subScope)
            {
                bool isTemplate = Templates.SubScopeTemplates.Contains(subScope);
                
                bool isTemplated = Templates.SubScopeTemplates.Contains(SubScopeSynchronizer.GetParent(subScope)) ||
                    Templates.SubScopeTemplates.Contains(SubScopeSynchronizer.GetParent(SubScopeSynchronizer.GetParent(subScope)));
                return isTemplate || isTemplated;
            }
            else if (item is TECEquipment equipment)
            {
                return Templates.EquipmentTemplates.Contains(equipment) || EquipmentSynchronizer.Contains(equipment);
            }
            else if (item is TECController controller)
            {
                return Templates.ControllerTemplates.Contains(controller);
            }
            else if (item is TECPanel panel)
            {
                return Templates.PanelTemplates.Contains(panel);
            }
            else if (item is TECMisc misc)
            {
                return Templates.MiscCostTemplates.Contains(misc);
            }
            else if (item is TECParameters parameters)
            {
                return Templates.Parameters.Contains(parameters);
            }
            else if (item is TECSystem system)
            {
                return Templates.SystemTemplates.Contains(system);
            }
            else
            {
                return false;
            }
        }
        public void UpdateSubScopeReferenceProperties(TECSubScope subScope)
        {
            var templateSS = SubScopeSynchronizer.GetTemplate(subScope);
            if(templateSS != subScope) {
                setProperties(subScope, templateSS);
            }
            SubScopeSynchronizer.ActOnReferences(templateSS, updateSubScope);
            void updateSubScope(TemplateSynchronizer<TECSubScope> synchronizer, TECSubScope template)
            {
                foreach (TECSubScope item in synchronizer.GetFullDictionary()[template])
                {
                    setProperties(template, item);
                }
            }
            void setProperties(TECSubScope tSS, TECSubScope rSS)
            {
                rSS.CopyPropertiesFromScope(tSS);
                rSS.ScopeBranches.ObservablyClear();
                foreach (TECScopeBranch branch in tSS.ScopeBranches)
                {
                    rSS.ScopeBranches.Add(new TECScopeBranch(branch));
                }
            }
            
        }
        
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
            foreach (TECSystem sys in Templates.SystemTemplates)
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
            foreach(TECEquipment equip in Templates.EquipmentTemplates)
            {
                removeChildFromScope(equip, child);
                foreach (TECSubScope ss in equip.SubScope)
                {
                    removeChildFromScope(ss, child);
                }
            }
            foreach(TECSubScope ss in Templates.SubScopeTemplates)
            {
                removeChildFromScope(ss, child);
            }
            foreach(TECController controller in Templates.ControllerTemplates)
            {
                removeChildFromScope(controller, child);
            }
            foreach(TECPanel panel in Templates.PanelTemplates)
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

        private void syncSubScope(TemplateSynchronizer<TECSubScope> synchronizer,
            TECSubScope template, TECSubScope changed, TECChangedEventArgs args)
        {
            if (changed != template)
            {
                syncItem(changed, template);
            }
            foreach (TECSubScope item in synchronizer.GetFullDictionary()[template].Where(item => item != changed))
            {
                syncItem(changed, item);
            }

            void syncItem(TECSubScope changedItem, TECSubScope subject)
            {
                subject.CopyChildrenFromScope(changedItem);
                
                subject.Points.ObservablyClear();
                subject.Devices.ObservablyClear();
                subject.Interlocks.ObservablyClear();

                foreach (TECPoint point in changedItem.Points)
                {
                    subject.AddPoint(new TECPoint(point));
                }
                foreach (IEndDevice device in changedItem.Devices)
                {
                    subject.AddDevice(device);
                }
                foreach(TECInterlockConnection connection in changedItem.Interlocks)
                {
                    subject.Interlocks.Add(connection);
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
