using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using EstimatingLibrary;
using EstimatingUtilitiesLibrary.Database;

namespace EstimatingUtilitiesLibraryTests
{
    /// <summary>
    /// Summary description for LoadSpeedTests
    /// </summary>
    [TestClass]
    public class LoadSpeedTests
    {
        string path = @"C:\Users\dtaylor\Desktop\Speed Test\fortospeedtest.edb";

        public LoadSpeedTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestLoadSpeed()
        {

            //SQLiteDatabase db = new SQLiteDatabase(path);
            ////db.NonQueryCommand("CREATE INDEX bid_system on BidSystem (BidID, SystemID, ScopeIndex);");
            ////db.NonQueryCommand("CREATE INDEX typical_system on SystemHierarchy (ParentID, ChildID, ScopeIndex);");
            ////db.NonQueryCommand("CREATE INDEX system_equipment on SystemEquipment (SystemID, EquipmentID, ScopeIndex);");
            ////db.NonQueryCommand("CREATE INDEX equipment_subScope on EquipmentSubScope (EquipmentID, SubScopeID, ScopeIndex);");
            ////db.NonQueryCommand("CREATE INDEX system_controller on SystemController (SystemID, ControllerID, ScopeIndex);");
            ////db.NonQueryCommand("CREATE INDEX subScope_device on SubScopeDevice (SubScopeID, DeviceID, ScopeIndex);");


            Stopwatch watch = new Stopwatch();
            DatabaseManager<TECBid> manager = new DatabaseManager<TECBid>(path);
            watch.Start();
            TECBid bid = manager.Load();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

        }
    }
}
