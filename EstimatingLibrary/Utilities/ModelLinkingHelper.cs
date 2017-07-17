﻿using DebugLibrary;
using EstimatingLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public static class ModelLinkingHelper
    {
        #region Public Methods
        public static void LinkBid(TECBid bid, Dictionary<Guid, List<Guid>> guidDictionary)
        {
            ObservableCollection<TECController> allControllers = new ObservableCollection<TECController>();
            ObservableCollection<TECSubScope> allSubScope = new ObservableCollection<TECSubScope>();

            linkCatalogs(bid.Catalogs);

            foreach (TECSystem sys in bid.Systems)
            {
                foreach (TECController controller in sys.Controllers)
                {
                    allControllers.Add(controller);
                }
                foreach (TECSystem instance in sys.SystemInstances)
                {
                    foreach (TECController controller in instance.Controllers)
                    {
                        allControllers.Add(controller);
                    }
                    foreach (TECEquipment equip in instance.Equipment)
                    {
                        foreach (TECSubScope ss in equip.SubScope)
                        {
                            allSubScope.Add(ss);
                        }
                    }
                    linkPanelsToControllers(instance.Panels, instance.Controllers);
                }
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        allSubScope.Add(ss);
                    }
                }

                linkSystemToCatalogs(sys, bid.Catalogs);
                linkLocation(sys, bid.Locations);
                linkPanelsToControllers(sys.Panels, sys.Controllers);

                createScopeDictionary(sys, guidDictionary);
            }

            foreach (TECController controller in bid.Controllers)
            {
                allControllers.Add(controller);

                linkControllerToCatalogs(controller, bid.Catalogs);
            }

            foreach (TECPanel panel in bid.Panels)
            {
                linkPanelToCatalogs(panel, bid.Catalogs);
            }

            linkNetworkConnections(allControllers);
            linkSubScopeConnections(allControllers, allSubScope);
            linkPanelsToControllers(bid.Panels, bid.Controllers);
        }

        public static void LinkTemplates(TECTemplates templates)
        {
            linkCatalogs(templates.Catalogs);

            foreach (TECSystem sys in templates.SystemTemplates)
            {
                linkSystemToCatalogs(sys, templates.Catalogs);

                ObservableCollection<TECSubScope> allSysSubScope = new ObservableCollection<TECSubScope>();
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        allSysSubScope.Add(ss);
                    }
                }
                linkSubScopeConnections(sys.Controllers, allSysSubScope);
                linkPanelsToControllers(sys.Panels, sys.Controllers);
            }
            foreach (TECEquipment equip in templates.EquipmentTemplates)
            {
                linkEquipmentToCatalogs(equip, templates.Catalogs);
            }
            foreach (TECSubScope ss in templates.SubScopeTemplates)
            {
                linkSubScopeToCatalogs(ss, templates.Catalogs);
            }
            foreach (TECController controller in templates.ControllerTemplates)
            {
                linkControllerToCatalogs(controller, templates.Catalogs);
            }
            foreach (TECPanel panel in templates.PanelTemplates)
            {
                linkPanelToCatalogs(panel, templates.Catalogs);
            }
        }

        public static void LinkSystem(TECSystem system, TECScopeManager scopeManager, Dictionary<Guid, Guid> guidDictionary)
        {
            linkSystemToCatalogs(system, scopeManager.Catalogs);
            linkSubScopeConnections(system.Controllers, system.SubScope, guidDictionary);
            linkPanelsToControllers(system.Panels, system.Controllers, guidDictionary);
        }

        //Was LinkCharacteristicInstances()
        public static void LinkTypicalInstanceDictionary(ObservableItemToInstanceList<TECScope> oldDictionary, TECSystem newTypical)
        {
            ObservableItemToInstanceList<TECScope> newCharacteristicInstances = new ObservableItemToInstanceList<TECScope>();
            foreach (TECSystem instance in newTypical.SystemInstances)
            {
                linkCharacteristicCollections(newTypical.Equipment, instance.Equipment, oldDictionary, newCharacteristicInstances);
                foreach (TECEquipment equipment in newTypical.Equipment)
                {
                    foreach (TECEquipment instanceEquipment in instance.Equipment)
                    {
                        linkCharacteristicCollections(equipment.SubScope, instanceEquipment.SubScope, oldDictionary, newCharacteristicInstances);
                        foreach (TECSubScope subscope in equipment.SubScope)
                        {
                            foreach (TECSubScope instanceSubScope in instanceEquipment.SubScope)
                            {
                                linkCharacteristicCollections(subscope.Points, instanceSubScope.Points, oldDictionary, newCharacteristicInstances);
                            }
                        }
                    }
                }
                linkCharacteristicCollections(newTypical.Controllers, instance.Controllers, oldDictionary, newCharacteristicInstances);
                linkCharacteristicCollections(newTypical.Panels, instance.Panels, oldDictionary, newCharacteristicInstances);
            }
            newTypical.CharactersticInstances = newCharacteristicInstances;
        }

        #region public static void LinkScopeItem(TECScope scope, TECBid Bid)
        public static void LinkScopeItem(TECSystem scope, TECBid bid)
        {
            linkScopeChildrenToCatalogs(scope, bid.Catalogs);
            linkLocation(scope, bid.Locations);
            foreach (TECSystem instance in scope.SystemInstances)
            {
                LinkScopeItem(instance, bid);
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
            foreach (TECPoint point in scope.Points)
            {
                LinkScopeItem(point, bid);
            }
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

        public static void LinkBidToCatalogs(TECBid bid)
        {
            linkCatalogs(bid.Catalogs);
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
        #region Catalog Linking
        private static void linkCatalogs(TECCatalogs catalogs)
        {
            foreach (TECDevice device in catalogs.Devices)
            {
                linkDeviceToCatalogs(device, catalogs);
            }
            foreach (TECIOModule module in catalogs.IOModules)
            {
                linkModuleToManufacturer(module, catalogs.Manufacturers);
            }
            foreach (TECConnectionType connectionType in catalogs.ConnectionTypes)
            {
                linkElectricalMaterialComponentToCatalogs(connectionType, catalogs);
            }
            foreach (TECConduitType conduitType in catalogs.ConduitTypes)
            {
                linkElectricalMaterialComponentToCatalogs(conduitType, catalogs);
            }
        }

        private static void linkSystemToCatalogs(TECSystem system, TECCatalogs catalogs)
        {
            //Should assume linking a typical system with potential instances, controllers and panels.

            linkScopeChildrenToCatalogs(system, catalogs);
            foreach(TECSystem instance in system.SystemInstances)
            {
                linkSystemToCatalogs(instance, catalogs);
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
            linkSubScopeToDevices(ss, catalogs.Devices);
            foreach(TECPoint point in ss.Points)
            {
                linkScopeChildrenToCatalogs(point, catalogs);
            }
        }

        private static void linkDeviceToCatalogs(TECDevice dev, TECCatalogs catalogs)
        {
            linkDeviceToConnectionTypes(dev, catalogs.ConnectionTypes);
            linkDeviceToManufacturer(dev, catalogs.Manufacturers);
            linkScopeChildrenToCatalogs(dev, catalogs);
        }

        private static void linkControllerToCatalogs(TECController controller, TECCatalogs catalogs)
        {
            linkControllerToManufacturer(controller, catalogs.Manufacturers);
            foreach(TECIO io in controller.IO)
            {
                linkIOToModule(io, catalogs.IOModules);
            }
            foreach(TECConnection connection in controller.ChildrenConnections)
            {
                linkConnectionToCatalogs(connection, catalogs);
            }
            linkScopeChildrenToCatalogs(controller, catalogs);
        }

        private static void linkPanelToCatalogs(TECPanel panel, TECCatalogs catalogs)
        {
            linkPanelToPanelType(panel, catalogs.PanelTypes);
            linkScopeChildrenToCatalogs(panel, catalogs);
        }

        private static void linkElectricalMaterialComponentToCatalogs(ElectricalMaterialComponent component, TECCatalogs catalogs)
        {
            linkScopeChildrenToCatalogs((component as TECScope), catalogs);
            linkElectricalMaterialComponentToRatedCosts(component, catalogs.AssociatedCosts);
        }

        private static void linkConnectionToCatalogs(TECConnection connection, TECCatalogs catalogs)
        {
            linkConnectionToConduitType(connection, catalogs.ConduitTypes);
            TECNetworkConnection netConnect = connection as TECNetworkConnection;if (netConnect != null)
            {
                linkNetworkConnectionToConnectionType(netConnect, catalogs.ConnectionTypes);
            }
        }
        #endregion

        private static void linkPanelsToControllers(ObservableCollection<TECPanel> panels, ObservableCollection<TECController> controllers, Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECPanel panel in panels)
            {
                ObservableCollection<TECController> controllersToLink = new ObservableCollection<TECController>();
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
                panel.Controllers = controllersToLink;
            }
        }

        private static void linkNetworkConnections(ObservableCollection<TECController> controllers, Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECController controller in controllers)
            {
                foreach (TECConnection connection in controller.ChildrenConnections)
                {
                    if (connection is TECNetworkConnection)
                    {
                        TECNetworkConnection netConnect = connection as TECNetworkConnection;
                        ObservableCollection<TECController> controllersToAdd = new ObservableCollection<TECController>();
                        foreach (TECController child in netConnect.ChildrenControllers)
                        {
                            foreach (TECController bidController in controllers)
                            {
                                bool isCopy = (guidDictionary != null && guidDictionary[child.Guid] == guidDictionary[bidController.Guid]);
                                if (child.Guid == bidController.Guid || isCopy)
                                {
                                    controllersToAdd.Add(bidController);
                                    bidController.ParentConnection = netConnect;
                                }
                            }
                        }
                        netConnect.ChildrenControllers = controllersToAdd;
                    }
                }
            }
        }

        private static void linkSubScopeConnections(ObservableCollection<TECController> controllers, ObservableCollection<TECSubScope> subscope, Dictionary<Guid, Guid> guidDictionary = null)
        {
            foreach (TECSubScope subScope in subscope)
            {
                foreach (TECController controller in controllers)
                {
                    foreach (TECConnection connection in controller.ChildrenConnections)
                    {
                        if (connection is TECSubScopeConnection)
                        {
                            TECSubScopeConnection ssConnect = connection as TECSubScopeConnection;
                            bool isCopy = (guidDictionary != null && guidDictionary[ssConnect.SubScope.Guid] == guidDictionary[subScope.Guid]);
                            if (ssConnect.SubScope.Guid == subScope.Guid || isCopy)
                            {
                                ssConnect.SubScope = subScope;
                                subScope.LinkConnection(ssConnect);
                            }
                        }
                    }
                }
            }
        }

        private static void linkScopeChildrenToCatalogs(TECScope scope, TECCatalogs catalogs)
        {
            linkAssociatedCostsInScope(catalogs.AssociatedCosts, scope);
            linkTagsInScope(catalogs.Tags, scope);
        }

        private static void linkModuleToManufacturer(TECIOModule module, ObservableCollection<TECManufacturer> manufacturers)
        {
            foreach (TECManufacturer manufacturer in manufacturers)
            {
                if (module.Manufacturer.Guid == manufacturer.Guid)
                {
                    module.Manufacturer = manufacturer;
                }
            }
        }

        private static void linkDeviceToConnectionTypes(TECDevice device, ObservableCollection<TECConnectionType> connectionTypes)
        {
            ObservableCollection<TECConnectionType> linkedTypes = new ObservableCollection<TECConnectionType>();
            foreach (TECConnectionType deviceType in device.ConnectionTypes)
            {
                bool found = false;
                foreach (TECConnectionType connectionType in connectionTypes)
                {
                    if (deviceType.Guid == connectionType.Guid)
                    {
                        linkedTypes.Add(connectionType);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Device connection type not found.");
                }
            }
            device.ConnectionTypes = linkedTypes;
        }

        private static void linkDeviceToManufacturer(TECDevice device, ObservableCollection<TECManufacturer> manufacturers)
        {
            foreach (TECManufacturer man in manufacturers)
            {
                if (device.Manufacturer.Guid == man.Guid)
                {
                    device.Manufacturer = man;
                }
            }
        }

        private static void linkElectricalMaterialComponentToRatedCosts(ElectricalMaterialComponent component, ObservableCollection<TECCost> costs)
        {
            ObservableCollection<TECCost> costsToAssign = new ObservableCollection<TECCost>();
            foreach (TECCost cost in costs)
            {
                foreach (TECCost scopeCost in component.RatedCosts)
                {
                    if (scopeCost.Guid == cost.Guid)
                    { costsToAssign.Add(cost); }
                }
            }
            component.RatedCosts = costsToAssign;
        }

        private static void linkControllerToManufacturer(TECController controller, ObservableCollection<TECManufacturer> manufacturers)
        {
            foreach (TECManufacturer manufacturer in manufacturers)
            {
                if (controller.Manufacturer != null)
                {
                    if (controller.Manufacturer.Guid == manufacturer.Guid)
                    {
                        controller.Manufacturer = manufacturer;
                    }
                }
            }
        }

        private static void linkIOToModule(TECIO io, ObservableCollection<TECIOModule> modules)
        {
            if (io.IOModule != null)
            {
                foreach (TECIOModule module in modules)
                {
                    if (io.IOModule.Guid == module.Guid)
                    {
                        io.IOModule = module;
                        break;
                    }
                }
            }
        }

        private static void linkConnectionToConduitType(TECConnection connection, ObservableCollection<TECConduitType> conduitTypes)
        {
            if (connection.ConduitType != null)
            {
                foreach (TECConduitType type in conduitTypes)
                {
                    if (connection.ConduitType.Guid == type.Guid)
                    {
                        connection.ConduitType = type;
                        return;
                    }
                }
            }
        }

        private static void linkNetworkConnectionToConnectionType(TECNetworkConnection netConnect, ObservableCollection<TECConnectionType> connectionTypes)
        {
            foreach (TECConnectionType type in connectionTypes)
            {
                if (netConnect.ConnectionType.Guid == type.Guid)
                {
                    netConnect.ConnectionType = type;
                    break;
                }
            }
        }

        private static void linkPanelToPanelType(TECPanel panel, ObservableCollection<TECPanelType> panelTypes)
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

        private static void linkSubScopeToDevices(TECSubScope subScope, ObservableCollection<TECDevice> devices)
        {
            ObservableCollection<TECDevice> replacements = new ObservableCollection<TECDevice>();
            foreach(TECDevice device in subScope.Devices)
            {
                bool found = false;
                foreach(TECDevice catalogDevice in devices)
                {
                    if (device.Guid == catalogDevice.Guid)
                    {
                        replacements.Add(catalogDevice);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Subscope device not found.");
                }
            }
            subScope.Devices = replacements;
        }

        #region Location Linking
        private static void linkLocation(TECSystem system, ObservableCollection<TECLocation> locations)
        {
            linkLocation(system as TECScope, locations);
            foreach (TECSystem instance in system.SystemInstances)
            {
                linkLocation(instance, locations);
            }
            foreach (TECEquipment equip in system.Equipment)
            {
                linkLocation(equip, locations);
            }
        }

        private static void linkLocation(TECEquipment equipment, ObservableCollection<TECLocation> locations)
        {
            linkLocation(equipment as TECScope, locations);
            foreach (TECSubScope ss in equipment.SubScope)
            {
                linkLocation(ss, locations);
            }
        }

        static private void linkLocation(TECScope scope, ObservableCollection<TECLocation> locations)
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
            ObservableItemToInstanceList<TECScope> oldCharacteristicInstances,
            ObservableItemToInstanceList<TECScope> newCharacteristicInstances)
        {
            foreach (var item in oldCharacteristicInstances.GetFullDictionary())
            {
                foreach (TECScope charItem in characteristic)
                {
                    if (item.Key.Guid == charItem.Guid)
                    {
                        foreach (var sub in item.Value)
                        {
                            foreach (TECScope subInstance in instances)
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
        private static void createScopeDictionary(TECSystem typical, Dictionary<Guid, List<Guid>> guidDictionary)
        {
            if(guidDictionary.Count == 0)
            {
                return;
            }
            foreach (TECSystem instance in typical.SystemInstances)
            {
                foreach (TECEquipment equipment in typical.Equipment)
                {
                    linkCharacteristicWithInstances(equipment, instance.Equipment, guidDictionary, typical.CharactersticInstances);
                    foreach (TECSubScope subscope in equipment.SubScope)
                    {
                        foreach (TECEquipment instanceEquipment in instance.Equipment)
                        {
                            linkCharacteristicWithInstances(subscope, instanceEquipment.SubScope, guidDictionary, typical.CharactersticInstances);
                            foreach (TECPoint point in subscope.Points)
                            {
                                foreach (TECSubScope instanceSubScope in instanceEquipment.SubScope)
                                {
                                    linkCharacteristicWithInstances(point, instanceSubScope.Points, guidDictionary, typical.CharactersticInstances);
                                }
                            }
                        }
                    }
                }
                foreach (TECController controller in typical.Controllers)
                {
                    linkCharacteristicWithInstances(controller, instance.Controllers, guidDictionary, typical.CharactersticInstances);
                }
                foreach (TECPanel panel in typical.Panels)
                {
                    linkCharacteristicWithInstances(panel, instance.Panels, guidDictionary, typical.CharactersticInstances);
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
        private static void linkCharacteristicWithInstances(TECScope characteristic, IList instances,
            Dictionary<Guid, List<Guid>> referenceDict,
            ObservableItemToInstanceList<TECScope> characteristicList)
        {
            foreach (TECScope item in instances)
            {
                if (referenceDict[characteristic.Guid].Contains(item.Guid))
                {
                    characteristicList.AddItem(characteristic, item);
                }
            }
        }
        #endregion

        #region Scope Children
        static private void linkAssociatedCostsInScope(ObservableCollection<TECCost> costs, TECScope scope)
        {
            ObservableCollection<TECCost> costsToAssign = new ObservableCollection<TECCost>();
            foreach (TECCost scopeCost in scope.AssociatedCosts)
            {
                bool found = false;
                foreach (TECCost catalogCost in costs)
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
            scope.AssociatedCosts = costsToAssign;
        }
        static private void linkTagsInScope(ObservableCollection<TECTag> tags, TECScope scope)
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
            scope.Tags = linkedTags;
        }
        #endregion
        #endregion
    }
}