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
    public class CostBatchTests
    {
        [TestMethod()]
        public void CostBatchTest()
        {
            CostBatch cost = new CostBatch();

            foreach(CostType item in Enum.GetValues(typeof(CostType)))
            {
                Assert.AreEqual(0, cost.GetCost(item));
                Assert.AreEqual(0, cost.GetLabor(item));
            }
        }

        [TestMethod()]
        public void CostBatchTest1()
        {
            TECAssociatedCost aCost = new TECAssociatedCost(CostType.TEC);
            aCost.Cost = 10;
            aCost.Labor = 11;

            CostBatch cost = new CostBatch(aCost);

            Assert.AreEqual(10, cost.GetCost(CostType.TEC));
            Assert.AreEqual(11, cost.GetLabor(CostType.TEC));

        }

        [TestMethod()]
        public void CostBatchTest2()
        {
            TECAssociatedCost aCost1 = new TECAssociatedCost(CostType.TEC);
            aCost1.Cost = 10;
            aCost1.Labor = 11;

            TECAssociatedCost aCost2 = new TECAssociatedCost(CostType.TEC);
            aCost2.Cost = 10;
            aCost2.Labor = 11;

            TECAssociatedCost aCost3 = new TECAssociatedCost(CostType.Electrical);
            aCost3.Cost = 10;
            aCost3.Labor = 11;

            CostBatch cost = new CostBatch(new List<ICost>() { aCost1, aCost2, aCost3 });

            Assert.AreEqual(20, cost.GetCost(CostType.TEC));
            Assert.AreEqual(22, cost.GetLabor(CostType.TEC));

            Assert.AreEqual(10, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(11, cost.GetLabor(CostType.Electrical));
        }

        [TestMethod()]
        public void CostBatchTest3()
        {
            TECAssociatedCost aCost1 = new TECAssociatedCost(CostType.TEC);
            aCost1.Cost = 10;
            aCost1.Labor = 11;

            TECAssociatedCost aCost2 = new TECAssociatedCost(CostType.TEC);
            aCost2.Cost = 10;
            aCost2.Labor = 11;

            TECAssociatedCost aCost3 = new TECAssociatedCost(CostType.Electrical);
            aCost3.Cost = 10;
            aCost3.Labor = 11;

            CostBatch ogCost = new CostBatch(new List<ICost>() { aCost1, aCost2, aCost3 });


            CostBatch cost = new CostBatch(ogCost);

            Assert.AreEqual(20, cost.GetCost(CostType.TEC));
            Assert.AreEqual(22, cost.GetLabor(CostType.TEC));

            Assert.AreEqual(10, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(11, cost.GetLabor(CostType.Electrical));
        }

        [TestMethod()]
        public void CostBatchTest4()
        {
            CostBatch cost = new CostBatch(10, 11, CostType.TEC);

            Assert.AreEqual(10, cost.GetCost(CostType.TEC));
            Assert.AreEqual(11, cost.GetLabor(CostType.TEC));
        }

        [TestMethod()]
        public void GetCostTest()
        {
            CostBatch cost = new CostBatch(10, 0, CostType.TEC);

            Assert.AreEqual(10, cost.GetCost(CostType.TEC));

            cost = new CostBatch(10, 0, CostType.Electrical);

            Assert.AreEqual(10, cost.GetCost(CostType.Electrical));
        }

        [TestMethod()]
        public void GetLaborTest()
        {
            CostBatch cost = new CostBatch(0, 11, CostType.TEC);

            Assert.AreEqual(11, cost.GetLabor(CostType.TEC));

            cost = new CostBatch(0, 11, CostType.Electrical);

            Assert.AreEqual(11, cost.GetLabor(CostType.Electrical));
        }

        [TestMethod()]
        public void AddCostTest()
        {
            CostBatch cost = new CostBatch();

            TECAssociatedCost aCost1 = new TECAssociatedCost(CostType.TEC);
            aCost1.Cost = 10;
            aCost1.Labor = 11;

            TECAssociatedCost aCost2 = new TECAssociatedCost(CostType.Electrical);
            aCost2.Cost = 10;
            aCost2.Labor = 11;

            cost.AddCost(aCost1);
            Assert.AreEqual(10, cost.GetCost(CostType.TEC));
            Assert.AreEqual(11, cost.GetLabor(CostType.TEC));

            cost.AddCost(aCost2);
            Assert.AreEqual(10, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(11, cost.GetLabor(CostType.Electrical));

        }

        [TestMethod()]
        public void RemoveCostTest()
        {
            TECAssociatedCost aCost1 = new TECAssociatedCost(CostType.TEC);
            aCost1.Cost = 10;
            aCost1.Labor = 11;

            TECAssociatedCost aCost2 = new TECAssociatedCost(CostType.Electrical);
            aCost2.Cost = 10;
            aCost2.Labor = 11;

            CostBatch cost = new CostBatch(new List<ICost>() { aCost1, aCost2 });

            cost.RemoveCost(aCost1);
            Assert.AreEqual(0, cost.GetCost(CostType.TEC));
            Assert.AreEqual(0, cost.GetLabor(CostType.TEC));

            cost.RemoveCost(aCost2);
            Assert.AreEqual(0, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(0, cost.GetLabor(CostType.Electrical));

        }

        [TestMethod()]
        public void AddTest()
        {
            CostBatch cost = new CostBatch();

            cost.Add(CostType.TEC, 10, 11);
            Assert.AreEqual(10, cost.GetCost(CostType.TEC));
            Assert.AreEqual(11, cost.GetLabor(CostType.TEC));

            cost.Add(CostType.Electrical, 12, 13);
            Assert.AreEqual(12, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(13, cost.GetLabor(CostType.Electrical));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            TECAssociatedCost aCost1 = new TECAssociatedCost(CostType.TEC);
            aCost1.Cost = 10;
            aCost1.Labor = 11;

            TECAssociatedCost aCost2 = new TECAssociatedCost(CostType.Electrical);
            aCost2.Cost = 12;
            aCost2.Labor = 13;

            CostBatch cost = new CostBatch(new List<ICost>() { aCost1, aCost2 });

            cost.Remove(CostType.TEC, 10, 11);
            Assert.AreEqual(0, cost.GetCost(CostType.TEC));
            Assert.AreEqual(0, cost.GetLabor(CostType.TEC));

            cost.Remove(CostType.Electrical, 12, 13);
            Assert.AreEqual(0, cost.GetCost(CostType.Electrical));
            Assert.AreEqual(0, cost.GetLabor(CostType.Electrical));
        }
    }
}