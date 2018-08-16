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
    public class TECControllerTypeTests
    {

        [TestMethod()]
        public void CatalogCopyTest()
        {
            TECControllerType type = new TECControllerType(new TECManufacturer());
            var copy = type.CatalogCopy();
            Assert.AreEqual(type.Manufacturer, copy.Manufacturer);
            Assert.AreEqual(type.Cost, copy.Cost);
            Assert.AreEqual(type.Labor, copy.Labor);
            Assert.AreEqual(type.IOModules.Count, copy.IOModules.Count);
            Assert.AreEqual(type.IO.Count, copy.IO.Count);
        }
    }
}