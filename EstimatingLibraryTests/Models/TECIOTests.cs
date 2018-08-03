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
    public class TECIOTests
    {
        [TestMethod()]
        public void GetUniversalTypeTest()
        {
            Assert.AreEqual(TECIO.GetUniversalType(IOType.AI), IOType.UI);
            Assert.AreEqual(TECIO.GetUniversalType(IOType.DI), IOType.UI);
            Assert.AreEqual(TECIO.GetUniversalType(IOType.AO), IOType.UO);
            Assert.AreEqual(TECIO.GetUniversalType(IOType.DO), IOType.UO);
        }
        
        //Constructor Tests
        [TestMethod()]
        public void TECIOTest()
        {
            TECIO io = new TECIO(IOType.AI);

            Assert.AreEqual(IOType.AI,io.Type);
            Assert.IsNull(io.Protocol);
            Assert.AreEqual(1, io.Quantity);
        }

        [TestMethod()]
        public void TECIOTest1()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECIO io = new TECIO(protocol);

            Assert.AreEqual(IOType.Protocol, io.Type);
            Assert.AreEqual(protocol, io.Protocol);
            Assert.AreEqual(1, io.Quantity);
        }

        [TestMethod()]
        public void TECIOSetType()
        {
            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());
            TECIO io = new TECIO(protocol);

            io.Type = IOType.AI;

            Assert.AreEqual(IOType.AI, io.Type);
            Assert.IsNull(io.Protocol);
            Assert.AreEqual(1, io.Quantity);
        }

        [TestMethod()]
        public void TECIOSetProtocol()
        {
            TECIO io = new TECIO(IOType.AI);

            TECProtocol protocol = new TECProtocol(new List<TECConnectionType>());

            io.Protocol = protocol;

            Assert.AreEqual(IOType.Protocol, io.Type);
            Assert.AreEqual(protocol, io.Protocol);
            Assert.AreEqual(1, io.Quantity);
        }
        
    }
}