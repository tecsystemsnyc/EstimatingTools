using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TECUserControlLibrary.ViewModels;
using TECUserControlLibrary.ViewModels.Interfaces;
using TestLibrary;
using TestLibrary.ModelTestingUtilities;

namespace ViewModels
{
    [TestClass]
    public class ItemSummaryVMTests
    {
        Random rand;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }
        #region Hardware Summary VM
        [TestMethod]
        public void AddControllerTypeToHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECControllerType controllerType = catalogs.ControllerTypes[0];

            //Act
            CostBatch delta = hardwareVM.AddHardware(controllerType);

            CostBatch assocCostTotal = new CostBatch();
            foreach (TECAssociatedCost cost in controllerType.AssociatedCosts)
            {
                assocCostTotal += cost.CostBatch;
            }

            //Assert
            //Check hardware properties
            Assert.AreEqual(controllerType.Cost, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(controllerType.Labor, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(assocCostTotal.GetCost(CostType.TEC), hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocCostTotal.GetLabor(CostType.TEC), hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocCostTotal.GetCost(CostType.Electrical), hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocCostTotal.GetLabor(CostType.Electrical), hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(controllerType.CostBatch, delta);
        }
        [TestMethod]
        public void RemoveControllerTypeFromHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECControllerType controllerType = catalogs.ControllerTypes[0];

            //Act
            hardwareVM.AddHardware(controllerType);
            CostBatch delta = hardwareVM.RemoveHardware(controllerType);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(-controllerType.CostBatch, delta);
        }
        [TestMethod]
        public void AddPanelTypeToHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECPanelType panelType = catalogs.PanelTypes[0];

            //Act
            CostBatch delta = hardwareVM.AddHardware(panelType);
            
            CostBatch assocTotal = new CostBatch();
            foreach (TECAssociatedCost cost in panelType.AssociatedCosts)
            {
                assocTotal += cost.CostBatch;
            }

            //Assert
            //Check hardware properties
            Assert.AreEqual(panelType.Cost, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(panelType.Labor, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(assocTotal.GetCost(CostType.TEC), hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetLabor(CostType.TEC), hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetCost(CostType.Electrical), hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetLabor(CostType.Electrical), hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(panelType.CostBatch, delta);
        }
        [TestMethod]
        public void RemovePanelTypeFromHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECPanelType panelType = catalogs.PanelTypes[0];

            //Act
            hardwareVM.AddHardware(panelType);
            CostBatch delta = hardwareVM.RemoveHardware(panelType);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(-panelType.CostBatch, delta);
        }
        [TestMethod]
        public void AddDeviceToHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECDevice device = catalogs.Devices[0];

            //Act
            CostBatch delta = hardwareVM.AddHardware(device);

            CostBatch assocTotal = new CostBatch();
            foreach (TECAssociatedCost cost in device.AssociatedCosts)
            {
                assocTotal += cost.CostBatch;
            }

            //Assert
            //Check hardware properties
            Assert.AreEqual(device.Cost, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(device.Labor, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(assocTotal.GetCost(CostType.TEC), hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetLabor(CostType.TEC), hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetCost(CostType.Electrical), hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(assocTotal.GetLabor(CostType.Electrical), hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(device.CostBatch, delta);
        }
        [TestMethod]
        public void RemoveDeviceFromHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECDevice device = catalogs.Devices[0];

            //Act
            hardwareVM.AddHardware(device);
            CostBatch delta = hardwareVM.RemoveHardware(device);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(-device.CostBatch, delta);
        }
        [TestMethod]
        public void AddTECCostToHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost tecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.TEC)
                {
                    tecCost = cost;
                    break;
                }
            }

            //Act
            CostBatch delta = hardwareVM.AddCost(tecCost);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(tecCost.Cost, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(tecCost.Labor, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(tecCost.Cost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(tecCost.Labor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveTECCostFromHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost tecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.TEC)
                {
                    tecCost = cost;
                    break;
                }
            }

            //Act
            hardwareVM.AddCost(tecCost);
            CostBatch delta = hardwareVM.RemoveCost(tecCost);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(-tecCost.Cost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(-tecCost.Labor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void AddElecCostToHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost elecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.Electrical)
                {
                    elecCost = cost;
                    break;
                }
            }

            //Act
            CostBatch delta = hardwareVM.AddCost(elecCost);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(elecCost.Cost, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(elecCost.Labor, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(elecCost.Cost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(elecCost.Labor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveElecCostFromHardwareVM()
        {
            //Arrange
            HardwareSummaryVM hardwareVM = new HardwareSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost elecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.Electrical)
                {
                    elecCost = cost;
                    break;
                }
            }

            //Act
            hardwareVM.AddCost(elecCost);
            CostBatch delta = hardwareVM.RemoveCost(elecCost);

            //Assert
            //Check hardware properties
            Assert.AreEqual(0, hardwareVM.HardwareCost, GeneralTestingUtilities.DELTA, "HardwareCost property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.HardwareLabor, GeneralTestingUtilities.DELTA, "HardwareLabor property in HardwareSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, hardwareVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in HardwareSummaryVM is wrong.");
            Assert.AreEqual(0, hardwareVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in HardwareSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(-elecCost.Cost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(-elecCost.Labor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        #endregion

        #region Length Summary VM
        [TestMethod]
        public void AddElectricalMaterialRunToLengthVM()
        {
            //Arrange
            LengthSummaryVM lengthVM = new LengthSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            List<TECElectricalMaterial> elecMats = new List<TECElectricalMaterial>();
            elecMats.AddRange(catalogs.ConnectionTypes);
            elecMats.AddRange(catalogs.ConduitTypes);
            TECElectricalMaterial elecMat = elecMats[0];
            double length = 123.8;

            //Act
            CostBatch delta = lengthVM.AddRun(elecMat, length);

            double expectedLengthCost = elecMat.Cost * length;
            double expectedLengthLabor = elecMat.Labor * length;
            
            CostBatch expectedAssocTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.AssociatedCosts)
            {
                expectedAssocTotal += cost.CostBatch;
            }

            CostBatch expectedRatedTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.RatedCosts)
            {
                expectedRatedTotal += cost.CostBatch;
            }
            expectedRatedTotal *= length;

            CostBatch expectedDelta = (expectedAssocTotal + expectedRatedTotal);
            expectedDelta.AddCost(new TECAssociatedCost(CostType.Electrical)
            {
                Cost = expectedLengthCost,
                Labor = expectedLengthLabor
            });

            //Assert
            //Check length properties
            Assert.AreEqual(expectedLengthCost, lengthVM.LengthCostTotal, GeneralTestingUtilities.DELTA, "LengthCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedLengthLabor, lengthVM.LengthLaborTotal, GeneralTestingUtilities.DELTA, "LengthLaborTotal property in LengthSummaryVM is wrong.");
            //Check assoc properties
            Assert.AreEqual(expectedAssocTotal.GetCost(CostType.TEC), lengthVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetLabor(CostType.TEC), lengthVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetCost(CostType.Electrical), lengthVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetLabor(CostType.Electrical), lengthVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check rated properties
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.TEC), lengthVM.RatedTECCostTotal, GeneralTestingUtilities.DELTA, "RatedTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.TEC), lengthVM.RatedTECLaborTotal, GeneralTestingUtilities.DELTA, "RatedTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.Electrical), lengthVM.RatedElecCostTotal, GeneralTestingUtilities.DELTA, "RatedElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.Electrical), lengthVM.RatedElecLaborTotal, GeneralTestingUtilities.DELTA, "RatedElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(expectedDelta, delta);
        }
        [TestMethod]
        public void RemoveElectricalMaterialRunFromLengthVM()
        {
            //Arrange
            LengthSummaryVM lengthVM = new LengthSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            List<TECElectricalMaterial> elecMats = new List<TECElectricalMaterial>();
            elecMats.AddRange(catalogs.ConnectionTypes);
            elecMats.AddRange(catalogs.ConduitTypes);
            TECElectricalMaterial elecMat = elecMats[0];
            double addLength = 736;
            double removeLength = 23.4;
            double length = addLength - removeLength;

            //Act
            lengthVM.AddRun(elecMat, addLength);
            CostBatch delta = lengthVM.RemoveRun(elecMat, removeLength);

            double expectedLengthCost = elecMat.Cost * length;
            double expectedLengthLabor = elecMat.Labor * length;
            double removedLengthCost = elecMat.Cost * removeLength;
            double removedLengthLabor = elecMat.Labor * removeLength;

            CostBatch removedAssocTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.AssociatedCosts)
            {
                removedAssocTotal += cost.CostBatch;
            }
            
            CostBatch ratedTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.RatedCosts)
            {
                ratedTotal += cost.CostBatch;
            }

            CostBatch expectedRatedTotal = ratedTotal * length;
            CostBatch removedRatedTotal = ratedTotal * removeLength;

            CostBatch expectedDelta = -removedAssocTotal + -removedRatedTotal;
            expectedDelta.AddCost(new TECAssociatedCost(CostType.Electrical)
            {
                Cost = -removedLengthCost,
                Labor = -removedLengthLabor
            });

            //Assert
            //Check length properties
            Assert.AreEqual(expectedLengthCost, lengthVM.LengthCostTotal, GeneralTestingUtilities.DELTA, "LengthCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedLengthLabor, lengthVM.LengthLaborTotal, GeneralTestingUtilities.DELTA, "LengthLaborTotal property in LengthSummaryVM is wrong.");
            //Check assoc properties
            Assert.AreEqual(0, lengthVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check rated properties
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.TEC), lengthVM.RatedTECCostTotal, GeneralTestingUtilities.DELTA, "RatedTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.TEC), lengthVM.RatedTECLaborTotal, GeneralTestingUtilities.DELTA, "RatedTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.Electrical), lengthVM.RatedElecCostTotal, GeneralTestingUtilities.DELTA, "RatedElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.Electrical), lengthVM.RatedElecLaborTotal, GeneralTestingUtilities.DELTA, "RatedElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(expectedDelta, delta);
        }
        [TestMethod]
        public void AddElectricalMaterialLengthToLengthVM()
        {
            //Arrange
            LengthSummaryVM lengthVM = new LengthSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            List<TECElectricalMaterial> elecMats = new List<TECElectricalMaterial>();
            elecMats.AddRange(catalogs.ConnectionTypes);
            elecMats.AddRange(catalogs.ConduitTypes);
            TECElectricalMaterial elecMat = elecMats[0];
            double length = 435.2;

            //Act
            CostBatch delta = lengthVM.AddLength(elecMat, length);

            double expectedLengthCost = elecMat.Cost * length;
            double expectedLengthLabor = elecMat.Labor * length;
            
            CostBatch expectedRatedTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.RatedCosts)
            {
                expectedRatedTotal += cost.CostBatch;
            }
            expectedRatedTotal *= length;

            CostBatch expectedDelta = new CostBatch(expectedLengthCost, expectedLengthLabor, CostType.Electrical);
            expectedDelta += expectedRatedTotal;

            //Assert
            //Check length properties
            Assert.AreEqual(expectedLengthCost, lengthVM.LengthCostTotal, GeneralTestingUtilities.DELTA, "LengthCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedLengthLabor, lengthVM.LengthLaborTotal, GeneralTestingUtilities.DELTA, "LengthLaborTotal property in LengthSummaryVM is wrong.");
            //Check assoc properties
            Assert.AreEqual(0, lengthVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(0, lengthVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check rated properties
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.TEC), lengthVM.RatedTECCostTotal, GeneralTestingUtilities.DELTA, "RatedTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.TEC), lengthVM.RatedTECLaborTotal, GeneralTestingUtilities.DELTA, "RatedTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.Electrical), lengthVM.RatedElecCostTotal, GeneralTestingUtilities.DELTA, "RatedElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.Electrical), lengthVM.RatedElecLaborTotal, GeneralTestingUtilities.DELTA, "RatedElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(expectedDelta, delta);
        }
        [TestMethod]
        public void RemoveElectricalMaterialLengthFromLengthVM()
        {
            //Arrange
            LengthSummaryVM lengthVM = new LengthSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            List<TECElectricalMaterial> elecMats = new List<TECElectricalMaterial>();
            elecMats.AddRange(catalogs.ConnectionTypes);
            elecMats.AddRange(catalogs.ConduitTypes);
            TECElectricalMaterial elecMat = elecMats[0];
            double addLength = 543.6;
            double removeLength = 23.4;
            double length = addLength - removeLength;

            //Act
            lengthVM.AddRun(elecMat, addLength);
            CostBatch delta = lengthVM.RemoveLength(elecMat, removeLength);

            double expectedLengthCost = elecMat.Cost * length;
            double expectedLengthLabor = elecMat.Labor * length;
            double removedLengthCost = elecMat.Cost * removeLength;
            double removedLengthLabor = elecMat.Labor * removeLength;
            
            CostBatch expectedAssocTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.AssociatedCosts)
            {
                expectedAssocTotal += cost.CostBatch;
            }
            
            CostBatch ratedTotal = new CostBatch();
            foreach (TECAssociatedCost cost in elecMat.RatedCosts)
            {
                ratedTotal += cost.CostBatch;
            }

            CostBatch expectedRatedTotal = ratedTotal * length;
            CostBatch removedRatedTotal = ratedTotal * removeLength;

            CostBatch expectedDelta = new CostBatch(-removedLengthCost, -removedLengthLabor, CostType.Electrical);
            expectedDelta -= removedRatedTotal;

            //Assert
            //Check length properties
            Assert.AreEqual(expectedLengthCost, lengthVM.LengthCostTotal, GeneralTestingUtilities.DELTA, "LengthCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedLengthLabor, lengthVM.LengthLaborTotal, GeneralTestingUtilities.DELTA, "LengthLaborTotal property in LengthSummaryVM is wrong.");
            //Check assoc properties
            Assert.AreEqual(expectedAssocTotal.GetCost(CostType.TEC), lengthVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetLabor(CostType.TEC), lengthVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetCost(CostType.Electrical), lengthVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedAssocTotal.GetLabor(CostType.Electrical), lengthVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check rated properties
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.TEC), lengthVM.RatedTECCostTotal, GeneralTestingUtilities.DELTA, "RatedTECCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.TEC), lengthVM.RatedTECLaborTotal, GeneralTestingUtilities.DELTA, "RatedTECLaborTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetCost(CostType.Electrical), lengthVM.RatedElecCostTotal, GeneralTestingUtilities.DELTA, "RatedElecCostTotal property in LengthSummaryVM is wrong.");
            Assert.AreEqual(expectedRatedTotal.GetLabor(CostType.Electrical), lengthVM.RatedElecLaborTotal, GeneralTestingUtilities.DELTA, "RatedElecLaborTotal property in LengthSummaryVM is wrong.");
            //Check returned delta
            AssertCostBatchesEqual(expectedDelta, delta);
        }
        #endregion

        #region Misc Costs Summary VM
        [TestMethod]
        public void AddTECCostToMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost tecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.TEC)
                {
                    tecCost = cost;
                    break;
                }
            }

            //Act
            CostBatch delta = miscVM.AddCost(tecCost);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(tecCost.Cost, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(tecCost.Labor, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(tecCost.Cost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(tecCost.Labor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveTECCostFromMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost tecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.TEC)
                {
                    tecCost = cost;
                    break;
                }
            }

            //Act
            miscVM.AddCost(tecCost);
            CostBatch delta = miscVM.RemoveCost(tecCost);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(-tecCost.Cost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(-tecCost.Labor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void AddElecCostToMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost elecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.Electrical)
                {
                    elecCost = cost;
                    break;
                }
            }

            //Act
            CostBatch delta = miscVM.AddCost(elecCost);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(elecCost.Cost, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(elecCost.Labor, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(elecCost.Cost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(elecCost.Labor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveElecCostFromMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            TECAssociatedCost elecCost = null;
            foreach (TECAssociatedCost cost in catalogs.AssociatedCosts)
            {
                if (cost.Type == CostType.Electrical)
                {
                    elecCost = cost;
                    break;
                }
            }

            //Act
            miscVM.AddCost(elecCost);
            CostBatch delta = miscVM.RemoveCost(elecCost);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(-elecCost.Cost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(-elecCost.Labor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void AddTECMiscToMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc tecMisc = new TECMisc(CostType.TEC);
            tecMisc.Cost = 542.7;
            tecMisc.Labor = 467.4;
            tecMisc.Quantity = 3;

            double expectedCost = tecMisc.Cost * tecMisc.Quantity;
            double expectedLabor = tecMisc.Labor * tecMisc.Quantity;

            //Act
            CostBatch delta = miscVM.AddCost(tecMisc);

            //Assert
            //Check Misc properties
            Assert.AreEqual(expectedCost, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(expectedLabor, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(expectedCost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(expectedLabor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveTECMiscFromMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc tecMisc = new TECMisc(CostType.TEC);
            tecMisc.Cost = 5478.124;
            tecMisc.Labor = 14.6;
            tecMisc.Quantity = 3;

            double expectedCost = tecMisc.Cost * tecMisc.Quantity;
            double expectedLabor = tecMisc.Labor * tecMisc.Quantity;

            //Act
            miscVM.AddCost(tecMisc);
            CostBatch delta = miscVM.RemoveCost(tecMisc);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(-expectedCost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(-expectedLabor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void AddElecMiscToMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc elecMisc = new TECMisc(CostType.Electrical);
            elecMisc.Cost = 129.3;
            elecMisc.Labor = 12.3;
            elecMisc.Quantity = 3;

            double expectedCost = elecMisc.Cost * elecMisc.Quantity;
            double expectedLabor = elecMisc.Labor * elecMisc.Quantity;

            //Act
            CostBatch delta = miscVM.AddCost(elecMisc);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(expectedCost, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(expectedLabor, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(expectedCost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(expectedLabor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void RemoveElecMiscFromMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc elecMisc = new TECMisc(CostType.Electrical);
            elecMisc.Cost = 395.4;
            elecMisc.Labor = 843.45;
            elecMisc.Quantity = 3;

            double expectedCost = elecMisc.Cost * elecMisc.Quantity;
            double expectedLabor = elecMisc.Labor * elecMisc.Quantity;

            //Act
            miscVM.AddCost(elecMisc);
            CostBatch delta = miscVM.RemoveCost(elecMisc);

            //Assert
            //Check Misc properties
            Assert.AreEqual(0, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(0, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(-expectedCost, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(-expectedLabor, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void ChangeCostQuantityInMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc misc = new TECMisc(CostType.TEC);
            misc.Cost = 942.2;
            misc.Labor = 375.23;
            misc.Quantity = 3;

            //Act
            miscVM.AddCost(misc);
            int deltaQuantity = 2;
            CostBatch delta = miscVM.ChangeQuantity(misc, deltaQuantity);

            int finalQuantity = misc.Quantity + deltaQuantity;
            double expectedCost = finalQuantity * misc.Cost;
            double expectedLabor = finalQuantity * misc.Labor;

            double expectedDeltaCost = deltaQuantity * misc.Cost;
            double expectedDeltaLabor = deltaQuantity * misc.Labor;

            //Check Misc properties
            Assert.AreEqual(expectedCost, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(expectedLabor, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(expectedDeltaCost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(expectedDeltaLabor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        [TestMethod]
        public void UpdateCostInMiscCostsVM()
        {
            //Arrange
            MiscCostsSummaryVM miscVM = new MiscCostsSummaryVM();
            TECMisc misc = new TECMisc(CostType.TEC);
            misc.Cost = 129.3;
            misc.Labor = 532.54;
            misc.Quantity = 3;

            //Act
            miscVM.AddCost(misc);
            double deltaCost = 121;
            double deltaLabor = 45.6;
            misc.Cost += deltaCost;
            misc.Labor += deltaLabor;

            CostBatch delta = miscVM.UpdateCost(misc);

            double expectedCost = misc.Cost * misc.Quantity;
            double expectedLabor = misc.Labor * misc.Quantity;

            double expectedDeltaCost = deltaCost * misc.Quantity;
            double expectedDeltaLabor = deltaLabor * misc.Quantity;

            //Assert
            //Check Misc properties
            Assert.AreEqual(expectedCost, miscVM.MiscTECCostTotal, GeneralTestingUtilities.DELTA, "MiscTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(expectedLabor, miscVM.MiscTECLaborTotal, GeneralTestingUtilities.DELTA, "MiscTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecCostTotal, GeneralTestingUtilities.DELTA, "MiscElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.MiscElecLaborTotal, GeneralTestingUtilities.DELTA, "MiscElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check Assoc properties
            Assert.AreEqual(0, miscVM.AssocTECCostTotal, GeneralTestingUtilities.DELTA, "AssocTECCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocTECLaborTotal, GeneralTestingUtilities.DELTA, "AssocTECLaborTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecCostTotal, GeneralTestingUtilities.DELTA, "AssocElecCostTotal in MiscCostsSummaryVM is wrong.");
            Assert.AreEqual(0, miscVM.AssocElecLaborTotal, GeneralTestingUtilities.DELTA, "AssocElecLaborTotal in MiscCostsSummaryVM is wrong.");
            //Check returned delta
            Assert.AreEqual(expectedDeltaCost, delta.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC cost delta is wrong.");
            Assert.AreEqual(expectedDeltaLabor, delta.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA, "Returned TEC labor delta is wrong.");
            Assert.AreEqual(0, delta.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec cost delta is wrong.");
            Assert.AreEqual(0, delta.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA, "Returned Elec labor delta is wrong.");
        }
        #endregion

        private static void AssertCostBatchesEqual(CostBatch expected, CostBatch actual)
        {
            Assert.AreEqual(expected.GetCost(CostType.TEC), actual.GetCost(CostType.TEC), GeneralTestingUtilities.DELTA);
            Assert.AreEqual(expected.GetCost(CostType.Electrical), actual.GetCost(CostType.Electrical), GeneralTestingUtilities.DELTA);
            Assert.AreEqual(expected.GetLabor(CostType.TEC), actual.GetLabor(CostType.TEC), GeneralTestingUtilities.DELTA);
            Assert.AreEqual(expected.GetLabor(CostType.Electrical), actual.GetLabor(CostType.Electrical), GeneralTestingUtilities.DELTA);
        }
    }
}
