using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [TestClass()]
    public class TECAssociatedCostTests
    {

        [TestMethod()]
        public void CatalogCopyTest()
        {
            TECAssociatedCost cost = new TECAssociatedCost(CostType.TEC);
            TECAssociatedCost copiedCost = cost.CatalogCopy();

            Assert.AreEqual(cost.Name, copiedCost.Name);
            Assert.AreEqual(cost.Type, copiedCost.Type);
            Assert.AreEqual(cost.Labor, copiedCost.Labor);
            Assert.IsTrue(cost.Tags.SequenceEqual(copiedCost.Tags));

        }

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECAssociatedCost cost = new TECAssociatedCost(CostType.TEC);
            TECAssociatedCost copiedCost = cost.DragDropCopy(new TECBid()) as TECAssociatedCost;

            Assert.AreEqual(cost, copiedCost);
        }
    }
}