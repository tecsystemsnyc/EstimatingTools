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
            Assert.AreEqual(vm.TotalMiscCost, total.cost, "Total misc cost didn't update properly.");
            Assert.AreEqual(vm.TotalMiscLabor, total.labor, "Total misc labor didn't update properly.");
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
        #endregion

        #region Remove

        #endregion
        #endregion

        #region ElectricalSummaryTests

        #endregion

        #region Calculation Methods

        private Total calculateTotal(TECCost cost, CostType type)
        {
            if (cost.Type == type)
            {
                Total total = new Total();
                total.cost = cost.Cost * cost.Quantity;
                total.labor = cost.Labor * cost.Quantity;
                return total;
            }
            else
            {
                return new Total();
            }
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
