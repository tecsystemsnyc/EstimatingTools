using Microsoft.VisualStudio.TestTools.UnitTesting;
using TECUserControlLibrary.Utilities;
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
    public class SearchUtilitiesTests
    {
        [TestMethod()]
        public void GetSearchResultTest()
        {
            TECSubScope subScope1 = new TECSubScope();
            subScope1.Name = "Test 1";
            TECSubScope subScope2 = new TECSubScope();
            subScope2.Name = "Thingy";

            List<ITECObject> obj = new List<ITECObject>
            {
                subScope1,
                subScope2
            };

            var results = SearchUtilities.GetSearchResult(obj, "test");
            Assert.IsTrue(results.Contains(subScope1));
        }
    }
}