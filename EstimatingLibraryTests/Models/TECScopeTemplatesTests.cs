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
    public class TECScopeTemplatesTests
    {

        [TestMethod()]
        public void FillTest()
        {
            var rand = new Random(0);
            var catalogs = ModelCreation.TestCatalogs(rand, 1);

            var first = ModelCreation.TestScopeTemplates(catalogs, rand);
            var second = ModelCreation.TestScopeTemplates(catalogs, rand);

            first.Unionize(second);
            foreach (var item in second.GetAll<ITECObject>().Where(x => !(x is TECScopeTemplates)))
            {
                Assert.IsTrue(first.GetAll<ITECObject>().Contains(item));
            }
        }

        [TestMethod()]
        public void UnionizeTest()
        {
            var rand = new Random(0);
            var bid = ModelCreation.TestBid(rand, 1);

            var first = bid.Templates;
            var second = ModelCreation.TestScopeTemplates(bid.Catalogs, rand);
            TECSystem testSystem = ModelCreation.TestSystem(bid.Catalogs, rand);
            second.SystemTemplates.Add(testSystem);

            TECSystem systemToOverwrite = new TECSystem(testSystem.Guid);
            first.SystemTemplates.Add(systemToOverwrite);

            first.Unionize(second);
            foreach (var item in second.GetAll<ITECObject>().Where(x => !(x is TECScopeTemplates)))
            {
                Assert.IsTrue(first.GetAll<ITECObject>().Contains(item));
            }

            Assert.IsTrue(first.SystemTemplates.Contains(testSystem));
            Assert.IsFalse(first.SystemTemplates.Contains(systemToOverwrite));
        }
    }
}