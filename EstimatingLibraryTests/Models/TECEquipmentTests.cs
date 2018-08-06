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
    public class TECEquipmentTests
    {
        [TestMethod]
        public void CopyTemplateWithReferences()
        {
            //Arrange
            TECTemplates templates = new TECTemplates();

            TECEquipment tempEquip = new TECEquipment();
            templates.Templates.EquipmentTemplates.Add(tempEquip);

            TECSubScope tempSS = new TECSubScope();
            tempSS.Name = "Template SS";
            templates.Templates.SubScopeTemplates.Add(tempSS);
            tempEquip.SubScope.Add(templates.SubScopeSynchronizer.NewItem(tempSS));

            TECSubScope equipSS = new TECSubScope();
            equipSS.Name = "Equipment SS";
            tempEquip.SubScope.Add(equipSS);

            //Act
            TECEquipment equipCopy = new TECEquipment(tempEquip, ssSynchronizer: templates.SubScopeSynchronizer);

            //Assert
            TECSubScope newTempSS = null, newEquipSS = null;
            foreach (TECSubScope ss in equipCopy.SubScope)
            {
                if (ss.Name == "Template SS")
                {
                    newTempSS = ss;
                }
                else if (ss.Name == "Equipment SS")
                {
                    newEquipSS = ss;
                }
                else
                {
                    Assert.Fail("Different subScope than expected in equipment copy.");
                }
            }
            Assert.IsNotNull(newTempSS, "Template SubScope didn't copy properly.");
            Assert.IsNotNull(newEquipSS, "Equipment SubScope didn't copy properly.");

            TemplateSynchronizer<TECSubScope> ssSync = templates.SubScopeSynchronizer;

            Assert.IsTrue(ssSync.Contains(newTempSS));
            Assert.IsTrue(ssSync.GetFullDictionary()[tempSS].Contains(newTempSS));
        }
        

        [TestMethod()]
        public void DragDropCopyTest()
        {
            Random rand = new Random(0);
            TECBid bid = ModelCreation.TestBid(rand);
            TECEquipment equip = ModelCreation.TestEquipment(bid.Catalogs, rand);
            bid.Systems.First().Equipment.Add(equip);

            TECEquipment copy = equip.DragDropCopy(bid) as TECEquipment;

            Assert.AreEqual(equip.Name, copy.Name);
            Assert.AreEqual(equip.SubScope.Count, copy.SubScope.Count);
            Assert.IsTrue(equip.CostBatch.CostsEqual(copy.CostBatch));

        }
    }
}
