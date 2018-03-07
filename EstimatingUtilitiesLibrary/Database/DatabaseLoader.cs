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
            TECBid bid = GetBidInfo(SQLiteDB);
            
            getScopeManagerProperties(bid);
            
            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(),
                ScopeTagTable.ScopeID.Name, ScopeTagTable.TagID.Name, bid.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(),
                ScopeAssociatedCostTable.ScopeID.Name, ScopeAssociatedCostTable.AssociatedCostID.Name, bid.Catalogs.AssociatedCosts);
            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            Dictionary<Guid, List<IEndDevice>> endDevices = getEndDevices(bid.Catalogs);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), 
                ConnectionConduitTypeTable.ConnectionID.Name, ConnectionConduitTypeTable.TypeID.Name, bid.Catalogs.ConduitTypes);
            bid.Locations = getObjectsFromTable(new LocationTable(), getLocationFromRow);
            Dictionary<Guid, TECLocation> locationDictionary = getOneToOneRelationships(new LocatedLocationTable(),
                LocatedLocationTable.ScopeID.Name, LocatedLocationTable.LocationID.Name, bid.Locations);
            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(),
                ControllerControllerTypeTable.ControllerID.Name, ControllerControllerTypeTable.TypeID.Name, bid.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(),
                PanelPanelTypeTable.PanelID.Name, PanelPanelTypeTable.PanelTypeID.Name, bid.Catalogs.PanelTypes);
            Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypeRelationships = getOneToManyRelationships(new NetworkConnectionConnectionTypeTable(),
                NetworkConnectionConnectionTypeTable.ConnectionID.Name, NetworkConnectionConnectionTypeTable.TypeID.Name, bid.Catalogs.ConnectionTypes);
            

            bid.Parameters = getBidParameters(bid);
            bid.ExtraLabor = getExtraLabor(bid);
            bid.Schedule = getSchedule(bid);
            bid.ScopeTree = getChildObjects(new BidScopeBranchTable(), new ScopeBranchTable(),
                bid.Guid, data => { return getScopeBranchFromRow(data, false); });
            bid.Systems = getChildObjects(new BidSystemTable(), new SystemTable(),
                bid.Guid, data => { return getTypicalFromRow(data, controllerTypeDictionary, panelTypeDictionary); });
            bid.Notes = getObjectsFromTable(new NoteTable(), getNoteFromRow);
            bid.Exclusions = getObjectsFromTable(new ExclusionTable(), getNoteFromRow);
            bid.SetControllers(getOrphanControllers(controllerTypeDictionary));
            bid.MiscCosts = getChildObjects(new BidMiscTable(), new MiscTable(), bid.Guid, data => { return getMiscFromRow(data, false); }); 
            bid.Panels = getOrphanPanels(panelTypeDictionary);
            
            List<TECController> allControllers = getAll<TECController>(bid);
            Dictionary<Guid, List<TECController>> panelControllerRelationships = getOneToManyRelationships(new PanelControllerTable(), PanelControllerTable.PanelID.Name, PanelControllerTable.ControllerID.Name, allControllers);
            
            List<TECLocated> allLocated = getAll<TECLocated>(bid);
            allLocated.ForEach(item => populateLocatedProperties(item, locationDictionary));
            List<TECScope> allScope = getAll<TECScope>(bid);
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));
            List<TECSubScope> allSubScope = getAll<TECSubScope>(bid);
            allSubScope.ForEach(item => populateSubScopeChildren(item, points, endDevices));

            List<INetworkConnectable> allNetworkConnectable = new List<INetworkConnectable>(allSubScope);
            allNetworkConnectable.AddRange(allControllers);
            Dictionary<Guid, List<INetworkConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(),
                NetworkConnectionChildrenTable.ConnectionID.Name, NetworkConnectionChildrenTable.ChildID.Name, allNetworkConnectable);
            
            Dictionary<Guid, List<TECSubScope>> subScopeConnectionChildrenRelationships = getOneToManyRelationships(new SubScopeConnectionChildrenTable(),
                SubScopeConnectionChildrenTable.ConnectionID.Name, SubScopeConnectionChildrenTable.ChildID.Name, allSubScope);

            List<TECPanel> allPanels = getAll<TECPanel>(bid);
            allPanels.ForEach(item => populatePanelControllers(item, panelControllerRelationships));

            List<TECSubScopeConnection> subScopeConnections = getAll<TECSubScopeConnection>(bid);
            subScopeConnections.ForEach(item =>
            {
                populateConduitTypesInConnection(item, connectionConduitTypes);
                populateSubScopeConnectionChildren(item, subScopeConnectionChildrenRelationships);
            });
            List<TECNetworkConnection> networkConnections = getAll<TECNetworkConnection>(bid);
            networkConnections.ForEach(item =>
            {
                populateConnectionTypesInConnection(item, connectionConnectionTypeRelationships);
                populateConduitTypesInConnection(item, connectionConduitTypes);
                populateNetworkConnectionChildren(item, networkChildrenRelationships);
            });

            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ControllerIOModuleTable(), 
                ControllerIOModuleTable.ControllerID.Name, ControllerIOModuleTable.ModuleID.Name, bid.Catalogs.IOModules, ControllerIOModuleTable.Quantity.Name);
            allControllers.ForEach(item => populateControllerIOModules(item, controllerModuleRelationships));


            var placeholderDict = getCharacteristicInstancesList();
            bool needsSave = ModelLinkingHelper.LinkBid(bid, placeholderDict);

            return (bid, needsSave);
        }
        
        static private (TECTemplates templates, bool needsUpdate) loadTemplates()
        {
            TECTemplates templates = new TECTemplates();
            templates = GetTemplatesInfo(SQLiteDB);
            getScopeManagerProperties(templates);

            Dictionary<Guid, List<TECTag>> tagRelationships = getOneToManyRelationships(new ScopeTagTable(),
                            ScopeTagTable.ScopeID.Name, ScopeTagTable.TagID.Name, templates.Catalogs.Tags);
            Dictionary<Guid, List<TECAssociatedCost>> costRelationships = getOneToManyRelationships(new ScopeAssociatedCostTable(),
                ScopeAssociatedCostTable.ScopeID.Name, ScopeAssociatedCostTable.AssociatedCostID.Name, templates.Catalogs.AssociatedCosts);

            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            Dictionary<Guid, List<IEndDevice>> endDevices = getEndDevices(templates.Catalogs);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(),
                ConnectionConduitTypeTable.ConnectionID.Name, ConnectionConduitTypeTable.TypeID.Name, templates.Catalogs.ConduitTypes);
            Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypeRelationships = getOneToManyRelationships(new NetworkConnectionConnectionTypeTable(),
                NetworkConnectionConnectionTypeTable.ConnectionID.Name, NetworkConnectionConnectionTypeTable.TypeID.Name, templates.Catalogs.ConnectionTypes);
            Dictionary<Guid, List<TECIOModule>> controllerModuleRelationships = getOneToManyRelationships(new ControllerIOModuleTable(),
               ControllerIOModuleTable.ControllerID.Name, ControllerIOModuleTable.ModuleID.Name, templates.Catalogs.IOModules, ControllerIOModuleTable.Quantity.Name);

            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(),
                ControllerControllerTypeTable.ControllerID.Name, ControllerControllerTypeTable.TypeID.Name, templates.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(),
                PanelPanelTypeTable.PanelID.Name, PanelPanelTypeTable.PanelTypeID.Name, templates.Catalogs.PanelTypes);
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
            templates.Parameters = getObjectsFromTable(new ParametersTable(), getParametersFromRow);
            
            List<TECController> allControllers = getAll<TECController>(templates);
            Dictionary<Guid, List<TECController>> panelControllerDictionary = getOneToManyRelationships(new PanelControllerTable(), PanelControllerTable.PanelID.Name, PanelControllerTable.ControllerID.Name, allControllers);
            allControllers.ForEach(item => populateControllerIOModules(item, controllerModuleRelationships));

            List<TECScope> allScope = getAll<TECScope>(templates);
            allScope.ForEach(item => populateScopeProperties(item, tagRelationships, costRelationships));
            List<TECSubScope> allSubScope = getAll<TECSubScope>(templates);
            allSubScope.ForEach(item => populateSubScopeChildren(item, points, endDevices));
            List<TECPanel> allPanels = getAll<TECPanel>(templates);
            allPanels.ForEach(item => populatePanelControllers(item, panelControllerDictionary));
            
            List<INetworkConnectable> allNetworkConnectable = new List<INetworkConnectable>(allSubScope);
            allNetworkConnectable.AddRange(allControllers);
            Dictionary<Guid, List<INetworkConnectable>> networkChildrenRelationships = getOneToManyRelationships(new NetworkConnectionChildrenTable(),
                NetworkConnectionChildrenTable.ConnectionID.Name, NetworkConnectionChildrenTable.ChildID.Name, allNetworkConnectable);

            Dictionary<Guid, List<TECSubScope>> subScopeConnectionChildrenRelationships = getOneToManyRelationships(new SubScopeConnectionChildrenTable(),
                SubScopeConnectionChildrenTable.ConnectionID.Name, SubScopeConnectionChildrenTable.ChildID.Name, allSubScope);
            
            List<TECConnection> allConnections = getAll<TECConnection>(templates);

            allConnections.ForEach(item =>
            {
                populateConnectionTypesInConnection(item, connectionConnectionTypeRelationships);
                populateConduitTypesInConnection(item, connectionConduitTypes);
                populateNetworkConnectionChildren(item, networkChildrenRelationships);
                populateSubScopeConnectionChildren(item, subScopeConnectionChildrenRelationships);
            });

            

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

        private static Dictionary<Guid, List<IEndDevice>> getEndDevices(TECCatalogs catalogs)
        {
            TableBase table = new SubScopeDeviceTable();
            string command = string.Format("select {0} from {1} order by {2}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString,
                SubScopeDeviceTable.Index.Name);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            Dictionary<Guid, List<IEndDevice>> dictionary = new Dictionary<Guid, List<IEndDevice>>();
            List<IEndDevice> endDevices = new List<IEndDevice>();
            endDevices.AddRange(catalogs.Devices);
            endDevices.AddRange(catalogs.Valves);
            foreach (DataRow row in data.Rows)
            {
                Guid subScopeID = new Guid(row[SubScopeDeviceTable.SubScopeID.Name].ToString());
                Guid deviceID = new Guid(row[SubScopeDeviceTable.DeviceID.Name].ToString());
                IEndDevice device = endDevices.First(item => item.Guid == deviceID);
                int quantity = row[SubScopeDeviceTable.Quantity.Name].ToString().ToInt();
                for(int x = 0; x < quantity; x++)
                {
                    if (dictionary.ContainsKey(subScopeID))
                    {
                        dictionary[subScopeID].Add(device);
                    }
                    else
                    {
                        dictionary[subScopeID] = new List<IEndDevice> { device };
                    }
                }
               
            }
            return dictionary;
        }
        private static Dictionary<Guid, List<TECPoint>> getSubScopePoints()
        {
            TableBase table = new PointTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            List<TECPoint> points = new List<TECPoint>();
            foreach(DataRow row in data.Rows)
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
                } else
                {
                    dictionary[subScopeID] = new List<TECPoint> { point };
                }
            }
            return dictionary;
        }
        private static Dictionary<Guid, List<Guid>> getScopeTags()
        {
            Dictionary<Guid, List<Guid>> dictionary = new Dictionary<Guid, List<Guid>>();
            TableBase table = new ScopeTagTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach(DataRow row in data.Rows)
            {
                Guid scopeGuid = new Guid(row[ScopeTagTable.ScopeID.Name].ToString());
                Guid childGuid = new Guid(row[ScopeTagTable.TagID.Name].ToString());
                if (dictionary.ContainsKey(scopeGuid))
                {
                    dictionary[scopeGuid].Add(childGuid);
                } else
                {
                    dictionary[scopeGuid] = new List<Guid> { childGuid };
                }
            }
            return dictionary;
        }
        private static Dictionary<Guid, List<Guid>> getScopeAssociatedCosts()
        {
            Dictionary<Guid, List<Guid>> dictionary = new Dictionary<Guid, List<Guid>>();
            TableBase table = new ScopeAssociatedCostTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            {
                int quantity = row[ScopeAssociatedCostTable.Quantity.Name].ToString().ToInt();
                Guid scopeGuid = new Guid(row[ScopeAssociatedCostTable.ScopeID.Name].ToString());
                Guid childGuid = new Guid(row[ScopeAssociatedCostTable.AssociatedCostID.Name].ToString());
                for (int x = 0; x < quantity; x++) {
                    if (dictionary.ContainsKey(scopeGuid))
                    {
                        dictionary[scopeGuid].Add(childGuid);
                    }
                    else
                    {
                        dictionary[scopeGuid] = new List<Guid> { childGuid };
                    }
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
        private static void populateControllerTypeIOModules(TECControllerType type, Dictionary<Guid, List<TECIOModule>> modules)
        {
            if (modules.ContainsKey(type.Guid))
            {
                modules[type.Guid].ForEach(item => type.IOModules.Add(item));
            }
        }
        private static void populateRatedCostInMaterial(TECElectricalMaterial material, Dictionary<Guid, List<TECAssociatedCost>> ratedCosts)
        {
            if (ratedCosts.ContainsKey(material.Guid))
            {
                ratedCosts[material.Guid].ForEach(item => material.RatedCosts.Add(item));
            }
        }
        private static void populateConduitTypesInConnection(TECConnection connection, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            if (connectionConduitTypes.ContainsKey(connection.Guid))
            {
                connection.ConduitType = connectionConduitTypes[connection.Guid];
            }
        }
        private static void populateConnectionTypesInConnection(TECConnection connection, Dictionary<Guid, List<TECConnectionType>> connectionConnectionTypes)
        {
            if (connection is TECNetworkConnection netConnection &&
                        connectionConnectionTypes.ContainsKey(netConnection.Guid))
            {
                connectionConnectionTypes[connection.Guid].ForEach(item => netConnection.ConnectionTypes.Add(item));
            }
        }
        private static void populateNetworkConnectionChildren(TECConnection connection, Dictionary<Guid, List<INetworkConnectable>> connectables)
        {
            if (connection is TECNetworkConnection netConnection &&
                        connectables.ContainsKey(netConnection.Guid))
            {
                connectables[connection.Guid].ForEach(item => netConnection.Children.Add(item));
            }
        }
        private static void populateSubScopeConnectionChildren(TECConnection connection, Dictionary<Guid, List<TECSubScope>> subScope)
        {
            if (connection is TECSubScopeConnection subConnection &&
                        subScope.ContainsKey(subConnection.Guid))
            {
                subScope[subConnection.Guid].ForEach(item => subConnection.SubScope = item);
            }
        }

        private static List<T> getAll<T>(ITECObject obj) where T : ITECObject
        {
            List<T> list = new List<T>();
            if(obj is T typedObj)
            {
                list.Add(typedObj);
            }
            if(obj is IRelatable rel)
            {
                foreach(var item in rel.PropertyObjects.Objects.Where(x => !rel.LinkedObjects.Contains(x)))
                {
                    list.AddRange(getAll<T>(item));
                }
            }
            return list;
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
            catalogs.Manufacturers = getObjectsFromTable(new ManufacturerTable(), getManufacturerFromRow);
            catalogs.ConnectionTypes = getObjectsFromTable(new ConnectionTypeTable(), getConnectionTypeFromRow);
            catalogs.ConduitTypes = getObjectsFromTable(new ConduitTypeTable(), getConduitTypeFromRow);
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getOneToOneRelationships(new HardwareManufacturerTable(), 
                HardwareManufacturerTable.HardwareID.Name, HardwareManufacturerTable.ManufacturerID.Name, catalogs.Manufacturers);
            Dictionary<Guid, List<TECConnectionType>> deviceConnectionType = getOneToManyRelationships(new DeviceConnectionTypeTable(),
                DeviceConnectionTypeTable.DeviceID.Name, DeviceConnectionTypeTable.TypeID.Name, catalogs.ConnectionTypes, DeviceConnectionTypeTable.Quantity.Name);
            catalogs.Devices = getObjectsFromTable(new DeviceTable(), data => { return getDeviceFromRow(data, hardwareManufacturer, deviceConnectionType); });
            Dictionary<Guid, TECDevice> actuators = getOneToOneRelationships(new ValveActuatorTable(), ValveActuatorTable.ValveID.Name, ValveActuatorTable.ActuatorID.Name, catalogs.Devices);
            catalogs.Valves = getObjectsFromTable(new ValveTable(), data => { return getValveFromRow(data, hardwareManufacturer, actuators); });
            catalogs.AssociatedCosts = getObjectsFromTable(new AssociatedCostTable(), getAssociatedCostFromRow);
            catalogs.PanelTypes = getObjectsFromTable(new PanelTypeTable(), data => { return getPanelTypeFromRow(data, hardwareManufacturer); });
            catalogs.IOModules = getObjectsFromTable(new IOModuleTable(), data => { return getIOModuleFromRow(data, hardwareManufacturer); });
            catalogs.ControllerTypes = getObjectsFromTable(new ControllerTypeTable(), data => { return getControllerTypeFromRow(data, hardwareManufacturer); });
            catalogs.Tags = getObjectsFromTable(new TagTable(), getTagFromRow);


            Dictionary<Guid, List<TECAssociatedCost>> ratedCostsRelationShips = getOneToManyRelationships(new ElectricalMaterialRatedCostTable(),
                ElectricalMaterialRatedCostTable.ComponentID.Name, ElectricalMaterialRatedCostTable.CostID.Name, catalogs.AssociatedCosts, ElectricalMaterialRatedCostTable.Quantity.Name);

            Dictionary<Guid, List<TECIOModule>> controllerTypeModuleRelationships = getOneToManyRelationships(new ControllerTypeIOModuleTable(),
                ControllerTypeIOModuleTable.TypeID.Name, ControllerTypeIOModuleTable.ModuleID.Name, catalogs.IOModules, ControllerTypeIOModuleTable.Quantity.Name);

            catalogs.ControllerTypes.ForEach(item => populateControllerTypeIOModules(item, controllerTypeModuleRelationships));
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

        public static TECBid GetBidInfo(SQLiteDatabase db)
        {
            DataTable bidInfoDT = db.GetDataFromTable(BidInfoTable.TableName);
            if (bidInfoDT.Rows.Count < 1)
            {
                logger.Error("Bid info not found in database. Bid info and labor will be missing.");
                return new TECBid();
            }

            DataRow bidInfoRow = bidInfoDT.Rows[0];

            TECBid outBid = new TECBid(new Guid(bidInfoRow[BidInfoTable.ID.Name].ToString()));
            assignValuePropertiesFromTable(outBid, new BidInfoTable(), bidInfoRow);

            string dueDateString = bidInfoRow[BidInfoTable.DueDate.Name].ToString();
            outBid.DueDate = DateTime.ParseExact(dueDateString, DB_FMT, CultureInfo.InvariantCulture);
            
            return outBid;
        }
        public static TECTemplates GetTemplatesInfo(SQLiteDatabase db)
        {
            DataTable templateInfoDT = db.GetDataFromTable(TemplatesInfoTable.TableName);

            if (templateInfoDT.Rows.Count < 1)
            {
                logger.Error("Template info not found in database.");
                return new TECTemplates();
            }
            DataRow templateInfoRow = templateInfoDT.Rows[0];

            Guid infoGuid = new Guid(templateInfoRow[TemplatesInfoTable.ID.Name].ToString());

            return new TECTemplates(infoGuid);
        }
        static private TECExtraLabor getExtraLabor(TECBid bid)
        {
            DataTable DT = SQLiteDB.GetDataFromTable(ExtraLaborTable.TableName);
            if (DT.Rows.Count > 1)
            {
                logger.Error("Multiple rows found in extra labor table. Using first found.");
            }
            else if (DT.Rows.Count < 1)
            {
                logger.Error("Extra labor not found in database, using default values. Reload labor constants from loaded templates in the labor tab.");
                return new TECExtraLabor(bid.Guid);
            }
            return getExtraLaborFromRow(DT.Rows[0]);
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
        
        static private TECParameters getBidParameters(TECBid bid)
        {
            string constsCommand = "select " + DatabaseHelper.AllFieldsInTableString(new ParametersTable()) + " from " + ParametersTable.TableName;

            DataTable DT = SQLiteDB.GetDataFromCommand(constsCommand);

            if (DT.Rows.Count > 1)
            {
                logger.Error("Multiple rows found in bid paramters table. Using first found.");
            }
            else if (DT.Rows.Count < 1)
            {
                logger.Error("Bid paramters not found in database, using default values. Reload labor constants from loaded templates in the labor tab.");
                return new TECParameters(bid.Guid);
            }
            return getParametersFromRow(DT.Rows[0]);
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
        private static TECConnectionType getConnectionTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ConnectionTypeTable.ID.Name].ToString());
            var outConnectionType = new TECConnectionType(guid);
            assignValuePropertiesFromTable(outConnectionType, new ConnectionTypeTable(), row);
            return outConnectionType;
        }
        private static TECElectricalMaterial getConduitTypeFromRow(DataRow row)
        {
            Guid conduitGuid = new Guid(row[ConduitTypeTable.ID.Name].ToString());
            var conduitType = new TECElectricalMaterial(conduitGuid);
            assignValuePropertiesFromTable(conduitType, new ConduitTypeTable(), row);
            return conduitType;
        }
        private static TECAssociatedCost getAssociatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[AssociatedCostTable.ID.Name].ToString());
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[AssociatedCostTable.Type.Name].ToString());
            var associatedCost = new TECAssociatedCost(guid, type);
            assignValuePropertiesFromTable(associatedCost, new AssociatedCostTable(), row);
            return associatedCost;
        }
        private static TECDevice getDeviceFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers, Dictionary<Guid, List<TECConnectionType>> connectionTypes)
        {
            Guid deviceID = new Guid(row[DeviceTable.ID.Name].ToString());
            ObservableCollection<TECConnectionType> connectionType = connectionTypes.ContainsKey(deviceID) ? 
                new ObservableCollection<TECConnectionType>(connectionTypes[deviceID]) : new ObservableCollection<TECConnectionType>();
            TECManufacturer manufacturer = manufacturers[deviceID];
            TECDevice deviceToAdd = new TECDevice(deviceID, connectionType, manufacturer);
            assignValuePropertiesFromTable(deviceToAdd, new DeviceTable(), row);
            return deviceToAdd;
        }
        private static TECManufacturer getManufacturerFromRow(DataRow row)
        {
            Guid manufacturerID = new Guid(row[ManufacturerTable.ID.Name].ToString());
            var manufacturer = new TECManufacturer(manufacturerID);
            assignValuePropertiesFromTable(manufacturer, new ManufacturerTable(), row);
            return manufacturer;
        }
        private static TECLocation getLocationFromRow(DataRow row)
        {
            Guid locationID = new Guid(row[LocationTable.ID.Name].ToString());
            TECLocation location = new TECLocation(locationID);
            assignValuePropertiesFromTable(location, new LocationTable(), row);
            return location;
        }
        private static TECTag getTagFromRow(DataRow row)
        {
            var tag = new TECTag(new Guid(row[TagTable.ID.Name].ToString()));
            assignValuePropertiesFromTable(tag, new TagTable(), row);
            return tag;
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
            } else
            {
                throw new KeyNotFoundException();
            }
            TECPanelType panelType = new TECPanelType(guid, manufacturer);
            assignValuePropertiesFromTable(panelType, new PanelTypeTable(), row);
            return panelType;
        }
        private static TECIOModule getIOModuleFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid guid = new Guid(row[IOModuleTable.ID.Name].ToString());
            TECManufacturer manufacturer = manufacturers[guid];
            TECIOModule module = new TECIOModule(guid, manufacturer);
            module.IO = getChildObjects(new IOModuleIOTable(), new IOTable(), guid, getIOFromRow);
            assignValuePropertiesFromTable(module, new IOModuleTable(), row);
            return module;
        }
        private static TECControllerType getControllerTypeFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid guid = new Guid(row[ControllerTypeTable.ID.Name].ToString());
            TECManufacturer manufacturer = manufacturers[guid];
            TECControllerType controllerType = new TECControllerType(guid, manufacturer);
            controllerType.IO = getChildObjects(new ControllerTypeIOTable(), new IOTable(), guid, getIOFromRow);
            assignValuePropertiesFromTable(controllerType, new ControllerTypeTable(), row);
            return controllerType;
        }
        private static TECValve getValveFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers, Dictionary<Guid, TECDevice> actuators)
        {
            Guid id = new Guid(row[DeviceTable.ID.Name].ToString());
            TECManufacturer manufacturer = manufacturers[id];
            TECDevice actuator = actuators[id];
            TECValve valve = new TECValve(id, manufacturer, actuator);
            assignValuePropertiesFromTable(valve, new ValveTable(), row);
            return valve;
        }
        private static TECScopeBranch getScopeBranchFromRow(DataRow row, bool isTypical)
        {
            Guid scopeBranchID = new Guid(row[ScopeBranchTable.ID.Name].ToString());
            TECScopeBranch branch = new TECScopeBranch(scopeBranchID, isTypical);
            assignValuePropertiesFromTable(branch, new ScopeBranchTable(), row);
            branch.Branches = getChildObjects(new ScopeBranchHierarchyTable(), new ScopeBranchTable(), scopeBranchID, data => { return getScopeBranchFromRow(data, isTypical); });
            return branch;
        }
        private static TECLabeled getNoteFromRow(DataRow row)
        {
            Guid noteID = new Guid(row[NoteTable.ID.Name].ToString());
            var note = new TECLabeled(noteID);
            assignValuePropertiesFromTable(note, new NoteTable(), row);
            return note;
        }
        private static TECLabeled getExclusionFromRow(DataRow row)
        {
            Guid exclusionId = new Guid(row[ExclusionTable.ID.Name].ToString());
            TECLabeled exclusion = new TECLabeled(exclusionId);
            assignValuePropertiesFromTable(exclusion, new ExclusionTable(), row);
            return exclusion;
        }
        private static TECPanel getPanelFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = panelTypes[guid];
            TECPanel panel = new TECPanel(guid, type, isTypical);
            assignValuePropertiesFromTable(panel, new PanelTable(), row);

            return panel;
        }
        private static TECController getControllerFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECController controller = new TECController(guid, controllerTypes[guid], isTypical);
            assignValuePropertiesFromTable(controller, new ControllerTable(), row);
            controller.ChildrenConnections = getConnectionsInController(controller, isTypical);
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
        private static TECParameters getParametersFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ParametersTable.ID.Name].ToString());
            TECParameters paramters = new TECParameters(guid);
            paramters.DesiredConfidence = UtilitiesMethods.StringToEnum<Confidence>(row[ParametersTable.DesiredConfidence.Name].ToString());
            assignValuePropertiesFromTable(paramters, new ParametersTable(), row);
            return paramters;
        }
        private static TECExtraLabor getExtraLaborFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ExtraLaborTable.ID.Name].ToString());
            TECExtraLabor labor = new TECExtraLabor(guid);
            assignValuePropertiesFromTable(labor, new ExtraLaborTable(), row);
            return labor;
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
        private static void assignValuePropertiesFromTable(object item, TableBase table, DataRow row)
        {
            foreach(TableField field in table.Fields)
            {
                if (field.Property.DeclaringType.IsInstanceOfType(item) && field.Property.SetMethod != null)
                {
                    if(field.Property.PropertyType == typeof(string))
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
                }
            }
        }
        #endregion

        #region Generic Database Query Methods
        private static Dictionary<Guid, List<T>> getOneToManyRelationships<T>(TableBase table, String parentField, String childField, IEnumerable<T> references, string qtyField = "") where T : ITECObject
        {
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
        private static Dictionary<Guid, T> getOneToOneRelationships<T>(TableBase table, String parentField, String childField, IEnumerable<T> references) where T : ITECObject
        {
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
            ObservableCollection<T> items = new ObservableCollection<T>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(table), table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            { items.Add(dataHandler(row)); }
            return items;
        }
        static private ObservableCollection<T> getChildObjects<T>(TableBase relationTable, TableBase childTable, Guid parentID, Func<DataRow, T> dataHandler)
        {
            ObservableCollection<T> children = new ObservableCollection<T>();
            DataTable data = getChildData(relationTable, childTable, parentID);
            foreach (DataRow row in data.Rows)
            {
                children.Add(dataHandler(row));
            }
            return children;
        }
        private static DataTable getChildData(TableBase relationTable, TableBase childTable, Guid parentID)
        {
            string orderKey = relationTable.IndexString;
            string orderString = "";
            if(orderKey != "")
            {
                orderString = string.Format(" order by {0}", orderKey);
            }
            if(relationTable.PrimaryKeys.Count != 2)
            {
                throw new Exception("Relation table must have primary keys for each object");
            }
            if(childTable.PrimaryKeys.Count != 1)
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
        private static DataTable getChildIDs(TableBase relationTable, Guid parentID, string quantityField = "")
        {

            if (relationTable.PrimaryKeys.Count != 2)
            {
                throw new Exception("Relation table must have primary keys for each object");
            }
            string fieldString = relationTable.PrimaryKeys[1].Name;
            if(quantityField != "")
            {
                fieldString += ", " + quantityField;
            }
            string command = string.Format("select {0} from {1} where {2} = '{3}'",
                fieldString, relationTable.NameString,
                relationTable.PrimaryKeys[0].Name, parentID.ToString());
            return SQLiteDB.GetDataFromCommand(command);
        }
        #endregion
    }
}
