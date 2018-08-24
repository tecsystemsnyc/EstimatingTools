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
    public class TECProvidedControllerTests
    {

        [TestMethod()]
        public void CopyControllerTest()
        {
            Random rand = new Random(0);

            TECBid bid = ModelCreation.TestBid(rand);
            TECProvidedController controller = ModelCreation.TestProvidedController(bid.Catalogs, rand);
            bid.AddController(controller);

            TECProvidedController copy = controller.CopyController(new Dictionary<Guid, Guid>()) as TECProvidedController;

            Assert.AreEqual(controller.Type, copy.Type);
            Assert.IsTrue(controller.IOModules.SequenceEqual(copy.IOModules));
            Assert.AreEqual(controller.ChildrenConnections.Count, copy.ChildrenConnections.Count);
        }

        [TestMethod()]
        public void CanAddModuleTest()
        {
            TECIOModule module = new TECIOModule(new TECManufacturer());
            
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            cType.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(cType);
            Assert.IsTrue(controller.CanAddModule(module));
            controller.AddModule(module);
            Assert.IsFalse(controller.CanAddModule(module));

        }

        [TestMethod()]
        public void AddModuleTest()
        {
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AI));

            TECControllerType cType = new TECControllerType(new TECManufacturer());
            cType.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(cType);

            Assert.IsFalse(controller.IO.Contains(IOType.AI));

            controller.AddModule(module);

            Assert.IsTrue(controller.IOModules.Contains(module));
            Assert.IsTrue(controller.IO.Contains(IOType.AI));
        }

        [TestMethod()]
        public void CanChangeTypeTest()
        {
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            cType.IO.Add(new TECIO(protocol));
            cType.IO.Add(new TECIO(IOType.AI));

            TECControllerType otherCType = new TECControllerType(new TECManufacturer());
            otherCType.IO.Add(new TECIO(protocol));
            otherCType.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(cType);

            TECSubScope ss1 = new TECSubScope();
            TECSubScope ss2 = new TECSubScope();

            TECDevice proDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECDevice hardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());

            ss1.Devices.Add(proDevice);
            ss2.Devices.Add(hardDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            ss2.AddPoint(point);

            controller.Connect(ss1, protocol);
            controller.Connect(ss2, ss2.HardwiredProtocol());

            Assert.IsTrue(controller.CanChangeType(otherCType));
        }

        [TestMethod()]
        public void CanChangeTypeTest1()
        {
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            cType.IO.Add(new TECIO(protocol));
            cType.IO.Add(new TECIO(IOType.AI));

            TECControllerType otherCType = new TECControllerType(new TECManufacturer());

            TECProvidedController controller = new TECProvidedController(cType);

            TECSubScope ss1 = new TECSubScope();
            TECSubScope ss2 = new TECSubScope();

            TECDevice proDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECDevice hardDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());

            ss1.Devices.Add(proDevice);
            ss2.Devices.Add(hardDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            ss2.AddPoint(point);

            controller.Connect(ss1, protocol);
            controller.Connect(ss2, ss2.HardwiredProtocol());

            Assert.IsFalse(controller.CanChangeType(otherCType));
        }

        [TestMethod()]
        public void ChangeTypeTest()
        {
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            cType.IO.Add(new TECIO(protocol));
            cType.IO.Add(new TECIO(IOType.AI));

            TECControllerType otherCType = new TECControllerType(new TECManufacturer());
            otherCType.IO.Add(new TECIO(protocol));
            otherCType.IO.Add(new TECIO(IOType.AI));

            TECProvidedController controller = new TECProvidedController(cType);

            TECSubScope ss1 = new TECSubScope();
            TECSubScope ss2 = new TECSubScope();

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice proDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECDevice hardDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            ss1.Devices.Add(proDevice);
            ss2.Devices.Add(hardDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            ss2.AddPoint(point);

            controller.Connect(ss1, protocol);
            controller.Connect(ss2, ss2.HardwiredProtocol());

            controller.ChangeType(otherCType);

            Assert.AreEqual(2, controller.ChildrenConnections.Count);
            Assert.AreEqual(controller, (ss1 as IConnectable).GetParentConnection().ParentController);
            Assert.AreEqual(controller, (ss2 as IConnectable).GetParentConnection().ParentController);
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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));

            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));

            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);


            Assert.IsFalse(controller.CanConnect(subScope));

        }

        [TestMethod()]
        public void CanConnectTest2()
        {
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AI));
            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AO));
            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));
            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));
            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);


            Assert.IsFalse(controller.CanConnect(subScope, firstProtocol));
            Assert.IsFalse(controller.CanConnect(subScope, fourthProtocol));

        }

        [TestMethod()]
        public void CanConnectTest6()
        {
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AI));
            type.IOModules.Add(module);


            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(controller.CanConnect(subScope, subScope.HardwiredProtocol()));

        }

        [TestMethod()]
        public void CanConnectTest7()
        {
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AO));
            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));
            type.IOModules.Add(module);

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
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            module.IO.Add(new TECIO(secondProtocol));
            module.IO.Add(new TECIO(thirdProtocol));
            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);
            var connection = controller.Connect(subScope, firstProtocol);

            Assert.IsNull(connection);

        }

        [TestMethod()]
        public void ConnectTest2()
        {
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AI));
            type.IOModules.Add(module);

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
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AO));
            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);
            TECHardwiredConnection connection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;

            Assert.IsNull(connection);
        }

        [TestMethod()]
        public void ConnectTest4()
        {
            TECConnectionType connectionType = new TECConnectionType();
            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(compatibleDevice);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            point.Quantity = 3;
            subScope.Points.Add(point);

            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(IOType.AI));
            type.IOModules.Add(module);
            type.IOModules.Add(module);
            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);

            TECHardwiredConnection connection = controller.Connect(subScope, subScope.HardwiredProtocol()) as TECHardwiredConnection;

            Assert.IsTrue(connection.Child == subScope);
            Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
            Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
            Assert.IsTrue(connection.Protocol is TECHardwiredProtocol);
        }

        [TestMethod()]
        public void ConnectTest5()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope1 = new TECSubScope();
            subScope1.Devices.Add(compatibleDevice);
            TECSubScope subScope2 = new TECSubScope();
            subScope2.Devices.Add(compatibleDevice);
            TECSubScope subScope3 = new TECSubScope();
            subScope3.Devices.Add(compatibleDevice);
            TECSubScope subScope4 = new TECSubScope();
            subScope4.Devices.Add(compatibleDevice);

            List<IConnectable> connectables = new List<IConnectable>
            {
                subScope1,
                subScope2,
                subScope3,
                subScope4
            };


            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));
            type.IOModules.Add(module);

            TECProvidedController controller = new TECProvidedController(type);

            foreach(var subScope in connectables)
            {
                TECNetworkConnection connection = controller.Connect(subScope, firstProtocol) as TECNetworkConnection;

                Assert.IsTrue(connection.Children.Contains(subScope));
                Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
                Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
                Assert.IsTrue(connection.Protocol == firstProtocol);
            }
        }

        [TestMethod()]
        public void ConnectTest6()
        {
            TECProtocol firstProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol secondProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol thirdProtocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol fourthProtocol = new TECProtocol(new List<TECConnectionType>());

            TECConnectionType connectionType = new TECConnectionType();
            TECDevice hardDevice = new TECDevice(new List<TECConnectionType> { connectionType }, new List<TECProtocol>(), new TECManufacturer());

            TECSubScope hSubScope1 = new TECSubScope();
            hSubScope1.Devices.Add(hardDevice);
            hSubScope1.Points.Add(new TECPoint { Type = IOType.AI });
            
            TECSubScope hSubScope2 = new TECSubScope();
            hSubScope2.Devices.Add(hardDevice);
            hSubScope2.Points.Add(new TECPoint { Type = IOType.AI });

            TECSubScope hSubScope3 = new TECSubScope();
            hSubScope3.Devices.Add(hardDevice);
            hSubScope3.Points.Add(new TECPoint { Type = IOType.AI });
            
            TECSubScope hSubScope4 = new TECSubScope();
            hSubScope4.Devices.Add(hardDevice);
            hSubScope4.Points.Add(new TECPoint { Type = IOType.AI });
            
            TECSubScope hSubScope5 = new TECSubScope();
            hSubScope5.Devices.Add(hardDevice);
            hSubScope5.Points.Add(new TECPoint { Type = IOType.AI });


            TECDevice compatibleDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { secondProtocol, firstProtocol, fourthProtocol }, new TECManufacturer());
            TECSubScope subScope1 = new TECSubScope();
            subScope1.Devices.Add(compatibleDevice);
            TECSubScope subScope2 = new TECSubScope();
            subScope2.Devices.Add(compatibleDevice);
            TECSubScope subScope3 = new TECSubScope();
            subScope3.Devices.Add(compatibleDevice);
            TECSubScope subScope4 = new TECSubScope();
            subScope4.Devices.Add(compatibleDevice);

            List<IConnectable> connectables = new List<IConnectable>
            {
                subScope1,
                subScope2,
                subScope3,
                subScope4,
                hSubScope1,
                hSubScope2,
                hSubScope3,
                hSubScope4,
                hSubScope5
            };


            TECControllerType type = new TECControllerType(new TECManufacturer());
            TECIOModule module = new TECIOModule(new TECManufacturer());
            module.IO.Add(new TECIO(firstProtocol));

            TECIOModule hModule = new TECIOModule(new TECManufacturer());
            hModule.IO.Add(new TECIO(IOType.AI) { Quantity = 2 });

            type.IOModules.Add(module);
            type.IOModules.Add(hModule);
            type.IOModules.Add(hModule);
            type.IOModules.Add(hModule);
            
            TECProvidedController controller = new TECProvidedController(type);

            foreach (var subScope in connectables)
            {
                IProtocol protocol = subScope.HardwiredProtocol() as IProtocol ?? firstProtocol;
                IControllerConnection connection = controller.Connect(subScope, protocol);

                bool containsChild = connection is TECNetworkConnection netConnect ?
                    netConnect.Children.Contains(subScope) : (connection as TECHardwiredConnection).Child == subScope;

                Assert.IsTrue(containsChild);
                Assert.IsTrue(controller.ChildrenConnections.Contains(connection));
                Assert.IsTrue((subScope as IConnectable).GetParentConnection() == connection);
                Assert.AreEqual(connection.Protocol, protocol);
            }
        }
    }
}