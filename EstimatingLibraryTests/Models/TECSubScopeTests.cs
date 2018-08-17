using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;
using EstimatingLibrary.Interfaces;

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
        public void AddPointToConnectedHardwired()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());

            type.IO.Add(new TECIO(IOType.AI));
            TECProvidedController controller = new TECProvidedController(type);

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice device = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            controller.Connect(subScope, subScope.HardwiredProtocol());
            
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.AddPoint(point);

            Assert.IsTrue(subScope.Points.Contains(point));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 0);


            TECPoint otherPoint = new TECPoint();
            otherPoint.Type = IOType.AI;

            subScope.AddPoint(otherPoint);
            Assert.IsFalse(subScope.Points.Contains(otherPoint));
        }

        [TestMethod()]
        public void AddPointToConnectedNetwork()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());

            type.IO.Add(new TECIO(IOType.AI));
            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { protocol }, new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            controller.Connect(subScope, protocol);

            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.AddPoint(point);

            Assert.IsTrue(subScope.Points.Contains(point));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 1);
            
            TECPoint otherPoint = new TECPoint();
            otherPoint.Type = IOType.AI;

            subScope.AddPoint(otherPoint);
            Assert.IsTrue(subScope.Points.Contains(otherPoint));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 1);

        }
        
        [TestMethod()]
        public void AddDeviceToConnectedHardwired()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());

            type.IO.Add(new TECIO(IOType.AI));
            TECProvidedController controller = new TECProvidedController(type);

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice device = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.AddDevice(device);
            
            controller.Connect(subScope, subScope.HardwiredProtocol());

            TECDevice otherDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            subScope.AddDevice(otherDevice);

            Assert.IsTrue(subScope.Devices.Contains(otherDevice));

            TECConnectionType otherConnectionType = new TECConnectionType();
            TECDevice nextDevice = new TECDevice(new List<TECConnectionType> { otherConnectionType }, new List<TECProtocol>(), new TECManufacturer());
            bool deviceAdded = subScope.AddDevice(nextDevice);

            Assert.IsFalse(deviceAdded);
            Assert.IsFalse(subScope.Devices.Contains(nextDevice));

        }

        [TestMethod()]
        public void AddDeviceToConnectedNetwork()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());

            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { protocol }, new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            controller.Connect(subScope, protocol);

            TECDevice otherDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { protocol }, new TECManufacturer());
            
            subScope.AddDevice(otherDevice);
            Assert.IsTrue(subScope.Devices.Contains(otherDevice));

            TECProtocol otherProtocol = new TECProtocol(new List<TECConnectionType>());
            TECDevice nextDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { otherProtocol }, new TECManufacturer());

            subScope.AddDevice(nextDevice);
            Assert.IsFalse(subScope.Devices.Contains(nextDevice));

        }

        [TestMethod()]
        public void RemovePointFromConnectedHardwired()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());

            type.IO.Add(new TECIO(IOType.AI));
            TECProvidedController controller = new TECProvidedController(type);

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice device = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);
            
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.AddPoint(point);

            controller.Connect(subScope, subScope.HardwiredProtocol());
            
            subScope.RemovePoint(point);

            Assert.IsFalse(subScope.Points.Contains(point));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 1);
            
        }

        [TestMethod()]
        public void RemovePointFromConnectedNetwork()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());

            type.IO.Add(new TECIO(IOType.AI));
            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { protocol }, new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);
            
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.AddPoint(point);

            TECPoint otherPoint = new TECPoint();
            otherPoint.Type = IOType.AI;

            controller.Connect(subScope, protocol);

            subScope.RemovePoint(point);
            
            Assert.IsFalse(subScope.Points.Contains(point));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 1);
            
            subScope.RemovePoint(otherPoint);
            Assert.IsFalse(subScope.Points.Contains(otherPoint));
            Assert.IsTrue(controller.AvailableIO.IONumber(IOType.AI) == 1);

        }

        [TestMethod()]
        public void RemoveDeviceFromConnectedHardwired()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());

            TECProvidedController controller = new TECProvidedController(type);

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice device = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            TECHardwiredConnection connection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;
            
            subScope.RemoveDevice(device);

            Assert.IsNull((subScope as IConnectable).GetParentConnection());
            Assert.IsFalse(controller.ChildrenConnections.Contains(connection));
            
        }

        [TestMethod()]
        public void RemoveDeviceFromConnectedNetwork()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());

            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol> { protocol }, new TECManufacturer());

            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);
            
            IControllerConnection connection = controller.Connect(subScope, protocol);
            
            subScope.RemoveDevice(device);

            Assert.IsNull((subScope as IConnectable).GetParentConnection());
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));

        }
    }
}