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

        static private Logger logger = LogManager.GetCurrentClassLogger();
        static private SQLiteDatabase SQLiteDB;

        static private bool justUpdated;
        static private TECManufacturer tempManufacturer;
        static private TECPanelType tempPanelType;
        static private TECControllerType tempControllerType;

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

        static private (TECBid bid, bool needsUpdate) loadBid()
        {
            TECBid bid = getObjectFromTable(new BidInfoTable(), id => { return new TECBid(id); }, new TECBid());

            getScopeManagerProperties(bid);

            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(), bid.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(), bid.Catalogs.AssociatedCosts);
            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            List<IEndDevice> allEndDevices = new List<IEndDevice>(bid.Catalogs.Devices);
            allEndDevices.AddRange(bid.Catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices); Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), bid.Catalogs.ConduitTypes);
            bid.Locations = getObjectsFromTable(new LocationTable(), id => { return new TECLocation(id); });
            Dictionary<Guid, TECLocation> locationDictionary = getOneToOneRelationships(new LocatedLocationTable(), bid.Locations);
            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(), bid.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(), bid.Catalogs.PanelTypes);
            Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypeRelationships = getOneToManyRelationships(new NetworkConnectionConnectionTypeTable(), bid.Catalogs.ConnectionTypes);

            bid.Parameters = getObjectFromTable(new ParametersTable(), id => { return new TECParameters(id); }, new TECParameters(bid.Guid));
            bid.ExtraLabor = getObjectFromTable(new ExtraLaborTable(), id => { return new TECExtraLabor(id); }, new TECExtraLabor(bid.Guid));
            bid.Schedule = getSchedule(bid);
            bid.ScopeTree = getChildObjects(new BidScopeBranchTable(), new ScopeBranchTable(),
                bid.Guid, data => { return getScopeBranchFromRow(data, false); });
            //bid.Systems = getChildObjects(new BidSystemTable(), new SystemTable(),
            //    bid.Guid, data => { return getTypicalFromRow(data, controllerTypeDictionary, panelTypeDictionary); });
            bid.Systems = getScopeHierarchy(bid.Guid, controllerTypeDictionary, panelTypeDictionary);
            bid.Notes = getObjectsFromTable(new NoteTable(), id => { return new TECLabeled(id); });
            bid.Exclusions = getObjectsFromTable(new ExclusionTable(), id => { return new TECLabeled(id); });
            bid.SetControllers(getOrphanControllers(controllerTypeDictionary));
            bid.MiscCosts = getChildObjects(new BidMiscTable(), new MiscTable(), bid.Guid, data => { return getMiscFromRow(data, false); });
            bid.Panels = getOrphanPanels(panelTypeDictionary);

            List<TECController> allControllers = getAll<TECController>(bid);
            Dictionary<Guid, List<TECController>> panelControllerRelationships = getOneToManyRelationships(new PanelControllerTable(), allControllers);

            List<TECLocated> allLocated = getAll<TECLocated>(bid);
            allLocated.ForEach(item => populateLocatedProperties(item, locationDictionary));
            List<TECScope> allScope = getAll<TECScope>(bid);
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));
            List<TECSubScope> allSubScope = getAll<TECSubScope>(bid);
            allSubScope.ForEach(item => populateSubScopeChildren(item, points, endDevices));

            List<INetworkConnectable> allNetworkConnectable = new List<INetworkConnectable>(allSubScope);
            allNetworkConnectable.AddRange(allControllers);
            Dictionary<Guid, List<INetworkConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(), allNetworkConnectable);

            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new SubScopeConnectionChildrenTable(), allSubScope);

            List<TECPanel> allPanels = getAll<TECPanel>(bid);
            allPanels.ForEach(item => populatePanelControllers(item, panelControllerRelationships));

            List<TECSubScopeConnection> subScopeConnections = getAll<TECSubScopeConnection>(bid);
            subScopeConnections.ForEach(item => { populateSubScopeConnectionProperties(item,
                 subScopeConnectionChildrenRelationships, connectionConduitTypes); });
            List<TECNetworkConnection> networkConnections = getAll<TECNetworkConnection>(bid);
            networkConnections.ForEach(item => { populateNetworkConnectionProperties(item, networkChildrenRelationships,
                    connectionConduitTypes, connectionConnectionTypeRelationships); });

            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ControllerIOModuleTable(), bid.Catalogs.IOModules);
            allControllers.ForEach(item => populateControllerIOModules(item, controllerModuleRelationships));

            var placeholderDict = getCharacteristicInstancesList();
            bool needsSave = ModelLinkingHelper.LinkBid(bid, placeholderDict);

            return (bid, needsSave);
        }

        static private (TECTemplates templates, bool needsUpdate) loadTemplates()
        {
            TECTemplates templates = new TECTemplates();
            templates = getObjectFromTable(new TemplatesInfoTable(), id => { return new TECTemplates(id); }, new TECTemplates());
            getScopeManagerProperties(templates);

            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(), templates.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(), templates.Catalogs.AssociatedCosts);

            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            List<IEndDevice> allEndDevices = new List<IEndDevice>(templates.Catalogs.Devices);
            allEndDevices.AddRange(templates.Catalogs.Valves);
            Dictionary<Guid, List<IEndDevice>> endDevices = getOneToManyRelationships(new SubScopeDeviceTable(), allEndDevices);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), templates.Catalogs.ConduitTypes);
            Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypeRelationships = getOneToManyRelationships(new NetworkConnectionConnectionTypeTable(), templates.Catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ControllerIOModuleTable(), templates.Catalogs.IOModules);

            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(), templates.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(), templates.Catalogs.PanelTypes);
            templates.SystemTemplates = getChildObjects(new TemplatesSystemTable(), new SystemTable(),
                templates.Guid, data => { return getSystemFromRow(data, controllerTypeDictionary, panelTypeDictionary); });
            templates.EquipmentTemplates = getChildObjects(new TemplatesEquipmentTable(), new EquipmentTable(),
                templates.Guid, data => { return getEquipmentFromRow(data, false); });
            templates.SubScopeTemplates = getChildObjects(new TemplatesSubScopeTable(), new SubScopeTable(),
                templates.Guid, data => { return getSubScopeFromRow(data, false); });
            templates.MiscCostTemplates = getChildObjects(new TemplatesMiscCostTable(), new MiscTable(),
                templates.Guid, data => { return getMiscFromRow(data, false); });
            templates.ControllerTemplates = getChildObjects(new TemplatesControllerTable(), new ControllerTable(),
                templates.Guid, data => { return getControllerFromRow(data, false, controllerTypeDictionary); });
            templates.PanelTemplates = getChildObjects(new TemplatesPanelTable(), new PanelTable(),
                templates.Guid, data => { return getPanelFromRow(data, false, panelTypeDictionary); });
            templates.Parameters = getObjectsFromTable(new ParametersTable(), id => { return new TECParameters(id); });

            List<TECController> allControllers = getAll<TECController>(templates);
            Dictionary<Guid, List<TECController>> panelControllerDictionary = getOneToManyRelationships(new PanelControllerTable(), allControllers);
            allControllers.ForEach(item => populateControllerIOModules(item, controllerModuleRelationships));

            List<TECScope> allScope = getAll<TECScope>(templates);
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));
            List<TECSubScope> allSubScope = getAll<TECSubScope>(templates);
            allSubScope.ForEach(item => populateSubScopeChildren(item, points, endDevices));
            List<TECPanel> allPanels = getAll<TECPanel>(templates);
            allPanels.ForEach(item => populatePanelControllers(item, panelControllerDictionary));

            List<INetworkConnectable> allNetworkConnectable = new List<INetworkConnectable>(allSubScope);
            allNetworkConnectable.AddRange(allControllers);
            Dictionary<Guid, List<INetworkConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(), allNetworkConnectable);
            Dictionary<Guid, TECSubScope> subScopeConnectionChildrenRelationships = getOneToOneRelationships(new SubScopeConnectionChildrenTable(), allSubScope);

            List<TECSubScopeConnection> subScopeConnections = getAll<TECSubScopeConnection>(templates);
            List<TECNetworkConnection> networkConnections = getAll<TECNetworkConnection>(templates);

            subScopeConnections.ForEach(item => { populateSubScopeConnectionProperties(item,
                subScopeConnectionChildrenRelationships, connectionConduitTypes); });
            networkConnections.ForEach(item => { populateNetworkConnectionProperties(item,
                networkChildrenRelationships, connectionConduitTypes, connectionConnectionTypeRelationships); });

            Dictionary<Guid, List<Guid>> templateReferences = getTemplateReferences();
            bool needsSave = ModelLinkingHelper.LinkTemplates(templates, templateReferences);
            return (templates, needsSave);
        }

        static private void getScopeManagerProperties(TECScopeManager scopeManager)
        {
            scopeManager.Catalogs = getCatalogs();
            if (justUpdated)
            {
                scopeManager.Catalogs.Manufacturers.Add(tempManufacturer);
                scopeManager.Catalogs.PanelTypes.Add(tempPanelType);
                scopeManager.Catalogs.ControllerTypes.Add(tempControllerType);
            }
        }

        private static Dictionary<Guid, List<TECPoint>> getSubScopePoints()
        {
            TableBase table = new PointTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            List<TECPoint> points = new List<TECPoint>();
            foreach (DataRow row in data.Rows)
            {
                points.Add(getPointFromRow(row, false));
            }

            table = new SubScopePointTable();
            command = string.Format("select {0} from {1} order by {2}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString,
                SubScopePointTable.Index.Name);
            data = SQLiteDB.GetDataFromCommand(command);
            Dictionary<Guid, List<TECPoint>> dictionary = new Dictionary<Guid, List<TECPoint>>();
            foreach (DataRow row in data.Rows)
            {
                Guid subScopeID = new Guid(row[SubScopePointTable.SubScopeID.Name].ToString());
                Guid pointID = new Guid(row[SubScopePointTable.PointID.Name].ToString());
                TECPoint point = points.First(item => item.Guid == pointID);
                if (dictionary.ContainsKey(subScopeID))
                {
                    dictionary[subScopeID].Add(point);
                }
                else
                {
                    dictionary[subScopeID] = new List<TECPoint> { point };
                }
            }
            return dictionary;
        }

        private static void populateScopeProperties(TECScope scope, Dictionary<Guid, List<TECTag>> tags, Dictionary<Guid, List<TECAssociatedCost>> costs)
        {
            if (tags.ContainsKey(scope.Guid))
            {
                List<TECTag> allTags = new List<TECTag>();
                tags[scope.Guid].ForEach(item => allTags.Add(item));
                scope.Tags = new ObservableCollection<TECTag>(allTags);
            }
            if (costs.ContainsKey(scope.Guid))
            {
                List<TECAssociatedCost> allCosts = new List<TECAssociatedCost>();
                costs[scope.Guid].ForEach(item => allCosts.Add(item));
                scope.AssociatedCosts = new ObservableCollection<TECAssociatedCost>(allCosts);
            }
        }
        private static void populateLocatedProperties(TECLocated located, Dictionary<Guid, TECLocation> locations)
        {
            if (locations.ContainsKey(located.Guid))
            {
                located.Location = locations[located.Guid];
            }
        }
        private static void populateSubScopeChildren(TECSubScope subScope, Dictionary<Guid, List<TECPoint>> points, Dictionary<Guid, List<IEndDevice>> devices)
        {
            if (points.ContainsKey(subScope.Guid))
            {
                List<TECPoint> allPoints = new List<TECPoint>();
                foreach (TECPoint point in points[subScope.Guid])
                {
                    TECPoint newPoint = new TECPoint(point.Guid, subScope.IsTypical);
                    newPoint.Label = point.Label;
                    newPoint.Quantity = point.Quantity;
                    newPoint.Type = point.Type;
                    allPoints.Add(newPoint);
                }
                subScope.Points = new ObservableCollection<TECPoint>(allPoints);
            }
            if (devices.ContainsKey(subScope.Guid))
            {
                List<IEndDevice> allDevices = new List<IEndDevice>();
                foreach (IEndDevice device in devices[subScope.Guid])
                {
                    allDevices.Add(device);
                }
                subScope.Devices = new ObservableCollection<IEndDevice>(allDevices);
            }
        }
        private static void populatePanelControllers(TECPanel panel, Dictionary<Guid, List<TECController>> controllers)
        {
            if (controllers.ContainsKey(panel.Guid))
            {
                controllers[panel.Guid].ForEach(item => panel.Controllers.Add(item));
            }
        }
        private static void populateControllerIOModules(TECController controller, Dictionary<Guid, List<TECIOModule>> modules)
        {
            if (modules.ContainsKey(controller.Guid))
            {
                modules[controller.Guid].ForEach(item => controller.IOModules.Add(item));
            }
        }
        private static void populateRatedCostInMaterial(TECElectricalMaterial material, Dictionary<Guid, List<TECAssociatedCost>> ratedCosts)
        {
            if (ratedCosts.ContainsKey(material.Guid))
            {
                ratedCosts[material.Guid].ForEach(item => material.RatedCosts.Add(item));
            }
        }
        private static void populateSubScopeConnectionProperties(TECSubScopeConnection connection, Dictionary<Guid, TECSubScope> subScope, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            connection.SubScope = subScope[connection.Guid];
            populateConnectionProperties(connection, connectionConduitTypes);
        }
        private static void populateNetworkConnectionProperties(TECNetworkConnection connection, Dictionary<Guid, List<INetworkConnectable>> connectables,
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes, Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypes)
        {
            if (connectables.ContainsKey(connection.Guid))
            {
                connectables[connection.Guid].ForEach(item => connection.Children.Add(item));
            }
            if (connectionConnectionTypes.ContainsKey(connection.Guid))
            {
                connectionConnectionTypes[connection.Guid].ForEach(item => connection.ConnectionTypes.Add(item));
            }
            populateConnectionProperties(connection, connectionConduitTypes);
        }
        private static void populateConnectionProperties(TECConnection connection, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            if (connectionConduitTypes.ContainsKey(connection.Guid))
            {
                connection.ConduitType = connectionConduitTypes[connection.Guid];
            }
        }
        private static void populateIOModuleIO(TECIOModule module, Dictionary<Guid, List<TECIO>> moduleIORelationships)
        {
            if (moduleIORelationships.ContainsKey(module.Guid))
            {
                moduleIORelationships[module.Guid].ForEach(item => module.IO.Add(item));
            }
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
        
        static private void setupTemps()
        {
            tempManufacturer = new TECManufacturer();
            tempManufacturer.Label = "TEMPORARY";

            tempControllerType = new TECControllerType(tempManufacturer);
            TECIO input = new TECIO(IOType.UI);
            input.Quantity = 100;
            TECIO output = new TECIO(IOType.UO);
            output.Quantity = 100;
            TECIO bacnetIP = new TECIO(IOType.BACnetIP);
            bacnetIP.Quantity = 100;
            TECIO bacnetMSTP = new TECIO(IOType.BACnetMSTP);
            bacnetMSTP.Quantity = 100;
            TECIO lon = new TECIO(IOType.LonWorks);
            lon.Quantity = 100;
            TECIO modbusRTU = new TECIO(IOType.ModbusRTU);
            modbusRTU.Quantity = 100;
            TECIO modbusTCP = new TECIO(IOType.ModbusTCP);
            modbusTCP.Quantity = 100;
            tempControllerType.IO.Add(input);
            tempControllerType.IO.Add(output);
            tempControllerType.IO.Add(bacnetIP);
            tempControllerType.IO.Add(bacnetMSTP);
            tempControllerType.IO.Add(lon);
            tempControllerType.IO.Add(modbusRTU);
            tempControllerType.IO.Add(modbusTCP);

            tempPanelType = new TECPanelType(tempManufacturer);
        }
        static private TECCatalogs getCatalogs()
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Manufacturers = getObjectsFromTable(new ManufacturerTable(), id => { return new TECManufacturer(id); });
            catalogs.ConnectionTypes = getObjectsFromTable(new ConnectionTypeTable(), id => { return new TECConnectionType(id); });
            catalogs.ConduitTypes = getObjectsFromTable(new ConduitTypeTable(), id => { return new TECElectricalMaterial(id); });
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getOneToOneRelationships(new HardwareManufacturerTable(), catalogs.Manufacturers);
            Dictionary<Guid, List<TECConnectionType>> deviceConnectionType = getOneToManyRelationships(new DeviceConnectionTypeTable(), catalogs.ConnectionTypes);
            catalogs.Devices = getObjectsFromTable(new DeviceTable(), id => { return new TECDevice(id, deviceConnectionType.valueOrNew(id), hardwareManufacturer[id]); });
            Dictionary<Guid, TECDevice> actuators = getOneToOneRelationships(new ValveActuatorTable(), catalogs.Devices);
            catalogs.Valves = getObjectsFromTable(new ValveTable(), id => { return new TECValve(id, hardwareManufacturer[id], actuators[id]); });
            catalogs.AssociatedCosts = getObjectsFromTable(new AssociatedCostTable(), getAssociatedCostFromRow);
            catalogs.PanelTypes = getObjectsFromTable(new PanelTypeTable(), data => { return getPanelTypeFromRow(data, hardwareManufacturer); });
            catalogs.IOModules = getObjectsFromTable(new IOModuleTable(), id => { return new TECIOModule(id, hardwareManufacturer[id]); });
            catalogs.ControllerTypes = getObjectsFromTable(new ControllerTypeTable(), id => { return new TECControllerType(id, hardwareManufacturer[id]); });
            catalogs.Tags = getObjectsFromTable(new TagTable(), id => { return new TECTag(id); });

            List<TECIO> io = getObjectsFromTable(new IOTable(), getIOFromRow).ToList();

            Dictionary<Guid, List<TECAssociatedCost>> ratedCostsRelationShips = getOneToManyRelationships(new ElectricalMaterialRatedCostTable(), catalogs.AssociatedCosts);
            Dictionary<Guid, List<TECIOModule>> controllerTypeModuleRelationships = getOneToManyRelationships(new ControllerTypeIOModuleTable(), catalogs.IOModules);
            Dictionary<Guid, List<TECIO>> controllerTypeIORelationships = getOneToManyRelationships(new ControllerTypeIOTable(), io);
            Dictionary<Guid, List<TECIO>> moduleIORelationships = getOneToManyRelationships(new IOModuleIOTable(), io);

            catalogs.IOModules.ForEach(item => populateIOModuleIO(item, moduleIORelationships));
            catalogs.ControllerTypes.ForEach(item => populateControllerTypeProperties(item, controllerTypeModuleRelationships, controllerTypeIORelationships));
            catalogs.ConnectionTypes.ForEach(item => populateRatedCostInMaterial(item, ratedCostsRelationShips));
            catalogs.ConduitTypes.ForEach(item => populateRatedCostInMaterial(item, ratedCostsRelationShips));

            return catalogs;
        }
        
        static private Dictionary<Guid, List<Guid>> getCharacteristicInstancesList()
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
        
        private static TECSchedule getSchedule(TECBid bid)
        {
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
            Guid guid = new Guid(DT.Rows[0][ScheduleTable.ID.Name].ToString());
            List<TECScheduleTable> tables = getChildObjects(new ScheduleScheduleTableTable(), new ScheduleTableTable(), guid, getScheduleTableFromRow).ToList();
            TECSchedule schedule = new TECSchedule(guid, tables);
            return schedule;
        }
        static private ObservableCollection<TECController> getOrphanControllers(Dictionary<Guid, TECControllerType> controllerTypes)
        {
            //Returns the controllers that are not in the ControlledScopeController table.
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ControllerTable()) + " from " + ControllerTable.TableName;
            command += " where " + ControllerTable.ID.Name + " not in ";
            command += "(select " + SystemControllerTable.ControllerID.Name;
            command += " from " + SystemControllerTable.TableName + ")";

            DataTable controllersDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in controllersDT.Rows)
            {
                controllers.Add(getControllerFromRow(row, false, controllerTypes));
            }

            return controllers;
        }
        static private ObservableCollection<TECPanel> getOrphanPanels(Dictionary<Guid, TECPanelType> panelTypes)
        {
            //Returns the panels that are not in the ControlledScopePanel table.
            ObservableCollection<TECPanel> panels = new ObservableCollection<TECPanel>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new PanelTable()) + " from " + PanelTable.TableName;
            command += " where " + PanelTable.ID.Name + " not in ";
            command += "(select " + SystemPanelTable.PanelID.Name;
            command += " from " + SystemPanelTable.TableName + ")";

            DataTable panelsDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in panelsDT.Rows)
            {
                panels.Add(getPanelFromRow(row, false, panelTypes));
            }

            return panels;
        }

        static private ObservableCollection<TECConnection> getConnectionsInController(TECController controller, bool isTypical)
        {
            ObservableCollection<TECConnection> outScope = new ObservableCollection<TECConnection>();
            string command;
            DataTable scopeDT;

            command = "select " + DatabaseHelper.AllFieldsInTableString(new NetworkConnectionTable()) + " from " + NetworkConnectionTable.TableName + " where " + NetworkConnectionTable.ID.Name + " in ";
            command += "(select " + ControllerConnectionTable.ConnectionID.Name + " from " + ControllerConnectionTable.TableName + " where ";
            command += ControllerConnectionTable.ControllerID.Name + " = '" + controller.Guid;
            command += "')";

            scopeDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in scopeDT.Rows)
            {
                var networkConnection = getNetworkConnectionFromRow(row, isTypical);
                networkConnection.ParentController = controller;

                outScope.Add(networkConnection);
            }

            command = "select " + DatabaseHelper.AllFieldsInTableString(new SubScopeConnectionTable()) + " from " + SubScopeConnectionTable.TableName + " where " + SubScopeConnectionTable.ID.Name + " in ";
            command += "(select " + ControllerConnectionTable.ConnectionID.Name + " from " + ControllerConnectionTable.TableName + " where ";
            command += ControllerConnectionTable.ControllerID.Name + " = '" + controller.Guid;
            command += "')";

            scopeDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in scopeDT.Rows)
            {
                var subScopeConnection = getSubScopeConnectionFromRow(row, isTypical);
                subScopeConnection.ParentController = controller;
                outScope.Add(subScopeConnection);
            }

            return outScope;
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

        private static ObservableCollection<TECTypical> getScopeHierarchy(Guid bidID, Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Dictionary<Guid, List<Guid>> bidTypicals = getOneToManyRelationships(new BidSystemTable());
            List<Guid> typicalIDs = bidTypicals[bidID];
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
            
            DataTable allSystemData = SQLiteDB.GetDataFromTable(SystemTable.TableName);
            var typicalRows = from row in allSystemData.AsEnumerable() where typicalSystems.ContainsKey(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            var systemRows = from row in allSystemData.AsEnumerable() where !typicalSystems.ContainsKey(new Guid(row[SystemTable.ID.Name].ToString())) select row;
            DataTable typicalData = typicalRows.CopyToDataTable();
            DataTable systemData = systemRows.CopyToDataTable();

            DataTable equipmentData = SQLiteDB.GetDataFromTable(EquipmentTable.TableName);
            DataTable subScopeData = SQLiteDB.GetDataFromTable(SubScopeTable.TableName);
            DataTable pointData = SQLiteDB.GetDataFromTable(PointTable.TableName);
            DataTable controllerData = SQLiteDB.GetDataFromTable(ControllerTable.TableName);
            DataTable panelData = SQLiteDB.GetDataFromTable(PanelTable.TableName);
            DataTable subScopeConnectionData = SQLiteDB.GetDataFromTable(SubScopeConnectionTable.TableName);
            DataTable networkConnectionData = SQLiteDB.GetDataFromTable(SubScopeConnectionTable.TableName);
            DataTable miscData = SQLiteDB.GetDataFromTable(MiscTable.TableName);
            DataTable scopeBranchData = SQLiteDB.GetDataFromTable(ScopeBranchTable.TableName);

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

            List<TECPoint> points = getObjectsFromData(new PointTable(), pointData, row => { return getPointFromRow(row, typicalDictionary); });
            List<TECSubScope> subScope = getObjectsFromData(new SubScopeTable(), subScopeData, id => { return new TECSubScope(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECEquipment> equipment = getObjectsFromData(new EquipmentTable(), equipmentData, id => { return new TECEquipment(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECMisc> misc = getObjectsFromData(new MiscTable(), miscData, row => { return getMiscFromRow(row, typicalDictionary); });
            List<TECScopeBranch> scopeBranches = getObjectsFromData(new ScopeBranchTable(), scopeBranchData, id => { return new TECScopeBranch(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECController> controllers = getObjectsFromData(new ControllerTable(), controllerData, row => { return getControllerFromRow(row, typicalDictionary, controllerTypes); });
            List<TECPanel> panels = getObjectsFromData(new PanelTable(), panelData, row => { return getPanelFromRow(row, typicalDictionary, panelTypes); });
            List<TECSubScopeConnection> subScopeConnections = getObjectsFromData(new SubScopeConnectionTable(), subScopeConnectionData, id => { return new TECSubScopeConnection(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECNetworkConnection> networkConnections = getObjectsFromData(new NetworkConnectionTable(), networkConnectionData, id => { return new TECNetworkConnection(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECConnection> allConnections = new List<TECConnection>(subScopeConnections);
            allConnections.AddRange(networkConnections);
            List<TECSystem> systems = getObjectsFromData(new SystemTable(), systemData, id => { return new TECSystem(id, typicalDictionary.ContainsKey(id) ? typicalDictionary[id] : false); });
            List<TECTypical> typicals = getObjectsFromData(new SystemTable(), typicalData, id => { return new TECTypical(id); });

            foreach (TECSubScope item in subScope.Where(x => subScopePoints.ContainsKey(x.Guid)))
            {
                item.Points = new ObservableCollection<TECPoint>(getRelatedReferences(subScopePoints[item.Guid], points));
            }
            foreach(TECEquipment item in equipment.Where(x => equipmentSubScope.ContainsKey(x.Guid)))
            {
                item.SubScope = new ObservableCollection<TECSubScope>(getRelatedReferences(equipmentSubScope[item.Guid], subScope));
            }
            foreach(TECController item in controllers.Where(x => controllerConnections.ContainsKey(x.Guid)))
            {
                item.ChildrenConnections = new ObservableCollection<TECConnection>(getRelatedReferences(controllerConnections[item.Guid], allConnections));
            }
            foreach(TECSystem item in systems)
            {
                setSystemChildren(item);
            }
            foreach(TECTypical item in typicals)
            {
                setSystemChildren(item);
                if (typicalSystems.ContainsKey(item.Guid)) 
                    item.Instances = new ObservableCollection<TECSystem>(getRelatedReferences(typicalSystems[item.Guid], systems));
            }

            return new ObservableCollection<TECTypical>(typicals);

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
            void linkBranchHierarchy(TECScopeBranch branch)
            {
                if (scopeBranchHierarchy.ContainsKey(branch.Guid))
                {
                    branch.Branches = new ObservableCollection<TECScopeBranch>(getRelatedReferences(scopeBranchHierarchy[branch.Guid], scopeBranches));
                    foreach (var subBranch in branch.Branches)
                    {
                        linkBranchHierarchy(subBranch);
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
                    item.Equipment = new ObservableCollection<TECEquipment>(getRelatedReferences(systemEquipment[item.Guid], equipment));
                if (systemControllers.ContainsKey(item.Guid))
                    item.SetControllers(getRelatedReferences(systemControllers[item.Guid], controllers));
                if (systemPanels.ContainsKey(item.Guid))
                    item.Panels = new ObservableCollection<TECPanel>(getRelatedReferences(systemPanels[item.Guid], panels));
                if (systemMisc.ContainsKey(item.Guid))
                    item.MiscCosts = new ObservableCollection<TECMisc>(getRelatedReferences(systemMisc[item.Guid], misc));
                if (systemScopeBranches.ContainsKey(item.Guid))
                    item.ScopeBranches = new ObservableCollection<TECScopeBranch>(getRelatedReferences(systemScopeBranches[item.Guid], scopeBranches));
                item.ScopeBranches.ForEach(branch => linkBranchHierarchy(branch));
            }
        }

        private static TECTypical getEmptyTypicalFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECTypical system = new TECTypical(guid);
            assignValuePropertiesFromTable(system, new SystemTable(), row);
            return system;
        }

        #region Data Handlers
        private static TECTypical getTypicalFromRow(DataRow row, Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECTypical system = new TECTypical(guid);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            system.Instances = getChildObjects(new SystemHierarchyTable(), new SystemTable(), guid, data => { return getSystemFromRow(data, controllerTypes, panelTypes); });

            var controllers = getChildObjects(new SystemControllerTable(), new ControllerTable(), guid, data => { return getControllerFromRow(data, true, controllerTypes); });
            system.SetControllers(controllers);
            system.Equipment = getChildObjects(new SystemEquipmentTable(), new EquipmentTable(), guid, data => { return getEquipmentFromRow(data, true); });
            system.Panels = getChildObjects(new SystemPanelTable(), new PanelTable(), guid, data => { return getPanelFromRow(data, true, panelTypes); });
            system.MiscCosts = getChildObjects(new SystemMiscTable(), new MiscTable(), guid, data => { return getMiscFromRow(data, true); });
            system.ScopeBranches = getChildObjects(new SystemScopeBranchTable(), new ScopeBranchTable(), guid, data => { return getScopeBranchFromRow(data, true); });

            return system;
        }
        private static TECSystem getSystemFromRow(DataRow row, Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECSystem system = new TECSystem(guid, false);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            var controllers = getChildObjects(new SystemControllerTable(), new ControllerTable(), guid, data => { return getControllerFromRow(data, false, controllerTypes); });
            system.SetControllers(controllers);
            system.Equipment = getChildObjects(new SystemEquipmentTable(), new EquipmentTable(), guid, data => { return getEquipmentFromRow(data, false); });
            system.Panels = getChildObjects(new SystemPanelTable(), new PanelTable(), guid, data => { return getPanelFromRow(data, false, panelTypes); });
            system.MiscCosts = getChildObjects(new SystemMiscTable(), new MiscTable(), guid, data => { return getMiscFromRow(data, false); });
            system.ScopeBranches = getChildObjects(new SystemScopeBranchTable(), new ScopeBranchTable(), guid, data => { return getScopeBranchFromRow(data, false); });

            return system;
        }
        private static TECEquipment getEquipmentFromRow(DataRow row, bool isTypical)
        {
            Guid equipmentID = new Guid(row[EquipmentTable.ID.Name].ToString());
            TECEquipment equipmentToAdd = new TECEquipment(equipmentID, isTypical);
            assignValuePropertiesFromTable(equipmentToAdd, new EquipmentTable(), row);
            equipmentToAdd.SubScope = getChildObjects(new EquipmentSubScopeTable(), new SubScopeTable(),
                equipmentID, data => { return getSubScopeFromRow(data, isTypical); });
            return equipmentToAdd;
        }
        private static TECSubScope getSubScopeFromRow(DataRow row, bool isTypical)
        {
            Guid subScopeID = new Guid(row[SubScopeTable.ID.Name].ToString());
            TECSubScope subScopeToAdd = new TECSubScope(subScopeID, isTypical);
            assignValuePropertiesFromTable(subScopeToAdd, new SubScopeTable(), row);
            return subScopeToAdd;
        }
        private static TECPoint getPointFromRow(DataRow row, bool isTypical)
        {
            Guid pointID = new Guid(row[PointTable.ID.Name].ToString());
            TECPoint pointToAdd = new TECPoint(pointID, isTypical);
            assignValuePropertiesFromTable(pointToAdd, new PointTable(), row);
            string pointType = row[PointTable.Type.Name].ToString();
            if (pointType == "BO")
            {
                pointType = "DO";
            }
            else if (pointType == "BI")
            {
                pointType = "DI";
            }
            pointToAdd.Type = UtilitiesMethods.StringToEnum<IOType>(pointType);
            return pointToAdd;
        }
        private static TECPoint getPointFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary)
        {
            Guid pointID = new Guid(row[PointTable.ID.Name].ToString());
            bool isTypical = typicalDictionary.ContainsKey(pointID) ? typicalDictionary[pointID] : false;
            TECPoint pointToAdd = new TECPoint(pointID, isTypical);
            assignValuePropertiesFromTable(pointToAdd, new PointTable(), row);
            string pointType = row[PointTable.Type.Name].ToString();
            if (pointType == "BO")
            {
                pointType = "DO";
            }
            else if (pointType == "BI")
            {
                pointType = "DI";
            }
            pointToAdd.Type = UtilitiesMethods.StringToEnum<IOType>(pointType);
            return pointToAdd;
        }
        private static TECScopeBranch getScopeBranchFromRow(DataRow row, bool isTypical)
        {
            Guid scopeBranchID = new Guid(row[ScopeBranchTable.ID.Name].ToString());
            TECScopeBranch branch = new TECScopeBranch(scopeBranchID, isTypical);
            assignValuePropertiesFromTable(branch, new ScopeBranchTable(), row);
            branch.Branches = getChildObjects(new ScopeBranchHierarchyTable(), new ScopeBranchTable(), scopeBranchID, data => { return getScopeBranchFromRow(data, isTypical); });
            return branch;
        }
        private static TECPanel getPanelFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = null;
            if (!panelTypes.ContainsKey(guid) && justUpdated)
            {
                type = tempPanelType;
            }
            else
            {
                type = panelTypes[guid];
            }
            TECPanel panel = new TECPanel(guid, type, isTypical);
            assignValuePropertiesFromTable(panel, new PanelTable(), row);

            return panel;
        }
        private static TECPanel getPanelFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = null;
            if (!panelTypes.ContainsKey(guid) && justUpdated)
            {
                type = tempPanelType;
            }
            else
            {
                type = panelTypes[guid];
            }
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            TECPanel panel = new TECPanel(guid, type, isTypical);
            assignValuePropertiesFromTable(panel, new PanelTable(), row);

            return panel;
        }

        private static TECController getControllerFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECControllerType type = null;
            if (!controllerTypes.ContainsKey(guid) && justUpdated)
            {
                type = tempControllerType;
            }
            else
            {
                type = controllerTypes[guid];
            }
            TECController controller = new TECController(guid, type, isTypical);
            assignValuePropertiesFromTable(controller, new ControllerTable(), row);
            controller.ChildrenConnections = getConnectionsInController(controller, isTypical);
            return controller;
        }
        private static TECController getControllerFromRow(DataRow row, Dictionary<Guid, bool> typicalDictionary, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECControllerType type = null;
            if (!controllerTypes.ContainsKey(guid) && justUpdated)
            {
                type = tempControllerType;
            }
            else
            {
                type = controllerTypes[guid];
            }
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            TECController controller = new TECController(guid, type, isTypical);
            assignValuePropertiesFromTable(controller, new ControllerTable(), row);
            return controller;
        }

        private static TECIO getIOFromRow(DataRow row)
        {
            Guid guid = new Guid(row[IOTable.ID.Name].ToString());
            IOType type = UtilitiesMethods.StringToEnum<IOType>(row[IOTable.IOType.Name].ToString());
            var io = new TECIO(guid, type);
            assignValuePropertiesFromTable(io, new IOTable(), row);
            return io;
        }
        private static TECSubScopeConnection getSubScopeConnectionFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[SubScopeConnectionTable.ID.Name].ToString());
            TECSubScopeConnection connection = new TECSubScopeConnection(guid, isTypical);
            assignValuePropertiesFromTable(connection, new SubScopeConnectionTable(), row);
            return connection;
        }
        private static TECNetworkConnection getNetworkConnectionFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[NetworkConnectionTable.ID.Name].ToString());
            TECNetworkConnection connection = new TECNetworkConnection(guid, isTypical);
            assignValuePropertiesFromTable(connection, new NetworkConnectionTable(), row);
            connection.IOType = UtilitiesMethods.StringToEnum<IOType>(row[NetworkConnectionTable.IOType.Name].ToString());
            return connection;
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
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[MiscTable.Type.Name].ToString());
            bool isTypical = typicalDictionary.ContainsKey(guid) ? typicalDictionary[guid] : false;
            TECMisc cost = new TECMisc(guid, type, isTypical);
            assignValuePropertiesFromTable(cost, new MiscTable(), row);
            return cost;
        }
        private static TECScheduleItem getScheduleItemFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScheduleItemTable.ID.Name].ToString());
            String tag = row[ScheduleItemTable.Tag.Name].ToString();
            String service = row[ScheduleItemTable.Service.Name].ToString();
            String location = row[ScheduleItemTable.Location.Name].ToString();
            DataTable dataTable = getChildIDs(new ScheduleItemScopeTable(), guid);
            TECScope scope = dataTable.Rows.Count > 0 ?
                new TECSubScope(new Guid(dataTable.Rows[0][ScheduleItemScopeTable.ScopeID.Name].ToString()), false) : null;
            TECScheduleItem item = new TECScheduleItem(guid);
            item.Tag = tag;
            item.Service = service;
            item.Location = location;
            item.Scope = scope;
            return item;
        }
        private static TECScheduleTable getScheduleTableFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScheduleTableTable.ID.Name].ToString());
            String name = row[ScheduleTableTable.Name.Name].ToString();
            List<TECScheduleItem> items = getChildObjects(new ScheduleTableScheduleItemTable(), new ScheduleItemTable(), guid, getScheduleItemFromRow).ToList();
            TECScheduleTable table = new TECScheduleTable(guid, items);
            table.Name = name;
            return table;
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
            TECManufacturer manufacturer = null;
            if (manufacturers.ContainsKey(guid))
            {
                manufacturer = manufacturers[guid];

            }
            else if (justUpdated)
            {
                manufacturer = tempManufacturer;
            }
            else
            {
                throw new KeyNotFoundException();
            }
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
                }
            }
        }
        #endregion

        #region Generic Database Query Methods
        private static Dictionary<Guid, List<T>> getOneToManyRelationships<T>(TableBase table, IEnumerable<T> references) where T : ITECObject
        {
            if (table.PrimaryKeys.Count != 2) { throw new Exception("Table must have two primary keys"); }
            string parentField = table.PrimaryKeys[0].Name;
            string childField = table.PrimaryKeys[1].Name;

            string qtyField = getQuantityField(table);

            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            Dictionary<Guid, List<T>> dictionary = new Dictionary<Guid, List<T>>();
            foreach (DataRow row in data.Rows)
            {
                Guid parentID = new Guid(row[parentField].ToString());
                Guid childID = new Guid(row[childField].ToString());
                T reference = references.First(item => item.Guid == childID);
                int quantity = 1;
                if (qtyField != "")
                {
                    quantity = row[qtyField].ToString().ToInt();
                }
                for (int x = 0; x < quantity; x++)
                {
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
            return dictionary;
        }
        private static Dictionary<Guid, List<Guid>> getOneToManyRelationships(TableBase table) 
        {
            if (table.PrimaryKeys.Count != 2) { throw new Exception("Table must have two primary keys"); }
            string parentField = table.PrimaryKeys[0].Name;
            string childField = table.PrimaryKeys[1].Name;

            string orderKey = table.IndexString;
            string orderString = "";
            if (orderKey != "")
            {
                orderString = string.Format(" order by {0}", orderKey);
            }


            string qtyField = getQuantityField(table);

            string command = string.Format("select {0} from {1}{2}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString,
                orderString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            Dictionary<Guid, List<Guid>> dictionary = new Dictionary<Guid, List<Guid>>();
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
            return dictionary;
        }
        private static Dictionary<Guid, T> getOneToOneRelationships<T>(TableBase table, IEnumerable<T> references) where T : ITECObject
        {
            if (table.PrimaryKeys.Count != 2)
            {
                throw new Exception("Table must have two primary keys");
            }
            string parentField = table.PrimaryKeys[0].Name;
            string childField = table.PrimaryKeys[1].Name;

            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            {
                Guid parentID = new Guid(row[parentField].ToString());
                Guid childID = new Guid(row[childField].ToString());
                T outItem = references.First(item => item.Guid == childID);
                dictionary[parentID] = outItem;
            }
            return dictionary;
        }
        static private ObservableCollection<T> getObjectsFromTable<T>(TableBase table, Func<DataRow, T> dataHandler)
        {
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(table), table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            return new ObservableCollection<T>(getObjectsFromData(table, data, dataHandler));
        }
        static private ObservableCollection<T> getObjectsFromTable<T>(TableBase table, Func<Guid, T> constructor)
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
        static private ObservableCollection<T> getChildObjects<T>(TableBase relationTable, TableBase childTable, Guid parentID, Func<DataRow, T> dataHandler)
        {
            DataTable data = getChildData(relationTable, childTable, parentID);
            return new ObservableCollection<T>(getObjectsFromData(childTable, data, dataHandler));
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
        private static DataTable getChildIDs(TableBase relationTable, Guid parentID)
        {

            if (relationTable.PrimaryKeys.Count != 2)
            {
                throw new Exception("Relation table must have primary keys for each object");
            }
            string fieldString = relationTable.PrimaryKeys[1].Name;
            string quantityField = getQuantityField(relationTable);
            if (quantityField != "")
            {
                fieldString += ", " + quantityField;
            }
            string command = string.Format("select {0} from {1} where {2} = '{3}'",
                fieldString, relationTable.NameString,
                relationTable.PrimaryKeys[0].Name, parentID.ToString());
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

        private static string getQuantityField(TableBase table)
        {
            string qtyField = "";
            foreach (TableField field in table.Fields)
            {
                if (field.Property.DeclaringType == typeof(HelperProperties) && field.Property.Name == "Quantity")
                {
                    qtyField = field.Name;
                    break;
                }
            }
            return qtyField;
        }

        private static List<T> getAll<T>(ITECObject obj) where T : ITECObject
        {
            List<T> list = new List<T>();
            if (obj is T typedObj)
            {
                list.Add(typedObj);
            }
            if (obj is IRelatable rel)
            {
                foreach (var item in rel.PropertyObjects.Objects.Where(x => !rel.LinkedObjects.Contains(x)))
                {
                    list.AddRange(getAll<T>(item));
                }
            }
            return list;
        }
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
    }

    internal static class HelperExtensions
    {
        public static ObservableCollection<T> valueOrNew<T>(this Dictionary<Guid, List<T>> dictionary, Guid id)
        {
            return dictionary.ContainsKey(id) ? new ObservableCollection<T>(dictionary[id]) : new ObservableCollection<T>();
        }
    }
}
