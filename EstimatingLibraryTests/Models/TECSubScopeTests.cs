using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass()]
    public class TECSubScopeTests
    {
        [TestMethod()]
        public void DragDropCopyTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand, 2);
            TECSubScope subScope = ModelCreation.TestSubScope(bid.Catalogs, rand);

            var copy = subScope.DragDropCopy(bid) as TECSubScope;

            Assert.AreEqual(subScope.Name, copy.Name);
            Assert.IsTrue(subScope.CostBatch.CostsEqual(copy.CostBatch));
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
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand, 2);
            TECSubScope subScope = ModelCreation.TestSubScope(catalogs, rand);

            var copy = subScope.Copy(new Dictionary<Guid, Guid>());

            Assert.AreEqual(subScope.Name, copy.Name);
            Assert.IsTrue(subScope.CostBatch.CostsEqual(copy.CostBatch));
        }

        [TestMethod()]
        public void AddPointToConnected()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddDeviceToConnected()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemovePointFromConnected()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemoveDeviceFromConnected()
        {
            Assert.Fail();
        }
    }
}