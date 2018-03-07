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

        #region Loading from DB Methods
        static private (TECBid bid, bool needsUpdate) loadBid()
        {
            TECBid bid = GetBidInfo(SQLiteDB);
            
            Dictionary<Guid, List<Guid>> tags = getScopeTags();
            Dictionary<Guid, List<Guid>> costs = getScopeAssociatedCosts();

            getScopeManagerProperties(bid, tags, costs);

            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            Dictionary<Guid, List<IEndDevice>> endDevices = getEndDevices(bid.Catalogs);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(), 
                ConnectionConduitTypeTable.ConnectionID.Name, ConnectionConduitTypeTable.TypeID.Name, bid.Catalogs.ConduitTypes);
            bid.Locations = getItemsFromTable(new LocationTable(), getLocationFromRow);
            Dictionary<Guid, TECLocation> locationDictionary = getOneToOneRelationships(new LocatedLocationTable(),
                LocatedLocationTable.ScopeID.Name, LocatedLocationTable.LocationID.Name, bid.Locations);
            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(),
                ControllerControllerTypeTable.ControllerID.Name, ControllerControllerTypeTable.TypeID.Name, bid.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(),
                PanelPanelTypeTable.PanelID.Name, PanelPanelTypeTable.PanelTypeID.Name, bid.Catalogs.PanelTypes);


            bid.Parameters = getBidParameters(bid);
            bid.ExtraLabor = getExtraLabor(bid);
            bid.Schedule = getSchedule(bid);
            bid.ScopeTree = getBidScopeBranches();
            bid.Systems = getChildObjects(new BidSystemTable(), new SystemTable(),
                bid.Guid, data => { return getTypicalFromRow(data, controllerTypeDictionary, panelTypeDictionary); });
            bid.Notes = getItemsFromTable(new NoteTable(), getNoteFromRow);
            bid.Exclusions = getItemsFromTable(new ExclusionTable(), getNoteFromRow);
            bid.SetControllers(getOrphanControllers(controllerTypeDictionary));
            bid.MiscCosts = getMiscInBid(bid.Guid);
            bid.Panels = getOrphanPanels(panelTypeDictionary);
            var placeholderDict = getCharacteristicInstancesList();
            populateTypicalProperties(bid.Systems, tags, costs, points, endDevices, connectionConduitTypes);
            bid.Controllers.ForEach(controller => populateConduitTypesInControllerConnections(controller, connectionConduitTypes));
            populateScopeProperties(bid, tags, costs);
            populateLocatedProperties(bid, locationDictionary);

            bool needsSave = ModelLinkingHelper.LinkBid(bid, placeholderDict);

            return (bid, needsSave);
        }
        
        static private (TECTemplates templates, bool needsUpdate) loadTemplates()
        {
            TECTemplates templates = new TECTemplates();
            templates = GetTemplatesInfo(SQLiteDB);

            Dictionary<Guid, List<Guid>> tags = getScopeTags();
            Dictionary<Guid, List<Guid>> costs = getScopeAssociatedCosts();
            getScopeManagerProperties(templates, tags, costs);

            Dictionary<Guid, List<TECPoint>> points = getSubScopePoints();
            Dictionary<Guid, List<IEndDevice>> endDevices = getEndDevices(templates.Catalogs);
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getOneToOneRelationships(new ConnectionConduitTypeTable(),
                ConnectionConduitTypeTable.ConnectionID.Name, ConnectionConduitTypeTable.TypeID.Name, templates.Catalogs.ConduitTypes);

            Dictionary<Guid, TECControllerType> controllerTypeDictionary = getOneToOneRelationships(new ControllerControllerTypeTable(),
                ControllerControllerTypeTable.ControllerID.Name, ControllerControllerTypeTable.TypeID.Name, templates.Catalogs.ControllerTypes);
            Dictionary<Guid, TECPanelType> panelTypeDictionary = getOneToOneRelationships(new PanelPanelTypeTable(),
                PanelPanelTypeTable.PanelID.Name, PanelPanelTypeTable.PanelTypeID.Name, templates.Catalogs.PanelTypes);

            templates.SystemTemplates = getSystemTemplates(controllerTypeDictionary, panelTypeDictionary);
            templates.EquipmentTemplates = getEquipmentTemplates();
            templates.SubScopeTemplates = getSubScopeTemplates();
            templates.ControllerTemplates = getControllerTemplates(controllerTypeDictionary);
            templates.MiscCostTemplates = getMiscTemplates();
            templates.PanelTemplates = getPanelTemplates(panelTypeDictionary);
            templates.Parameters = getTemplatesParameters();
            populateTemplates(templates, tags, costs, points, endDevices, connectionConduitTypes);

            Dictionary<Guid, List<Guid>> templateReferences = getTemplateReferences();
            bool needsSave = ModelLinkingHelper.LinkTemplates(templates, templateReferences);
            return (templates, needsSave);
        }

        private static void populateTemplates(TECTemplates templates, Dictionary<Guid, List<Guid>> tags, 
            Dictionary<Guid, List<Guid>> costs, Dictionary<Guid, List<TECPoint>> points, 
            Dictionary<Guid, List<IEndDevice>> endDevices, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            populateScopeProperties(templates, tags, costs);

            foreach (TECSystem system in templates.SystemTemplates)
            {
                foreach(TECSubScope subScope in system.GetAllSubScope())
                {
                    populateSubScopeChildren(subScope, points, endDevices);
                }
                foreach(TECController controller in system.Controllers)
                {
                    populateConduitTypesInControllerConnections(controller, connectionConduitTypes);
                }
            }
            foreach(TECEquipment equpiment in templates.EquipmentTemplates)
            {
                foreach (TECSubScope subScope in equpiment.SubScope)
                {
                    populateSubScopeChildren(subScope, points, endDevices);
                }
            }
            foreach(TECSubScope subScope in templates.SubScopeTemplates)
            {
                populateSubScopeChildren(subScope, points, endDevices);
            }
            
        }

        static private void getScopeManagerProperties(TECScopeManager scopeManager, Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs)
        {
            scopeManager.Catalogs = getCatalogs(tags, costs);
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
        private static Dictionary<Guid, List<T>> getOneToManyRelationships<T>(TableBase table, String parentField, String childField, IEnumerable<T> references, string qtyField = "") where T : TECObject
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
                if(qtyField != "")
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
        private static Dictionary<Guid, T> getOneToOneRelationships<T>(TableBase table, String parentField, String childField, IEnumerable<T> references) where T : TECObject
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

        private static void populateTypicalProperties(IEnumerable<TECTypical> typicals, Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs,
            Dictionary<Guid, List<TECPoint>> points, Dictionary<Guid, List<IEndDevice>> devices, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            foreach(TECTypical typical in typicals)
            {
                foreach(TECSubScope subScope in typical.GetAllSubScope())
                {
                    populateSubScopeChildren(subScope, points, devices);
                }
                foreach (TECController controller in typical.Controllers)
                {
                    populateConduitTypesInControllerConnections(controller, connectionConduitTypes);
                }
                foreach (TECSystem instance in typical.Instances)
                {
                    foreach (TECSubScope subScope in instance.GetAllSubScope())
                    {
                        populateSubScopeChildren(subScope, points, devices);
                    }
                    foreach(TECController controller in instance.Controllers)
                    {
                        populateConduitTypesInControllerConnections(controller, connectionConduitTypes);
                    }
                }
                
            }
        }

        private static void populateConduitTypesInControllerConnections(TECController controller, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            foreach(TECConnection connection in controller.ChildrenConnections)
            {
                if (connectionConduitTypes.ContainsKey(connection.Guid))
                {
                    connection.ConduitType = connectionConduitTypes[connection.Guid];
                }
            }
        }
        private static void populateSubScopeChildren(TECSubScope subScope, Dictionary<Guid, List<TECPoint>> points, Dictionary<Guid, List<IEndDevice>> devices)
        {
            if (points.ContainsKey(subScope.Guid))
            {
                List<TECPoint> allPoints = new List<TECPoint>();
                foreach(TECPoint point in points[subScope.Guid])
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
                foreach(IEndDevice device in devices[subScope.Guid])
                {
                    allDevices.Add(device);
                }
                subScope.Devices = new ObservableCollection<IEndDevice>(allDevices);
            }
        }
        private static void populateScopeProperties(TECObject obj, Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs)
        {
            if(obj is TECScope scope)
            {
                if (tags.ContainsKey(scope.Guid))
                {
                    List<TECTag> allTags = new List<TECTag>();
                    foreach (Guid tagID in tags[scope.Guid])
                    {
                        allTags.Add(new TECTag(tagID));
                    }
                    scope.Tags = new ObservableCollection<TECTag>(allTags);
                }
                if (costs.ContainsKey(scope.Guid))
                {
                    List<TECAssociatedCost> allCosts = new List<TECAssociatedCost>();
                    foreach (Guid costID in costs[scope.Guid])
                    {
                        allCosts.Add(new TECAssociatedCost(costID, CostType.TEC));
                    }
                    scope.AssociatedCosts = new ObservableCollection<TECAssociatedCost>(allCosts);
                }
            }
            
            if(obj is IRelatable rel)
            {
                foreach(TECObject item in rel.PropertyObjects.Objects)
                {
                    if(item is TECScope childScope && !rel.LinkedObjects.Contains(item))
                    {
                        populateScopeProperties(childScope, tags, costs);
                    }
                }
            }
        }
        private static void populateLocatedProperties(TECObject obj, Dictionary<Guid, TECLocation> locations)
        {
            if (obj is TECLocated located)
            {
                if (locations.ContainsKey(located.Guid))
                {
                    located.Location = locations[located.Guid];
                }
            }

            if (obj is IRelatable rel)
            {
                foreach (TECObject item in rel.PropertyObjects.Objects)
                {
                    if (item is TECScope childScope && !rel.LinkedObjects.Contains(item))
                    {
                        populateLocatedProperties(childScope, locations);
                    }
                }
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
        #region Catalogs
        static private TECCatalogs getCatalogs(Dictionary<Guid, List<Guid>> tagRelationsips, Dictionary<Guid, List<Guid>> costRelationships)
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Manufacturers = getItemsFromTable(new ManufacturerTable(), getManufacturerFromRow);
            catalogs.ConnectionTypes = getItemsFromTable(new ConnectionTypeTable(), getConnectionTypeFromRow);
            catalogs.ConduitTypes = getItemsFromTable(new ConduitTypeTable(), getConduitTypeFromRow);
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getOneToOneRelationships(new HardwareManufacturerTable(), 
                HardwareManufacturerTable.HardwareID.Name, HardwareManufacturerTable.ManufacturerID.Name, catalogs.Manufacturers);
            Dictionary<Guid, List<TECConnectionType>> deviceConnectionType = getOneToManyRelationships(new DeviceConnectionTypeTable(),
                DeviceConnectionTypeTable.DeviceID.Name, DeviceConnectionTypeTable.TypeID.Name, catalogs.ConnectionTypes, DeviceConnectionTypeTable.Quantity.Name);
            catalogs.Devices = getItemsFromTable(new DeviceTable(), data => { return getDeviceFromRow(data, hardwareManufacturer, deviceConnectionType); });
            Dictionary<Guid, TECDevice> actuators = getOneToOneRelationships(new ValveActuatorTable(), ValveActuatorTable.ValveID.Name, ValveActuatorTable.ActuatorID.Name, catalogs.Devices);
            catalogs.Valves = getItemsFromTable(new ValveTable(), data => { return getValveFromRow(data, hardwareManufacturer, actuators); });
            catalogs.AssociatedCosts = getItemsFromTable(new AssociatedCostTable(), getAssociatedCostFromRow);
            catalogs.PanelTypes = getItemsFromTable(new PanelTypeTable(), data => { return getPanelTypeFromRow(data, hardwareManufacturer); });
            catalogs.IOModules = getItemsFromTable(new IOModuleTable(), data => { return getIOModuleFromRow(data, hardwareManufacturer); });
            catalogs.ControllerTypes = getItemsFromTable(new ControllerTypeTable(), data => { return getControllerTypeFromRow(data, hardwareManufacturer); });
            catalogs.Tags = getItemsFromTable(new TagTable(), getTagFromRow);

            populateScopeProperties(catalogs, tagRelationsips, costRelationships);

            return catalogs;
        }
        
        static private ObservableCollection<T> getItemsFromTable<T>(TableBase table, Func<DataRow, T> dataHandler)
        {
            ObservableCollection<T> items = new ObservableCollection<T>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(table), table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            { items.Add(dataHandler(row)); }
            return items;
        }
        static private ObservableCollection<TECAssociatedCost> getRatedCostsInComponent(Guid componentID)
        {

            string command = "select " + ElectricalMaterialRatedCostTable.CostID.Name + ", " + ElectricalMaterialRatedCostTable.Quantity.Name + " from " + ElectricalMaterialRatedCostTable.TableName + " where ";
            command += ElectricalMaterialRatedCostTable.ComponentID.Name + " = '" + componentID;
            command += "'";
            DataTable DT = SQLiteDB.GetDataFromCommand(command);
            var costs = new ObservableCollection<TECAssociatedCost>();
            foreach (DataRow row in DT.Rows)
            {
                TECAssociatedCost costToAdd = getPlaceholderRatedCostFromRow(row);
                int quantity = row[ElectricalMaterialRatedCostTable.Quantity.Name].ToString().ToInt();
                for (int x = 0; x < quantity; x++) { costs.Add(costToAdd); }
            }
            return costs;
        }
        #endregion
        #region System Components
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
        #endregion

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

        static private ObservableCollection<T> getChildObjects<T>(TableBase relationTable, TableBase childTable, Guid parentID, Func<DataRow,T> dataHandler) where T: TECObject
        {
            ObservableCollection<T> children = new ObservableCollection<T>();
            DataTable data = getChildData(relationTable, childTable, parentID);
            foreach (DataRow row in data.Rows)
            {
                children.Add(dataHandler(row));
            }
            return children;
        }

        static private ObservableCollection<TECScopeBranch> getBidScopeBranches()
        {
            ObservableCollection<TECScopeBranch> mainBranches = new ObservableCollection<TECScopeBranch>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ScopeBranchTable()) + " from " + ScopeBranchTable.TableName;
            command += " where " + ScopeBranchTable.ID.Name;
            command += " in (select " + BidScopeBranchTable.ScopeBranchID.Name;
            command += " from " + BidScopeBranchTable.TableName + " where " + BidScopeBranchTable.ScopeBranchID.Name + " not in ";
            command += "(select " + ScopeBranchHierarchyTable.ChildID.Name + " from " + ScopeBranchHierarchyTable.TableName + "))";

            DataTable mainBranchDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in mainBranchDT.Rows)
            {
                mainBranches.Add(getScopeBranchFromRow(row, false));
            }

            return mainBranches;
        }
        static private ObservableCollection<TECConnectionType> getConnectionTypesInNetworkConnection(Guid netConnectionID)
        {
            ObservableCollection<TECConnectionType> outTypes = new ObservableCollection<TECConnectionType>();
            DataTable connectionTypeDT = getChildIDs(new NetworkConnectionConnectionTypeTable(), netConnectionID);
            foreach(DataRow row in connectionTypeDT.Rows)
            {
                outTypes.Add(getPlaceholderConnectionTypeFromRow(row,
                    NetworkConnectionConnectionTypeTable.TypeID.Name));
            }
            return outTypes;
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
        static private ObservableCollection<TECIOModule> getIOModuleInController(Guid guid)
        {
            ObservableCollection<TECIOModule> outModules = new ObservableCollection<TECIOModule>();
            string command = string.Format("select {0}, {4} from {1} where {2} = '{3}'",
                ControllerIOModuleTable.ModuleID.Name, ControllerIOModuleTable.TableName,
                ControllerIOModuleTable.ControllerID.Name, guid, ControllerIOModuleTable.Quantity.Name);
            DataTable dt = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in dt.Rows)
            {
                var module = getPlaceholderIOModuleFromRow(row);
                int quantity = row[ControllerIOModuleTable.Quantity.Name].ToString().ToInt(1);
                for (int x = 0; x < quantity; x++)
                { outModules.Add(module); }
            }
            return outModules;
        }
        static private ObservableCollection<TECIOModule> getIOModuleInControllerType(Guid guid)
        {
            ObservableCollection<TECIOModule> outModules = new ObservableCollection<TECIOModule>();
            string command = string.Format("select {0}, {4} from {1} where {2} = '{3}'",
                ControllerTypeIOModuleTable.ModuleID.Name, ControllerTypeIOModuleTable.TableName,
                ControllerTypeIOModuleTable.TypeID.Name, guid, ControllerTypeIOModuleTable.Quantity.Name);
            DataTable dt = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in dt.Rows)
            {
                var module = getPlaceholderIOModuleFromRow(row);
                int quantity = row[ControllerTypeIOModuleTable.Quantity.Name].ToString().ToInt(1);
                for (int x = 0; x < quantity; x++)
                { outModules.Add(module); }
            }
            return outModules;
        }
        
        static private TECSubScope getSubScopeInSubScopeConnection(Guid connectionID, bool isTypical)
        {
            TECSubScope outScope = null;

            //string command = "select * from " + SubScopeTable.TableName + " where " + SubScopeTable.SubScopeID.Name + " in ";
            //command += "(select " + SubScopeConnectionChildrenTable.ChildID.Name + " from " + SubScopeConnectionChildrenTable.TableName + " where ";
            //command += SubScopeConnectionChildrenTable.ConnectionID.Name + " = '" + connectionID;
            //command += "')";
            string command = "select " + SubScopeConnectionChildrenTable.ChildID.Name + " from " + SubScopeConnectionChildrenTable.TableName + " where ";
            command += SubScopeConnectionChildrenTable.ConnectionID.Name + " = '" + connectionID;
            command += "'";

            DataTable scopeDT = SQLiteDB.GetDataFromCommand(command);
            if (scopeDT.Rows.Count > 0)
            {
                return getSubScopeConnectionChildPlaceholderFromRow(scopeDT.Rows[0], isTypical);
            }

            return outScope;
        }
        static private ObservableCollection<INetworkConnectable> getChildrenInNetworkConnection(Guid connectionID, bool isTypical)
        {
            var outScope = new ObservableCollection<INetworkConnectable>();

            DataTable dt = getChildData(new NetworkConnectionChildrenTable(), new ControllerTable(), connectionID);
            foreach (DataRow row in dt.Rows)
            { outScope.Add(getControllerPlaceholderFromRow(row, isTypical)); }

            dt = getChildData(new NetworkConnectionChildrenTable(), new SubScopeTable(), connectionID);
            foreach (DataRow row in dt.Rows)
            { outScope.Add(getPlaceholderSubScopeFromRow(row, isTypical)); }

            return outScope;
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
            return getBidParametersFromRow(DT.Rows[0]);
        }
        static private ObservableCollection<TECParameters> getTemplatesParameters()
        {
            ObservableCollection<TECParameters> outParameters = new ObservableCollection<TECParameters>();
            string constsCommand = "select " + DatabaseHelper.AllFieldsInTableString(new ParametersTable()) + " from " + ParametersTable.TableName;
            
            DataTable DT = SQLiteDB.GetDataFromCommand(constsCommand);
            foreach(DataRow row in DT.Rows)
            {
                outParameters.Add(getBidParametersFromRow(row));
            }
            return outParameters;
        }
        static private ObservableCollection<TECMisc> getAllMisc()
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();

            DataTable miscDT = SQLiteDB.GetDataFromTable(MiscTable.TableName);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row, false));
            }

            return misc;
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
        static private ObservableCollection<TECSystem> getSystems(Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            ObservableCollection<TECSystem> systems = new ObservableCollection<TECSystem>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SystemTable()) + " from " + SystemTable.TableName;
            command += " where " + SystemTable.ID.Name;
            command += " in (select " + SystemTable.ID.Name;
            command += " from " + SystemTable.TableName + " where " + SystemTable.ID.Name + " not in ";
            command += "(select " + SystemHierarchyTable.ChildID.Name + " from " + SystemHierarchyTable.TableName + "))";

            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable systemsDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in systemsDT.Rows)
            {
                systems.Add(getSystemFromRow(row, controllerTypes, panelTypes));
            }
            return systems;
        }
        static private ObservableCollection<TECController> getControllersInPanel(Guid guid, bool isTypical)
        {
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();
            string command = String.Format("select {0} from {1} where {2} = '{3}'",
                PanelControllerTable.ControllerID.Name, PanelControllerTable.TableName,
                PanelControllerTable.PanelID.Name, guid);

            DataTable controllerDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in controllerDT.Rows)
            { controllers.Add(getPlaceholderPanelControllerFromRow(row, isTypical)); }

            return controllers;
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
        
        
        #region Template Scope
        static private ObservableCollection<TECSystem> getSystemTemplates(Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            ObservableCollection<TECSystem> systems = new ObservableCollection<TECSystem>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new SystemTable()),
                SystemTable.TableName,
                SystemTable.ID.Name,
                TemplatesSystemTable.SystemID.Name,
                TemplatesSystemTable.TableName);

            DataTable systemsDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in systemsDT.Rows)
            {
                systems.Add(getSystemFromRow(row, controllerTypes, panelTypes));
            }
            return systems;
        }
        static private ObservableCollection<TECEquipment> getEquipmentTemplates()
        {
            ObservableCollection<TECEquipment> equipment = new ObservableCollection<TECEquipment>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new EquipmentTable()),
                EquipmentTable.TableName,
                EquipmentTable.ID.Name,
                TemplatesEquipmentTable.EquipmentID.Name,
                TemplatesEquipmentTable.TableName);

            DataTable equipmentDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in equipmentDT.Rows)
            {
                equipment.Add(getEquipmentFromRow(row, false));
            }
            return equipment;
        }
        static private ObservableCollection<TECSubScope> getSubScopeTemplates()
        {
            ObservableCollection<TECSubScope> subScope = new ObservableCollection<TECSubScope>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new SubScopeTable()),
                SubScopeTable.TableName,
                SubScopeTable.ID.Name,
                TemplatesSubScopeTable.SubScopeID.Name,
                TemplatesSubScopeTable.TableName);

            DataTable systemsDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in systemsDT.Rows)
            {
                subScope.Add(getSubScopeFromRow(row, false));
            }
            return subScope;
        }
        static private ObservableCollection<TECMisc> getMiscTemplates()
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new MiscTable()),
                MiscTable.TableName,
                MiscTable.ID.Name,
                TemplatesMiscCostTable.MiscID.Name,
                TemplatesMiscCostTable.TableName);

            DataTable miscDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row, false));
            }
            return misc;
        }
        static private ObservableCollection<TECController> getControllerTemplates(Dictionary<Guid, TECControllerType> controllerTypes)
        {
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new ControllerTable()),
                ControllerTable.TableName,
                ControllerTable.ID.Name,
                TemplatesControllerTable.ControllerID.Name,
                TemplatesControllerTable.TableName);

            DataTable controllerDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in controllerDT.Rows)
            {
                controllers.Add(getControllerFromRow(row, false, controllerTypes));
            }
            return controllers;
        }
        static private ObservableCollection<TECPanel> getPanelTemplates(Dictionary<Guid, TECPanelType> panelTypes)
        {
            ObservableCollection<TECPanel> panels = new ObservableCollection<TECPanel>();

            string command = String.Format("select {0} from {1} where {2} in (select {3} from {4})",
                DatabaseHelper.AllFieldsInTableString(new PanelTable()),
                PanelTable.TableName,
                PanelTable.ID.Name,
                TemplatesPanelTable.PanelID.Name,
                TemplatesPanelTable.TableName);

            DataTable panelsDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in panelsDT.Rows)
            {
                panels.Add(getPanelFromRow(row, false, panelTypes));
            }
            return panels;
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
        #endregion
        #endregion //Loading from DB Methods

        #region Row to Object Methods
        #region Base Scope
        private static TECTypical getTypicalFromRow(DataRow row, Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECTypical system = new TECTypical(guid);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            system.SetControllers(getControllersInSystem(guid, true, controllerTypes));
            system.Equipment = getEquipmentInSystem(guid, true);
            system.Panels = getPanelsInSystem(guid, true, panelTypes);
            system.Instances = getChildrenSystems(guid, controllerTypes, panelTypes);
            system.MiscCosts = getMiscInSystem(guid, true);
            system.ScopeBranches = getScopeBranchesInSystem(guid, true);

            return system;
        }
        private static TECSystem getSystemFromRow(DataRow row, Dictionary<Guid, TECControllerType> controllerTypes, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECSystem system = new TECSystem(guid, false);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            system.SetControllers(getControllersInSystem(guid, false, controllerTypes));
            system.Equipment = getEquipmentInSystem(guid, false);
            system.Panels = getPanelsInSystem(guid, false, panelTypes);
            system.MiscCosts = getMiscInSystem(guid, false);
            system.ScopeBranches = getScopeBranchesInSystem(guid, false);

            return system;
        }

        private static TECEquipment getEquipmentFromRow(DataRow row, bool isTypical)
        {
            Guid equipmentID = new Guid(row[EquipmentTable.ID.Name].ToString());
            TECEquipment equipmentToAdd = new TECEquipment(equipmentID, isTypical);
            assignValuePropertiesFromTable(equipmentToAdd, new EquipmentTable(), row);
            equipmentToAdd.SubScope = getChildObjects(new EquipmentSubScopeTable(), new SubScopeTable(), 
                equipmentID, data => { return getPlaceholderSubScopeFromRow(data, isTypical); });
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
        #endregion
        #region Catalogs
        private static TECConnectionType getConnectionTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ConnectionTypeTable.ID.Name].ToString());
            var outConnectionType = new TECConnectionType(guid);
            assignValuePropertiesFromTable(outConnectionType, new ConnectionTypeTable(), row);
            outConnectionType.RatedCosts = getRatedCostsInComponent(outConnectionType.Guid);
            return outConnectionType;
        }
        private static TECElectricalMaterial getConduitTypeFromRow(DataRow row)
        {
            Guid conduitGuid = new Guid(row[ConduitTypeTable.ID.Name].ToString());
            var conduitType = new TECElectricalMaterial(conduitGuid);
            assignValuePropertiesFromTable(conduitType, new ConduitTypeTable(), row);
            conduitType.RatedCosts = getRatedCostsInComponent(conduitType.Guid);
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
            module.IO = getIOInModule(module.Guid);
            assignValuePropertiesFromTable(module, new IOModuleTable(), row);
            return module;
        }
        private static TECControllerType getControllerTypeFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid guid = new Guid(row[ControllerTypeTable.ID.Name].ToString());
            TECManufacturer manufacturer = manufacturers[guid];
            TECControllerType controllerType = new TECControllerType(guid, manufacturer);
            controllerType.IO = getChildObjects(new ControllerTypeIOTable(), new IOTable(), guid, getIOFromRow);
            controllerType.IOModules = getIOModuleInControllerType(guid);
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
        #endregion
        #region Scope Qualifiers
        private static TECScopeBranch getScopeBranchFromRow(DataRow row, bool isTypical)
        {
            Guid scopeBranchID = new Guid(row[ScopeBranchTable.ID.Name].ToString());
            TECScopeBranch branch = new TECScopeBranch(scopeBranchID, isTypical);
            assignValuePropertiesFromTable(branch, new ScopeBranchTable(), row);
            branch.Branches = getChildObjects(new ScopeBranchHierarchyTable(), new ScopeBranchTable(), scopeBranchID, data => { return getScopeBranchFromRow(data, isTypical); });
                
                
                //getChildBranchesInBranch(scopeBranchID, isTypical);
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
        #endregion
        
        #region Control Scope
        private static TECPanel getPanelFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECPanelType> panelTypes)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = panelTypes[guid];
            TECPanel panel = new TECPanel(guid, type, isTypical);

            assignValuePropertiesFromTable(panel, new PanelTable(), row);
            panel.Controllers = getControllersInPanel(guid, isTypical);

            return panel;
        }
        private static TECController getControllerFromRow(DataRow row, bool isTypical, Dictionary<Guid, TECControllerType> controllerTypes)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECController controller = new TECController(guid, controllerTypes[guid], isTypical);

            assignValuePropertiesFromTable(controller, new ControllerTable(), row);
            controller.ChildrenConnections = getConnectionsInController(controller, isTypical);
            controller.IOModules = getIOModuleInController(guid);
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
            connection.SubScope = getSubScopeInSubScopeConnection(connection.Guid, isTypical);
            return connection;
        }
        private static TECNetworkConnection getNetworkConnectionFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[NetworkConnectionTable.ID.Name].ToString());
            TECNetworkConnection connection = new TECNetworkConnection(guid, isTypical);
            assignValuePropertiesFromTable(connection, new NetworkConnectionTable(), row);
            connection.IOType = UtilitiesMethods.StringToEnum<IOType>(row[NetworkConnectionTable.IOType.Name].ToString());
            connection.Children = getChildrenInNetworkConnection(connection.Guid, isTypical);
            connection.ConnectionTypes = getConnectionTypesInNetworkConnection(connection.Guid);
            return connection;
        }
        #endregion

        #region Misc
        private static TECMisc getMiscFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[MiscTable.ID.Name].ToString());
            CostType type = UtilitiesMethods.StringToEnum<CostType>(row[MiscTable.Type.Name].ToString());
            TECMisc cost = new TECMisc(guid, type, isTypical);
            assignValuePropertiesFromTable(cost, new MiscTable(), row);
            return cost;
        }
        private static TECParameters getBidParametersFromRow(DataRow row)
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
        #endregion

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
        #region Placeholder
        private static TECSubScope getSubScopeConnectionChildPlaceholderFromRow(DataRow row, bool isTypical)
        {
            Guid subScopeID = new Guid(row[SubScopeConnectionChildrenTable.ChildID.Name].ToString());
            TECSubScope subScopeToAdd = new TECSubScope(subScopeID, isTypical);
            return subScopeToAdd;
        }
        private static TECController getControllerPlaceholderFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECController controller = new TECController(guid, new TECControllerType(new TECManufacturer()), isTypical);

            controller.Name = row[ControllerTable.Name.Name].ToString();
            controller.Description = row[ControllerTable.Description.Name].ToString();
            return controller;
        }

        private static TECController getPlaceholderPanelControllerFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[PanelControllerTable.ControllerID.Name].ToString());
            TECController controller = new TECController(guid, new TECControllerType(new TECManufacturer()), isTypical);
            return controller;
        }
        private static TECAssociatedCost getPlaceholderRatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ElectricalMaterialRatedCostTable.CostID.Name].ToString());
            TECAssociatedCost associatedCost = new TECAssociatedCost(guid, CostType.TEC);
            return associatedCost;
        }
        private static TECConnectionType getPlaceholderConnectionTypeFromRow(DataRow row, string keyField)
        {
            Guid guid = new Guid(row[keyField].ToString());
            TECConnectionType connectionType = new TECConnectionType(guid);
            return connectionType;
        }

        private static TECIOModule getPlaceholderIOModuleFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerIOModuleTable.ModuleID.Name].ToString());
            TECIOModule module = new TECIOModule(guid, new TECManufacturer());
            module.Description = "placeholder";
            return module;
        }
        private static TECSubScope getPlaceholderSubScopeFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[SubScopeTable.ID.Name].ToString());
            return new TECSubScope(guid, isTypical);
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
        #endregion
        #endregion
        
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
    }
}
