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

            Assert.IsFalse(controller.AvailableIO.Contains(IOType.AI));

            controller.AddModule(module);

            Assert.IsTrue(controller.IOModules.Contains(module));
            Assert.IsTrue(controller.AvailableIO.Contains(IOType.AI));
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
    }
}