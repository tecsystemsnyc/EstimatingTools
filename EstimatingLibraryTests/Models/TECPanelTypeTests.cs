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
    public class TECPanelTypeTests
    {

        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECPanelType type = ModelCreation.TestPanelType(catalogs, rand);

            var copy = type.CatalogCopy();

            Assert.AreEqual(type.Manufacturer, copy.Manufacturer);
            Assert.AreEqual(type.Cost, copy.Cost);
        }
    }
}