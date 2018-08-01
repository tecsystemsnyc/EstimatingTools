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
    public class TECHardwiredProtocolTests
    {

        [TestMethod()]
        public void EqualsTest()
        {
            TECConnectionType type1 = new TECConnectionType();
            TECConnectionType type2 = new TECConnectionType();

            TECHardwiredProtocol first = new TECHardwiredProtocol(new List<TECConnectionType>() { type1, type2 });
            TECHardwiredProtocol second = new TECHardwiredProtocol(new List<TECConnectionType>() { type1, type2 });
            Assert.AreEqual(first, second);
        }
        
        [TestMethod()]
        public void GetHashCodeTest()
        {
            TECConnectionType type1 = new TECConnectionType();
            TECConnectionType type2 = new TECConnectionType();

            TECHardwiredProtocol first = new TECHardwiredProtocol(new List<TECConnectionType>() { type1, type2 });
            TECHardwiredProtocol second = new TECHardwiredProtocol(new List<TECConnectionType>() { type1, type2 });
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}