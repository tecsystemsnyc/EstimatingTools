﻿using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingUtilitiesLibrary;
using EstimatingUtilitiesLibrary.Database;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace TECUserControlLibrary.Debug
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class EBDebugWindow : Window
    {
        private TECBid bid;
        private ICommand testNetwork;
        private ICommand addTypical;
        private ICommand throwException;
        private ICommand exportDBCommand;
        private ICommand sendTestEmailCommand;

        public EBDebugWindow(TECBid bid)
        {
            InitializeComponent();
            this.bid = bid;
            setupCommands();
            addResources();
        }

        private void setupCommands()
        {
            testNetwork = new RelayCommand(testNetworkExecute);
            addTypical = new RelayCommand(addTypicalExecute);
            throwException = new RelayCommand(throwExceptionExecute);
            exportDBCommand = new RelayCommand(exportDBCVSExecute);
            sendTestEmailCommand = new RelayCommand(sendTestEmailExecute);
        }
        
        private void addResources()
        {
            this.Resources.Add("TestNetworkCommand", testNetwork);
            this.Resources.Add("AddTypicalCommand", addTypical);
            this.Resources.Add("ThrowExceptionCommand", throwException);
            this.Resources.Add("ExportDBCommand", exportDBCommand);
            this.Resources.Add("SendTestEmailCommand", sendTestEmailCommand);
        }

        private void testNetworkExecute()
        {
            TECControllerType type = new TECControllerType(bid.Catalogs.Manufacturers[0]);
            type.Name = "Controller Type";
            type.IO.Add(new TECIO(IOType.AI) );

            bid.Catalogs.Add(type);

            TECProvidedController controller = new TECProvidedController(type);
            controller.Name = "Test Server";
            controller.Description = "For testing.";
            controller.IsServer = true;

            bid.AddController(controller);

            TECProvidedController child = new TECProvidedController(type);
            child.Name = "Child";

            bid.AddController(child);

            TECProvidedController emptyController = new TECProvidedController(type);
            emptyController.Name = "EmptyController";

            bid.AddController(emptyController);

            TECNetworkConnection connection = controller.AddNetworkConnection(bid.Catalogs.Protocols[0]);

            connection.AddChild(child);

            TECTypical typical = new TECTypical();
            TECEquipment equip = new TECEquipment();
            TECSubScope ss = new TECSubScope();
            ss.Name = "Test Subscope";
            ss.AddDevice(bid.Catalogs.Devices[0]);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            point.Quantity = 1;
            ss.AddPoint(point);
            equip.SubScope.Add(ss);
            typical.Equipment.Add(equip);

            bid.Systems.Add(typical);
            typical.AddInstance();
        }
        private void addTypicalExecute()
        {
            TECTypical typical = new TECTypical();
            typical.Name = "test";
            TECEquipment equipment = new TECEquipment();
            equipment.Name = "test equipment";
            TECSubScope ss = new TECSubScope();
            ss.Name = "Test Subscope";
            ss.AddDevice(bid.Catalogs.Devices[0]);
            TECPoint point = new TECPoint();
            point.Type = IOType.AI;
            point.Quantity = 1;
            ss.AddPoint(point);
            equipment.SubScope.Add(ss);
            typical.Equipment.Add(equipment);

            TECSubScope connected = new TECSubScope();
            connected.Name = "Connected";
            connected.AddDevice(bid.Catalogs.Devices[0]);
            TECPoint point2 = new TECPoint();
            point2.Type = IOType.AI;
            point2.Quantity = 1;
            connected.AddPoint(point2);
            equipment.SubScope.Add(connected);

            TECSubScope toConnect = new TECSubScope();
            toConnect.Name = "To Connect";
            toConnect.AddDevice(bid.Catalogs.Devices[0]);
            TECPoint point3 = new TECPoint();
            point3.Type = IOType.AI;
            point3.Quantity = 1;
            toConnect.AddPoint(point3);
            equipment.SubScope.Add(toConnect);

            TECControllerType controllerType = new TECControllerType(new TECManufacturer());
            controllerType.IOModules.Add(bid.Catalogs.IOModules[0]);
            TECIO io = new TECIO(IOType.AI);
            io.Quantity = 10;
            controllerType.IO.Add(io);
            bid.Catalogs.IOModules[0].IO.Add(io);
            controllerType.Name = "Test Type";

            TECProvidedController controller = new TECProvidedController(controllerType);
            controller.IOModules.Add(bid.Catalogs.IOModules[0]);
            controller.Name = "Test Controller";
            typical.AddController(controller);
            TECProvidedController otherController = new TECProvidedController(controllerType);
            otherController.Name = "Other Controller";
            typical.AddController(otherController);
            IControllerConnection connection = controller.Connect(connected, (connected as IConnectable).AvailableProtocols.First());
            connection.Length = 10;
            connection.ConduitLength = 20;
            connection.ConduitType = bid.Catalogs.ConduitTypes[1];

            TECPanelType panelType = new TECPanelType(new TECManufacturer());
            panelType.Name = "test type";

            TECPanel panel = new TECPanel(panelType);
            panel.Name = "Test Panel";
            typical.Panels.Add(panel);

            TECMisc misc = new TECMisc(CostType.TEC);
            misc.Name = "test Misc";
            typical.MiscCosts.Add(misc);

            bid.Systems.Add(typical);
            typical.AddInstance();
        }
        private void throwExceptionExecute()
        {
            throw new Exception("Test Exception.");
        }
        private void exportDBCVSExecute()
        {
            DatabaseManager<TECScopeManager>.ExportDef();
        }
        private void sendTestEmailExecute()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "IncreaseContrastTeriostar.gif");
            BugReporter.SendBugReport("Test", "Greg", "ghanson@tec-system.com", "My program crashed!", path);
        }
    }
}
