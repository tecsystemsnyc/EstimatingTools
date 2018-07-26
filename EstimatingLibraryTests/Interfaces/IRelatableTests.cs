using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using EstimatingLibrary;
using TestLibrary.ModelTestingUtilities;

namespace Interfaces
{
    [TestClass()]
    public class IRelatableTests
    {
        Random rand;

        [TestInitialize()]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestMethod()]
        public void GetAllTest()
        {
            var test = new RelatableTest();
            test.PropertyObjects.Add(new TECMisc(CostType.TEC), "TestingDirect");
            test.PropertyObjects.Add(new TECLocation(), "TestingRelated");

            test.LinkedObjects.Add(new TECLocation(), "TestingRelated");
            
            Assert.IsTrue(test.GetAll<TECAssociatedCost>().Count == 0);
            Assert.IsTrue(test.GetAll<TECLocation>().Count == 0);

            Assert.IsTrue(test.GetAll<TECMisc>().Count == 1);
        }

        [TestMethod()]
        public void GetDirectChildrenTest()
        {
            var test = new RelatableTest();
            test.PropertyObjects.Add(new TECMisc(CostType.TEC), "TestingDirect");
            test.PropertyObjects.Add(new TECLocation(), "TestingRelated");

            test.LinkedObjects.Add(new TECLocation(), "TestingRelated");

            Assert.IsTrue(test.GetDirectChildren().Count == 1);
        }

        [TestMethod()]
        public void IsDirectChildPropertyTest()
        {
            var test = new RelatableTest();
            var misc = new TECMisc(CostType.TEC);
            test.PropertyObjects.Add(misc, "TestingDirect");
            var location = new TECLocation();
            test.PropertyObjects.Add(location, "TestingRelated");

            test.LinkedObjects.Add(location, "TestingRelated");

            Assert.IsTrue(test.IsDirectChildProperty("TestingDirect"));
            Assert.IsFalse(test.IsDirectChildProperty("TestingRelated"));
        }

        [TestMethod()]
        public void IsDirectDescendantTest()
        {
            //Arrange
            TECBid bid = ModelCreation.TestBid(rand);

            TECTypical typical = ModelCreation.TestTypical(bid.Catalogs, rand);

            TECSubScope newSS = ModelCreation.TestSubScope(bid.Catalogs, rand);

            //Act
            typical.AddInstance();

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
            TECBid bid = ModelCreation.TestBid(rand);
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

    class RelatableTest : IRelatable
    {
        public SaveableMap PropertyObjects { get; set; } = new SaveableMap();

        public SaveableMap LinkedObjects { get; set; } = new SaveableMap();

        public Guid Guid { get; set; } = new Guid();

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<TECChangedEventArgs> TECChanged;
    }
}