using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Linq;

namespace EstimatingUtilitiesLibrary.Database
{
    internal class DatabaseLoader
    {
        //FMT is used by DateTime to convert back and forth between the DateTime type and string
        private const string DB_FMT = "O";
        static private List<String> deprecatedPointTypes = new List<string>() { "BACnetMSTP", "BACnetIP", "LonWorks", "ModbusTCP", "ModbusRTU" };

        static private Logger logger = LogManager.GetCurrentClassLogger();
        static private SQLiteDatabase SQLiteDB;

        static private bool justUpdated;
        static private TECManufacturer tempManufacturer;
        static private TECPanelType tempPanelType;
        static private TECControllerType tempControllerType;
        static private TECProtocol tempProtocol;
        static private TECConnectionType tempConnectionType;
        static private Dictionary<String, List<TECPoint>> networkPoints;

        public static (TECScopeManager scopeManager, bool needsSaveNew) Load(string path, bool versionUpdated = false)
        {
            justUpdated = versionUpdated;
            if (justUpdated)
            {
                setupTemps();
            }
            TECScopeManager workingScopeManager = null;
            SQLiteDB = new SQLiteDatabase(path);
            SQLiteDB.NonQueryCommand("BEGIN TRANSACTION");

            var tableNames = DatabaseHelper.TableNames(SQLiteDB);
            bool needsUpdate;
            if (tableNames.Contains("BidInfo"))
            {
                (workingScopeManager, needsUpdate) = loadBid();
            }
            else if (tableNames.Contains("TemplatesInfo"))
            {
                (workingScopeManager, needsUpdate) = loadTemplates();
            }
            else
            {
                MessageBox.Show("File is not a compatible database.");
                return (null, false);
            }

            SQLiteDB.NonQueryCommand("END TRANSACTION");
            SQLiteDB.Connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return (workingScopeManager, needsUpdate);
        }

        private static (TECBid bid, bool needsUpdate) loadBid()
        {
            TECBid bid = getObjectFromTable(new BidInfoTable(), id => { return new TECBid(id); }, new TECBid());

            getScopeManagerProperties(bid);
            List<TECLocation> locations = getObjectsFromTable(new LocationTable(), id => new TECLocation(id));
            Dictionary<Guid, List<TECLocation>> bidLocations = getOneToManyRelationships(new BidLocationTable(), locations);
            bid.Locations = bidLocations.ValueOrNew(bid.Guid);
            Dictionary<Guid, TECLocation> locationDictionary = getOneToOneRelationships(new LocatedLocationTable(), locations);

            List<TECScopeBranch> branches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id, false));
            Dictionary<Guid, List<Guid>> branchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable());
            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(), bid.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(), bid.Catalogs.AssociatedCosts);

            bid.Parameters = getObjectFromTable(new ParametersTable(), id => { return new TECParameters(id); }, new TECParameters(bid.Guid));
            bid.ExtraLabor = getObjectFromTable(new ExtraLaborTable(), id => { return new TECExtraLabor(id); }, new TECExtraLabor(bid.Guid));
            bid.ScopeTree = getChildObjects(new BidScopeBranchTable(), new ScopeBranchTable(), bid.Guid, id => new TECScopeBranch(id, false)).ToOC();
            bid.ScopeTree.ForEach(item => linkBranchHierarchy(item, branches, branchHierarchy));
            (var typicals, var controllers, var panels)  = getScopeHierarchy(bid.Guid, bid.Catalogs);
            bid.Systems = typicals.ToOC();
            bid.SetControllers(controllers);
            bid.Panels = panels.ToOC();
            bid.Notes = getObjectsFromTable(new NoteTable(), id => new TECLabeled(id)).ToOC();
            bid.Exclusions = getObjectsFromTable(new ExclusionTable(), id => new TECLabeled(id)).ToOC();
            bid.MiscCosts = getChildObjects(new BidMiscTable(), new MiscTable(), bid.Guid, data => getMiscFromRow(data, false)).ToOC();
            bid.Schedule = getSchedule(bid);
            
            List<TECLocated> allLocated = bid.GetAll<TECLocated>();
            allLocated.ForEach(item => item.Location = locationDictionary.ValueOrDefault(item.Guid, null));
            List<TECScope> allScope = bid.GetAll<TECScope>();
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));
            
            var placeholderDict = getCharacteristicInstancesList();
            bool needsSave = ModelLinkingHelper.LinkLoadedBid(bid, placeholderDict);

            return (bid, needsSave);
        }
        private static (TECTemplates templates, bool needsUpdate) loadTemplates()
        {
            TECTemplates templates = new TECTemplates();
            templates = getObjectFromTable(new TemplatesInfoTable(), id => { return new TECTemplates(id); }, new TECTemplates());
            getScopeManagerProperties(templates);

            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(), templates.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(), templates.Catalogs.AssociatedCosts);

            List<IEndDevice> allEndDevices = new List<IEndDevice>(templates.Catalogs.Devices);
            allEndDevices.AddRange(templates.Catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), templates.Catalogs.ConduitTypes);
            Dictionary<Guid, List<TECIOModule>> providedControllerModuleRelationships = getOneToManyRelationships(new ProvidedControllerIOModuleTable(), templates.Catalogs.IOModules);

            Dictionary<Guid, TECControllerType> providedControllerTypeDictionary = getOneToOneRelationships(new ProvidedControllerControllerTypeTable(), templates.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(), templates.Catalogs.PanelTypes);
            Dictionary<Guid, TECProtocol> connectionProtocol = getOneToOneRelationships(new NetworkConnectionProtocolTable(), templates.Catalogs.Protocols);
            Dictionary<Guid, List<TECConnectionType>> hardwiredConnectionTypes = getOneToManyRelationships(new HardwiredConnectionConnectionTypeTable(), templates.Catalogs.ConnectionTypes);

            List<TECSystem> systems = getObjectsFromTable(new SystemTable(), id => new TECSystem(id, false));
            List<TECEquipment> equipment = getObjectsFromTable(new EquipmentTable(), id => new TECEquipment(id, false));
            List<TECSubScope> subScope = getObjectsFromTable(new SubScopeTable(), id => new TECSubScope(id, false));
            List<TECPoint> points = getObjectsFromTable(new PointTable(), id => new TECPoint(id, false));
            List<TECMisc> misc = getObjectsFromTable(new MiscTable(), data => getMiscFromRow(data, false));
            List<TECProvidedController> providedControllers = getObjectsFromTable(new ProvidedControllerTable(), data => getProvidedControllerFromRow(data, false, providedControllerTypeDictionary));
            List<TECFBOController> fboControllers = getObjectsFromTable(new FBOControllerTable(), id => new TECFBOController(id, false, templates.Catalogs));
            List<TECController> controllers = new List<TECController>(providedControllers);
            controllers.AddRange(fboControllers);
            List<TECPanel> panels = getObjectsFromTable(new PanelTable(), data => getPanelFromRow(data, false, panelTypeDictionary));

            Dictionary<Guid, TECController> connectionParents = getChildIDToParentRelationships(new ControllerConnectionTable(), controllers);

            List<IConnectable> allNetworkConnectable = new List<IConnectable>(subScope);
            allNetworkConnectable.AddRange(controllers);
            Dictionary<Guid, List<IConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(), allNetworkConnectable);
            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new SubScopeConnectionChildrenTable(), subScope);
            
            List<TECHardwiredConnection> subScopeConnections = getObjectsFromTable(new SubScopeConnectionTable(), id => new TECHardwiredConnection(id, subScopeConnectionChildrenRelationships[id],
                connectionParents[id], new TECHardwiredProtocol(hardwiredConnectionTypes[id]), false));
            List<TECNetworkConnection> networkConnections = getObjectsFromTable(new NetworkConnectionTable(), id => new TECNetworkConnection(id, connectionParents[id], connectionProtocol[id], false));
            List<IControllerConnection> connections = new List<IControllerConnection>(subScopeConnections);
            connections.AddRange(networkConnections);

            Dictionary<Guid, List<Guid>> branchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable());
            List<TECScopeBranch> scopeBranches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id, false));
            scopeBranches.ForEach(x => linkBranchHierarchy(x, scopeBranches, branchHierarchy));

            templates.Parameters = getObjectsFromTable(new ParametersTable(), id => new TECParameters(id)).ToOC();    

            Dictionary<Guid, List<TECEquipment>> systemEquipment = getOneToManyRelationships(new SystemEquipmentTable(), equipment);
            Dictionary<Guid, List<TECController>> systemController = getOneToManyRelationships(new SystemControllerTable(), controllers);
            Dictionary<Guid, List<TECPanel>> systemPanels = getOneToManyRelationships(new SystemPanelTable(), panels);
            Dictionary<Guid, List<TECMisc>> systemMisc = getOneToManyRelationships(new SystemMiscTable(), misc);
            Dictionary<Guid, List<TECScopeBranch>> systemScopeBranch = getOneToManyRelationships(new SystemScopeBranchTable(), scopeBranches);
            Dictionary<Guid, List<TECSubScope>> equipmentSubScope = getOneToManyRelationships(new EquipmentSubScopeTable(), subScope);
            Dictionary<Guid, List<TECPoint>> subScopePoint = getOneToManyRelationships(new SubScopePointTable(), points);
            Dictionary<Guid, List<IControllerConnection>> controllerConnection = getOneToManyRelationships(new ControllerConnectionTable(), connections);
            
            subScope.ForEach(item => item.Points = subScopePoint.ValueOrNew(item.Guid));
            equipment.ForEach(item => item.SubScope = equipmentSubScope.ValueOrNew(item.Guid));
            foreach (TECSystem system in systems)
            {
                system.Equipment = systemEquipment.ValueOrNew(system.Guid);
                system.SetControllers(systemController.ValueOrNew(system.Guid));
                system.Panels = systemPanels.ValueOrNew(system.Guid);
                system.MiscCosts = systemMisc.ValueOrNew(system.Guid);
                system.ScopeBranches = systemScopeBranch.ValueOrNew(system.Guid);
            }
            controllers.ForEach(item => {
                item.ChildrenConnections = controllerConnection.ValueOrNew(item.Guid);
            });
            
            Dictionary<Guid, List<TECController>> panelControllerDictionary = getOneToManyRelationships(new PanelControllerTable(), controllers);
            controllers.ForEach(item => { if (item is TECProvidedController provided) provided.IOModules = providedControllerModuleRelationships.ValueOrNew(provided.Guid);});

            subScope.ForEach(item => item.Devices = endDevices.ValueOrNew(item.Guid));
            panels.ForEach(item => item.Controllers = panelControllerDictionary.ValueOrNew(item.Guid));
            
            subScopeConnections.ForEach(item => { populateSubScopeConnectionProperties(item, connectionConduitTypes); });
            networkConnections.ForEach(item => { populateNetworkConnectionProperties(item, networkChildrenRelationships, 
                connectionConduitTypes); });

            Dictionary<Guid, List<Guid>> systemTemplates = getOneToManyRelationships(new TemplatesSystemTable());
            Dictionary<Guid, List<Guid>> equipmentTemplates = getOneToManyRelationships(new TemplatesEquipmentTable());
            Dictionary<Guid, List<Guid>> subScopeTemplates = getOneToManyRelationships(new TemplatesSubScopeTable());
            Dictionary<Guid, List<Guid>> controllerTemplates = getOneToManyRelationships(new TemplatesControllerTable());
            Dictionary<Guid, List<Guid>> panelTemplates = getOneToManyRelationships(new TemplatesPanelTable());
            Dictionary<Guid, List<Guid>> miscTemplates = getOneToManyRelationships(new TemplatesMiscCostTable());

            templates.SystemTemplates = getRelatedReferences(systemTemplates.ContainsKey(templates.Guid) ? systemTemplates[templates.Guid] : new List<Guid>(), systems).ToOC();
            templates.EquipmentTemplates = getRelatedReferences(equipmentTemplates.ContainsKey(templates.Guid) ? equipmentTemplates[templates.Guid] : new List<Guid>(), equipment).ToOC();
            templates.SubScopeTemplates = getRelatedReferences(subScopeTemplates.ContainsKey(templates.Guid) ? subScopeTemplates[templates.Guid] : new List<Guid>(), subScope).ToOC();
            templates.ControllerTemplates = getRelatedReferences(controllerTemplates.ContainsKey(templates.Guid) ? controllerTemplates[templates.Guid] : new List<Guid>(), controllers).ToOC();
            templates.PanelTemplates = getRelatedReferences(panelTemplates.ContainsKey(templates.Guid) ? panelTemplates[templates.Guid] : new List<Guid>(), panels).ToOC();
            templates.MiscCostTemplates = getRelatedReferences(miscTemplates.ContainsKey(templates.Guid) ? miscTemplates[templates.Guid] : new List<Guid>(), misc).ToOC();
            
            List<TECScope> allScope = templates.GetAll<TECScope>();
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));

            Dictionary<Guid, List<Guid>> templateReferences = getTemplateReferences();
            bool needsSave = ModelLinkingHelper.LinkLoadedTemplates(templates, templateReferences);
            return (templates, needsSave);
        }

        private static void getScopeManagerProperties(TECScopeManager scopeManager)
        {
            scopeManager.Catalogs = getCatalogs();
            if (justUpdated)
            {
                scopeManager.Catalogs.Manufacturers.Add(tempManufacturer);
                scopeManager.Catalogs.PanelTypes.Add(tempPanelType);
                scopeManager.Catalogs.ControllerTypes.Add(tempControllerType);
                scopeManager.Catalogs.ConnectionTypes.Add(tempConnectionType);
                scopeManager.Catalogs.Protocols.Add(tempProtocol);
            }
        }
        private static (List<TECTypical> typicals, List<TECController> controllers, List<TECPanel> panels) getScopeHierarchy(Guid bidID, TECCatalogs catalogs)
        {
            Dictionary<Guid, List<Guid>> bidTypicals = getOneToManyRelationships(new BidSystemTable());
            List<Guid> typicalIDs = bidTypicals.Count > 0 ? bidTypicals[bidID] : new List<Guid>();
            Dictionary<Guid, List<Guid>> typicalSystems = getOneToManyRelationships(new SystemHierarchyTable());
            Dictionary<Guid, List<Guid>> systemEquipment = getOneToManyRelationships(new SystemEquipmentTable());
            Dictionary<Guid, List<Guid>> equipmentSubScope = getOneToManyRelationships(new EquipmentSubScopeTable());
            Dictionary<Guid, List<Guid>> subScopePoints = getOneToManyRelationships(new SubScopePointTable());
            Dictionary<Guid, List<Guid>> systemControllers = getOneToManyRelationships(new SystemControllerTable());
            Dictionary<Guid, List<Guid>> systemPanels = getOneToManyRelationships(new SystemPanelTable());
            Dictionary<Guid, List<Guid>> controllerConnections = getOneToManyRelationships(new ControllerConnectionTable());
            Dictionary<Guid, List<Guid>> systemMisc = getOneToManyRelationships(new SystemMiscTable());
            Dictionary<Guid, List<Guid>> systemScopeBranches = getOneToManyRelationships(new SystemScopeBranchTable());
            Dictionary<Guid, List<Guid>> scopeBranchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable());
            Dictionary<Guid, List<Guid>> networkChildren = getOneToManyRelationships(new NetworkConnectionChildrenTable());
            Dictionary<Guid, Guid> subScopeConnectionChildren = getOneToOneRelationships(new SubScopeConnectionChildrenTable());

            Dictionary<Guid, TECControllerType> controllerTypes = getOneToOneRelationships(new ProvidedControllerControllerTypeTable(), catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypes = getOneToOneRelationships(new PanelPanelTypeTable(), catalogs.PanelTypes);
            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ProvidedControllerIOModuleTable(), catalogs.IOModules);
            List<IEndDevice> allEndDevices = new List<IEndDevice>(catalogs.Devices);
            allEndDevices.AddRange(catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), catalogs.ConduitTypes);
            Dictionary<Guid, TECProtocol> connectionProtocols = getOneToOneRelationships(new NetworkConnectionProtocolTable(), catalogs.Protocols);
            Dictionary<Guid, List<TECConnectionType>> hardwiredConnectionTypes = getOneToManyRelationships(new HardwiredConnectionConnectionTypeTable(), catalogs.ConnectionTypes);

            DataTable allSystemData = SQLiteDB.GetDataFromTable(SystemTable.TableName);
            var typicalRows = from row in allSystemData.AsEnumerable() where typicalIDs.Contains(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            var systemRows = from row in allSystemData.AsEnumerable() where !typicalIDs.Contains(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            DataTable typicalData = typicalRows.Any() ? typicalRows.CopyToDataTable() : new DataTable();
            DataTable systemData = systemRows.Any() ? systemRows.CopyToDataTable() : new DataTable();
            
            Dictionary<Guid, bool> typicalDictionary = new Dictionary<Guid, bool>();
            foreach(Guid typID in typicalIDs)
            {
                typicalDictionary.Add(typID, true);
                if (typicalSystems.ContainsKey(typID))
                {
                    foreach (Guid sysID in typicalSystems[typID])
                    {
                        typicalDictionary.Add(sysID, false);
                        addSystemChildrenToTypicalDict(sysID, false);
                    }
                }
                addSystemChildrenToTypicalDict(typID, true);
            }

            List<TECPoint> points = getObjectsFromTable(new PointTable(), id => new TECPoint(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECSubScope> subScope = getObjectsFromTable(new SubScopeTable(), id => new TECSubScope(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECEquipment> equipment = getObjectsFromTable(new EquipmentTable(), id => new TECEquipment(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECMisc> misc = getObjectsFromTable(new MiscTable(), row => { return getMiscFromRow(row, typicalDictionary); });
            List<TECScopeBranch> scopeBranches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECProvidedController> providedControllers = getObjectsFromTable(new ProvidedControllerTable(), data => getProvidedControllerFromRow(data, false, controllerTypes));
            List<TECFBOController> fboControllers = getObjectsFromTable(new FBOControllerTable(), id => new TECFBOController(id, false, catalogs));
            List<TECPanel> panels = getObjectsFromTable(new PanelTable(), row => getPanelFromRow(row, typicalDictionary, panelTypes));

            List<TECController> controllers = new List<TECController>(providedControllers);
            controllers.AddRange(fboControllers);

            List<IConnectable> connectables = new List<IConnectable>(controllers);
            connectables.AddRange(subScope);

            Dictionary<Guid, TECController> connectionParents = getChildIDToParentRelationships(new ControllerConnectionTable(), controllers);
            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new SubScopeConnectionChildrenTable(), subScope);

            List<TECHardwiredConnection> subScopeConnections = getObjectsFromTable(new SubScopeConnectionTable(), 
                id => new TECHardwiredConnection(id, subScopeConnectionChildrenRelationships[id], connectionParents[id],
                new TECHardwiredProtocol(hardwiredConnectionTypes[id]), typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECNetworkConnection> networkConnections = getObjectsFromTable(new NetworkConnectionTable(), 
                id => new TECNetworkConnection(id, connectionParents[id], connectionProtocols[id], typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<IControllerConnection> allConnections = new List<IControllerConnection>(subScopeConnections);
            allConnections.AddRange(networkConnections);
            List<TECSystem> systems = getObjectsFromData(new SystemTable(), systemData, id => new TECSystem(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false));
            List<TECTypical> typicals = getObjectsFromData(new SystemTable(), typicalData, id => new TECTypical(id));
            
            Dictionary<Guid, List<TECController>> panelControllers = getOneToManyRelationships(new PanelControllerTable(), controllers);

            controllers.ForEach(item => { if (item is TECProvidedController provided) provided.IOModules = controllerModuleRelationships.ValueOrNew(provided.Guid); });
            subScope.ForEach(item => item.Devices = endDevices.ValueOrNew(item.Guid));


            foreach(IControllerConnection item in allConnections)
            {
                item.ConduitType = connectionConduitTypes.ValueOrDefault(item.Guid, null);
                if(item is TECNetworkConnection netItem)
                {
                    if (networkChildren.ContainsKey(netItem.Guid))
                    {
                        netItem.Children = getRelatedReferences(networkChildren[item.Guid], connectables).ToOC();
                    }
                    netItem.Children.ForEach(x => x.SetParentConnection(netItem));
                }
                else if (item is TECHardwiredConnection hardItem)
                {
                    hardItem.Child.SetParentConnection(item);
                }
            }
            foreach (TECSubScope item in subScope.Where(x => subScopePoints.ContainsKey(x.Guid)))
            {
                item.Points = getRelatedReferences(subScopePoints[item.Guid], points).ToOC();
            }
            foreach(TECEquipment item in equipment.Where(x => equipmentSubScope.ContainsKey(x.Guid)))
            {
                item.SubScope = getRelatedReferences(equipmentSubScope[item.Guid], subScope).ToOC();
            }
            foreach(TECController item in controllers.Where(x => controllerConnections.ContainsKey(x.Guid)))
            {
                item.ChildrenConnections = getRelatedReferences(controllerConnections[item.Guid], allConnections).ToOC();
            }
            panels.ForEach(item => item.Controllers = panelControllers.ValueOrNew(item.Guid));
            foreach(TECSystem item in systems)
            {
                setSystemChildren(item);
            }
            foreach(TECTypical item in typicals)
            {
                setSystemChildren(item);
                if (typicalSystems.ContainsKey(item.Guid)) 
                    item.Instances = getRelatedReferences(typicalSystems[item.Guid], systems).ToOC();
            }

            var outControllers = new List<TECController>(controllers.Where(x => !systemControllers.Any(pair => pair.Value.Contains(x.Guid))));
            var outPanels = new List<TECPanel>(panels.Where(x => !systemPanels.Any(pair => pair.Value.Contains(x.Guid))));

            return (typicals, outControllers, outPanels);

            void addSystemChildrenToTypicalDict(Guid parentID, bool isTypical)
            {
                if (systemEquipment.ContainsKey(parentID))
                {
                    foreach (Guid equipID in systemEquipment[parentID])
                    {
                        typicalDictionary.Add(equipID, isTypical);
                        if (equipmentSubScope.ContainsKey(equipID))
                        {
                            foreach (Guid subID in equipmentSubScope[equipID])
                            {
                                typicalDictionary.Add(subID, isTypical);
                                if (subScopePoints.ContainsKey(subID))
                                {
                                    foreach (Guid pointID in subScopePoints[subID])
                                    {
                                        typicalDictionary.Add(pointID, isTypical);
                                    }
                                }
                            }
                        }
                    }
                }
                if (systemControllers.ContainsKey(parentID))
                {
                    foreach (Guid controllerID in systemControllers[parentID])
                    {
                        typicalDictionary.Add(controllerID, isTypical);
                        if (controllerConnections.ContainsKey(controllerID))
                        {
                            foreach (Guid connectionID in controllerConnections[controllerID])
                            {
                                typicalDictionary.Add(connectionID, isTypical);
                            }
                        }
                    }
                }
                if (systemPanels.ContainsKey(parentID))
                {
                    foreach (Guid panelGuid in systemPanels[parentID])
                    {
                        typicalDictionary.Add(panelGuid, isTypical);
                    }
                }
                if (systemScopeBranches.ContainsKey(parentID))
                {
                    foreach (Guid branchGuid in systemScopeBranches[parentID])
                    {
                        typicalDictionary.Add(branchGuid, isTypical);
                    }
                }
            }
            
            void setChildBranchIsTypical(Guid branchID, bool isTypical)
            {
                if (scopeBranchHierarchy.ContainsKey(branchID))
                { 
                    foreach (Guid childID in scopeBranchHierarchy[branchID])
                    {
                        typicalDictionary.Add(childID, isTypical);
                        setChildBranchIsTypical(childID, isTypical);
                    }
                }
            }
            void setSystemChildren(TECSystem item)
            {
                if (systemEquipment.ContainsKey(item.Guid))
                    item.Equipment = getRelatedReferences(systemEquipment[item.Guid], equipment).ToOC();
                if (systemControllers.ContainsKey(item.Guid))
                    item.SetControllers(getRelatedReferences(systemControllers[item.Guid], controllers));
                if (systemPanels.ContainsKey(item.Guid))
                    item.Panels = getRelatedReferences(systemPanels[item.Guid], panels).ToOC();
                if (systemMisc.ContainsKey(item.Guid))
                    item.MiscCosts = getRelatedReferences(systemMisc[item.Guid], misc).ToOC();
                if (systemScopeBranches.ContainsKey(item.Guid))
                    item.ScopeBranches = getRelatedReferences(systemScopeBranches[item.Guid], scopeBranches).ToOC();
                item.ScopeBranches.ForEach(branch => linkBranchHierarchy(branch, scopeBranches, scopeBranchHierarchy));
            }
        }
        private static TECCatalogs getCatalogs()
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Manufacturers = getObjectsFromTable(new ManufacturerTable(), id => new TECManufacturer(id)).ToOC();
            catalogs.ConnectionTypes = getObjectsFromTable(new ConnectionTypeTable(), id => new TECConnectionType(id)).ToOC();
            catalogs.ConduitTypes = getObjectsFromTable(new ConduitTypeTable(), id => new TECElectricalMaterial(id)).ToOC();
            Dictionary<Guid, List<TECConnectionType>> protocolConnectionType = getOneToManyRelationships(new ProtocolConnectionTypeTable(), catalogs.ConnectionTypes);
            catalogs.Protocols = getObjectsFromTable(new ProtocolTable(), id => new TECProtocol(id, protocolConnectionType[id])).ToOC();
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getOneToOneRelationships(new HardwareManufacturerTable(), catalogs.Manufacturers);
            Dictionary<Guid, List<TECConnectionType>> deviceConnectionType = getOneToManyRelationships(new DeviceConnectionTypeTable(), catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECProtocol>> deviceProtocols = getOneToManyRelationships(new DeviceProtocolTable(), catalogs.Protocols);
            catalogs.Devices = getObjectsFromTable(new DeviceTable(), id => new TECDevice(id, deviceConnectionType.ValueOrNew(id), deviceProtocols.ValueOrNew(id), hardwareManufacturer[id])).ToOC();
            Dictionary<Guid, TECDevice> actuators = getOneToOneRelationships(new ValveActuatorTable(), catalogs.Devices);
            catalogs.Valves = getObjectsFromTable(new ValveTable(), id => new TECValve(id, hardwareManufacturer[id], actuators[id])).ToOC();
            catalogs.AssociatedCosts = getObjectsFromTable(new AssociatedCostTable(), getAssociatedCostFromRow).ToOC();
            catalogs.PanelTypes = getObjectsFromTable(new PanelTypeTable(), data => getPanelTypeFromRow(data, hardwareManufacturer)).ToOC();
            catalogs.IOModules = getObjectsFromTable(new IOModuleTable(), id => new TECIOModule(id, hardwareManufacturer[id])).ToOC();
            catalogs.ControllerTypes = getObjectsFromTable(new ControllerTypeTable(), id => new TECControllerType(id, hardwareManufacturer[id])).ToOC();
            catalogs.Tags = getObjectsFromTable(new TagTable(), id => new TECTag(id)).ToOC();
            Dictionary<Guid, TECProtocol> ioProtocols = getOneToOneRelationships(new IOProtocolTable(), catalogs.Protocols);

            List<TECIO> io = getObjectsFromTable(new IOTable(), row => getIOFromRow(row, ioProtocols)).ToList();
            Dictionary<Guid, TECProtocol> ioProtocol = getOneToOneRelationships(new IOProtocolTable(), catalogs.Protocols);
            io.ForEach(x => { if (ioProtocol.ContainsKey(x.Guid)) { x.Protocol = ioProtocol[x.Guid]; } });

            Dictionary<Guid, List<TECAssociatedCost>> ratedCostsRelationShips = getOneToManyRelationships(new ElectricalMaterialRatedCostTable(), catalogs.AssociatedCosts);
            Dictionary<Guid, List<TECIOModule>> controllerTypeModuleRelationships = getOneToManyRelationships(new ControllerTypeIOModuleTable(), catalogs.IOModules);
            Dictionary<Guid, List<TECIO>> controllerTypeIORelationships = getOneToManyRelationships(new ControllerTypeIOTable(), io);
            Dictionary<Guid, List<TECIO>> moduleIORelationships = getOneToManyRelationships(new IOModuleIOTable(), io);

            catalogs.IOModules.ForEach(item => item.IO = moduleIORelationships.ValueOrNew(item.Guid));
            catalogs.ControllerTypes.ForEach(item => populateControllerTypeProperties(item, controllerTypeModuleRelationships, controllerTypeIORelationships));
            catalogs.ConnectionTypes.ForEach(item => item.RatedCosts = ratedCostsRelationShips.ValueOrNew(item.Guid));
            catalogs.ConduitTypes.ForEach(item => item.RatedCosts = ratedCostsRelationShips.ValueOrNew(item.Guid));

            return catalogs;
        }
        private static Dictionary<Guid, List<Guid>> getCharacteristicInstancesList()
        {
            Dictionary<Guid, List<Guid>> outDict = new Dictionary<Guid, List<Guid>>();
            DataTable dictDT = SQLiteDB.GetDataFromTable(TypicalInstanceTable.TableName);
            foreach (DataRow row in dictDT.Rows)
            {
                addRowToPlaceholderDict(row, outDict,
                    TypicalInstanceTable.TypicalID.Name, TypicalInstanceTable.InstanceID.Name);
            }
            return outDict;
        }
        private static Dictionary<Guid, List<Guid>> getTemplateReferences()
        {
            Dictionary<Guid, List<Guid>> outDict = new Dictionary<Guid, List<Guid>>();
            DataTable dictDT = SQLiteDB.GetDataFromTable(TemplateReferenceTable.TableName);
            foreach (DataRow row in dictDT.Rows)
            {
                addRowToPlaceholderDict(row, outDict,
                    TemplateReferenceTable.TemplateID.Name, TemplateReferenceTable.ReferenceID.Name);
            }
            return outDict;
        }
        private static TECSchedule getSchedule(TECBid bid)
        {
            var items = getObjectsFromTable(new ScheduleItemTable(), id => new TECScheduleItem(id));
            var tableItems = getOneToManyRelationships(new ScheduleTableScheduleItemTable(), items);
            var tables = getObjectsFromTable(new ScheduleTableTable(), id => new TECScheduleTable(id, tableItems.ValueOrNew(id)));
            var scheduleTables = getOneToManyRelationships(new ScheduleScheduleTableTable(), tables);

            DataTable DT = SQLiteDB.GetDataFromTable(ScheduleTable.TableName);
            if (DT.Rows.Count > 1)
            {
                logger.Error("Multiple rows found in schedule table. Using first found.");
            }
            else if (DT.Rows.Count < 1)
            {
                logger.Error("Schedule not found in database. Creating a new schedule.");
                return new TECSchedule();
            }
            TECSchedule schedule = getObjectFromRow(DT.Rows[0], new ScheduleTable(), id => new TECSchedule(id, scheduleTables.ValueOrNew(id)));

            var itemScope = getOneToOneRelationships(new ScheduleItemScopeTable(), bid.GetAll<TECScope>());
            items.ForEach(x => x.Scope = itemScope.ValueOrDefault(x.Guid, null));

            return schedule;
        }

        private static void populateScopeProperties(TECScope scope, Dictionary<Guid, List<TECTag>> tags, Dictionary<Guid, List<TECAssociatedCost>> costs)
        {
            if (tags.ContainsKey(scope.Guid))
            {
                List<TECTag> allTags = new List<TECTag>();
                tags[scope.Guid].ForEach(item => allTags.Add(item));
                scope.Tags = allTags.ToOC();
            }
            if (costs.ContainsKey(scope.Guid))
            {
                List<TECAssociatedCost> allCosts = new List<TECAssociatedCost>();
                costs[scope.Guid].ForEach(item => allCosts.Add(item));
                scope.AssociatedCosts = allCosts.ToOC();
            }
        }
        private static void populateSubScopeConnectionProperties(TECHardwiredConnection connection, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            connection.Child.SetParentConnection(connection);
            connection.ConduitType = connectionConduitTypes.ValueOrDefault(connection.Guid, null);
        }
        private static void populateNetworkConnectionProperties(TECNetworkConnection connection, Dictionary<Guid, List<IConnectable>> connectables,
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            if (connectables.ContainsKey(connection.Guid))
            {
                connectables[connection.Guid].ForEach(item => connection.Children.Add(item));
                connectables[connection.Guid].ForEach(item => item.SetParentConnection(connection));
            }
            connection.ConduitType = connectionConduitTypes.ValueOrDefault(connection.Guid, null);
        }
        private static void populateControllerTypeProperties(TECControllerType type, Dictionary<Guid, List<TECIOModule>> controllerTypeModuleRelationships, Dictionary<Guid, List<TECIO>> controllerTypeIORelationships)
        {
            if (controllerTypeModuleRelationships.ContainsKey(type.Guid))
            {
                controllerTypeModuleRelationships[type.Guid].ForEach(item => type.IOModules.Add(item));
            }
            if (controllerTypeIORelationships.ContainsKey(type.Guid))
            {
                controllerTypeIORelationships[type.Guid].ForEach(item => type.IO.Add(item));
            }
        }
        private static void linkBranchHierarchy(TECScopeBranch branch, IEnumerable<TECScopeBranch> branches, Dictionary<Guid, List<Guid>> scopeBranchHierarchy)
        {
            if (scopeBranchHierarchy.ContainsKey(branch.Guid))
            {
                branch.Branches = getRelatedReferences(scopeBranchHierarchy[branch.Guid], branches).ToOC();
                foreach (var subBranch in branch.Branches)
                {
                    linkBranchHierarchy(subBranch, branches, scopeBranchHierarchy);
                }
            }
        }
        
        private static void setupTemps()
        {
            tempManufacturer = new TECManufacturer();
            tempManufacturer.Label = "TEMPORARY";

            tempControllerType = new TECControllerType(tempManufacturer);
            TECIO input = new TECIO(IOType.UI);
            input.Quantity = 100;
            TECIO output = new TECIO(IOType.UO);
            output.Quantity = 100;
            tempControllerType.IO.Add(input);
            tempControllerType.IO.Add(output);

            tempPanelType = new TECPanelType(tempManufacturer);

            tempConnectionType = new TECConnectionType();
            tempProtocol = new TECProtocol(new List<TECConnectionType>() { tempConnectionType });
            tempProtocol.Label = "TEMPORARY";
            
            networkPoints = new Dictionary<string, List<TECPoint>>();

        }
        
        #region Data Handlers
        private static TECPanel getPanelFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = justUpdated ? panelTypes.ValueOrDefault(guid, tempPanelType) : panelTypes[guid];
            TECPanel panel = new TECPanel(guid, type, isTypical);
            assignValuePropertiesFromTable(panel, new PanelTable(), row);
            return panel;
        }
        private static TECPanel getPanelFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            return getPanelFromRow(row, isTypical, panelTypes);
        }

        private static TECProvidedController getProvidedControllerFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ProvidedControllerTable.ID.Name].ToString());
            TECControllerType type = justUpdated ? controllerTypes.ValueOrDefault(guid, tempControllerType) : controllerTypes[guid];
            TECProvidedController controller = new TECProvidedController(guid, type, isTypical);
            assignValuePropertiesFromTable(controller, new ProvidedControllerTable(), row);
            return controller;
        }
        private static TECProvidedController getProvidedControllerFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ProvidedControllerTable.ID.Name].ToString());
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            return getProvidedControllerFromRow(row, isTypical, controllerTypes);
        }

        private static TECIO getIOFromRow(DataRow row, Dictionary<Guid, TECProtocol> protocols)
        {
            Guid guid = new Guid(row[IOTable.ID.Name].ToString());
            IOType type = UtilitiesMethods.StringToEnum<IOType>(row[IOTable.IOType.Name].ToString());
            TECIO io;
            if(type == IOType.Protocol)
            {
                io = new TECIO(guid, protocols[guid]);
            }
            else
            {
                io = new TECIO(guid, type);
            }
            assignValuePropertiesFromTable(io, new IOTable(), row);
            return io;
        }
        
        private static TECMisc getMiscFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[MiscTable.ID.Name].ToString());
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[MiscTable.Type.Name].ToString());
            TECMisc cost = new TECMisc(guid, type, isTypical);
            assignValuePropertiesFromTable(cost, new MiscTable(), row);
            return cost;
        }
        private static TECMisc getMiscFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary)
        {
            Guid guid = new Guid(row[MiscTable.ID.Name].ToString());
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            return getMiscFromRow(row, isTypical);
        }

        private static TECAssociatedCost getAssociatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[AssociatedCostTable.ID.Name].ToString());
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[AssociatedCostTable.Type.Name].ToString());
            var associatedCost = new TECAssociatedCost(guid, type);
            assignValuePropertiesFromTable(associatedCost, new AssociatedCostTable(), row);
            return associatedCost;
        }
        private static TECPanelType getPanelTypeFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid guid = new Guid(row[PanelTypeTable.ID.Name].ToString());
            TECManufacturer manufacturer = justUpdated ? manufacturers.ValueOrDefault(guid, tempManufacturer) : manufacturers[guid];
            TECPanelType panelType = new TECPanelType(guid, manufacturer);
            assignValuePropertiesFromTable(panelType, new PanelTypeTable(), row);
            return panelType;
        }
        
        private static void addRowToPlaceholderDict(DataRow row, Dictionary<Guid, List<Guid>> dict, string keyString, string valueString)
        {
            Guid key = new Guid(row[keyString].ToString());
            Guid value = new Guid(row[valueString].ToString());

            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<Guid>();
            }
            dict[key].Add(value);
        }

        private static T getObjectFromRow<T>(DataRow row, TableBase table, Func<Guid, T> constructor)
        {
            if (table.PrimaryKeys.Count != 1) { throw new Exception("Table must have one primary key."); }

            Guid guid = new Guid(row[table.PrimaryKeys[0].Name].ToString());
            T newObject = constructor(guid);
            assignValuePropertiesFromTable(newObject, table, row);
            return newObject;
        }
        private static void assignValuePropertiesFromTable(object item, TableBase table, DataRow row)
        {
            foreach (TableField field in table.Fields)
            {
                if (field.Property.DeclaringType.IsInstanceOfType(item) && field.Property.SetMethod != null)
                {
                    if (field.Property.PropertyType == typeof(string))
                    {
                        field.Property.SetValue(item, row[field.Name].ToString());
                    }
                    else if (field.Property.PropertyType == typeof(bool))
                    {
                        field.Property.SetValue(item, row[field.Name].ToString().ToInt(0).ToBool());
                    }
                    else if (field.Property.PropertyType == typeof(int))
                    {
                        field.Property.SetValue(item, row[field.Name].ToString().ToInt());
                    }
                    else if (field.Property.PropertyType == typeof(double))
                    {
                        field.Property.SetValue(item, row[field.Name].ToString().ToDouble(0));
                    }
                    else if (field.Property.PropertyType == typeof(Confidence))
                    {
                        field.Property.SetValue(item, UtilitiesMethods.StringToEnum<Confidence>(row[field.Name].ToString()));
                    }
                    else if (field.Property.PropertyType == typeof(DateTime))
                    {
                        string dueDateString = row[field.Name].ToString();
                        field.Property.SetValue(item, DateTime.ParseExact(dueDateString, DB_FMT, CultureInfo.InvariantCulture));
                    }
                    else if (field.Property.PropertyType == typeof(IOType))
                    {
                        string typeString = row[field.Name].ToString();
                        if (typeString == "BO")
                        {
                            typeString = "DO";
                        }
                        else if (typeString == "BI")
                        {
                            typeString = "DI";
                        }
                        if ((new List<String>{"AI", "AO", "DI", "DO" }).Contains(typeString))
                        {
                            field.Property.SetValue(item, UtilitiesMethods.StringToEnum<IOType>(typeString));
                        }
                    }
                }
            }
        }
        #endregion

        #region Generic Database Query Methods
        private static DataTable getRelationshipData(TableBase table)
        {
            string orderKey = table.IndexString;
            string orderString = orderKey != "" ? orderString = string.Format(" order by {0}", orderKey) : "";
            string command = string.Format("select {0} from {1}{2}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString,
                orderString);
            return SQLiteDB.GetDataFromCommand(command);
        }
        private static (string, string) getRelationshipKeys(TableBase table)
        {
            if (table.PrimaryKeys.Count != 2)
            {
                throw new Exception("Table must have two primary keys");
            }
            return (table.PrimaryKeys[0].Name, table.PrimaryKeys[1].Name);
        }
        private static void relationshipIterator(TableBase table, Action<Guid, Guid> action)
        {
            (string parentField, string childField) = getRelationshipKeys(table);
            string qtyField = table.QuantityString;
            var data = getRelationshipData(table);
            foreach (DataRow row in data.Rows)
            {
                Guid parentID = new Guid(row[parentField].ToString());
                Guid childID = new Guid(row[childField].ToString());
                int quantity = 1;
                if (qtyField != "")
                {
                    quantity = row[qtyField].ToString().ToInt();
                }
                for (int x = 0; x < quantity; x++)
                {
                    action(parentID, childID);
                }
            }
        }
        private static Dictionary<Guid, List<T>> getOneToManyRelationships<T>(TableBase table, IEnumerable<T> references) where T : ITECObject
        {
            Dictionary<Guid, List<T>> dictionary = new Dictionary<Guid, List<T>>();
            relationshipIterator(table, addPairToDictionary);
            return dictionary;

            void addPairToDictionary(Guid parentID, Guid childID)
            {
                if (references.Count(x => x.Guid == childID) > 0)
                {
                    T reference = references.First(item => item.Guid == childID);
                    if (dictionary.ContainsKey(parentID))
                    {
                        dictionary[parentID].Add(reference);
                    }
                    else
                    {
                        dictionary[parentID] = new List<T> { reference };
                    }
                }
            }
        }
        private static Dictionary<Guid, T> getChildIDToParentRelationships<T>(TableBase table, IEnumerable<T> parentReferences) where T : TECObject
        {
            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            relationshipIterator(table, addPairToDictionary);
            return dictionary;

            void addPairToDictionary(Guid parentID, Guid childID)
            {
                T reference = parentReferences.First(item => item.Guid == parentID);
                dictionary[childID] = reference;
            }
        }
        private static Dictionary<Guid, List<Guid>> getOneToManyRelationships(TableBase table) 
        {
            Dictionary<Guid, List<Guid>> dictionary = new Dictionary<Guid, List<Guid>>();
            relationshipIterator(table, addPairToDictionary);
            return dictionary;

            void addPairToDictionary(Guid parentID, Guid childID)
            {
                if (dictionary.ContainsKey(parentID))
                {
                    dictionary[parentID].Add(childID);
                }
                else
                {
                    dictionary[parentID] = new List<Guid> { childID };
                }
            }
        }
        private static Dictionary<Guid, T> getOneToOneRelationships<T>(TableBase table, IEnumerable<T> references) where T : ITECObject
        {
            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            relationshipIterator(table, addPairToDictionary);
            return dictionary;

            void addPairToDictionary(Guid parentID, Guid childID)
            {
                T outItem = references.First(item => item.Guid == childID);
                dictionary[parentID] = outItem;
            }
        }
        private static Dictionary<Guid, Guid> getOneToOneRelationships(TableBase table)
        {
            Dictionary<Guid, Guid> dictionary = new Dictionary<Guid, Guid>();
            relationshipIterator(table, addPairToDictionary);
            return dictionary;

            void addPairToDictionary(Guid parentID, Guid childID)
            {
                dictionary[parentID] = childID;
            }
        }
        static private List<T> getObjectsFromTable<T>(TableBase table, Func<DataRow, T> dataHandler)
        {
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(table), table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            return new List<T>(getObjectsFromData(table, data, dataHandler));
        }
        static private List<T> getObjectsFromTable<T>(TableBase table, Func<Guid, T> constructor)
        {
            return getObjectsFromTable(table, data => { return getObjectFromRow(data, table, constructor); });
        }
        public static T getObjectFromTable<T>(TableBase table, Func<DataRow, T> dataHandler, T defaultValue)
        {
            DataTable data = SQLiteDB.GetDataFromTable(table.NameString);
            if (data.Rows.Count < 1)
            {
                logger.Error("Object not found in database.");
                return defaultValue;
            }
            else if (data.Rows.Count > 1)
            {
                logger.Error("Multiple rows found in table. Using first found.");
            }

            DataRow row = data.Rows[0];
            return dataHandler(row);
        }
        public static T getObjectFromTable<T>(TableBase table, Func<Guid, T> constructor, T defaultValue)
        {
            return getObjectFromTable(table, data => { return getObjectFromRow(data, table, constructor); }, defaultValue);
        }
        static private List<T> getChildObjects<T>(TableBase relationTable, TableBase childTable, Guid parentID, Func<DataRow, T> dataHandler)
        {
            DataTable data = getChildData(relationTable, childTable, parentID);
            return new List<T>(getObjectsFromData(childTable, data, dataHandler));
        }
        static private List<T> getChildObjects<T>(TableBase relationTable, TableBase childTable, Guid parentID, Func<Guid, T> constructor)
        {
            DataTable data = getChildData(relationTable, childTable, parentID);
            return new List<T>(getObjectsFromData(childTable, data, row => getObjectFromRow(row, childTable, constructor)));
        }
        private static DataTable getChildData(TableBase relationTable, TableBase childTable, Guid parentID)
        {
            string orderKey = relationTable.IndexString;
            string orderString = "";
            if (orderKey != "")
            {
                orderString = string.Format(" order by {0}", orderKey);
            }
            if (relationTable.PrimaryKeys.Count != 2)
            {
                throw new Exception("Relation table must have primary keys for each object");
            }
            if (childTable.PrimaryKeys.Count != 1)
            {
                throw new Exception("Child object table must haveone primary key");
            }
            string command = string.Format("select {0} from {1} join {2} on {3} = {4} AND {5} = '{6}' {7}",
                DatabaseHelper.AllFieldsInTableString(childTable),
                childTable.NameString,
                relationTable.NameString,
                childTable.PrimaryKeys[0].Name,
                relationTable.PrimaryKeys[1].Name,
                relationTable.PrimaryKeys[0].Name,
                parentID.ToString(),
                orderString);
            return SQLiteDB.GetDataFromCommand(command);
        }
        private static List<T> getObjectsFromData<T>(TableBase table, DataTable data, Func<DataRow, T> dataHandler)
        {
            List<T> objs = new List<T>();
            foreach (DataRow row in data.Rows)
            {
                objs.Add(dataHandler(row));
            }
            return objs;
        }
        private static List<T> getObjectsFromData<T>(TableBase table, DataTable data, Func<Guid, T> constructor)
        {
            return getObjectsFromData(table, data, row => { return getObjectFromRow(row, table, constructor); });
        }
        #endregion

        #region Database Cleaning Methods
        private static void removeRelationshipFragment(TableBase relationTable, TableBase objectTable, string objectKey)
        {
            if(objectTable.PrimaryKeys.Count != 1)
            {
                throw new Exception("Object table must have one primary key.");
            }
            string objectTableKey = objectTable.PrimaryKeys[0].Name;
            string command = String.Format("delete from {0} where {1} in (select {1} from {0} where {1} not in (select {2} from {3}))",
                relationTable.NameString, objectKey, objectTableKey, objectTable.NameString);

            SQLiteDB.NonQueryCommand(command);
        }
        #endregion
        
        private static List<T> getRelatedReferences<T>(List<Guid> relationships, IEnumerable<T> references) where T: ITECObject
        {
            List<T> list = new List<T>();
            foreach (Guid id in relationships)
            {
                foreach (T reference in references)
                {
                    if (reference.Guid == id)
                    {
                        list.Add(reference);
                        break;
                    }
                }
            }
            return list;
        }
        private static T getRelatedReference<T>(Guid relationship, IEnumerable<T> references) where T: ITECObject
        {
            foreach (T reference in references)
            {
                if (reference.Guid == relationship)
                {
                    return reference;
                }
            }
            return default(T);
        }
    }
}
