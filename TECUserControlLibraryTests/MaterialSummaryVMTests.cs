using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TECUserControlLibrary.ViewModels;
using static Tests.CostTestingUtilities;

namespace Tests
{
    /// <summary>
    /// Summary description for SummaryVMTests
    /// </summary>
    [TestClass]
    public class MaterialSummaryVMTests
    {
        #region Material Summary VM
        #region Add
        [TestMethod]
        public void AddTECCostToSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            bid.Catalogs = TestHelper.CreateTestCatalogs();
            TECAssociatedCost cost = null;
            int x = 0;
            while(cost == null)
            {
                TECAssociatedCost randomCost = bid.Catalogs.AssociatedCosts[x];
                if (randomCost.Type == CostType.TEC)
                {
                    cost = randomCost;
                }
                x++;
            }

            Total totalTEC = CalculateTotal(cost, CostType.TEC);
            Total totalElec = CalculateTotal(cost, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.AssociatedCosts.Add(cost);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddElectricalCostToSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECAssociatedCost cost = null;
            foreach (TECAssociatedCost assoc in bid.Catalogs.AssociatedCosts)
            {
                if (assoc.Type == CostType.Electrical)
                {
                    cost = assoc;
                    break;
                }
            }
            if (cost == null) { Assert.Fail("No electrical cost in catalogs."); }

            Total totalTEC = CalculateTotal(cost, CostType.TEC);
            Total totalElec = CalculateTotal(cost, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.AssociatedCosts.Add(cost);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddTECMiscToBid()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC, false);

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.MiscCosts.Add(misc);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddElectricalMiscToBid()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical, false);

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.MiscCosts.Add(misc);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddTECMiscToSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC, true);

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.MiscCosts.Add(misc);

            //Arrange
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddElectricalMiscToSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical, true);

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.MiscCosts.Add(misc);
            
            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddPanel()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECPanel panel = TestHelper.CreateTestPanel(false, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(panel, bid.Catalogs);

            Total totalTEC = CalculateTotal(panel, CostType.TEC);
            Total totalElec = CalculateTotal(panel, CostType.Electrical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.Panels.Add(panel);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddController()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECController controller = TestHelper.CreateTestController(false, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(controller, bid.Catalogs);

            Total totalTEC = CalculateTotal(controller, CostType.TEC);
            Total totalElec = CalculateTotal(controller, CostType.Electrical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.AddController(controller);
            
            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddDevice()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECDevice device = TestHelper.CreateTestDevice(bid.Catalogs);
            TestHelper.AssignSecondaryProperties(device, bid.Catalogs);

            Total totalTEC = CalculateTotal(device, CostType.TEC);
            Total totalElec = CalculateTotal(device, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope(true);
            typEquip.SubScope.Add(typSS);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typSS.Devices.Add(device);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddValve()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECValve valve = TestHelper.CreateTestValve(bid.Catalogs);
            TestHelper.AssignSecondaryProperties(valve, bid.Catalogs);

            Total totalTEC = CalculateTotal(valve, CostType.TEC);
            Total totalElec = CalculateTotal(valve, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope(true);
            typEquip.SubScope.Add(typSS);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typSS.Devices.Add(valve);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddSubScope()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECSubScope subscope = TestHelper.CreateTestSubScope(true, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(subscope, bid.Catalogs);

            Total totalTEC = CalculateTotal(subscope, CostType.TEC);
            Total totalElec = CalculateTotal(subscope, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typEquip.SubScope.Add(subscope);
            
            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddEquipment()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECEquipment equipment = TestHelper.CreateTestEquipment(true, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(equipment, bid.Catalogs);

            Total totalTEC = CalculateTotal(equipment, CostType.TEC);
            Total totalElec = CalculateTotal(equipment, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);
            
            //Act
            typical.Equipment.Add(equipment);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddInstanceSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECTypical typical = TestHelper.CreateTestTypical(bid.Catalogs);
            TestHelper.AssignSecondaryProperties(typical, bid.Catalogs);
            bid.Systems.Add(typical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            TECSystem instance = typical.AddInstance(bid);

            Total totalTEC = CalculateTotal(instance, CostType.TEC);
            Total totalElec = CalculateTotal(instance, CostType.Electrical);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update proplery.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void AddConnection()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECControllerType controllerType = new TECControllerType(bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.ControllerTypes.Add(controllerType);
            TECController controller = new TECProvidedController(controllerType, false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope(true);
            typEquip.SubScope.Add(typSS);

            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            connectionTypes.Add(bid.Catalogs.ConnectionTypes[0]);
            TECDevice dev = new TECDevice(connectionTypes, new List<TECProtocol>(), bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.Devices.Add(dev);
            typSS.Devices.Add(dev);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            IControllerConnection connection = controller.Connect(typSS, (typSS as IConnectable).AvailableProtocols.First());
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            TECSystem instance = typical.AddInstance(bid);
            TECSubScope instanceSubScope = instance.GetAllSubScope()[0];
            IControllerConnection instanceConnection = controller.Connect(instanceSubScope, (instanceSubScope as IConnectable).AvailableProtocols.First());
            instanceConnection.Length = 50;
            instanceConnection.ConduitLength = 50;
            instanceConnection.ConduitType = bid.Catalogs.ConduitTypes[0];

            Total totalTEC = CalculateTotal(connection, CostType.TEC);
            Total totalElec = CalculateTotal(connection, CostType.Electrical);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }
        #endregion

        #region Remove
        [TestMethod]
        public void RemoveTECCost()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECAssociatedCost cost = null;
            int x = 0;
            while (cost == null)
            {
                TECAssociatedCost randomCost = bid.Catalogs.AssociatedCosts[x];
                if (randomCost.Type == CostType.TEC)
                {
                    cost = randomCost;
                }
                x++;
            }
            TECTypical system = new TECTypical();
            bid.Systems.Add(system);
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(cost, CostType.TEC);
            Total totalElec = CalculateTotal(cost, CostType.Electrical);

            //Act
            system.AssociatedCosts.Remove(cost);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveElectricalCost()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECAssociatedCost cost = null;
            while (cost == null)
            {
                TECAssociatedCost randomCost = bid.Catalogs.AssociatedCosts[0];
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }
            TECTypical system = new TECTypical();
            bid.Systems.Add(system);
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(cost, CostType.TEC);
            Total totalElec = CalculateTotal(cost, CostType.Electrical);

            //Act
            system.AssociatedCosts.Remove(cost);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveTECMisc()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC, false);
            bid.MiscCosts.Add(misc);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            //Act
            bid.MiscCosts.Remove(misc);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveElectricalMisc()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical, false);
            bid.MiscCosts.Add(misc);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(misc, CostType.TEC);
            Total totalElec = CalculateTotal(misc, CostType.Electrical);

            //Act
            bid.MiscCosts.Remove(misc);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemovePanel()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECPanel panel = TestHelper.CreateTestPanel(false, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(panel, bid.Catalogs);
            bid.Panels.Add(panel);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(panel, CostType.TEC);
            Total totalElec = CalculateTotal(panel, CostType.Electrical);

            //Act
            bid.Panels.Remove(panel);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveController()
        {
            //Arrange
            TECBid bid = TestHelper.CreateTestBid();
            ChangeWatcher cw = new ChangeWatcher(bid);
            TECController controller = TestHelper.CreateTestController(false, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(controller, bid.Catalogs);
            bid.AddController(controller);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(controller, CostType.TEC);
            Total totalElec = CalculateTotal(controller, CostType.Electrical);

            //Act
            bid.RemoveController(controller);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveDevice()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope(true);
            typEquip.SubScope.Add(typSS);

            TECDevice device = bid.Catalogs.Devices[0];
            TestHelper.AssignSecondaryProperties(device, bid.Catalogs);
            typSS.Devices.Add(device);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(device, CostType.TEC);
            Total totalElec = CalculateTotal(device, CostType.Electrical);

            //Act
            typSS.Devices.Remove(device);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveValve()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            bid.Catalogs.Valves.Add(TestHelper.CreateTestValve(bid.Catalogs));
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope(true);
            typEquip.SubScope.Add(typSS);

            TECValve valve = bid.Catalogs.Valves[0];
            TestHelper.AssignSecondaryProperties(valve, bid.Catalogs);
            typSS.Devices.Add(valve);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(valve, CostType.TEC);
            Total totalElec = CalculateTotal(valve, CostType.Electrical);

            //Act
            typSS.Devices.Remove(valve);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveSubScope()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope subScope = TestHelper.CreateTestSubScope(true, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(subScope, bid.Catalogs);
            typEquip.SubScope.Add(subScope);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(subScope, CostType.TEC);
            Total totalElec = CalculateTotal(subScope, CostType.Electrical);

            //Act
            typEquip.SubScope.Remove(subScope);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveEquipment()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment equip = TestHelper.CreateTestEquipment(true, bid.Catalogs);
            TestHelper.AssignSecondaryProperties(equip, bid.Catalogs);
            typical.Equipment.Add(equip);

            typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(equip, CostType.TEC);
            Total totalElec = CalculateTotal(equip, CostType.Electrical);

            //Act
            typical.Equipment.Remove(equip);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveInstanceSystem()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = TestHelper.CreateTestTypical(bid.Catalogs);
            bid.Systems.Add(typical);

            TECSystem instance = typical.AddInstance(bid);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = CalculateTotal(instance, CostType.TEC);
            Total totalElec = CalculateTotal(instance, CostType.Electrical);

            //Act
            typical.Instances.Remove(instance);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }

        [TestMethod]
        public void RemoveConnection()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECController controller = new TECProvidedController(bid.Catalogs.ControllerTypes[0], false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment(true);
            typical.Equipment.Add(typEquip);

            TECSubScope ss = new TECSubScope(true);
            ss.Devices.Add(bid.Catalogs.Devices[0]);
            typEquip.SubScope.Add(ss);
            
            TECSystem instance = typical.AddInstance(bid);

            TECSubScope instanceSubScope = instance.GetAllSubScope()[0];
            IControllerConnection connection = controller.Connect(instanceSubScope, (instanceSubScope as IConnectable).AvailableProtocols.First());
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            double initialTecCost = matVM.TotalTECCost;
            double initialTecLabor = matVM.TotalTECLabor;

            double initialElecCost = matVM.TotalElecCost;
            double initialElecLabor = matVM.TotalElecLabor;

            Total totalTEC = new Total(connection.CostBatch.GetCost(CostType.TEC), connection.CostBatch.GetLabor(CostType.TEC));
            Total totalElec = new Total(connection.CostBatch.GetCost(CostType.Electrical), connection.CostBatch.GetLabor(CostType.Electrical));

            //Act
            controller.Disconnect(instance.GetAllSubScope()[0]);

            //Assert
            Assert.AreEqual(matVM.TotalTECCost, initialTecCost - totalTEC.Cost, DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalTECLabor, initialTecLabor - totalTEC.Labor, DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(matVM.TotalElecCost, initialElecCost - totalElec.Cost, DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(matVM.TotalElecLabor, initialElecLabor - totalElec.Labor, DELTA, "Total elec labor didn't update properly.");

        }
        #endregion

        #region Special Tests
        [TestMethod]
        public void AddTypicalSubScopeConnectionToController()
        {
            //Arrange
            TECBid bid = TestHelper.CreateEmptyCatalogBid();
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECControllerType controllerType = new TECControllerType(bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.ControllerTypes.Add(controllerType);
            TECController controller = new TECProvidedController(controllerType, false);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();

            TECEquipment equip = new TECEquipment(true);
            typical.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(true);
            equip.SubScope.Add(ss);

            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);

            bid.Systems.Add(typical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            IControllerConnection connection = controller.Connect(ss, (ss as IConnectable).AvailableProtocols.First());
            connection.Length = 100;
            connection.ConduitLength = 100;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            Assert.AreEqual(0, matVM.TotalTECCost, "Typical connection added to tec cost.");
            Assert.AreEqual(0, matVM.TotalTECLabor, "Typical connection added to tec labor.");
            Assert.AreEqual(0, matVM.TotalElecCost, "Typical connection added to elec cost.");
            Assert.AreEqual(0, matVM.TotalElecLabor, "Typical connection added to elec labor.");

        }
        #endregion

        #endregion
        
    }
}
