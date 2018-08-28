using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass]
    public class TECTypicalTests
    {
        Random rand;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestMethod]
        public void AddInstances()
        {
            TECBid bid = new TECBid();
            int qty = 3;
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            TECTypical system = ModelCreation.TestTypical(bid.Catalogs, rand);
            bid.Systems.Add(system);

            for (int x = 0; x < qty; x++)
            {
                system.AddInstance();
            }

            Assert.AreEqual(system.Instances.Count, qty);
            foreach (TECSystem instance in system.Instances)
            {
                Assert.AreEqual(system.Equipment.Count, instance.Equipment.Count);
                Assert.AreEqual(system.Controllers.Count, instance.Controllers.Count);
                Assert.AreEqual(system.Panels.Count, instance.Panels.Count);
            }

        }

        [TestMethod]
        public void EditInstances()
        {
            TECBid bid = new TECBid();
            int qty = 3;
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            TECTypical system = ModelCreation.TestTypical(bid.Catalogs, rand);
            bid.Systems.Add(system);
            for (int x = 0; x < qty; x++)
            {
                system.AddInstance();
            }

            system.Equipment.Add(ModelCreation.TestEquipment(bid.Catalogs, rand));
            system.AddController(ModelCreation.TestProvidedController(bid.Catalogs, rand));
            system.Panels.Add(ModelCreation.TestPanel(bid.Catalogs, rand));

            foreach (TECSystem instance in system.Instances)
            {
                Assert.AreEqual(system.Equipment.Count, instance.Equipment.Count);
                Assert.AreEqual(system.Controllers.Count, instance.Controllers.Count);
                Assert.AreEqual(system.Panels.Count, instance.Panels.Count);
            }
        }

        [TestMethod]
        public void AddRemoveSystemInstanceWithBidConnection()
        {
            var bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            var bidController = new TECProvidedController(bid.Catalogs.ControllerTypes.RandomElement(rand));
            bid.AddController(bidController);

            var system = new TECTypical();
            var equipment = new TECEquipment();
            var subScope = new TECSubScope();

            TECDevice dev = bid.Catalogs.Devices.First(x => x.HardwiredConnectionTypes.Count > 0);

            subScope.Devices.Add(dev);

            TECHardwiredProtocol hardProt = subScope.AvailableProtocols.First(x => x is TECHardwiredProtocol) as TECHardwiredProtocol;

            system.Equipment.Add(equipment);
            equipment.SubScope.Add(subScope);
            var instance = system.AddInstance();
            var instanceSubScope = instance.GetAllSubScope().First();

            bidController.Connect(instanceSubScope, hardProt);

            Assert.AreEqual(1, bidController.ChildrenConnections.Count, "Connection not added");

            system.Instances.Remove(instance);

            Assert.AreEqual(0, bidController.ChildrenConnections.Count, "Connection not removed");
        }

        [TestMethod]
        public void RemoveControllerFromTypicalWithInstanceConnections()
        {
            TECBid bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            TECTypical system = ModelCreation.TestTypical(bid.Catalogs, rand);
            TECController controller = system.Controllers[0];
            TECEquipment equipment = system.Equipment[0];
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(bid.Catalogs.Devices.First());
            equipment.SubScope.Add(subScope);
            controller.Connect(subScope, subScope.AvailableProtocols.First());
            bid.Systems.Add(system);
            TECSystem instance = system.AddInstance();

            TECController instanceController = system.TypicalInstanceDictionary.GetInstances(controller)[0] as TECController;
            TECSubScope instanceSubScope = system.TypicalInstanceDictionary.GetInstances(subScope)[0] as TECSubScope;

            system.RemoveController(controller);

            Assert.IsFalse(instance.Controllers.Contains(instanceController));
            Assert.IsTrue(instanceSubScope.Connection == null);
        }

        [TestMethod]
        public void AddEquipmentToTypicalWithInstance()
        {
            TECBid bid = new TECBid();
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance();

            TECEquipment toAdd = new TECEquipment();
            typical.Equipment.Add(toAdd);

            TECEquipment instanceEquipment = typical.TypicalInstanceDictionary.GetInstances(toAdd)[0] as TECEquipment;

            Assert.IsNotNull(instanceEquipment, "Not added to instance dictionary");
            Assert.IsTrue(instance.Equipment.Contains(instanceEquipment));
        }

        [TestMethod]
        public void AddSubScopeConnectionToTypicalWithInstance()
        {
            TECBid bid = new TECBid();
            TECTypical typical = new TECTypical();
            TECEquipment equipment = new TECEquipment();
            typical.Equipment.Add(equipment);
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance();

            TECSubScope toAdd = new TECSubScope();
            equipment.SubScope.Add(toAdd);

            TECEquipment instanceEquipment = typical.TypicalInstanceDictionary.GetInstances(equipment)[0] as TECEquipment;
            TECSubScope instanceSubScope = typical.TypicalInstanceDictionary.GetInstances(toAdd)[0] as TECSubScope;

            Assert.IsNotNull(instanceSubScope, "Not added to instance dictionary");
            Assert.IsTrue(instanceEquipment.SubScope.Contains(instanceSubScope));
        }

        [TestMethod]
        public void MakeTypical()
        {
            TECBid bid = ModelCreation.TestBid(rand);

            TECTypical typ = bid.Systems.First();

            Assert.IsTrue(typ.Equipment.Count > 0);
            foreach (TECEquipment equip in typ.Equipment)
            {
                Assert.IsTrue(equip.IsTypical);
                Assert.IsTrue(equip.SubScope.Count > 0);
                foreach (TECSubScope ss in equip.SubScope)
                {
                    Assert.IsTrue(ss.IsTypical);
                    Assert.IsTrue(ss.Points.Count > 0);
                    foreach (TECPoint point in ss.Points)
                    {
                        Assert.IsTrue(point.IsTypical);
                    }
                }
            }
            Assert.IsTrue(typ.Controllers.Count > 0);
            foreach (TECController controller in typ.Controllers)
            {
                Assert.IsTrue(controller.IsTypical);
            }
            Assert.IsTrue(typ.Panels.Count > 0);
            foreach (TECPanel panel in typ.Panels)
            {
                Assert.IsTrue(panel.IsTypical);
            }
            Assert.IsTrue(typ.MiscCosts.Count > 0);
            foreach (TECMisc misc in typ.MiscCosts)
            {
                Assert.IsTrue(misc.IsTypical);
            }
            Assert.IsTrue(typ.Instances.Count > 0);
            foreach (TECSystem sys in typ.Instances)
            {
                Assert.IsFalse(sys.IsTypical);
            }
        }

        [TestMethod]
        public void AddPointToTypicalSubScope()
        {
            TECBid bid = ModelCreation.TestBid(rand);

            TECTypical typical = null;
            TECSubScope typSS = null;

            foreach (TECTypical typ in bid.Systems)
            {
                if (typ.Instances.Count > 0)
                {
                    foreach (TECEquipment equip in typ.Equipment)
                    {
                        if (equip.SubScope.Count > 0)
                        {
                            typical = typ;
                            typSS = equip.SubScope[0];
                        }
                    }
                }
            }

            Assert.IsNotNull(typical);
            Assert.IsNotNull(typSS);

            TECPoint newPoint = ModelCreation.TestPoint(rand, IOType.AI);
            newPoint.Label = "New Point";

            typSS.Points.Add(newPoint);

            List<TECSubScope> instanceSubScope = typical.GetInstancesFromTypical(typSS);
            List<TECPoint> instancePoints = typical.GetInstancesFromTypical(newPoint);

            foreach (TECSubScope instanceSS in instanceSubScope)
            {
                TECPoint newInstancePoint = null;
                foreach (TECPoint point in instanceSS.Points)
                {
                    if (point.Label == newPoint.Label)
                    {
                        newInstancePoint = point;
                    }
                }

                Assert.IsNotNull(newInstancePoint);
                Assert.IsTrue(instancePoints.Contains(newInstancePoint));
            }
        }
        

        [TestMethod()]
        public void AddInstanceTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECTypical typical = ModelCreation.TestTypical(catalogs, rand);

            TECSystem system = typical.AddInstance();

            Assert.AreEqual(typical.Equipment.Count, system.Equipment.Count);
            Assert.AreEqual(typical.ScopeBranches.Count, system.ScopeBranches.Count);
            Assert.AreEqual(typical.ProposalItems.Count, system.ProposalItems.Count);
            Assert.AreEqual(typical.Controllers.Count, system.Controllers.Count);
            Assert.AreEqual(typical.Panels.Count, system.Panels.Count);
            Assert.AreEqual(typical.MiscCosts.Count, system.MiscCosts.Count);
            Assert.AreEqual(typical.Controllers.Aggregate(0, (total, next) => total += next.GetAll<TECConnection>().Count), system.GetAll<TECConnection>().Count);
        }

        [TestMethod()]
        public void UpdateInstanceConnectionsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CanUpdateInstanceConnectionsTest()
        {
            Assert.Fail();
        }
        
        [TestMethod()]
        public void GetInstancesFromTypicalTest()
        {
            TECSubScope subScope = new TECSubScope();
            TECEquipment equipment = new TECEquipment();
            TECTypical typical = new TECTypical();
            equipment.SubScope.Add(subScope);
            typical.Equipment.Add(equipment);
            
            typical.AddInstance();
            typical.AddInstance();

            Assert.AreEqual(2, typical.GetInstancesFromTypical(subScope).Count);
            Assert.AreEqual(2, typical.GetInstancesFromTypical(equipment).Count);
        }

        [TestMethod()]
        public void DragDropCopyTest()
        {
            Assert.Fail();
        }
    }
}
