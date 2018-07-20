using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using EstimatingUtilitiesLibrary.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TestLibrary.ModelTestingUtilities;
using static TestLibrary.GeneralTestingUtilities;

namespace EstimatingUtilitiesLibraryTests
{
    [TestClass]
    public class SaveBidTests
    {
        string path;
        Random rand;            

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(path);
        }

        public DeltaStacker SaveBid(TECBid bid)
        {
            path = Path.GetTempFileName();
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            manager.New(bid);
            ChangeWatcher watcher = new ChangeWatcher(bid);
            return new DeltaStacker(watcher, bid);
        }

        #region Save BidInfo
        [TestMethod]
        public void Save_BidInfo_Name()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            string expectedName = "Save Name";
            bid.Name = expectedName;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            string actualName = actualBid.Name;

            //Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void Save_BidInfo_BidNo()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            string expectedBidNo = "Save BidNo";
            bid.BidNumber = expectedBidNo;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            string actualBidNo = actualBid.BidNumber;

            //Assert
            Assert.AreEqual(expectedBidNo, actualBidNo);
        }

        [TestMethod]
        public void Save_BidInfo_DueDate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            DateTime expectedDueDate = DateTime.Now;
            bid.DueDate = expectedDueDate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            DateTime actualDueDate = actualBid.DueDate;

            //Assert
            Assert.AreEqual(expectedDueDate, actualDueDate);
        }

        [TestMethod]
        public void Save_BidInfo_Salesperson()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            string expectedSalesperson = "Save Salesperson";
            bid.Salesperson = expectedSalesperson;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            string actualSalesperson = actualBid.Salesperson;

            //Assert
            Assert.AreEqual(expectedSalesperson, actualSalesperson);
        }

        [TestMethod]
        public void Save_BidInfo_Estimator()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            string expectedEstimator = "Save Estimator";
            bid.Estimator = expectedEstimator;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            string actualEstimator = actualBid.Estimator;

            //Assert
            Assert.AreEqual(expectedEstimator, actualEstimator);
        }
        #endregion Save BidInfo

        #region Save Labor
        [TestMethod]
        public void Save_Bid_Labor_PMCoef()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedPM = 0.123;
            bid.Parameters.PMCoef = expectedPM;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualPM = actualBid.Parameters.PMCoef;

            //Assert
            Assert.AreEqual(expectedPM, actualPM);
        }

        [TestMethod]
        public void Save_Bid_Labor_PMRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 564.05;
            bid.Parameters.PMRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.PMRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_PMExtraHours()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedHours = 457.69;
            bid.ExtraLabor.PMExtraHours = expectedHours;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualHours = actualBid.ExtraLabor.PMExtraHours;

            //Assert
            Assert.AreEqual(expectedHours, actualHours);
        }

        [TestMethod]
        public void Save_Bid_Labor_ENGCoef()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedENG = 0.123;
            bid.Parameters.ENGCoef = expectedENG;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualENG = actualBid.Parameters.ENGCoef;

            //Assert
            Assert.AreEqual(expectedENG, actualENG);
        }

        [TestMethod]
        public void Save_Bid_Labor_ENGRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 564.05;
            bid.Parameters.ENGRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.ENGRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_ENGExtraHours()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedHours = 457.69;
            bid.ExtraLabor.ENGExtraHours = expectedHours;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualHours = actualBid.ExtraLabor.ENGExtraHours;

            //Assert
            Assert.AreEqual(expectedHours, actualHours);
        }

        [TestMethod]
        public void Save_Bid_Labor_CommCoef()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedComm = 0.123;
            bid.Parameters.CommCoef = expectedComm;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualComm = actualBid.Parameters.CommCoef;

            //Assert
            Assert.AreEqual(expectedComm, actualComm);
        }

        [TestMethod]
        public void Save_Bid_Labor_CommRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 564.05;
            bid.Parameters.CommRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.CommRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_CommExtraHours()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedHours = 457.69;
            bid.ExtraLabor.CommExtraHours = expectedHours;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualHours = actualBid.ExtraLabor.CommExtraHours;

            //Assert
            Assert.AreEqual(expectedHours, actualHours);
        }

        [TestMethod]
        public void Save_Bid_Labor_SoftCoef()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedSoft = 0.123;
            bid.Parameters.SoftCoef = expectedSoft;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualSoft = actualBid.Parameters.SoftCoef;

            //Assert
            Assert.AreEqual(expectedSoft, actualSoft);
        }

        [TestMethod]
        public void Save_Bid_Labor_SoftRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 564.05;
            bid.Parameters.SoftRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.SoftRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_SoftExtraHours()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedHours = 457.69;
            bid.ExtraLabor.SoftExtraHours = expectedHours;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualHours = actualBid.ExtraLabor.SoftExtraHours;

            //Assert
            Assert.AreEqual(expectedHours, actualHours);
        }

        [TestMethod]
        public void Save_Bid_Labor_GraphCoef()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedGraph = 0.123;
            bid.Parameters.GraphCoef = expectedGraph;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualGraph = actualBid.Parameters.GraphCoef;

            //Assert
            Assert.AreEqual(expectedGraph, actualGraph);
        }

        [TestMethod]
        public void Save_Bid_Labor_GraphRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 564.05;
            bid.Parameters.GraphRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.GraphRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_GraphExtraHours()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedHours = 457.69;
            bid.ExtraLabor.GraphExtraHours = expectedHours;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualHours = actualBid.ExtraLabor.GraphExtraHours;

            //Assert
            Assert.AreEqual(expectedHours, actualHours);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 0.123;
            bid.Parameters.ElectricalRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.ElectricalRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecNonUnionRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 0.456;
            bid.Parameters.ElectricalNonUnionRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.ElectricalNonUnionRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecSuperRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 0.123;
            bid.Parameters.ElectricalSuperRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.ElectricalSuperRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecSuperNonUnionRate()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            double expectedRate = 23.94;
            bid.Parameters.ElectricalSuperNonUnionRate = expectedRate;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            double actualRate = actualBid.Parameters.ElectricalSuperNonUnionRate;

            //Assert
            Assert.AreEqual(expectedRate, actualRate);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecIsOnOT()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            bid.Parameters.ElectricalIsOnOvertime = !bid.Parameters.ElectricalIsOnOvertime;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            Assert.AreEqual(bid.Parameters.ElectricalIsOnOvertime, actualBid.Parameters.ElectricalIsOnOvertime);
        }

        [TestMethod]
        public void Save_Bid_Labor_ElecIsUnion()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            bid.Parameters.ElectricalIsUnion = !bid.Parameters.ElectricalIsUnion;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            Assert.AreEqual(bid.Parameters.ElectricalIsUnion, actualBid.Parameters.ElectricalIsUnion);
        }

        #endregion Save Labor

        #region Save System
        [TestMethod]
        public void Save_Bid_Add_System()
        {
            //Act
            TECBid bid = new TECBid();
            DeltaStacker testStack = SaveBid(bid);

            TECTypical expectedSystem = new TECTypical();
            expectedSystem.Name = "New system";
            expectedSystem.Description = "New system desc";

            bid.Systems.Add(expectedSystem);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                if (expectedSystem.Guid == system.Guid)
                {
                    actualSystem = system;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
            Assert.AreEqual(expectedSystem.ProposeEquipment, actualSystem.ProposeEquipment);
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
        }

        [TestMethod]
        public void Save_Bid_Add_System_Instance()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECTypical typical = bid.Systems[0];

            TECSystem expectedSystem = typical.AddInstance(bid);
           
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            foreach (TECTypical system in actualBid.Systems)
            {
                foreach(TECSystem instance in system.Instances)
                {
                    if (expectedSystem.Guid == instance.Guid)
                    {
                        actualSystem = instance;
                        break;
                    }
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
            Assert.AreEqual(expectedSystem.ProposeEquipment, actualSystem.ProposeEquipment);
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
        }

        [TestMethod]
        public void Save_Bid_Add_System_Instance_Edit()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECTypical typical = bid.Systems[0];

            typical.Equipment.Add(ModelCreation.TestEquipment(bid.Catalogs, rand));
            TECSystem expectedSystem = typical.AddInstance(bid);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            foreach (TECTypical system in actualBid.Systems)
            {
                foreach (TECSystem instance in system.Instances)
                {
                    if (expectedSystem.Guid == instance.Guid)
                    {
                        actualSystem = instance;
                        break;
                    }
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
            Assert.AreEqual(expectedSystem.ProposeEquipment, actualSystem.ProposeEquipment);
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
        }

        [TestMethod]
        public void Save_Bid_Remove_System()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            int oldNumSystems = bid.Systems.Count;
            TECTypical systemToRemove = bid.Systems[0];

            bid.Systems.Remove(systemToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            //Assert
            foreach (TECSystem system in finalBid.Systems)
            {
                if (system.Guid == systemToRemove.Guid)
                {
                    Assert.Fail();
                }
            }

            Assert.AreEqual((oldNumSystems - 1), bid.Systems.Count);
        }

        #region Edit System
        [TestMethod]
        public void Save_Bid_System_Name()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSystem expectedSystem = bid.Systems[0];
            expectedSystem.Name = "Save System Name";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                if (system.Guid == expectedSystem.Guid)
                {
                    actualSystem = system;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Name, actualSystem.Name);
        }

        [TestMethod]
        public void Save_Bid_System_Description()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSystem expectedSystem = bid.Systems[0];
            expectedSystem.Description = "Save System Description";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                if (system.Guid == expectedSystem.Guid)
                {
                    actualSystem = system;
                }
            }

            //Assert
            Assert.AreEqual(expectedSystem.Description, actualSystem.Description);
        }
        
        [TestMethod]
        public void Save_Bid_System_Misc()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSystem expectedSystem = bid.Systems[0];
            var expectedMisc = new TECMisc(CostType.Electrical);
            expectedSystem.MiscCosts.Add(expectedMisc);
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSystem actualSystem = null;
            TECMisc actualMisc = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                if (system.Guid == expectedSystem.Guid)
                {
                    actualSystem = system;
                    foreach(TECMisc misc in actualSystem.MiscCosts)
                    {
                        if(misc.Guid == expectedMisc.Guid)
                        {
                            actualMisc = misc;
                            break;
                        }
                    }
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedMisc.Guid, actualMisc.Guid);
        }
        #endregion Edit System
        #endregion Save System

        #region Save Equipment
        [TestMethod]
        public void Save_Bid_Add_Equipment()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECEquipment expectedEquipment = new TECEquipment();
            expectedEquipment.Name = "New Equipment";
            expectedEquipment.Description = "New Description";
            Guid systemGuid = bid.Systems[0].Guid;
            bid.Systems[0].Equipment.Add(expectedEquipment);
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;
            TECSystem actualSystem = null;
            foreach(TECSystem system in actualBid.Systems)
            {
                if(system.Guid == systemGuid)
                {
                    actualSystem = system;
                    break;
                }
            }
            TECEquipment actualEquipment = null;
            foreach (TECEquipment equip in actualSystem.Equipment)
            {
                if (expectedEquipment.Guid == equip.Guid)
                {
                    actualEquipment = equip;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedEquipment.Name, actualEquipment.Name);
            Assert.AreEqual(expectedEquipment.Description, actualEquipment.Description);
        }

        [TestMethod]
        public void Save_Bid_Remove_Equipment()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSystem systemToModify = null; 
            foreach(TECSystem system in bid.Systems)
            {
                if(system.Equipment.Count > 0)
                {
                    systemToModify = system;
                }
            }
            int oldNumEquip = systemToModify.Equipment.Count();
            TECEquipment equipToRemove = systemToModify.Equipment[0];

            systemToModify.Equipment.Remove(equipToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            TECSystem modifiedSystem = null;
            foreach (TECSystem system in bid.Systems)
            {
                if (system.Guid == systemToModify.Guid)
                {
                    modifiedSystem = system;
                    break;
                }
            }

            //Assert
            foreach (TECEquipment equip in modifiedSystem.Equipment)
            {
                if (equipToRemove.Guid == equip.Guid)
                {
                    Assert.Fail();
                }
            }

            Assert.AreEqual((oldNumEquip - 1), modifiedSystem.Equipment.Count);
        }

        #region Edit Equipment
        [TestMethod]
        public void Save_Bid_Equipment_Name()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECEquipment expectedEquip = bid.Systems[0].Equipment[0];
            expectedEquip.Name = "Save Equip Name";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECEquipment actualEquip = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECEquipment equip in system.Equipment)
                {
                    if (equip.Guid == expectedEquip.Guid)
                    {
                        actualEquip = equip;
                        break;
                    }
                }

            }

            //Assert
            Assert.AreEqual(expectedEquip.Name, actualEquip.Name);
        }

        [TestMethod]
        public void Save_Bid_Equipment_Description()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECEquipment expectedEquip = bid.Systems[0].Equipment[0];
            expectedEquip.Description = "Save Equip Description";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECEquipment actualEquip = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECEquipment equip in system.Equipment)
                {
                    if (equip.Guid == expectedEquip.Guid)
                    {
                        actualEquip = equip;
                        break;
                    }
                }

            }

            //Assert
            Assert.AreEqual(expectedEquip.Description, actualEquip.Description);
        }
        
        #endregion Edit Equipment

        #endregion Save Equipment

        #region Save SubScope
        [TestMethod]
        public void Save_Bid_Add_SubScope()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope expectedSubScope = new TECSubScope();
            expectedSubScope.Name = "New SubScope";
            expectedSubScope.Description = "New Description";

            bid.Systems[0].Equipment[0].SubScope.Add(expectedSubScope);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope actualSubScope = null;
            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECEquipment equip in system.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == expectedSubScope.Guid)
                        {
                            actualSubScope = ss;
                            break;
                        }
                    }
                    if (actualSubScope != null) break;
                }
                if (actualSubScope != null) break;
            }

            //Assert
            Assert.AreEqual(expectedSubScope.Name, actualSubScope.Name);
            Assert.AreEqual(expectedSubScope.Description, actualSubScope.Description);
        }

        [TestMethod]
        public void Save_Bid_Remove_SubScope()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECEquipment equipToModify = bid.Systems[0].Equipment[0];
            int oldNumSubScope = equipToModify.SubScope.Count();
            TECSubScope subScopeToRemove = equipToModify.SubScope[0];

            equipToModify.SubScope.Remove(subScopeToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECEquipment modifiedEquip = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    if (equip.Guid == equipToModify.Guid)
                    {
                        modifiedEquip = equip;
                        break;
                    }
                }
                if (modifiedEquip != null) break;
            }

            //Assert
            foreach (TECSubScope ss in modifiedEquip.SubScope)
            {
                if (subScopeToRemove.Guid == ss.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumSubScope - 1), modifiedEquip.SubScope.Count);
        }

        #region Edit SubScope
        [TestMethod]
        public void Save_Bid_SubScope_Name()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope expectedSubScope = bid.Systems[0].Equipment[0].SubScope[0];
            expectedSubScope.Name = "Save SubScope Name";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope actualSubScope = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == expectedSubScope.Guid)
                        {
                            actualSubScope = ss;
                            break;
                        }
                    }
                    if (actualSubScope != null) break;
                }
                if (actualSubScope != null) break;
            }

            //Assert
            Assert.AreEqual(expectedSubScope.Name, actualSubScope.Name);
        }

        [TestMethod]
        public void Save_Bid_SubScope_Description()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope expectedSubScope = bid.Systems[0].Equipment[0].SubScope[0];
            expectedSubScope.Description = "Save SubScope Description";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope actualSubScope = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == expectedSubScope.Guid)
                        {
                            actualSubScope = ss;
                            break;
                        }
                    }
                    if (actualSubScope != null) break;
                }
                if (actualSubScope != null) break;
            }

            //Assert
            Assert.AreEqual(expectedSubScope.Description, actualSubScope.Description);
        }
        
        #endregion Edit SubScope
        #endregion Save SubScope

        #region Save Device

        [TestMethod]
        public void Save_Bid_Add_Device()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECDevice expectedDevice = bid.Catalogs.Devices[0];
            
            TECSubScope subScopeToModify = bid.Systems[0].Equipment[0].SubScope[0];

            //Makes a copy, as devices can only be added via drag drop.
            subScopeToModify.Devices.ObservablyClear();
            int expectedQuantity = 5;
            subScopeToModify.Devices.Add(expectedDevice);
            subScopeToModify.Devices.Add(expectedDevice);
            subScopeToModify.Devices.Add(expectedDevice);
            subScopeToModify.Devices.Add(expectedDevice);
            subScopeToModify.Devices.Add(expectedDevice);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            IEndDevice actualDevice = null;
            int actualQuantity = 0;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == subScopeToModify.Guid)
                        {
                            foreach (IEndDevice dev in ss.Devices)
                            {
                                if (dev.Guid == expectedDevice.Guid)
                                { actualQuantity++; }
                            }
                            foreach (IEndDevice dev in ss.Devices)
                            {
                                if (dev.Guid == expectedDevice.Guid)
                                {
                                    actualDevice = dev;
                                    break;
                                }
                            }
                        }
                        if (actualDevice != null) break;
                    }
                    if (actualDevice != null) break;
                }
                if (actualDevice != null) break;
            }

            //Assert
            Assert.AreEqual(expectedDevice.Name, actualDevice.Name);
            Assert.AreEqual(expectedDevice.Description, actualDevice.Description);
            Assert.AreEqual(expectedQuantity, actualQuantity);
            Assert.AreEqual(expectedDevice.Price, (actualDevice as TECHardware).Price, DELTA);
            Assert.AreEqual(expectedDevice.HardwiredConnectionTypes.Count, actualDevice.HardwiredConnectionTypes.Count);
        }

        [TestMethod]
        public void Save_Bid_Remove_Device()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope ssToModify = null;
            foreach (TECSystem sys in bid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Devices.Count > 0)
                        {
                            ssToModify = ss;
                            break;
                        }
                    }
                    if (ssToModify != null) break;
                }
                if (ssToModify != null) break;
            }

            int oldNumDevices = ssToModify.Devices.Count();
            IEndDevice deviceToRemove = ssToModify.Devices[0] as IEndDevice;

            int numThisDevice = 0;
            foreach (IEndDevice dev in ssToModify.Devices)
            {
                if (dev == deviceToRemove)
                {
                    numThisDevice++;
                }
            }

            for (int i = 0; i < numThisDevice; i++)
            {
                ssToModify.Devices.Remove(deviceToRemove);
            }

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope modifiedSubScope = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == ssToModify.Guid)
                        {
                            modifiedSubScope = ss;
                            break;
                        }
                    }
                    if (modifiedSubScope != null) break;
                }
                if (modifiedSubScope != null) break;
            }

            //Assert
            foreach (IEndDevice dev in modifiedSubScope.Devices)
            {
                if (deviceToRemove.Guid == dev.Guid) Assert.Fail("Device not removed properly.");
            }
            bool devFound = false;
            foreach (IEndDevice dev in actualBid.Catalogs.Devices)
            {
                if (deviceToRemove.Guid == dev.Guid) devFound = true;
            }
            if (!devFound) Assert.Fail();

            Assert.AreEqual(bid.Catalogs.Devices.Count(), actualBid.Catalogs.Devices.Count());
            Assert.AreEqual((oldNumDevices - numThisDevice), modifiedSubScope.Devices.Count);
        }

        [TestMethod]
        public void Save_Bid_LowerQuantity_Device()
        {
            //Act
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope ssToModify = null;
            foreach (TECSystem sys in bid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Devices.Count > 0)
                        {
                            ssToModify = ss;
                            break;
                        }
                    }
                    if (ssToModify != null) break;
                }
                if (ssToModify != null) break;
            }
            IEndDevice deviceToRemove = (ssToModify.Devices.First() as IEndDevice);

            int oldNumDevices = ssToModify.Devices.Count;        

            ssToModify.Devices.Remove(deviceToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope modifiedSubScope = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == ssToModify.Guid)
                        {
                            modifiedSubScope = ss;
                            break;
                        }
                    }
                    if (modifiedSubScope != null) break;
                }
                if (modifiedSubScope != null) break;
            }

            //Assert
            bool devFound = false;
            foreach (IEndDevice dev in actualBid.Catalogs.Devices)
            {
                if (deviceToRemove.Guid == dev.Guid) devFound = true;
            }
            foreach (IEndDevice dev in actualBid.Catalogs.Valves)
            {
                if (deviceToRemove.Guid == dev.Guid) devFound = true;
            }
            if (!devFound) Assert.Fail();
            

            Assert.AreEqual(bid.Catalogs.Devices.Count(), actualBid.Catalogs.Devices.Count());
            Assert.AreEqual((oldNumDevices - 1), modifiedSubScope.Devices.Count);
        }

        #region Edit Device
        [TestMethod]
        public void Save_Bid_Device_Quantity()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECSubScope ssToModify = null;
            foreach (TECSystem sys in bid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Devices.Count > 0)
                        {
                            ssToModify = ss;
                            break;
                        }
                    }
                    if (ssToModify != null) break;
                }
                if (ssToModify != null) break;
            }
            IEndDevice expectedDevice = (ssToModify.Devices[0] as IEndDevice);

            int expectedNumDevices = 0;

            foreach (IEndDevice dev in ssToModify.Devices)
            {
                if (dev.Guid == expectedDevice.Guid) expectedNumDevices++;
            }

            ssToModify.Devices.Add(expectedDevice);
            expectedNumDevices++;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope modifiedSS = null;
            int actualQuantity = 0;

            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {

                        if (ss.Guid == ssToModify.Guid)
                        {
                            modifiedSS = ss;
                            break;
                        }
                    }
                    if (modifiedSS != null) break;
                }
                if (modifiedSS != null) break;
            }

            IEndDevice actualDevice = null;
            foreach (IEndDevice dev in modifiedSS.Devices)
            {
                if (dev.Guid == expectedDevice.Guid)
                {
                    actualQuantity++;
                }
            }
            foreach (IEndDevice dev in modifiedSS.Devices)
            {
                if (expectedDevice.Guid == dev.Guid)
                {
                    actualDevice = dev;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNumDevices, actualQuantity);
        }
        #endregion Edit Device

        #endregion Save Device

        #region Save Point

        [TestMethod]
        public void Save_Bid_Add_Point()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPoint expectedPoint = new TECPoint();
            expectedPoint.Type = IOType.AI;
            expectedPoint.Label = "New Point";
            expectedPoint.Quantity = 5;

            TECSubScope subScopeToModify = null;
            foreach(TECSystem system in bid.Systems)
            {
                foreach(TECSubScope subScope in system.GetAllSubScope())
                {
                    if(subScope.Connection == null)
                    {
                        subScopeToModify = subScope;
                        break;
                    }
                }
                if (subScopeToModify != null) break;
            }
            subScopeToModify.Points.Add(expectedPoint);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPoint actualPoint = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == subScopeToModify.Guid)
                        {
                            foreach (TECPoint point in ss.Points)
                            {
                                if (expectedPoint.Guid == point.Guid)
                                {
                                    actualPoint = point;
                                    break;
                                }
                            }
                        }
                        if (actualPoint != null) break;
                    }
                    if (actualPoint != null) break;
                }
                if (actualPoint != null) break;
            }

            //Assert
            Assert.AreEqual(expectedPoint.Label, actualPoint.Label);
            Assert.AreEqual(expectedPoint.Quantity, actualPoint.Quantity);
            Assert.AreEqual(expectedPoint.Type, actualPoint.Type);
        }

        [TestMethod]
        public void Save_Bid_Remove_Point()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECSubScope ssToModify = bid.Systems[0].Equipment[0].SubScope[0];
            int oldNumPoints = ssToModify.Points.Count();
            TECPoint pointToRemove = ssToModify.Points[0];
            ssToModify.Points.Remove(pointToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECSubScope modifiedSubScope = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        if (ss.Guid == ssToModify.Guid)
                        {
                            modifiedSubScope = ss;
                            break;
                        }
                    }
                    if (modifiedSubScope != null) break;
                }
                if (modifiedSubScope != null) break;
            }

            //Assert
            foreach (TECPoint point in modifiedSubScope.Points)
            {
                if (pointToRemove.Guid == point.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumPoints - 1), modifiedSubScope.Points.Count);
        }

        #region Edit Point
        [TestMethod]
        public void Save_Bid_Point_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPoint expectedPoint = bid.Systems[0].Equipment[0].SubScope[0].Points[0];
            expectedPoint.Label = "Point name save test";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPoint actualPoint = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        foreach (TECPoint point in ss.Points)
                        {
                            if (point.Guid == expectedPoint.Guid)
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

            //Assert
            Assert.AreEqual(expectedPoint.Label, actualPoint.Label);
        }
        
        [TestMethod]
        public void Save_Bid_Point_Quantity()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPoint expectedPoint = null;
            foreach(TECSystem system in bid.Systems)
            {
                foreach(TECSubScope subScope in system.GetAllSubScope().Where(item => 
                (item.Connection == null && item.Points.Count > 0)))
                {
                    expectedPoint = subScope.Points[0];
                    break;
                }
                if (expectedPoint != null) break;
            }
            expectedPoint.Quantity = 4;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPoint actualPoint = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        foreach (TECPoint point in ss.Points)
                        {
                            if (point.Guid == expectedPoint.Guid)
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

            //Assert
            Assert.AreEqual(expectedPoint.Quantity, actualPoint.Quantity);
        }

        [TestMethod]
        public void Save_Bid_Point_Type()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPoint expectedPoint = bid.Systems[0].Equipment[0].SubScope[0].Points[0];
            expectedPoint.Type = IOType.DI;
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPoint actualPoint = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                foreach (TECEquipment equip in sys.Equipment)
                {
                    foreach (TECSubScope ss in equip.SubScope)
                    {
                        foreach (TECPoint point in ss.Points)
                        {
                            if (point.Guid == expectedPoint.Guid)
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

            //Assert
            Assert.AreEqual(expectedPoint.Type, actualPoint.Type);
        }
        #endregion Edit Point
        #endregion Save Point

        #region Save Tag
        [TestMethod]
        public void Save_Bid_Add_Tag_ToSystem()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSystem systemToEdit = bid.Systems[0];
            TECTag tagToAdd = null;
            foreach(TECTag tag in bid.Catalogs.Tags)
            {
                if (!systemToEdit.Tags.Contains(tag))
                {
                    systemToEdit.Tags.Add(tag);
                    tagToAdd = tag;
                    break;
                }
            }
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            TECSystem finalSystem = null;
            foreach (TECSystem system in finalBid.Systems)
            {
                if (system.Guid == systemToEdit.Guid)
                {
                    finalSystem = system;
                    break;
                }
            }

            bool tagExists = false;
            foreach (TECTag tag in finalSystem.Tags)
            {
                if (tag.Guid == tagToAdd.Guid) { tagExists = true; }
            }

            Assert.IsTrue(tagExists);
        }

        [TestMethod]
        public void Save_Bid_Add_Tag_ToEquipment()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECEquipment equipmentToEdit = bid.Systems[0].Equipment[0];
            TECTag tagToAdd = null;
            foreach (TECTag tag in bid.Catalogs.Tags)
            {
                if (!equipmentToEdit.Tags.Contains(tag))
                {
                    equipmentToEdit.Tags.Add(tag);
                    tagToAdd = tag;
                    break;
                }
            }

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            TECEquipment finalEquipment = null;
            foreach (TECSystem system in finalBid.Systems)
            {
                foreach (TECEquipment equipment in system.Equipment)
                {
                    if (equipment.Guid == equipmentToEdit.Guid)
                    {
                        finalEquipment = equipment;
                        break;
                    }
                }
                if (finalEquipment != null)
                {
                    break;
                }
            }

            bool tagExists = false;
            foreach (TECTag tag in finalEquipment.Tags)
            {
                if (tag.Guid == tagToAdd.Guid) { tagExists = true; }
            }

            Assert.IsTrue(tagExists);
        }

        [TestMethod]
        public void Save_Bid_Add_Tag_ToSubScope()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECSubScope subScopeToEdit = bid.Systems[0].Equipment[0].SubScope[0];
            TECTag tagToAdd = null;
            foreach (TECTag tag in bid.Catalogs.Tags)
            {
                if (!subScopeToEdit.Tags.Contains(tag))
                {
                    subScopeToEdit.Tags.Add(tag);
                    tagToAdd = tag;
                    break;
                }
            }

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            TECSubScope finalSubScope = null;
            foreach (TECSystem system in finalBid.Systems)
            {
                foreach (TECEquipment equip in system.Equipment)
                {
                    foreach (TECSubScope subScope in equip.SubScope)
                    {
                        if (subScope.Guid == subScopeToEdit.Guid)
                        {
                            finalSubScope = subScope;
                            break;
                        }
                    }
                    if (finalSubScope != null) { break; }
                }
                if (finalSubScope != null) { break; }
            }

            bool tagExists = false;
            foreach (TECTag tag in finalSubScope.Tags)
            {
                if (tag.Guid == tagToAdd.Guid) { tagExists = true; }
            }

            Assert.IsTrue(tagExists);
        }
        
        [TestMethod]
        public void Save_Bid_Add_Tag_ToController()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECController ControllerToEdit = bid.Controllers[0];
            TECTag tagToAdd = null;
            foreach (TECTag tag in bid.Catalogs.Tags)
            {
                if (!ControllerToEdit.Tags.Contains(tag))
                {
                    ControllerToEdit.Tags.Add(tag);
                    tagToAdd = tag;
                    break;
                }
            }

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid finalBid= loaded as TECBid;

            TECController finalController = null;
            foreach (TECController Controller in finalBid.Controllers)
            {
                if (Controller.Guid == ControllerToEdit.Guid)
                {
                    finalController = Controller;
                    break;
                }
            }

            bool tagExists = false;
            foreach (TECTag tag in finalController.Tags)
            {
                if (tag.Guid == tagToAdd.Guid) { tagExists = true; }
            }

            Assert.IsTrue(tagExists);
        }

        #endregion Save Tag

        #region Save Scope Branch

        [TestMethod]
        public void Save_Bid_Add_Branch()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumBranches = bid.ScopeTree.Count();
            TECScopeBranch expectedBranch = new TECScopeBranch();
            expectedBranch.Label = "New Branch";
            bid.ScopeTree.Add(expectedBranch);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECScopeBranch actualBranch = null;
            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == expectedBranch.Guid)
                {
                    actualBranch = branch;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedBranch.Label, actualBranch.Label);
            Assert.AreEqual((oldNumBranches + 1), actualBid.ScopeTree.Count);
        }

        [TestMethod]
        public void Save_Bid_Add_Branch_InBranch()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECScopeBranch expectedBranch = new TECScopeBranch();
            expectedBranch.Label = "New Child";
            TECScopeBranch branchToModify = bid.ScopeTree[0];
            branchToModify.Branches.Add(expectedBranch);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECScopeBranch modifiedBranch = null;
            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == branchToModify.Guid)
                {
                    modifiedBranch = branch;
                    break;
                }
            }

            TECScopeBranch actualBranch = null;
            foreach (TECScopeBranch branch in modifiedBranch.Branches)
            {
                if (branch.Guid == expectedBranch.Guid)
                {
                    actualBranch = branch;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedBranch.Label, actualBranch.Label);
        }

        [TestMethod]
        public void Save_Bid_Remove_Branch()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumBranches = bid.ScopeTree.Count();
            TECScopeBranch branchToRemove = bid.ScopeTree[0];
            bid.ScopeTree.Remove(branchToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == branchToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumBranches - 1), actualBid.ScopeTree.Count);
        }

        [TestMethod]
        public void Save_Bid_Remove_Branch_FromBranch()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECScopeBranch branchToModify = null;

            foreach (TECScopeBranch branch in bid.ScopeTree)
            {
                if (branch.Branches.Count > 0)
                {
                    branchToModify = branch;
                    break;
                }
            }

            int oldNumBranches = branchToModify.Branches.Count();
            TECScopeBranch branchToRemove = branchToModify.Branches[0];
            branchToModify.Branches.Remove(branchToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECScopeBranch modifiedBranch = null;
            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == branchToModify.Guid)
                {
                    modifiedBranch = branch;
                    break;
                }
            }

            //Assert
            foreach (TECScopeBranch branch in modifiedBranch.Branches)
            {
                if (branch.Guid == branchToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumBranches - 1), modifiedBranch.Branches.Count);
        }

        [TestMethod]
        public void Save_Bid_Branch_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            TECScopeBranch expectedBranch = bid.ScopeTree[0];
            expectedBranch.Label = "Test Branch Save";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECScopeBranch actualBranch = null;
            foreach (TECScopeBranch branch in actualBid.ScopeTree)
            {
                if (branch.Guid == expectedBranch.Guid)
                {
                    actualBranch = branch;
                }
            }

            //Assert
            Assert.AreEqual(expectedBranch.Label, actualBranch.Label);
        }
        
        #endregion Save Scope Branch

        #region Save Location
        [TestMethod]
        public void Save_Bid_Add_Location()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLocation expectedLocation = new TECLocation();
            expectedLocation.Name = "New Location";
            expectedLocation.Label = "NL";
            bid.Locations.Add(expectedLocation);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualLocation = null;
            foreach (TECLabeled loc in actualBid.Locations)
            {
                if (loc.Guid == expectedLocation.Guid)
                {
                    actualLocation = loc;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedLocation.Label, actualLocation.Label);
            Assert.AreEqual(expectedLocation.Guid, actualLocation.Guid);
        }

        [TestMethod]
        public void Save_Bid_Remove_Location()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumLocations = bid.Locations.Count;
            TECLocation locationToRemove = bid.Locations[0];
            bid.Locations.Remove(locationToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECLabeled loc in actualBid.Locations)
            {
                if (loc.Guid == locationToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumLocations - 1), actualBid.Locations.Count);
        }

        [TestMethod]
        public void Save_Bid_Edit_Location_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLabeled expectedLocation = bid.Locations[0];
            expectedLocation.Label = "Location Name Save";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualLocation = null;
            foreach (TECLabeled loc in actualBid.Locations)
            {
                if (loc.Guid == expectedLocation.Guid)
                {
                    actualLocation = loc;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedLocation.Label, actualLocation.Label);
        }

        [TestMethod]
        public void Save_Bid_Add_Location_ToScope()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLocation expectedLocation = bid.Locations[0];

            TECSystem sysToModify = bid.Systems.First(x => x.Equipment.Count > 0 && x.Equipment[0].SubScope.Count > 0);
            TECEquipment equipToModify = sysToModify.Equipment[0];
            TECSubScope ssToModify = equipToModify.SubScope[0];

            sysToModify.Location = expectedLocation;
            equipToModify.Location = expectedLocation;
            ssToModify.Location = expectedLocation;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualLocation = null;
            foreach (TECLabeled loc in actualBid.Locations)
            {
                if (loc.Guid == expectedLocation.Guid)
                {
                    actualLocation = loc;
                    break;
                }
            }

            TECSystem actualSystem = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                if (sys.Guid == sysToModify.Guid)
                {
                    actualSystem = sys;
                    break;
                }
            }
            TECEquipment actualEquip = actualSystem.Equipment[0];
            TECSubScope actualSS = actualEquip.SubScope[0];

            //Assert
            Assert.AreEqual(expectedLocation.Label, actualLocation.Label);
            Assert.AreEqual(expectedLocation.Guid, actualLocation.Guid);

            Assert.AreEqual(actualLocation, actualSystem.Location);
            Assert.AreEqual(actualLocation, actualEquip.Location);
            Assert.AreEqual(actualLocation, actualSS.Location);
        }

        [TestMethod]
        public void Save_Bid_Remove_Location_FromScope()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int expectedNumLocations = bid.Locations.Count;

            TECSystem expectedSys = bid.Systems.First(x => x.Equipment.Count > 0 && x.Equipment.Any(y => y.SubScope.Count > 0));
            TECEquipment expectedEquip = expectedSys.Equipment.First(x => x.SubScope.Count > 0);
            TECSubScope expectedSS = expectedEquip.SubScope.First();

            expectedSys.Location = null;
            expectedEquip.Location = null;
            expectedSS.Location = null;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            int actualNumLocations = actualBid.Locations.Count;

            TECSystem actualSys = actualBid.Systems.First(x => x.Guid == expectedSys.Guid);
            TECEquipment actualEquip = actualSys.Equipment.First(x => x.Guid == expectedEquip.Guid);
            TECSubScope actualSS = actualEquip.SubScope.First(x => x.Guid == expectedSS.Guid);

            //Assert
            Assert.AreEqual(expectedNumLocations, actualNumLocations);

            Assert.IsNull(actualSys.Location);
            Assert.IsNull(actualEquip.Location);
            Assert.IsNull(actualSS.Location);
        }

        [TestMethod]
        public void Save_Bid_Edit_Location_InScope()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            TECLocation expectedLocation = new TECLocation() { Name = "New Location" };
            bid.Locations.Add(expectedLocation);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int expectedNumLocations = bid.Locations.Count;
            TECSystem expectedSystem = bid.Systems.First(x => x.Location != expectedLocation);

            expectedSystem.Location = expectedLocation;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            int actualNumLocations = actualBid.Locations.Count;

            TECLabeled actualLocation = null;
            foreach (TECLabeled loc in actualBid.Locations)
            {
                if (loc.Guid == expectedLocation.Guid)
                {
                    actualLocation = loc;
                    break;
                }
            }

            TECSystem actualSystem = null;
            foreach (TECSystem sys in actualBid.Systems)
            {
                if (sys.Guid == expectedSystem.Guid)
                {
                    actualSystem = sys;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNumLocations, actualNumLocations);

            Assert.AreEqual(expectedLocation.Label, actualLocation.Label);
            Assert.AreEqual(actualLocation.Guid, actualSystem.Location.Guid);
        }

        [TestMethod]
        public void Save_Bid_Move_Location()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            bid.Locations.Add(new TECLocation() { Name = "New Location" });
            DeltaStacker testStack = SaveBid(bid);

            int initialIndex = 0;
            int expectedIndex = 1;
            TECLocation expectedLocation = bid.Locations[initialIndex];
            bid.Locations.Move(initialIndex, expectedIndex);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLocation actualLocation = actualBid.Locations[expectedIndex];

            //Assert
            Assert.AreEqual(expectedLocation.Guid, actualLocation.Guid);
        }
        #endregion Save Location

        #region Save Note
        [TestMethod]
        public void Save_Bid_Add_Note()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLabeled expectedNote = new TECLabeled();
            expectedNote.Label = "New Note";
            bid.Notes.Add(expectedNote);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualNote = null;
            foreach (TECLabeled note in actualBid.Notes)
            {
                if (note.Guid == expectedNote.Guid)
                {
                    actualNote = note;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNote.Label, actualNote.Label);
        }

        [TestMethod]
        public void Save_Bid_Remove_Note()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumNotes = bid.Notes.Count;
            TECLabeled noteToRemove = bid.Notes[0];
            bid.Notes.Remove(noteToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECLabeled note in actualBid.Notes)
            {
                if (note.Guid == noteToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumNotes - 1), bid.Notes.Count);
        }

        [TestMethod]
        public void Save_Bid_Note_Text()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLabeled expectedNote = bid.Notes[0];
            expectedNote.Label = "Test Save Text";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualNote = null;
            foreach (TECLabeled note in actualBid.Notes)
            {
                if (note.Guid == expectedNote.Guid)
                {
                    actualNote = note;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNote.Label, actualNote.Label);
        }
        #endregion Save Note

        #region Save Internal Note
        [TestMethod]
        public void Save_Bid_Add_InternalNote()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECInternalNote expectedNote = new TECInternalNote();
            expectedNote.Label = "New Note";
            bid.Notes.Add(expectedNote);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualNote = null;
            foreach (TECLabeled note in actualBid.Notes)
            {
                if (note.Guid == expectedNote.Guid)
                {
                    actualNote = note;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNote.Label, actualNote.Label);
        }

        [TestMethod]
        public void Save_Bid_Remove_InternalNote()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumNotes = bid.InternalNotes.Count;
            TECInternalNote noteToRemove = bid.InternalNotes[0];
            bid.InternalNotes.Remove(noteToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECInternalNote note in actualBid.InternalNotes)
            {
                if (note.Guid == noteToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumNotes - 1), bid.InternalNotes.Count);
        }

        [TestMethod]
        public void Save_Bid_InternalNote_Text()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECInternalNote expectedNote = bid.InternalNotes[0];
            expectedNote.Label = "Test Save Text";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECInternalNote actualNote = null;
            foreach (TECInternalNote note in actualBid.InternalNotes)
            {
                if (note.Guid == expectedNote.Guid)
                {
                    actualNote = note;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedNote.Label, actualNote.Label);
        }
        #endregion Save Note

        #region Save Exclusion

        [TestMethod]
        public void Save_Bid_Add_Exclusion()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLabeled expectedExclusion = new TECLabeled();
            expectedExclusion.Label = "New Exclusion";
            bid.Exclusions.Add(expectedExclusion);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualExclusion = null;
            foreach (TECLabeled Exclusion in actualBid.Exclusions)
            {
                if (Exclusion.Guid == expectedExclusion.Guid)
                {
                    actualExclusion = Exclusion;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedExclusion.Label, actualExclusion.Label);
        }

        [TestMethod]
        public void Save_Bid_Remove_Exclusion()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumExclusions = bid.Exclusions.Count;
            TECLabeled ExclusionToRemove = bid.Exclusions[0];
            bid.Exclusions.Remove(ExclusionToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECLabeled Exclusion in actualBid.Exclusions)
            {
                if (Exclusion.Guid == ExclusionToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumExclusions - 1), bid.Exclusions.Count);
        }

        [TestMethod]
        public void Save_Bid_Exclusion_Text()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECLabeled expectedExclusion = bid.Exclusions[0];
            expectedExclusion.Label = "Test Save Text";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECLabeled actualExclusion = null;
            foreach (TECLabeled Exclusion in actualBid.Exclusions)
            {
                if (Exclusion.Guid == expectedExclusion.Guid)
                {
                    actualExclusion = Exclusion;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedExclusion.Label, actualExclusion.Label);
        }
        #endregion Save Exclusion

        #region Save Controller
        [TestMethod]
        public void Save_Bid_Add_Controller()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECController expectedController = new TECProvidedController(Guid.NewGuid(), bid.Catalogs.ControllerTypes[0]);
            expectedController.Name = "Test Add Controller";
            expectedController.Description = "Test description";

            bid.AddController(expectedController);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECController actualController = null;
            foreach (TECController controller in actualBid.Controllers)
            {
                if (controller.Guid == expectedController.Guid)
                {
                    actualController = controller;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedController.Name, actualController.Name);
            Assert.AreEqual(expectedController.Description, actualController.Description);
        }

        [TestMethod]
        public void Save_Bid_Remove_Controller()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumControllers = bid.Controllers.Count;
            TECController controllerToRemove = bid.Controllers[0];

            bid.RemoveController(controllerToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECController controller in actualBid.Controllers)
            {
                if (controller.Guid == controllerToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumControllers - 1), actualBid.Controllers.Count);

        }

        [TestMethod]
        public void Save_Bid_Controller_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECController expectedController = bid.Controllers[0];
            expectedController.Name = "Test save controller name";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECController actualController = null;
            foreach (TECController controller in actualBid.Controllers)
            {
                if (controller.Guid == expectedController.Guid)
                {
                    actualController = controller;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedController.Name, actualController.Name);
        }

        [TestMethod]
        public void Save_Bid_Controller_Description()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECController expectedController = bid.Controllers[0];
            expectedController.Description = "Save Device Description";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECController actualController = null;
            foreach (TECController controller in actualBid.Controllers)
            {
                if (controller.Guid == expectedController.Guid)
                {
                    actualController = controller;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedController.Description, actualController.Description);
        }
        #endregion
        
        #region Save Misc Cost
        [TestMethod]
        public void Save_Bid_Add_MiscCost()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECMisc expectedCost = new TECMisc(CostType.Electrical);
            expectedCost.Name = "Add cost addition";
            expectedCost.Cost = 978.3;
            expectedCost.Quantity = 21;

            bid.MiscCosts.Add(expectedCost);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECMisc actualCost = null;
            foreach (TECMisc cost in actualBid.MiscCosts)
            {
                if (cost.Guid == expectedCost.Guid)
                {
                    actualCost = cost;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Cost, actualCost.Cost);
            Assert.AreEqual(expectedCost.Quantity, actualCost.Quantity);
        }

        [TestMethod]
        public void Save_Bid_Remove_MiscCost()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECMisc costToRemove = bid.MiscCosts[0];
            bid.MiscCosts.Remove(costToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECMisc cost in actualBid.MiscCosts)
            {
                if (cost.Guid == costToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual(bid.MiscCosts.Count, actualBid.MiscCosts.Count);
        }

        [TestMethod]
        public void Save_Bid_MiscCost_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECMisc expectedCost = bid.MiscCosts[0];
            expectedCost.Name = "Test Save Cost Name";

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECMisc actualCost = null;
            foreach (TECMisc cost in actualBid.MiscCosts)
            {
                if (cost.Guid == expectedCost.Guid)
                {
                    actualCost = cost;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
        }

        [TestMethod]
        public void Save_Bid_MiscCost_Cost()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECMisc expectedCost = bid.MiscCosts[0];
            expectedCost.Cost = 489.1238;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECMisc actualCost = null;
            foreach (TECMisc cost in actualBid.MiscCosts)
            {
                if (cost.Guid == expectedCost.Guid)
                {
                    actualCost = cost;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedCost.Cost, actualCost.Cost);
        }

        [TestMethod]
        public void Save_Bid_MiscCost_Quantity()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECMisc expectedCost = bid.MiscCosts[0];
            expectedCost.Quantity = 492;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECMisc actualCost = null;
            foreach (TECMisc cost in actualBid.MiscCosts)
            {
                if (cost.Guid == expectedCost.Guid)
                {
                    actualCost = cost;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedCost.Quantity, actualCost.Quantity);
        }
        #endregion

        #region Save Panel Type
        [TestMethod]
        public void Save_Bid_Add_PanelType()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPanelType expectedCost = new TECPanelType(bid.Catalogs.Manufacturers[0]);
            expectedCost.Name = "Add cost addition";
            expectedCost.Price = 978.3;

            bid.Catalogs.PanelTypes.Add(expectedCost);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPanelType actualCost = null;
            foreach (TECPanelType cost in bid.Catalogs.PanelTypes)
            {
                if (cost.Guid == expectedCost.Guid)
                {
                    actualCost = cost;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedCost.Name, actualCost.Name);
            Assert.AreEqual(expectedCost.Cost, actualCost.Cost);
        }
        
        #endregion

        #region Save Panel
        [TestMethod]
        public void Save_Bid_Add_Panel()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPanel expectedPanel = new TECPanel(bid.Catalogs.PanelTypes[0]);
            expectedPanel.Name = "Test Add Controller";
            expectedPanel.Description = "Test description";
            bid.Panels.Add(expectedPanel);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPanel actualpanel = null;
            foreach (TECPanel panel in actualBid.Panels)
            {
                if (panel.Guid == expectedPanel.Guid)
                {
                    actualpanel = panel;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedPanel.Name, actualpanel.Name);
            Assert.AreEqual(expectedPanel.Description, actualpanel.Description);
        }

        [TestMethod]
        public void Save_Bid_Remove_Panel()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            int oldNumPanels = bid.Panels.Count;
            TECPanel panelToRemove = bid.Panels[0];

            bid.Panels.Remove(panelToRemove);

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            //Assert
            foreach (TECPanel panel in actualBid.Panels)
            {
                if (panel.Guid == panelToRemove.Guid) Assert.Fail();
            }

            Assert.AreEqual((oldNumPanels - 1), actualBid.Panels.Count);

        }

        [TestMethod]
        public void Save_Bid_Edit_PanelType_In_Panel()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPanel panelToEdit = bid.Panels[0];

            TECPanelType newType = null;
            int x = 0;
            while(newType == null)
            {
                newType = bid.Catalogs.PanelTypes[x] != panelToEdit.Type ? bid.Catalogs.PanelTypes[x] : null;
                x++;
            }
            panelToEdit.Type = newType;

            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path);
            TECBid actualBid = loaded as TECBid;

            //Assert
            TECPanel actualPanel = null;
            foreach (TECPanel panel in actualBid.Panels)
            {
                if (panel.Guid == panelToEdit.Guid)
                {
                    actualPanel = panel;
                    break;
                }
            }
            Assert.AreEqual(newType.Guid, actualPanel.Type.Guid);
        }
        
        [TestMethod]
        public void Save_Bid_Panel_Name()
        {
            TECBid bid = ModelCreation.TestBid(rand);
            DeltaStacker testStack = SaveBid(bid);

            //Act
            TECPanel expectedPanel = bid.Panels[0];
            expectedPanel.Name = "Test save panel name";
            DatabaseUpdater.Update(path, testStack.CleansedStack());

            (TECScopeManager loaded, bool needsUpdate) = DatabaseLoader.Load(path); TECBid actualBid = loaded as TECBid;

            TECPanel actualPanel = null;
            foreach (TECPanel panel in actualBid.Panels)
            {
                if (panel.Guid == expectedPanel.Guid)
                {
                    actualPanel = panel;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(expectedPanel.Name, actualPanel.Name);
        }
        #endregion
    }
}
