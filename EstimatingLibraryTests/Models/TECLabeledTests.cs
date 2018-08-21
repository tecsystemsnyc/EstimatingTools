using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [TestClass()]
    public class TECLabeledTests
    {
        [TestMethod()]
        public void DragDropCopyTest()
        {
            TECBid bid = new TECBid();

            TECLabeled labeled = new TECLabeled();
            labeled.Label = "test";

            TECLabeled copy = labeled.DropData() as TECLabeled;

            Assert.AreNotEqual(labeled.Guid, copy.Guid);
            Assert.AreEqual(labeled.Label, copy.Label);
        }
    }
}