using EstimatingLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TypicalTests
    {
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
        
        [TestMethod]
        public void AddInstances()
        {
            TECBid bid = new TECBid();
            int qty = 3;
            bid.Catalogs = TestHelper.CreateTestCatalogs();
            TECTypical system = TestHelper.CreateTestTypical(bid.Catalogs);
            bid.Systems.Add(system);

            for (int x = 0; x < qty; x++)
            {
                system.AddInstance(bid);
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
            bid.Catalogs = TestHelper.CreateTestCatalogs();
            TECTypical system = TestHelper.CreateTestTypical(bid.Catalogs);
            bid.Systems.Add(system);
            for (int x = 0; x < qty; x++)
            {
                system.AddInstance(bid);
            }

            system.Equipment.Add(TestHelper.CreateTestEquipment(true, bid.Catalogs));
            system.AddController(TestHelper.CreateTestController(true, bid.Catalogs));
            system.Panels.Add(TestHelper.CreateTestPanel(true, bid.Catalogs));

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
            var bidController = new TECController(new TECControllerType(new TECManufacturer()), false);
            bid.AddController(bidController);

            var system = new TECTypical();
            var equipment = new TECEquipment(true);
            var subScope = new TECSubScope(true);
            system.Equipment.Add(equipment);
            equipment.SubScope.Add(subScope);
            bidController.AddSubScopeConnection(subScope);
            var instance = system.AddInstance(bid);
            bidController.AddSubScopeConnection(instance.GetAllSubScope()[0]);
            
            Assert.AreEqual(2, bidController.ChildrenConnections.Count, "Connection not added");

            system.Instances.Remove(instance);

            Assert.AreEqual(1, bidController.ChildrenConnections.Count, "Connection not removed");
        }

        [TestMethod]
        public void RemoveTypicalSubScopeFromBidController()
        {
            //Arrange
            TECBid bid = new TECBid();
            TECController controller = new TECController(new TECControllerType(new TECManufacturer()), false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            typical.Equipment.Add(equip);
            equip.SubScope.Add(ss);
            bid.Systems.Add(typical);

            TECSystem instance = typical.AddInstance(bid);
            TECSubScope instanceSS = instance.Equipment[0].SubScope[0];

            controller.AddSubScopeConnection(ss);

            //Act
            controller.RemoveSubScope(ss);

            //Assert
            Assert.IsTrue(instanceSS.Connection == null, "Instance subscope connection wasn't removed.");
            Assert.AreEqual(0, controller.ChildrenConnections.Count, "Bid controller still contains connections.");
        }
        
        [TestMethod]
        public void RemoveControllerFromTypicalWithInstanceConnections()
        {
            TECBid bid = new TECBid();
            bid.Catalogs = TestHelper.CreateTestCatalogs();
            TECTypical system = TestHelper.CreateTestTypical(bid.Catalogs);
            TECController controller = system.Controllers[0];
            TECEquipment equipment = system.Equipment[0];
            TECSubScope subScope = new TECSubScope(false);
            equipment.SubScope.Add(subScope);
            controller.AddSubScopeConnection(subScope);
            bid.Systems.Add(system);
            TECSystem instance = system.AddInstance(bid);
            
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
            TECSystem instance = typical.AddInstance(bid);

            TECEquipment toAdd = new TECEquipment(true);
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
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);

            TECSubScope toAdd = new TECSubScope(true);
            equipment.SubScope.Add(toAdd);

            TECEquipment instanceEquipment = typical.TypicalInstanceDictionary.GetInstances(equipment)[0] as TECEquipment;
            TECSubScope instanceSubScope = typical.TypicalInstanceDictionary.GetInstances(toAdd)[0] as TECSubScope;

            Assert.IsNotNull(instanceSubScope, "Not added to instance dictionary");
            Assert.IsTrue(instanceEquipment.SubScope.Contains(instanceSubScope));
        }

    }
}
