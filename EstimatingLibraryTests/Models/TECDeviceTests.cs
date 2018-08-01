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
    public class TECDeviceTests
    {

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECDevice dev = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            var copy = dev.DragDropCopy(new TECBid());

            Assert.AreEqual(dev, copy);
        }

        [TestMethod()]
        public void CatalogCopyTest()
        {
            Assert.Fail();
        }
    }
}