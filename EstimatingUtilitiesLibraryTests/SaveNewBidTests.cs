using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TestLibrary.ModelTestingUtilities;

namespace EstimatingUtilitiesLibraryTests
{
    [TestClass]
    public class SaveNewBidTests
    {
        public static double DELTA = 0.0001;

        static TECBid expectedBid;
        static TECParameters expectedParameters;
        static TECExtraLabor expectedLabor;
        static TECSystem expectedSystem;
        static TECSystem expectedSystem1;
        static TECEquipment expectedEquipment;
        static TECSubScope expectedSubScope;
        static TECDevice expectedDevice;
        static TECManufacturer expectedManufacturer;
        static TECPoint expectedPoint;
        static TECScopeBranch expectedBranch;
        static TECLabeled expectedNote;
        static TECLabeled expectedExclusion;
        static TECLabeled expectedTag;
        static TECProvidedController expectedController;

        static string path;

        static TECBid actualBid;
        static TECExtraLabor actualLabor;
        static TECSystem actualSystem;
        static TECSystem actualSystem1;
        static TECEquipment actualEquipment;
        static TECSubScope actualSubScope;
        static TECDevice actualDevice;
        static ObservableCollection<IEndDevice> actualDevices;
        static TECManufacturer actualManufacturer;
        static TECPoint actualPoint;
        static TECScopeBranch actualBranch;
        static TECLabeled actualNote;
        static TECLabeled actualExclusion;
        static TECLabeled actualTag;
        static TECProvidedController actualController;


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

        static Random rand;
            
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            rand = new Random(0);
            //Arrange
            expectedBid = ModelCreation.TestBid(rand);
            expectedLabor = expectedBid.ExtraLabor;
            expectedParameters = expectedBid.Parameters;
            expectedSystem = expectedBid.Systems.First(x => x.Location != null);
            expectedSystem1 = expectedBid.Systems.First(x => x != expectedSystem);

            expectedEquipment = expectedSystem.Equipment.First(x => x.SubScope.Count > 0 && x.Location != null);
            expectedSubScope = expectedEquipment.SubScope.First(x => x.Location != null);
            expectedDevice = expectedBid.Catalogs.Devices.Where(x => x.Tags.Count > 0).First();

            expectedManufacturer = expectedDevice.Manufacturer;
            expectedPoint = expectedSubScope.Points[0];

            expectedBranch = expectedBid.ScopeTree.First(x => x.Branches.Count > 0 && x.Branches[0].Branches.Count > 0);
            expectedNote = expectedBid.Notes[0];
            expectedExclusion = expectedBid.Exclusions[0];
            expectedTag = expectedBid.Catalogs.Tags[0];

            expectedController = (TECProvidedController)expectedBid.Controllers.First(item => item is TECProvidedController);

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(expectedBid);
            actualBid = manager.Load();

            if (actualBid.Systems.Count == 0)
            {
                string failDirectory = Path.GetTempPath() + "Estimating Tools\\";
                Directory.CreateDirectory(failDirectory);
                string failPath = failDirectory + "SaveNewBidTestFailed.edb";
                if (File.Exists(failPath))
                {
                    File.Delete(failPath);
                }
                File.Copy(path, failPath);
                Assert.Fail(string.Format("No systems loaded into bid. File saved at: {0}", failPath));
            }

            actualLabor = actualBid.ExtraLabor;
            actualBid.Parameters = actualBid.Parameters;
            actualSystem = actualBid.FindChild(expectedSystem.Guid) as TECTypical;
            actualSystem1 = actualBid.FindChild(expectedSystem1.Guid) as TECTypical;
            actualEquipment = actualBid.FindChild(expectedEquipment.Guid) as TECEquipment;
            actualSubScope = actualBid.FindChild(expectedSubScope.Guid) as TECSubScope;
            actualDevices = actualSubScope.Devices;
            actualDevice = actualBid.FindChild(expectedDevice.Guid) as TECDevice;
            actualPoint = actualBid.FindChild(expectedPoint.Guid) as TECPoint;
            actualManufacturer = actualBid.FindChild(expectedManufacturer.Guid) as TECManufacturer;
            actualBranch = actualBid.FindChild(expectedBranch.Guid) as TECScopeBranch;
            actualNote = actualBid.FindChild(expectedNote.Guid) as TECLabeled;
            actualExclusion = actualBid.FindChild(expectedExclusion.Guid) as TECLabeled;
            actualTag = actualBid.FindChild(expectedTag.Guid) as TECTag;
            actualController = actualBid.FindChild(expectedController.Guid) as TECProvidedController;
            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(path);
        }

        [TestMethod]
        public void SaveAs_Bid_Info()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.Name = "Test name";
            bid.BidNumber = "1234";
            bid.DueDate = new DateTime();
            bid.Salesperson = "Ms. Salesperson";
            bid.Estimator = "Mr. Estimator";
            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            //Assert
            Assert.AreEqual(bid.Name, loadedBid.Name);
            Assert.AreEqual(bid.BidNumber, loadedBid.BidNumber);
            Assert.AreEqual(bid.DueDate, loadedBid.DueDate);
            Assert.AreEqual(bid.Salesperson, loadedBid.Salesperson);
            Assert.AreEqual(bid.Estimator, loadedBid.Estimator);
        }

        [TestMethod]
        public void SaveAs_Bid_LaborConstants()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.Parameters.PMCoef = 0.5;
            bid.Parameters.PMRate = 0.5;
            bid.Parameters.ENGCoef = 0.5;
            bid.Parameters.ENGRate = 0.5;
            bid.Parameters.CommCoef = 0.5;
            bid.Parameters.CommRate = 0.5;
            bid.Parameters.SoftCoef = 0.5;
            bid.Parameters.SoftRate = 0.5;
            bid.Parameters.GraphCoef = 0.5;
            bid.Parameters.GraphRate = 0.5;
            bid.Parameters.IsTaxExempt = true;

            bid.Parameters.ElectricalRate = 0.5;
            bid.Parameters.ElectricalNonUnionRate = 0.5;
            bid.Parameters.ElectricalSuperRate = 0.5;
            bid.Parameters.ElectricalSuperNonUnionRate = 0.5;
            bid.Parameters.ElectricalIsOnOvertime = true;
            bid.Parameters.ElectricalIsUnion = false;

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            //Assert
            Assert.AreEqual(bid.Parameters.IsTaxExempt, loadedBid.Parameters.IsTaxExempt);
            
            Assert.AreEqual(bid.Parameters.PMCoef, loadedBid.Parameters.PMCoef);
            Assert.AreEqual(bid.Parameters.PMRate, loadedBid.Parameters.PMRate);

            Assert.AreEqual(bid.Parameters.ENGCoef, loadedBid.Parameters.ENGCoef);
            Assert.AreEqual(bid.Parameters.ENGRate, loadedBid.Parameters.ENGRate);

            Assert.AreEqual(bid.Parameters.CommCoef, loadedBid.Parameters.CommCoef);
            Assert.AreEqual(bid.Parameters.CommRate, loadedBid.Parameters.CommRate);

            Assert.AreEqual(bid.Parameters.SoftCoef, loadedBid.Parameters.SoftCoef);
            Assert.AreEqual(bid.Parameters.SoftRate, loadedBid.Parameters.SoftRate);

            Assert.AreEqual(bid.Parameters.GraphCoef, loadedBid.Parameters.GraphCoef);
            Assert.AreEqual(bid.Parameters.GraphRate, loadedBid.Parameters.GraphRate);

            //Assert
            Assert.AreEqual(bid.Parameters.ElectricalRate, loadedBid.Parameters.ElectricalRate);
            Assert.AreEqual(bid.Parameters.ElectricalNonUnionRate, loadedBid.Parameters.ElectricalNonUnionRate);
            Assert.AreEqual(bid.Parameters.ElectricalSuperRate, loadedBid.Parameters.ElectricalSuperRate);
            Assert.AreEqual(bid.Parameters.ElectricalSuperNonUnionRate, loadedBid.Parameters.ElectricalSuperNonUnionRate);

            Assert.AreEqual(bid.Parameters.ElectricalIsOnOvertime, loadedBid.Parameters.ElectricalIsOnOvertime);
            Assert.AreEqual(bid.Parameters.ElectricalIsUnion, loadedBid.Parameters.ElectricalIsUnion);
        }
        
        [TestMethod]
        public void SaveAs_Bid_UserAdjustments()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.ExtraLabor.PMExtraHours = 0.5;
            bid.ExtraLabor.ENGExtraHours = 0.5;
            bid.ExtraLabor.CommExtraHours = 0.5;
            bid.ExtraLabor.SoftExtraHours = 0.5;
            bid.ExtraLabor.GraphExtraHours = 0.5;

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            //Assert
            Assert.AreEqual(bid.ExtraLabor.PMExtraHours, loadedBid.ExtraLabor.PMExtraHours);
            Assert.AreEqual(bid.ExtraLabor.ENGExtraHours, loadedBid.ExtraLabor.ENGExtraHours);
            Assert.AreEqual(bid.ExtraLabor.CommExtraHours, loadedBid.ExtraLabor.CommExtraHours);
            Assert.AreEqual(bid.ExtraLabor.SoftExtraHours, loadedBid.ExtraLabor.SoftExtraHours);
            Assert.AreEqual(bid.ExtraLabor.GraphExtraHours, loadedBid.ExtraLabor.GraphExtraHours);
        }
        
        [TestMethod]
        public void SaveAs_Bid_System()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand, 5);
            TECTypical expectedSystem = ModelCreation.TestTypical(bid.Catalogs, rand);
            bid.Systems.Add(expectedSystem);
            expectedSystem.AddInstance();

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            TECTypical actualSystem = null;
            foreach(TECTypical system in loadedBid.Systems)
            {
                if(system.Guid == expectedSystem.Guid)
                {
                    actualSystem = system;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
            Assert.AreEqual(expectedSystem.Instances.Count, actualSystem.Instances.Count);
            Assert.AreEqual(expectedSystem.Equipment.Count, actualSystem.Equipment.Count);
            Assert.AreEqual(expectedSystem.Controllers.Count, actualSystem.Controllers.Count);
            Assert.AreEqual(expectedSystem.Panels.Count, actualSystem.Panels.Count);
            Assert.AreEqual(expectedSystem.ScopeBranches.Count, actualSystem.ScopeBranches.Count);
            Assert.AreEqual(expectedSystem.AssociatedCosts.Count, actualSystem.AssociatedCosts.Count);
            Assert.AreEqual(expectedSystem.MiscCosts.Count, actualSystem.MiscCosts.Count);
            Assert.AreEqual(expectedSystem.TypicalInstanceDictionary.GetFullDictionary().Count, actualSystem.TypicalInstanceDictionary.GetFullDictionary().Count);
            Assert.IsTrue(compareCosts(expectedSystem.CostBatch, actualSystem.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_Equipment()
        {

            //Arrange
            TECBid bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand, 5);
            TECTypical system = new TECTypical();
            TECEquipment expectedEquipment = ModelCreation.TestEquipment(bid.Catalogs, rand);
            system.Equipment.Add(expectedEquipment);
            bid.Systems.Add(system);

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            TECEquipment actualEquipment = loadedBid.FindChild(expectedEquipment.Guid) as TECEquipment;

            //Assert
            Assert.AreEqual(expectedEquipment.Name, actualEquipment.Name);
            Assert.AreEqual(expectedEquipment.Description, actualEquipment.Description);
            Assert.IsTrue(compareCosts(expectedEquipment.CostBatch, actualEquipment.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_SubScope()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand, 5);
            TECTypical system = new TECTypical();
            TECEquipment expectedEquipment = new TECEquipment();
            TECSubScope expectedSubScope = ModelCreation.TestSubScope(bid.Catalogs, rand);
            expectedEquipment.SubScope.Add(expectedSubScope);
            system.Equipment.Add(expectedEquipment);

            bid.Systems.Add(system);

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            TECSubScope actualSubScope = loadedBid.FindChild(expectedSubScope.Guid) as TECSubScope;
            
            //Assert
            Assert.AreEqual(expectedSubScope.Name, actualSubScope.Name);
            Assert.AreEqual(expectedSubScope.Description, actualSubScope.Description);
            Assert.AreEqual(expectedSubScope.Interlocks.Count, actualSubScope.Interlocks.Count);
            Assert.AreEqual(expectedSubScope.ScopeBranches.Count, actualSubScope.ScopeBranches.Count);
            Assert.IsTrue(compareCosts(expectedSubScope.CostBatch, actualSubScope.CostBatch));
        }
        
        [TestMethod]
        public void SaveAs_Bid_Note()
        {
            //Arrange
            TECBid bid = new TECBid();
            TECLabeled expectedNote = new TECLabeled();
            expectedNote.Label = "test";
            bid.Notes.Add(expectedNote);

            path = Path.GetTempFileName();

            //Act
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            TECBid loadedBid = manager.Load() as TECBid;

            TECLabeled actualNote = null;
            foreach (TECLabeled note in loadedBid.Notes.Where(item => item.Guid == expectedNote.Guid))
            {
                actualNote = note;
            }

            //Assert
            Assert.AreEqual(expectedNote.Label, actualNote.Label);
        }

        [TestMethod]
        public void SaveAs_Bid_Device()
        {
            //Assert
            Assert.AreEqual(expectedDevice.Name, actualDevice.Name);
            Assert.AreEqual(expectedDevice.Description, actualDevice.Description);
            int actualQuantity = 0;
            foreach (IEndDevice device in actualDevices)
            {
                if (device.Guid == actualDevice.Guid)
                {
                    actualQuantity++;
                }
            }
            int expectedQuantity = 0;
            foreach (IEndDevice device in expectedSubScope.Devices)
            {
                if (device.Guid == expectedDevice.Guid)
                {
                    expectedQuantity++;
                }
            }
            Assert.AreEqual(expectedQuantity, actualQuantity);
            Assert.AreEqual(expectedDevice.Price, actualDevice.Price, DELTA);

            foreach (TECElectricalMaterial expectedConnectionType in expectedDevice.HardwiredConnectionTypes)
            {
                bool found = false;
                foreach (TECElectricalMaterial actualConnectionType in actualDevice.HardwiredConnectionTypes)
                {
                    if (actualConnectionType.Guid == expectedConnectionType.Guid)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Assert.Fail("ConnectionType not found on device.");
                }
            }

            Assert.AreEqual(actualManufacturer.Guid, actualDevice.Manufacturer.Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Manufacturer()
        {
            //Assert
            Assert.AreEqual(expectedManufacturer.Label, actualManufacturer.Label);
            Assert.AreEqual(expectedManufacturer.Multiplier, actualManufacturer.Multiplier, DELTA,
                "Expected: " + expectedManufacturer.Multiplier + " Actual: " + actualManufacturer.Multiplier);

            Assert.AreEqual(expectedDevice.Manufacturer.Label, expectedDevice.Manufacturer.Label);
            Assert.AreEqual(expectedDevice.Manufacturer.Multiplier, expectedDevice.Manufacturer.Multiplier, DELTA);
            Assert.AreEqual(expectedDevice.Manufacturer.Guid, expectedDevice.Manufacturer.Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Point()
        {
            //Assert
            Assert.AreEqual(expectedPoint.Label, actualPoint.Label);
            Assert.AreEqual(expectedPoint.Quantity, actualPoint.Quantity);
            Assert.AreEqual(expectedPoint.Type, actualPoint.Type);
        }

        [TestMethod]
        public void SaveAs_Bid_Location()
        {
            //Assert
            Assert.AreEqual(expectedBid.Locations.Count, actualBid.Locations.Count);
            Assert.AreEqual(expectedSystem.Location.Guid, actualSystem.Location.Guid);
            Assert.AreEqual(expectedSystem1.Location.Guid, actualSystem1.Location.Guid);
            Assert.AreEqual(expectedEquipment.Location.Guid, actualEquipment.Location.Guid);
            Assert.AreEqual(expectedSubScope.Location.Guid, actualSubScope.Location.Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_ScopeBranch()
        {
            //Assert
            Assert.AreEqual(expectedBid.ScopeTree.Count, actualBid.ScopeTree.Count);
            Assert.AreEqual(expectedBranch.Label, actualBranch.Label);
            Assert.AreEqual(expectedBranch.Guid, actualBranch.Guid);

            Assert.AreEqual(expectedBranch.Branches[0].Label, actualBranch.Branches[0].Label);
            Assert.AreEqual(expectedBranch.Branches[0].Guid, actualBranch.Branches[0].Guid);

            Assert.AreEqual(expectedBranch.Branches[0].Branches[0].Label, actualBranch.Branches[0].Branches[0].Label);
            Assert.AreEqual(expectedBranch.Branches[0].Branches[0].Guid, actualBranch.Branches[0].Branches[0].Guid);
        }
        
        [TestMethod]
        public void SaveAs_Bid_Exclusion()
        {
            //Assert
            Assert.AreEqual(expectedExclusion.Label, actualExclusion.Label);
        }

        [TestMethod]
        public void SaveAs_Bid_Tag()
        {
            //Assert
            Assert.AreEqual(expectedTag.Label, actualTag.Label);

            string expectedText = actualTag.Label;
            Guid expectedGuid = actualTag.Guid;

            Assert.AreEqual(expectedSystem.Tags[0].Guid, actualSystem.Tags[0].Guid);
            Assert.AreEqual(expectedSystem.Tags[0].Label, actualSystem.Tags[0].Label);

            Assert.AreEqual(expectedEquipment.Tags[0].Guid, actualEquipment.Tags[0].Guid);
            Assert.AreEqual(expectedEquipment.Tags[0].Label, actualEquipment.Tags[0].Label);

            Assert.AreEqual(expectedSubScope.Tags[0].Guid, actualSubScope.Tags[0].Guid);
            Assert.AreEqual(expectedSubScope.Tags[0].Label, actualSubScope.Tags[0].Label);

            Assert.AreEqual(expectedDevice.Tags[0].Guid, actualDevice.Tags[0].Guid);
            Assert.AreEqual(expectedDevice.Tags[0].Label, actualDevice.Tags[0].Label);
        }

        [TestMethod]
        public void SaveAs_Bid_Controller()
        {
            //Assert
            Assert.AreEqual(expectedController.Name, actualController.Name);
            Assert.AreEqual(expectedController.Description, actualController.Description);
            Assert.AreEqual(expectedController.Type.Guid, actualController.Type.Guid);

            foreach (TECIO expectedIO in expectedController.Type.IO)
            {
                bool ioExists = false;
                foreach (TECIO actualIO in actualController.Type.IO)
                {
                    if ((expectedIO.Type == actualIO.Type) && (expectedIO.Quantity == actualIO.Quantity))
                    {
                        ioExists = true;
                        break;
                    }
                }
                Assert.IsTrue(ioExists);
            }
            CostBatch expectedBatch = expectedController.CostBatch;
            CostBatch actualBatch = actualController.CostBatch;
            Assert.IsTrue(compareCosts(expectedBatch, actualBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_SubScopeConnection()
        {
            //Arrange
            TECController expectedConnectedController = null;
            TECHardwiredConnection expectedConnection = null;
            foreach (TECController controller in expectedBid.Controllers)
            {
                foreach (IControllerConnection connection in controller.ChildrenConnections)
                {
                    if (connection is TECHardwiredConnection)
                    {
                        expectedConnectedController = controller;
                        expectedConnection = connection as TECHardwiredConnection;
                        break;
                    }
                }
                if (expectedConnectedController != null)
                {
                    break;
                }
            }
            TECController actualConnectedController = actualBid.FindChild(expectedConnectedController.Guid) as TECController;
            TECHardwiredConnection actualConnection = actualConnectedController.FindChild(expectedConnection.Guid) as TECHardwiredConnection;

            //Assert
            Assert.AreEqual(expectedConnection.Guid, actualConnection.Guid);
            Assert.AreEqual(expectedConnection.ConduitType.Guid, actualConnection.ConduitType.Guid);
            Assert.AreEqual(expectedConnection.Length, actualConnection.Length, DELTA);
            Assert.AreEqual(expectedConnection.ParentController.Guid, actualConnection.ParentController.Guid);
            Assert.AreEqual(expectedConnection.Child.Guid, actualConnection.Child.Guid);
            Assert.IsTrue(compareCosts(expectedConnection.CostBatch, actualConnection.CostBatch));
            //Assert.IsFalse(actualConnection.IsTypical);

        }

        [TestMethod]
        public void SaveAs_Bid_MiscCost()
        {
            //Arrange
            TECMisc expectedCost = expectedBid.MiscCosts[0];
            TECMisc actualCost = null;
            foreach (TECMisc misc in actualBid.MiscCosts)
            {
                if (misc.Guid == expectedCost.Guid)
                {
                    actualCost = misc;
                }
            }

            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Cost, actualCost.Cost, DELTA);
            Assert.AreEqual(expectedCost.Quantity, actualCost.Quantity);
            Assert.IsTrue(compareCosts(expectedCost.CostBatch, actualCost.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_Panel()
        {
            //Arrange
            TECPanel expectedPanel = expectedBid.Panels[0];
            TECPanel actualPanel = null;
            foreach (TECPanel panel in actualBid.Panels)
            {
                if (panel.Guid == expectedPanel.Guid)
                {
                    actualPanel = panel;
                    break;
                }
            }

            Assert.AreEqual(expectedPanel.Name, actualPanel.Name);
            Assert.AreEqual(expectedPanel.Type.Guid, actualPanel.Type.Guid);
            Assert.IsTrue(compareCosts(expectedPanel.CostBatch, actualPanel.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_PanelType()
        {
            //Arrange
            TECPanelType expectedCost = expectedBid.Catalogs.PanelTypes[0];
            TECPanelType actualCost = null;
            foreach(TECPanelType type in actualBid.Catalogs.PanelTypes)
            {
                if(type.Guid == expectedCost.Guid)
                {
                    actualCost = type;
                    break;
                }
            }

            Assert.AreEqual(expectedCost.Guid, actualCost.Guid);
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Price, actualCost.Price, DELTA);
            Assert.AreEqual(expectedCost.Manufacturer.Guid, actualCost.Manufacturer.Guid);
            Assert.IsTrue(compareCosts(expectedCost.CostBatch, actualCost.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_ControllerType()
        {
            //Arrange
            TECControllerType expectedCost = expectedBid.Catalogs.ControllerTypes[0];
            TECControllerType actualCost = null;
            foreach (TECControllerType type in actualBid.Catalogs.ControllerTypes)
            {
                if (type.Guid == expectedCost.Guid)
                {
                    actualCost = type;
                    break;
                }
            }

            Assert.AreEqual(expectedCost.Guid, actualCost.Guid);
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Price, actualCost.Price, DELTA);
            Assert.AreEqual(expectedCost.Manufacturer.Guid, actualCost.Manufacturer.Guid);
            Assert.IsTrue(compareCosts(expectedCost.CostBatch, actualCost.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_IOMOdule()
        {
            //Arrange
            TECIOModule expectedCost = expectedBid.Catalogs.IOModules[0];
            TECIOModule actualCost = null;
            foreach (TECIOModule type in actualBid.Catalogs.IOModules)
            {
                if (type.Guid == expectedCost.Guid)
                {
                    actualCost = type;
                    break;
                }
            }

            Assert.AreEqual(expectedCost.Guid, actualCost.Guid);
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Price, actualCost.Price, DELTA);
            Assert.AreEqual(expectedCost.Manufacturer.Guid, actualCost.Manufacturer.Guid);
            Assert.IsTrue(compareCosts(expectedCost.CostBatch, actualCost.CostBatch));
        }

        [TestMethod]
        public void SaveAs_Bid_ControlledScope()
        {
            Assert.AreEqual(expectedSystem.Guid, actualSystem.Guid);
            Assert.AreEqual(expectedSystem.Equipment.Count, actualSystem.Equipment.Count);
            Assert.AreEqual(expectedSystem.Controllers.Count, actualSystem.Controllers.Count);
            Assert.AreEqual(expectedSystem.Panels.Count, actualSystem.Panels.Count);

            foreach (TECPanel panel in expectedSystem.Panels)
            {
                foreach (TECController controller in panel.Controllers)
                {
                    foreach (TECPanel obervedPanel in actualSystem.Panels)
                    {
                        if (obervedPanel.Guid == panel.Guid)
                        {
                            bool containsController = false;
                            foreach (TECController observedController in obervedPanel.Controllers)
                            {
                                if (observedController.Guid == controller.Guid)
                                {
                                    containsController = true;
                                }
                            }
                            Assert.IsTrue(containsController);
                        }
                    }
                }
            }
            Assert.AreEqual(expectedSystem.Panels.Count, actualSystem.Panels.Count);
            Assert.IsTrue(compareCosts(expectedSystem.CostBatch, actualSystem.CostBatch));

        }

        [TestMethod]
        public void SaveAs_Bid_SystemInstances()
        {
            TECBid saveBid = new TECBid();
            saveBid.Catalogs = ModelCreation.TestCatalogs(rand, 5);
            TECTypical system = ModelCreation.TestTypical(saveBid.Catalogs, rand);
            saveBid.Systems.Add(system);

            //Act
            path = Path.GetTempFileName();
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(saveBid);
            TECBid loadedBid = manager.Load() as TECBid;
            TECTypical loadedSystem = loadedBid.Systems[0];

            Assert.AreEqual(system.Instances.Count, loadedSystem.Instances.Count);
            foreach (TECSystem loadedInstance in loadedSystem.Instances)
            {
                foreach (TECSystem saveInstance in system.Instances)
                {
                    if (loadedInstance.Guid == saveInstance.Guid)
                    {
                        Assert.AreEqual(loadedInstance.Equipment.Count, saveInstance.Equipment.Count);
                        Assert.AreEqual(loadedInstance.Panels.Count, saveInstance.Panels.Count);
                        Assert.AreEqual(loadedInstance.Controllers.Count, saveInstance.Controllers.Count);
                        Assert.IsTrue(compareCosts(loadedInstance.CostBatch, saveInstance.CostBatch));
                    }
                }
            }
        }

        [TestMethod]
        public void SaveAs_Bid_Estimate()
        {
            TECBid saveBid = ModelCreation.TestBid(rand);
            var watcher = new ChangeWatcher(saveBid);
            TECEstimator estimate = new TECEstimator(saveBid, watcher);
            var expectedTotalCost = estimate.TotalCost;
            double delta = 0.0001;

            Dictionary<Guid, INotifyCostChanged> saveCostDictionary = new Dictionary<Guid, INotifyCostChanged>();
            addToCost(saveCostDictionary, saveBid, saveBid);

            //Act
            path = Path.GetTempFileName();
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(saveBid);
            TECBid loadedBid = manager.Load() as TECBid;
            var loadedWatcher = new ChangeWatcher(loadedBid);
            TECEstimator loadedEstimate = new TECEstimator(loadedBid, loadedWatcher);

            Dictionary<Guid, INotifyCostChanged> loadCostDictionary = new Dictionary<Guid, INotifyCostChanged>();
            addToCost(loadCostDictionary, loadedBid, loadedBid);
            
            compareCosts(saveCostDictionary, loadCostDictionary);
            compareEstimators(estimate, loadedEstimate);
            Assert.AreEqual(expectedTotalCost, loadedEstimate.TotalCost, delta);
            
        }

        private void compareCosts(Dictionary<Guid, INotifyCostChanged> saveCostDictionary, Dictionary<Guid, INotifyCostChanged> loadCostDictionary)
        {
            foreach(KeyValuePair<Guid, INotifyCostChanged> pair in saveCostDictionary.Reverse())
            {
                INotifyCostChanged saveObj = pair.Value;

                if (!loadCostDictionary.ContainsKey(pair.Key))
                {
                    Assert.Fail("Guid not found with cost: " + saveObj.Guid);
                }
                else
                {
                    INotifyCostChanged loadObj = loadCostDictionary[pair.Key];
                    Assert.IsTrue(compareCosts(saveObj.CostBatch, loadObj.CostBatch), "Loaded value not correct: " + loadObj);
                }
            }
        }

        public void addToCost(Dictionary<Guid, INotifyCostChanged> costDictionary, ITECObject item, TECBid referenceBid)
        {
            if(item is TECCatalogs)
            {
                return;
            }
            if(item is INotifyCostChanged costItem)
            {
                if (referenceBid.FindChild(item.Guid) == null)
                {
                    var errant = item;
                    throw new Exception();
                }
                costDictionary[item.Guid] = costItem;
            }
            if(item is IRelatable saveable)
            {
                foreach(ITECObject child in saveable.GetDirectChildren())
                {
                    addToCost(costDictionary, child, referenceBid);
                }
            }
        } 

        public bool compareCosts(CostBatch expected, CostBatch actual)
        {
            if (Math.Abs(expected.GetCost(CostType.Electrical) - actual.GetCost(CostType.Electrical)) < DELTA &&
                Math.Abs(expected.GetCost(CostType.TEC) - actual.GetCost(CostType.TEC)) < DELTA &&
                Math.Abs(expected.GetLabor(CostType.Electrical) - actual.GetLabor(CostType.Electrical)) < DELTA &&
                Math.Abs(expected.GetLabor(CostType.TEC) - actual.GetLabor(CostType.TEC)) < DELTA)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void compareEstimators(TECEstimator expected, TECEstimator actual)
        {
            Assert.AreEqual(expected.TotalPointNumber, actual.TotalPointNumber);
            Assert.AreEqual(expected.TECCost, actual.TECCost, DELTA);
            Assert.AreEqual(expected.TECLaborHours, actual.TECLaborHours, DELTA);
            Assert.AreEqual(expected.SubcontractorCost, actual.SubcontractorCost, DELTA);
            Assert.AreEqual(expected.SubcontractorLaborHours, actual.SubcontractorLaborHours, DELTA);
        }
    }
}
