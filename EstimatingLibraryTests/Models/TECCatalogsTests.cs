using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;
using EstimatingLibrary.Interfaces;

namespace Models
{
    [TestClass()]
    public class TECCatalogsTests
    {
        [TestMethod()]
        public void UnionizeTest()
        {
            var rand = new Random(0);
            var first = ModelCreation.TestCatalogs(rand);
            var second = ModelCreation.TestCatalogs(rand);
            var firstDevice = first.Devices.RandomElement(rand);
            firstDevice.RequireQuote = false;
            firstDevice.QuotedPrice = -1;
            second.Add(firstDevice.CatalogCopy());
            firstDevice.RequireQuote = true;
            firstDevice.QuotedPrice = 12;

            first.Unionize(second);
            foreach (var item in second.GetAll<ITECObject>().Where(x => !(x is TECCatalogs)))
            {
                Assert.IsTrue(first.GetAll<ITECObject>().Contains(item));
            }

            var unionDevice = first.Devices.Where(x => x.Guid == firstDevice.Guid).First();
            Assert.AreEqual(12, unionDevice.QuotedPrice);
            Assert.AreEqual(true, unionDevice.RequireQuote);
            
        }

        [TestMethod()]
        public void FillTest()
        {
            var first = ModelCreation.TestCatalogs(new Random(0));
            var second = ModelCreation.TestCatalogs(new Random(1));

            first.Fill(second);
            foreach(var item in second.GetAll<ITECObject>().Where(x => !(x is TECCatalogs)))
            {
                Assert.IsTrue(first.GetAll<ITECObject>().Contains(item));
            }
        }
    }
}