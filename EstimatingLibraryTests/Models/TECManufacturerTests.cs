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
    public class TECManufacturerTests
    {
        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECManufacturer man = ModelCreation.TestManufacturer(rand);
            var copy = man.CatalogCopy();

            Assert.AreNotEqual(man.Guid, copy.Guid);
            Assert.AreEqual(man.Label, copy.Label);
            Assert.AreEqual(man.Multiplier, copy.Multiplier);
        }

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECManufacturer manufacturer = new TECManufacturer();
            var copy = manufacturer.DropData() as TECManufacturer;

            Assert.AreEqual(manufacturer, copy);
        }
    }
}