using EstimatingLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Tests;

namespace Models
{
    [TestClass]
    public class TECControllerTests
    {
        [TestMethod]
        public void Controller_AddSubScope()
        {
            TECCatalogs catalogs = TestHelper.CreateTestCatalogs();
            TECController controller = new TECProvidedController(new TECControllerType(new TECManufacturer()), false);
            TECSubScope subScope = new TECSubScope(false);
            TECDevice dev = catalogs.Devices.First();
            subScope.Devices.Add(dev);

            controller.Connect(subScope, subScope.AvailableProtocols.First());

            Assert.AreEqual(1, controller.ChildrenConnections.Count, "Connection not added to controller");
            Assert.AreNotEqual(null, subScope.Connection, "Connection not added to subscope");
        }

        [TestMethod]
        public void Controller_RemoveSubScope()
        {
            TECCatalogs catalogs = TestHelper.CreateTestCatalogs();
            TECController controller = new TECProvidedController(new TECControllerType(new TECManufacturer()), false);
            TECSubScope subScope = new TECSubScope(false);
            TECDevice dev = catalogs.Devices.First();
            subScope.Devices.Add(dev);

            controller.Connect(subScope, subScope.AvailableProtocols.First());
            controller.Disconnect(subScope);

            Assert.AreEqual(0, controller.ChildrenConnections.Count, "Connection not removed from controller");
            Assert.AreEqual(null, subScope.Connection, "Connection not removed from subscope");
        }

        [TestMethod]
        public void Controller_AddNetworkConnection()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECController controller = new TECProvidedController(type, false);
            TECController childController = new TECProvidedController(type, false);

            TECProtocol protocol = new TECProtocol(new List<TECConnectionType> { });
            type.IO.Add(new TECIO(protocol));

            TECNetworkConnection connection = controller.AddNetworkConnection(protocol);
            connection.AddChild(childController);
            
            Assert.AreEqual(1, controller.ChildrenConnections.Count, "Connection not added to controller");
            Assert.AreEqual(connection, childController.ParentConnection, "Connection not added to child");
        }

        [TestMethod]
        public void Controller_RemoveNetworkConnection()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECController controller = new TECProvidedController(type, false);
            TECController childController = new TECProvidedController(type, false);

            TECProtocol protocol = new TECProtocol(new List<TECConnectionType> { });
            type.IO.Add(new TECIO(protocol));

            TECNetworkConnection connection = controller.AddNetworkConnection(protocol);
            connection.AddChild(childController);

            controller.RemoveNetworkConnection(connection);

            Assert.AreEqual(0, controller.ChildrenConnections.Count, "Connection not removed from controller");
            Assert.AreEqual(null, childController.ParentConnection, "Connection not removed from child");
        }

        [TestMethod]
        public void Controller_RemoveAllChildNetworkConnections()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECController controller = new TECProvidedController(type, false);
            TECController childController = new TECProvidedController(type, false);

            TECProtocol protocol = new TECProtocol(new List<TECConnectionType> { });
            type.IO.Add(new TECIO(protocol));

            TECNetworkConnection connection = controller.AddNetworkConnection(protocol);
            connection.AddChild(childController);

            controller.RemoveAllChildNetworkConnections();

            Assert.AreEqual(0, controller.ChildrenConnections.Count, "Connection not removed from controller");
            Assert.AreEqual(null, childController.ParentConnection, "Connection not removed from child");
        }

        [TestMethod]
        public void Controller_RemoveAllConnections()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));
            type.IO.Add(new TECIO(IOType.AI));

            TECController controller = new TECProvidedController(type, false);
            TECController childController = new TECProvidedController(type, false);
            TECController childestController = new TECProvidedController(type, false);

            TECProtocol protocol = new TECProtocol(new List<TECConnectionType> { });
            type.IO.Add(new TECIO(protocol));

            TECNetworkConnection connection = controller.AddNetworkConnection(protocol);
            connection.AddChild(childController);

            TECNetworkConnection childConnection = childController.AddNetworkConnection(protocol);
            childConnection.AddChild(childestController);

            childController.DisconnectAll();

            Assert.AreEqual(0, childController.ChildrenConnections.Count, "Connection not removed from controller");
            Assert.AreEqual(null, childController.ParentConnection, "Connection not removed from child");
            Assert.AreEqual(null, childestController.ParentConnection, "Connection not removed from childest");
        }
    }
}
