using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary.Interfaces;

namespace Models
{
    [TestClass()]
    public class TECNetworkConnectionTests
    {

        [TestMethod()]
        public void CanAddChildTest()
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

            TECNetworkConnection connection = controller.AddNetworkConnection(firstProtocol);
            
            Assert.IsTrue(connection.CanAddChild(subScope));
        }

        [TestMethod()]
        public void CanAddChildTest1()
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

            TECNetworkConnection connection = controller.AddNetworkConnection(thirdProtocol);

            Assert.IsFalse(connection.CanAddChild(subScope));
        }

        [TestMethod()]
        public void AddChildTest()
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

            TECNetworkConnection connection = controller.AddNetworkConnection(firstProtocol);
            connection.AddChild(subScope);

            Assert.IsTrue(connection.Children.Contains(subScope));
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
            Assert.IsTrue(connection.Protocol == firstProtocol);
        }

        [TestMethod()]
        public void RemoveChildTest()
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

            connection.RemoveChild(subScope);

            Assert.IsFalse(connection.Children.Contains(subScope));
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            Assert.IsNull((subScope as IConnectable).GetParentConnection());
        }
    }
}