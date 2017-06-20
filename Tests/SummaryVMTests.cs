﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using TECUserControlLibrary.ViewModels;

namespace Tests
{
    /// <summary>
    /// Summary description for SummaryVMTests
    /// </summary>
    [TestClass]
    public class SummaryVMTests
    {
        public SummaryVMTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        static private TECCatalogs catalogs;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            catalogs = TestHelper.CreateTestCatalogs();
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        #region TECSummaryTests
        #region Add
        [TestMethod]
        public void AddTECCost()
        {
            TECCost cost = null;
            while(cost == null)
            {
                TECCost randomCost = catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.TEC)
                {
                    cost = randomCost;
                }
            }

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addAssCost", cost);

            Total total = calculateTotal(cost, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddElectricalCost()
        {
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addAssCost", cost);

            Total total = calculateTotal(cost, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }

        [TestMethod]
        public void AddTECMiscToBid()
        {
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addMiscCost", misc, null);

            Total total = calculateTotal(misc, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddElectricalMiscToBid()
        {
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addMiscCost", misc, null);

            Total total = calculateTotal(misc, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddTECMiscToSystem()
        {
            TECSystem system = TestHelper.CreateTestSystem(catalogs);
            for(int i = 0; i < TestHelper.RandomInt(0, 10); i++)
            {
                system.AddInstance(new TECBid());
            }
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addMiscCost", misc, system);

            Total total = calculateTotal(misc, CostType.TEC);
            total *= system.SystemInstances.Count;

            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }

        [TestMethod]
        public void AddElectricalMiscToSystem()
        {
            TECSystem system = TestHelper.CreateTestSystem(catalogs);
            for (int i = 0; i < TestHelper.RandomInt(0, 10); i++)
            {
                system.AddInstance(new TECBid());
            }
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addMiscCost", misc, system);

            Total total = calculateTotal(misc, CostType.TEC);
            total *= system.SystemInstances.Count;

            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }

        [TestMethod]
        public void AddPanel()
        {
            TECPanel panel = TestHelper.CreateTestPanel(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addPanel", panel);

            Total total = calculateTotal(panel, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddController()
        {
            TECController controller = TestHelper.CreateTestController(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addController", controller);

            Total total = calculateTotal(controller, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddPoint()
        {
            TECPoint point = TestHelper.CreateTestPoint(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addPoint", point);

            Total total = calculateTotal(point, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddDevice()
        {
            TECDevice device = TestHelper.CreateTestDevice(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addDevice", device);

            Total total = calculateTotal(device, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddSubScope()
        {
            TECSubScope subscope = TestHelper.CreateTestSubScope(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addSubScope", subscope);

            Total total = calculateTotal(subscope, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddEquipment()
        {
            TECEquipment equipment = TestHelper.CreateTestEquipment(catalogs);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addEquipment", equipment);

            Total total = calculateTotal(equipment, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void AddInstanceSystem()
        {
            TECSystem system = TestHelper.CreateTestSystem(catalogs);

            system.AddInstance(new TECBid());

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addInstanceSystem", system);

            Total total = calculateTotal(system, CostType.TEC);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }
        #endregion

        #region Remove
        [TestMethod]
        public void RemoveTECCost()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = bid.Catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.TEC)
                {
                    cost = randomCost;
                }
            }
            var system = bid.Systems.RandomObject();
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = cost;
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeAssCost", removed);

            Assert.AreEqual(vm.TotalCost, initialTotalCost - total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, initialTotalLabor - total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveElectricalCost()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = bid.Catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }
            var system = bid.Systems.RandomObject();
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = cost;
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeAssCost", removed);

            Assert.AreEqual(vm.TotalCost, initialTotalCost - total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, initialTotalLabor - total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveTECMisc()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC);
            bid.MiscCosts.Add(misc);
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = misc;
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeMiscCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveElectricalMisc()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical);
            bid.MiscCosts.Add(misc);
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = misc;
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeMiscCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemovePanel()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECPanel panel = TestHelper.CreateTestPanel(catalogs);
            bid.Panels.Add(panel);
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            TECPanel removed = panel;
            Total total = calculateTotal(panel, CostType.TEC);

            testVM.Invoke("removePanel", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveController()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECController controller = TestHelper.CreateTestController(catalogs);
            bid.Controllers.Add(controller);
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            TECController removed = controller;
            Total total = calculateTotal(controller, CostType.TEC);

            testVM.Invoke("removeController", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemovePoint()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = bid.RandomPoint();
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removePoint", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveDevice()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            TECDevice removed = bid.RandomDevice();
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeDevice", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveSubScope()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            TECSubScope removed = bid.RandomSubScope();

            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            Total delta = calculateTotal(removed, CostType.TEC);

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("removeSubScope", removed);

            Assert.AreEqual(initialTotalCost - delta.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - delta.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveEquipment()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            TECEquipment removed = bid.RandomEquipment();

            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            Total delta = calculateTotal(removed, CostType.TEC);

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("removeEquipment", removed);

            Assert.AreEqual(initialTotalCost - delta.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - delta.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void RemoveInstanceSystem()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECSystem system = TestHelper.CreateTestSystem(bid.Catalogs);
            bid.Systems.Add(system);
            system.AddInstance(bid);

            TECMaterialSummaryVM vm = new TECMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = system.SystemInstances.RandomObject();
            Total total = calculateTotal(removed, CostType.TEC);

            testVM.Invoke("removeInstanceSystem", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }
        #endregion
        #endregion

        #region ElectricalSummaryTests
        #region Add
        [TestMethod]
        public void ElectricalSummary_AddTECCost()
        {
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addAssCost", cost);

            Total total = calculateTotal(cost, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddElectricalCost()
        {
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addAssCost", cost);

            Total total = calculateTotal(cost, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddElectricalMiscToBid()
        {
            TECMisc misc = null;
            while (misc == null)
            {
                TECMisc randomMisc = TestHelper.CreateTestMisc();
                if (randomMisc.Type == CostType.Electrical)
                {
                    misc = randomMisc;
                }
            }

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addMiscCost", misc, null);

            Total total = calculateTotal(misc, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
        }
        
        [TestMethod]
        public void ElectricalSummary_AddInstanceSystem()
        {
            TECSystem system = TestHelper.CreateTestSystem(catalogs);

            system.AddInstance(new TECBid());

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addInstanceSystem", system);

            Total total = calculateTotal(system, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_addController()
        {
            TECController controller = TestHelper.CreateTestController(catalogs);

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addController", controller);

            Total total = calculateTotal(controller, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddConnection()
        {
            TECController controller = TestHelper.CreateTestController(catalogs);
            TECSubScope subScope = TestHelper.CreateTestSubScope(catalogs);
            TECConnection connection = controller.AddSubScope(subScope);
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = catalogs.ConduitTypes.RandomObject();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addConnection", connection);

            Total total = calculateTotal(connection, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddEquipment()
        {
            TECEquipment equipment = TestHelper.CreateTestEquipment(catalogs);

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addEquipment", equipment);

            Total total = calculateTotal(equipment, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddSubScope()
        {
            TECSubScope subScope = TestHelper.CreateTestSubScope(catalogs);

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addSubScope", subScope);

            Total total = calculateTotal(subScope, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddPoint()
        {
            TECPoint point = new TECPoint();
            foreach(TECCost cost in catalogs.AssociatedCosts)
            {
                point.AssociatedCosts.Add(cost);
            }

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addPoint", point);

            Total total = calculateTotal(point, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_AddAssCost()
        {
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(new TECBid());

            PrivateObject testVM = new PrivateObject(vm);
            testVM.Invoke("addAssCost", cost);

            Total total = calculateTotal(cost, CostType.Electrical);

            Assert.AreEqual(vm.TotalCost, total.cost, "Total cost didn't update properly.");
            Assert.AreEqual(vm.TotalLabor, total.labor, "Total labor didn't update properly.");
        }

        #endregion

        #region Remove
        [TestMethod]
        public void ElectricalSummary_RemoveTECCost()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = bid.Catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.TEC)
                {
                    cost = randomCost;
                }
            }
            var system = bid.Systems.RandomObject();
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = cost;
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeAssCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveElectricalCost()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECCost cost = null;
            while (cost == null)
            {
                TECCost randomCost = bid.Catalogs.AssociatedCosts.RandomObject();
                if (randomCost.Type == CostType.Electrical)
                {
                    cost = randomCost;
                }
            }
            var system = bid.Systems.RandomObject();
            system.AddInstance(bid);
            system.AssociatedCosts.Add(cost);
           
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = cost;
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeAssCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveTECMiscFromBid()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMisc misc = TestHelper.CreateTestMisc(CostType.TEC);
            bid.MiscCosts.Add(misc);
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = misc;
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeMiscCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveElectricalMiscFromBid()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECMisc misc = TestHelper.CreateTestMisc(CostType.Electrical);
            bid.MiscCosts.Add(misc);
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = misc;
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeMiscCost", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }
        
        [TestMethod]
        public void ElectricalSummary_RemoveInstanceSystem()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECSystem system = TestHelper.CreateTestSystem(bid.Catalogs);
            bid.Systems.Add(system);
            system.AddInstance(bid);

            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = system.SystemInstances.RandomObject();
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeInstanceSystem", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveController()
        {
            TECBid bid = TestHelper.CreateTestBid();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = bid.Controllers.RandomObject();
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeController", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveConnection()
        {
            TECBid bid = TestHelper.CreateTestBid();
            TECController controller = bid.Controllers.RandomObject();
            TECSubScope subScope = bid.RandomSubScope();
            TECConnection connection = controller.AddSubScope(subScope);
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = catalogs.ConduitTypes.RandomObject();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = connection;
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeConnection", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveEquipemnt()
        {
            TECBid bid = TestHelper.CreateTestBid();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = bid.RandomEquipment();
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeEquipment", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemoveSubScope()
        {
            TECBid bid = TestHelper.CreateTestBid();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);
            
            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = bid.RandomSubScope();
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removeSubScope", removed);
            
            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        [TestMethod]
        public void ElectricalSummary_RemovePoint()
        {
            TECBid bid = TestHelper.CreateTestBid();
            ElectricalMaterialSummaryVM vm = new ElectricalMaterialSummaryVM(bid);

            PrivateObject testVM = new PrivateObject(vm);
            double initialTotalCost = vm.TotalCost;
            double initialTotalLabor = vm.TotalLabor;

            var removed = bid.RandomPoint();
            Total total = calculateTotal(removed, CostType.Electrical);

            testVM.Invoke("removePoint", removed);

            Assert.AreEqual(initialTotalCost - total.cost, vm.TotalCost, "Total cost didn't update properly.");
            Assert.AreEqual(initialTotalLabor - total.labor, vm.TotalLabor, "Total labor didn't update properly.");
        }

        #endregion
        #endregion

        #region Calculation Methods

        private Total calculateTotal(TECCost cost, CostType type)
        {
            int qty = 1;
            if(cost is TECMisc)
            {
                qty = cost.Quantity;
            }
            if (cost.Type == type)
            {
                Total total = new Total();
                total.cost = cost.Cost * qty;
                total.labor = cost.Labor * qty;
                return total;
            }
            else
            {
                return new Total();
            }
        }

        private Total calculateTotal(TECScope scope, CostType type)
        {
            Total total = new Total();
            foreach(TECCost cost in scope.AssociatedCosts)
            {
                total += calculateTotal(cost, type);
            }
            return total;
        }

        private Total calculateTotal(TECDevice device, CostType type)
        {
            Total total = new Total();
            total.cost = device.ExtendedCost;
            total.labor = device.Labor;
            total += calculateTotal(device as TECScope, type);
            return total;
        }

        private Total calculateTotal(TECSubScope subScope, CostType type)
        {
            Total total = new Total();
            foreach(TECDevice device in subScope.Devices)
            {
                total += calculateTotal(device, type);
            }
            foreach(TECPoint point in subScope.Points)
            {
                total += calculateTotal(point, type);
            }
            total += calculateTotal(subScope as TECScope, type);
            return total;
        }

        private Total calculateTotal(TECEquipment equipment, CostType type)
        {
            Total total = new Total();
            foreach (TECSubScope subScope in equipment.SubScope)
            {
                total += calculateTotal(subScope, type);
            }
            total += calculateTotal(equipment as TECScope, type);
            return total;
        }

        private Total calculateTotal(TECController controller, CostType type)
        {
            Total total = new Total();
            total += calculateTotal(controller as TECScope, type);
            total += calculateTotal(controller as TECCost, type);
            foreach(TECConnection connection in controller.ChildrenConnections)
            {
                total += calculateTotal(connection, type);
            }
            return total;
        }

        private Total calculateTotal(TECPanel panel, CostType type)
        {
            Total total = new Total();
            total += calculateTotal(panel as TECScope, type);
            total += calculateTotal(panel.Type as TECCost, type);
            return total;
        }

        private Total calculateTotal(TECSystem system, CostType type)
        {
            Total total = new Total();
            foreach (TECEquipment equipment in system.Equipment)
            {
                total += calculateTotal(equipment, type);
            }
            foreach(TECMisc misc in system.MiscCosts)
            {
                total += calculateTotal(misc, type) * system.SystemInstances.Count;
            }
            foreach(TECController controller in system.Controllers)
            {
                total += calculateTotal(controller, type);
            }
            foreach(TECPanel panel in system.Panels)
            {
                total += calculateTotal(panel, type);
            }
            total += calculateTotal(system as TECScope, type);
            return total;
        }

        private Total calculateTotal(TECConnection connection, CostType type)
        {
            Total total = new Total();
            if(connection is TECSubScopeConnection)
            {
                foreach(TECConnectionType conType in (connection as TECSubScopeConnection).ConnectionTypes)
                {
                    total += calculateTotal(conType, type) * connection.Length;
                    total += calculateTotal(conType as TECScope, type);
                    foreach(TECCost cost in conType.RatedCosts)
                    {
                        total += calculateTotal(cost, type) * connection.Length;
                    }
                }
            } else if(connection is TECNetworkConnection)
            {
                total += calculateTotal((connection as TECNetworkConnection).ConnectionType, type) * connection.Length;
                total += calculateTotal((connection as TECNetworkConnection).ConnectionType as TECScope, type);
                foreach (TECCost cost in (connection as TECNetworkConnection).ConnectionType.RatedCosts)
                {
                    total += calculateTotal(cost, type) * connection.Length;
                }
            }
            if(connection.ConduitType != null)
            {
                total += calculateTotal(connection.ConduitType, type) * connection.Length;
                total += calculateTotal(connection.ConduitType as TECScope, type);
                foreach (TECCost cost in connection.ConduitType.RatedCosts)
                {
                    total += calculateTotal(cost, type) * connection.Length;
                }
            }
            return total;
        }

        #endregion

        private class Total
        {
            public double cost;
            public double labor;

            public Total()
            {
                cost = 0;
                labor = 0;
            }

            public static Total operator +(Total left, Total right)
            {
                Total total = new Total();
                total.cost = left.cost + right.cost;
                total.labor = left.labor + right.labor;
                return total;
            }

            public static Total operator -(Total left, Total right)
            {
                Total total = new Total();
                total.cost = left.cost - right.cost;
                total.labor = left.labor - right.labor;
                return total;
            }

            public static Total operator *(Total left, double right)
            {
                Total total = new Total();
                total.cost = left.cost * right;
                total.labor = left.labor * right;
                return total;
            }
        }
    }
}
