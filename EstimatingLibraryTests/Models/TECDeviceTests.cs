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
    public class TECDeviceTests
    {

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECBid bid = new TECBid();
            TECDevice dev = new TECDevice(new List<TECConnectionType>(), new List<TECProtocol>(), new TECManufacturer());
            bid.Catalogs.Add(dev);
            var copy = dev.DropData();

            Assert.AreEqual(dev, copy);
        }

        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECDevice dev = ModelCreation.TestDevice(catalogs, rand);
            var copy = dev.CatalogCopy();

            Assert.AreNotEqual(dev.Guid, copy.Guid);
            Assert.AreEqual(dev.Name, copy.Name);
            Assert.AreEqual(dev.Description, copy.Description);
            Assert.AreEqual(dev.Price, copy.Price);
            Assert.AreEqual(dev.Manufacturer, copy.Manufacturer);

        }
    }
}