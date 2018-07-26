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
            bid.Locations.AddRange(bidLocations.ValueOrNew(bid.Guid));
            Dictionary<Guid, TECLocation> locationDictionary = getOneToOneRelationships(new LocatedLocationTable(), locations);

            List<TECScopeBranch> branches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id));
            Dictionary<Guid, List<TECScopeBranch>> branchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable(), branches);
            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(), bid.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(), bid.Catalogs.AssociatedCosts);

            var parameters = getObjectsFromTable(new ParametersTable(), id => new TECParameters(id));
            bid.Parameters = parameters.First(x => x.Guid == bid.Guid);
            bid.ExtraLabor = getObjectFromTable(new ExtraLaborTable(), id => { return new TECExtraLabor(id); }, new TECExtraLabor(bid.Guid));
            bid.ScopeTree.AddRange(getChildObjects(new BidScopeBranchTable(), new ScopeBranchTable(), bid.Guid, id => new TECScopeBranch(id)));
            bid.ScopeTree.ForEach(item => linkBranchHierarchy(item, branches, branchHierarchy));
            (var typicals, var controllers, var panels, var typicalSystems, var instances)  = getScopeHierarchy(bid.Guid, bid.Catalogs);
            bid.Systems.AddRange(typicals);
            bid.SetControllers(controllers);
            bid.Panels.AddRange(panels);
            bid.Notes.AddRange(getObjectsFromTable(new NoteTable(), id => new TECLabeled(id)));
            bid.Exclusions.AddRange(getObjectsFromTable(new ExclusionTable(), id => new TECLabeled(id)));
            bid.MiscCosts.AddRange(getChildObjects(new BidMiscTable(), new MiscTable(), bid.Guid, data => getMiscFromRow(data)));
            bid.Schedule = getSchedule(bid);
            bid.InternalNotes.AddRange(getChildObjects(new BidInternalNoteTable(), new InternalNoteTable(), bid.Guid, id => { return new TECInternalNote(id); }));

            List<TECLocated> allLocated = bid.GetAll<TECLocated>();
            instances.ForEach(x => allLocated.AddRange(x.GetAll<TECLocated>()));
            allLocated.ForEach(item => item.Location = locationDictionary.ValueOrDefault(item.Guid, null));
            List<TECTagged> allTagged = bid.GetAll<TECTagged>();
            instances.ForEach(x => allTagged.AddRange(x.GetAll<TECTagged>()));

            foreach (var item in allTagged)
            {
                if( item is TECScope scope)
                {
                    populateScopeProperties(scope, tagRelationships, costRelationships);
                }
                else
                {
                    populateTaggedProperties(item, tagRelationships);
                }
            }

            foreach (TECTypical item in bid.Systems)
            {
                if (typicalSystems.ContainsKey(item.Guid))
                    item.Instances.AddRange(getRelatedReferences(typicalSystems[item.Guid], instances));
            }
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

            List<TECTagged> allTagged = templates.GetAll<TECTagged>();
            foreach (var item in allTagged)
            {
                if (item is TECScope scope)
                {
                    populateScopeProperties(scope, tagRelationships, costRelationships);
                }
                else
                {
                    populateTaggedProperties(item, tagRelationships);
                }
            }

            Dictionary<Guid, List<Guid>> templateReferences = getTemplateReferences();
            bool needsSave = ModelLinkingHelper.LinkLoadedTemplates(templates, templateReferences);
            return (templates, needsSave);
        }
        
        private static void getScopeManagerProperties(TECScopeManager scopeManager)
        {
            scopeManager.Catalogs = getCatalogs();
            scopeManager.Templates = getScopeTemplates(scopeManager.Catalogs);
            if (justUpdated)
            {
                scopeManager.Catalogs.Manufacturers.Add(tempManufacturer);
                scopeManager.Catalogs.PanelTypes.Add(tempPanelType);
                scopeManager.Catalogs.ControllerTypes.Add(tempControllerType);
                scopeManager.Catalogs.ConnectionTypes.Add(tempConnectionType);
                scopeManager.Catalogs.Protocols.Add(tempProtocol);
            }
        }
        private static (List<TECTypical> typicals, List<TECController> controllers, List<TECPanel> panels, Dictionary<Guid, List<Guid>> typicalSystems, List<TECSystem> instances) getScopeHierarchy(Guid bidID, TECCatalogs catalogs)
        {
            #region Catalogs

            Dictionary<Guid, TECControllerType> controllerTypes = getOneToOneRelationships(new ProvidedControllerControllerTypeTable(), catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypes = getOneToOneRelationships(new PanelPanelTypeTable(), catalogs.PanelTypes);
            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ProvidedControllerIOModuleTable(), catalogs.IOModules);
            List<IEndDevice> allEndDevices = new List<IEndDevice>(catalogs.Devices);
            allEndDevices.AddRange(catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), catalogs.ConduitTypes);
            Dictionary<Guid, TECProtocol> connectionProtocols = getOneToOneRelationships(new NetworkConnectionProtocolTable(), catalogs.Protocols);
            Dictionary<Guid, List<TECConnectionType>> hardwiredConnectionTypes = getOneToManyRelationships(new HardwiredConnectionConnectionTypeTable(), catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECConnectionType>> interlockConnectionTypes = getOneToManyRelationships(new InterlockConnectionConnectionTypeTable(), catalogs.ConnectionTypes);
            #endregion
            
            Dictionary<Guid, List<Guid>> bidTypicals = getOneToManyRelationships(new BidSystemTable());
            List<Guid> typicalIDs = bidTypicals.Count > 0 ? bidTypicals[bidID] : new List<Guid>();
            Dictionary<Guid, List<Guid>> typicalSystems = getOneToManyRelationships(new SystemHierarchyTable());

            DataTable allSystemData = SQLiteDB.GetDataFromTable(SystemTable.TableName);
            var typicalRows = from row in allSystemData.AsEnumerable() where typicalIDs.Contains(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            var systemRows = from row in allSystemData.AsEnumerable() where !typicalIDs.Contains(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            DataTable typicalData = typicalRows.Any() ? typicalRows.CopyToDataTable() : new DataTable();
            DataTable systemData = systemRows.Any() ? systemRows.CopyToDataTable() : new DataTable();
            List<TECSystem> systems = getObjectsFromData(new SystemTable(), systemData, id => new TECSystem(id));
            List<TECTypical> typicals = getObjectsFromData(new SystemTable(), typicalData, id => new TECTypical(id));

            List<TECPoint> points = getObjectsFromTable(new PointTable(), id => new TECPoint(id));
            List<TECSubScope> subScope = getObjectsFromTable(new SubScopeTable(), id => new TECSubScope(id));
            List<TECEquipment> equipment = getObjectsFromTable(new EquipmentTable(), id => new TECEquipment(id));
            List<TECMisc> misc = getObjectsFromTable(new MiscTable(), row => { return getMiscFromRow(row); });
            List<TECScopeBranch> scopeBranches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id));
            List<TECProvidedController> providedControllers = getObjectsFromTable(new ProvidedControllerTable(), data => getProvidedControllerFromRow(data, controllerTypes));
            List<TECFBOController> fboControllers = getObjectsFromTable(new FBOControllerTable(), id => new TECFBOController(id, catalogs));
            List<TECController> allControllers = new List<TECController>(fboControllers);
            allControllers.AddRange(providedControllers);
            List<TECPanel> panels = getObjectsFromTable(new PanelTable(), row => getPanelFromRow(row, panelTypes));
            List<TECInterlockConnection> interlockConnections = getObjectsFromTable(new InterlockConnectionTable(),
                id => new TECInterlockConnection(id, interlockConnectionTypes.ValueOrNew(id)));

            Dictionary<Guid, List<TECEquipment>> systemEquipment = getOneToManyRelationships(new SystemEquipmentTable(), equipment);
            Dictionary<Guid, List<TECSubScope>> equipmentSubScope = getOneToManyRelationships(new EquipmentSubScopeTable(), subScope);
            Dictionary<Guid, List<TECPoint>> subScopePoints = getOneToManyRelationships(new SubScopePointTable(), points);
            Dictionary<Guid, List<TECController>> systemControllers = getOneToManyRelationships(new SystemControllerTable(), allControllers);
            Dictionary<Guid, List<TECPanel>> systemPanels = getOneToManyRelationships(new SystemPanelTable(), panels);
            Dictionary<Guid, List<TECMisc>> systemMisc = getOneToManyRelationships(new SystemMiscTable(), misc);
            Dictionary<Guid, List<TECScopeBranch>> systemScopeBranches = getOneToManyRelationships(new SystemScopeBranchTable(), scopeBranches);
            Dictionary<Guid, List<TECScopeBranch>> scopeBranchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable(), scopeBranches);
            Dictionary<Guid, List<TECScopeBranch>> subScopeScopeBranch = getOneToManyRelationships(new SubScopeScopeBranchTable(), scopeBranches);

            Dictionary<Guid, TECEquipment> proposalItemDisplayScope = getOneToOneRelationships(new ProposalItemDisplayScopeTable(), equipment);
            List<TECProposalItem> proposalItems = getObjectsFromTable(new ProposalItemTable(), id => new TECProposalItem(id, proposalItemDisplayScope[id]));
            Dictionary<Guid, List<TECEquipment>> proposalItemScope = getOneToManyRelationships(new ProposalItemContainingScopeTable(), equipment);
            Dictionary<Guid, List<TECProposalItem>> systemProposalItems = getOneToManyRelationships(new SystemProposalItemTable(), proposalItems);
            Dictionary<Guid, List<TECInterlockConnection>> subScopeInterlocks = getOneToManyRelationships(new InterlockableInterlockTable(), interlockConnections);
            proposalItems.ForEach(x => x.ContainingScope.AddRange(proposalItemScope.ValueOrNew(x.Guid)));
            
            List<IConnectable> connectables = new List<IConnectable>(allControllers);
            connectables.AddRange(subScope);

            Dictionary<Guid, List<Guid>> networkChildren = getOneToManyRelationships(new NetworkConnectionChildrenTable());
            Dictionary<Guid, Guid> subScopeConnectionChildren = getOneToOneRelationships(new HardwiredConnectionChildrenTable());
            Dictionary<Guid, List<Guid>> controllerConnections = getOneToManyRelationships(new ControllerConnectionTable());
            Dictionary<Guid, TECController> connectionParents = getChildIDToParentRelationships(new ControllerConnectionTable(), allControllers);
            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new HardwiredConnectionChildrenTable(), subScope);

            List<TECHardwiredConnection> subScopeConnections = getObjectsFromTable(new SubScopeConnectionTable(), 
                id => new TECHardwiredConnection(id, subScopeConnectionChildrenRelationships[id], connectionParents[id],
                new TECHardwiredProtocol(hardwiredConnectionTypes.ValueOrNew(id))));
            List<TECNetworkConnection> networkConnections = getObjectsFromTable(new NetworkConnectionTable(), 
                id => new TECNetworkConnection(id, connectionParents[id], connectionProtocols[id]));
            
            List<IControllerConnection> allConnections = new List<IControllerConnection>(subScopeConnections);
            allConnections.AddRange(networkConnections);
            
            Dictionary<Guid, List<TECController>> panelControllers = getOneToManyRelationships(new PanelControllerTable(), allControllers);

            allControllers.ForEach(item => { if (item is TECProvidedController provided) provided.IOModules = controllerModuleRelationships.ValueOrNew(provided.Guid); });
            subScope.ForEach(item => item.Devices.AddRange(endDevices.ValueOrNew(item.Guid)));
            
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
                item.Points.AddRange(subScopePoints.ValueOrNew(item.Guid));
                item.Interlocks.AddRange(subScopeInterlocks.ValueOrNew(item.Guid));
            }
            foreach(TECEquipment item in equipment.Where(x => equipmentSubScope.ContainsKey(x.Guid)))
            {
                item.SubScope.AddRange(equipmentSubScope[item.Guid]);
            }
            foreach(TECController item in allControllers.Where(x => controllerConnections.ContainsKey(x.Guid)))
            {
                item.ChildrenConnections.AddRange(getRelatedReferences(controllerConnections[item.Guid], allConnections));
            }
            panels.ForEach(item => item.Controllers = panelControllers.ValueOrNew(item.Guid));
            foreach(TECSystem item in systems)
            {
                setSystemChildren(item);
            }
            foreach(TECTypical item in typicals)
            {
                setSystemChildren(item);
            }

            var outControllers = new List<TECController>(allControllers.Where(x => !systemControllers.Any(pair => pair.Value.Contains(x))));
            var outPanels = new List<TECPanel>(panels.Where(x => !systemPanels.Any(pair => pair.Value.Contains(x))));

            return (typicals, outControllers, outPanels, typicalSystems, systems);
            
            void setSystemChildren(TECSystem item)
            {
                item.Equipment.AddRange(systemEquipment.ValueOrNew(item.Guid));
                item.SetControllers(systemControllers.ValueOrNew(item.Guid));
                item.Panels.AddRange(systemPanels.ValueOrNew(item.Guid));
                item.MiscCosts.AddRange(systemMisc.ValueOrNew(item.Guid));
                item.ScopeBranches.AddRange(systemScopeBranches.ValueOrNew(item.Guid));
                item.ScopeBranches.ForEach(branch => linkBranchHierarchy(branch, scopeBranches, scopeBranchHierarchy));
                item.ProposalItems.AddRange(systemProposalItems.ValueOrNew(item.Guid));

            }
        }
        private static TECCatalogs getCatalogs()
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Manufacturers.AddRange(getObjectsFromTable(new ManufacturerTable(), id => new TECManufacturer(id)));
            catalogs.ConnectionTypes.AddRange(getObjectsFromTable(new ConnectionTypeTable(), id => new TECConnectionType(id)));
            catalogs.ConduitTypes.AddRange(getObjectsFromTable(new ConduitTypeTable(), id => new TECElectricalMaterial(id)));
            Dictionary<Guid, List<TECConnectionType>> protocolConnectionType = getOneToManyRelationships(new ProtocolConnectionTypeTable(), catalogs.ConnectionTypes);
            catalogs.Protocols.AddRange(getObjectsFromTable(new ProtocolTable(), id => new TECProtocol(id, protocolConnectionType[id])));
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getOneToOneRelationships(new HardwareManufacturerTable(), catalogs.Manufacturers);
            Dictionary<Guid, List<TECConnectionType>> deviceConnectionType = getOneToManyRelationships(new DeviceConnectionTypeTable(), catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECProtocol>> deviceProtocols = getOneToManyRelationships(new DeviceProtocolTable(), catalogs.Protocols);
            catalogs.Devices.AddRange(getObjectsFromTable(new DeviceTable(), id => new TECDevice(id, deviceConnectionType.ValueOrNew(id), deviceProtocols.ValueOrNew(id), hardwareManufacturer[id])));
            Dictionary<Guid, TECDevice> actuators = getOneToOneRelationships(new ValveActuatorTable(), catalogs.Devices);
            catalogs.Valves.AddRange(getObjectsFromTable(new ValveTable(), id => new TECValve(id, hardwareManufacturer[id], actuators[id])));
            catalogs.AssociatedCosts.AddRange(getObjectsFromTable(new AssociatedCostTable(), getAssociatedCostFromRow));
            catalogs.PanelTypes.AddRange(getObjectsFromTable(new PanelTypeTable(), data => getPanelTypeFromRow(data, hardwareManufacturer)));
            catalogs.IOModules.AddRange(getObjectsFromTable(new IOModuleTable(), id => new TECIOModule(id, hardwareManufacturer[id])));
            catalogs.ControllerTypes.AddRange(getObjectsFromTable(new ControllerTypeTable(), id => new TECControllerType(id, hardwareManufacturer[id])));
            catalogs.Tags.AddRange(getObjectsFromTable(new TagTable(), id => new TECTag(id)));
            Dictionary<Guid, TECProtocol> ioProtocols = getOneToOneRelationships(new IOProtocolTable(), catalogs.Protocols);

            List<TECIO> io = getObjectsFromTable(new IOTable(), row => getIOFromRow(row, ioProtocols)).ToList();
            Dictionary<Guid, TECProtocol> ioProtocol = getOneToOneRelationships(new IOProtocolTable(), catalogs.Protocols);
            io.ForEach(x => { if (ioProtocol.ContainsKey(x.Guid)) { x.Protocol = ioProtocol[x.Guid]; } });

            Dictionary<Guid, List<TECAssociatedCost>> ratedCostsRelationShips = getOneToManyRelationships(new ElectricalMaterialRatedCostTable(), catalogs.AssociatedCosts);
            Dictionary<Guid, List<TECIOModule>> controllerTypeModuleRelationships = getOneToManyRelationships(new ControllerTypeIOModuleTable(), catalogs.IOModules);
            Dictionary<Guid, List<TECIO>> controllerTypeIORelationships = getOneToManyRelationships(new ControllerTypeIOTable(), io);
            Dictionary<Guid, List<TECIO>> moduleIORelationships = getOneToManyRelationships(new IOModuleIOTable(), io);

            catalogs.IOModules.ForEach(item => item.IO.AddRange(moduleIORelationships.ValueOrNew(item.Guid)));
            catalogs.ControllerTypes.ForEach(item => populateControllerTypeProperties(item, controllerTypeModuleRelationships, controllerTypeIORelationships));
            catalogs.ConnectionTypes.ForEach(item => item.RatedCosts.AddRange(ratedCostsRelationShips.ValueOrNew(item.Guid)));
            catalogs.ConduitTypes.ForEach(item => item.RatedCosts.AddRange(ratedCostsRelationShips.ValueOrNew(item.Guid)));
            
            return catalogs;
        }
        private static ScopeTemplates getScopeTemplates(TECCatalogs catalogs)
        {
            ScopeTemplates templates = new ScopeTemplates();
            templates = getObjectFromTable(new ScopeTemplatesTable(), id => { return new ScopeTemplates(id); }, new ScopeTemplates());
            
            List<IEndDevice> allEndDevices = new List<IEndDevice>(catalogs.Devices);
            allEndDevices.AddRange(catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), catalogs.ConduitTypes);
            Dictionary<Guid, List<TECIOModule>> providedControllerModuleRelationships = getOneToManyRelationships(new ProvidedControllerIOModuleTable(), catalogs.IOModules);

            Dictionary<Guid, TECControllerType> providedControllerTypeDictionary = getOneToOneRelationships(new ProvidedControllerControllerTypeTable(), catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(), catalogs.PanelTypes);
            Dictionary<Guid, TECProtocol> connectionProtocol = getOneToOneRelationships(new NetworkConnectionProtocolTable(), catalogs.Protocols);
            Dictionary<Guid, List<TECConnectionType>> hardwiredConnectionTypes = getOneToManyRelationships(new HardwiredConnectionConnectionTypeTable(), catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECConnectionType>> interlockConnectionTypes = getOneToManyRelationships(new InterlockConnectionConnectionTypeTable(), catalogs.ConnectionTypes);


            List<TECSystem> systems = getObjectsFromTable(new SystemTable(), id => new TECSystem(id));
            List<TECEquipment> equipment = getObjectsFromTable(new EquipmentTable(), id => new TECEquipment(id));
            List<TECSubScope> subScope = getObjectsFromTable(new SubScopeTable(), id => new TECSubScope(id));
            List<TECPoint> points = getObjectsFromTable(new PointTable(), id => new TECPoint(id));
            List<TECMisc> misc = getObjectsFromTable(new MiscTable(), data => getMiscFromRow(data));
            List<TECProvidedController> providedControllers = getObjectsFromTable(new ProvidedControllerTable(), data => getProvidedControllerFromRow(data, providedControllerTypeDictionary));
            List<TECFBOController> fboControllers = getObjectsFromTable(new FBOControllerTable(), id => new TECFBOController(id, catalogs));
            List<TECController> controllers = new List<TECController>(providedControllers);
            controllers.AddRange(fboControllers);
            List<TECPanel> panels = getObjectsFromTable(new PanelTable(), data => getPanelFromRow(data, panelTypeDictionary));
            List<TECParameters> parameters = getObjectsFromTable(new ParametersTable(), id => new TECParameters(id));
            List<TECInterlockConnection> interlockConnections = getObjectsFromTable(new InterlockConnectionTable(),
                id => new TECInterlockConnection(id, interlockConnectionTypes.ValueOrNew(id)));

            Dictionary<Guid, TECController> connectionParents = getChildIDToParentRelationships(new ControllerConnectionTable(), controllers);

            List<IConnectable> allNetworkConnectable = new List<IConnectable>(subScope);
            allNetworkConnectable.AddRange(controllers);
            Dictionary<Guid, List<IConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(), allNetworkConnectable);
            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new HardwiredConnectionChildrenTable(), subScope);

            List<TECHardwiredConnection> subScopeConnections = getObjectsFromTable(new SubScopeConnectionTable(), id => new TECHardwiredConnection(id, subScopeConnectionChildrenRelationships[id],
                connectionParents[id], new TECHardwiredProtocol(hardwiredConnectionTypes[id])));
            List<TECNetworkConnection> networkConnections = getObjectsFromTable(new NetworkConnectionTable(), id => new TECNetworkConnection(id, connectionParents[id], connectionProtocol[id]));
            List<IControllerConnection> connections = new List<IControllerConnection>(subScopeConnections);
            connections.AddRange(networkConnections);

            List<TECScopeBranch> scopeBranches = getObjectsFromTable(new ScopeBranchTable(), id => new TECScopeBranch(id));
            Dictionary<Guid, List<TECScopeBranch>> branchHierarchy = getOneToManyRelationships(new ScopeBranchHierarchyTable(), scopeBranches);
            scopeBranches.ForEach(x => linkBranchHierarchy(x, scopeBranches, branchHierarchy));

            Dictionary<Guid, List<TECEquipment>> systemEquipment = getOneToManyRelationships(new SystemEquipmentTable(), equipment);
            Dictionary<Guid, List<TECController>> systemController = getOneToManyRelationships(new SystemControllerTable(), controllers);
            Dictionary<Guid, List<TECPanel>> systemPanels = getOneToManyRelationships(new SystemPanelTable(), panels);
            Dictionary<Guid, List<TECMisc>> systemMisc = getOneToManyRelationships(new SystemMiscTable(), misc);
            Dictionary<Guid, List<TECScopeBranch>> systemScopeBranch = getOneToManyRelationships(new SystemScopeBranchTable(), scopeBranches);
            Dictionary<Guid, List<TECSubScope>> equipmentSubScope = getOneToManyRelationships(new EquipmentSubScopeTable(), subScope);
            Dictionary<Guid, List<TECPoint>> subScopePoint = getOneToManyRelationships(new SubScopePointTable(), points);
            Dictionary<Guid, List<IControllerConnection>> controllerConnection = getOneToManyRelationships(new ControllerConnectionTable(), connections);
            Dictionary<Guid, List<TECParameters>> templateParameters = getOneToManyRelationships(new TemplatesParametersTable(), parameters);
            Dictionary<Guid, List<TECInterlockConnection>> subScopeInterlocks = getOneToManyRelationships(new InterlockableInterlockTable(), interlockConnections);
            Dictionary<Guid, List<TECScopeBranch>> subScopeScopeBranch = getOneToManyRelationships(new SubScopeScopeBranchTable(), scopeBranches);
            
            Dictionary<Guid, TECEquipment> proposalItemDisplayScope = getOneToOneRelationships(new ProposalItemDisplayScopeTable(), equipment);
            List<TECProposalItem> proposalItems = getObjectsFromTable(new ProposalItemTable(), id => new TECProposalItem(id, proposalItemDisplayScope[id]));
            Dictionary<Guid, List<TECEquipment>> proposalItemScope = getOneToManyRelationships(new ProposalItemContainingScopeTable(), equipment);
            Dictionary<Guid, List<TECProposalItem>> systemProposalItems = getOneToManyRelationships(new SystemProposalItemTable(), proposalItems);
            proposalItems.ForEach(x => x.ContainingScope.AddRange(proposalItemScope.ValueOrNew(x.Guid)));
            
            subScope.ForEach(item => item.Points.AddRange(subScopePoint.ValueOrNew(item.Guid)));
            equipment.ForEach(item => item.SubScope.AddRange(equipmentSubScope.ValueOrNew(item.Guid)));
            foreach (TECSystem system in systems)
            {
                system.Equipment.AddRange(systemEquipment.ValueOrNew(system.Guid));
                system.SetControllers(systemController.ValueOrNew(system.Guid));
                system.Panels.AddRange(systemPanels.ValueOrNew(system.Guid));
                system.MiscCosts.AddRange(systemMisc.ValueOrNew(system.Guid));
                system.ScopeBranches.AddRange(systemScopeBranch.ValueOrNew(system.Guid));
                system.ProposalItems.AddRange(systemProposalItems.ValueOrNew(system.Guid));
            }
            controllers.ForEach(item => {
                item.ChildrenConnections.AddRange(controllerConnection.ValueOrNew(item.Guid));
            });

            Dictionary<Guid, List<TECController>> panelControllerDictionary = getOneToManyRelationships(new PanelControllerTable(), controllers);
            controllers.ForEach(item => { if (item is TECProvidedController provided) provided.IOModules = providedControllerModuleRelationships.ValueOrNew(provided.Guid); });

            subScope.ForEach(item => { item.Devices.AddRange(endDevices.ValueOrNew(item.Guid));
                item.Interlocks.AddRange(subScopeInterlocks.ValueOrNew(item.Guid));
                item.ScopeBranches.AddRange(subScopeScopeBranch.ValueOrNew(item.Guid)); });
            panels.ForEach(item => item.Controllers = panelControllerDictionary.ValueOrNew(item.Guid));

            subScopeConnections.ForEach(item => { populateSubScopeConnectionProperties(item, connectionConduitTypes); });
            networkConnections.ForEach(item => {
                populateNetworkConnectionProperties(item, networkChildrenRelationships, connectionConduitTypes);
            });

            Dictionary<Guid, List<Guid>> systemTemplates = getOneToManyRelationships(new TemplatesSystemTable());
            Dictionary<Guid, List<Guid>> equipmentTemplates = getOneToManyRelationships(new TemplatesEquipmentTable());
            Dictionary<Guid, List<Guid>> subScopeTemplates = getOneToManyRelationships(new TemplatesSubScopeTable());
            Dictionary<Guid, List<Guid>> controllerTemplates = getOneToManyRelationships(new TemplatesControllerTable());
            Dictionary<Guid, List<Guid>> panelTemplates = getOneToManyRelationships(new TemplatesPanelTable());
            Dictionary<Guid, List<Guid>> miscTemplates = getOneToManyRelationships(new TemplatesMiscCostTable());

            templates.SystemTemplates.AddRange(getRelatedReferences(systemTemplates.ContainsKey(templates.Guid) ? systemTemplates[templates.Guid] : new List<Guid>(), systems));
            templates.EquipmentTemplates.AddRange(getRelatedReferences(equipmentTemplates.ContainsKey(templates.Guid) ? equipmentTemplates[templates.Guid] : new List<Guid>(), equipment));
            templates.SubScopeTemplates.AddRange(getRelatedReferences(subScopeTemplates.ContainsKey(templates.Guid) ? subScopeTemplates[templates.Guid] : new List<Guid>(), subScope));
            templates.ControllerTemplates.AddRange(getRelatedReferences(controllerTemplates.ContainsKey(templates.Guid) ? controllerTemplates[templates.Guid] : new List<Guid>(), controllers));
            templates.PanelTemplates.AddRange(getRelatedReferences(panelTemplates.ContainsKey(templates.Guid) ? panelTemplates[templates.Guid] : new List<Guid>(), panels));
            templates.MiscCostTemplates.AddRange(getRelatedReferences(miscTemplates.ValueOrDefault(templates.Guid, new List<Guid>()), misc));
            templates.Parameters.AddRange(templateParameters.ValueOrNew(templates.Guid));

            return templates;
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

        private static void populateTaggedProperties(TECTagged tagged, Dictionary<Guid, List<TECTag>> tags)
        {
            if (tags.ContainsKey(tagged.Guid))
            {
                List<TECTag> allTags = new List<TECTag>();
                tags[tagged.Guid].ForEach(item => allTags.Add(item));
                tagged.Tags.AddRange(allTags);
            }
        }
        private static void populateScopeProperties(TECScope scope, Dictionary<Guid, List<TECTag>> tags, Dictionary<Guid, List<TECAssociatedCost>> costs)
        {
            populateTaggedProperties(scope, tags);
            if (costs.ContainsKey(scope.Guid))
            {
                List<TECAssociatedCost> allCosts = new List<TECAssociatedCost>();
                costs[scope.Guid].ForEach(item => allCosts.Add(item));
                scope.AssociatedCosts.AddRange(allCosts);
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
        private static void linkBranchHierarchy(TECScopeBranch branch, IEnumerable<TECScopeBranch> branches, Dictionary<Guid, List<TECScopeBranch>> scopeBranchHierarchy)
        {
            if (scopeBranchHierarchy.ContainsKey(branch.Guid))
            {
                branch.Branches.AddRange(scopeBranchHierarchy.ValueOrNew(branch.Guid));
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
        private static TECPanel getPanelFromRow(DataRow row, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = justUpdated ? panelTypes.ValueOrDefault(guid, tempPanelType) : panelTypes[guid];
            TECPanel panel = new TECPanel(guid, type);
            assignValuePropertiesFromTable(panel, new PanelTable(), row);
            return panel;
        }

        private static TECProvidedController getProvidedControllerFromRow(DataRow row, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ProvidedControllerTable.ID.Name].ToString());
            TECControllerType type = justUpdated ? controllerTypes.ValueOrDefault(guid, tempControllerType) : controllerTypes[guid];
            TECProvidedController controller = new TECProvidedController(guid, type);
            assignValuePropertiesFromTable(controller, new ProvidedControllerTable(), row);
            return controller;
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
        
        private static TECMisc getMiscFromRow(DataRow row)
        {
            Guid guid = new Guid(row[MiscTable.ID.Name].ToString());
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[MiscTable.Type.Name].ToString());
            TECMisc cost = new TECMisc(guid, type);
            assignValuePropertiesFromTable(cost, new MiscTable(), row);
            return cost;
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
