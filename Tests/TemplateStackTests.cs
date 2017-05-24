﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingUtilitiesLibrary;
using EstimatingLibrary;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Tests
{
    /// <summary>
    /// Summary description for TemplateStackTests
    /// </summary>
    [TestClass]
    public class TemplateStackTests
    {
        public TemplateStackTests()
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

        #region Undo

        [TestMethod]
        public void Undo_Template_Systems()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECSystem> expected = new ObservableCollection<TECSystem>();
            foreach (TECSystem item in Template.SystemTemplates)
            {
                expected.Add(item);
            }
            TECSystem edit = new TECSystem();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECSystem> actual = Template.SystemTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Equipment()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECEquipment> expected = new ObservableCollection<TECEquipment>();
            foreach (TECEquipment item in Template.EquipmentTemplates)
            {
                expected.Add(item);
            }
            TECEquipment edit = new TECEquipment();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.EquipmentTemplates.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECEquipment> actual = Template.EquipmentTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECSubScope> expected = new ObservableCollection<TECSubScope>();
            foreach (TECSubScope item in Template.SubScopeTemplates)
            {
                expected.Add(item);
            }
            TECSubScope edit = new TECSubScope();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SubScopeTemplates.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECSubScope> actual = Template.SubScopeTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Catalogs_Devices()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECDevice> expected = new ObservableCollection<TECDevice>();
            foreach (TECDevice item in Template.Catalogs.Devices)
            {
                expected.Add(item);
            }
            ObservableCollection<TECConnectionType> types = new ObservableCollection<TECConnectionType>();
            types.Add(Template.Catalogs.ConnectionTypes[0]);
            TECDevice edit = new TECDevice(types,
                Template.Catalogs.Manufacturers[0]);

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Devices.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECDevice> actual = Template.Catalogs.Devices;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Catalogs_Manufacturers()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECManufacturer> expected = new ObservableCollection<TECManufacturer>();
            foreach (TECManufacturer item in Template.Catalogs.Manufacturers)
            {
                expected.Add(item);
            }
            TECManufacturer edit = new TECManufacturer();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Manufacturers.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECManufacturer> actual = Template.Catalogs.Manufacturers;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Catalogs_AssociatedCosts()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECCost> expected = new ObservableCollection<TECCost>();
            foreach (TECCost item in Template.Catalogs.AssociatedCosts)
            {
                expected.Add(item);
            }
            TECCost edit = new TECCost();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.AssociatedCosts.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECCost> actual = Template.Catalogs.AssociatedCosts;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Catalogs_ConnectionTypes()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECConnectionType> expected = new ObservableCollection<TECConnectionType>();
            foreach (TECConnectionType item in Template.Catalogs.ConnectionTypes)
            {
                expected.Add(item);
            }
            TECConnectionType edit = new TECConnectionType();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.ConnectionTypes.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECConnectionType> actual = Template.Catalogs.ConnectionTypes;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Catalogs_ConduitTypes()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECConduitType> expected = new ObservableCollection<TECConduitType>();
            foreach (TECConduitType item in Template.Catalogs.ConduitTypes)
            {
                expected.Add(item);
            }
            TECConduitType edit = new TECConduitType();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.ConduitTypes.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECConduitType> actual = Template.Catalogs.ConduitTypes;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Tags()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECTag> expected = new ObservableCollection<TECTag>();
            foreach (TECTag item in Template.Catalogs.Tags)
            {
                expected.Add(item);
            }
            TECTag edit = new TECTag();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Tags.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECTag> actual = Template.Catalogs.Tags;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_System_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Name;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Name = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Name;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_System_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Description;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Description = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Description;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_System_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Quantity;
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Quantity = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Quantity;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_System_Equipment()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECEquipment> expected = new ObservableCollection<TECEquipment>();
            foreach (TECEquipment item in Template.SystemTemplates[0].Equipment)
            {
                expected.Add(item);
            }
            TECEquipment edit = new TECEquipment();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECEquipment> actual = Template.SystemTemplates[0].Equipment;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Equipment_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].Name;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Name = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].Name;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Equipment_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].Description;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Description = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].Description;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Equipment_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Equipment[0].Quantity;
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Quantity = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].Quantity;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Equipment_SubScope()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECSubScope> expected = new ObservableCollection<TECSubScope>();
            foreach (TECSubScope item in Template.SystemTemplates[0].Equipment[0].SubScope)
            {
                expected.Add(item);
            }
            TECSubScope edit = new TECSubScope();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECSubScope> actual = Template.SystemTemplates[0].Equipment[0].SubScope;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Name;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Name = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Name;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Description;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Description = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Description;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Quantity;
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Quantity = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Quantity;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_Points()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECPoint> expected = new ObservableCollection<TECPoint>();
            foreach (TECPoint item in Template.SystemTemplates[0].Equipment[0].SubScope[0].Points)
            {
                expected.Add(item);
            }
            TECPoint edit = new TECPoint();
            edit.Type = PointTypes.AI;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points.Add(edit);
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECPoint> actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_Device()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECDevice> expected = new ObservableCollection<TECDevice>();
            foreach (TECDevice item in Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices)
            {
                expected.Add(item);
            }

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices.Add(Template.Catalogs.Devices[0]);
            Assert.AreEqual((beforeCount + 2), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECDevice> actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_SubScope_AssociatedCost()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expectedCount = Template.SystemTemplates[0].Equipment[0].SubScope[0].AssociatedCosts.Count;
            TECCost edit = new TECCost();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].AssociatedCosts.Add(edit);
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].AssociatedCosts.Count;
            Assert.AreEqual(expectedCount, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Name;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Name = edit;
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Name;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Description;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Description = edit;
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Description;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_Cost()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            double expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Cost;
            double edit = 123;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Cost = edit;
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            double actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Cost;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_Manufacturer()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            Guid expected = new Guid(Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Manufacturer.Guid.ToString());
            TECManufacturer edit = new TECManufacturer();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Manufacturer = edit;
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            Guid actual = new Guid(Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Manufacturer.Guid.ToString());
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_ConnectionType()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].ConnectionTypes.Count;
            TECConnectionType edit = Template.Catalogs.ConnectionTypes[1];

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].ConnectionTypes.Add(edit);
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].ConnectionTypes.Count;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Device_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Quantity;
            int edit = 123;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Quantity = edit;
            //Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Quantity;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Point_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Name;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Name = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Name;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Point_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Description;
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Description = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Description;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Point_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Quantity;
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Quantity = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Quantity;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Point_Type()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string expected = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Type.ToString();
            PointTypes edit = PointTypes.AO;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Type = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Type.ToString(); ;
            Assert.AreEqual(expected, actual, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Panel()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECPanel> expected = new ObservableCollection<TECPanel>();
            foreach (TECPanel item in Template.PanelTemplates)
            {
                expected.Add(item);
            }
            TECPanel edit = new TECPanel(Template.Catalogs.PanelTypes[0]);

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.PanelTemplates.Add(edit);
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECPanel> actual = Template.PanelTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_Panel_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECPanel expected = Template.PanelTemplates[0];
            string expectedName = expected.Name;

            string edit = "edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.PanelTemplates[0].Name = edit;
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            TECPanel actual = Template.PanelTemplates[0];
            Assert.AreEqual(expectedName, actual.Name, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_MiscCost()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECMisc> expected = new ObservableCollection<TECMisc>();
            foreach (TECMisc item in Template.MiscCostTemplates)
            {
                expected.Add(item);
            }
            TECMisc edit = new TECMisc();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.MiscCostTemplates.Add(edit);
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECMisc> actual = Template.MiscCostTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_PanelType()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECPanelType> expected = new ObservableCollection<TECPanelType>();
            foreach (TECPanelType item in Template.Catalogs.PanelTypes)
            {
                expected.Add(item);
            }
            TECPanelType edit = new TECPanelType();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.Catalogs.PanelTypes.Add(edit);
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECPanelType> actual = Template.Catalogs.PanelTypes;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Undo_Template_IOModule()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECIOModule> expected = new ObservableCollection<TECIOModule>();
            foreach (TECIOModule item in Template.Catalogs.IOModules)
            {
                expected.Add(item);
            }
            TECIOModule edit = new TECIOModule();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.Catalogs.IOModules.Add(edit);
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();

            //assert
            ObservableCollection<TECIOModule> actual = Template.Catalogs.IOModules;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        #endregion

        #region Redo

        [TestMethod]
        public void Redo_Template_Systems()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECSystem edit = new TECSystem();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates.Add(edit);
            var expected = new ObservableCollection<TECSystem>();
            foreach (TECSystem item in Template.SystemTemplates)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECSystem> actual = Template.SystemTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Equipment()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECEquipment edit = new TECEquipment();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.EquipmentTemplates.Add(edit);
            var expected = new ObservableCollection<TECEquipment>();
            foreach (TECEquipment item in Template.EquipmentTemplates)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECEquipment> actual = Template.EquipmentTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Redo_Template_SubScope()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECSubScope edit = new TECSubScope();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SubScopeTemplates.Add(edit);
            var expected = new ObservableCollection<TECSubScope>();
            foreach (TECSubScope item in Template.SubScopeTemplates)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECSubScope> actual = Template.SubScopeTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Redo_Template_Catalogs_Devices()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECConnectionType> types = new ObservableCollection<TECConnectionType>();
            types.Add(Template.Catalogs.ConnectionTypes[0]);
            TECDevice edit = new TECDevice(types,
                Template.Catalogs.Manufacturers[0]);

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Devices.Add(edit);
            var expected = new ObservableCollection<TECDevice>();
            foreach (TECDevice item in Template.Catalogs.Devices)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECDevice> actual = Template.Catalogs.Devices;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Catalogs_Manufacturers()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECManufacturer edit = new TECManufacturer();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Manufacturers.Add(edit);
            var expected = new ObservableCollection<TECManufacturer>();
            foreach (TECManufacturer item in Template.Catalogs.Manufacturers)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECManufacturer> actual = Template.Catalogs.Manufacturers;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Tags()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECTag edit = new TECTag();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.Tags.Add(edit);
            var expected = new ObservableCollection<TECTag>();
            foreach (TECTag item in Template.Catalogs.Tags)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECTag> actual = Template.Catalogs.Tags;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_System_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Name = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Name;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_System_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Description = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Description;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_System_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Quantity = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            int actual = Template.SystemTemplates[0].Quantity;
            Assert.AreEqual(edit, actual, "Not Undone");

        }

        [TestMethod]
        public void Redo_Template_System_Equipment()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECEquipment edit = new TECEquipment();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment.Add(edit);
            var expected = new ObservableCollection<TECEquipment>();
            foreach (TECEquipment item in Template.SystemTemplates[0].Equipment)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECEquipment> actual = Template.SystemTemplates[0].Equipment;
            Assert.AreEqual(expected.Count, actual.Count, "Not Undone");

        }

        [TestMethod]
        public void Redo_Template_Equipment_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Name = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].Name;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Equipment_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Description = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].Description;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Equipment_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].Quantity = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].Quantity;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Equipment_SubScope()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECSubScope edit = new TECSubScope();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope.Add(edit);
            var expected = new ObservableCollection<TECSubScope>();
            foreach (TECSubScope item in Template.SystemTemplates[0].Equipment[0].SubScope)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECSubScope> actual = Template.SystemTemplates[0].Equipment[0].SubScope;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_SubScope_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Name = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Name;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_SubScope_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Description = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Description;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_SubScope_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Quantity = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Quantity;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_SubScope_Points()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECPoint edit = new TECPoint();
            edit.Type = PointTypes.AI;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points.Add(edit);
            var expected = new ObservableCollection<TECPoint>();
            foreach (TECPoint item in Template.SystemTemplates[0].Equipment[0].SubScope[0].Points)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECPoint> actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_SubScope_Device()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECConnectionType> types = new ObservableCollection<TECConnectionType>();
            types.Add(Template.Catalogs.ConnectionTypes[0]);
            TECDevice edit = new TECDevice(types,
                Template.Catalogs.Manufacturers[0]);

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices.Add(edit);
            var expected = new ObservableCollection<TECDevice>();
            foreach (TECDevice item in Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECDevice> actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Device_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Name = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Name;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Device_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Description = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Description;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Device_Cost()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            double edit = 123;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Cost = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            double actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Cost;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Device_Manufacturer()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECManufacturer edit = new TECManufacturer();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Manufacturer = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            TECManufacturer actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Manufacturer;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Device_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int edit = 123;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Quantity = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Devices[0].Quantity;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Point_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Name = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Name;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Point_Description()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            string edit = "Edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Description = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            string actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Description;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Point_Quantity()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            int edit = 3;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Quantity = edit;
            Assert.AreEqual(1, testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();
            testStack.Redo();

            //assert
            int actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Quantity;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Point_Type()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            PointTypes edit = PointTypes.AO;

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Type = edit;
            testStack.Undo();
            testStack.Redo();

            //assert
            PointTypes actual = Template.SystemTemplates[0].Equipment[0].SubScope[0].Points[0].Type;
            Assert.AreEqual(edit, actual, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Panel()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            ObservableCollection<TECPanel> expected = new ObservableCollection<TECPanel>();
            foreach (TECPanel item in Template.PanelTemplates)
            {
                expected.Add(item);
            }
            TECPanel edit = new TECPanel(Template.Catalogs.PanelTypes[0]);

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.PanelTemplates.Add(edit);
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECPanel> actual = Template.PanelTemplates;
            Assert.AreEqual(expected.Count + 1, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_Panel_Name()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECPanel expected = Template.PanelTemplates[0];

            string edit = "edit";

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            int beforeCount = testStack.UndoStack.Count;
            Template.PanelTemplates[0].Name = edit;
            Assert.AreEqual((beforeCount + 1), testStack.UndoStack.Count, "Not added to undo stack");
            testStack.Undo();
            testStack.Redo();

            //assert
            TECPanel actual = Template.PanelTemplates[0];
            Assert.AreEqual(edit, actual.Name, "Not Redone");
        }

        [TestMethod]
        public void Redo_Template_MiscCost()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECMisc edit = new TECMisc();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.MiscCostTemplates.Add(edit);
            var expected = new ObservableCollection<TECMisc>();
            foreach (TECMisc item in Template.MiscCostTemplates)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECMisc> actual = Template.MiscCostTemplates;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_PanelType()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECPanelType edit = new TECPanelType();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.PanelTypes.Add(edit);
            var expected = new ObservableCollection<TECPanelType>();
            foreach (TECPanelType item in Template.Catalogs.PanelTypes)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECPanelType> actual = Template.Catalogs.PanelTypes;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        [TestMethod]
        public void Redo_Template_IOModule()
        {
            //Arrange
            var Template = TestHelper.CreateTestTemplates();
            TECIOModule edit = new TECIOModule();

            //Act
            ChangeStack testStack = new ChangeStack(Template);
            Template.Catalogs.IOModules.Add(edit);
            var expected = new ObservableCollection<TECIOModule>();
            foreach (TECIOModule item in Template.Catalogs.IOModules)
            {
                expected.Add(item);
            }
            testStack.Undo();
            testStack.Redo();

            //assert
            ObservableCollection<TECIOModule> actual = Template.Catalogs.IOModules;
            Assert.AreEqual(expected.Count, actual.Count, "Not Redone");

        }

        #endregion
    }
}
