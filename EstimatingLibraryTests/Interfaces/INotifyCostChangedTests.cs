using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Interfaces
{
    /// <summary>
    /// Summary description for CostChangedTests
    /// </summary>
    [TestClass]
    public class INotifyCostChangedTests
    {
        CostBatch costs;
        TECBid bid;
        TECManufacturer manufacturer;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            bid = new TECBid();
            manufacturer = new TECManufacturer();
            manufacturer.Multiplier = 1;
            ChangeWatcher watcher = new ChangeWatcher(bid);
            costs = new CostBatch();
            watcher.CostChanged += (e) =>
            {
                costs += e;
            };
        }

        [TestMethod]
        public void Bid_AddController()
        {
            TECControllerType controllerType = new TECControllerType(manufacturer);
            controllerType.Price = 100;
            TECController controller = new TECProvidedController(controllerType, false);

            bid.AddController(controller);
            
            Assert.AreEqual(100, costs.GetCost(CostType.TEC));
            Assert.AreEqual(0, costs.GetCost(CostType.Electrical));
        }

        [TestMethod]
        public void Bid_AddPanel()
        {
            TECPanelType panelType = new TECPanelType(manufacturer);
            panelType.Price = 100;
            TECPanel panel = new TECPanel(panelType, false);

            bid.Panels.Add(panel);
            
            Assert.AreEqual(100, costs.GetCost(CostType.TEC));
            Assert.AreEqual(0, costs.GetCost(CostType.Electrical));
        }
    }
}
