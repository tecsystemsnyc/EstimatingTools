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
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            TECTypical system = ModelCreation.TestTypical(bid.Catalogs, rand);
            bid.Systems.Add(system);
            for (int x = 0; x < qty; x++)
            {
                system.AddInstance(bid);
            }

            system.Equipment.Add(ModelCreation.TestEquipment(bid.Catalogs,rand));
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
            var bidController = new TECProvidedController(new TECControllerType(new TECManufacturer()));
            bid.AddController(bidController);

            var system = new TECTypical();
            var equipment = new TECEquipment();
            var subScope = new TECSubScope();
            TECDevice dev = bid.Catalogs.Devices.First(x => x.HardwiredConnectionTypes.Count > 0);
            subScope.Devices.Add(dev);
            system.Equipment.Add(equipment);
            equipment.SubScope.Add(subScope);
            Console.WriteLine("Num SubScope Protocol: {0}", subScope.AvailableProtocols.Count);
            Console.WriteLine("Num Controller Protocol: {0}", bidController.AvailableProtocols.Count);
            foreach (IProtocol prot in subScope.AvailableProtocols)
            {
                Console.WriteLine("SubScope Protocol: {0}", prot.Label);
            }
            foreach (IProtocol prot in bidController.AvailableProtocols)
            {
                Console.WriteLine("Controller Protocol: {0}", prot.Label);
            }
            bidController.Connect(subScope, subScope.AvailableProtocols.First());
            var instance = system.AddInstance(bid);
            var instanceSubScope = instance.GetAllSubScope().First();
            bidController.Connect(instanceSubScope, instanceSubScope.AvailableProtocols.First());
            
            Assert.AreEqual(2, bidController.ChildrenConnections.Count, "Connection not added");

            system.Instances.Remove(instance);

            Assert.AreEqual(1, bidController.ChildrenConnections.Count, "Connection not removed");
        }

        [TestMethod]
        public void RemoveTypicalSubScopeFromBidController()
        {
            //Arrange
            TECBid bid = new TECBid();
            bid.Catalogs = ModelCreation.TestCatalogs(rand);
            TECController controller = new TECProvidedController(new TECControllerType(new TECManufacturer()));
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment();
            TECSubScope ss = new TECSubScope();
            TECDevice dev = bid.Catalogs.Devices.First();
            ss.Devices.Add(dev);
            typical.Equipment.Add(equip);
            equip.SubScope.Add(ss);
            bid.Systems.Add(typical);

            TECSystem instance = typical.AddInstance(bid);
            TECSubScope instanceSS = instance.Equipment[0].SubScope[0];

            controller.Connect(ss, ss.AvailableProtocols.First());

            //Act
            controller.Disconnect(ss);

            //Assert
            Assert.IsTrue(instanceSS.Connection == null, "Instance subscope connection wasn't removed.");
            Assert.AreEqual(0, controller.ChildrenConnections.Count, "Bid controller still contains connections.");
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
            TECSystem instance = typical.AddInstance(bid);

            TECSubScope toAdd = new TECSubScope();
            equipment.SubScope.Add(toAdd);

            TECEquipment instanceEquipment = typical.TypicalInstanceDictionary.GetInstances(equipment)[0] as TECEquipment;
            TECSubScope instanceSubScope = typical.TypicalInstanceDictionary.GetInstances(toAdd)[0] as TECSubScope;

            Assert.IsNotNull(instanceSubScope, "Not added to instance dictionary");
            Assert.IsTrue(instanceEquipment.SubScope.Contains(instanceSubScope));
        }

    }
}
