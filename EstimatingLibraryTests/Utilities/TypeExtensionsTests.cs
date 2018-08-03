using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;
using EstimatingLibrary.Interfaces;

namespace Utilities
{
    [TestClass()]
    public class TypeExtensionsTests
    {
        [TestMethod()]
        public void IsImplementationOfTest()
        {
            Assert.IsTrue(typeof(TECScope).IsImplementationOf(typeof(ITECScope)));
            Assert.IsTrue(typeof(TECValve).IsImplementationOf(typeof(ICatalog<TECValve>)));
        }

        [TestMethod()]
        public void ForEachTest()
        {
            int x = 0;
            List<string> list = new List<string>() { "first", "second" };
            list.ForEach(item => x += 1);
            Assert.AreEqual(2, x);
        }
    }
}