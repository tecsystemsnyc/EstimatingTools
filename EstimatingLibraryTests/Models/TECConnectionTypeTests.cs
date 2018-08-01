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
    public class TECConnectionTypeTests
    {
        [TestMethod()]
        public void GetCostsTest()
        {
            TECConnectionType type = new TECConnectionType();
            TECAssociatedCost aCost = new TECAssociatedCost(CostType.TEC);
            aCost.Cost = 3;
            TECAssociatedCost rCost = new TECAssociatedCost(CostType.TEC);
            rCost.Cost = 5;
            type.Cost = 2;
            type.Labor = 1;

            type.AssociatedCosts.Add(aCost);
            type.RatedCosts.Add(rCost);

            var costs = type.GetCosts(2);
            Assert.AreEqual(costs.GetCost(CostType.TEC), 15);
            Assert.AreEqual(costs.GetLabor(CostType.TEC), 2);

            type.PlenumCost = 1;
            type.PlenumLabor = 1;
            costs = type.GetCosts(2, true);
            Assert.AreEqual(costs.GetCost(CostType.TEC), 17);
            Assert.AreEqual(costs.GetLabor(CostType.TEC), 4);

        }
    }
}