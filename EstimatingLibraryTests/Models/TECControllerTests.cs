﻿using EstimatingLibrary;
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

            controller.Connect(subScope, subScope.AvailableProtocols.First());

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

            controller.Connect(subScope, subScope.AvailableProtocols.First());

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
    }
}
