using EstimatingLibrary;
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
    public class TECValveTests
    {
        Random rand;

        [TestInitialize]
        public void TestInitialize()
        {
            rand = new Random(0);
        }

        [TestMethod]
        public void SetCost()
        {
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);

            TECValve valve = catalogs.Valves.RandomElement(rand);

            double newValue = rand.NextDouble() * 100;

            valve.Cost = newValue;

            Assert.AreEqual(newValue, valve.Cost);
        }

        [TestMethod]
        public void SetLabor()
        {
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);

            TECValve valve = catalogs.Valves.RandomElement(rand);

            double newValue = rand.NextDouble() * 100;

            valve.Labor = newValue;

            Assert.AreEqual(newValue, valve.Labor);
        }
    }
}
