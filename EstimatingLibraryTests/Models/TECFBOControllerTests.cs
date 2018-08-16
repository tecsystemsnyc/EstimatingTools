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
    public class TECFBOControllerTests
    {
        [TestMethod()]
        public void TECFBOControllerTest()
        {
            Random rand = new Random(0);
            var catalogs = ModelCreation.TestCatalogs(rand);

            var controller = new TECFBOController(catalogs);

            Assert.IsTrue(controller.AvailableProtocols.SequenceEqual(catalogs.Protocols));
        }
        

        [TestMethod()]
        public void CopyControllerTest()
        {
            Random rand = new Random(0);
            var catalogs = ModelCreation.TestCatalogs(rand);

            var controller = new TECFBOController(catalogs);
            controller.Name = "test";

            var copy = controller.CopyController(new Dictionary<Guid, Guid>());

            Assert.IsTrue(controller.IO.Protocols.SequenceEqual(copy.IO.Protocols));
            Assert.AreEqual(controller.Name, copy.Name);
        }
    }
}