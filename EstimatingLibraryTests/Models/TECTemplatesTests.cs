using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass()]
    public class TECTemplatesTests
    {
        [TestMethod()]
        public void IsTemplateObjectTest()
        {
            Random rand = new Random(0);
            TECTemplates templates = ModelCreation.TestTemplates(rand);
            var scopeTemplates = templates.Templates;

            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.SubScopeTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.EquipmentTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.SystemTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.ControllerTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.PanelTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.MiscCostTemplates.First()));
            Assert.IsTrue(templates.IsTemplateObject(scopeTemplates.Parameters.First()));



        }
    }
}