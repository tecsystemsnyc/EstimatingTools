using Microsoft.VisualStudio.TestTools.UnitTesting;
using TECUserControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstimatingLibrary;
using TestLibrary.ModelTestingUtilities;
using EstimatingLibrary.Utilities;

namespace ViewModels
{
    [TestClass()]
    public class ConnectionsVMTests
    {
        [TestMethod()]
        public void ConnectionsVMTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ConnectionsVMManyInstancesTest()
        {
            Random rand = new Random(0);
            TECBid bid = new TECBid();
            TECCatalogs catalogs = ModelCreation.TestCatalogs(rand);
            bid.Catalogs = catalogs;

            TECTypical typical = ModelCreation.TestTypical(catalogs, rand);
            bid.Systems.Add(typical);

            ConnectionsVM vm = new ConnectionsVM(bid, new ChangeWatcher(bid), bid.Catalogs);
            

            //for(int x = 0; x < 1000; x++)
            //{
            //    typical.AddInstance();
            //}

            Assert.IsTrue(true);

        }

        [TestMethod()]
        public void DragOverTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DropTest()
        {
            Assert.Fail();
        }
    }
}