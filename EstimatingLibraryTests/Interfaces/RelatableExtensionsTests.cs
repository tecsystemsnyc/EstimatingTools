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

        [TestMethod()]
        public void GetObjectPathTest()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();
            TECSystem sys = bid.Systems[0];
            TECEquipment equip = sys.Equipment[0];
            TECSubScope ss = equip.SubScope[0];
            TECPoint point = ss.Points[0];
            TECController bidController = bid.Controllers[0];
            TECController sysController = sys.Controllers[0];

            //Act
            List<ITECObject> bidToPointPath = bid.GetObjectPath(point);
            List<ITECObject> bidToBidControllerPath = bid.GetObjectPath(bidController);
            List<ITECObject> bidToSysControllerPath = bid.GetObjectPath(sysController);
            List<ITECObject> sysToControllerPath = sys.GetObjectPath(sysController);
            List<ITECObject> noPath = sys.GetObjectPath(bidController);

            //Assert

            //Bid to Point Path
            Assert.AreEqual(bid, bidToPointPath[0]);
            Assert.AreEqual(sys, bidToPointPath[1]);
            Assert.AreEqual(equip, bidToPointPath[2]);
            Assert.AreEqual(ss, bidToPointPath[3]);
            Assert.AreEqual(point, bidToPointPath[4]);

            Assert.AreEqual(5, bidToPointPath.Count);

            //Bid to Bid Controller Path
            Assert.AreEqual(bid, bidToBidControllerPath[0]);
            Assert.AreEqual(bidController, bidToBidControllerPath[1]);

            Assert.AreEqual(2, bidToBidControllerPath.Count);

            //Bid to System Controller Path
            Assert.AreEqual(bid, bidToSysControllerPath[0]);
            Assert.AreEqual(sys, bidToSysControllerPath[1]);
            Assert.AreEqual(sysController, bidToSysControllerPath[2]);

            Assert.AreEqual(3, bidToSysControllerPath.Count);

            //System to Controller Path
            Assert.AreEqual(sys, sysToControllerPath[0]);
            Assert.AreEqual(sysController, sysToControllerPath[1]);

            Assert.AreEqual(2, sysToControllerPath.Count);
        }
    }
}