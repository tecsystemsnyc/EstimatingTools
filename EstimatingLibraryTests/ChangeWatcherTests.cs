﻿using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tests.CostTestingUtilities;
using static Tests.TestHelper;

namespace Tests
{
    [TestClass]
    public class ChangeWatcherTests
    {
        private TECBid bid;
        private ChangeWatcher cw;

        private TECTemplates templates;
        private ChangeWatcher tcw;

        private TECChangedEventArgs changedArgs;
        private TECChangedEventArgs instanceChangedArgs;
        private CostBatch costDelta;
        private int pointDelta;

        private bool changedRaised;
        private bool instanceChangedRaised;
        private bool costChangedRaised;
        private bool pointChangedRaised;

        [TestInitialize]
        public void TestInitialize()
        {
            bid = TestHelper.CreateTestBid();
            cw = new ChangeWatcher(bid);
            
            templates = TestHelper.CreateTestTemplates();
            tcw = new ChangeWatcher(templates);

            resetRaised();

            cw.Changed += (args) =>
            {
                changedArgs = args;
                changedRaised = true;
            };
            cw.InstanceChanged += (args) =>
            {
                instanceChangedArgs = args;
                instanceChangedRaised = true;
            };
            cw.CostChanged += (costs) =>
            {
                costDelta = costs;
                costChangedRaised = true;
            };
            cw.PointChanged += (numPoints) =>
            {
                pointDelta = numPoints;
                pointChangedRaised = true;
            };
            
            tcw.Changed += (args) =>
            {
                changedArgs = args;
                changedRaised = true;
            };
            tcw.InstanceChanged += (args) =>
            {
                instanceChangedArgs = args;
                instanceChangedRaised = true;
            };
            tcw.CostChanged += (costs) =>
            {
                costDelta = costs;
                costChangedRaised = true;
            };
            tcw.PointChanged += (numPoints) =>
            {
                pointDelta = numPoints;
                pointChangedRaised = true;
            };

        }

        #region Change Events Tests
        #region Add Tests
        [TestMethod]
        public void AddTypicalToBid()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            //Ensure typical has points and cost:
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);

            resetRaised();

            //Act
            bid.Systems.Add(typical);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Systems", bid, typical);
        }

        [TestMethod]
        public void AddControllerToBid()
        {
            //Arrange
            TECControllerType controllerType = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(controllerType, false);

            //Act
            bid.Controllers.Add(controller);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "Controllers", bid, controller);
            checkCostDelta(controller.CostBatch);
        }

        [TestMethod]
        public void AddPanelToBid()
        {
            //Arrange
            TECPanelType panelType = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(panelType, false);

            //Act
            bid.Panels.Add(panel);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "Panels", bid, panel);
            checkCostDelta(panel.CostBatch);
        }

        [TestMethod]
        public void AddMiscToBid()
        {
            //Arrange
            TECMisc misc = new TECMisc(CostType.TEC, false);
            misc.Cost = 165.46;
            misc.Labor = 25.11;
            misc.Quantity = 3;

            //Act
            bid.MiscCosts.Add(misc);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "MiscCosts", bid, misc);
            checkCostDelta(misc.CostBatch);
        }

        [TestMethod]
        public void AddScopeBranchToBid()
        {
            //Arrange
            TECScopeBranch sb = new TECScopeBranch(false);

            //Act
            bid.ScopeTree.Add(sb);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Add, "ScopeTree", bid, sb);
        }

        [TestMethod]
        public void AddNoteToBid()
        {
            //Arrange
            TECLabeled note = new TECLabeled();

            //Act
            bid.Notes.Add(note);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Add, "Notes", bid, note);
        }

        [TestMethod]
        public void AddExclusionToBid()
        {
            //Arrange
            TECLabeled exclusion = new TECLabeled();

            //Act
            bid.Exclusions.Add(exclusion);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Add, "Exclusions", bid, exclusion);
        }

        [TestMethod]
        public void AddLocationToBid()
        {
            //Arrange
            TECLabeled location = new TECLabeled();

            //Act
            bid.Locations.Add(location);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Add, "Locations", bid, location);
        }

        [TestMethod]
        public void AddInstanceToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            //Ensure typical has points and cost:
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);

            resetRaised();

            //Act
            TECSystem instance = typical.AddInstance(bid);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Add, "Instances", typical, instance);
            checkCostDelta(instance.CostBatch);
            checkPointDelta(instance.PointNumber);
        }

        [TestMethod]
        public void AddEquipmentToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equip = new TECEquipment(true);
            //Ensure equip has points and cost:
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);

            resetRaised();

            //Act
            typical.Equipment.Add(equip);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Equipment", typical, equip);
        }

        [TestMethod]
        public void AddControllerToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(type, true);

            resetRaised();

            //Act
            typical.Controllers.Add(controller);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Controllers", typical, controller);
        }

        [TestMethod]
        public void AddPanelToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECPanelType type = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(type, true);

            resetRaised();

            //Act
            typical.Panels.Add(panel);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Panels", typical, panel);
        }

        [TestMethod]
        public void AddMiscCostToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECMisc misc = new TECMisc(CostType.TEC, true);
            misc.Cost = 46.49;
            misc.Labor = 97.4;
            misc.Quantity = 3;

            resetRaised();

            //Act
            typical.MiscCosts.Add(misc);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "MiscCosts", typical, misc);
        }

        [TestMethod]
        public void AddAssociatedCostToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECCost assCost = new TECCost(CostType.TEC);
            assCost.Cost = 79.23;
            assCost.Labor = 84.69;

            resetRaised();

            //Act
            typical.AssociatedCosts.Add(assCost);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "AssociatedCosts", typical, assCost);
        }

        [TestMethod]
        public void AddScopeBranchToTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECScopeBranch sb = new TECScopeBranch(true);

            resetRaised();

            //Act
            typical.ScopeBranches.Add(sb);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "ScopeBranches", typical, sb);
        }

        [TestMethod]
        public void AddEquipmentToInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);

            resetRaised();

            //Act
            instance.Equipment.Add(equip);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Add, "Equipment", instance, equip);
            checkCostDelta(equip.CostBatch);
            checkPointDelta(equip.PointNumber);
        }

        [TestMethod]
        public void AddControllerToInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(type, false);

            resetRaised();

            //Act
            instance.Controllers.Add(controller);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "Controllers", instance, controller);
            checkCostDelta(controller.CostBatch);
        }

        [TestMethod]
        public void AddPanelToInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECPanelType panelType = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(panelType, false);

            resetRaised();

            //Act
            instance.Panels.Add(panel);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "Panels", instance, panel);
            checkCostDelta(panel.CostBatch);
        }

        [TestMethod]
        public void AddMiscCostToInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECMisc misc = new TECMisc(CostType.TEC, false);
            misc.Cost = 97.43;
            misc.Labor = 46.15;
            misc.Quantity = 8;

            resetRaised();

            //Act
            instance.MiscCosts.Add(misc);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "MiscCosts", instance, misc);
            checkCostDelta(misc.CostBatch);
        }

        [TestMethod]
        public void AddSubScopeToTypicalEquipment()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.BI;
            point.Quantity = 2;
            ss.Points.Add(point);

            resetRaised();

            //Act
            equip.SubScope.Add(ss);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "SubScope", equip, ss);
        }

        [TestMethod]
        public void AddAssociatedCostToTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            typical.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            equip.SubScope.Add(ss);
            TECCost assCost = bid.Catalogs.AssociatedCosts[0];
            bid.Systems.Add(typical);

            resetRaised();
            cw = new ChangeWatcher(bid);
            resetRaised();

            //Act
            ss.AssociatedCosts.Add(assCost);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "AssociatedCosts", ss, assCost);
        }

        [TestMethod]
        public void AddDeviceToTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);

            TECDevice dev = bid.Catalogs.Devices[0];

            resetRaised();

            //Act
            ss.Devices.Add(dev);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Devices", ss, dev);
        }

        [TestMethod]
        public void AddPointToTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;

            resetRaised();

            //Act
            ss.Points.Add(point);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "Points", ss, point);
        }

        [TestMethod]
        public void AddSubScopeToInstanceEquipment()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            instance.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(false);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            resetRaised();

            //Act
            equip.SubScope.Add(ss);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Add, "SubScope", equip, ss);
            checkCostDelta(ss.CostBatch);
            checkPointDelta(ss.PointNumber);
        }

        [TestMethod]
        public void AddAssociatedCostToInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            instance.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(false);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            equip.SubScope.Add(ss);
            TECCost assCost = bid.Catalogs.AssociatedCosts[0];

            resetRaised();

            //Act

            ss.AssociatedCosts.Add(assCost);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "AssociatedCosts", ss, assCost);
            checkCostDelta(assCost.CostBatch);
        }

        [TestMethod]
        public void AddDeviceToInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            equip.SubScope.Add(ss);
            instance.Equipment.Add(equip);

            TECDevice dev = bid.Catalogs.Devices[0];

            resetRaised();

            //Act
            ss.Devices.Add(dev);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "Devices", ss, dev);
            checkCostDelta(dev.CostBatch);
        }

        [TestMethod]
        public void AddPointToInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            equip.SubScope.Add(ss);
            instance.Equipment.Add(equip);

            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;

            resetRaised();

            //Act
            ss.Points.Add(point);

            //Assert
            checkRaised(true, false, true);
            checkInstanceChangedArgs(Change.Add, "Points", ss, point);
            checkPointDelta(point.PointNumber);
        }

        [TestMethod]
        public void AddNetworkConnectionToBidController()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);

            TECController childController = new TECController(type, false);
            bid.Controllers.Add(childController);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];

            resetRaised();

            //Act
            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "ChildrenConnections", parentController, netConnect);
            checkCostDelta(netConnect.CostBatch);
        }

        [TestMethod]
        public void AddNetworkConnectionToInstanceController()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECController childController = new TECController(type, false);
            typical.Controllers.Add(childController);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];
            
            resetRaised();

            //Act
            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "ChildrenConnections", parentController, netConnect);
            checkCostDelta(netConnect.CostBatch);
        }

        [TestMethod]
        public void AddInstanceSubScopeConnectionToBidController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);
            system.AddInstance(bid);
            TECSubScope instanceSubScope = system.Instances[0].Equipment[0].SubScope[0];

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);

            resetRaised();

            //Act
            TECSubScopeConnection connection = controller.AddSubScope(instanceSubScope);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "ChildrenConnections", controller, connection);
            checkCostDelta(connection.CostBatch);
        }

        [TestMethod]
        public void AddTypicalSubScopeConnectionToTypicalController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);
           
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], true);
            system.Controllers.Add(controller);

            resetRaised();

            //Act
            TECSubScopeConnection connection = controller.AddSubScope(subScope);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "ChildrenConnections", controller, connection);
        }

        [TestMethod]
        public void AddTypicalSubScopeConnectionToBidController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);

            resetRaised();

            //Act
            TECSubScopeConnection connection = controller.AddSubScope(subScope);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Add, "ChildrenConnections", controller, connection);
        }

        [TestMethod]
        public void AddInstanceSubScopeConnectionToInstanceController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            system.Controllers.Add(controller);
            bid.Systems.Add(system);
            TECSystem instance = system.AddInstance(bid);
            TECSubScope instanceSubScope = instance.Equipment[0].SubScope[0];
            TECController instanceController = instance.Controllers[0];

            resetRaised();

            //Act
            TECSubScopeConnection connection = instanceController.AddSubScope(instanceSubScope);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Add, "ChildrenConnections", instanceController, connection);
            checkCostDelta(connection.CostBatch);
        }

        [TestMethod]
        public void AddControllerToNetworkConnection()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);

            TECController childController = new TECController(type, false);
            bid.Controllers.Add(childController);

            TECController daisyController = new TECController(type, false);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];
            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);

            resetRaised();

            //Act
            parentController.AddController(daisyController, netConnect);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Add, "ChildrenControllers", netConnect, daisyController);
        }
        #endregion

        #region Remove Tests
        [TestMethod]
        public void RemoveTypicalFromBid()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            //Ensure typical has points and cost:
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);

            resetRaised();

            //Act
            bid.Systems.Remove(typical);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Systems", bid, typical);
        }

        [TestMethod]
        public void RemoveControllerFormBid()
        {
            //Arrange
            TECControllerType controllerType = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(controllerType, false);
            bid.Controllers.Add(controller);

            resetRaised();

            //Act
            bid.Controllers.Remove(controller);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "Controllers", bid, controller);
            checkCostDelta(controller.CostBatch * -1);
        }

        [TestMethod]
        public void RemovePanelFromBid()
        {
            //Arrange
            TECPanelType panelType = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(panelType, false);
            bid.Panels.Add(panel);

            resetRaised();
            //Act
            bid.Panels.Remove(panel);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "Panels", bid, panel);
            checkCostDelta(panel.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveMiscFromBid()
        {
            //Arrange
            TECMisc misc = new TECMisc(CostType.TEC, false);
            misc.Cost = 48.63;
            misc.Labor = 964.45;
            misc.Quantity = 6;
            bid.MiscCosts.Add(misc);

            resetRaised();
            //Act
            bid.MiscCosts.Remove(misc);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "MiscCosts", bid, misc);
            checkCostDelta(misc.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveScopeBranchFromBid()
        {
            //Arrange
            TECScopeBranch sb = new TECScopeBranch(false);
            bid.ScopeTree.Add(sb);

            resetRaised();
            //Act
            bid.ScopeTree.Remove(sb);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Remove, "ScopeTree", bid, sb);
        }

        [TestMethod]
        public void RemoveNoteFromBid()
        {
            //Arrange
            TECLabeled note = new TECLabeled();
            bid.Notes.Add(note);

            resetRaised();
            //Act
            bid.Notes.Remove(note);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Remove, "Notes", bid, note);
        }

        [TestMethod]
        public void RemoevExclusionFromBid()
        {
            //Arrange
            TECLabeled exclusion = new TECLabeled();
            bid.Exclusions.Add(exclusion);

            resetRaised();
            //Act
            bid.Exclusions.Remove(exclusion);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Remove, "Exclusions", bid, exclusion);
        }

        [TestMethod]
        public void RemoveLocationFromBid()
        {
            //Arrange
            TECLabeled location = new TECLabeled();
            bid.Locations.Add(location);

            resetRaised();

            //Act
            bid.Locations.Remove(location);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Remove, "Locations", bid, location);
        }

        [TestMethod]
        public void RemoveInstanceFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            //Ensure typical has points and cost:
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);

            resetRaised();

            //Act
            typical.Instances.Remove(instance);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Remove, "Instances", typical, instance);
            checkCostDelta(instance.CostBatch * -1);
            checkPointDelta(instance.PointNumber * -1);
        }

        [TestMethod]
        public void RemoveEquipmentFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equip = new TECEquipment(true);
            //Ensure equip has points and cost:
            TECSubScope ss = new TECSubScope(true);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);

            resetRaised();

            //Act
            typical.Equipment.Remove(equip);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Equipment", typical, equip);
        }

        [TestMethod]
        public void RemoveControllerFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(type, true);
            typical.Controllers.Add(controller);

            resetRaised();

            //Act
            typical.Controllers.Remove(controller);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Controllers", typical, controller);
        }

        [TestMethod]
        public void RemovePanelFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECPanelType type = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(type, true);
            typical.Panels.Add(panel);

            resetRaised();

            //Act
            typical.Panels.Remove(panel);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Panels", typical, panel);
        }

        [TestMethod]
        public void RemoveMiscCostFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECMisc misc = new TECMisc(CostType.TEC, true);
            misc.Cost = 64.19;
            misc.Labor = 74.85;
            misc.Quantity = 7;

            typical.MiscCosts.Add(misc);

            resetRaised();

            //Act
            typical.MiscCosts.Remove(misc);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "MiscCosts", typical, misc);
        }

        [TestMethod]
        public void RemoveAssociatedCostFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECCost cost = new TECCost(CostType.TEC);
            cost.Cost = 15.45;
            cost.Labor = 67.41;
            typical.AssociatedCosts.Add(cost);

            resetRaised();

            //Act
            typical.AssociatedCosts.Remove(cost);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "AssociatedCosts", typical, cost);
        }

        [TestMethod]
        public void RemoveScopeBranchFromTypicalSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECScopeBranch sb = new TECScopeBranch(true);
            typical.ScopeBranches.Add(sb);

            resetRaised();

            //Act
            typical.ScopeBranches.Remove(sb);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "ScopeBranches", typical, sb);
        }

        [TestMethod]
        public void RemoveEquipmentFromInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            instance.Equipment.Add(equip);

            resetRaised();

            //Act
            instance.Equipment.Remove(equip);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Remove, "Equipment", instance, equip);
            checkCostDelta(equip.CostBatch * -1);
            checkPointDelta(equip.PointNumber * -1);
        }

        [TestMethod]
        public void RemoveControllerFromInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController controller = new TECController(type, false);
            instance.Controllers.Add(controller);

            resetRaised();

            //Act
            instance.Controllers.Remove(controller);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "Controllers", instance, controller);
            checkCostDelta(controller.CostBatch * -1);
        }

        [TestMethod]
        public void RemovePanelFromInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECPanelType panelType = bid.Catalogs.PanelTypes[0];
            TECPanel panel = new TECPanel(panelType, false);
            instance.Panels.Add(panel);

            resetRaised();

            //Act
            instance.Panels.Remove(panel);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "Panels", instance, panel);
            checkCostDelta(panel.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveMiscCostFromInstanceSystem()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECMisc misc = new TECMisc(CostType.TEC, false);
            misc.Cost = 31.11;
            misc.Labor = 49.73;
            misc.Quantity = 5;
            instance.MiscCosts.Add(misc);

            resetRaised();

            //Act
            instance.MiscCosts.Remove(misc);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "MiscCosts", instance, misc);
            checkCostDelta(misc.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveSubScopeFromTypicalEquipment()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.BI;
            point.Quantity = 2;
            ss.Points.Add(point);
            equip.SubScope.Add(ss);

            resetRaised();

            //Act
            equip.SubScope.Remove(ss);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "SubScope", equip, ss);
        }

        [TestMethod]
        public void RemoveAssociatedCostToTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equip = new TECEquipment(true);
            typical.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            equip.SubScope.Add(ss);
            TECCost assCost = bid.Catalogs.AssociatedCosts[0];
            ss.AssociatedCosts.Add(assCost);

            resetRaised();

            //Act

            ss.AssociatedCosts.Remove(assCost);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "AssociatedCosts", ss, assCost);
        }

        [TestMethod]
        public void RemoveDeviceFromTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);

            resetRaised();

            //Act
            ss.Devices.Remove(dev);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Devices", ss, dev);
        }

        [TestMethod]
        public void RemovePointFromTypicalSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            bid.Systems.Add(typical);
            TECPoint point = new TECPoint(true);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            resetRaised();

            //Act
            ss.Points.Remove(point);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "Points", ss, point);
        }

        [TestMethod]
        public void RemoveSubScopeFromInstanceEquipment()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            instance.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(false);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);
            equip.SubScope.Add(ss);

            resetRaised();

            //Act
            equip.SubScope.Remove(ss);

            //Assert
            checkRaised(true, true, true);
            checkInstanceChangedArgs(Change.Remove, "SubScope", equip, ss);
            checkCostDelta(ss.CostBatch * -1);
            checkPointDelta(ss.PointNumber * -1);
        }

        [TestMethod]
        public void RemoveAssociatedCostFromInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            instance.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope(false);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            equip.SubScope.Add(ss);
            TECCost assCost = bid.Catalogs.AssociatedCosts[0];
            ss.AssociatedCosts.Add(assCost);

            resetRaised();

            //Act

            ss.AssociatedCosts.Remove(assCost);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "AssociatedCosts", ss, assCost);
            checkCostDelta(assCost.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveDeviceFromInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            equip.SubScope.Add(ss);
            instance.Equipment.Add(equip);

            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);

            resetRaised();

            //Act
            ss.Devices.Remove(dev);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "Devices", ss, dev);
            checkCostDelta(dev.CostBatch * -1);
        }

        [TestMethod]
        public void RemovePointFromInstanceSubScope()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance(bid);
            TECEquipment equip = new TECEquipment(false);
            TECSubScope ss = new TECSubScope(false);
            equip.SubScope.Add(ss);
            instance.Equipment.Add(equip);

            TECPoint point = new TECPoint(false);
            point.Type = PointTypes.AI;
            point.Quantity = 2;
            ss.Points.Add(point);

            resetRaised();

            //Act
            ss.Points.Remove(point);

            //Assert
            checkRaised(true, false, true);
            checkInstanceChangedArgs(Change.Remove, "Points", ss, point);
            checkPointDelta(point.PointNumber * -1);
        }

        [TestMethod]
        public void RemoveNetworkConnectionFromBidController()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);

            TECController childController = new TECController(type, false);
            bid.Controllers.Add(childController);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];

            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);

            resetRaised();

            //Act
            parentController.RemoveController(childController);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "ChildrenConnections", parentController, netConnect);
            checkCostDelta(netConnect.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveNetworkConnectionFromInstanceController()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);
            
            TECTypical system = new TECTypical();
            bid.Systems.Add(system);

            TECController childController = new TECController(type, false);
            system.Controllers.Add(childController);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];

            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);
            netConnect.Length = 10;

            resetRaised();

            //Act
            parentController.RemoveController(childController);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "ChildrenConnections", parentController, netConnect);
            checkCostDelta(netConnect.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveSubScopeConnectionFromBidController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);
            system.AddInstance(bid);
            TECSubScope instanceSubScope = system.Instances[0].Equipment[0].SubScope[0];

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            TECSubScopeConnection connection = controller.AddSubScope(instanceSubScope);
            connection.Length = 10;
            resetRaised();

            //Act
            controller.RemoveSubScope(instanceSubScope);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "ChildrenConnections", controller, connection);
            checkCostDelta(connection.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveSubScopeConnectionFromTypicalController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            system.Controllers.Add(controller);
            TECSubScopeConnection connection = controller.AddSubScope(subScope);
            connection.Length = 10;
            resetRaised();

            //Act
            controller.RemoveSubScope(subScope);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "ChildrenConnections", controller, connection);
        }

        [TestMethod]
        public void RemoveBidSubScopeConnectionFromTypicalController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            bid.Systems.Add(system);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            TECSubScopeConnection connection = controller.AddSubScope(subScope);
            connection.Length = 10;
            resetRaised();

            //Act
            controller.RemoveSubScope(subScope);

            //Assert
            checkRaised(false, false, false);
            checkChangedArgs(Change.Remove, "ChildrenConnections", controller, connection);
        }

        [TestMethod]
        public void RemoveSubScopeConnectionFromInstanceController()
        {
            //Arrange
            TECTypical system = new TECTypical();
            TECEquipment equipment = new TECEquipment(true);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            system.Equipment.Add(equipment);
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], true);
            system.Controllers.Add(controller);
            bid.Systems.Add(system);
            TECSystem instance = system.AddInstance(bid);
            TECSubScope instanceSubScope = instance.Equipment[0].SubScope[0];
            TECController instanceController = instance.Controllers[0];
            TECSubScopeConnection connection = instanceController.AddSubScope(instanceSubScope);
            connection.Length = 10;
            resetRaised();

            //Act
            instanceController.RemoveSubScope(instanceSubScope);

            //Assert
            checkRaised(true, true, false);
            checkInstanceChangedArgs(Change.Remove, "ChildrenConnections", instanceController, connection);
            checkCostDelta(connection.CostBatch * -1);
        }

        [TestMethod]
        public void RemoveControllerFromNetworkConnection()
        {
            //Arrange
            TECControllerType type = bid.Catalogs.ControllerTypes[0];
            TECController parentController = new TECController(type, false);
            bid.Controllers.Add(parentController);

            TECController childController = new TECController(type, false);
            bid.Controllers.Add(childController);

            TECController daisyController = new TECController(type, false);

            TECElectricalMaterial connectionType = bid.Catalogs.ConnectionTypes[0];
            TECNetworkConnection netConnect = parentController.AddController(childController, connectionType);
            parentController.AddController(daisyController, netConnect);

            resetRaised();

            //Act
            netConnect.ChildrenControllers.Remove(daisyController);

            //Assert
            checkRaised(true, false, false);
            checkInstanceChangedArgs(Change.Remove, "ChildrenControllers", netConnect, daisyController);
        }
        #endregion

        #region Edit Tests
        [TestMethod]
        public void EditBid()
        {
            //Arrange
            var original = bid.Name;
            var edited = "edit";
            resetRaised();

            //Act
            bid.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", bid, edited, original);
        }

        [TestMethod]
        public void EditBidController()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            controller.Name = original;
            bid.Controllers.Add(controller);

            resetRaised();

            //Act
            controller.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", controller, edited, original);
        }

        [TestMethod]
        public void EditBidControllerType()
        {
            //Arrange
            var original = bid.Catalogs.ControllerTypes[0];
            var edited = new TECControllerType(bid.Catalogs.Manufacturers[0]);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            bid.Catalogs.ControllerTypes.Add(edited);

            resetRaised();

            //Act
            controller.Type = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Type", controller, edited, original);
        }

        [TestMethod]
        public void EditBidNetworkConnection()
        {
            //Arrange
            Double original = 10;
            Double edited = 12;

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            TECController child = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            bid.Controllers.Add(child);
            TECNetworkConnection connection = controller.AddController(child, bid.Catalogs.ConnectionTypes[0]);
            connection.Length = original;

            resetRaised();

            //Act
            connection.Length = 12;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Length", connection, edited, original);
        }

        [TestMethod]
        public void EditBidNetworkConnectionConnectionType()
        {
            //Arrange
            var original = bid.Catalogs.ConnectionTypes[0];
            var edited = new TECElectricalMaterial();
            bid.Catalogs.ConnectionTypes.Add(edited);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            TECController child = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            bid.Controllers.Add(child);
            TECNetworkConnection connection = controller.AddController(child, original);

            resetRaised();

            //Act
            connection.ConnectionType = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "ConnectionType", connection, edited, original);
        }

        [TestMethod]
        public void EditBidNetworkConnectionConduitType()
        {
            //Arrange
            var original = bid.Catalogs.ConduitTypes[0];
            var edited = new TECElectricalMaterial();
            bid.Catalogs.ConduitTypes.Add(edited);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            TECController child = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);
            bid.Controllers.Add(child);
            TECNetworkConnection connection = controller.AddController(child, bid.Catalogs.ConnectionTypes[0]);
            connection.ConduitType = original;
            resetRaised();

            //Act
            connection.ConduitType = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "ConduitType", connection, edited, original);
        }

        [TestMethod]
        public void EditBidPanel()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECPanel panel = new TECPanel(bid.Catalogs.PanelTypes[0], false);
            panel.Name = original;
            bid.Panels.Add(panel);

            resetRaised();

            //Act
            panel.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", panel, edited, original);
        }

        [TestMethod]
        public void EditBidPanelType()
        {
            //Arrange
            var original = bid.Catalogs.PanelTypes[0];
            var edited = new TECPanelType(bid.Catalogs.Manufacturers[0]);

            TECPanel panel= new TECPanel(bid.Catalogs.PanelTypes[0], false);
            bid.Panels.Add(panel);
            bid.Catalogs.PanelTypes.Add(edited);

            resetRaised();

            //Act
            panel.Type = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Type", panel, edited, original);
        }

        [TestMethod]
        public void EditBidMisc()
        {
            //Arrange
            var original = 2;
            var edited = 3;

            TECMisc misc = new TECMisc(CostType.TEC, false);
            misc.Quantity = original;
            bid.MiscCosts.Add(misc);

            resetRaised();

            //Act
            misc.Quantity = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Quantity", misc, edited, original);
        }

        [TestMethod]
        public void EditBidScopeBranch()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECScopeBranch branch = new TECScopeBranch(false);
            branch.Label = original;
            bid.ScopeTree.Add(branch);

            resetRaised();

            //Act
            branch.Label = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", branch, edited, original);
        }

        [TestMethod]
        public void EditBidParamters()
        {
            //Arrange
            var original = bid.Parameters;
            var edited = new TECParameters(bid.Guid);

            resetRaised();

            //Act
            bid.Parameters = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Parameters", bid, edited, original);
        }

        [TestMethod]
        public void EditBidExtraLabor()
        {
            //Arrange
            var original = bid.ExtraLabor;
            var edited = new TECExtraLabor(bid.Guid);

            resetRaised();

            //Act
            bid.ExtraLabor = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "ExtraLabor", bid, edited, original);
        }

        [TestMethod]
        public void EditTypical()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            typical.Name = original;
            bid.Systems.Add(typical);

            resetRaised();

            //Act
            typical.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", typical, edited, original);
        }

        [TestMethod]
        public void EditTypicalController()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], true);
            controller.Name = original;
            typical.Controllers.Add(controller);

            resetRaised();

            //Act
            controller.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", controller, edited, original);
        }

        [TestMethod]
        public void EditTypicalPanel()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECPanel panel = new TECPanel(bid.Catalogs.PanelTypes[0], true);
            panel.Name = original;
            typical.Panels.Add(panel);

            resetRaised();

            //Act
            panel.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", panel, edited, original);
        }

        [TestMethod]
        public void EditTypicalMisc()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECMisc misc = new TECMisc(CostType.TEC, true);
            misc.Name = original;
            typical.MiscCosts.Add(misc);

            resetRaised();

            //Act
            misc.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", misc, edited, original);
        }

        [TestMethod]
        public void EditTypicalScopeBranch()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECScopeBranch branch = new TECScopeBranch(true);
            branch.Label = original;
            typical.ScopeBranches.Add(branch);

            resetRaised();

            //Act
            branch.Label = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", branch, edited, original);
        }

        [TestMethod]
        public void EditTypicalEquipment()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            equipment.Name = original;
            typical.Equipment.Add(equipment);

            resetRaised();

            //Act
            equipment.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", equipment, edited, original);
        }

        [TestMethod]
        public void EditTypicalSubScope()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSubScope subScope = new TECSubScope(true);
            subScope.Name = original;
            equipment.SubScope.Add(subScope);

            resetRaised();

            //Act
            subScope.Name = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", subScope, edited, original);
        }

        [TestMethod]
        public void EditTypicalPoint()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            TECPoint point = new TECPoint(true);
            subScope.Points.Add(point);
            point.Label = original;

            resetRaised();

            //Act
            point.Label = edited;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", point, edited, original);
        }

        [TestMethod]
        public void EditTypicalSubScopeConnectionInBidController()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);

            TECElectricalMaterial conduitType = bid.Catalogs.ConduitTypes[0];

            bid.Systems.Add(typical);
            TECConnection connection = controller.AddSubScope(ss);
            connection.Length = 16.46;
            connection.ConduitLength = 81.64;

            resetRaised();

            //Act
            connection.ConduitType = conduitType;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "ConduitType", connection, conduitType, null);
        }

        [TestMethod]
        public void EditTypicalSubScopeConnectionInTypicalController()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment(true);
            TECSubScope ss = new TECSubScope(true);
            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], true);
            typical.Controllers.Add(controller);

            TECElectricalMaterial conduitType = bid.Catalogs.ConduitTypes[0];

            bid.Systems.Add(typical);
            TECConnection connection = controller.AddSubScope(ss);
            connection.Length = 16.43;
            connection.ConduitLength = 74.13;

            resetRaised();

            //Act
            connection.ConduitType = conduitType;

            //Assert
            checkRaised(instanceChanged: false, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "ConduitType", connection, conduitType, null);
        }

        [TestMethod]
        public void EditSystem()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem system = typical.AddInstance(bid);
            system.Name = original;

            resetRaised();

            //Act
            system.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkInstanceChangedArgs(Change.Edit, "Name", system, edited, original);
        }

        [TestMethod]
        public void EditSystemLocation()
        {
            //Arrange
            var original = bid.Locations[0];
            var edited = new TECLabeled();
            edited.Label = "edit";
            bid.Locations.Add(edited);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem system = typical.AddInstance(bid);
            system.Location = original;

            resetRaised();

            //Act
            system.Location = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Location", system, edited, original);
        }

        [TestMethod]
        public void EditSystemController()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], true);
            typical.Controllers.Add(controller);
            TECSystem system = typical.AddInstance(bid);
            TECController systemController = system.Controllers[0];
            systemController.Name = original;

            resetRaised();

            //Act
            systemController.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", systemController, edited, original);
        }

        [TestMethod]
        public void EditSystemPanel()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECPanel panel = new TECPanel(bid.Catalogs.PanelTypes[0], true);
            typical.Panels.Add(panel);
            TECSystem system = typical.AddInstance(bid);
            TECPanel systemPanel = system.Panels[0];
            systemPanel.Name = original;

            resetRaised();

            //Act
            systemPanel.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", systemPanel, edited, original);
        }

        [TestMethod]
        public void EditSystemMisc()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECMisc misc = new TECMisc(CostType.TEC, true);
            typical.MiscCosts.Add(misc);
            TECSystem system = typical.AddInstance(bid);
            TECMisc systemMisc = system.MiscCosts[0];
            systemMisc.Name = original;

            resetRaised();

            //Act
            systemMisc.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", systemMisc, edited, original);
        }

        [TestMethod]
        public void EditSystemScopeBranch()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECScopeBranch branch = new TECScopeBranch(true);
            typical.ScopeBranches.Add(branch);
            TECSystem system = typical.AddInstance(bid);
            TECScopeBranch systemScopeBranch = system.ScopeBranches[0];
            systemScopeBranch.Label = original;

            resetRaised();

            //Act
            systemScopeBranch.Label = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", systemScopeBranch, edited, original);
        }

        [TestMethod]
        public void EditSystemEquipment()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSystem system = typical.AddInstance(bid);
            TECEquipment systemEquipment = system.Equipment[0];
            systemEquipment.Name = original;

            resetRaised();

            //Act
            systemEquipment.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", systemEquipment, edited, original);
        }

        [TestMethod]
        public void EditSystemSubScope()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            TECSystem system = typical.AddInstance(bid);
            TECSubScope systemSubScope = system.Equipment[0].SubScope[0];
            systemSubScope.Name = original;

            resetRaised();

            //Act
            systemSubScope.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", systemSubScope, edited, original);
        }

        [TestMethod]
        public void EditSystemPoint()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            TECPoint point = new TECPoint(true);
            subScope.Points.Add(point);
            TECSystem system = typical.AddInstance(bid);
            TECPoint systemPoint = system.Equipment[0].SubScope[0].Points[0];
            systemPoint.Label = original;

            resetRaised();

            //Act
            systemPoint.Label = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", systemPoint, edited, original);
        }

        [TestMethod]
        public void EditSystemPointQuantity()
        {
            //Arrange
            var original = 2;
            var edited = 3;

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECEquipment equipment = new TECEquipment(true);
            typical.Equipment.Add(equipment);
            TECSubScope subScope = new TECSubScope(true);
            equipment.SubScope.Add(subScope);
            TECPoint point = new TECPoint(true);
            subScope.Points.Add(point);
            TECSystem system = typical.AddInstance(bid);
            TECPoint systemPoint = system.Equipment[0].SubScope[0].Points[0];
            point.Quantity = original;

            resetRaised();

            //Act
            point.Quantity = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: true);
            checkChangedArgs(Change.Edit, "Quantity", point, edited, original);
        }

        [TestMethod]
        public void EditSystemSubScopeConnectionInBidController()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment typEquip = new TECEquipment(true);
            TECSubScope typSS = new TECSubScope(true);
            TECDevice typDev = bid.Catalogs.Devices[0];
            typSS.Devices.Add(typDev);
            typEquip.SubScope.Add(typSS);
            typical.Equipment.Add(typEquip);
            bid.Systems.Add(typical);

            TECController controller = new TECController(bid.Catalogs.ControllerTypes[0], false);
            bid.Controllers.Add(controller);

            controller.AddSubScope(typSS);

            TECSystem instance = typical.AddInstance(bid);
            TECConnection ssConnect = instance.Equipment[0].SubScope[0].Connection;
            ssConnect.Length = 461.31;
            ssConnect.ConduitLength = 734.16;

            TECElectricalMaterial conduitType = bid.Catalogs.ConduitTypes[0];

            resetRaised();

            //Act
            ssConnect.ConduitType = conduitType;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkInstanceChangedArgs(Change.Edit, "ConduitType", ssConnect, conduitType, null);
            checkCostDelta(conduitType.GetCosts(ssConnect.ConduitLength));
        }

        [TestMethod]
        public void EditSystemSubScopeConnectionInSystemController()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            TECEquipment typEquip = new TECEquipment(true);
            TECSubScope typSS = new TECSubScope(true);
            TECDevice typDev = bid.Catalogs.Devices[0];
            typSS.Devices.Add(typDev);
            typEquip.SubScope.Add(typSS);
            typical.Equipment.Add(typEquip);
            bid.Systems.Add(typical);

            TECController typController = new TECController(bid.Catalogs.ControllerTypes[0], true);
            typical.Controllers.Add(typController);

            typController.AddSubScope(typSS);

            TECSystem instance = typical.AddInstance(bid);
            TECConnection ssConnect = instance.Equipment[0].SubScope[0].Connection;
            ssConnect.Length = 134.12;
            ssConnect.ConduitLength = 91.15;

            TECElectricalMaterial conduitType = bid.Catalogs.ConduitTypes[0];

            resetRaised();

            //Act
            ssConnect.ConduitType = conduitType;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkInstanceChangedArgs(Change.Edit, "ConduitType", ssConnect, conduitType, null);
            checkCostDelta(conduitType.GetCosts(ssConnect.ConduitLength));
        }

        [TestMethod]
        public void EditHardwareManufacturer()
        {
            //Arrange
            TECDevice device = templates.Catalogs.Devices[0];

            var original = device.Manufacturer;
            var edited = new TECManufacturer();
            edited.Label = "edit";
            templates.Catalogs.Manufacturers.Add(edited);
            
            resetRaised();

            //Act
            device.Manufacturer = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Manufacturer", device, edited, original);
        }

        [TestMethod]
        public void EditValveActuator()
        {
            //Arrange
            TECValve valve = templates.Catalogs.Valves[0];

            var original = valve.Actuator;
            var edited = new TECDevice(templates.Catalogs.Devices[0]);
            edited.Name = "edit";
            templates.Catalogs.Devices.Add(edited);

            resetRaised();

            //Act
            valve.Actuator = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: true, pointChanged: false);
            checkChangedArgs(Change.Edit, "Actuator", valve, edited, original);
        }

        [TestMethod]
        public void EditConnectionType()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECElectricalMaterial material = templates.Catalogs.ConnectionTypes[0];
            material.Name = original;

            resetRaised();

            //Act
            material.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", material, edited, original);
        }

        [TestMethod]
        public void EditConduitType()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECElectricalMaterial material = templates.Catalogs.ConduitTypes[0];
            material.Name = original;

            resetRaised();

            //Act
            material.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", material, edited, original);
        }

        [TestMethod]
        public void EditAssociatedCost()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECCost cost = templates.Catalogs.AssociatedCosts[0];
            cost.Name = original;

            resetRaised();

            //Act
            cost.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", cost, edited, original);
        }

        [TestMethod]
        public void EditPanelType()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECPanelType panelType = templates.Catalogs.PanelTypes[0];
            panelType.Name = original;

            resetRaised();

            //Act
            panelType.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", panelType, edited, original);
        }

        [TestMethod]
        public void EditControllerType()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECControllerType controllerType = templates.Catalogs.ControllerTypes[0];
            controllerType.Name = original;

            resetRaised();

            //Act
            controllerType.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", controllerType, edited, original);
        }

        [TestMethod]
        public void EditIOModule()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECIOModule ioModule = templates.Catalogs.IOModules[0];
            ioModule.Name = original;

            resetRaised();

            //Act
            ioModule.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", ioModule, edited, original);
        }

        [TestMethod]
        public void EditDevice()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECDevice device = templates.Catalogs.Devices[0];
            device.Name = original;

            resetRaised();

            //Act
            device.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", device, edited, original);
        }

        [TestMethod]
        public void EditValve()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECValve valve = templates.Catalogs.Valves[0];
            valve.Name = original;

            resetRaised();

            //Act
            valve.Name = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Name", valve, edited, original);
        }

        [TestMethod]
        public void EditManufacturer()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECManufacturer manufacturer = templates.Catalogs.Manufacturers[0];
            manufacturer.Label = original;

            resetRaised();

            //Act
            manufacturer.Label= edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", manufacturer, edited, original);
        }

        [TestMethod]
        public void EditTags()
        {
            //Arrange
            var original = "original";
            var edited = "edit";

            TECLabeled tag = templates.Catalogs.Tags[0];
            tag.Label = original;

            resetRaised();

            //Act
            tag.Label = edited;

            //Assert
            checkRaised(instanceChanged: true, costChanged: false, pointChanged: false);
            checkChangedArgs(Change.Edit, "Label", tag, edited, original);
        }

        #endregion

        #endregion
        
        private void resetRaised()
        {
            changedRaised = false;
            instanceChangedRaised = false;
            costChangedRaised = false;
            pointChangedRaised = false;
        }

        #region Check Methods
        private void checkRaised(bool instanceChanged, bool costChanged, bool pointChanged)
        {
            Assert.IsTrue(changedRaised, "Changed event on the ChangeWatcher wasn't raised.");

            if (instanceChanged)
            {
                Assert.IsTrue(instanceChangedRaised, "InstanceChanged event on the ChangeWatcher wasn't raised when it should have been.");
            }
            else
            {
                Assert.IsFalse(instanceChangedRaised, "InstanceChanged event on the ChangeWatcher was raised when it shouldn't have been.");
            }
            
            if (costChanged)
            {
                Assert.IsTrue(costChangedRaised, "CostChanged event on the ChangeWatcher wasn't raised when it should have been.");
            }
            else
            {
                Assert.IsFalse(costChangedRaised, "CostChanged event on the ChangeWatcher was raised when it shouldn't have been.");
            }
            
            if (pointChanged)
            {
                Assert.IsTrue(pointChangedRaised, "PointChanged event on the ChangeWatcher wasn't raised when it should have been.");
            }
            else
            {
                Assert.IsFalse(pointChangedRaised, "PointChanged event on the ChangeWatcher was raised when it shouldn't have been.");
            }
        }

        private void checkChangedArgs(Change change, string propertyName, TECObject sender, object value, object oldValue = null)
        {
            Assert.AreEqual(change, changedArgs.Change, "Change type is wrong.");
            Assert.AreEqual(propertyName, changedArgs.PropertyName, "PropertyName is wrong.");
            Assert.AreEqual(sender, changedArgs.Sender, "Sender is wrong.");
            Assert.AreEqual(value, changedArgs.Value, "Value is wrong.");

            if (oldValue != null)
            {
                Assert.AreEqual(oldValue, changedArgs.OldValue, "OldValue is wrong.");
            }
        }
        private void checkInstanceChangedArgs(Change change, string propertyName, TECObject sender, object value, object oldValue = null)
        {
            checkChangedArgs(change, propertyName, sender, value, oldValue);

            Assert.AreEqual(instanceChangedArgs.Change, change, "Change type is wrong.");
            Assert.AreEqual(instanceChangedArgs.PropertyName, propertyName, "PropertyName is wrong.");
            Assert.AreEqual(instanceChangedArgs.Sender, sender, "Sender is wrong.");
            Assert.AreEqual(instanceChangedArgs.Value, value, "Value is wrong.");

            if (oldValue != null)
            {
                Assert.AreEqual(instanceChangedArgs.OldValue, oldValue, "OldValue is wrong.");
            }
        }

        private void checkCostDelta(CostBatch cb)
        {
            Assert.AreEqual(cb.GetCost(CostType.TEC), costDelta.GetCost(CostType.TEC), DELTA, "ChangeWatcher TEC Cost delta is wrong.");
            Assert.AreEqual(cb.GetLabor(CostType.TEC), costDelta.GetLabor(CostType.TEC), DELTA, "ChangeWatcher TEC Labor delta is wrong.");
            Assert.AreEqual(cb.GetCost(CostType.Electrical), costDelta.GetCost(CostType.Electrical), DELTA, "ChangeWatcher Elec Cost delta is wrong.");
            Assert.AreEqual(cb.GetLabor(CostType.Electrical), costDelta.GetLabor(CostType.Electrical), DELTA, "ChangeWatcher Elec Labor delta is wrong.");
        }

        private void checkPointDelta(int points)
        {
            Assert.AreEqual(points, pointDelta, "ChangeWatcher point delta is wrong.");
        }
        #endregion
    }
}
