﻿using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingUtilitiesLibrary;
using System.Windows;
using System.Collections.ObjectModel;
using System.Data;
using DebugLibrary;
using EstimatingLibrary.Utilities;
using System.Globalization;

namespace EstimatingUtilitiesLibrary.DatabaseHelpers
{
    public class DatabaseLoader
    {
        //FMT is used by DateTime to convert back and forth between the DateTime type and string
        private const string DB_FMT = "O";

        static private SQLiteDatabase SQLiteDB;

        public static TECScopeManager Load(string path)
        {
            TECScopeManager workingScopeManager = null;
            var SQLiteDB = new SQLiteDatabase(path);
            SQLiteDB.nonQueryCommand("BEGIN TRANSACTION");

            var tableNames = DatabaseHelper.TableNames(SQLiteDB);
            if (tableNames.Contains("TECBidInfo"))
            {
                workingScopeManager = loadBid();
            }
            else if (tableNames.Contains("TECTemplatesInfo"))
            {
                workingScopeManager = loadTemplates();
            }
            else
            {
                MessageBox.Show("File is not a compatible database.");
                return null;
            }

            SQLiteDB.nonQueryCommand("END TRANSACTION");
            SQLiteDB.Connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return workingScopeManager;
        }

        #region Loading from DB Methods
        static private TECBid loadBid()
        {
            TECBid bid = GetBidInfo(SQLiteDB);
            //updateCatalogs(bid, templates);

            getScopeManagerProperties(bid);

            bid.Parameters = getBidParameters(bid);
            bid.ScopeTree = getBidScopeBranches();
            bid.Systems = getAllSystemsInBid();
            bid.Locations = getAllLocations();
            bid.Catalogs.Tags = getAllTags();
            bid.Notes = getNotes();
            bid.Exclusions = getExclusions();
            bid.Controllers = getOrphanControllers();
            bid.MiscCosts = getMiscInBid();
            bid.Panels = getOrphanPanels();
            var placeholderDict = getCharacteristicInstancesList();

            ModelLinkingHelper.LinkBid(bid, placeholderDict);
            getUserAdjustments(bid);
            //Breaks Visual Scope in a page
            //populatePageVisualConnections(bid.Drawings, bid.Connections);

            return bid;
        }
        static private TECTemplates loadTemplates()
        {
            TECTemplates templates = new TECTemplates();
            templates = GetTemplatesInfo(SQLiteDB);
            getScopeManagerProperties(templates);
            templates.SystemTemplates = getSystems();
            templates.EquipmentTemplates = getOrphanEquipment();
            templates.SubScopeTemplates = getOrphanSubScope();
            templates.ControllerTemplates = getOrphanControllers();
            templates.MiscCostTemplates = getMisc();
            templates.PanelTemplates = getOrphanPanels();
            ModelLinkingHelper.LinkTemplates(templates);
            return templates;
        }

        static private void getScopeManagerProperties(TECScopeManager scopeManager)
        {
            scopeManager.Catalogs = getCatalogs();
            scopeManager.Labor = getLaborConsts(scopeManager);
        }
        static private void getUserAdjustments(TECBid bid)
        {
            DataTable adjDT = SQLiteDB.getDataFromTable(UserAdjustmentsTable.TableName);

            if (adjDT.Rows.Count < 1)
            {
                DebugHandler.LogError("UserAdjustments not found in database.");
                return;
            }

            DataRow adjRow = adjDT.Rows[0];

            bid.Labor.PMExtraHours = adjRow[UserAdjustmentsTable.PMExtraHours.Name].ToString().ToDouble();
            bid.Labor.ENGExtraHours = adjRow[UserAdjustmentsTable.ENGExtraHours.Name].ToString().ToDouble();
            bid.Labor.CommExtraHours = adjRow[UserAdjustmentsTable.CommExtraHours.Name].ToString().ToDouble();
            bid.Labor.SoftExtraHours = adjRow[UserAdjustmentsTable.SoftExtraHours.Name].ToString().ToDouble();
            bid.Labor.GraphExtraHours = adjRow[UserAdjustmentsTable.GraphExtraHours.Name].ToString().ToDouble();
        }

        static private TECLabor getLaborConsts(TECScopeManager scopeManager)
        {
            DataTable laborDT = null;
            DataTable subConstsDT = null;
            if (scopeManager is TECBid)
            {
                string constsCommand = "select * from (" + LaborConstantsTable.TableName + " inner join ";
                constsCommand += BidLaborTable.TableName + " on ";
                constsCommand += "(TECLaborConst.LaborID = TECBidTECLabor.LaborID";
                constsCommand += " and " + BidLaborTable.BidID.Name + " = '";
                constsCommand += scopeManager.Guid;
                constsCommand += "'))";

                laborDT = SQLiteDB.getDataFromCommand(constsCommand);

                string subConstsCommand = "select * from (" + SubcontractorConstantsTable.TableName + " inner join ";
                subConstsCommand += BidLaborTable.TableName + " on ";
                subConstsCommand += "(TECSubcontractorConst.LaborID = TECBidTECLabor.LaborID";
                subConstsCommand += " and " + BidLaborTable.BidID.Name + " = '";
                subConstsCommand += scopeManager.Guid;
                subConstsCommand += "'))";

                subConstsDT = SQLiteDB.getDataFromCommand(subConstsCommand);
            }
            else if (scopeManager is TECTemplates)
            {
                laborDT = SQLiteDB.getDataFromTable(LaborConstantsTable.TableName);
                subConstsDT = SQLiteDB.getDataFromTable(SubcontractorConstantsTable.TableName);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (laborDT.Rows.Count > 1)
            {
                DebugHandler.LogError("Multiple rows found in labor constants table. Using first found.");
            }
            else if (laborDT.Rows.Count < 1)
            {
                DebugHandler.LogError("Labor constants not found in database, using default values. Reload labor constants from loaded templates in the labor tab.");
                return new TECLabor();
            }

            DataRow laborRow = laborDT.Rows[0];
            Guid laborID = new Guid(laborRow[LaborConstantsTable.LaborID.Name].ToString());
            TECLabor labor = new TECLabor(laborID);

            labor.PMCoef = laborRow[LaborConstantsTable.PMCoef.Name].ToString().ToDouble(0);
            labor.PMRate = laborRow[LaborConstantsTable.PMRate.Name].ToString().ToDouble(0);

            labor.ENGCoef = laborRow[LaborConstantsTable.ENGCoef.Name].ToString().ToDouble(0);
            labor.ENGRate = laborRow[LaborConstantsTable.ENGRate.Name].ToString().ToDouble(0);

            labor.CommCoef = laborRow[LaborConstantsTable.CommCoef.Name].ToString().ToDouble(0);
            labor.CommRate = laborRow[LaborConstantsTable.CommRate.Name].ToString().ToDouble(0);

            labor.SoftCoef = laborRow[LaborConstantsTable.SoftCoef.Name].ToString().ToDouble(0);
            labor.SoftRate = laborRow[LaborConstantsTable.SoftRate.Name].ToString().ToDouble(0);

            labor.GraphCoef = laborRow[LaborConstantsTable.GraphCoef.Name].ToString().ToDouble(0);
            labor.GraphRate = laborRow[LaborConstantsTable.GraphRate.Name].ToString().ToDouble(0);



            if (subConstsDT.Rows.Count > 1)
            {
                DebugHandler.LogError("Multiple rows found in subcontractor constants table. Using first found.");
            }
            else if (subConstsDT.Rows.Count < 1)
            {
                DebugHandler.LogError("Subcontractor constants not found in database, using default values. Reload labor constants from loaded templates in the labor tab.");
                return labor;
            }

            DataRow subContractRow = subConstsDT.Rows[0];

            labor.ElectricalRate = subContractRow[SubcontractorConstantsTable.ElectricalRate.Name].ToString().ToDouble(0);
            labor.ElectricalNonUnionRate = subContractRow[SubcontractorConstantsTable.ElectricalNonUnionRate.Name].ToString().ToDouble(0);
            labor.ElectricalSuperRate = subContractRow[SubcontractorConstantsTable.ElectricalSuperRate.Name].ToString().ToDouble(0);
            labor.ElectricalSuperNonUnionRate = subContractRow[SubcontractorConstantsTable.ElectricalSuperNonUnionRate.Name].ToString().ToDouble(0);
            labor.ElectricalSuperRatio = subContractRow[SubcontractorConstantsTable.ElectricalSuperRatio.Name].ToString().ToDouble(0);

            labor.ElectricalIsOnOvertime = subContractRow[SubcontractorConstantsTable.ElectricalIsOnOvertime.Name].ToString().ToInt(0).ToBool();
            labor.ElectricalIsUnion = subContractRow[SubcontractorConstantsTable.ElectricalIsUnion.Name].ToString().ToInt(0).ToBool();

            return labor;
        }

        #region Catalogs
        static private TECCatalogs getCatalogs()
        {
            TECCatalogs catalogs = new TECCatalogs();
            catalogs.Devices = getAllDevices();
            catalogs.Manufacturers = getAllManufacturers();
            catalogs.ConnectionTypes = getConnectionTypes();
            catalogs.ConduitTypes = getConduitTypes();
            catalogs.AssociatedCosts = getAssociatedCosts();
            catalogs.PanelTypes = getPanelTypes();
            catalogs.IOModules = getIOModules();
            catalogs.Tags = getAllTags();
            return catalogs;
        }
        static private ObservableCollection<TECDevice> getAllDevices()
        {
            ObservableCollection<TECDevice> devices = new ObservableCollection<TECDevice>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new DeviceTable()), DeviceTable.TableName);
            DataTable devicesDT = SQLiteDB.getDataFromCommand(command);

            foreach (DataRow row in devicesDT.Rows)
            { devices.Add(getDeviceFromRow(row)); }
            return devices;
        }
        static private ObservableCollection<TECManufacturer> getAllManufacturers()
        {
            ObservableCollection<TECManufacturer> manufacturers = new ObservableCollection<TECManufacturer>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ManufacturerTable()), ManufacturerTable.TableName);
            DataTable manufacturersDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in manufacturersDT.Rows)
            { manufacturers.Add(getManufacturerFromRow(row)); }
            return manufacturers;
        }
        static private ObservableCollection<TECElectricalMaterial> getConduitTypes()
        {
            ObservableCollection<TECElectricalMaterial> conduitTypes = new ObservableCollection<TECElectricalMaterial>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ConduitTypeTable()), ConduitTypeTable.TableName);
            DataTable conduitTypesDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in conduitTypesDT.Rows)
            { conduitTypes.Add(getConduitTypeFromRow(row)); }
            return conduitTypes;
        }
        static private ObservableCollection<TECPanelType> getPanelTypes()
        {
            ObservableCollection<TECPanelType> panelTypes = new ObservableCollection<TECPanelType>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new PanelTypeTable()), PanelTypeTable.TableName);
            DataTable panelTypesDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in panelTypesDT.Rows)
            {
                panelTypes.Add(getPanelTypeFromRow(row));
            }

            return panelTypes;
        }
        static private ObservableCollection<TECElectricalMaterial> getConnectionTypes()
        {
            ObservableCollection<TECElectricalMaterial> connectionTypes = new ObservableCollection<TECElectricalMaterial>();
            string command = string.Format("select {0} from {1}", DatabaseHelper.AllFieldsInTableString(new ConnectionTypeTable()), ConnectionTypeTable.TableName);
            DataTable connectionTypesDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in connectionTypesDT.Rows)
            { connectionTypes.Add(getConnectionTypeFromRow(row)); }
            return connectionTypes;
        }
        static private ObservableCollection<TECCost> getRatedCostsInComponent(Guid componentID)
        {

            string command = "select " + ElectricalMaterialRatedCostTable.CostID.Name + ", " + ElectricalMaterialRatedCostTable.Quantity.Name + " from " + ElectricalMaterialRatedCostTable.TableName + " where ";
            command += ElectricalMaterialRatedCostTable.ComponentID.Name + " = '" + componentID;
            command += "'";
            DataTable DT = SQLiteDB.getDataFromCommand(command);
            var costs = new ObservableCollection<TECCost>();
            foreach (DataRow row in DT.Rows)
            {
                TECCost costToAdd = getPlaceholderRatedCostFromRow(row);
                int quantity = row[ElectricalMaterialRatedCostTable.Quantity.Name].ToString().ToInt();
                for (int x = 0; x < quantity; x++) { costs.Add(costToAdd); }
            }
            return costs;
        }
        #endregion
        #region System Components
        static private ObservableCollection<TECPanel> getPanelsInSystem(Guid guid)
        {
            ObservableCollection<TECPanel> panels = new ObservableCollection<TECPanel>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new PanelTable()) + " from " + PanelTable.TableName + " where " + PanelTable.PanelID.Name + " in ";
            command += "(select " + SystemPanelTable.PanelID.Name + " from " + SystemPanelTable.TableName + " where ";
            command += SystemPanelTable.SystemID.Name + " = '" + guid;
            command += "')";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable dt = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in dt.Rows)
            { panels.Add(getPanelFromRow(row)); }

            return panels;
        }
        static private ObservableCollection<TECEquipment> getEquipmentInSystem(Guid systemID)
        {
            ObservableCollection<TECEquipment> equipment = new ObservableCollection<TECEquipment>();

            string command = "select " +
                DatabaseHelper.AllFieldsInTableString(new EquipmentTable());
            command += " from (" + EquipmentTable.TableName + " inner join ";
            command += SystemEquipmentTable.TableName + " on ";
            command += "(TECEquipment.EquipmentID = TECSystemTECEquipment.EquipmentID";
            command += " and " + SystemEquipmentTable.SystemID.Name + " = '";
            command += systemID;
            command += "')) order by " + SystemEquipmentTable.ScopeIndex.Name;
            DatabaseHelper.Explain(command, SQLiteDB);
            //string command = string.Format("select * from {0} where {1} in (select {2} from {3} indexed by {4} where {5} = '{6}')",
            //    EquipmentTable.TableName, EquipmentTable.EquipmentID.Name, SystemEquipmentTable.EquipmentID.Name, SystemEquipmentTable.TableName,
            //    systemEquipmentIndex, SystemEquipmentTable.SystemID.Name, systemID);

            DataTable equipmentDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in equipmentDT.Rows)
            { equipment.Add(getEquipmentFromRow(row)); }
            return equipment;
        }
        static private ObservableCollection<TECSystem> getChildrenSystems(Guid parentID)
        {
            ObservableCollection<TECSystem> children = new ObservableCollection<TECSystem>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SystemTable()) + " from " + SystemTable.TableName;
            command += " where " + SystemTable.SystemID.Name + " in ";
            command += "(select " + SystemHierarchyTable.ChildID.Name + " from " + SystemHierarchyTable.TableName;
            command += " where " + SystemHierarchyTable.ParentID.Name + " = '";
            command += parentID;
            command += "')";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable childDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in childDT.Rows)
            {
                children.Add(getSystemFromRow(row));
            }

            return children;
        }
        static private ObservableCollection<TECController> getControllersInSystem(Guid guid)
        {
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ControllerTable()) + " from " + ControllerTable.TableName + " where " + ControllerTable.ControllerID.Name + " in ";
            command += "(select " + SystemControllerTable.ControllerID.Name + " from " + SystemControllerTable.TableName + " where ";
            command += SystemControllerTable.SystemID.Name + " = '" + guid;
            command += "')";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable controllerDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in controllerDT.Rows)
            {
                var controller = getControllerFromRow(row);
                controller.IsGlobal = false;
                controllers.Add(controller);
            }

            return controllers;
        }
        static private ObservableCollection<TECScopeBranch> getScopeBranchesInSystem(Guid guid)
        {
            ObservableCollection<TECScopeBranch> branches = new ObservableCollection<TECScopeBranch>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ScopeBranchTable()) + " from " + ScopeBranchTable.TableName + " where " + ScopeBranchTable.ScopeBranchID.Name + " in ";
            command += "(select " + SystemScopeBranchTable.BranchID.Name + " from " + SystemScopeBranchTable.TableName + " where ";
            command += SystemScopeBranchTable.SystemID.Name + " = '" + guid;
            command += "')";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable branchDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in branchDT.Rows)
            { branches.Add(getScopeBranchFromRow(row)); }

            return branches;
        }
        static private Dictionary<Guid, List<Guid>> getCharacteristicInstancesList()
        {
            Dictionary<Guid, List<Guid>> outDict = new Dictionary<Guid, List<Guid>>();
            DataTable dictDT = SQLiteDB.getDataFromTable(CharacteristicScopeInstanceScopeTable.TableName);
            foreach (DataRow row in dictDT.Rows)
            {
                addRowToPlaceholderDict(row, outDict);
            }
            return outDict;
        }
        static private ObservableCollection<TECMisc> getMiscInSystem(Guid guid)
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new MiscTable()) + " from " + MiscTable.TableName + " where " + MiscTable.MiscID.Name + " in ";
            command += "(select " + SystemMiscTable.MiscID.Name + " from " + SystemMiscTable.TableName + " where ";
            command += SystemMiscTable.SystemID.Name + " = '" + guid;
            command += "')";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable miscDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row));
            }

            return misc;
        }

        #endregion
        #region Scope Children
        static private ObservableCollection<TECLabeled> getTagsInScope(Guid scopeID)
        {
            ObservableCollection<TECLabeled> tags = new ObservableCollection<TECLabeled>();
            //string command = "select * from "+TagTable.TableName+" where "+TagTable.TagID.Name+" in ";
            //command += "(select "+ScopeTagTable.TagID.Name+" from "+ScopeTagTable.TableName+" where ";
            //command += ScopeTagTable.ScopeID.Name + " = '"+scopeID;
            //command += "')";
            string command = "select " + ScopeTagTable.TagID.Name + " from " + ScopeTagTable.TableName + " where ";
            command += ScopeTagTable.ScopeID.Name + " = '" + scopeID;
            command += "'";
            DataTable tagsDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in tagsDT.Rows)
            { tags.Add(getPlaceholderTagFromRow(row)); }
            return tags;
        }
        static private ObservableCollection<TECCost> getAssociatedCostsInScope(Guid scopeID)
        {
            //string command = "select * from " + AssociatedCostTable.TableName + " where " + AssociatedCostTable.AssociatedCostID.Name + " in ";
            //command += "(select " + AssociatedCostTable.AssociatedCostID.Name + " from " + ScopeAssociatedCostTable.TableName + " where ";
            //command += ScopeAssociatedCostTable.ScopeID.Name + " = '" + scopeID;
            //command += "')";
            string command = "select " + ScopeAssociatedCostTable.AssociatedCostID.Name + ", " + ScopeAssociatedCostTable.Quantity.Name + " from " + ScopeAssociatedCostTable.TableName + " where ";
            command += ScopeAssociatedCostTable.ScopeID.Name + " = '" + scopeID;
            command += "'";
            DataTable DT = SQLiteDB.getDataFromCommand(command);
            var associatedCosts = new ObservableCollection<TECCost>();
            foreach (DataRow row in DT.Rows)
            {
                TECCost costToAdd = getPlaceholderAssociatedCostFromRow(row);
                int quantity = row[ScopeAssociatedCostTable.Quantity.Name].ToString().ToInt();
                for (int x = 0; x < quantity; x++) { associatedCosts.Add(costToAdd); }
            }
            return associatedCosts;
        }
        static private TECLabeled getLocationInScope(Guid ScopeID)
        {
            var tables = DatabaseHelper.TableNames(SQLiteDB);
            if (tables.Contains(LocationTable.TableName))
            {
                //string command = "select * from " + LocationTable.TableName + " where " + LocationTable.LocationID.Name + " in ";
                //command += "(select " + LocationScopeTable.LocationID.Name + " from " + LocationScopeTable.TableName + " where ";
                //command += LocationScopeTable.ScopeID.Name + " = '" + ScopeID;
                //command += "')";
                string command = "select " + LocationScopeTable.LocationID.Name + " from " + LocationScopeTable.TableName + " where ";
                command += LocationScopeTable.ScopeID.Name + " = '" + ScopeID;
                command += "'";
                DataTable locationDT = SQLiteDB.getDataFromCommand(command);
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
            DataTable bidInfoDT = db.getDataFromTable(BidInfoTable.TableName);
            if (bidInfoDT.Rows.Count < 1)
            {
                DebugHandler.LogError("Bid info not found in database. Bid info and labor will be missing.");
                return new TECBid();
            }

            DataRow bidInfoRow = bidInfoDT.Rows[0];

            TECBid outBid = new TECBid(new Guid(bidInfoRow[BidInfoTable.BidID.Name].ToString()));
            outBid.Name = bidInfoRow[BidInfoTable.BidName.Name].ToString();
            outBid.BidNumber = bidInfoRow[BidInfoTable.BidNumber.Name].ToString();

            string dueDateString = bidInfoRow[BidInfoTable.DueDate.Name].ToString();
            outBid.DueDate = DateTime.ParseExact(dueDateString, DB_FMT, CultureInfo.InvariantCulture);

            outBid.Salesperson = bidInfoRow[BidInfoTable.Salesperson.Name].ToString();
            outBid.Estimator = bidInfoRow[BidInfoTable.Estimator.Name].ToString();

            return outBid;
        }
        public static TECTemplates GetTemplatesInfo(SQLiteDatabase db)
        {
            DataTable templateInfoDT = db.getDataFromTable(TemplatesInfoTable.TableName);

            if (templateInfoDT.Rows.Count < 1)
            {
                DebugHandler.LogError("Template info not found in database.");
                return new TECTemplates();
            }
            DataRow templateInfoRow = templateInfoDT.Rows[0];

            Guid infoGuid = new Guid(templateInfoRow[TemplatesInfoTable.TemplateID.Name].ToString());

            return new TECTemplates(infoGuid);
        }
        static private ObservableCollection<TECScopeBranch> getBidScopeBranches()
        {
            ObservableCollection<TECScopeBranch> mainBranches = new ObservableCollection<TECScopeBranch>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ScopeBranchTable()) + " from " + ScopeBranchTable.TableName;
            command += " where " + ScopeBranchTable.ScopeBranchID.Name;
            command += " in (select " + ScopeBranchTable.ScopeBranchID.Name;
            command += " from " + BidScopeBranchTable.TableName + " where " + BidScopeBranchTable.ScopeBranchID.Name + " not in ";
            command += "(select " + ScopeBranchHierarchyTable.ChildID.Name + " from " + ScopeBranchHierarchyTable.TableName + "))";

            DataTable mainBranchDT = SQLiteDB.getDataFromCommand(command);

            foreach (DataRow row in mainBranchDT.Rows)
            {
                mainBranches.Add(getScopeBranchFromRow(row));
            }

            return mainBranches;
        }
        static private ObservableCollection<TECScopeBranch> getChildBranchesInBranch(Guid parentID)
        {
            ObservableCollection<TECScopeBranch> childBranches = new ObservableCollection<TECScopeBranch>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ScopeBranchTable()) + " from " + ScopeBranchTable.TableName;
            command += " where " + ScopeBranchTable.ScopeBranchID.Name + " in ";
            command += "(select " + ScopeBranchHierarchyTable.ChildID.Name + " from " + ScopeBranchHierarchyTable.TableName;
            command += " where " + ScopeBranchHierarchyTable.ParentID.Name + " = '";
            command += parentID;
            command += "')";

            DataTable childBranchDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in childBranchDT.Rows)
            {
                childBranches.Add(getScopeBranchFromRow(row));
            }

            return childBranches;
        }
        static private ObservableCollection<TECSystem> getAllSystemsInBid()
        {
            ObservableCollection<TECSystem> systems = new ObservableCollection<TECSystem>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SystemTable()) + " from ("
                + SystemTable.TableName
                + " inner join "
                + BidSystemTable.TableName
                + " on ("
                + SystemTable.TableName + "." + SystemTable.SystemID.Name
                + " = "
                + BidSystemTable.TableName + "." + BidSystemTable.SystemID.Name
                + ")) order by "
                + BidSystemTable.Index.Name;

            DataTable systemsDT = SQLiteDB.getDataFromCommand(command);
            if (systemsDT.Rows.Count < 1)
            {
                command = "select " + DatabaseHelper.AllFieldsInTableString(new SystemTable()) + " from " + SystemTable.TableName;
                systemsDT = SQLiteDB.getDataFromCommand(command);
            }
            foreach (DataRow row in systemsDT.Rows)
            { systems.Add(getSystemFromRow(row)); }
            return systems;
        }

        static private ObservableCollection<TECEquipment> getOrphanEquipment()
        {
            ObservableCollection<TECEquipment> equipment = new ObservableCollection<TECEquipment>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new EquipmentTable()) + " from " + EquipmentTable.TableName;
            command += " where " + EquipmentTable.EquipmentID.Name + " not in ";
            command += "(select " + SystemEquipmentTable.EquipmentID.Name;
            command += " from " + SystemEquipmentTable.TableName + ")";

            DataTable equipmentDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in equipmentDT.Rows)
            { equipment.Add(getEquipmentFromRow(row)); }

            return equipment;
        }
        static private ObservableCollection<TECSubScope> getOrphanSubScope()
        {
            ObservableCollection<TECSubScope> subScope = new ObservableCollection<TECSubScope>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SubScopeTable()) + " from " + SubScopeTable.TableName;
            command += " where " + SubScopeTable.SubScopeID.Name + " not in ";
            command += "(select " + EquipmentSubScopeTable.SubScopeID.Name + " from " + EquipmentSubScopeTable.TableName + ")";
            DataTable subScopeDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in subScopeDT.Rows)
            { subScope.Add(getSubScopeFromRow(row)); }
            return subScope;
        }
        static private ObservableCollection<TECLabeled> getAllLocations()
        {
            ObservableCollection<TECLabeled> locations = new ObservableCollection<TECLabeled>();
            DataTable locationsDT = SQLiteDB.getDataFromTable(LocationTable.TableName);
            foreach (DataRow row in locationsDT.Rows)
            { locations.Add(getLocationFromRow(row)); }
            return locations;
        }
        static private ObservableCollection<TECCost> getAssociatedCosts()
        {
            ObservableCollection<TECCost> associatedCosts = new ObservableCollection<TECCost>();
            DataTable associatedCostsDT = SQLiteDB.getDataFromTable(AssociatedCostTable.TableName);
            foreach (DataRow row in associatedCostsDT.Rows)
            { associatedCosts.Add(getAssociatedCostFromRow(row)); }
            return associatedCosts;
        }
        static private ObservableCollection<TECSubScope> getSubScopeInEquipment(Guid equipmentID)
        {
            ObservableCollection<TECSubScope> subScope = new ObservableCollection<TECSubScope>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SubScopeTable()) + " from (TECSubScope inner join " + EquipmentSubScopeTable.TableName + " on ";
            command += "(TECSubScope.SubScopeID = TECEquipmentTECSubScope.SubScopeID and ";
            command += EquipmentSubScopeTable.EquipmentID.Name + "= '" + equipmentID;
            command += "')) order by " + EquipmentSubScopeTable.ScopeIndex.Name + "";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable subScopeDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in subScopeDT.Rows)
            { subScope.Add(getSubScopeFromRow(row)); }
            return subScope;
        }
        static private ObservableCollection<TECDevice> getDevicesInSubScope(Guid subScopeID)
        {
            ObservableCollection<TECDevice> devices = new ObservableCollection<TECDevice>();

            string command = string.Format("select {0}, {4} from {1} where {2} = '{3}'",
                SubScopeDeviceTable.DeviceID.Name, SubScopeDeviceTable.TableName,
                SubScopeDeviceTable.SubScopeID.Name, subScopeID, SubScopeDeviceTable.Quantity.Name);
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable devicesDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in devicesDT.Rows)
            {
                var deviceToAdd = getPlaceholderSubScopeDeviceFromRow(row);
                int quantity = row[SubScopeDeviceTable.Quantity.Name].ToString().ToInt();
                for (int x = 0; x < quantity; x++)
                { devices.Add(deviceToAdd); }
            }

            return devices;
        }
        static private ObservableCollection<TECPoint> getPointsInSubScope(Guid subScopeID)
        {
            ObservableCollection<TECPoint> points = new ObservableCollection<TECPoint>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new PointTable()) + " from (" + PointTable.TableName + " inner join " + SubScopePointTable.TableName + " on ";
            command += "(TECPoint.PointID = TECSubScopeTECPoint.PointID and ";
            command += SubScopePointTable.SubScopeID.Name + " = '" + subScopeID;
            command += "'))";
            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable pointsDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in pointsDT.Rows)
            { points.Add(getPointFromRow(row)); }

            return points;
        }
        static private TECManufacturer getManufacturerInDevice(Guid deviceID)
        {
            string command = "select " + DeviceManufacturerTable.ManufacturerID.Name + " from " + DeviceManufacturerTable.TableName;
            command += " where " + DeviceManufacturerTable.DeviceID.Name + " = '";
            command += deviceID;
            command += "'";
            DataTable manTable = SQLiteDB.getDataFromCommand(command);
            if (manTable.Rows.Count > 0)
            { return getPlaceholderDeviceManufacturerFromRow(manTable.Rows[0]); }
            else
            { return null; }
        }
        static private TECManufacturer getManufacturerInIOModule(Guid guid)
        {
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ManufacturerTable()) + " from " + ManufacturerTable.TableName + " where " + ManufacturerTable.ManufacturerID.Name + " in ";
            command += "(select " + IOModuleManufacturerTable.ManufacturerID.Name + " from " + IOModuleManufacturerTable.TableName;
            command += " where " + IOModuleManufacturerTable.IOModuleID.Name + " = '";
            command += guid;
            command += "')";

            DataTable manTable = SQLiteDB.getDataFromCommand(command);
            if (manTable.Rows.Count > 0)
            { return getManufacturerFromRow(manTable.Rows[0]); }
            else
            { return null; }
        }
        static private ObservableCollection<TECElectricalMaterial> getConnectionTypesInDevice(Guid deviceID)
        {
            ObservableCollection<TECElectricalMaterial> connectionTypes = new ObservableCollection<TECElectricalMaterial>();
            string command = string.Format("select {0}, {1} from {2} where {3} = '{4}'",
                DeviceConnectionTypeTable.TypeID.Name, DeviceConnectionTypeTable.Quantity.Name, DeviceConnectionTypeTable.TableName,
                DeviceConnectionTypeTable.DeviceID.Name, deviceID);

            DataTable connectionTypeTable = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in connectionTypeTable.Rows)
            {
                var connectionTypeToAdd = new TECElectricalMaterial(new Guid(row[DeviceConnectionTypeTable.TypeID.Name].ToString()));
                int quantity = row[DeviceConnectionTypeTable.Quantity.Name].ToString().ToInt(1);
                for (int x = 0; x < quantity; x++)
                { connectionTypes.Add(connectionTypeToAdd); }
            }
            return connectionTypes;
        }
        static private TECElectricalMaterial getConduitTypeInConnection(Guid connectionID)
        {
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ConduitTypeTable()) + " from " + ConduitTypeTable.TableName + " where " + ConduitTypeTable.ConduitTypeID.Name + " in ";
            command += "(select " + ConnectionConduitTypeTable.TypeID.Name + " from " + ConnectionConduitTypeTable.TableName + " where ";
            command += ConnectionConduitTypeTable.ConnectionID.Name + " = '" + connectionID;
            command += "')";

            DataTable conduitTypeTable = SQLiteDB.getDataFromCommand(command);
            if (conduitTypeTable.Rows.Count > 0)
            { return (getConduitTypeFromRow(conduitTypeTable.Rows[0])); }
            else
            { return null; }
        }
        static private TECElectricalMaterial getConnectionTypeInNetworkConnection(Guid netConnectionID)
        {
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ConnectionTypeTable()) + " from " + ConnectionTypeTable.TableName + " where " + ConnectionTypeTable.ConnectionTypeID.Name + " in ";
            command += "(select " + NetworkConnectionConnectionTypeTable.TypeID.Name + " from " + NetworkConnectionConnectionTypeTable.TableName + " where ";
            command += NetworkConnectionConnectionTypeTable.ConnectionID.Name + " = '" + netConnectionID;
            command += "')";

            DataTable connectionTypeDT = SQLiteDB.getDataFromCommand(command);
            if (connectionTypeDT.Rows.Count > 0)
            {
                return (getConnectionTypeFromRow(connectionTypeDT.Rows[0]));
            }
            else
            {
                return null;
            }
        }
        static private ObservableCollection<TECLabeled> getNotes()
        {
            ObservableCollection<TECLabeled> notes = new ObservableCollection<TECLabeled>();
            DataTable notesDT = SQLiteDB.getDataFromTable(NoteTable.TableName);
            foreach (DataRow row in notesDT.Rows)
            { notes.Add(getNoteFromRow(row)); }
            return notes;
        }
        static private ObservableCollection<TECLabeled> getExclusions()
        {
            ObservableCollection<TECLabeled> exclusions = new ObservableCollection<TECLabeled>();
            DataTable exclusionsDT = SQLiteDB.getDataFromTable(ExclusionTable.TableName);
            foreach (DataRow row in exclusionsDT.Rows)
            { exclusions.Add(getExclusionFromRow(row)); }
            return exclusions;
        }
        static private ObservableCollection<TECLabeled> getAllTags()
        {
            ObservableCollection<TECLabeled> tags = new ObservableCollection<TECLabeled>();
            DataTable tagsDT = SQLiteDB.getDataFromTable(TagTable.TableName);
            foreach (DataRow row in tagsDT.Rows)
            { tags.Add(getTagFromRow(row)); }
            return tags;
        }
        static private ObservableCollection<TECController> getOrphanControllers()
        {
            //Returns the controllers that are not in the ControlledScopeController table.
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ControllerTable()) + " from " + ControllerTable.TableName;
            command += " where " + ControllerTable.ControllerID.Name + " not in ";
            command += "(select " + SystemControllerTable.ControllerID.Name;
            command += " from " + SystemControllerTable.TableName + ")";

            DataTable controllersDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in controllersDT.Rows)
            {
                var controller = getControllerFromRow(row);
                controller.IsGlobal = true;
                controllers.Add(controller);
            }

            return controllers;
        }
        static private ObservableCollection<TECIO> getIOInControllerType(Guid typeId)
        {
            ObservableCollection<TECIO> outIO = new ObservableCollection<TECIO>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new IOTable()) + " from " + IOTable.TableName + " where " + IOTable.IOID.Name + " in ";
            command += "(select " + ControllerTypeIOTable.IOID.Name + " from " + ControllerTypeIOTable.TableName + " where ";
            command += ControllerTypeIOTable.TypeID.Name + " = '" + typeId;
            command += "')";

            DataTable typeDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in typeDT.Rows)
            { outIO.Add(getIOFromRow(row)); }
            return outIO;
        }
        static private ObservableCollection<TECIOModule> getIOModules()
        {
            ObservableCollection<TECIOModule> ioModules = new ObservableCollection<TECIOModule>();
            DataTable ioModuleDT = SQLiteDB.getDataFromTable(IOModuleTable.TableName);

            foreach (DataRow row in ioModuleDT.Rows)
            { ioModules.Add(getIOModuleFromRow(row)); }
            return ioModules;
        }

        static private TECSubScope getSubScopeInSubScopeConnection(Guid connectionID)
        {
            TECSubScope outScope = null;

            //string command = "select * from " + SubScopeTable.TableName + " where " + SubScopeTable.SubScopeID.Name + " in ";
            //command += "(select " + SubScopeConnectionChildrenTable.ChildID.Name + " from " + SubScopeConnectionChildrenTable.TableName + " where ";
            //command += SubScopeConnectionChildrenTable.ConnectionID.Name + " = '" + connectionID;
            //command += "')";
            string command = "select " + SubScopeConnectionChildrenTable.ChildID.Name + " from " + SubScopeConnectionChildrenTable.TableName + " where ";
            command += SubScopeConnectionChildrenTable.ConnectionID.Name + " = '" + connectionID;
            command += "'";

            DataTable scopeDT = SQLiteDB.getDataFromCommand(command);
            if (scopeDT.Rows.Count > 0)
            {
                return getSubScopeConnectionChildPlaceholderFromRow(scopeDT.Rows[0]);
            }

            return outScope;
        }
        static private ObservableCollection<TECController> getControllersInNetworkConnection(Guid connectionID)
        {
            var outScope = new ObservableCollection<TECController>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ControllerTable()) + " from " + ControllerTable.TableName + " where " + ControllerTable.ControllerID.Name + " in ";
            command += "(select " + NetworkConnectionControllerTable.ControllerID.Name + " from " + NetworkConnectionControllerTable.TableName + " where ";
            command += NetworkConnectionControllerTable.ConnectionID.Name + " = '" + connectionID;
            command += "')";

            DataTable scopeDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in scopeDT.Rows)
            { outScope.Add(getControllerPlaceholderFromRow(row)); }

            return outScope;
        }

        static private TECControllerType getTypeInController(Guid controllerID)
        {

            string command = "select " + ControllerControllerTypeTable.TypeID.Name + " from " + ControllerControllerTypeTable.TableName;
            command += " where " + ControllerControllerTypeTable.ControllerID.Name + " = '";
            command += controllerID;
            command += "'";

            DataTable manTable = SQLiteDB.getDataFromCommand(command);
            if (manTable.Rows.Count > 0)
            { return getPlaceholderControllerTypeFromRow(manTable.Rows[0]); }
            else
            { return null; }
        }
        static private TECBidParameters getBidParameters(TECBid bid)
        {
            string constsCommand = "select " + DatabaseHelper.AllFieldsInTableString(new BidParametersTable()) + " from (" + BidParametersTable.TableName + " inner join ";
            constsCommand += BidBidParametersTable.TableName + " on ";
            constsCommand += "(TECBidTECParameters.ParametersID = TECBidTECParameters.ParametersID";
            constsCommand += " and " + BidBidParametersTable.BidID.Name + " = '";
            constsCommand += bid.Guid;
            constsCommand += "'))";

            DataTable DT = SQLiteDB.getDataFromCommand(constsCommand);

            if (DT.Rows.Count > 1)
            {
                DebugHandler.LogError("Multiple rows found in bid paramters table. Using first found.");
            }
            else if (DT.Rows.Count < 1)
            {
                DebugHandler.LogError("Bid paramters not found in database, using default values. Reload labor constants from loaded templates in the labor tab.");
                return new TECBidParameters();
            }
            return getBidParametersFromRow(DT.Rows[0]);
        }
        static private ObservableCollection<TECMisc> getMisc()
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();

            DataTable miscDT = SQLiteDB.getDataFromTable(MiscTable.TableName);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row));
            }

            return misc;
        }
        static private ObservableCollection<TECPanel> getOrphanPanels()
        {
            //Returns the panels that are not in the ControlledScopePanel table.
            ObservableCollection<TECPanel> panels = new ObservableCollection<TECPanel>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new PanelTable()) + " from " + PanelTable.TableName;
            command += " where " + PanelTable.PanelID.Name + " not in ";
            command += "(select " + SystemPanelTable.PanelID.Name;
            command += " from " + SystemPanelTable.TableName + ")";

            DataTable panelsDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in panelsDT.Rows)
            {
                panels.Add(getPanelFromRow(row));
            }

            return panels;
        }
        static private ObservableCollection<TECSystem> getSystems()
        {
            ObservableCollection<TECSystem> systems = new ObservableCollection<TECSystem>();

            string command = "select " + DatabaseHelper.AllFieldsInTableString(new SystemTable()) + " from " + SystemTable.TableName;
            command += " where " + SystemTable.SystemID.Name;
            command += " in (select " + SystemTable.SystemID.Name;
            command += " from " + SystemTable.TableName + " where " + SystemTable.SystemID.Name + " not in ";
            command += "(select " + SystemHierarchyTable.ChildID.Name + " from " + SystemHierarchyTable.TableName + "))";

            DatabaseHelper.Explain(command, SQLiteDB);
            DataTable systemsDT = SQLiteDB.getDataFromCommand(command);

            foreach (DataRow row in systemsDT.Rows)
            {
                systems.Add(getSystemFromRow(row));
            }
            return systems;
        }
        static private TECPanelType getPanelTypeInPanel(Guid guid)
        {
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new PanelTypeTable()) + " from " + PanelTypeTable.TableName + " where " + PanelTypeTable.PanelTypeID.Name + " in ";
            command += "(select " + PanelPanelTypeTable.PanelTypeID.Name + " from " + PanelPanelTypeTable.TableName;
            command += " where " + PanelPanelTypeTable.PanelID.Name + " = '";
            command += guid;
            command += "')";

            DataTable manTable = SQLiteDB.getDataFromCommand(command);
            if (manTable.Rows.Count > 0)
            { return getPanelTypeFromRow(manTable.Rows[0]); }
            else
            { return null; }
        }
        static private ObservableCollection<TECController> getControllersInPanel(Guid guid)
        {
            ObservableCollection<TECController> controllers = new ObservableCollection<TECController>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new ControllerTable()) + " from " + ControllerTable.TableName + " where " + ControllerTable.ControllerID.Name + " in ";
            command += "(select " + PanelControllerTable.ControllerID.Name + " from " + PanelControllerTable.TableName + " where ";
            command += PanelControllerTable.PanelID.Name + " = '" + guid;
            command += "')";

            DataTable controllerDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in controllerDT.Rows)
            { controllers.Add(getControllerFromRow(row)); }

            return controllers;
        }

        static private ObservableCollection<TECConnection> getConnectionsInController(TECController controller)
        {
            var tables = DatabaseHelper.TableNames(SQLiteDB);
            ObservableCollection<TECConnection> outScope = new ObservableCollection<TECConnection>();
            string command;
            DataTable scopeDT;

            if (tables.Contains(NetworkConnectionTable.TableName))
            {
                command = "select " + DatabaseHelper.AllFieldsInTableString(new NetworkConnectionTable()) + " from " + NetworkConnectionTable.TableName + " where " + NetworkConnectionTable.ConnectionID.Name + " in ";
                command += "(select " + ControllerConnectionTable.ConnectionID.Name + " from " + ControllerConnectionTable.TableName + " where ";
                command += ControllerConnectionTable.ControllerID.Name + " = '" + controller.Guid;
                command += "')";

                scopeDT = SQLiteDB.getDataFromCommand(command);
                foreach (DataRow row in scopeDT.Rows)
                {
                    var networkConnection = getNetworkConnectionFromRow(row);
                    networkConnection.ParentController = controller;

                    outScope.Add(networkConnection);
                }
            }

            command = "select " + DatabaseHelper.AllFieldsInTableString(new SubScopeConnectionTable()) + " from " + SubScopeConnectionTable.TableName + " where " + SubScopeConnectionTable.ConnectionID.Name + " in ";
            command += "(select " + ControllerConnectionTable.ConnectionID.Name + " from " + ControllerConnectionTable.TableName + " where ";
            command += ControllerConnectionTable.ControllerID.Name + " = '" + controller.Guid;
            command += "')";

            scopeDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in scopeDT.Rows)
            {
                var subScopeConnection = getSubScopeConnectionFromRow(row);
                subScopeConnection.ParentController = controller;
                outScope.Add(subScopeConnection);
            }

            return outScope;
        }
        static private TECIOModule getModuleInIO(Guid ioID)
        {
            //string command = "select * from " + IOModuleTable.TableName + " where " + IOModuleTable.IOModuleID.Name + " in ";
            //command += "(select " + IOIOModuleTable.ModuleID.Name + " from " + IOIOModuleTable.TableName;
            //command += " where " + IOIOModuleTable.IOID.Name + " = '";
            //command += ioID;
            //command += "')";
            string command = "select " + IOIOModuleTable.ModuleID.Name + " from " + IOIOModuleTable.TableName;
            command += " where " + IOIOModuleTable.IOID.Name + " = '";
            command += ioID;
            command += "'";

            DataTable moduleTable = SQLiteDB.getDataFromCommand(command);
            if (moduleTable.Rows.Count > 0)
            { return getPlaceholderIOModuleFromRow(moduleTable.Rows[0]); }
            else
            { return null; }
        }

        static private ObservableCollection<TECMisc> getMiscInBid()
        {
            ObservableCollection<TECMisc> misc = new ObservableCollection<TECMisc>();
            string command = "select " + DatabaseHelper.AllFieldsInTableString(new MiscTable()) + " from " + MiscTable.TableName + " where " + MiscTable.MiscID.Name + " in ";
            command += "(select " + BidMiscTable.MiscID.Name + " from " + BidMiscTable.TableName;
            command += ")";
            DataTable miscDT = SQLiteDB.getDataFromCommand(command);
            foreach (DataRow row in miscDT.Rows)
            {
                misc.Add(getMiscFromRow(row));
            }

            return misc;
        }

        #endregion //Loading from DB Methods
        
        #region Row to Object Methods
        #region Base Scope
        private static TECSystem getSystemFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SystemTable.SystemID.Name].ToString());
            TECSystem system = new TECSystem(guid);

            system.Name = row[SystemTable.Name.Name].ToString();
            system.Description = row[SystemTable.Description.Name].ToString();
            system.ProposeEquipment = row[SystemTable.ProposeEquipment.Name].ToString().ToInt(0).ToBool();
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            system.Controllers = getControllersInSystem(guid);
            //watch.Stop();
            //Console.WriteLine("getControllersInSystem: " + watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            system.Equipment = getEquipmentInSystem(guid);
            //watch.Stop();
            //Console.WriteLine("getEquipmentInSystem: " + watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            system.Panels = getPanelsInSystem(guid);
            //watch.Stop();
            //Console.WriteLine("getPanelsInSystem: " + watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            system.SystemInstances = getChildrenSystems(guid);
            //watch.Stop();
            //Console.WriteLine("getChildrenSystems: " + watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            system.MiscCosts = getMiscInSystem(guid);
            //watch.Stop();
            //Console.WriteLine("getMiscInSystem: " + watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            system.ScopeBranches = getScopeBranchesInSystem(guid);
            // watch.Stop();
            // Console.WriteLine("getScopeBranchesInSystem: " + watch.ElapsedMilliseconds);
            // watch = System.Diagnostics.Stopwatch.StartNew();
            getScopeChildren(system);
            // watch.Stop();
            // Console.WriteLine("getScopeChildren: " + watch.ElapsedMilliseconds);
            system.RefreshReferences();

            return system;
        }

        private static TECEquipment getEquipmentFromRow(DataRow row)
        {
            Guid equipmentID = new Guid(row[EquipmentTable.EquipmentID.Name].ToString());
            TECEquipment equipmentToAdd = new TECEquipment(equipmentID);
            equipmentToAdd.Name = row[EquipmentTable.Name.Name].ToString();
            equipmentToAdd.Description = row[EquipmentTable.Description.Name].ToString();
            getScopeChildren(equipmentToAdd);
            equipmentToAdd.SubScope = getSubScopeInEquipment(equipmentID);
            return equipmentToAdd;
        }
        private static TECSubScope getSubScopeFromRow(DataRow row)
        {
            Guid subScopeID = new Guid(row[SubScopeTable.SubScopeID.Name].ToString());
            TECSubScope subScopeToAdd = new TECSubScope(subScopeID);
            subScopeToAdd.Name = row[SubScopeTable.Name.Name].ToString();
            subScopeToAdd.Description = row[SubScopeTable.Description.Name].ToString();
            subScopeToAdd.Devices = getDevicesInSubScope(subScopeID);
            subScopeToAdd.Points = getPointsInSubScope(subScopeID);
            getScopeChildren(subScopeToAdd);
            return subScopeToAdd;
        }
        private static TECPoint getPointFromRow(DataRow row)
        {
            Guid pointID = new Guid(row[PointTable.PointID.Name].ToString());
            TECPoint pointToAdd = new TECPoint(pointID);
            pointToAdd.Label = row[PointTable.Name.Name].ToString();
            pointToAdd.Type = TECPoint.convertStringToType(row[PointTable.Type.Name].ToString());
            pointToAdd.Quantity = row[PointTable.Quantity.Name].ToString().ToInt();
            return pointToAdd;
        }
        #endregion
        #region Catalogs
        private static TECElectricalMaterial getConnectionTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ConnectionTypeTable.ConnectionTypeID.Name].ToString());
            string name = row[ConnectionTypeTable.Name.Name].ToString();
            string laborString = row[ConnectionTypeTable.Labor.Name].ToString();
            string costString = row[ConnectionTypeTable.Cost.Name].ToString();

            double cost = costString.ToDouble(0);
            double labor = laborString.ToDouble(0);

            var outConnectionType = new TECElectricalMaterial(guid);
            outConnectionType.Name = name;
            outConnectionType.Cost = cost;
            outConnectionType.Labor = labor;
            getScopeChildren(outConnectionType);
            outConnectionType.RatedCosts = getRatedCostsInComponent(outConnectionType.Guid);
            return outConnectionType;
        }
        private static TECElectricalMaterial getConduitTypeFromRow(DataRow row)
        {
            Guid conduitGuid = new Guid(row[ConduitTypeTable.ConduitTypeID.Name].ToString());
            string name = row[ConduitTypeTable.Name.Name].ToString();
            double cost = row[ConduitTypeTable.Cost.Name].ToString().ToDouble(0);
            double labor = row[ConduitTypeTable.Labor.Name].ToString().ToDouble(0);
            var conduitType = new TECElectricalMaterial(conduitGuid);
            conduitType.Name = name;
            conduitType.Cost = cost;
            conduitType.Labor = labor;
            getScopeChildren(conduitType);
            conduitType.RatedCosts = getRatedCostsInComponent(conduitType.Guid);
            return conduitType;
        }

        private static TECCost getAssociatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[AssociatedCostTable.AssociatedCostID.Name].ToString());
            string name = row[AssociatedCostTable.Name.Name].ToString();
            double cost = row[AssociatedCostTable.Cost.Name].ToString().ToDouble(0);
            double labor = row[AssociatedCostTable.Labor.Name].ToString().ToDouble(0);
            string costTypeString = row[AssociatedCostTable.Type.Name].ToString();

            var associatedCost = new TECCost(guid);

            associatedCost.Name = name;
            associatedCost.Cost = cost;
            associatedCost.Labor = labor;
            associatedCost.Type = UtilitiesMethods.StringToEnum(costTypeString, CostType.None);

            return associatedCost;
        }
        private static TECDevice getDeviceFromRow(DataRow row)
        {
            Guid deviceID = new Guid(row[DeviceTable.DeviceID.Name].ToString());
            ObservableCollection<TECElectricalMaterial> connectionType = getConnectionTypesInDevice(deviceID);
            TECManufacturer manufacturer = getManufacturerInDevice(deviceID);
            TECDevice deviceToAdd = new TECDevice(deviceID, connectionType, manufacturer);
            deviceToAdd.Name = row[DeviceTable.Name.Name].ToString();
            deviceToAdd.Description = row[DeviceTable.Description.Name].ToString();
            deviceToAdd.Cost = row[DeviceTable.Cost.Name].ToString().ToDouble();
            getScopeChildren(deviceToAdd);
            return deviceToAdd;
        }
        private static TECManufacturer getManufacturerFromRow(DataRow row)
        {
            Guid manufacturerID = new Guid(row[ManufacturerTable.ManufacturerID.Name].ToString());
            var manufacturer = new TECManufacturer(manufacturerID);
            manufacturer.Label = row[ManufacturerTable.Name.Name].ToString();
            manufacturer.Multiplier = row[ManufacturerTable.Multiplier.Name].ToString().ToDouble(1);
            return manufacturer;
        }
        private static TECLabeled getLocationFromRow(DataRow row)
        {
            Guid locationID = new Guid(row[LocationTable.LocationID.Name].ToString());
            var location = new TECLabeled(locationID);
            location.Label = row[LocationTable.Name.Name].ToString();
            return location;
        }
        private static TECLabeled getTagFromRow(DataRow row)
        {
            var tag = new TECLabeled(new Guid(row["TagID"].ToString()));
            tag.Label = row["TagString"].ToString();
            tag.Flavor = Flavor.Tag;
            return tag;
        }
        /// <summary>
        /// MUST BE FIXED
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static TECPanelType getPanelTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[PanelTypeTable.PanelTypeID.Name].ToString());
            TECPanelType panelType = new TECPanelType(guid, new TECManufacturer());

            panelType.Name = row[PanelTypeTable.Name.Name].ToString();
            panelType.Cost = row[PanelTypeTable.Cost.Name].ToString().ToDouble(0);
            panelType.Labor = row[PanelTypeTable.Labor.Name].ToString().ToDouble(0);

            return panelType;
        }
        private static TECIOModule getIOModuleFromRow(DataRow row)
        {
            Guid guid = new Guid(row[IOModuleTable.IOModuleID.Name].ToString());
            TECManufacturer manufacturer = getManufacturerInIOModule(guid);

            TECIOModule module = new TECIOModule(guid, manufacturer);

            module.Name = row[IOModuleTable.Name.Name].ToString();
            module.Description = row[IOModuleTable.Description.Name].ToString();
            module.Cost = row[IOModuleTable.Cost.Name].ToString().ToDouble(0);
            module.IOPerModule = row[IOModuleTable.IOPerModule.Name].ToString().ToInt(1);
            return module;
        }

        #endregion
        #region Scope Qualifiers
        private static TECScopeBranch getScopeBranchFromRow(DataRow row)
        {
            Guid scopeBranchID = new Guid(row[ScopeBranchTable.ScopeBranchID.Name].ToString());
            TECScopeBranch branch = new TECScopeBranch(scopeBranchID);
            branch.Label = row[ScopeBranchTable.Label.Name].ToString();
            branch.Branches = getChildBranchesInBranch(scopeBranchID);
            return branch;
        }
        private static TECLabeled getNoteFromRow(DataRow row)
        {
            Guid noteID = new Guid(row[NoteTable.NoteID.Name].ToString());
            var note = new TECLabeled(noteID);
            note.Label = row["NoteText"].ToString();
            note.Flavor = Flavor.Note;
            return note;
        }
        private static TECLabeled getExclusionFromRow(DataRow row)
        {
            Guid exclusionId = new Guid(row["ExclusionID"].ToString());
            TECLabeled exclusion = new TECLabeled(exclusionId);
            exclusion.Label = row["ExclusionText"].ToString();
            exclusion.Flavor = Flavor.Exclusion;
            return exclusion;
        }
        #endregion
        
        #region Control Scope
        private static TECPanel getPanelFromRow(DataRow row)
        {
            Guid guid = new Guid(row[PanelTable.PanelID.Name].ToString());
            TECPanelType type = getPanelTypeInPanel(guid);
            TECPanel panel = new TECPanel(guid, type);

            panel.Name = row[PanelTable.Name.Name].ToString();
            panel.Description = row[PanelTable.Description.Name].ToString();
            panel.Controllers = getControllersInPanel(guid);
            getScopeChildren(panel);

            return panel;
        }
        private static TECController getControllerFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerTable.ControllerID.Name].ToString());
            TECController controller = new TECController(guid, getTypeInController(guid));

            controller.Name = row[ControllerTable.Name.Name].ToString();
            controller.Description = row[ControllerTable.Description.Name].ToString();
            controller.NetworkType = UtilitiesMethods.StringToEnum<NetworkType>(row[ControllerTable.Type.Name].ToString(), 0);
            getScopeChildren(controller);
            controller.ChildrenConnections = getConnectionsInController(controller);
            return controller;
        }
        private static TECIO getIOFromRow(DataRow row)
        {
            Guid guid = new Guid(row[IOTable.IOID.Name].ToString());
            var io = new TECIO(guid);
            io.Type = TECIO.convertStringToType(row[IOTable.IOType.Name].ToString());
            io.Quantity = row[IOTable.Quantity.Name].ToString().ToInt();
            io.IOModule = getModuleInIO(guid);
            return io;
        }
        private static TECSubScopeConnection getSubScopeConnectionFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SubScopeConnectionTable.ConnectionID.Name].ToString());
            TECSubScopeConnection connection = new TECSubScopeConnection(guid);
            connection.Length = row[SubScopeConnectionTable.Length.Name].ToString().ToDouble();
            connection.ConduitLength = row[SubScopeConnectionTable.ConduitLength.Name].ToString().ToDouble(0);
            connection.ConduitType = getConduitTypeInConnection(connection.Guid);
            connection.SubScope = getSubScopeInSubScopeConnection(connection.Guid);
            return connection;
        }
        private static TECNetworkConnection getNetworkConnectionFromRow(DataRow row)
        {
            Guid guid = new Guid(row[NetworkConnectionTable.ConnectionID.Name].ToString());
            TECNetworkConnection connection = new TECNetworkConnection(guid);
            connection.Length = row[NetworkConnectionTable.Length.Name].ToString().ToDouble();
            connection.ConduitLength = row[NetworkConnectionTable.ConduitLength.Name].ToString().ToDouble(0);
            connection.IOType = UtilitiesMethods.StringToEnum<IOType>(row[NetworkConnectionTable.IOType.Name].ToString());
            connection.ConduitType = getConduitTypeInConnection(connection.Guid);
            connection.ChildrenControllers = getControllersInNetworkConnection(connection.Guid);
            connection.ConnectionType = getConnectionTypeInNetworkConnection(connection.Guid);
            return connection;
        }
        #endregion

        #region Misc
        private static TECMisc getMiscFromRow(DataRow row)
        {
            Guid guid = new Guid(row[MiscTable.MiscID.Name].ToString());
            TECMisc cost = new TECMisc(guid);

            cost.Name = row[MiscTable.Name.Name].ToString();
            cost.Cost = row[MiscTable.Cost.Name].ToString().ToDouble(0);
            cost.Labor = row[MiscTable.Labor.Name].ToString().ToDouble(0);
            cost.Quantity = row[MiscTable.Quantity.Name].ToString().ToInt(1);
            string costTypeString = row[AssociatedCostTable.Type.Name].ToString();
            cost.Type = UtilitiesMethods.StringToEnum(costTypeString, CostType.None);
            getScopeChildren(cost);
            return cost;
        }
        private static TECBidParameters getBidParametersFromRow(DataRow row)
        {
            Guid guid = new Guid(row[BidParametersTable.ParametersID.Name].ToString());
            TECBidParameters paramters = new TECBidParameters(guid);

            paramters.Escalation = row[BidParametersTable.Escalation.Name].ToString().ToDouble(0);
            paramters.Overhead = row[BidParametersTable.Overhead.Name].ToString().ToDouble(0);
            paramters.Profit = row[BidParametersTable.Profit.Name].ToString().ToDouble(0);
            paramters.SubcontractorMarkup = row[BidParametersTable.SubcontractorMarkup.Name].ToString().ToDouble(0);
            paramters.SubcontractorEscalation = row[BidParametersTable.SubcontractorEscalation.Name].ToString().ToDouble(0);

            paramters.IsTaxExempt = row[BidParametersTable.IsTaxExempt.Name].ToString().ToInt(0).ToBool();
            paramters.RequiresBond = row[BidParametersTable.RequiresBond.Name].ToString().ToInt(0).ToBool();
            paramters.RequiresWrapUp = row[BidParametersTable.RequiresWrapUp.Name].ToString().ToInt(0).ToBool();

            return paramters;
        }

        #endregion

        private static void getScopeChildren(TECScope scope)
        {
            scope.Tags = getTagsInScope(scope.Guid);
            scope.AssociatedCosts = getAssociatedCostsInScope(scope.Guid);
        }

        private static void getLocatedChildren(TECLocated located)
        {
            located.Location = getLocationInScope(located.Guid);
        }

        #region Placeholder
        private static TECSubScope getSubScopeConnectionChildPlaceholderFromRow(DataRow row)
        {
            Guid subScopeID = new Guid(row[SubScopeConnectionChildrenTable.ChildID.Name].ToString());
            TECSubScope subScopeToAdd = new TECSubScope(subScopeID);
            return subScopeToAdd;
        }
        private static TECController getControllerPlaceholderFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerTable.ControllerID.Name].ToString());
            TECController controller = new TECController(guid, new TECControllerType(new TECManufacturer()));

            controller.Name = row[ControllerTable.Name.Name].ToString();
            controller.Description = row[ControllerTable.Description.Name].ToString();
            return controller;
        }
        private static TECLabeled getPlaceholderTagFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScopeTagTable.TagID.Name].ToString());
            TECLabeled tag = new TECLabeled(guid);
            return tag;
        }
        private static TECCost getPlaceholderAssociatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ScopeAssociatedCostTable.AssociatedCostID.Name].ToString());
            TECCost associatedCost = new TECCost(guid);
            return associatedCost;
        }
        private static TECCost getPlaceholderRatedCostFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ElectricalMaterialRatedCostTable.CostID.Name].ToString());
            TECCost associatedCost = new TECCost(guid);
            return associatedCost;
        }
        private static TECLabeled getPlaceholderLocationFromRow(DataRow row)
        {
            Guid guid = new Guid(row[LocationScopeTable.LocationID.Name].ToString());
            TECLabeled location = new TECLabeled(guid);
            return location;
        }
        private static TECDevice getPlaceholderSubScopeDeviceFromRow(DataRow row)
        {
            Guid guid = new Guid(row[SubScopeDeviceTable.DeviceID.Name].ToString());
            ObservableCollection<TECElectricalMaterial> connectionTypes = new ObservableCollection<TECElectricalMaterial>();
            TECManufacturer manufacturer = new TECManufacturer();
            TECDevice device = new TECDevice(guid, connectionTypes, manufacturer);
            device.Description = "placeholder";
            return device;
        }
        private static TECManufacturer getPlaceholderDeviceManufacturerFromRow(DataRow row)
        {
            Guid guid = new Guid(row[DeviceManufacturerTable.ManufacturerID.Name].ToString());
            TECManufacturer man = new TECManufacturer(guid);
            return man;
        }
        private static TECControllerType getPlaceholderControllerTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[ControllerTypeTable.TypeID.Name].ToString());
            TECControllerType type = new TECControllerType(guid, new TECManufacturer());
            return type;
        }
        private static TECElectricalMaterial getPlaceholderDeviceConnectionTypeFromRow(DataRow row)
        {
            Guid guid = new Guid(row[DeviceConnectionTypeTable.TypeID.Name].ToString());
            TECElectricalMaterial connectionType = new TECElectricalMaterial(guid);
            return connectionType;
        }
        private static TECIOModule getPlaceholderIOModuleFromRow(DataRow row)
        {
            Guid guid = new Guid(row[IOIOModuleTable.ModuleID.Name].ToString());
            TECIOModule module = new TECIOModule(guid, new TECManufacturer());
            module.Description = "placeholder";
            return module;
        }
        private static void addRowToPlaceholderDict(DataRow row, Dictionary<Guid, List<Guid>> dict)
        {
            Guid key = new Guid(row[CharacteristicScopeInstanceScopeTable.CharacteristicID.Name].ToString());
            Guid value = new Guid(row[CharacteristicScopeInstanceScopeTable.InstanceID.Name].ToString());

            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<Guid>();
            }
            dict[key].Add(value);
        }
        #endregion
        #endregion
        
    }
}
