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
    public class TECMiscTests
    {

        [TestMethod()]
        public void TECMiscCostBatchTest()
        {
            TECMisc misc = new TECMisc(CostType.TEC);
            misc.Quantity = 2;
            misc.Cost = 10;
            misc.Labor = 11;

            var cost = misc.CostBatch;

            Assert.AreEqual(20, cost.GetCost(CostType.TEC));
            Assert.AreEqual(22, cost.GetLabor(CostType.TEC));
        }

        [TestMethod()]
        public void DragDropCopyTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.TEC);

            TECMisc copy = misc.DropData() as TECMisc;

            Assert.AreEqual(misc.Name, copy.Name);
            Assert.AreEqual(misc.Type, copy.Type);
            Assert.IsTrue(misc.CostBatch.CostsEqual(copy.CostBatch));

        }
    }
}