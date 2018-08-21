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
    public class TECElectricalMaterialTests
    {

        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECBid bid = new TECBid();
            TECElectricalMaterial dev = new TECElectricalMaterial();
            bid.Catalogs.Add(dev);
            var copy = dev.DropData();

            Assert.AreEqual(dev, copy);
        }

        [TestMethod()]
        public void GetCostsTest()
        {
            TECElectricalMaterial type = new TECElectricalMaterial();
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
            
        }

        [TestMethod()]
        public void CatalogCopyTest()
        {
            Random rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECElectricalMaterial mat = ModelCreation.TestElectricalMaterial(catalogs, rand, "conduit");
            var copy = mat.CatalogCopy();

            Assert.AreNotEqual(mat.Guid, copy.Guid);
            Assert.AreEqual(mat.Name, copy.Name);
            Assert.AreEqual(mat.Description, copy.Description);
            Assert.AreEqual(mat.Cost, copy.Cost);
        }
    }
}