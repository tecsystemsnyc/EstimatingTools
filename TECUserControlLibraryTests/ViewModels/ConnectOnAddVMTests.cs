using Microsoft.VisualStudio.TestTools.UnitTesting;
using TECUserControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;

namespace ViewModels
{
    [TestClass()]
    public class ConnectOnAddVMTests
    {
        [TestMethod()]
        public void UpdateTest()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            cType.IO.Add(new TECIO(protocol));

            TECProvidedController controller = new TECProvidedController(cType);

            TECProtocol otherProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            ConnectOnAddVM vm = new ConnectOnAddVM(new List<TECSubScope>() { subScope },
                new List<TECController> { controller },
                new List<TECElectricalMaterial>());
            
            vm.SelectedController = vm.ParentControllers.First();

            Assert.IsTrue(vm.CanConnect());

            TECDevice otherDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { otherProtocol }, new TECManufacturer());
            subScope.Devices.Remove(device);
            subScope.Devices.Add(otherDevice);

            vm.Update(new List<IConnectable>() { subScope });

            Assert.IsNull(vm.SelectedController);
            Assert.AreEqual(0, vm.ParentControllers.Count);

        }

        [TestMethod()]
        public void UpdateTest1()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            cType.IO.Add(new TECIO(protocol));

            TECProvidedController controller = new TECProvidedController(cType);

            TECProtocol otherProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { otherProtocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);

            ConnectOnAddVM vm = new ConnectOnAddVM(new List<TECSubScope>() { subScope },
                new List<TECController> { controller },
                new List<TECElectricalMaterial>());


            Assert.AreEqual(0, vm.ParentControllers.Count);

            TECDevice otherDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            subScope.Devices.Remove(device);
            subScope.Devices.Add(otherDevice);

            vm.Update(new List<IConnectable>() { subScope });

            Assert.AreEqual(1, vm.ParentControllers.Count);

        }
        
        [TestMethod()]
        public void CanConnectTest()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECControllerType cType = new TECControllerType(new TECManufacturer());
            cType.IO.Add(new TECIO(protocol));

            TECProvidedController controller = new TECProvidedController(cType);

            TECProtocol otherProtocol = new TECProtocol(new List<TECConnectionType>());
            TECControllerType otherCType = new TECControllerType(new TECManufacturer());
            otherCType.IO.Add(new TECIO(otherProtocol));

            TECProvidedController otherController = new TECProvidedController(otherCType);

            TECDevice device = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECSubScope subScope = new TECSubScope();
            subScope.Devices.Add(device);
            
            ConnectOnAddVM vm = new ConnectOnAddVM(new List<TECSubScope>() { subScope },
                new List<TECController> { controller, otherController },
                new List<TECElectricalMaterial>());

            Assert.AreEqual(1, vm.ParentControllers.Count);

            vm.SelectedController = null;
            vm.Connect = true;

            Assert.IsFalse(vm.CanConnect());

            vm.SelectedController = vm.ParentControllers.First();

            Assert.IsTrue(vm.CanConnect());
        }
    }
}