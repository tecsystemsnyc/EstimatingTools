using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System.IO;
using EstimatingUtilitiesLibrary.Database;
using System.Linq;
using EstimatingLibrary.Interfaces;

namespace Legacy
{
    /// <summary>
    /// Summary description for LegacyDBTests
    /// </summary>
    [TestClass]
    public class LegacyTemplateDBTests
    {

        static TECTemplates actualTemplates;

        static Guid TEST_TAG_GUID = new Guid("09fd531f-94f9-48ee-8d16-00e80c1d58b9");
        static Guid TEST_TEC_COST_GUID = new Guid("1c2a7631-9e3b-4006-ada7-12d6cee52f08");
        static Guid TEST_ELECTRICAL_COST_GUID = new Guid("63ed1eb7-c05b-440b-9e15-397f64ff05c7");
        static Guid TEST_LOCATION_GUID = new Guid("4175d04b-82b1-486b-b742-b2cc875405cb");
        static Guid TEST_RATED_COST_GUID = new Guid("b7c01526-c195-442f-a1f1-28d07db61144");

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            var path = Path.GetTempFileName();
            LegacyDBGenerator.CreateTestTemplates(path);
            DatabaseManager<TECTemplates> manager = new DatabaseManager<TECTemplates>(path);
            actualTemplates = manager.Load() as TECTemplates;
        }
        
        [TestMethod]
        public void Load_Templates_Parameters()
        {
            double expectedEscalation = 10;
            double expectedSubcontractorEscalation = 10;
            bool expectedIsTaxExempt = false;
            bool expectedRequiresBond = false;
            bool expectedRequiresWrapUp = false;
            double expectedMarkup = 20;

            Assert.AreEqual(expectedEscalation, actualTemplates.Templates.Parameters.First().Escalation, "Escalation didn't load properly.");
            Assert.AreEqual(expectedMarkup, actualTemplates.Templates.Parameters.First().Markup, "Markup didn't load properly.");
            Assert.AreEqual(expectedSubcontractorEscalation, actualTemplates.Templates.Parameters.First().SubcontractorEscalation, "Subcontractor escalation didn't load properly.");
            Assert.AreEqual(expectedIsTaxExempt, actualTemplates.Templates.Parameters.First().IsTaxExempt, "Is tax exempt didn't load properly.");
            Assert.AreEqual(expectedRequiresBond, actualTemplates.Templates.Parameters.First().RequiresBond, "Requires bond didn't load properly.");
            Assert.AreEqual(expectedRequiresWrapUp, actualTemplates.Templates.Parameters.First().RequiresWrapUp, "Requires wrap up didn't load properly.");
        }

        [TestMethod]
        public void Load_Templates_LaborConsts()
        {
            //Assert
            double expectedPMCoef = 2;
            double expectedPMRate = 30;
            Assert.AreEqual(expectedPMCoef, actualTemplates.Templates.Parameters.First().PMCoef, "PM Coefficient didn't load properly.");
            Assert.AreEqual(expectedPMRate, actualTemplates.Templates.Parameters.First().PMRate, "PM Rate didn't load properly.");

            double expectedENGCoef = 2;
            double expectedENGRate = 40;
            Assert.AreEqual(expectedENGCoef, actualTemplates.Templates.Parameters.First().ENGCoef, "ENG Coefficient didn't load properly.");
            Assert.AreEqual(expectedENGRate, actualTemplates.Templates.Parameters.First().ENGRate, "ENG Rate didn't load properly.");

            double expectedCommCoef = 2;
            double expectedCommRate = 50;
            Assert.AreEqual(expectedCommCoef, actualTemplates.Templates.Parameters.First().CommCoef, "Comm Coefficient didn't load properly.");
            Assert.AreEqual(expectedCommRate, actualTemplates.Templates.Parameters.First().CommRate, "Comm Rate didn't load properly.");

            double expectedSoftCoef = 2;
            double expectedSoftRate = 60;
            Assert.AreEqual(expectedSoftCoef, actualTemplates.Templates.Parameters.First().SoftCoef, "Software Coefficient didn't load properly.");
            Assert.AreEqual(expectedSoftRate, actualTemplates.Templates.Parameters.First().SoftRate, "Software Rate didn't load properly.");

            double expectedGraphCoef = 2;
            double expectedGraphRate = 70;
            Assert.AreEqual(expectedGraphCoef, actualTemplates.Templates.Parameters.First().GraphCoef, "Graphics Coefficient didn't load properly.");
            Assert.AreEqual(expectedGraphRate, actualTemplates.Templates.Parameters.First().GraphRate, "Graphics Rate didn't load properly.");
        }

        [TestMethod]
        public void Load_Templates_SubcontractorConsts()
        {
            //Assert
            double expectedElectricalRate = 50;
            double expectedElectricalSuperRate = 60;
            double expectedElectricalNonUnionRate = 30;
            double expectedElectricalSuperNonUnionRate = 40;
            double expectedElectricalSuperRatio = 0.25;
            bool expectedOT = false;
            bool expectedUnion = true;
            Assert.AreEqual(expectedElectricalRate, actualTemplates.Templates.Parameters.First().ElectricalRate, "Electrical rate didn't load properly.");
            Assert.AreEqual(expectedElectricalSuperRate, actualTemplates.Templates.Parameters.First().ElectricalSuperRate, "Electrical Supervision rate didn't load properly.");
            Assert.AreEqual(expectedElectricalNonUnionRate, actualTemplates.Templates.Parameters.First().ElectricalNonUnionRate, "Electrical Non-Union rate didn't load properly.");
            Assert.AreEqual(expectedElectricalSuperNonUnionRate, actualTemplates.Templates.Parameters.First().ElectricalSuperNonUnionRate, "Electrical Supervision Non-Union rate didn't load properly.");
            Assert.AreEqual(expectedElectricalSuperRatio, actualTemplates.Templates.Parameters.First().ElectricalSuperRatio, "Electrical Supervision time ratio didn't load properly.");
            Assert.AreEqual(expectedOT, actualTemplates.Templates.Parameters.First().ElectricalIsOnOvertime, "Electrical overtime bool didn't load properly.");
            Assert.AreEqual(expectedUnion, actualTemplates.Templates.Parameters.First().ElectricalIsUnion, "Electrical union bool didn't load properly.");
        }
        
        [TestMethod]
        public void Load_Templates_SystemTemplates_ScopeTree()
        {
            Guid expectedParentGuid = new Guid("814710f1-f2dd-4ae6-9bc4-9279288e4994");
            string expectedParentName = "System Scope Branch";
            Guid expectedChildGuid = new Guid("542802f6-a7b1-4020-9be4-e58225c433a8");
            string expectedChildName = "System Child Branch";

            TECScopeBranch actualParent = null;
            TECScopeBranch actualChild = null;
            foreach (TECSystem typical in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECScopeBranch branch in typical.ScopeBranches)
                {
                    if (branch.Guid == expectedParentGuid)
                    {
                        actualParent = branch;
                        foreach (TECScopeBranch child in branch.Branches)
                        {
                            if (child.Guid == expectedChildGuid)
                            {
                                actualChild = child;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (actualParent != null) break;
            }

            Assert.AreEqual(expectedParentName, actualParent.Label, "Parent scope branch name didn't load properly.");
            Assert.AreEqual(expectedChildName, actualChild.Label, "Child scope branch name didn't load properly.");
        }

        [TestMethod]
        public void Load_Templates_TypicalSystem()
        {
            //Arrange
            Guid expectedGuid = new Guid("ebdbcc85-10f4-46b3-99e7-d896679f874a");
            string expectedName = "Typical System";
            string expectedDescription = "Typical System Description";

            Guid childEquipment = new Guid("8a9bcc02-6ae2-4ac9-bbe1-e33d9a590b0e");
            Guid childController = new Guid("1bb86714-2512-4fdd-a80f-46969753d8a0");
            Guid childPanel = new Guid("e7695d68-d79f-44a2-92f5-b303436186af");

            TECSystem actualSystem = null;
            foreach (TECSystem system in actualTemplates.Templates.SystemTemplates)
            {
                if (system.Guid == expectedGuid)
                {
                    actualSystem = system;
                    break;
                }
            }

            bool foundEquip = false;
            foreach (TECEquipment equip in actualSystem.Equipment)
            {
                if (equip.Guid == childEquipment)
                {
                    foundEquip = true;
                    break;
                }
            }
            bool foundControl = false;
            foreach (TECController control in actualSystem.Controllers)
            {
                if (control.Guid == childController)
                {
                    foundControl = true;
                    break;
                }
            }
            bool foundPanel = false;
            foreach (TECPanel panel in actualSystem.Panels)
            {
                if (panel.Guid == childPanel)
                {
                    foundPanel = true;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualSystem.Name);
            Assert.AreEqual(expectedDescription, actualSystem.Description);

            Assert.IsTrue(foundEquip, "Typical equipment not loaded properly into typical system.");
            Assert.IsTrue(foundControl, "Typical controller not loaded properly into typical system.");
            Assert.IsTrue(foundPanel, "Typical panel not loaded properly into typical system.");

            testForTag(actualSystem);
            testForCosts(actualSystem);
        }
        
        [TestMethod]
        public void Load_Templates_TypicalEquipment()
        {
            Guid expectedGuid = new Guid("8a9bcc02-6ae2-4ac9-bbe1-e33d9a590b0e");
            string expectedName = "Typical Equip";
            string expectedDescription = "Typical Equip Description";

            Guid childSubScope = new Guid("fbe0a143-e7cd-4580-a1c4-26eff0cd55a6");

            TECEquipment actualEquipment = null;
            foreach (TECSystem typical in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECEquipment equip in typical.Equipment)
                {
                    if (equip.Guid == expectedGuid)
                    {
                        actualEquipment = equip;
                        break;
                    }
                }
                if (actualEquipment != null) break;
            }

            bool foundSubScope = false;
            foreach (TECSubScope ss in actualEquipment.SubScope)
            {
                if (ss.Guid == childSubScope)
                {
                    foundSubScope = true;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualEquipment.Name);
            Assert.AreEqual(expectedDescription, actualEquipment.Description);

            Assert.IsTrue(foundSubScope, "Typical subscope not loaded properly into typical equipment.");

            testForTag(actualEquipment);
            testForCosts(actualEquipment);
        }
        
        [TestMethod]
        public void Load_Templates_TypicalSubScope()
        {
            //Arrange
            Guid expectedGuid = new Guid("fbe0a143-e7cd-4580-a1c4-26eff0cd55a6");
            string expectedName = "Typical SS";
            string expectedDescription = "Typical SS Description";

            Guid childPoint = new Guid("03a16819-9205-4e65-a16b-96616309f171");
            Guid childDevice = new Guid("95135fdf-7565-4d22-b9e4-1f177febae15");
            Guid expectedConnectionGuid = new Guid("5723e279-ac5c-4ee0-ae01-494a0c524b5c");

            TECSubScope actualSubScope = null;
            foreach (TECSystem system in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECEquipment equipment in system.Equipment)
                {
                    foreach (TECSubScope subScope in equipment.SubScope)
                    {
                        if (subScope.Guid == expectedGuid)
                        {
                            actualSubScope = subScope;
                            break;
                        }
                    }
                    if (actualSubScope != null)
                    {
                        break;
                    }
                }
                if (actualSubScope != null)
                {
                    break;
                }
            }

            bool foundPoint = false;
            foreach (TECPoint point in actualSubScope.Points)
            {
                if (point.Guid == childPoint)
                {
                    foundPoint = true;
                    break;
                }
            }
            bool foundDevice = false;
            foreach (TECDevice device in actualSubScope.Devices)
            {
                if (device.Guid == childDevice)
                {
                    foundDevice = true;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualSubScope.Name, "Name not loaded");
            Assert.AreEqual(expectedDescription, actualSubScope.Description, "Description not loaded");
            Assert.AreEqual(expectedConnectionGuid, actualSubScope.Connection.Guid, "Connection not loaded");

            Assert.IsTrue(foundPoint, "Typical point not loaded into typical subscope properly.");
            Assert.IsTrue(foundDevice, "Typical device not loaded into typical subscope properly.");

            testForTag(actualSubScope);
            testForCosts(actualSubScope);
        }
        
        [TestMethod]
        public void Load_Templates_Device()
        {
            Guid expectedGuid = new Guid("95135fdf-7565-4d22-b9e4-1f177febae15");
            string expectedName = "Test Device";
            string expectedDescription = "Test Device Description";
            double expectedCost = 123.45;

            Guid manufacturerGuid = new Guid("90cd6eae-f7a3-4296-a9eb-b810a417766d");
            Guid connectionTypeGuid = new Guid("f38867c8-3846-461f-a6fa-c941aeb723c7");

            TECDevice actualDevice = null;
            foreach (TECDevice dev in actualTemplates.Catalogs.Devices)
            {
                if (dev.Guid == expectedGuid)
                {
                    actualDevice = dev;
                    break;
                }
            }

            bool foundConnectionType = false;
            foreach (TECElectricalMaterial connectType in actualDevice.HardwiredConnectionTypes)
            {
                if (connectType.Guid == connectionTypeGuid)
                {
                    foundConnectionType = true;
                    break;
                }
            }

            Assert.AreEqual(expectedName, actualDevice.Name, "Device name didn't load properly.");
            Assert.AreEqual(expectedDescription, actualDevice.Description, "Device description didn't load properly.");
            Assert.AreEqual(expectedCost, actualDevice.Price, "Device cost didn't load properly.");
            Assert.AreEqual(manufacturerGuid, actualDevice.Manufacturer.Guid, "Manufacturer didn't load properly into device.");

            Assert.IsTrue(foundConnectionType, "Connection type didn't load properly into device.");

            testForTag(actualDevice);
            testForCosts(actualDevice);
        }

        [TestMethod]
        public void Load_Templates_TypicalPoint()
        {
            Guid expectedGuid = new Guid("03a16819-9205-4e65-a16b-96616309f171");
            string expectedName = "Typical Point";
            int expectedQuantity = 1;
            IOType expectedType = IOType.AI;

            TECPoint actualPoint = null;
            foreach (TECSystem typical in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECEquipment equip in typical.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        foreach (TECPoint point in ss.Points)
                        {
                            if (point.Guid == expectedGuid)
                            {
                                actualPoint = point;
                                break;
                            }
                        }
                        if (actualPoint != null) break;
                    }
                    if (actualPoint != null) break;
                }
                if (actualPoint != null) break;
            }

            Assert.AreEqual(expectedName, actualPoint.Label, "Typical point name didn't load properly.");
            Assert.AreEqual(expectedQuantity, actualPoint.Quantity, "Typical point quantity didn't load properly.");
            Assert.AreEqual(expectedType, actualPoint.Type, "Typical point type didn't load properly.");
        }
        
        [TestMethod]
        public void Load_Templates_Tag()
        {
            Guid expectedGuid = new Guid("09fd531f-94f9-48ee-8d16-00e80c1d58b9");
            string expectedString = "Test Tag";

            TECLabeled actualTag = null;
            foreach (TECTag tag in actualTemplates.Catalogs.Tags)
            {
                if (tag.Guid == expectedGuid)
                {
                    actualTag = tag;
                    break;
                }
            }

            Assert.AreEqual(expectedString, actualTag.Label, "Tag text didn't load properly.");
        }

        [TestMethod]
        public void Load_Templates_Manufacturer()
        {
            //Arrange
            Guid expectedGuid = new Guid("90cd6eae-f7a3-4296-a9eb-b810a417766d");
            string expectedName = "Test Manufacturer";
            double expectedMultiplier = 0.5;


            TECManufacturer actualManufacturer = null;
            foreach (TECManufacturer man in actualTemplates.Catalogs.Manufacturers)
            {
                if (man.Guid == expectedGuid)
                {
                    actualManufacturer = man;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualManufacturer.Label);
            Assert.AreEqual(expectedMultiplier, actualManufacturer.Multiplier);
        }
        
        [TestMethod]
        public void Load_Templates_ConnectionType()
        {
            //Arrange
            Guid expectedGuid = new Guid("f38867c8-3846-461f-a6fa-c941aeb723c7");
            string expectedName = "Test Connection Type";
            double expectedCost = 12.48;
            double expectedLabor = 84.21;

            TECElectricalMaterial actualConnectionType = null;
            foreach (TECElectricalMaterial connectionType in actualTemplates.Catalogs.ConnectionTypes)
            {
                if (connectionType.Guid == expectedGuid)
                {
                    actualConnectionType = connectionType;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualConnectionType.Name);
            Assert.AreEqual(expectedCost, actualConnectionType.Cost);
            Assert.AreEqual(expectedLabor, actualConnectionType.Labor);

            testForCosts(actualConnectionType);
            testForRatedCosts(actualConnectionType);
        }

        [TestMethod]
        public void Load_Templates_ConduitType()
        {
            //Arrange
            Guid expectedGuid = new Guid("8d442906-efa2-49a0-ad21-f6b27852c9ef");
            string expectedName = "Test Conduit Type";
            double expectedCost = 45.67;
            double expectedLabor = 76.54;

            TECElectricalMaterial actualConduitType = null;
            foreach (TECElectricalMaterial conduitType in actualTemplates.Catalogs.ConduitTypes)
            {
                if (conduitType.Guid == expectedGuid)
                {
                    actualConduitType = conduitType;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualConduitType.Name);
            Assert.AreEqual(expectedCost, actualConduitType.Cost);
            Assert.AreEqual(expectedLabor, actualConduitType.Labor);

            testForCosts(actualConduitType);
            testForRatedCosts(actualConduitType);
        }

        [TestMethod]
        public void Load_Templates_AssociatedCosts()
        {
            Guid expectedTECGuid = new Guid("1c2a7631-9e3b-4006-ada7-12d6cee52f08");
            string expectedTECName = "Test TEC Associated Cost";
            double expectedICost = 31;
            double expectedTECLabor = 13;
            CostType expectedTECType = CostType.TEC;

            Guid expectedElectricalGuid = new Guid("63ed1eb7-c05b-440b-9e15-397f64ff05c7");
            string expectedElectricalName = "Test Electrical Associated Cost";
            double expectedElectricalCost = 42;
            double expectedElectricalLabor = 24;
            CostType expectedElectricalType = CostType.Electrical;

            ICost actualICost = null;
            ICost actualElectricalCost = null;
            foreach (ICost cost in actualTemplates.Catalogs.AssociatedCosts)
            {
                if (cost.Guid == expectedTECGuid)
                {
                    actualICost = cost;
                }
                else if (cost.Guid == expectedElectricalGuid)
                {
                    actualElectricalCost = cost;
                }
                if (actualICost != null && actualElectricalCost != null)
                {
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedTECName, actualICost.Name, "TEC cost name didn't load properly.");
            Assert.AreEqual(expectedICost, actualICost.Cost, "TEC cost cost didn't load properly.");
            Assert.AreEqual(expectedTECLabor, actualICost.Labor, "TEC cost labor didn't load properly.");
            Assert.AreEqual(expectedTECType, actualICost.Type, "TEC cost type didn't load properly.");

            Assert.AreEqual(expectedElectricalName, actualElectricalCost.Name, "Electrical cost name didn't load properly.");
            Assert.AreEqual(expectedElectricalCost, actualElectricalCost.Cost, "Electrical cost cost didn't load properly.");
            Assert.AreEqual(expectedElectricalLabor, actualElectricalCost.Labor, "Electrical cost labor didn't load properly.");
            Assert.AreEqual(expectedElectricalType, actualElectricalCost.Type, "Electrical cost type didn't load properly.");
        }

        [TestMethod]
        public void Load_Templates_TypicalSubScopeConnection()
        {
            Guid expectedGuid = new Guid("5723e279-ac5c-4ee0-ae01-494a0c524b5c");
            double expectedWireLength = 40;
            double expectedConduitLength = 20;

            Guid expectedParentControllerGuid = new Guid("1bb86714-2512-4fdd-a80f-46969753d8a0");
            Guid expectedConduitTypeGuid = new Guid("8d442906-efa2-49a0-ad21-f6b27852c9ef");
            Guid expectedSubScopeGuid = new Guid("fbe0a143-e7cd-4580-a1c4-26eff0cd55a6");

            TECHardwiredConnection actualSSConnect = null;
            foreach (TECSystem typical in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECController controller in typical.Controllers)
                {
                    foreach (IControllerConnection connection in controller.ChildrenConnections)
                    {
                        if (connection.Guid == expectedGuid)
                        {
                            actualSSConnect = (connection as TECHardwiredConnection);
                            break;
                        }
                    }
                    if (actualSSConnect != null) break;
                }
                if (actualSSConnect != null) break;
            }

            //Assert
            Assert.AreEqual(expectedWireLength, actualSSConnect.Length, "Length didn't load properly in subscope connection.");
            Assert.AreEqual(expectedConduitLength, actualSSConnect.ConduitLength, "ConduitLength didn't load properly in subscope connection.");

            Assert.AreEqual(expectedParentControllerGuid, actualSSConnect.ParentController.Guid, "Parent controller didn't load properly in subscope connection.");
            Assert.AreEqual(expectedConduitTypeGuid, actualSSConnect.ConduitType.Guid, "Conduit type didn't load properly in subscope connection.");
            Assert.AreEqual(expectedSubScopeGuid, actualSSConnect.Child.Guid, "Subscope didn't load properly in subscope connection.");
        }
        
        [TestMethod]
        public void Load_Templates_SystemTypicalController()
        {
            //Arrange
            Guid expectedGuid = new Guid("1bb86714-2512-4fdd-a80f-46969753d8a0");
            string expectedName = "Typical Controller";
            string expectedDescription = "Typical Controller Description";

            TECController actualController = null;
            foreach (TECSystem system in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECController controller in system.Controllers)
                {
                    if (controller.Guid == expectedGuid)
                    {
                        actualController = controller;
                        break;
                    }
                }
            }

            Guid expectedConnectionGuid = new Guid("5723e279-ac5c-4ee0-ae01-494a0c524b5c");
            Guid expectedIOGuid = new Guid("fbae3851-3320-4e94-a674-ddec86bc4964");

            //bool hasIO = false;
            //foreach (TECIO io in actualController.IO)
            //{
            //    if (io.Guid == expectedIOGuid)
            //    {
            //        hasIO = true;
            //        break;
            //    }
            //}

            bool hasConnection = false;
            foreach (IControllerConnection conn in actualController.ChildrenConnections)
            {
                if (conn.Guid == expectedConnectionGuid)
                {
                    hasConnection = true;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualController.Name);
            Assert.AreEqual(expectedDescription, actualController.Description);
            //Assert.IsTrue(hasIO, "IO not loaded");
            Assert.IsTrue(hasConnection, "Connection not loaded");
            testForTag(actualController);
            testForCosts(actualController);
        }
        
        [TestMethod]
        public void Load_Templates_MiscCost()
        {
            //Arrange
            Guid expectedGuid = new Guid("5df99701-1d7b-4fbe-843d-40793f4145a8");
            string expectedName = "Bid Misc";
            double expectedCost = 1298;
            double expectedLabor = 8921;
            double expectedQuantity = 2;
            CostType expectedType = CostType.Electrical;
            TECMisc actualMisc = null;
            foreach (TECMisc misc in actualTemplates.Templates.MiscCostTemplates)
            {
                if (misc.Guid == expectedGuid)
                {
                    actualMisc = misc;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualMisc.Name);
            Assert.AreEqual(expectedQuantity, actualMisc.Quantity);
            Assert.AreEqual(expectedCost, actualMisc.Cost);
            Assert.AreEqual(expectedLabor, actualMisc.Labor);
            Assert.AreEqual(expectedType, actualMisc.Type);
        }

        [TestMethod]
        public void Load_Templates_SystemMiscCost()
        {
            //Arrange
            Guid expectedGuid = new Guid("e3ecee54-1f90-415a-b493-90a78f618476");
            string expectedName = "System Misc";
            double expectedCost = 1492;
            double expectedLabor = 2941;
            double expectedQuantity = 3;
            CostType expectedType = CostType.TEC;
            TECMisc actualMisc = null;
            foreach (TECSystem system in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECMisc misc in system.MiscCosts)
                {
                    if (misc.Guid == expectedGuid)
                    {
                        actualMisc = misc;
                        break;
                    }
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualMisc.Name);
            Assert.AreEqual(expectedQuantity, actualMisc.Quantity);
            Assert.AreEqual(expectedCost, actualMisc.Cost);
            Assert.AreEqual(expectedLabor, actualMisc.Labor);
            Assert.AreEqual(expectedType, actualMisc.Type);
        }

        [TestMethod]
        public void Load_Templates_PanelType()
        {
            //Arrange
            Guid expectedGuid = new Guid("04e3204c-b35f-4e1a-8a01-db07f7eb055e");
            string expectedName = "Test Panel Type";
            double expectedCost = 1324;
            double expectedLabor = 4231;

            TECPanelType actualType = null;
            foreach (TECPanelType type in actualTemplates.Catalogs.PanelTypes)
            {
                if (type.Guid == expectedGuid)
                {
                    actualType = type;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualType.Name);
            Assert.AreEqual(expectedCost, actualType.Cost);
            Assert.AreEqual(expectedLabor, actualType.Labor);
        }
        
        [TestMethod]
        public void Load_Templates_TypicalPanel()
        {
            //Arrange
            Guid expectedGuid = new Guid("e7695d68-d79f-44a2-92f5-b303436186af");
            string expectedName = "Typical Panel";
            string expectedDescription = "Typical Panel Description";
            //int expectedQuantity = 1;

            Guid expectedTypeGuid = new Guid("04e3204c-b35f-4e1a-8a01-db07f7eb055e");

            TECPanel actualPanel = null;
            foreach (TECSystem system in actualTemplates.Templates.SystemTemplates)
            {
                foreach (TECPanel panel in system.Panels)
                {
                    if (panel.Guid == expectedGuid)
                    {
                        actualPanel = panel;
                        break;
                    }
                }
                if (actualPanel != null) break;
            }

            //Assert
            Assert.AreEqual(expectedName, actualPanel.Name);
            Assert.AreEqual(expectedDescription, actualPanel.Description);
            Assert.AreEqual(expectedTypeGuid, actualPanel.Type.Guid);
            testForCosts(actualPanel);
        }
        
        [TestMethod]
        public void Load_Templates_IOModule()
        {
            //Arrange
            Guid expectedGuid = new Guid("b346378d-dc72-4dda-b275-bbe03022dd12");
            string expectedName = "Test IO Module";
            string expectedDescription = "Test IO Module Description";
            double expectedCost = 2233;

            TECIOModule actualModule = null;
            foreach (TECIOModule module in actualTemplates.Catalogs.IOModules)
            {
                if (module.Guid == expectedGuid)
                {
                    actualModule = module;
                }
            }

            //Assert
            Assert.AreEqual(expectedName, actualModule.Name);
            Assert.AreEqual(expectedDescription, actualModule.Description);
            Assert.AreEqual(expectedCost, actualModule.Price);
        }

        private void testForScopeChildren(TECScope scope)
        {
            testForTag(scope);
            testForCosts(scope);
            if (scope is TECLocated located)
            {
                testForLocation(located);
            }
        }

        private void testForTag(TECScope scope)
        {
            bool foundTag = false;

            foreach (TECTag tag in scope.Tags)
            {
                if (tag.Guid == TEST_TAG_GUID)
                {
                    foundTag = true;
                    break;
                }
            }

            Assert.IsTrue(foundTag, "Tag not loaded properly into scope.");
        }
        private void testForCosts(TECScope scope)
        {
            bool foundICost = false;
            bool foundElectricalCost = false;

            foreach (ICost cost in scope.AssociatedCosts)
            {
                if (cost.Guid == TEST_TEC_COST_GUID)
                {
                    foundICost = true;
                    break;
                }
            }
            foreach (ICost cost in scope.AssociatedCosts)
            {
                if (cost.Guid == TEST_ELECTRICAL_COST_GUID)
                {
                    foundElectricalCost = true;
                    break;
                }
            }

            Assert.IsTrue(foundICost, "TEC Cost not loaded properly into scope.");
            Assert.IsTrue(foundElectricalCost, "Electrical Cost not loaded properly into scope.");
        }
        private void testForLocation(TECLocated scope)
        {
            bool foundLocation = (scope.Location.Guid == TEST_LOCATION_GUID);
            Assert.IsTrue(foundLocation, "Location not loaded properly into scope.");
        }

        private void testForRatedCosts(TECElectricalMaterial component)
        {
            bool foundCost = false;

            foreach (ICost cost in component.RatedCosts)
            {
                if (cost.Guid == TEST_RATED_COST_GUID)
                {
                    foundCost = true;
                    break;
                }
            }

            Assert.IsTrue(foundCost, "Rated Cost not loaded properly into scope.");
        }

    }
}
