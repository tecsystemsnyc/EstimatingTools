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
        static private bool isBid = false;

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
                isBid = true;
                (workingScopeManager, needsUpdate) = loadBid();
            }
            else if (tableNames.Contains("TemplatesInfo"))
            {
                isBid = false;
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
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getConnectionConduitTypes(bid.Catalogs);
            
            bid.Parameters = getBidParameters(bid);
            bid.ExtraLabor = getExtraLabor(bid);
            bid.Schedule = getSchedule(bid);
            bid.ScopeTree = getBidScopeBranches();
            bid.Systems = getAllSystemsInBid(bid.Guid);
            bid.Locations = getAllLocations();
            bid.Notes = getNotes();
            bid.Exclusions = getExclusions();
            bid.SetControllers(getOrphanControllers());
            bid.MiscCosts = getMiscInBid(bid.Guid);
            bid.Panels = getOrphanPanels();
            var placeholderDict = getCharacteristicInstancesList();
            populateTypicalProperties(bid.Systems, tags, costs, points, endDevices, connectionConduitTypes);
            bid.Controllers.ForEach(controller => populateScopeProperties(controller, tags, costs));
            bid.Controllers.ForEach(controller => populateConduitTypesInControllerConnections(controller, connectionConduitTypes));
            bid.Panels.ForEach(panel => populateScopeProperties(panel, tags, costs));
            bid.MiscCosts.ForEach(misc => populateScopeProperties(misc, tags, costs));
            
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
            Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes = getConnectionConduitTypes(templates.Catalogs); templates.SystemTemplates = getSystemTemplates();

            templates.EquipmentTemplates = getEquipmentTemplates();
            templates.SubScopeTemplates = getSubScopeTemplates();
            templates.ControllerTemplates = getControllerTemplates();
            templates.MiscCostTemplates = getMiscTemplates();
            templates.PanelTemplates = getPanelTemplates();
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
            foreach(TECSystem system in templates.SystemTemplates)
            {
                populateScopeProperties(system, tags, costs);
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
                populateScopeProperties(equpiment, tags, costs);
                foreach (TECSubScope subScope in equpiment.SubScope)
                {
                    populateSubScopeChildren(subScope, points, endDevices);
                }
            }
            foreach(TECSubScope subScope in templates.SubScopeTemplates)
            {
                populateScopeProperties(subScope, tags, costs);
                populateSubScopeChildren(subScope, points, endDevices);
            }
            foreach(TECPanel panel in templates.PanelTemplates)
            {
                populateScopeProperties(panel, tags, costs);
            }
            foreach(TECController controller in templates.ControllerTemplates)
            {
                populateScopeProperties(controller, tags, costs);
            }
            foreach(TECMisc misc in templates.MiscCostTemplates)
            {
                populateScopeProperties(misc, tags, costs);
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

        private static Dictionary<Guid, TECElectricalMaterial> getConnectionConduitTypes(TECCatalogs catalogs)
        {
            Dictionary<Guid, TECElectricalMaterial> dictionary = new Dictionary<Guid, TECElectricalMaterial>();
            TableBase table = new ConnectionConduitTypeTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            {
                Guid connectionID = new Guid(row[ConnectionConduitTypeTable.ConnectionID.Name].ToString());
                Guid typeID = new Guid(row[ConnectionConduitTypeTable.TypeID.Name].ToString());
                TECElectricalMaterial conduitType = catalogs.ConduitTypes.First(item => item.Guid == typeID);
                dictionary[connectionID] = conduitType;
            }
            return dictionary;
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
        private static Dictionary<Guid, TECManufacturer> getHardwareManufacturers(IEnumerable<TECManufacturer> manufacturers)
        {
            Dictionary<Guid, TECManufacturer> dictionary = new Dictionary<Guid, TECManufacturer>();
            TableBase table = new HardwareManufacturerTable();
            string command = string.Format("select {0} from {1}",
                DatabaseHelper.AllFieldsInTableString(table),
                table.NameString);
            DataTable data = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in data.Rows)
            {
                Guid hardwareID = new Guid(row[HardwareManufacturerTable.HardwareID.Name].ToString());
                Guid manID = new Guid(row[HardwareManufacturerTable.ManufacturerID.Name].ToString());
                TECManufacturer manufacturer = manufacturers.First(item => item.Guid == manID);
                dictionary[hardwareID] = manufacturer;
            }
            return dictionary;
        }

        private static void populateTypicalProperties(IEnumerable<TECTypical> typicals, Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs,
            Dictionary<Guid, List<TECPoint>> points, Dictionary<Guid, List<IEndDevice>> devices, Dictionary<Guid, TECElectricalMaterial> connectionConduitTypes)
        {
            foreach(TECTypical typical in typicals)
            {
                populateScopeProperties(typical, tags, costs);
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
                foreach(TECPoint point in points[subScope.Guid])
                {
                    TECPoint newPoint = new TECPoint(point.Guid, subScope.IsTypical);
                    newPoint.Label = point.Label;
                    newPoint.Quantity = point.Quantity;
                    newPoint.Type = point.Type;
                    subScope.Points.Add(newPoint);
                }
            }
            if (devices.ContainsKey(subScope.Guid))
            {
                foreach(IEndDevice device in devices[subScope.Guid])
                {
                    subScope.Devices.Add(device);
                }
            }
        }
        private static void populateScopeProperties(TECScope scope, Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs)
        {
            if (tags.ContainsKey(scope.Guid))
            {
                foreach (Guid tagID in tags[scope.Guid])
                {
                    scope.Tags.Add(new TECTag(tagID));
                }
            }
            if (costs.ContainsKey(scope.Guid))
            {
                foreach(Guid costID in costs[scope.Guid])
                {
                    scope.AssociatedCosts.Add(new TECAssociatedCost(costID, CostType.TEC));
                }
            }
            if(scope is IRelatable rel)
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
        static private TECCatalogs getCatalogs(Dictionary<Guid, List<Guid>> tags, Dictionary<Guid, List<Guid>> costs)
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Manufacturers = getAllManufacturers();
            Dictionary<Guid, TECManufacturer> hardwareManufacturer = getHardwareManufacturers(catalogs.Manufacturers);
            catalogs.Devices = getAllDevices(hardwareManufacturer);
            catalogs.Valves = getValves(hardwareManufacturer);
            catalogs.ConnectionTypes = getConnectionTypes();
            catalogs.ConduitTypes = getConduitTypes();
            catalogs.AssociatedCosts = getAssociatedCosts();
            catalogs.PanelTypes = getPanelTypes(hardwareManufacturer);
            catalogs.IOModules = getIOModules(hardwareManufacturer);
            catalogs.ControllerTypes = getControllerTypes(hardwareManufacturer);
            catalogs.Tags = getAllTags();

            catalogs.Devices.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.Valves.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.ConnectionTypes.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.ConduitTypes.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.AssociatedCosts.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.PanelTypes.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.IOModules.ForEach(item => populateScopeProperties(item, tags, costs));
            catalogs.ControllerTypes.ForEach(item => populateScopeProperties(item, tags, costs));

            return catalogs;
        }
        
        static private ObservableCollection<TECDevice> getAllDevices(Dictionary<Guid, TECManufacturer> manufacturers)
        {
            ObservableCollection<TECDevice> devices = new ObservableCollection<TECDevice>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new DeviceTable()), DeviceTable.TableName);
            DataTable devicesDT = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in devicesDT.Rows)
            { devices.Add(getDeviceFromRow(row, manufacturers)); }
            return devices;
        }
        static private ObservableCollection<TECManufacturer> getAllManufacturers()
        {
            ObservableCollection<TECManufacturer> manufacturers = new ObservableCollection<TECManufacturer>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ManufacturerTable()), ManufacturerTable.TableName);
            DataTable manufacturersDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in manufacturersDT.Rows)
            { manufacturers.Add(getManufacturerFromRow(row)); }
            return manufacturers;
        }
        static private ObservableCollection<TECElectricalMaterial> getConduitTypes()
        {
            ObservableCollection<TECElectricalMaterial> conduitTypes = new ObservableCollection<TECElectricalMaterial>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ConduitTypeTable()), ConduitTypeTable.TableName);
            DataTable conduitTypesDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in conduitTypesDT.Rows)
            { conduitTypes.Add(getConduitTypeFromRow(row)); }
            return conduitTypes;
        }
        static private ObservableCollection<TECPanelType> getPanelTypes(Dictionary<Guid, TECManufacturer> manufacturers)
        {
            ObservableCollection<TECPanelType> panelTypes = new ObservableCollection<TECPanelType>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new PanelTypeTable()), PanelTypeTable.TableName);
            DataTable panelTypesDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in panelTypesDT.Rows)
            {
                panelTypes.Add(getPanelTypeFromRow(row, manufacturers));
            }

            return panelTypes;
        }
        static private ObservableCollection<TECConnectionType> getConnectionTypes()
        {
            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ConnectionTypeTable()), ConnectionTypeTable.TableName);
            DataTable connectionTypesDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in connectionTypesDT.Rows)
            { connectionTypes.Add(getConnectionTypeFromRow(row)); }
            return connectionTypes;
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
        static private ObservableCollection<TECControllerType> getControllerTypes(Dictionary<Guid, TECManufacturer> manufacturers)
        {
            ObservableCollection<TECControllerType> controllerTypes = new ObservableCollection<TECControllerType>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ControllerTypeTable()), ControllerTypeTable.TableName);
            DataTable dt = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in dt.Rows)
            {
                controllerTypes.Add(getControllerTypeFromRow(row, manufacturers));
            }

            return controllerTypes;
        }
        static private ObservableCollection<TECValve> getValves(Dictionary<Guid, TECManufacturer> manufacturers)
        {
            ObservableCollection<TECValve> valves = new ObservableCollection<TECValve>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ValveTable()), ValveTable.TableName);
            DataTable dt = SQLiteDB.GetDataFromCommand(command);

            foreach (DataRow row in dt.Rows)
            { valves.Add(getValveFromRow(row, manufacturers)); }
            return valves;
        }
        #endregion
        #region System Components
        static private ObservableCollection<TECPanel> getPanelsInSystem(Guid guid, bool isTypical)
        {
            ObservableCollection<TECPanel> panels = new ObservableCollection<TECPanel>();
            DataTable dt = getChildObjects(new SystemPanelTable(), new PanelTable(), guid, SystemPanelTable.Index.Name);
            foreach (DataRow row in dt.Rows)
            { panels.Add(getPanelFromRow(row, isTypical)); }

            return panels;
        }
        static private ObservableCollection<TECEquipment> getEquipmentInSystem(Guid systemID, bool isTypical)
        {
            ObservableCollection<TECEquipment> equipment = new ObservableCollection<TECEquipment>();
            DataTable equipmentDT = getChildObjects(new SystemEquipmentTable(), new EquipmentTable(),
                systemID, SystemEquipmentTable.Index.Name);
            foreach (DataRow row in equipmentDT.Rows)
            { equipment.Add(getEquipmentFromRow(row, isTypical)); }
            return equipment;
        }
        static private ObservableCollection<TECSystem> getChildrenSystems(Guid parentID)
        {
            ObservableCollection<TECSystem> children = new ObservableCollection<TECSystem>();
            DataTable childDT = getChildObjects(new SystemHierarchyTable(), new SystemTable(), parentID, SystemHierarchyTable.Index.Name);
            foreach (DataRow row in childDT.Rows)
            {
                children.Add(getSystemFromRow(row));
            }

            return children;
        }
        static private ObservableCollection<TECController> getControllersInSystem(Guid guid, bool isTypical)
        {
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();
            DataTable controllerDT = getChildObjects(new SystemControllerTable(), new ControllerTable(), guid, SystemControllerTable.Index.Name);
            foreach (DataRow row in controllerDT.Rows)
            {
                controllers.Add(getControllerFromRow(row, isTypical));
            }

            return controllers;
        }
        static private ObservableCollection<TECScopeBranch> getScopeBranchesInSystem(Guid guid, bool isTypical)
        {
            ObservableCollection<TECScopeBranch> branches = new ObservableCollection<TECScopeBranch>();
            DataTable branchDT = getChildObjects(new SystemScopeBranchTable(), new ScopeBranchTable(), guid);
            foreach (DataRow row in branchDT.Rows)
            { branches.Add(getScopeBranchFromRow(row, isTypical)); }

            return branches;
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
        static private ObservableCollection<TECMisc> getMiscInSystem(Guid guid, bool isTypical)
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();
            DataTable miscDT = getChildObjects(new SystemMiscTable(), new MiscTable(), guid, SystemMiscTable.Index.Name);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row, isTypical));
            }

            return misc;
        }

        #endregion
        #region Scope Children
        static private ObservableCollection<TECTag> getTagsInScope(Guid scopeID)
        {
            ObservableCollection<TECTag> tags = new ObservableCollection<TECTag>();
            DataTable tagsDT = getChildIDs(new ScopeTagTable(), scopeID);
            foreach (DataRow row in tagsDT.Rows)
            { tags.Add(getPlaceholderTagFromRow(row, ScopeTagTable.TagID.Name)); }
            return tags;
        }
        static private ObservableCollection<TECAssociatedCost> getAssociatedCostsInScope(Guid scopeID)
        {
            DataTable DT = getChildIDs(new ScopeAssociatedCostTable(), scopeID, ScopeAssociatedCostTable.Quantity.Name);
            var associatedCosts = new ObservableCollection<TECAssociatedCost>();
            foreach (DataRow row in DT.Rows)
            {
                TECAssociatedCost costToAdd = getPlaceholderAssociatedCostFromRow(row);
                int quantity = row[ScopeAssociatedCostTable.Quantity.Name].ToString().ToInt();
                for (int x = 0; x < quantity; x++) { associatedCosts.Add(costToAdd); }
            }
            return associatedCosts;
        }
        static private TECLocation getLocationInLocated(Guid ScopeID)
        {
            if (isBid)
            {
                DataTable locationDT = getChildIDs(new LocatedLocationTable(), ScopeID);
                if (locationDT.Rows.Count > 0)
                { return getPlaceholderLocationFromRow(locationDT.Rows[0]); }
                else
                { return null; }
            }
            else
            { return null; }
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
            List<TECScheduleTable> tables = getTablesForSchedule(guid);
            TECSchedule schedule = new TECSchedule(guid, tables);
            return schedule;
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
        static private ObservableCollection<TECScopeBranch> getChildBranchesInBranch(Guid parentID, bool isTypical)
        {
            ObservableCollection<TECScopeBranch> childBranches = new ObservableCollection<TECScopeBranch>();
            DataTable childBranchDT = getChildObjects(new ScopeBranchHierarchyTable(), new ScopeBranchTable(), parentID, ScopeBranchHierarchyTable.Index.Name);
            foreach (DataRow row in childBranchDT.Rows)
            {
                childBranches.Add(getScopeBranchFromRow(row, isTypical));
            }
            return childBranches;
        }
        static private ObservableCollection<TECTypical> getAllSystemsInBid(Guid guid)
        {
            ObservableCollection<TECTypical> systems = new ObservableCollection<TECTypical>();
            DataTable systemsDT = getChildObjects(new BidSystemTable(), new SystemTable(), guid, BidSystemTable.Index.Name);
            foreach (DataRow row in systemsDT.Rows)
            { systems.Add(getTypicalFromRow(row)); }
            return systems;
        }

        static private ObservableCollection<TECEquipment> getOrphanEquipment(Dictionary<Guid, List<TECPoint>> points)
        {
            ObservableCollection<TECEquipment> equipment = new ObservableCollection<TECEquipment>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new EquipmentTable()) + " from " + EquipmentTable.TableName;
            command += " where " + EquipmentTable.ID.Name + " not in ";
            command += "(select " + SystemEquipmentTable.EquipmentID.Name;
            command += " from " + SystemEquipmentTable.TableName + ")";

            DataTable equipmentDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in equipmentDT.Rows)
            { equipment.Add(getEquipmentFromRow(row, false)); }

            return equipment;
        }
        static private ObservableCollection<TECSubScope> getOrphanSubScope(Dictionary<Guid, List<TECPoint>> points)
        {
            ObservableCollection<TECSubScope> subScope = new ObservableCollection<TECSubScope>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SubScopeTable()) + " from " + SubScopeTable.TableName;
            command += " where " + SubScopeTable.ID.Name + " not in ";
            command += "(select " + EquipmentSubScopeTable.SubScopeID.Name + " from " + EquipmentSubScopeTable.TableName + ")";
            DataTable subScopeDT = SQLiteDB.GetDataFromCommand(command);
            foreach (DataRow row in subScopeDT.Rows)
            { subScope.Add(getSubScopeFromRow(row, false)); }
            return subScope;
        }
        static private ObservableCollection<TECLocation> getAllLocations()
        {
            ObservableCollection<TECLocation> locations = new ObservableCollection<TECLocation>();
            DataTable locationsDT = SQLiteDB.GetDataFromTable(LocationTable.TableName);
            foreach (DataRow row in locationsDT.Rows)
            { locations.Add(getLocationFromRow(row)); }
            return locations;
        }
        static private ObservableCollection<TECAssociatedCost> getAssociatedCosts()
        {
            ObservableCollection<TECAssociatedCost> associatedCosts = new ObservableCollection<TECAssociatedCost>();
            DataTable associatedCostsDT = SQLiteDB.GetDataFromTable(AssociatedCostTable.TableName);
            foreach (DataRow row in associatedCostsDT.Rows)
            { associatedCosts.Add(getAssociatedCostFromRow(row)); }
            return associatedCosts;
        }
        static private ObservableCollection<TECSubScope> getSubScopeInEquipment(Guid equipmentID, bool isTypical)
        {
            ObservableCollection<TECSubScope> subScope = new ObservableCollection<TECSubScope>();
            DataTable subScopeDT = getChildObjects(new EquipmentSubScopeTable(), new SubScopeTable(),
                equipmentID, EquipmentSubScopeTable.Index.Name);
            foreach (DataRow row in subScopeDT.Rows)
            { subScope.Add(getSubScopeFromRow(row, isTypical)); }
            return subScope;
        }
        static private ObservableCollection<TECConnectionType> getConnectionTypesInDevice(Guid deviceID)
        {
            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            DataTable connectionTypeTable = getChildIDs(new DeviceConnectionTypeTable(), deviceID, DeviceConnectionTypeTable.Quantity.Name);
            foreach (DataRow row in connectionTypeTable.Rows)
            {
                var connectionTypeToAdd = new TECConnectionType(new Guid(row[DeviceConnectionTypeTable.TypeID.Name].ToString()));
                int quantity = row[DeviceConnectionTypeTable.Quantity.Name].ToString().ToInt(1);
                for (int x = 0; x < quantity; x++)
                { connectionTypes.Add(connectionTypeToAdd); }
            }
            return connectionTypes;
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
        static private ObservableCollection<TECLabeled> getNotes()
        {
            ObservableCollection<TECLabeled> notes = new ObservableCollection<TECLabeled>();
            DataTable notesDT = SQLiteDB.GetDataFromTable(NoteTable.TableName);
            foreach (DataRow row in notesDT.Rows)
            { notes.Add(getNoteFromRow(row)); }
            return notes;
        }
        static private ObservableCollection<TECLabeled> getExclusions()
        {
            ObservableCollection<TECLabeled> exclusions = new ObservableCollection<TECLabeled>();
            DataTable exclusionsDT = SQLiteDB.GetDataFromTable(ExclusionTable.TableName);
            foreach (DataRow row in exclusionsDT.Rows)
            { exclusions.Add(getExclusionFromRow(row)); }
            return exclusions;
        }
        static private ObservableCollection<TECTag> getAllTags()
        {
            ObservableCollection<TECTag> tags = new ObservableCollection<TECTag>();
            DataTable tagsDT = SQLiteDB.GetDataFromTable(TagTable.TableName);
            foreach (DataRow row in tagsDT.Rows)
            { tags.Add(getTagFromRow(row)); }
            return tags;
        }
        static private ObservableCollection<TECController> getOrphanControllers()
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
                controllers.Add(getControllerFromRow(row, false));
            }

            return controllers;
        }
        static private ObservableCollection<TECIO> getIOInControllerType(Guid typeId)
        {
            ObservableCollection<TECIO> outIO = new ObservableCollection<TECIO>();
            DataTable typeDT = getChildObjects(new ControllerTypeIOTable(), new IOTable(), typeId);
            foreach (DataRow row in typeDT.Rows)
            { outIO.Add(getIOFromRow(row)); }
            return outIO;
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
        private static List<TECScheduleTable> getTablesForSchedule(Guid guid)
        {
            List<TECScheduleTable> tables = new List<TECScheduleTable>();
            DataTable dataTable = getChildObjects(new ScheduleScheduleTableTable(), new ScheduleTableTable(),
                guid, ScheduleScheduleTableTable.Index.Name);
            foreach (DataRow row in dataTable.Rows)
            { tables.Add(getScheduleTableFromRow(row)); }
            return tables;
        }
        private static TECScheduleTable getScheduleTableFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScheduleTableTable.ID.Name].ToString());
            String name = row[ScheduleTableTable.Name.Name].ToString();
            List<TECScheduleItem> items = getScheduleItemsInTable(guid);
            TECScheduleTable table = new TECScheduleTable(guid, items);
            table.Name = name;
            return table;
        }
        private static List<TECScheduleItem> getScheduleItemsInTable(Guid guid)
        {
            List<TECScheduleItem> items = new List<TECScheduleItem>();
            DataTable dataTable = getChildObjects(new ScheduleTableScheduleItemTable(), new ScheduleItemTable(),
                guid, ScheduleTableScheduleItemTable.Index.Name);
            foreach (DataRow row in dataTable.Rows)
            { items.Add(getScheduleItemFromRow(row)); }
            return items;
        }

        static private ObservableCollection<TECIOModule> getIOModules(Dictionary<Guid, TECManufacturer> manufacturers)
        {
            ObservableCollection<TECIOModule> ioModules = new ObservableCollection<TECIOModule>();
            DataTable ioModuleDT = SQLiteDB.GetDataFromTable(IOModuleTable.TableName);

            foreach (DataRow row in ioModuleDT.Rows)
            { ioModules.Add(getIOModuleFromRow(row, manufacturers)); }
            return ioModules;
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

            DataTable dt = getChildObjects(new NetworkConnectionChildrenTable(), new ControllerTable(), connectionID, NetworkConnectionChildrenTable.Index.Name);
            foreach (DataRow row in dt.Rows)
            { outScope.Add(getControllerPlaceholderFromRow(row, isTypical)); }

            dt = getChildObjects(new NetworkConnectionChildrenTable(), new SubScopeTable(), connectionID, NetworkConnectionChildrenTable.Index.Name);
            foreach (DataRow row in dt.Rows)
            { outScope.Add(getPlaceholderSubScopeFromRow(row, isTypical)); }

            return outScope;
        }

        static private TECControllerType getTypeInController(Guid controllerID)
        {
            DataTable manTable = getChildIDs(new ControllerControllerTypeTable(), controllerID);
            if (manTable.Rows.Count > 0)
            { return getPlaceholderControllerTypeFromRow(manTable.Rows[0]); }
            else if (justUpdated)
            {
                return tempControllerType;
            }
            else
            { return null; }
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
        static private ObservableCollection<TECPanel> getOrphanPanels()
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
                panels.Add(getPanelFromRow(row, false));
            }

            return panels;
        }
        static private ObservableCollection<TECSystem> getSystems()
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
                systems.Add(getSystemFromRow(row));
            }
            return systems;
        }
        static private TECPanelType getPanelTypeInPanel(Guid guid)
        {
            DataTable manTable = getChildIDs(new PanelPanelTypeTable(), guid);
            if (manTable.Rows.Count > 0)
            { return getPlaceholderPanelTypeFromRow(manTable.Rows[0]); }
            else if (justUpdated)
            {
                return tempPanelType;
            }
            else
            { return null; }
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
        static private ObservableCollection<TECIO> getIOInModule(Guid moduleID)
        {
            DataTable moduleTable = getChildObjects(new IOModuleIOTable(), new IOTable(), moduleID);
            ObservableCollection<TECIO> outIO = new ObservableCollection<TECIO>();
            foreach(DataRow row in moduleTable.Rows)
            { outIO.Add(getIOFromRow(row)); }
            return outIO;
        }

        static private ObservableCollection<TECMisc> getMiscInBid(Guid guid)
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();
            DataTable miscDT = getChildObjects(new BidMiscTable(), new MiscTable(), guid, BidMiscTable.Index.Name);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row, false));
            }

            return misc;
        }
        
        #region Placeholders
        static private TECManufacturer getPlaceholderManufacturer(Guid hardwareGuid)
        {
            DataTable manTable = getChildIDs(new HardwareManufacturerTable(), hardwareGuid);
            if (manTable.Rows.Count > 0)
            { return getPlaceholderManufacturerFromRow(manTable.Rows[0]); }
            else if (justUpdated)
            {
                return tempManufacturer;
            }
            else
            { return null; }
        }
        static private TECDevice getPlaceholderActuator(Guid valveID)
        {
            string command = String.Format("select {0} from {1} where {2} = '{3}'",
                ValveActuatorTable.ActuatorID.Name, ValveActuatorTable.TableName, ValveActuatorTable.ValveID.Name, valveID);

            DataTable dt = SQLiteDB.GetDataFromCommand(command);
            if (dt.Rows.Count > 0)
            { return getPlaceholderActuatorFromRow(dt.Rows[0]); }
            else
            { return null; }
        }

        #endregion
        
        #region Template Scope
        static private ObservableCollection<TECSystem> getSystemTemplates()
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
                systems.Add(getSystemFromRow(row));
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
        static private ObservableCollection<TECController> getControllerTemplates()
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
                controllers.Add(getControllerFromRow(row, false));
            }
            return controllers;
        }
        static private ObservableCollection<TECPanel> getPanelTemplates()
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
                panels.Add(getPanelFromRow(row, false));
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
        private static TECTypical getTypicalFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECTypical system = new TECTypical(guid);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            system.SetControllers(getControllersInSystem(guid, true));
            system.Equipment = getEquipmentInSystem(guid, true);
            system.Panels = getPanelsInSystem(guid, true);
            system.Instances = getChildrenSystems(guid);
            system.MiscCosts = getMiscInSystem(guid, true);
            system.ScopeBranches = getScopeBranchesInSystem(guid, true);
            getLocatedChildren(system);

            return system;
        }
        private static TECSystem getSystemFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SystemTable.ID.Name].ToString());
            TECSystem system = new TECSystem(guid, false);

            assignValuePropertiesFromTable(system, new SystemTable(), row);
            system.SetControllers(getControllersInSystem(guid, false));
            system.Equipment = getEquipmentInSystem(guid, false);
            system.Panels = getPanelsInSystem(guid, false);
            system.MiscCosts = getMiscInSystem(guid, false);
            system.ScopeBranches = getScopeBranchesInSystem(guid, false);
            getLocatedChildren(system);

            return system;
        }

        private static TECEquipment getEquipmentFromRow(DataRow row, bool isTypical)
        {
            Guid equipmentID = new Guid(row[EquipmentTable.ID.Name].ToString());
            TECEquipment equipmentToAdd = new TECEquipment(equipmentID, isTypical);
            assignValuePropertiesFromTable(equipmentToAdd, new EquipmentTable(), row);
            getLocatedChildren(equipmentToAdd);
            equipmentToAdd.SubScope = getSubScopeInEquipment(equipmentID, isTypical);
            return equipmentToAdd;
        }
        private static TECSubScope getSubScopeFromRow(DataRow row, bool isTypical)
        {
            Guid subScopeID = new Guid(row[SubScopeTable.ID.Name].ToString());
            TECSubScope subScopeToAdd = new TECSubScope(subScopeID, isTypical);
            assignValuePropertiesFromTable(subScopeToAdd, new SubScopeTable(), row);
            getLocatedChildren(subScopeToAdd);
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
        private static TECDevice getDeviceFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid deviceID = new Guid(row[DeviceTable.ID.Name].ToString());
            ObservableCollection<TECConnectionType> connectionType = getConnectionTypesInDevice(deviceID);
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
            controllerType.IO = getIOInControllerType(guid);
            controllerType.IOModules = getIOModuleInControllerType(guid);
            assignValuePropertiesFromTable(controllerType, new ControllerTypeTable(), row);
            return controllerType;
        }
        private static TECValve getValveFromRow(DataRow row, Dictionary<Guid, TECManufacturer> manufacturers)
        {
            Guid id = new Guid(row[DeviceTable.ID.Name].ToString());
            TECManufacturer manufacturer = manufacturers[id];
            TECDevice actuator = getPlaceholderActuator(id);
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
            branch.Branches = getChildBranchesInBranch(scopeBranchID, isTypical);
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
        private static TECPanel getPanelFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[PanelTable.ID.Name].ToString());
            TECPanelType type = getPanelTypeInPanel(guid);
            TECPanel panel = new TECPanel(guid, type, isTypical);

            assignValuePropertiesFromTable(panel, new PanelTable(), row);
            panel.Controllers = getControllersInPanel(guid, isTypical);

            return panel;
        }
        private static TECController getControllerFromRow(DataRow row, bool isTypical)
        {
            Guid guid = new Guid(row[ControllerTable.ID.Name].ToString());
            TECController controller = new TECController(guid, getTypeInController(guid), isTypical);

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

        private static void getLocatedChildren(TECLocated located)
        {
            located.Location = getLocationInLocated(located.Guid);
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
        private static TECTag getPlaceholderTagFromRow(DataRow row, string keyString)
        {
            Guid guid = new Guid(row[ScopeTagTable.TagID.Name].ToString());
            TECTag tag = new TECTag(guid);
            return tag;
        }
        private static TECAssociatedCost getPlaceholderAssociatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScopeAssociatedCostTable.AssociatedCostID.Name].ToString());
            TECAssociatedCost associatedCost = new TECAssociatedCost(guid, CostType.TEC);
            return associatedCost;
        }
        private static TECAssociatedCost getPlaceholderRatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ElectricalMaterialRatedCostTable.CostID.Name].ToString());
            TECAssociatedCost associatedCost = new TECAssociatedCost(guid, CostType.TEC);
            return associatedCost;
        }
        private static TECLocation getPlaceholderLocationFromRow(DataRow row)
        {
            Guid guid = new Guid(row[LocatedLocationTable.LocationID.Name].ToString());
            TECLocation location = new TECLocation(guid);
            return location;
        }
        private static TECDevice getPlaceholderSubScopeDeviceFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SubScopeDeviceTable.DeviceID.Name].ToString());
            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            TECManufacturer manufacturer = new TECManufacturer();
            TECDevice device = new TECDevice(guid, connectionTypes, manufacturer);
            device.Description = "placeholder";
            return device;
        }
        private static TECManufacturer getPlaceholderManufacturerFromRow(DataRow row)
        {
            Guid guid = new Guid(row[HardwareManufacturerTable.ManufacturerID.Name].ToString());
            TECManufacturer man = new TECManufacturer(guid);
            return man;
        }
        private static TECControllerType getPlaceholderControllerTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerControllerTypeTable.TypeID.Name].ToString());
            TECControllerType type = new TECControllerType(guid, new TECManufacturer());
            return type;
        }
        private static TECPanelType getPlaceholderPanelTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[PanelPanelTypeTable.PanelTypeID.Name].ToString());
            TECPanelType type = new TECPanelType(guid, new TECManufacturer());
            return type;
        }
        private static TECConnectionType getPlaceholderConnectionTypeFromRow(DataRow row, string keyField)
        {
            Guid guid = new Guid(row[keyField].ToString());
            TECConnectionType connectionType = new TECConnectionType(guid);
            return connectionType;
        }
        private static TECElectricalMaterial getPlaceholderConduitTypeFromRow(DataRow row, string keyField)
        {
            Guid guid = new Guid(row[keyField].ToString());
            TECElectricalMaterial connectionType = new TECElectricalMaterial(guid);
            return connectionType;
        }

        private static TECIOModule getPlaceholderIOModuleFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerIOModuleTable.ModuleID.Name].ToString());
            TECIOModule module = new TECIOModule(guid, new TECManufacturer());
            module.Description = "placeholder";
            return module;
        }
        private static TECDevice getPlaceholderActuatorFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ValveActuatorTable.ActuatorID.Name].ToString());
            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            TECManufacturer manufacturer = new TECManufacturer();
            TECDevice device = new TECDevice(guid, connectionTypes, manufacturer);
            device.Description = "placeholder";
            return device;
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

        static private ObservableCollection<T> getListFromTable<T>(string tableName)
        {
            ObservableCollection<T> list = new ObservableCollection<T>();
            DataTable dt = SQLiteDB.GetDataFromTable(tableName);
            foreach (DataRow row in dt.Rows)
            { list.Add(getDataFromRow<T>(row)); }
            return list;
        }

        private static T getDataFromRow<T>(DataRow row)
        {
            throw new NotImplementedException();
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

        private static DataTable getChildObjects(TableBase relationTable, TableBase childTable, Guid parentID, string orderKey = "")
        {
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
