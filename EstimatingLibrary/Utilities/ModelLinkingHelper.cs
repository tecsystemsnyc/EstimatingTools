using EstimatingLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace EstimatingLibrary.Utilities
{
    public static class ModelLinkingHelper
    {
        #region Public Methods
        public static bool LinkLoadedBid(TECBid bid, Dictionary<Guid, List<Guid>> guidDictionary)
        {
            bool needsSave = false;

            foreach (TECController controller in bid.GetAll<TECController>())
            {
                if (controller is TECProvidedController provided)
                {
                    bool controllerNeedsSave = ModelCleanser.addRequiredIOModules(provided);
                    if (controllerNeedsSave)
                    {
                        needsSave = true;
                    }
                }
            }

            foreach (TECTypical typical in bid.Systems)
            {
                createScopeDictionary(typical, guidDictionary);
                typical.RefreshRegistration();
            }
            return needsSave;
        }        
        public static bool LinkLoadedTemplates(TECTemplates templates, Dictionary<Guid, List<Guid>> templateReferences)
        {
            bool needsSave = false;
            
            linkTemplateReferences(templates, templateReferences);

            return needsSave;
        }
        public static void LinkSystem(TECSystem system, Dictionary<Guid, Guid> guidDictionary)
        {
            linkSubScopeConnections(system.Controllers, system.GetAllSubScope(), guidDictionary);
            List<IConnectable> allChildren = new List<IConnectable>();
            allChildren.AddRange(system.Controllers);
            allChildren.AddRange(system.GetAllSubScope());
            linkNetworkConnections(system.Controllers, allChildren, guidDictionary);
            linkPanelsToControllers(system.Panels, system.Controllers, guidDictionary);
            linkProposalItmes(system.ProposalItems, system.Equipment, guidDictionary);
            if(system is TECTypical typical)
            {
                typical.RefreshRegistration();
            }
        }
        
        #region Linking Scope
        public static void LinkScopeItem(TECSystem scope, TECBid bid)
        {
            linkScopeChildrenToCatalogs(scope, bid.Catalogs);
            linkLocation(scope, bid.Locations);
            if(scope is TECTypical typical)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    LinkScopeItem(instance, bid);
                }
            }
            foreach (TECEquipment equip in scope.Equipment)
            {
                LinkScopeItem(equip, bid);
            }
        }
        public static void LinkScopeItem(TECEquipment scope, TECBid bid)
        {
            linkScopeChildrenToCatalogs(scope, bid.Catalogs);
            linkLocation(scope, bid.Locations);
            foreach (TECSubScope ss in scope.SubScope)
            {
                LinkScopeItem(ss, bid);
            }
        }
        public static void LinkScopeItem(TECSubScope scope, TECBid bid)
        {
            linkScopeChildrenToCatalogs(scope, bid.Catalogs);
            linkLocation(scope, bid.Locations);
            foreach (TECDevice dev in scope.Devices)
            {
                LinkScopeItem(dev, bid);
            }
        }
        public static void LinkScopeItem(TECScope scope, TECScopeManager manager)
        {
            linkScopeChildrenToCatalogs(scope, manager.Catalogs);
        }
        #endregion

        /// <summary>
        /// Links scope items in a bid to a catalog which is already linked
        /// </summary>
        /// <param name="bid"></param>
        public static void LinkBidToCatalogs(TECBid bid)
        {
            foreach(TECSystem typical in bid.Systems)
            {
                linkSystemToCatalogs(typical, bid.Catalogs);
            }
            foreach(TECController controller in bid.Controllers)
            {
                linkControllerToCatalogs(controller, bid.Catalogs);
            }
            foreach(TECPanel panel in bid.Panels)
            {
                linkPanelToCatalogs(panel, bid.Catalogs);
            }
        }
        
        #endregion

        #region Private Methods
        private static void linkSystemToCatalogs(TECSystem system, TECCatalogs catalogs)
        {
            //Should assume linking a typical system with potential instances, controllers and panels.

            linkScopeChildrenToCatalogs(system, catalogs);
            if(system is TECTypical typical)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    linkSystemToCatalogs(instance, catalogs);
                }
            }
            
            foreach(TECController controller in system.Controllers)
            {
                linkControllerToCatalogs(controller, catalogs);
            }
            foreach(TECPanel panel in system.Panels)
            {
                linkPanelToCatalogs(panel, catalogs);
            }
            foreach(TECEquipment equip in system.Equipment)
            {
                linkEquipmentToCatalogs(equip, catalogs);
            }
        }
        private static void linkEquipmentToCatalogs(TECEquipment equip, TECCatalogs catalogs)
        {
            linkScopeChildrenToCatalogs(equip, catalogs);
            foreach (TECSubScope subScope in equip.SubScope)
            {
                linkSubScopeToCatalogs(subScope, catalogs);
            }
        }
        private static void linkSubScopeToCatalogs(TECSubScope ss, TECCatalogs catalogs)
        {
            linkScopeChildrenToCatalogs(ss, catalogs);
            var devices = new List<IEndDevice>(catalogs.Devices);
            devices.AddRange(catalogs.Valves);
            linkSubScopeToDevices(ss, devices);
        }
        private static void linkControllerToCatalogs(TECController controller, TECCatalogs catalogs)
        {
            if (controller is TECProvidedController provided)
            {
                linkProvidedControllerToControllerType(provided, catalogs.ControllerTypes);
            }
            foreach(IControllerConnection connection in controller.ChildrenConnections)
            {
                linkConnectionToCatalogs(connection, catalogs);
            }
            linkScopeChildrenToCatalogs(controller, catalogs);
        }
        private static void linkConnectionToCatalogs(IControllerConnection connection, TECCatalogs catalogs)
        {
            linkConnectionToConduitType(connection, catalogs.ConduitTypes);
        }
        private static void linkPanelToCatalogs(TECPanel panel, TECCatalogs catalogs)
        {
            linkPanelToPanelType(panel, catalogs.PanelTypes);
            linkScopeChildrenToCatalogs(panel, catalogs);
        }

        private static void linkPanelsToControllers(IEnumerable<TECPanel> panels, IEnumerable<TECController> controllers, Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECPanel panel in panels)
            {
                List<TECController> controllersToLink = new List<TECController>();
                foreach (TECController panelController in panel.Controllers)
                {
                    foreach (TECController controller in controllers)
                    {
                        if (panelController.Guid == controller.Guid)
                        {
                            controllersToLink.Add(controller);
                            break;
                        }
                        else if (guidDictionary != null && guidDictionary[panelController.Guid] == guidDictionary[controller.Guid])
                        {
                            controllersToLink.Add(controller);
                            break;
                        }
                    }
                }
                panel.Controllers = new ObservableCollection<TECController>(controllersToLink);
            }
        }
        private static void linkNetworkConnections(IEnumerable<TECController> controllers, IEnumerable<IConnectable> children,
            Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECController controller in controllers)
            {
                foreach (IControllerConnection connection in controller.ChildrenConnections)
                {
                    if (connection is TECNetworkConnection)
                    {
                        TECNetworkConnection netConnect = connection as TECNetworkConnection;
                        ObservableCollection<IConnectable> controllersToAdd = new ObservableCollection<IConnectable>();
                        foreach (IConnectable child in netConnect.Children)
                        {
                            foreach (IConnectable item in children)
                            {
                                bool isCopy = (guidDictionary != null && guidDictionary[child.Guid] == guidDictionary[item.Guid]);
                                if (child.Guid == item.Guid || isCopy)
                                {
                                    controllersToAdd.Add(item);
                                    item.SetParentConnection(netConnect);
                                }
                            }
                        }
                        netConnect.Children = controllersToAdd;
                    }
                }
            }
        }
        private static void linkSubScopeConnections(IEnumerable<TECController> controllers, IEnumerable<TECSubScope> subscope,
            Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECSubScope subScope in subscope)
            {
                foreach (TECController controller in controllers)
                {
                    List<TECHardwiredConnection> newConnections = new List<TECHardwiredConnection>();
                    List<TECHardwiredConnection> oldConnections = new List<TECHardwiredConnection>();
                    foreach (IControllerConnection connection in controller.ChildrenConnections)
                    {
                        
                        if (connection is TECHardwiredConnection)
                        {
                            TECHardwiredConnection ssConnect = connection as TECHardwiredConnection;
                            bool isCopy = (guidDictionary != null && guidDictionary[ssConnect.Child.Guid] == guidDictionary[subScope.Guid]);
                            if (ssConnect.Child.Guid == subScope.Guid || isCopy)
                            {
                                TECHardwiredConnection linkedConnection = new TECHardwiredConnection(ssConnect, subScope, subScope.IsTypical || controller.IsTypical);
                                newConnections.Add(linkedConnection);
                                oldConnections.Add(ssConnect);
                            }
                        }
                    }
                    foreach(TECHardwiredConnection conn in newConnections)
                    {
                        controller.ChildrenConnections.Add(conn);
                    }
                    foreach(TECHardwiredConnection conn in oldConnections)
                    {
                        controller.ChildrenConnections.Remove(conn);
                    }
                }
            }
        }

        private static void linkConnectionToConduitType(IControllerConnection connection, IEnumerable<TECElectricalMaterial> conduitTypes)
        {
            if (connection.ConduitType != null)
            {
                foreach (TECElectricalMaterial type in conduitTypes)
                {
                    if (connection.ConduitType.Guid == type.Guid)
                    {
                        connection.ConduitType = type;
                        return;
                    }
                }
            }
        }
        private static void linkScopeChildrenToCatalogs(TECScope scope, TECCatalogs catalogs)
        {
            linkAssociatedCostsInScope(catalogs.AssociatedCosts, scope);
            linkTagsInScope(catalogs.Tags, scope);
        }
        private static void linkPanelToPanelType(TECPanel panel, IEnumerable<TECPanelType> panelTypes)
        {
            foreach(TECPanelType type in panelTypes)
            {
                if (panel.Type.Guid == type.Guid)
                {
                    panel.Type = type;
                    return;
                }
            }
        }
        private static void linkProvidedControllerToControllerType(TECProvidedController controller, IEnumerable<TECControllerType> controllerTypes)
        {
            foreach (TECControllerType type in controllerTypes)
            {
                if (controller.Type.Guid == type.Guid)
                {
                    controller.Type = type;
                    return;
                }
            }
        }
        private static void linkSubScopeToDevices(TECSubScope subScope, IEnumerable<IEndDevice> devices)
        {
            ObservableCollection<IEndDevice> replacements = new ObservableCollection<IEndDevice>();
            foreach (IEndDevice item in subScope.Devices)
            {
                bool found = false;
                foreach (IEndDevice device in devices)
                {
                    if (item.Guid == device.Guid)
                    {
                        replacements.Add(device);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Subscope device not found.");
                }
               

            }
            subScope.Devices.ObservablyClear();
            subScope.Devices.AddRange(replacements);
        }

        private static void linkProposalItmes(IEnumerable<TECProposalItem> proposalItems, IEnumerable<TECEquipment> equipment, Dictionary<Guid, Guid> guidDictionary)
        {
            foreach(var item in proposalItems)
            {
                List<TECEquipment> newEquip = new List<TECEquipment>();
                List<TECEquipment> oldEquip = new List<TECEquipment>();
                foreach(var equip in equipment)
                {
                    bool isCopy = (guidDictionary != null && guidDictionary[item.DisplayScope.Guid] == guidDictionary[equip.Guid]);
                    if(equip.Guid == item.DisplayScope.Guid || isCopy)
                    {
                        item.DisplayScope = equip;
                    }
                    else
                    {
                        foreach(var scope in item.ContainingScope)
                        {
                            isCopy = (guidDictionary != null && guidDictionary[scope.Guid] == guidDictionary[equip.Guid]);
                            if (equip.Guid == scope.Guid || isCopy)
                            {
                                newEquip.Add(equip);
                                oldEquip.Add(scope);
                            }
                        }
                    }
                }
                oldEquip.ForEach(x => item.ContainingScope.Remove(x));
                newEquip.ForEach(x => item.ContainingScope.Add(x));
            }

        }

        private static void linkTemplateReferences(TECTemplates templatesManager, Dictionary<Guid, List<Guid>> templateReferences)
        {
            var templates = templatesManager.Templates;

            List<TECSubScope> allSubScope = new List<TECSubScope>();
            List<TECEquipment> allEquipment = new List<TECEquipment>();
            foreach(TECEquipment equipment in templates.EquipmentTemplates)
            {
                allSubScope.AddRange(equipment.SubScope);
            }
            foreach(TECSystem system in templates.SystemTemplates)
            {
                allEquipment.AddRange(system.Equipment);
                allSubScope.AddRange(system.GetAllSubScope());
            }
            foreach(TECSubScope template in templates.SubScopeTemplates)
            {
                List<TECSubScope> references = findReferences(template, allSubScope, templateReferences);
                if(references.Count > 0)
                {
                    templatesManager.SubScopeSynchronizer.LinkExisting(template, references);
                }
            }
            foreach(TECEquipment template in templates.EquipmentTemplates)
            {
                List<TECEquipment> references = findReferences(template, allEquipment, templateReferences);
                if (references.Count > 0)
                {
                    templatesManager.EquipmentSynchronizer.LinkExisting(template, references);
                }
                foreach(TECSubScope subScope in template.SubScope)
                {
                    List<TECSubScope> subReferences = findReferences(subScope, allSubScope, templateReferences);
                    if(subReferences.Count > 0)
                    {
                        templatesManager.SubScopeSynchronizer.LinkExisting(subScope, subReferences);
                    }
                }
            }

        }
        private static List<T> findReferences<T>(T template, IEnumerable<T> referenceList, Dictionary<Guid, List<Guid>> templateReferences) where T : ITECObject 
        {
            List<T> references = new List<T>();
            foreach (T item in referenceList)
            {
                if (templateReferences.ContainsKey(template.Guid) && templateReferences[template.Guid].Contains(item.Guid))
                {
                    references.Add(item);
                }
            }
            return references;
        }
        
        #region Location Linking
        private static void linkLocation(TECSystem system, IEnumerable<TECLocation> locations)
        {
            linkLocation(system as TECLocated, locations);
            if(system is TECTypical typical)
            {
                foreach (TECSystem instance in typical.Instances)
                {
                    linkLocation(instance, locations);
                }
            }
            
            foreach (TECEquipment equip in system.Equipment)
            {
                linkLocation(equip, locations);
            }
        }
        private static void linkLocation(TECEquipment equipment, IEnumerable<TECLocation> locations)
        {
            linkLocation(equipment as TECLocated, locations);
            foreach (TECSubScope ss in equipment.SubScope)
            {
                linkLocation(ss, locations);
            }
        }
        private static void linkLocation(TECLocated scope, IEnumerable<TECLocation> locations)
        {
            if (scope.Location != null)
            {
                bool found = false;
                foreach (TECLocation location in locations)
                {
                    if (scope.Location.Guid == location.Guid)
                    {
                        scope.Location = location;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Location in scope not found.");
                }
            }
        }        
        #endregion

        #region System Instance Reference Methods
        /// <summary>
        /// Rereferences the objects in a typical, instances scope dictionary after copying a typical system.
        /// </summary>
        /// <param name="characteristic">The typical items (equipment, panels, controllers) in the typical system</param>
        /// <param name="instances">The instances of those items in child system instance</param>
        /// <param name="oldCharacteristicInstances">A previosuly linked scope dictionary, from the original system before copying</param>
        /// <param name="newCharacteristicInstances">The scope dictionary that must be linked</param>
        static private void linkCharacteristicCollections(IList characteristic, IList instances,
            ObservableListDictionary<ITECObject > oldCharacteristicInstances,
            ObservableListDictionary<ITECObject > newCharacteristicInstances)
        {
            foreach (var item in oldCharacteristicInstances.GetFullDictionary())
            {
                foreach (ITECObject  charItem in characteristic)
                {
                    if (item.Key.Guid == charItem.Guid)
                    {
                        foreach (var sub in item.Value)
                        {
                            foreach (ITECObject  subInstance in instances)
                            {
                                if (subInstance.Guid == sub.Guid)
                                {
                                    newCharacteristicInstances.AddItem(charItem, subInstance);
                                }
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Creates the typical, instances scope dictionary of a system after loading.
        /// </summary>
        /// <param name="typical">The typical system</param>
        /// <param name="guidDictionary">The dictionary of typical to instances guids loaded</param>
        private static void createScopeDictionary(TECTypical typical, Dictionary<Guid, List<Guid>> guidDictionary)
        {
            if(guidDictionary.Count == 0)
            {
                return;
            }
            foreach (TECSystem instance in typical.Instances)
            {
                foreach (TECEquipment equipment in typical.Equipment)
                {
                    linkCharacteristicWithInstances(equipment, instance.Equipment, guidDictionary, typical.TypicalInstanceDictionary);
                    foreach (TECSubScope subscope in equipment.SubScope)
                    {
                        foreach (TECEquipment instanceEquipment in instance.Equipment)
                        {
                            linkCharacteristicWithInstances(subscope, instanceEquipment.SubScope, guidDictionary, typical.TypicalInstanceDictionary);
                            foreach (TECPoint point in subscope.Points)
                            {
                                foreach (TECSubScope instanceSubScope in instanceEquipment.SubScope)
                                {
                                    linkCharacteristicWithInstances(point, instanceSubScope.Points, guidDictionary, typical.TypicalInstanceDictionary);
                                }
                            }
                        }
                    }
                }
                foreach (TECController controller in typical.Controllers)
                {
                    linkCharacteristicWithInstances(controller, instance.Controllers, guidDictionary, typical.TypicalInstanceDictionary);
                }
                foreach (TECPanel panel in typical.Panels)
                {
                    linkCharacteristicWithInstances(panel, instance.Panels, guidDictionary, typical.TypicalInstanceDictionary);
                }
                foreach(TECMisc misc in typical.MiscCosts)
                {
                    linkCharacteristicWithInstances(misc, instance.MiscCosts, guidDictionary, typical.TypicalInstanceDictionary);
                }
                foreach(TECScopeBranch branch in typical.ScopeBranches)
                {
                    linkCharacteristicWithInstances(branch, instance.ScopeBranches, guidDictionary, typical.TypicalInstanceDictionary);
                }
            }
        }
        /// <summary>
        /// Generically references scope to instances of scope into a dictionary from a guid, guids dictionary
        /// </summary>
        /// <param name="characteristic"></param>
        /// <param name="instances"></param>
        /// <param name="referenceDict"></param>
        /// <param name="characteristicList"></param>
        private static void linkCharacteristicWithInstances(ITECObject  characteristic, IList instances,
            Dictionary<Guid, List<Guid>> referenceDict,
            ObservableListDictionary<ITECObject > characteristicList)
        {
            foreach (ITECObject  item in instances)
            {
                if (referenceDict[characteristic.Guid].Contains(item.Guid))
                {
                    characteristicList.AddItem(characteristic, item);
                }
            }
        }
        #endregion

        #region Scope Children
        static private void linkAssociatedCostsInScope(IEnumerable<TECAssociatedCost> costs, TECScope scope)
        {
            ObservableCollection<TECAssociatedCost> costsToAssign = new ObservableCollection<TECAssociatedCost>();
            foreach (TECAssociatedCost scopeCost in scope.AssociatedCosts)
            {
                bool found = false;
                foreach (TECAssociatedCost catalogCost in costs)
                {
                    if (scopeCost.Guid == catalogCost.Guid)
                    {
                        costsToAssign.Add(catalogCost);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Associated cost not found.");
                }
            }
            scope.AssociatedCosts.ObservablyClear();
            scope.AssociatedCosts.AddRange(costsToAssign);
        }
        static private void linkTagsInScope(IEnumerable<TECTag> tags, TECScope scope)
        {
            ObservableCollection<TECTag> linkedTags = new ObservableCollection<TECTag>();
            foreach (TECTag tag in scope.Tags)
            {
                bool found = false;
                foreach (TECTag referenceTag in tags)
                {
                    if (tag.Guid == referenceTag.Guid)
                    {
                        linkedTags.Add(referenceTag);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Tag not found.");
                }
            }
            scope.Tags.ObservablyClear();
            scope.Tags.AddRange(linkedTags);
        }
        #endregion

        #endregion
    }
}
