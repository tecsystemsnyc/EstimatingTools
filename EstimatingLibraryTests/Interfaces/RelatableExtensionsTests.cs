using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace EstimatingLibrary.Interfaces.Tests
{
    [TestClass()]
    public class RelatableExtensionsTests
    {
        [TestMethod()]
        public void GetAllTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDirectChildrenTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsDirectChildPropertyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsDirectDescendantTest()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();

            TECTypical typical = TestHelper.CreateTestTypical(bid.Catalogs);

            TECSubScope newSS = TestHelper.CreateTestSubScope(false, bid.Catalogs);

            //Act
            typical.AddInstance(bid);

            TECSystem sys = typical.Instances[0];
            TECEquipment equip = sys.Equipment[0];
            TECSubScope ss = equip.SubScope[0];

            //Assert
            Assert.IsFalse(typical.IsDirectDescendant(newSS));

            Assert.IsTrue(typical.IsDirectDescendant(sys));
            Assert.IsTrue(typical.IsDirectDescendant(equip));
            Assert.IsTrue(typical.IsDirectDescendant(ss));
        }
    }
}