using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TECUserControlLibrary.ViewModels;
using TestLibrary.ModelTestingUtilities;
using TestLibrary;

namespace ViewModels
{
    /// <summary>
    /// Summary description for SummaryVMTests
    /// </summary>
    [TestClass]
    public class MaterialSummaryVMTests
    {

        TECBid bid;
        Random rand;
        ChangeWatcher cw;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            bid = new TECBid();
            bid.Catalogs = catalogs;
            cw = new ChangeWatcher(bid);
        }

        #region Material Summary VM
        #region Add
        [TestMethod]
        public void AddTECCostToSystem()
        {
            //Arrange
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

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.AssociatedCosts.Add(cost);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, cost.CostBatch);
        }

        [TestMethod]
        public void AddElectricalCostToSystem()
        {
            //Arrange
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

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            TECSystem instance = typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.AssociatedCosts.Add(cost);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, cost.CostBatch);
        }

        [TestMethod]
        public void AddTECMiscToBid()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.TEC);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.MiscCosts.Add(misc);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, misc.CostBatch);
        }

        [TestMethod]
        public void AddElectricalMiscToBid()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.Electrical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.MiscCosts.Add(misc);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, misc.CostBatch);
        }

        [TestMethod]
        public void AddTECMiscToSystem()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.TEC);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.MiscCosts.Add(misc);

            //Arrange
            AssertMaterialVMMatchesCostBatch(matVM, misc.CostBatch);
        }

        [TestMethod]
        public void AddElectricalMiscToSystem()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.Electrical);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);
            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typical.MiscCosts.Add(misc);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, misc.CostBatch);
        }

        [TestMethod]
        public void AddPanel()
        {
            //Arrange
            TECPanel panel = ModelCreation.TestPanel(bid.Catalogs, rand);
            panel.AssignRandomScopeProperties(bid.Catalogs, rand);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.Panels.Add(panel);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, panel.CostBatch);
        }

        [TestMethod]
        public void AddController()
        {
            //Arrange
            TECController controller = ModelCreation.TestProvidedController(bid.Catalogs, rand);
            controller.AssignRandomScopeProperties(bid.Catalogs, rand);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            bid.AddController(controller);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, controller.CostBatch);
        }

        [TestMethod]
        public void AddDevice()
        {
            //Arrange
            TECDevice device = ModelCreation.TestDevice(bid.Catalogs, rand);
            device.AssignRandomScopeProperties(bid.Catalogs, rand);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope();
            typEquip.SubScope.Add(typSS);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typSS.Devices.Add(device);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, device.CostBatch);
        }

        [TestMethod]
        public void AddValve()
        {
            //Arrange
            TECValve valve = ModelCreation.TestValve(bid.Catalogs, rand);
            valve.AssignRandomScopeProperties(bid.Catalogs, rand);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope();
            typEquip.SubScope.Add(typSS);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typSS.Devices.Add(valve);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, valve.CostBatch);
        }

        [TestMethod]
        public void AddSubScope()
        {
            //Arrange
            TECSubScope subscope = ModelCreation.TestSubScope(bid.Catalogs, rand);
            subscope.AssignRandomScopeProperties(bid.Catalogs, rand);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            typEquip.SubScope.Add(subscope);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, subscope.CostBatch);
        }

        [TestMethod]
        public void AddEquipment()
        {
            //Arrange
            TECEquipment equipment = ModelCreation.TestEquipment(bid.Catalogs, rand);
            equipment.AssignRandomScopeProperties(bid.Catalogs, rand);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);
            
            //Act
            typical.Equipment.Add(equipment);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, equipment.CostBatch);
        }

        [TestMethod]
        public void AddInstanceSystem()
        {
            //Arrange
            TECTypical typical = ModelCreation.TestTypical(bid.Catalogs, rand);
            typical.AssignRandomScopeProperties(bid.Catalogs, rand);
            bid.Systems.Add(typical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            TECSystem instance = typical.AddInstance();

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, instance.CostBatch);
        }

        [TestMethod]
        public void AddConnection()
        {
            //Arrange
            TECControllerType controllerType = new TECControllerType(bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.Add(controllerType);
            
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECController controller = new TECProvidedController(controllerType);
            typical.AddController(controller);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope();
            typEquip.SubScope.Add(typSS);

            ObservableCollection<TECConnectionType> connectionTypes = new ObservableCollection<TECConnectionType>();
            connectionTypes.Add(bid.Catalogs.ConnectionTypes[0]);
            TECDevice dev = new TECDevice(connectionTypes, new List<TECProtocol>(), bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.Add(dev);
            typSS.Devices.Add(dev);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            IControllerConnection connection = controller.Connect(typSS, (typSS as IConnectable).AvailableProtocols.First());
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            TECSystem instance = typical.AddInstance();
            TECSubScope instanceSubScope = instance.GetAllSubScope()[0];

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, connection.CostBatch);
        }
        #endregion

        #region Remove
        [TestMethod]
        public void RemoveTECCost()
        {
            //Arrange
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
            system.AddInstance();
            system.AssociatedCosts.Add(cost);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            system.AssociatedCosts.Remove(cost);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - cost.CostBatch);
        }

        [TestMethod]
        public void RemoveElectricalCost()
        {
            //Arrange
            TECAssociatedCost cost = bid.Catalogs.AssociatedCosts.First(x => x.Type == CostType.Electrical);
            TECTypical system = new TECTypical();
            bid.Systems.Add(system);
            system.AddInstance();
            system.AssociatedCosts.Add(cost);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            system.AssociatedCosts.Remove(cost);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - cost.CostBatch);
        }

        [TestMethod]
        public void RemoveTECMisc()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.TEC);
            bid.MiscCosts.Add(misc);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            bid.MiscCosts.Remove(misc);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - misc.CostBatch);
        }

        [TestMethod]
        public void RemoveElectricalMisc()
        {
            //Arrange
            TECMisc misc = ModelCreation.TestMisc(bid.Catalogs, rand, CostType.Electrical);
            bid.MiscCosts.Add(misc);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            bid.MiscCosts.Remove(misc);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - misc.CostBatch);
        }

        [TestMethod]
        public void RemovePanel()
        {
            //Arrange
            TECPanel panel = ModelCreation.TestPanel(bid.Catalogs, rand);
            panel.AssignRandomScopeProperties(bid.Catalogs, rand);
            bid.Panels.Add(panel);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            bid.Panels.Remove(panel);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - panel.CostBatch);
        }

        [TestMethod]
        public void RemoveController()
        {
            //Arrange
            TECController controller = ModelCreation.TestProvidedController(bid.Catalogs, rand);
            controller.AssignRandomScopeProperties(bid.Catalogs, rand);            
            bid.AddController(controller);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            bid.RemoveController(controller);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - controller.CostBatch);
        }

        [TestMethod]
        public void RemoveDevice()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope();
            typEquip.SubScope.Add(typSS);

            TECDevice device = bid.Catalogs.Devices[0];
            device.AssignRandomScopeProperties(bid.Catalogs, rand);
            typSS.Devices.Add(device);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            typSS.Devices.Remove(device);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - device.CostBatch);
        }

        [TestMethod]
        public void RemoveValve()
        {
            //Arrange
            bid.Catalogs.Add(ModelCreation.TestValve(bid.Catalogs, rand));
            ChangeWatcher cw = new ChangeWatcher(bid);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope typSS = new TECSubScope();
            typEquip.SubScope.Add(typSS);

            TECValve valve = bid.Catalogs.Valves[0];
            valve.AssignRandomScopeProperties(bid.Catalogs, rand);
            typSS.Devices.Add(valve);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            typSS.Devices.Remove(valve);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - valve.CostBatch);
        }

        [TestMethod]
        public void RemoveSubScope()
        {
            //Arrange

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope subScope = ModelCreation.TestSubScope(bid.Catalogs, rand);
            subScope.AssignRandomScopeProperties(bid.Catalogs, rand);
            typEquip.SubScope.Add(subScope);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            typEquip.SubScope.Remove(subScope);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - subScope.CostBatch);
        }

        [TestMethod]
        public void RemoveEquipment()
        {
            //Arrange
            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment equip = ModelCreation.TestEquipment(bid.Catalogs, rand);
            equip.AssignRandomScopeProperties(bid.Catalogs, rand);
            typical.Equipment.Add(equip);

            typical.AddInstance();

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            typical.Equipment.Remove(equip);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - equip.CostBatch);
        }

        [TestMethod]
        public void RemoveInstanceSystem()
        {
            //Arrange

            TECTypical typical = ModelCreation.TestTypical(bid.Catalogs, rand);
            bid.Systems.Add(typical);

            TECSystem instance = typical.AddInstance();

            CostBatch instanceCost = instance.CostBatch;

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch initial = MatVMToCostBatch(matVM);

            //Act
            typical.Instances.Remove(instance);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, initial - instanceCost);
        }

        [TestMethod]
        public void RemoveConnection()
        {
            //Arrange
            TECController controller = new TECProvidedController(bid.Catalogs.ControllerTypes[0]);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();
            bid.Systems.Add(typical);

            TECEquipment typEquip = new TECEquipment();
            typical.Equipment.Add(typEquip);

            TECSubScope ss = new TECSubScope();
            ss.Devices.Add(bid.Catalogs.Devices[0]);
            typEquip.SubScope.Add(ss);
            
            TECSystem instance = typical.AddInstance();

            TECSubScope instanceSubScope = instance.GetAllSubScope().First(sub => sub.AvailableProtocols.Any(prot => prot is TECHardwiredProtocol));
            IConnection connection = controller.Connect(instanceSubScope, (instanceSubScope as IConnectable).AvailableProtocols.First(x => x is TECHardwiredProtocol));
            connection.Length = 50;
            connection.ConduitLength = 50;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            CostBatch expected = MatVMToCostBatch(matVM) - connection.CostBatch;
            
            //Act
            controller.Disconnect(instanceSubScope);

            //Assert
            AssertMaterialVMMatchesCostBatch(matVM, expected);
        }
        #endregion

        #region Special Tests
        [TestMethod]
        public void AddTypicalSubScopeConnectionToController()
        {
            //Arrange

            TECControllerType controllerType = new TECControllerType(bid.Catalogs.Manufacturers[0]);
            bid.Catalogs.Add(controllerType);
            TECController controller = new TECProvidedController(controllerType);
            bid.AddController(controller);

            TECTypical typical = new TECTypical();

            TECEquipment equip = new TECEquipment();
            typical.Equipment.Add(equip);

            TECSubScope ss = new TECSubScope();
            equip.SubScope.Add(ss);

            TECDevice dev = bid.Catalogs.Devices[0];
            ss.Devices.Add(dev);

            bid.Systems.Add(typical);

            MaterialSummaryVM matVM = new MaterialSummaryVM(bid, cw);

            //Act
            IControllerConnection connection = controller.Connect(ss, (ss as IConnectable).AvailableProtocols.First(y => y is TECHardwiredProtocol));
            connection.Length = 100;
            connection.ConduitLength = 100;
            connection.ConduitType = bid.Catalogs.ConduitTypes[0];

            AssertMaterialVMMatchesCostBatch(matVM, connection.CostBatch);
        }
        #endregion

        #endregion
        
        private static void AssertMaterialVMMatchesCostBatch(MaterialSummaryVM vm, CostBatch cb)
        {
            Assert.AreEqual(vm.TotalTECCost, cb.GetCost(CostType.TEC),
                GeneralTestingUtilities.DELTA, "Total tec cost didn't update properly.");
            Assert.AreEqual(vm.TotalTECLabor, cb.GetLabor(CostType.TEC),
                GeneralTestingUtilities.DELTA, "Total tec labor didn't update properly.");
            Assert.AreEqual(vm.TotalElecCost, cb.GetCost(CostType.Electrical),
                GeneralTestingUtilities.DELTA, "Total elec cost didn't update properly.");
            Assert.AreEqual(vm.TotalElecLabor, cb.GetLabor(CostType.Electrical),
                GeneralTestingUtilities.DELTA, "Total elec labor didn't update properly.");
        }

        private static CostBatch MatVMToCostBatch(MaterialSummaryVM vm)
        {
            CostBatch cb = new CostBatch();
            cb.Add(CostType.TEC, vm.TotalTECCost, vm.TotalTECLabor);
            cb.Add(CostType.Electrical, vm.TotalElecCost, vm.TotalElecLabor);
            return cb;
        }
    }
}
