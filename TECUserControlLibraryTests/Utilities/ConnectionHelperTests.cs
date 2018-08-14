using Microsoft.VisualStudio.TestTools.UnitTesting;
using TECUserControlLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;

namespace Utilities
{
    [TestClass()]
    public class ConnectionHelperTests
    {
        [TestMethod()]
        public void CanConnectToControllerTest()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECDevice protDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());

            List<IConnectable> connectables = new List<IConnectable>();

            TECSubScope item1 = new TECSubScope();
            item1.Devices.Add(protDevice);
            connectables.Add(item1);
            TECSubScope item2 = new TECSubScope();
            item2.Devices.Add(protDevice);
            connectables.Add(item2);
            TECSubScope item3 = new TECSubScope();
            item3.Devices.Add(protDevice);
            connectables.Add(item3);
            TECSubScope item4 = new TECSubScope();
            item4.Devices.Add(protDevice);
            connectables.Add(item4);
            TECSubScope item5 = new TECSubScope();
            item5.Devices.Add(protDevice);
            connectables.Add(item5);
            TECSubScope item6 = new TECSubScope();
            item6.Devices.Add(protDevice);
            connectables.Add(item6);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(ConnectionHelper.CanConnectToController(connectables, controller));
            
        }


        //Fails because a device always has a hardwired connection method. It needs to be decided if this is desireable behavior.
        [TestMethod()]
        public void CanConnectToControllerTest1()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECProtocol otherProtocol = new TECProtocol(new List<TECConnectionType>());

            TECDevice protDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { protocol }, new TECManufacturer());
            TECDevice otherProtDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>() { otherProtocol }, new TECManufacturer());

            List<IConnectable> connectables = new List<IConnectable>();

            TECSubScope item1 = new TECSubScope();
            item1.Devices.Add(otherProtDevice);
            connectables.Add(item1);
            TECSubScope item2 = new TECSubScope();
            item2.Devices.Add(protDevice);
            connectables.Add(item2);
            TECSubScope item3 = new TECSubScope();
            item3.Devices.Add(protDevice);
            connectables.Add(item3);
            TECSubScope item4 = new TECSubScope();
            item4.Devices.Add(protDevice);
            connectables.Add(item4);
            TECSubScope item5 = new TECSubScope();
            item5.Devices.Add(protDevice);
            connectables.Add(item5);
            TECSubScope item6 = new TECSubScope();
            item6.Devices.Add(protDevice);
            connectables.Add(item6);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(protocol));
            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsFalse(ConnectionHelper.CanConnectToController(connectables, controller));

        }

        [TestMethod()]
        public void CanConnectToControllerTest2()
        {
            TECDevice protDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());

            List<IConnectable> connectables = new List<IConnectable>();

            TECSubScope item1 = new TECSubScope();
            item1.Devices.Add(protDevice);
            TECPoint aiPoint = new TECPoint();
            aiPoint.Type = IOType.AI;
            item1.AddPoint(aiPoint);
            connectables.Add(item1);
            TECSubScope item2 = new TECSubScope();
            item2.Devices.Add(protDevice);
            TECPoint diPoint = new TECPoint();
            diPoint.Type = IOType.DI;
            item2.AddPoint(diPoint);
            connectables.Add(item2);
            TECSubScope item3 = new TECSubScope();
            item3.Devices.Add(protDevice);
            TECPoint doPoint = new TECPoint();
            doPoint.Type = IOType.DO;
            item3.AddPoint(doPoint);
            connectables.Add(item3);
            TECSubScope item4 = new TECSubScope();
            item4.Devices.Add(protDevice);
            TECPoint aoPoint = new TECPoint();
            aoPoint.Type = IOType.AO;
            item4.AddPoint(aoPoint);
            connectables.Add(item4);
            TECSubScope item5 = new TECSubScope();
            item5.Devices.Add(protDevice);
            TECPoint otherAiPoint = new TECPoint();
            otherAiPoint.Type = IOType.AI;
            item5.AddPoint(otherAiPoint);
            connectables.Add(item5);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.UI));
            type.IO.Add(new TECIO(IOType.UI));
            type.IO.Add(new TECIO(IOType.UO));
            type.IO.Add(new TECIO(IOType.UO));
            type.IO.Add(new TECIO(IOType.UI));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsTrue(ConnectionHelper.CanConnectToController(connectables, controller));

        }
        [TestMethod()]
        public void CanConnectToControllerTest3()
        {
            TECDevice protDevice = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());

            List<IConnectable> connectables = new List<IConnectable>();

            TECSubScope item1 = new TECSubScope();
            item1.Devices.Add(protDevice);
            TECPoint aiPoint = new TECPoint();
            aiPoint.Type = IOType.AI;
            item1.AddPoint(aiPoint);
            connectables.Add(item1);
            TECSubScope item2 = new TECSubScope();
            item2.Devices.Add(protDevice);
            TECPoint diPoint = new TECPoint();
            diPoint.Type = IOType.DI;
            item2.AddPoint(diPoint);
            connectables.Add(item2);
            TECSubScope item3 = new TECSubScope();
            item3.Devices.Add(protDevice);
            TECPoint doPoint = new TECPoint();
            doPoint.Type = IOType.DO;
            item3.AddPoint(doPoint);
            connectables.Add(item3);
            TECSubScope item4 = new TECSubScope();
            item4.Devices.Add(protDevice);
            TECPoint aoPoint = new TECPoint();
            aoPoint.Type = IOType.AO;
            item4.AddPoint(aoPoint);
            connectables.Add(item4);
            TECSubScope item5 = new TECSubScope();
            item5.Devices.Add(protDevice);
            TECPoint otherAoPoint = new TECPoint();
            otherAoPoint.Type = IOType.AO;
            item5.AddPoint(otherAoPoint);
            connectables.Add(item5);


            TECControllerType type = new TECControllerType(new TECManufacturer());
            type.IO.Add(new TECIO(IOType.UI));
            type.IO.Add(new TECIO(IOType.UI));
            type.IO.Add(new TECIO(IOType.UO));
            type.IO.Add(new TECIO(IOType.UO));
            type.IO.Add(new TECIO(IOType.UI));

            TECProvidedController controller = new TECProvidedController(type);

            Assert.IsFalse(ConnectionHelper.CanConnectToController(connectables, controller));

        }


        [TestMethod()]
        public void ConnectToControllerTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetConnectablesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExistingNetworkIOTest()
        {
            Assert.Fail();
        }
    }
}