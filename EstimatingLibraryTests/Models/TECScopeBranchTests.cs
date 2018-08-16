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
    public class TECScopeBranchTests
    {
        [TestMethod()]
        public void TECScopeBranchTest()
        {
            Random rand = new Random(0);
            TECScopeBranch ogBranch = ModelCreation.TestScopeBranch(rand, 3);

            TECScopeBranch branch = new TECScopeBranch(ogBranch);

            Assert.AreEqual(ogBranch.Label, branch.Label);
            Assert.AreEqual(ogBranch.Branches.Count, branch.Branches.Count);

        }
        
    }
}