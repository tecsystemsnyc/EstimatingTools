using Microsoft.VisualStudio.TestTools.UnitTesting;
using TECUserControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;

namespace Utilities
{
    [TestClass()]
    public class ConnectableFilterTests
    {
        [TestMethod()]
        public void PassesFilterTest()
        {
            ConnectableFilter filter = new ConnectableFilter();
            filter.FilterLocation = new TECLocation();


            TECSubScope subScope = new TECSubScope();
            Assert.IsFalse(filter.PassesFilter(subScope));
        }
    }
}