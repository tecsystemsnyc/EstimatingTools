using System;
using System.Linq;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass]
    public class TECBidTests
    {
        Random rand;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestMethod]
        public void RemoveInstanceWithGlobalConnectionToSubScope()
        {
            TECBid bid = new TECBid();

            bid.Catalogs = ModelCreation.TestCatalogs(rand);

            TECController controller = ModelCreation.TestProvidedController(bid.Catalogs, rand);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECEquipment equipment = new TECEquipment();
            TECSubScope subScope = new TECSubScope();
            TECDevice device = null;
            foreach(TECDevice item in bid.Catalogs.Devices)
            {
                foreach(TECProtocol prot in item.PossibleProtocols)
                {
                    if (controller.AvailableProtocols.Contains(prot))
                    {
                        device = item;
                        break;
                    }
                }
                if (device != null) break;
            }
            if (device == null)
            {
                throw new NullReferenceException("Device is Null");
            }
            subScope.Devices.Add(device);
            equipment.SubScope.Add(subScope);
            typical.Equipment.Add(equipment);

            bid.Systems.Add(typical);
            TECSystem system = typical.AddInstance(bid);
            TECSubScope instanceSubScope = typical.GetInstancesFromTypical(subScope).First();
            IControllerConnection connection = controller.Connect(instanceSubScope, instanceSubScope.AvailableProtocols.First());
            Assert.IsTrue(connection is TECNetworkConnection);

            typical.Instances.Remove(system);

            Assert.IsTrue((connection as TECNetworkConnection).Children.Count == 0);
            
        }

        [TestMethod]
        public void RemoveInstanceWithGlobalConnectionToController()
        {
            TECBid bid = new TECBid();

            bid.Catalogs = ModelCreation.TestCatalogs(rand);

            TECControllerType type = bid.Catalogs.ControllerTypes.RandomElement(rand);

            TECController controller = new TECProvidedController(type);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECController typicalController = new TECProvidedController(type);

            typical.AddController(typicalController);

            bid.Systems.Add(typical);
            TECSystem system = typical.AddInstance(bid);
            TECController instanceController = typical.GetInstancesFromTypical(typicalController).First();

            Assert.IsTrue(controller.CanConnect(instanceController));

            IControllerConnection connection = controller.Connect(instanceController, instanceController.AvailableProtocols.First());

            Assert.IsTrue(connection is TECNetworkConnection);

            typical.Instances.Remove(system);

            Assert.IsTrue((connection as TECNetworkConnection).Children.Count == 0);

        }
    }
}
