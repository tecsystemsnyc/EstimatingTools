using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TestLibrary.MockClasses;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass]
    public class TECControllerTests
    {
        Random rand;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestMethod]
        public void Controller_AddSubScope()
        {
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECController controller = ModelCreation.TestProvidedController(catalogs, rand);
            TECSubScope subScope = new TECSubScope();
            TECDevice dev = catalogs.Devices.First();
            subScope.Devices.Add(dev);

            controller.Connect(subScope, subScope.AvailableProtocols.First(y => controller.AvailableProtocols.Contains(y)));

            Assert.AreEqual(1, controller.ChildrenConnections.Count, "Connection not added to controller");
            Assert.AreNotEqual(null, subScope.Connection, "Connection not added to subscope");
        }

        [TestMethod]
        public void Controller_RemoveSubScope()
        {
            //Arrange
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECController controller = ModelCreation.TestProvidedController(catalogs, rand);
            TECSubScope subScope = new TECSubScope();
            TECDevice dev = catalogs.Devices.First();
            subScope.Devices.Add(dev);

            controller.Connect(subScope, subScope.AvailableProtocols.First(y => controller.AvailableProtocols.Contains(y)));

            //Act
            TECNetworkConnection netConnect = controller.Disconnect(subScope);

            //Assert
            Assert.AreEqual(0, netConnect.Children.Count(), "SubScope not removed from connection");
            Assert.AreEqual(null, subScope.Connection, "Connection not removed from subscope");
        }

        [TestMethod]
        public void Controller_AddNetworkConnection()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECController controller = new TECProvidedController(type);
            TECController childController = new TECProvidedController(type);

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

            TECController controller = new TECProvidedController(type);
            TECController childController = new TECProvidedController(type);

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

            TECController controller = new TECProvidedController(type);
            TECController childController = new TECProvidedController(type);

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

            TECController controller = new TECProvidedController(type);
            TECController childController = new TECProvidedController(type);
            TECController childestController = new TECProvidedController(type);

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
        
        [TestMethod()]
        public void CompatibleProtocolsTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            var compatible = controller.CompatibleProtocols(subScope);

            Assert.AreEqual(2, compatible.Count);
            Assert.IsTrue(compatible.Contains(firstProtocol) && compatible.Contains(secondProtocol));

        }

        [TestMethod()]
        public void CompatibleProtocolsTest1()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            var compatible = controller.CompatibleProtocols(subScope);

            Assert.AreEqual(0, compatible.Count);

        }

        [TestMethod()]
        public void CompatibleProtocolsTest2()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);
            
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(type);

            var compatible = controller.CompatibleProtocols(subScope);

            Assert.AreEqual(1, compatible.Count);
            Assert.IsTrue(compatible.Any(x => x is TECHardwiredProtocol));

        }

        [TestMethod()]
        public void CompatibleProtocolsTest3()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AO));

            TECProvidedController controller = new TECProvidedController(type);

            var compatible = controller.CompatibleProtocols(subScope);

            Assert.AreEqual(0, compatible.Count);

        }

        [TestMethod()]
        public void CanConnectTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);
            
            Assert.IsTrue(controller.CanConnect(subScope));
        }

        [TestMethod()]
        public void CanConnectTest1()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);


            Assert.IsFalse(controller.CanConnect(subScope));

        }

        [TestMethod()]
        public void CanConnectTest2()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(type);
            
            Assert.IsTrue(controller.CanConnect(subScope));

        }

        [TestMethod()]
        public void CanConnectTest3()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AO));

            TECProvidedController controller = new TECProvidedController(type);
            
            Assert.IsFalse(controller.CanConnect(subScope));
        }

        [TestMethod()]
        public void CanConnectTest4()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(controller.CanConnect(subScope, firstProtocol));
            Assert.IsTrue(controller.CanConnect(subScope, secondProtocol));
            Assert.IsFalse(controller.CanConnect(subScope, fourthProtocol));
        }

        [TestMethod()]
        public void CanConnectTest5()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);


            Assert.IsFalse(controller.CanConnect(subScope, firstProtocol));
            Assert.IsFalse(controller.CanConnect(subScope, fourthProtocol));
            
        }

        [TestMethod()]
        public void CanConnectTest6()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(controller.CanConnect(subScope, subScope.HardwiredProtocol()));

        }

        [TestMethod()]
        public void CanConnectTest7()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AO));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsFalse(controller.CanConnect(subScope, subScope.AvailableProtocols.First(x => x is TECHardwiredProtocol)));
        }

        [TestMethod()]
        public void ConnectTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;
            
            Assert.IsTrue(connection.Children.Contains(subScope));
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
            Assert.IsTrue(connection.Protocol == firstProtocol);
        }

        [TestMethod()]
        public void ConnectTest1()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);
            var connection = controller.Connect(subScope, firstProtocol);

            Assert.IsNull(connection);

        }

        [TestMethod()]
        public void ConnectTest2()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(type);

            TECHardwiredConnection connection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;

            Assert.IsTrue(connection.Child == subScope);
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
            Assert.IsTrue(connection.Protocol is TECHardwiredProtocol);
        }

        [TestMethod()]
        public void ConnectTest3()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AO));

            TECProvidedController controller = new TECProvidedController(type);
            TECHardwiredConnection connection = controller.Connect(subScope, subScope.AvailableProtocols.First(x => x is TECHardwiredProtocol)) as TECHardwiredConnection;

            Assert.IsNull(connection);
        }

        [TestMethod()]
        public void DisconnectTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

            controller.Disconnect(subScope);

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());
        }

        [TestMethod()]
        public void DiconnectTest1()
        {
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(type);

            TECHardwiredConnection connection = controller.Connect(subScope, subScope.AvailableProtocols.First(x => x is TECHardwiredProtocol)) as TECHardwiredConnection;

            controller.Disconnect(subScope);

            Assert.IsFalse(connection.Child == subScope);
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());
        }

        [TestMethod()]
        public void CanAddNetworkConnectionTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());
            
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(controller.CanAddNetworkConnection(firstProtocol));
            Assert.IsFalse(controller.CanAddNetworkConnection(fourthProtocol));
        }

        [TestMethod()]
        public void AddNetworkConnectionTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());
            
            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.AddNetworkConnection(firstProtocol);

            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.IsTrue(connection.Protocol == firstProtocol);
        }

        [TestMethod()]
        public void AddNetworkConnectionTest1()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.AddNetworkConnection(fourthProtocol);

            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull(connection);
        }

        [TestMethod()]
        public void RemoveNetworkConnectionTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

            controller.RemoveNetworkConnection(connection);

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());
        }

        [TestMethod()]
        public void DisconnectAllTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;
            
            TECDevice compatibleHardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope hardSubScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            type.IO.Add(new TECIO(IOType.AI));
            
            TECHardwiredConnection hardConnection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;


            TECController parentController = new TECProvidedController(type);

            parentController.Connect(controller, secondProtocol);

            Assert.IsNotNull(controller.ParentConnection);
            
            controller.DisconnectAll();

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());

            Assert.IsNull(hardConnection.Child);
            Assert.IsFalse(controller.ChildrenConnections.Contains(hardConnection));
            Assert.IsNull((hardSubScope as IConnectable).GetParentConnection());

            Assert.IsNull(controller.ParentConnection);
        }

        [TestMethod()]
        public void RemoveAllChildNetworkConnectionsTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

            TECDevice compatibleHardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope hardSubScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            type.IO.Add(new TECIO(IOType.AI));

            TECHardwiredConnection hardConnection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;


            TECController parentController = new TECProvidedController(type);

            parentController.Connect(controller, secondProtocol);

            Assert.IsNotNull(controller.ParentConnection);

            controller.DisconnectAll();

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());

            Assert.AreEqual(hardConnection.Child, hardSubScope);
            Assert.IsTrue(controller.ChildrenConnections.Contains(hardConnection));
            Assert.AreEqual((hardSubScope as IConnectable).GetParentConnection(), hardConnection);

            Assert.IsNotNull(controller.ParentConnection);
        }

        [TestMethod()]
        public void RemoveAllChildHardwiredConnectionsTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

            TECDevice compatibleHardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope hardSubScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            type.IO.Add(new TECIO(IOType.AI));

            TECHardwiredConnection hardConnection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;


            TECController parentController = new TECProvidedController(type);

            parentController.Connect(controller, secondProtocol);

            Assert.IsNotNull(controller.ParentConnection);

            controller.RemoveAllChildHardwiredConnections();

            Assert.IsTrue(connection.Children.Contains(subScope));
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.AreEqual((subScope as IConnectable).GetParentConnection(), connection);

            Assert.IsNull(hardConnection.Child);
            Assert.IsFalse(controller.ChildrenConnections.Contains(hardConnection));
            Assert.IsNull((hardSubScope as IConnectable).GetParentConnection());

            Assert.IsNotNull(controller.ParentConnection);
        }

        [TestMethod()]
        public void RemoveAllChildConnectionsTest()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(firstProtocol));
            type.IO.Add(new TECIO(secondProtocol));
            type.IO.Add(new TECIO(thirdProtocol));

            TECProvidedController controller = new TECProvidedController(type);

            TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

            TECDevice compatibleHardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            TECSubScope hardSubScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            type.IO.Add(new TECIO(IOType.AI));

            TECHardwiredConnection hardConnection = controller.Connect(subScope, subScope.AvailableProtocols.First(x => x is TECHardwiredProtocol)) as TECHardwiredConnection;


            TECController parentController = new TECProvidedController(type);

            parentController.Connect(controller, secondProtocol);

            Assert.IsNotNull(controller.ParentConnection);

            controller.RemoveAllChildConnections();

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());

            Assert.IsNull(hardConnection.Child);
            Assert.IsFalse(controller.ChildrenConnections.Contains(hardConnection));
            Assert.IsNull((hardSubScope as IConnectable).GetParentConnection());

            Assert.IsNotNull(controller.ParentConnection);
        }

        [TestMethod()]
        public void CopyControllerTest()
        {
            Assert.Fail();
        }
    }
}
