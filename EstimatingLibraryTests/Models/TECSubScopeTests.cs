using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [TestClass()]
    public class TECSubScopeTests
    {

        [TestMethod()]
        public void DragDropCopyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddPointTest()
        {
            TECSubScope subScope = new TECSubScope();
            TECPoint point = new TECPoint();
            subScope.AddPoint(point);
            
            Assert.IsTrue(subScope.Points.Contains(point));
        }

        [TestMethod()]
        public void RemovePointTest()
        {
            TECSubScope subScope = new TECSubScope();
            TECPoint point = new TECPoint();
            subScope.AddPoint(point);

            subScope.RemovePoint(point);

            Assert.IsFalse(subScope.Points.Contains(point));

        }

        [TestMethod()]
        public void CanConnectToNetworkTest()
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

            Assert.IsTrue(subScope.CanConnectToNetwork(connection));
        }

        [TestMethod()]
        public void CanConnectToNetworkTest1()
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

            Assert.IsFalse(subScope.CanConnectToNetwork(connection));
        }

        [TestMethod()]
        public void CopyTest()
        {
            Assert.Fail();
        }
    }
}