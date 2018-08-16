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
    public class TECIOModuleTests
    {
        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);

            TECIOModule module = ModelCreation.TestIOModule(bid.Catalogs, rand);

            var copy = module.CatalogCopy();

            Assert.AreEqual(module.Name, copy.Name);
            Assert.AreEqual(module.Manufacturer, copy.Manufacturer);
            Assert.AreEqual(module.Cost, copy.Cost);
            Assert.AreEqual(module.IO.Count, copy.IO.Count);
        }
    }
}