using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass]
    public class TECSystemTests
    {
        [TestMethod]
        public void CopyTemplateWithReferences()
        {
            //Arrange
            TECTemplates templates = new TECTemplates();
            TemplateSynchronizer<TECEquipment> equipSync = templates.EquipmentSynchronizer;
            TemplateSynchronizer<TECSubScope> ssSync = templates.SubScopeSynchronizer;

            TECSystem templateSys = new TECSystem();
            templates.Templates.SystemTemplates.Add(templateSys);

            TECEquipment templateEquip = new TECEquipment();
            templateEquip.Name = "Template Equip";
            templates.Templates.EquipmentTemplates.Add(templateEquip);
            templateSys.Equipment.Add(equipSync.NewItem(templateEquip));

            TECSubScope templateSS = new TECSubScope();
            templateSS.Name = "Template SS";
            templates.Templates.SubScopeTemplates.Add(templateSS);
            templateEquip.SubScope.Add(ssSync.NewItem(templateSS));

            TECSubScope equipSS = new TECSubScope();
            equipSS.Name = "Equip SS";
            templateEquip.SubScope.Add(equipSS);

            //Act
            TECSystem sysCopy = new TECSystem(templateSys, synchronizers: new Tuple<TemplateSynchronizer<TECEquipment>, TemplateSynchronizer<TECSubScope>>(equipSync, ssSync));

            //Assert
            TECEquipment tempEquipCopy = sysCopy.Equipment[0];

            TECSubScope tempSSCopy = null, equipSSCopy = null;
            foreach (TECSubScope ss in tempEquipCopy.SubScope)
            {
                if (ss.Name == "Template SS")
                {
                    tempSSCopy = ss;
                }
                else if (ss.Name == "Equip SS")
                {
                    equipSSCopy = ss;
                }
                else
                {
                    Assert.Fail("Different subScope than expected in System copy.");
                }
            }
            Assert.IsNotNull(tempSSCopy, "Template SubScope didn't copy properly.");
            Assert.IsNotNull(equipSSCopy, "Equipment SubScope didn't copy properly.");

            Assert.IsTrue(equipSync.Contains(tempEquipCopy));
            Assert.IsTrue(equipSync.GetFullDictionary()[templateEquip].Contains(tempEquipCopy));

            Assert.IsTrue(ssSync.Contains(tempSSCopy));
            Assert.IsTrue(ssSync.Contains(equipSSCopy));
            Assert.IsTrue(ssSync.GetFullDictionary()[templateSS].Contains(ssSync.GetParent(tempSSCopy)));
            Assert.IsTrue(ssSync.GetFullDictionary()[equipSS].Contains(equipSSCopy));
        }

        [TestMethod()]
        public void TECSystemTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);
            TECSystem originalSystem = ModelCreation.TestSystem(bid.Catalogs, rand);

            var copy = new TECSystem(originalSystem);
            
            //Not fully covered
            Assert.AreEqual(originalSystem.Name, copy.Name);

            Assert.AreEqual(originalSystem.CostBatch.GetCost(CostType.TEC), copy.CostBatch.GetCost(CostType.TEC), 0.000001);
            Assert.AreEqual(originalSystem.CostBatch.GetCost(CostType.Electrical), copy.CostBatch.GetCost(CostType.Electrical), 0.000001);
            Assert.AreEqual(originalSystem.CostBatch.GetLabor(CostType.TEC), copy.CostBatch.GetLabor(CostType.TEC), 0.000001);
            Assert.AreEqual(originalSystem.CostBatch.GetLabor(CostType.Electrical), copy.CostBatch.GetLabor(CostType.Electrical), 0.000001);

        }
        
        [TestMethod()]
        public void AddControllerTest()
        {
            bool raised = false;
            TECSystem system = new TECSystem();
            system.TECChanged += args =>
            {
                raised = true;
            };

            TECProvidedController controller = new TECProvidedController(new TECControllerType(new TECManufacturer()));
            system.AddController(controller);

            Assert.IsTrue(raised);
            Assert.IsTrue(system.Controllers.Contains(controller));

        }

        [TestMethod()]
        public void RemoveControllerTest()
        {
            bool raised = false;
            TECSystem system = new TECSystem();
            TECProvidedController controller = new TECProvidedController(new TECControllerType(new TECManufacturer()));
            system.AddController(controller);

            system.TECChanged += args =>
            {
                raised = true;
            };

            system.RemoveController(controller);

            Assert.IsTrue(raised);
            Assert.IsFalse(system.Controllers.Contains(controller));
        }
        
        [TestMethod()]
        public void DragDropCopyTest()
        {
            Random rand = new Random(0);
            var catalogs = ModelCreation.TestCatalogs(rand);
            var system = ModelCreation.TestSystem(catalogs, rand);
            
            var copy = system.DropData() as TECSystem;
            
            Assert.AreEqual(system.Controllers.Count, copy.Controllers.Count);
            Assert.AreEqual(system.Panels.Count, copy.Panels.Count);
            Assert.AreEqual(system.Equipment.Count, copy.Equipment.Count);
            Assert.AreEqual(system.MiscCosts.Count, copy.MiscCosts.Count);

            Assert.AreEqual(system.CostBatch.GetCost(CostType.TEC), copy.CostBatch.GetCost(CostType.TEC), 0.000001);
            Assert.AreEqual(system.CostBatch.GetCost(CostType.Electrical), copy.CostBatch.GetCost(CostType.Electrical), 0.000001);
            Assert.AreEqual(system.CostBatch.GetLabor(CostType.TEC), copy.CostBatch.GetLabor(CostType.TEC), 0.000001);
            Assert.AreEqual(system.CostBatch.GetLabor(CostType.Electrical), copy.CostBatch.GetLabor(CostType.Electrical), 0.000001);
        }

        [TestMethod()]
        public void GetAllSubScopeTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);
            TECSystem system = ModelCreation.TestSystem(bid.Catalogs, rand);

            var allSubScope = system.GetAllSubScope();
            foreach(var subScope in system.Equipment.SelectMany(e => e.SubScope))
            {
                Assert.IsTrue(allSubScope.Contains(subScope));
            }
        }

        [TestMethod()]
        public void RemoveEquipmentTest()
        {
            TECSystem system = new TECSystem();
            TECEquipment equipment = new TECEquipment();

            system.Equipment.Add(equipment);
            system.ProposalItems.Add(new TECProposalItem(equipment));

            system.Equipment.Remove(equipment);
            Assert.IsTrue(system.ProposalItems.Count == 0);

        }

        [TestMethod()]
        public void RemoveEquipmentTest1()
        {
            TECSystem system = new TECSystem();
            TECEquipment firstEquipment = new TECEquipment();
            TECEquipment secondEquipment = new TECEquipment();

            system.Equipment.Add(firstEquipment);
            system.Equipment.Add(secondEquipment);
            TECProposalItem proposalItem = new TECProposalItem(firstEquipment); 
            system.ProposalItems.Add(proposalItem);
            proposalItem.ContainingScope.Add(secondEquipment);
            
            system.Equipment.Remove(firstEquipment);
            Assert.IsTrue(system.ProposalItems.Count == 1);
            Assert.IsTrue(proposalItem.DisplayScope == secondEquipment);
            Assert.IsFalse(proposalItem.ContainingScope.Contains(firstEquipment));

        }
    }
}
