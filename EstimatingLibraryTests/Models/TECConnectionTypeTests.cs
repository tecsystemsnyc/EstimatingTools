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
            var tecExpected = 2 * (5) + 3;
            var elecExpected = 2 * (2);
            Assert.AreEqual(tecExpected, costs.GetCost(CostType.TEC));
            Assert.AreEqual(elecExpected, costs.GetCost(CostType.Electrical));
            var tecLaborExpected = 0;
            var elecLabroExpected = 2 * (1);
            Assert.AreEqual(tecLaborExpected, costs.GetLabor(CostType.TEC));
            Assert.AreEqual(elecLabroExpected, costs.GetLabor(CostType.Electrical));

            type.PlenumCost = 1;
            type.PlenumLabor = 1;
            costs = type.GetCosts(2, true);
            tecExpected = 2 * (5) + 3;
            elecExpected = 2 * (2 + 1);
            Assert.AreEqual(tecExpected, costs.GetCost(CostType.TEC));
            Assert.AreEqual(elecExpected, costs.GetCost(CostType.Electrical));
            tecLaborExpected = 0;
            elecLabroExpected = 2 * (1 + 1);
            Assert.AreEqual(tecLaborExpected, costs.GetLabor(CostType.TEC));
            Assert.AreEqual(elecLabroExpected, costs.GetLabor(CostType.Electrical));

        }
    }
}