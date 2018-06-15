using System;
using System.Linq;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;

namespace Models
{
    [TestClass]
    public class TECBidTests
    {
        [TestMethod]
        public void RemoveInstanceWithGlobalConnectionToSubScope()
        {
            TECBid bid = new TECBid();

            bid.Catalogs = TestHelper.CreateTestCatalogs();

            TECController controller = new TECProvidedController(bid.Catalogs.ControllerTypes.First(), false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            TECDevice device = null;
            foreach(TECDevice item in bid.Catalogs.Devices)
            {
                if (item.PossibleProtocols.Count > 0 && controller.AvailableProtocols.Contains(item.PossibleProtocols.First()))
                {
                    device = item;
                    break;
                }
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

            bid.Catalogs = TestHelper.CreateTestCatalogs();

            TECController controller = new TECProvidedController(bid.Catalogs.ControllerTypes.First(), false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            TECController typicalController = new TECProvidedController(bid.Catalogs.ControllerTypes.First(), false);

            typical.AddController(typicalController);

            bid.Systems.Add(typical);
            TECSystem system = typical.AddInstance(bid);
            TECController instanceController = typical.GetInstancesFromTypical(typicalController).First();
            IControllerConnection connection = controller.Connect(instanceController, instanceController.AvailableProtocols.First());
            Assert.IsTrue(connection is TECNetworkConnection);

            typical.Instances.Remove(system);

            Assert.IsTrue((connection as TECNetworkConnection).Children.Count == 0);

        }
    }
}
