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
    public class TECManufacturerTests
    {
        [TestMethod()]
        public void CatalogCopyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECManufacturer manufacturer = new TECManufacturer();
            var copy = manufacturer.DragDropCopy(new TECBid()) as TECManufacturer;

            Assert.AreEqual(manufacturer, copy);
        }
    }
}