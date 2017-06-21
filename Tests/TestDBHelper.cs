﻿using EstimatingUtilitiesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Tests
{
    public static class TestDBHelper
    {
        static private SQLiteDatabase SQLiteDB;

        public static void CreateTestBid(string path)
        {
            DatabaseHelper.CreateDB(path);
            SQLiteDB = new SQLiteDatabase(path);
            SQLiteDB.nonQueryCommand("BEGIN TRANSACTION");
            AddToBidInfoTable();
            AddToBidParametersTable();
            AddToLaborConstantsTable();
            AddToSubcontractorConstantsTable();
            AddToUserAdjustmentsTable();
            AddToNoteTable();
            AddToExlusionTable();
            AddToScopeBranchTable();
            AddToSystemTable();
            AddToEquipmentTable();
            AddToSubScopeTable();
            AddToDeviceTable();
            AddToPointTable();
            AddToTagTable();
            AddToManufacturerTable();
            AddToDrawingTable();
            AddToPageTable();
            AddToLocationTable();
            AddToVisualScopeTable();
            AddToConnectionTypeTable();
            AddToConduitTypeTable();
            AddToAssociatedCostTable();
            AddToSubScopeConnectionTable();
            AddToNetworkConnectionTable();
            AddToControllerTable();
            AddToMiscTable();
            AddToPanelTypeTable();
            AddToPanelTable();
            AddToIOModuleTable();
            AddToIOTable();
            AddToBidLaborTable();
            AddToBidBidParametersTable();
            AddToBidScopeBranchTable();
            AddToBidMiscTable();
            AddToControllerIOTable();
            AddToIOModuleManufacturerTable();
            AddToIOIOModuleTable();
            AddToControllerConnectionTable();
            AddToScopeBranchHierarchyTable();
            AddToBidSystemTable();
            AddToSystemEquipmentTable();
            AddToEquipmentSubScopeTable();
            AddToSubScopeDeviceTable();
            AddToSubScopePointTable();
            AddToScopeTagTable();
            AddToDeviceManufacturerTable();
            AddToDeviceConnectionTypeTable();
            AddToDrawingPageTable();
            AddToPageVisualScopeTable();
            AddToVisualScopeScopeTable();
            AddToLocationScopeTable();
            AddToScopeAssociatedCostTable();
            AddToControllerManufacturerTable();
            AddToConnectionConduitTypeTable();
            AddToNetworkConnectionConnectionTypeTable();
            AddToNetworkConnectionControllerTable();
            AddToSubScopeConnectionChildrenTable();
            AddToPanelPanelTypeTable();
            AddToPanelControllerTable();
            AddToSystemPanelTable();
            AddToSystemScopeBranchTable();
            AddToSystemHierarchyTable();
            AddToSystemMiscTable();
            AddToCharacteristicScopeInstanceScopeTable();
            
            SQLiteDB.nonQueryCommand("END TRANSACTION");
            SQLiteDB.Connection.Close();
        }
        
        private static void AddDataToTable(TableBase table, List<string> values)
        {
            TableInfo info = new TableInfo(table);
            Dictionary<string, string> data = new Dictionary<string, string>();
            if(info.Fields.Count != values.Count)
            {
                throw new Exception("There must be one value per field");
            }
            for(int x = 0; x < info.Fields.Count; x++)
            {
                var field = info.Fields[x];
                var value = values[x];
                data[field.Name] = value;
            }

            SQLiteDB.Insert(info.Name, data);
        }

        private static void AddToBidInfoTable()
        {
            List<string> values = new List<string>();
            values.Add("1.6.0.13");
            values.Add("Testimate");
            values.Add("d8788062-92d2-4889-b9f2-02a7a28aff05");
            values.Add("7357");
            values.Add("1969-07-20T20:18:04.0000000");
            values.Add("Mrs. Salesperson");
            values.Add("Mr. Estimator");
            AddDataToTable(new BidInfoTable(), values);

        }
        private static void AddToTemplatesInfoTable()
        {
            throw new NotImplementedException();
            //List<string> values = new List<string>();
            //values.Add("1.6.0.13");
            //values.Add("Testimate");
            //values.Add("d8788062-92d2-4889-b9f2-02a7a28aff05");
            //values.Add("7357");
            //values.Add("1969-07-20T20:18:04.0000000");
            //values.Add("Mrs. Salesperson");
            //values.Add("Mr. Estimator");
            //AddDataToTable(new BidInfoTable(), values);
        }
        private static void AddToBidParametersTable()
        {
            List<string> values = new List<string>();
            values.Add("655ed4a6-4ce4-431f-ae4b-7185e28d20ef");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            AddDataToTable(new BidParametersTable(), values);

        }
        private static void AddToLaborConstantsTable()
        {
            List<string> values = new List<string>();
            values.Add("ab534ec6-73ec-4145-9c58-3abbbc9ae3d5");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            AddDataToTable(new LaborConstantsTable(), values);
        }
        private static void AddToSubcontractorConstantsTable()
        {
            List<string> values = new List<string>();
            values.Add("ab534ec6-73ec-4145-9c58-3abbbc9ae3d5");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            AddDataToTable(new SubcontractorConstantsTable(), values);
        }
        private static void AddToUserAdjustmentsTable()
        {
            List<string> values = new List<string>();
            values.Add("d8788062-92d2-4889-b9f2-02a7a28aff05");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            values.Add("NONE");
            AddDataToTable(new UserAdjustmentsTable(), values);
        }
        private static void AddToNoteTable()
        {
            List<string> values = new List<string>();
            values.Add("50f3a707-fc1b-4eb3-9413-1dbde57b1d90");
            values.Add("Test Note");
            AddDataToTable(new NoteTable(), values);
        }
        private static void AddToExlusionTable()
        {
            List<string> values = new List<string>();
            values.Add("15692e12-e728-4f1b-b65c-de365e016e7a");
            values.Add("Test Exclusion");
            AddDataToTable(new ExclusionTable(), values);
        }
        private static void AddToScopeBranchTable()
        {
            List<string> values = new List<string>();
            values.Add("25e815fa-4ac7-4b69-9640-5ae220f0cd40");
            values.Add("Bid Scope Branch");
            values.Add("Bid Scope Branch Description");
            AddDataToTable(new ScopeBranchTable(), values);

            values = new List<string>();
            values.Add("814710f1-f2dd-4ae6-9bc4-9279288e4994");
            values.Add("System Scope Branch");
            values.Add("System Scope Branch Description");
            AddDataToTable(new ScopeBranchTable(), values);
        }
        private static void AddToSystemTable()
        {
            List<string> values = new List<string>();
            values.Add("ebdbcc85-10f4-46b3-99e7-d896679f874a");
            values.Add("Typical System");
            values.Add("Typical System Description");
            values.Add("1");
            values.Add("100");
            values.Add("1");
            AddDataToTable(new SystemTable(), values);

            values = new List<string>();
            values.Add("ba2e71d4-a2b9-471a-9229-9fbad7432bf7");
            values.Add("Instance System");
            values.Add("Instance System Description");
            values.Add("1");
            values.Add("100");
            values.Add("0");
            AddDataToTable(new SystemTable(), values);
        }
        private static void AddToEquipmentTable()
        {
            List<string> values = new List<string>();
            values.Add("8a9bcc02-6ae2-4ac9-bbe1-e33d9a590b0e");
            values.Add("Typical Equip");
            values.Add("Typical Equip Description");
            values.Add("1");
            values.Add("50");
            AddDataToTable(new EquipmentTable(), values);

            values = new List<string>();
            values.Add("cdd9d7f7-ff3e-44ff-990f-c1b721e0ff8d");
            values.Add("Instance Equip");
            values.Add("Instance Equip Description");
            values.Add("1");
            values.Add("50");
            AddDataToTable(new EquipmentTable(), values);
        }
        private static void AddToSubScopeTable()
        {
            List<string> values = new List<string>();
            values.Add("fbe0a143-e7cd-4580-a1c4-26eff0cd55a6");
            values.Add("Typical SS");
            values.Add("Typical SS Description");
            values.Add("1");
            AddDataToTable(new SubScopeTable(), values);

            values = new List<string>();
            values.Add("cdd9d7f7-ff3e-44ff-990f-c1b721e0ff8d");
            values.Add("Instance SS");
            values.Add("Instance SS Description");
            values.Add("1");
            AddDataToTable(new SubScopeTable(), values);
        }
        private static void AddToDeviceTable()
        {
            List<string> values = new List<string>();
            values.Add("95135fdf-7565-4d22-b9e4-1f177febae15");
            values.Add("Test Device");
            values.Add("Test Device Description");
            values.Add("123.45");
            AddDataToTable(new DeviceTable(), values);
        }
        private static void AddToPointTable()
        {
            List<string> values = new List<string>();
            values.Add("03a16819-9205-4e65-a16b-96616309f171");
            values.Add("Typical Point");
            values.Add("Typical Point Description");
            values.Add("1");
            values.Add("AI");
            AddDataToTable(new PointTable(), values);

            values = new List<string>();
            values.Add("e60437bc-09a1-47eb-9fd5-78711d942a12");
            values.Add("Instance Point");
            values.Add("Instance Point Description");
            values.Add("1");
            values.Add("AI");
            AddDataToTable(new PointTable(), values);
        }
        private static void AddToTagTable()
        {
            List<string> values = new List<string>();
            values.Add("09fd531f-94f9-48ee-8d16-00e80c1d58b9");
            values.Add("Test Tag");
            AddDataToTable(new TagTable(), values);
        }
        private static void AddToManufacturerTable()
        {
            List<string> values = new List<string>();
            values.Add("90cd6eae-f7a3-4296-a9eb-b810a417766d");
            values.Add("Test Manufacturer");
            values.Add("0.5");
            AddDataToTable(new ManufacturerTable(), values);
        }
        private static void AddToDrawingTable()
        {

        }
        private static void AddToPageTable()
        {

        }
        private static void AddToLocationTable()
        {
            List<string> values = new List<string>();
            values.Add("4175d04b-82b1-486b-b742-b2cc875405cb");
            values.Add("Test Location");
            AddDataToTable(new LocationTable(), values);
        }
        private static void AddToVisualScopeTable()
        {

        }
        private static void AddToConnectionTypeTable()
        {
            List<string> values = new List<string>();
            values.Add("f38867c8-3846-461f-a6fa-c941aeb723c7");
            values.Add("Test Connection Type");
            values.Add("12.48");
            values.Add("84.21");
            AddDataToTable(new ConnectionTypeTable(), values);
        }
        private static void AddToConduitTypeTable()
        {
            List<string> values = new List<string>();
            values.Add("8d442906-efa2-49a0-ad21-f6b27852c9ef");
            values.Add("Test Conduit Type");
            values.Add("45.67");
            values.Add("76.54");
            AddDataToTable(new ConduitTypeTable(), values);
        }
        private static void AddToAssociatedCostTable()
        {
            List<string> values = new List<string>();
            values.Add("1c2a7631-9e3b-4006-ada7-12d6cee52f08");
            values.Add("Test Associated Cost");
            values.Add("31");
            values.Add("13");
            values.Add("TEC");
            AddDataToTable(new AssociatedCostTable(), values);

            values = new List<string>();
            values.Add("63ed1eb7-c05b-440b-9e15-397f64ff05c7");
            values.Add("Test Electrical Associated Cost");
            values.Add("42");
            values.Add("24");
            values.Add("Electrical");
            AddDataToTable(new AssociatedCostTable(), values);
        }
        private static void AddToSubScopeConnectionTable()
        {
            List<string> values = new List<string>();
            values.Add("5723e279-ac5c-4ee0-ae01-494a0c524b5c");
            values.Add("40");
            values.Add("20");
            AddDataToTable(new SubScopeConnectionTable(), values);

            values = new List<string>();
            values.Add("560ffd84-444d-4611-a346-266074f62f6f");
            values.Add("50");
            values.Add("30");
            AddDataToTable(new SubScopeConnectionTable(), values);
        }
        private static void AddToNetworkConnectionTable()
        {
            List<string> values = new List<string>();
            values.Add("4f93907a-9aab-4ed5-8e55-43aab2af5ef8");
            values.Add("100");
            values.Add("80");
            values.Add("BACnetIP");
            AddDataToTable(new SubScopeConnectionTable(), values);
        }
        private static void AddToControllerTable()
        {
            List<string> values = new List<string>();
            values.Add("98e6bc3e-31dc-4394-8b54-9ca53c193f46");
            values.Add("Bid Controller");
            values.Add("Bid Controller Description");
            values.Add("1812");
            values.Add("Server");
            AddDataToTable(new ControllerTable(), values);

            values = new List<string>();
            values.Add("1bb86714-2512-4fdd-a80f-46969753d8a0");
            values.Add("Typical Controller");
            values.Add("Typical Controller Description");
            values.Add("1776");
            values.Add("0");
            AddDataToTable(new AssociatedCostTable(), values);

            values = new List<string>();
            values.Add("f22913a6-e348-4a77-821f-80447621c6e0");
            values.Add("Instance Controller");
            values.Add("Instance Controller Description");
            values.Add("1776");
            values.Add("DDC");
            AddDataToTable(new AssociatedCostTable(), values);
        }
        private static void AddToMiscTable()
        {
            List<string> values = new List<string>();
            values.Add("5df99701-1d7b-4fbe-843d-40793f4145a8");
            values.Add("Bid Misc");
            values.Add("1298");
            values.Add("8921");
            values.Add("2");
            values.Add("Electrical");
            AddDataToTable(new MiscTable(), values);

            values = new List<string>();
            values.Add("e3ecee54-1f90-415a-b493-90a78f618476");
            values.Add("System Misc");
            values.Add("1492");
            values.Add("2941");
            values.Add("3");
            values.Add("TEC");
            AddDataToTable(new MiscTable(), values);
        }
        private static void AddToPanelTypeTable()
        {
            List<string> values = new List<string>();
            values.Add("04e3204c-b35f-4e1a-8a01-db07f7eb055e");
            values.Add("Test Panel Type");
            values.Add("1324");
            values.Add("4231");
            AddDataToTable(new PanelTypeTable(), values);
        }
        private static void AddToPanelTable()
        {
            List<string> values = new List<string>();
            values.Add("a8cdd31c-e690-4eaa-81ea-602c72904391");
            values.Add("Bid Panel");
            values.Add("Bid Panel Description");
            values.Add("1");
            AddDataToTable(new PanelTypeTable(), values);

            values = new List<string>();
            values.Add("04e3204c-b35f-4e1a-8a01-db07f7eb055e");
            values.Add("Test Panel Type");
            values.Add("1324");
            values.Add("4231");
            AddDataToTable(new PanelTypeTable(), values);

            values = new List<string>();
            values.Add("04e3204c-b35f-4e1a-8a01-db07f7eb055e");
            values.Add("Test Panel Type");
            values.Add("1324");
            values.Add("4231");
            AddDataToTable(new PanelTypeTable(), values);
        }
        private static void AddToIOModuleTable()
        {

        }
        private static void AddToIOTable()
        {

        }

        private static void AddToBidLaborTable()
        {

        }
        private static void AddToBidBidParametersTable()
        {

        }
        private static void AddToBidScopeBranchTable()
        {

        }
        private static void AddToBidMiscTable()
        {

        }
        private static void AddToControllerIOTable()
        {

        }
        private static void AddToIOModuleManufacturerTable()
        {

        }
        private static void AddToIOIOModuleTable()
        {

        }
        private static void AddToControllerConnectionTable()
        {

        }
        private static void AddToScopeBranchHierarchyTable()
        {

        }
        private static void AddToBidSystemTable()
        {

        }
        private static void AddToSystemEquipmentTable()
        {

        }
        private static void AddToEquipmentSubScopeTable()
        {

        }
        private static void AddToSubScopeDeviceTable()
        {

        }
        private static void AddToSubScopePointTable()
        {

        }
        private static void AddToScopeTagTable()
        {

        }
        private static void AddToDeviceManufacturerTable()
        {

        }
        private static void AddToDeviceConnectionTypeTable()
        {

        }
        private static void AddToDrawingPageTable()
        {

        }
        private static void AddToPageVisualScopeTable()
        {

        }
        private static void AddToVisualScopeScopeTable()
        {

        }
        private static void AddToLocationScopeTable()
        {

        }
        private static void AddToScopeAssociatedCostTable()
        {

        }
        private static void AddToControllerManufacturerTable()
        {

        }
        private static void AddToConnectionConduitTypeTable()
        {

        }
        private static void AddToNetworkConnectionConnectionTypeTable()
        {

        }
        private static void AddToNetworkConnectionControllerTable()
        {

        }
        private static void AddToSubScopeConnectionChildrenTable()
        {

        }
        private static void AddToPanelPanelTypeTable()
        {

        }
        private static void AddToPanelControllerTable()
        {

        }
        private static void AddToSystemControllerTable()
        {

        }
        private static void AddToSystemPanelTable()
        {

        }
        private static void AddToSystemScopeBranchTable()
        {

        }
        private static void AddToSystemHierarchyTable()
        {

        }
        private static void AddToSystemMiscTable()
        {

        }
        private static void AddToCharacteristicScopeInstanceScopeTable()
        {

        }
    }
}
