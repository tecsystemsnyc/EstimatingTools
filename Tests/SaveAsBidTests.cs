﻿using EstimatingLibrary;
using EstimatingUtilitiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SaveAsBidTests
    {
        private const bool DEBUG = false;


        static TECBid expectedBid;
        static TECLabor expectedLabor;
        static TECSystem expectedSystem;
        static TECSystem expectedSystem1;
        static TECEquipment expectedEquipment;
        static TECSubScope expectedSubScope;
        static TECDevice expectedDevice;
        static TECManufacturer expectedManufacturer;
        static TECPoint expectedPoint;
        static TECScopeBranch expectedBranch;
        static TECNote expectedNote;
        static TECExclusion expectedExclusion;
        static TECTag expectedTag;
        static TECDrawing expectedDrawing;
        static TECPage expectedPage;
        static TECVisualScope expectedVisualScope;
        static TECController expectedController;
        static TECProposalScope expectedPropScope;

        static string path;

        static TECBid actualBid;
        static TECLabor actualLabor;
        static TECSystem actualSystem;
        static TECSystem actualSystem1;
        static TECEquipment actualEquipment;
        static TECSubScope actualSubScope;
        static TECDevice actualDevice;
        static ObservableCollection<TECDevice> actualDevices;
        static TECManufacturer actualManufacturer;
        static TECPoint actualPoint;
        static TECScopeBranch actualBranch;
        static TECNote actualNote;
        static TECExclusion actualExclusion;
        static TECTag actualTag;
        static TECDrawing actualDrawing;
        static TECPage actualPage;
        static TECVisualScope actualVisualScope;
        static TECController actualController;
        static TECProposalScope actualPropScope;

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
            //Arrange
            expectedBid = TestHelper.CreateTestBid();
            expectedLabor = expectedBid.Labor;
            expectedSystem = expectedBid.Systems[0];
            expectedSystem1 = expectedBid.Systems[1];
            expectedEquipment = expectedSystem.Equipment[0];
            expectedSubScope = expectedEquipment.SubScope[0];
            expectedDevice = expectedSubScope.Devices[0];

            expectedManufacturer = expectedBid.ManufacturerCatalog[0];
            expectedPoint = expectedSubScope.Points[0];

            expectedBranch = null;
            foreach (TECScopeBranch branch in expectedBid.ScopeTree)
            {
                if (branch.Name == "Branch 1")
                {
                    expectedBranch = branch;
                    break;
                }
            }

            expectedNote = expectedBid.Notes[0];
            expectedExclusion = expectedBid.Exclusions[0];
            expectedTag = expectedBid.Tags[0];

            //expectedDrawing = expectedBid.Drawings[0];
            //expectedPage = expectedDrawing.Pages[0];
            //expectedVisualScope = expectedPage.PageScope[0];

            expectedController = expectedBid.Controllers[0];

            expectedPropScope = null;
            foreach (TECProposalScope propScope in expectedBid.ProposalScope)
            {
                if (propScope.Scope.Name == "Prop System")
                {
                    expectedPropScope = propScope;
                    break;
                }
            }

            path = Path.GetTempFileName();

            //Act
            EstimatingLibraryDatabase.SaveBidToNewDB(path, expectedBid);
            actualBid = EstimatingLibraryDatabase.LoadDBToBid(path, new TECTemplates());
            actualLabor = actualBid.Labor;

            foreach (TECSystem sys in actualBid.Systems)
            {
                if (sys.Guid == expectedSystem.Guid)
                {
                    actualSystem = sys;
                }
                else if (sys.Guid == expectedSystem1.Guid)
                {
                    actualSystem1 = sys;
                }
                if (actualSystem != null && actualSystem1 != null)
                {
                    break;
                }
            }

            foreach (TECEquipment equip in actualSystem.Equipment)
            {
                if (equip.Guid == expectedEquipment.Guid)
                {
                    actualEquipment = equip;
                    break;
                }
            }

            foreach (TECSubScope ss in actualEquipment.SubScope)
            {
                if (ss.Guid == expectedSubScope.Guid)
                {
                    actualSubScope = ss;
                    break;
                }
            }
            actualDevices = actualSubScope.Devices;
            foreach (TECDevice dev in actualSubScope.Devices)
            {
                if (dev.Guid == expectedDevice.Guid)
                {
                    actualDevice = dev;
                    break;
                }
            }

            foreach (TECManufacturer man in actualBid.ManufacturerCatalog)
            {
                if (man.Guid == expectedManufacturer.Guid)
                {
                    actualManufacturer = man;
                    break;
                }
            }

            foreach (TECPoint point in actualSubScope.Points)
            {
                if (point.Guid == expectedPoint.Guid)
                {
                    actualPoint = point;
                    break;
                }
            }

            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == expectedBranch.Guid)
                {
                    actualBranch = branch;
                    break;
                }
            }

            foreach (TECNote note in actualBid.Notes)
            {
                if (note.Guid == expectedNote.Guid)
                {
                    actualNote = note;
                    break;
                }
            }

            foreach (TECExclusion exclusion in actualBid.Exclusions)
            {
                if (exclusion.Guid == expectedExclusion.Guid)
                {
                    actualExclusion = exclusion;
                    break;
                }
            }

            foreach (TECTag tag in actualBid.Tags)
            {
                if (tag.Guid == expectedTag.Guid)
                {
                    actualTag = tag;
                    break;
                }
            }

            //foreach (TECDrawing drawing in actualBid.Drawings)
            //{
            //    if (drawing.Guid == expectedDrawing.Guid)
            //    {
            //        actualDrawing = drawing;
            //        break;
            //    }
            //}

            //foreach (TECPage page in actualDrawing.Pages)
            //{
            //    if (page.Guid == expectedPage.Guid)
            //    {
            //        actualPage = page;
            //        break;
            //    }
            //}

            //foreach (TECVisualScope vs in actualPage.PageScope)
            //{
            //    if (vs.Guid == expectedVisualScope.Guid)
            //    {
            //        actualVisualScope = vs;
            //        break;
            //    }
            //}
            
            foreach (TECController con in actualBid.Controllers)
            {
                if (con.Guid == expectedController.Guid)
                {
                    actualController = con;
                    break;
                }
            }

            foreach (TECProposalScope propScope in actualBid.ProposalScope)
            {
                if (propScope.Scope.Guid == expectedPropScope.Scope.Guid)
                {
                    actualPropScope = propScope;
                    break;
                }
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (DEBUG)
            {
                Console.WriteLine("SaveAs test bid saved to: " + path);
            }
            else
            {
                File.Delete(path);
            }


        }

        [TestMethod]
        public void SaveAs_Bid_Info()
        {
            //Assert
            Assert.AreEqual(expectedBid.Name, actualBid.Name);
            Assert.AreEqual(expectedBid.BidNumber, actualBid.BidNumber);
            Assert.AreEqual(expectedBid.DueDate, actualBid.DueDate);
            Assert.AreEqual(expectedBid.Salesperson, actualBid.Salesperson);
            Assert.AreEqual(expectedBid.Estimator, actualBid.Estimator);
        }

        [TestMethod]
        public void SaveAs_Bid_LaborConstants()
        {
            //Assert
            Assert.AreEqual(expectedLabor.PMCoef, actualLabor.PMCoef);
            Assert.AreEqual(expectedLabor.PMRate, actualLabor.PMRate);

            Assert.AreEqual(expectedLabor.ENGCoef, actualLabor.ENGCoef);
            Assert.AreEqual(expectedLabor.ENGRate, actualLabor.ENGRate);

            Assert.AreEqual(expectedLabor.CommCoef, actualLabor.CommCoef);
            Assert.AreEqual(expectedLabor.CommRate, actualLabor.CommRate);

            Assert.AreEqual(expectedLabor.SoftCoef, actualLabor.SoftCoef);
            Assert.AreEqual(expectedLabor.SoftRate, actualLabor.SoftRate);

            Assert.AreEqual(expectedLabor.GraphCoef, actualLabor.GraphCoef);
            Assert.AreEqual(expectedLabor.GraphRate, actualLabor.GraphRate);
        }

        [TestMethod]
        public void SaveAs_Bid_SubContracterConstants()
        {
            //Assert
            Assert.AreEqual(expectedLabor.ElectricalRate, actualLabor.ElectricalRate);
            Assert.AreEqual(expectedLabor.ElectricalSuperRate, actualLabor.ElectricalSuperRate);
        }

        [TestMethod]
        public void SaveAs_Bid_UserAdjustments()
        {
            //Assert
            Assert.AreEqual(expectedLabor.PMExtraHours, actualLabor.PMExtraHours);
            Assert.AreEqual(expectedLabor.ENGExtraHours, actualLabor.ENGExtraHours);
            Assert.AreEqual(expectedLabor.CommExtraHours, actualLabor.CommExtraHours);
            Assert.AreEqual(expectedLabor.SoftExtraHours, actualLabor.SoftExtraHours);
            Assert.AreEqual(expectedLabor.GraphExtraHours, actualLabor.GraphExtraHours);
        }

        [TestMethod]
        public void SaveAs_Bid_System()
        {
            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
            Assert.AreEqual(expectedSystem.Quantity, actualSystem.Quantity);
            Assert.AreEqual(expectedSystem.BudgetPrice, actualSystem.BudgetPrice);
        }

        [TestMethod]
        public void SaveAs_Bid_Equipment()
        {
            //Assert
            Assert.AreEqual(expectedEquipment.Name, actualEquipment.Name);
            Assert.AreEqual(expectedEquipment.Description, actualEquipment.Description);
            Assert.AreEqual(expectedEquipment.Quantity, actualEquipment.Quantity);
            Assert.AreEqual(expectedEquipment.BudgetPrice, actualEquipment.BudgetPrice);
        }

        [TestMethod]
        public void SaveAs_Bid_SubScope()
        {
            //Assert
            Assert.AreEqual(expectedSubScope.Name, actualSubScope.Name);
            Assert.AreEqual(expectedSubScope.Description, actualSubScope.Description);
            Assert.AreEqual(expectedSubScope.Quantity, actualSubScope.Quantity);
        }

        [TestMethod]
        public void SaveAs_Bid_Device()
        {
            //Assert
            Assert.AreEqual(expectedDevice.Name, actualDevice.Name);
            Assert.AreEqual(expectedDevice.Description, actualDevice.Description);
            int actualQuantity = 0;
            foreach(TECDevice device in actualDevices)
            {
                if(device.Guid == actualDevice.Guid)
                {
                    actualQuantity++;
                }
            }
            int expectedQuantity = 0;
            foreach (TECDevice device in expectedSubScope.Devices)
            {
                if (device.Guid == expectedDevice.Guid)
                {
                    expectedQuantity++;
                }
            }
            Assert.AreEqual(expectedQuantity, actualQuantity);
            Assert.AreEqual(expectedDevice.Cost, actualDevice.Cost);
            Assert.AreEqual(expectedDevice.ConnectionType.Guid, actualDevice.ConnectionType.Guid);

            Assert.AreEqual(actualManufacturer.Guid, actualDevice.Manufacturer.Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Manufacturer()
        {
            //Assert
            Assert.AreEqual(expectedManufacturer.Name, actualManufacturer.Name);
            Assert.AreEqual(expectedManufacturer.Multiplier, actualManufacturer.Multiplier);

            Assert.AreEqual(expectedDevice.Manufacturer.Name, expectedDevice.Manufacturer.Name);
            Assert.AreEqual(expectedDevice.Manufacturer.Multiplier, expectedDevice.Manufacturer.Multiplier);
            Assert.AreEqual(expectedDevice.Manufacturer.Guid, expectedDevice.Manufacturer.Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Point()
        {
            //Assert
            Assert.AreEqual(expectedPoint.Name, actualPoint.Name);
            Assert.AreEqual(expectedPoint.Description, actualPoint.Description);
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

            //In CreateTestBid, the first system has the same location as its equipment.
            Assert.AreEqual(expectedSystem.Location, expectedEquipment.Location);
        }

        [TestMethod]
        public void SaveAs_Bid_ScopeBranch()
        {
            //Assert
            Assert.AreEqual(expectedBranch.Name, actualBranch.Name);
            Assert.AreEqual(expectedBranch.Description, actualBranch.Description);
            Assert.AreEqual(expectedBranch.Guid, actualBranch.Guid);

            Assert.AreEqual(expectedBranch.Branches[0].Name, actualBranch.Branches[0].Name);
            Assert.AreEqual(expectedBranch.Branches[0].Description, actualBranch.Branches[0].Description);
            Assert.AreEqual(expectedBranch.Branches[0].Guid, actualBranch.Branches[0].Guid);

            Assert.AreEqual(expectedBranch.Branches[0].Branches[0].Name, actualBranch.Branches[0].Branches[0].Name);
            Assert.AreEqual(expectedBranch.Branches[0].Branches[0].Description, actualBranch.Branches[0].Branches[0].Description);
            Assert.AreEqual(expectedBranch.Branches[0].Branches[0].Guid, actualBranch.Branches[0].Branches[0].Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Note()
        {
            //Assert
            Assert.AreEqual(expectedNote.Text, actualNote.Text);
        }

        [TestMethod]
        public void SaveAs_Bid_Exclusion()
        {
            //Assert
            Assert.AreEqual(expectedExclusion.Text, actualExclusion.Text);
        }

        [TestMethod]
        public void SaveAs_Bid_Tag()
        {
            //Assert
            Assert.AreEqual(expectedTag.Text, actualTag.Text);

            string expectedText = actualTag.Text;
            Guid expectedGuid = actualTag.Guid;

            Assert.AreEqual(expectedGuid, actualSystem.Tags[0].Guid);
            Assert.AreEqual(expectedText, actualSystem.Tags[0].Text);

            Assert.AreEqual(expectedGuid, actualEquipment.Tags[0].Guid);
            Assert.AreEqual(expectedText, actualEquipment.Tags[0].Text);

            Assert.AreEqual(expectedGuid, actualSubScope.Tags[0].Guid);
            Assert.AreEqual(expectedText, actualSubScope.Tags[0].Text);

            Assert.AreEqual(expectedGuid, actualDevice.Tags[0].Guid);
            Assert.AreEqual(expectedText, actualDevice.Tags[0].Text);

            Assert.AreEqual(expectedGuid, actualPoint.Tags[0].Guid);
            Assert.AreEqual(expectedText, actualPoint.Tags[0].Text);
        }

        //[TestMethod]
        //public void SaveAs_Bid_Drawing()
        //{
        //    //Assert
        //    Assert.AreEqual(expectedDrawing.Name, actualDrawing.Name);
        //    Assert.AreEqual(expectedDrawing.Description, actualDrawing.Description);
        //}

        //[TestMethod]
        //public void SaveAs_Bid_Page()
        //{
        //    //Assert
        //    Assert.AreEqual(expectedPage.PageNum, actualPage.PageNum);
        //}

        //[TestMethod]
        //public void SaveAs_Bid_VisScope()
        //{
        //    //Assert
        //    Assert.AreEqual(expectedVisualScope.X, actualVisualScope.X);
        //    Assert.AreEqual(expectedVisualScope.Y, actualVisualScope.Y);

        //    Assert.AreEqual(expectedVisualScope.Scope.Guid, actualVisualScope.Scope.Guid);
        //}

        [TestMethod]
        public void SaveAs_Bid_PropScope()
        {
            //Assert
            Assert.AreEqual(expectedPropScope.IsProposed, actualPropScope.IsProposed);
            Assert.AreEqual(expectedPropScope.Notes[0].Guid, actualPropScope.Notes[0].Guid);
            Assert.AreEqual(expectedPropScope.Notes[0].Branches[0].Guid, actualPropScope.Notes[0].Branches[0].Guid);
        }

        [TestMethod]
        public void SaveAs_Bid_Controller()
        {
            //Assert
            Assert.AreEqual(expectedController.Name, actualController.Name);
            Assert.AreEqual(expectedController.Description, actualController.Description);
            Assert.AreEqual(expectedController.Cost, actualController.Cost);

            foreach (TECIO expectedIO in expectedController.IO)
            {
                bool ioExists = false;
                foreach (TECIO actualIO in actualController.IO)
                {
                    if ((expectedIO.Type == actualIO.Type) && (expectedIO.Quantity == actualIO.Quantity))
                    {
                        ioExists = true;
                        break;
                    }
                }
                Assert.IsTrue(ioExists);
            }
        }
    }
}
