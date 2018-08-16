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
    public class TECTagTests
    {

        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECTag tag = ModelCreation.TestTag(rand);
            var copy = tag.CatalogCopy();

            Assert.AreNotEqual(tag.Guid, copy.Guid);
            Assert.AreEqual(tag.Label, copy.Label);
        }
    }
}