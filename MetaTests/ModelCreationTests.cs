using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;

namespace MetaTests
{
    [TestClass]
    public class ModelCreationTests
    {
        [TestMethod]
        public void IdenticalSeedBidTest()
        {
            Random rand1 = new Random(0);
            Random rand2 = new Random(0);

            TECBid bid1 = ModelCreation.TestBid(rand1);
            TECBid bid2 = ModelCreation.TestBid(rand2);
            
            TECEstimator estimator1 = new TECEstimator(bid1, new ChangeWatcher(bid1));
            TECEstimator estimator2 = new TECEstimator(bid2, new ChangeWatcher(bid2));

            Assert.AreEqual(estimator1.TotalCost, estimator2.TotalCost);
        }
    }
}
