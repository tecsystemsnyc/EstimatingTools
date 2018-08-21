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
    public class TECPanelTests
    {
        [TestMethod()]
        public void DragDropCopyTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);
            TECPanel panel = ModelCreation.TestPanel(bid.Catalogs, rand);

            TECPanel copy = panel.DropData() as TECPanel;

            Assert.AreEqual(panel.Name, copy.Name);
            Assert.IsTrue(panel.CostBatch.CostsEqual(copy.CostBatch));

        }
    }
}