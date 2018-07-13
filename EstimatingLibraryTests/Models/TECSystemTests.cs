using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            TECSystem sysCopy = new TECSystem(templateSys, templates, synchronizers: new Tuple<TemplateSynchronizer<TECEquipment>, TemplateSynchronizer<TECSubScope>>(equipSync, ssSync));

            //Assert
            TECEquipment tempEquipCopy = sysCopy.Equipment[0];

            TECSubScope tempSSCopy = null, equipSSCopy = null;
            foreach(TECSubScope ss in tempEquipCopy.SubScope)
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
    }
}
