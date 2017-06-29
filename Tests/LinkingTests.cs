﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using System.IO;

namespace Tests
{
    [TestClass]
    public class LinkingTests
    {
        static TECBid bid;
        static string path;

        private TestContext testContextInstance;
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

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            path = Path.GetTempFileName();
            TestDBHelper.CreateTestBid(path);
            bid = TestHelper.LoadTestBid(path);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            File.Delete(path);
        }

        #region Catalog Linking
        [TestMethod]
        public void DeviceLinking()
        {
            foreach(TECDevice device in bid.Catalogs.Devices)
            {
                foreach(TECConnectionType connectionType in device.ConnectionTypes)
                {
                    if (!bid.Catalogs.ConnectionTypes.Contains(connectionType))
                    {
                        Assert.Fail("Connection type in device not linked.");
                    }
                }
                if (!bid.Catalogs.Manufacturers.Contains(device.Manufacturer))
                {
                    Assert.Fail("Manufacturer in device not linked.");
                }
                checkScopeChildrenCatalogLinks(device, bid.Catalogs);
            }
        }

        [TestMethod]
        public void IOModuleLinking()
        {
            foreach(TECIOModule module in bid.Catalogs.IOModules)
            {
                if (!bid.Catalogs.Manufacturers.Contains(module.Manufacturer))
                {
                    Assert.Fail("Manufacturer in IO module not linked.");
                }
                checkScopeChildrenCatalogLinks(module, bid.Catalogs);
            }
        }

        [TestMethod]
        public void ConnectionTypeLinking()
        {
            foreach(TECConnectionType connectionType in bid.Catalogs.ConnectionTypes)
            {
                foreach(TECCost cost in connectionType.RatedCosts)
                {
                    if (!bid.Catalogs.AssociatedCosts.Contains(cost))
                    {
                        Assert.Fail("Rated cost in connection type not linked.");
                    }
                }
                checkScopeChildrenCatalogLinks(connectionType, bid.Catalogs);
            }
        }

        [TestMethod]
        public void ConduitTypeLinking()
        {
            foreach(TECConduitType conduitType in bid.Catalogs.ConduitTypes)
            {
                foreach (TECCost cost in conduitType.RatedCosts)
                {
                    if (!bid.Catalogs.AssociatedCosts.Contains(cost))
                    {
                        Assert.Fail("Rated cost in conduit type not linked.");
                    }
                }
                checkScopeChildrenCatalogLinks(conduitType, bid.Catalogs);
            }
        }
        #endregion

        #region System Linking
        [TestMethod]
        public void SystemLinking()
        {
            foreach(TECSystem typical in bid.Systems)
            {
                checkScopeChildrenCatalogLinks(typical, bid.Catalogs);
                checkScopeLocationLinks(typical, bid);
                foreach(TECSystem instance in typical.SystemInstances)
                {
                    checkScopeChildrenCatalogLinks(typical, bid.Catalogs);
                    checkScopeLocationLinks(typical, bid);
                }
            }
        }


        #endregion

        #region Old Linking Tests
        [TestMethod]
        public void Load_Bid_Linked_Devices()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECEquipment equipment in system.Equipment)
                {
                    foreach (TECSubScope subScope in equipment.SubScope)
                    {
                        foreach (TECDevice device in subScope.Devices)
                        {
                            if (!actualBid.Catalogs.Devices.Contains(device))
                            {
                                Assert.Fail("Devices in systems not linked");
                            }
                        }
                    }
                }
            }

            Assert.IsTrue(true, "All Devices Linked");
        }

        [TestMethod]
        public void Load_Bid_Linked_AssociatedCosts()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECCost cost in system.AssociatedCosts)
                {
                    if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                    { Assert.Fail("Associated costs in system not linked"); }
                }
                foreach (TECEquipment equipment in system.Equipment)
                {
                    foreach (TECCost cost in equipment.AssociatedCosts)
                    {
                        if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                        { Assert.Fail("Associated costs in equipment not linked"); }
                    }
                    foreach (TECSubScope subScope in equipment.SubScope)
                    {
                        foreach (TECCost cost in subScope.AssociatedCosts)
                        {
                            if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                            { Assert.Fail("Associated costs in subscope not linked"); }
                        }
                        foreach (TECDevice device in subScope.Devices)
                        {
                            foreach (TECCost cost in device.AssociatedCosts)
                            {
                                if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                                { Assert.Fail("Associated costs in subscope not linked"); }
                            }
                        }
                    }
                }
            }

            foreach (TECDevice device in actualBid.Catalogs.Devices)
            {
                foreach (TECCost cost in device.AssociatedCosts)
                {
                    if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                    { Assert.Fail("Associated costs in device catalog not linked"); }
                }
            }
            foreach (TECConduitType conduitType in actualBid.Catalogs.ConduitTypes)
            {
                foreach (TECCost cost in conduitType.AssociatedCosts)
                {
                    if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                    { Assert.Fail("Associated costs in conduit type catalog not linked"); }
                }
            }
            foreach (TECConnectionType connectionType in actualBid.Catalogs.ConnectionTypes)
            {
                foreach (TECCost cost in connectionType.AssociatedCosts)
                {
                    if (!actualBid.Catalogs.AssociatedCosts.Contains(cost))
                    { Assert.Fail("Associated costs in connection type catalog not linked"); }
                }
            }

            Assert.IsTrue(true, "All Associated costs Linked");
        }

        [TestMethod]
        public void Load_Bid_Linked_Manufacturers()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECDevice device in actualBid.Catalogs.Devices)
            {
                if (device.Manufacturer == null)
                {
                    Assert.Fail("Device doesn't have manufacturer.");
                }
                if (!actualBid.Catalogs.Manufacturers.Contains(device.Manufacturer))
                {
                    Assert.Fail("Manufacturers not linked in device catalog");
                }
            }
            foreach (TECController controller in actualBid.Controllers)
            {
                if (controller.Manufacturer == null)
                {
                    Assert.Fail("Controller doesn't have manufacturer.");
                }
                if (!actualBid.Catalogs.Manufacturers.Contains(controller.Manufacturer))
                {
                    Assert.Fail("Manufacturers not linked in controllers");
                }
            }
            Assert.IsTrue(true, "All Manufacturers linked");
        }

        [TestMethod]
        public void Load_Bid_Linked_ConduitTypes()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECController controller in actualBid.Controllers)
            {
                foreach (TECConnection connection in controller.ChildrenConnections)
                {
                    if (!actualBid.Catalogs.ConduitTypes.Contains(connection.ConduitType) && connection.ConduitType != null)
                    { Assert.Fail("Conduit types in connection not linked"); }
                }
            }
            Assert.IsTrue(true, "All conduit types Linked");
        }

        [TestMethod]
        public void Load_Bid_Linked_Tags()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECSystem system in actualBid.Systems)
            {
                foreach (TECTag tag in system.Tags)
                {
                    if (!actualBid.Catalogs.Tags.Contains(tag))
                    { Assert.Fail("Tags in system templates not linked"); }
                }
                foreach (TECEquipment equipment in system.Equipment)
                {
                    foreach (TECTag tag in equipment.Tags)
                    {
                        if (!actualBid.Catalogs.Tags.Contains(tag))
                        { Assert.Fail("Tags in system templates not linked"); }
                    }
                    foreach (TECSubScope subScope in equipment.SubScope)
                    {
                        foreach (TECTag tag in subScope.Tags)
                        {
                            if (!actualBid.Catalogs.Tags.Contains(tag))
                            { Assert.Fail("Tags in system templates not linked"); }
                        }
                        foreach (TECDevice device in subScope.Devices)
                        {
                            foreach (TECTag tag in device.Tags)
                            {
                                if (!actualBid.Catalogs.Tags.Contains(tag))
                                { Assert.Fail("Tags in system templates not linked"); }
                            }
                        }
                    }
                }
            }

            foreach (TECDevice device in actualBid.Catalogs.Devices)
            {
                foreach (TECTag tag in device.Tags)
                {
                    if (!actualBid.Catalogs.Tags.Contains(tag))
                    { Assert.Fail("Tags in device catalog not linked"); }
                }
            }
            foreach (TECConduitType conduitType in actualBid.Catalogs.ConduitTypes)
            {
                foreach (TECTag tag in conduitType.Tags)
                {
                    if (!actualBid.Catalogs.Tags.Contains(tag))
                    { Assert.Fail("Tags in conduit type catalog not linked"); }
                }
            }
            foreach (TECConnectionType connectionType in actualBid.Catalogs.ConnectionTypes)
            {
                foreach (TECTag tag in connectionType.Tags)
                {
                    if (!actualBid.Catalogs.Tags.Contains(tag))
                    { Assert.Fail("Tags in connection type catalog not linked"); }
                }
            }

            Assert.IsTrue(true, "All Tags Linked");
        }

        [TestMethod]
        public void Load_Bid_Linked_ConnectionTypes()
        {
            TECBid actualBid = TestHelper.CreateTestBid();
            TECCatalogs copiedCatalogs = actualBid.Catalogs.Copy() as TECCatalogs;
            actualBid.Catalogs = copiedCatalogs;
            ModelLinkingHelper.LinkBid(actualBid, new Dictionary<Guid, List<Guid>>());

            foreach (TECDevice device in actualBid.Catalogs.Devices)
            {
                if (device.ConnectionTypes.Count == 0)
                {
                    Assert.Fail("Device doesn't have connectionType");
                }
                foreach (TECConnectionType type in device.ConnectionTypes)
                {
                    if (!actualBid.Catalogs.ConnectionTypes.Contains(type))
                    {
                        Assert.Fail("ConnectionTypes not linked in device catalog");
                    }
                }
            }
        }
        #endregion

        private void checkScopeChildrenCatalogLinks(TECScope scope, TECCatalogs catalogs)
        {
            foreach(TECCost cost in scope.AssociatedCosts)
            {
                if (!catalogs.AssociatedCosts.Contains(cost))
                {
                    Assert.Fail("Associated cost in scope not linked.");
                }
            }
            foreach(TECTag tag in scope.Tags)
            {
                if (!catalogs.Tags.Contains(tag))
                {
                    Assert.Fail("Tag in scope not linked.");
                }
            }
        }
        private void checkScopeLocationLinks(TECScope scope, TECBid bid)
        {
            if (!bid.Locations.Contains(scope.Location))
            {
                Assert.Fail("Location in scope not linked.");
            }
        }
    }
}
